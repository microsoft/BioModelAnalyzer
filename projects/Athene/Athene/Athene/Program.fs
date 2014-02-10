open System
open System.Collections.Generic

type stateReporter = {EventWriter:(string list->unit);StateWriter:(Map<QN.var,int> list->unit);FrameWriter:(Physics.Particle list->unit)}
[<Serializable>]
type systemState = {Physical: Physics.Particle list; Formal: Map<QN.var,int> list}
[<Serializable>]
type systemDefinition = {   Topology:Map<string,Map<string,Physics.Particle -> Physics.Particle -> Vector.Vector3D<Physics.zNewton>>>; 
                            BMA:QN.node list; 
                            Interface: Interface.interfaceTopology;
                            maxMove: float<Physics.um>;
                            systemOrigin: Vector.Vector3D<Physics.um>;
                            machineName: string;
                            staticGrid: Map<int*int*int,Physics.Particle list>}//{system, topology,machineStates,qn,iTop,maxMove,sOrigin,staticGrid,machName}
[<Serializable>]
type runningParameters = {  Temperature:            float<Physics.Kelvin>;
                            Steps:                  int;
                            Time:                   float<Physics.second>;
                            TimeStep:               float<Physics.second>;
                            InterfaceGranularity:   int;
                            PhysicalGranularity:    int;
                            MachineGranularity:     int;
                            VariableTimestepDepth:  int;
                            ReportingFrequency:     int;
                            EventLog:               string list;
                            NonBondedCutOff:        float<Physics.um>;
                            }

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

let rec simulate (state:systemState) (definition:systemDefinition) (runInfo:runningParameters) (output:stateReporter) rand =
//    let pUpdate (system: Physics.Particle list) staticGrid (machineForces: Vector.Vector3D<Physics.zNewton> list) (T: float<Physics.Kelvin>) (dT: float<Physics.second>) rand  =
//        let F = Physics.forceUpdate topology 6.<Physics.um> system staticGrid sOrigin machineForces
//        Physics.bdSystemUpdate system F Physics.bdOrientedAtomicUpdate T dT rand maxMove
    (*The order is:
    Update the interface
    Write the state
    Update the physics
    Update the machines
    We do it like this to ensure that the outputs aren't confusing; if we update the interface, then the machines, then write, the changes induced by the interface may be overridden
    Changing the order won't change the simulation itself *but* will change the outputs, making testing complicated. 
    *)
    let steps = runInfo.Steps
    let freq = runInfo.ReportingFrequency
    let mg = runInfo.MachineGranularity
    let ig = runInfo.InterfaceGranularity
    let pg = runInfo.PhysicalGranularity
    let eventLog = runInfo.EventLog

    let (system', machineStates', machineForces, eventLog') = if (steps%ig=0) then (Interface.interfaceUpdate state.Physical state.Formal (runInfo.TimeStep*(float)ig) definition.Interface runInfo.Time eventLog ) else (state.Physical,state.Formal,(List.map (fun x -> {x=0.<Physics.zNewton>;y=0.<Physics.zNewton>;z=0.<Physics.zNewton>}) state.Physical),eventLog)
    let write = match (steps%freq) with 
                    | 0 ->  output.FrameWriter system'
                            output.StateWriter machineStates'
                            output.EventWriter eventLog'
                            ignore (if (steps%(freq*100)=0) then (IO.dumpSystem "Checkpoint.txt" {Physical=system';Formal=machineStates'}) else ())
                            ()
                    | _ ->  ()
    let eventLog' = if (steps%freq=0) then [] else eventLog'
    let state' = match (steps%mg>0,steps%pg>0) with 
                        | (false,false)  ->    //let p = pUpdate nSystem staticGrid machineForces T dT rand 
                                               let p = Physics.integrate system' definition.Topology definition.staticGrid definition.systemOrigin machineForces runInfo.Temperature (runInfo.TimeStep*(float)pg) definition.maxMove runInfo.VariableTimestepDepth runInfo.NonBondedCutOff 1 rand None
                                               let a = Automata.updateMachines definition.BMA machineStates'
                                               {Physical=p;Formal=a}
                        | (true,false)   ->    //let p = pUpdate system' staticGrid machineForces T dT rand 
                                               let p = Physics.integrate system' definition.Topology definition.staticGrid definition.systemOrigin machineForces runInfo.Temperature (runInfo.TimeStep*(float)pg) definition.maxMove runInfo.VariableTimestepDepth runInfo.NonBondedCutOff 1 rand None
                                               {Formal=machineStates';Physical=p}
                        | (false,true)   ->    let a = Automata.updateMachines definition.BMA machineStates'
                                               {Formal=a;Physical=system'}
                        | (true,true)    ->    {Formal=machineStates';Physical=system'}

    if steps > 0 then simulate state' definition {runInfo with Steps=(steps-1); Time=(runInfo.Time+runInfo.TimeStep); EventLog=eventLog'} output rand else ()       

let defineSystem (cartFile:string) (topfile:string) (bmafile:string) (rng: System.Random) =
    let rec countCells acc (name: string) (s: Physics.Particle list) = 
        match s with
        | head::tail -> 
                        if System.String.Equals(head.name,name) then countCells (acc+1) name tail
                        else countCells acc name tail
        | [] -> acc
    let positions = IO.pdbRead cartFile rng
    // SI: consider defining a record type rather than tuple. 
    let (pTypes, nbTypes, (machName,machI0), interfaceTopology, (sOrigin,maxMove)) = IO.xmlTopRead topfile rng
    let uCart = [for cart in positions -> 
                    let (f,r,d,freeze) = pTypes.[cart.name]
                    match freeze with
                    | true -> {Physics.defaultParticle with Physics.id=cart.id;Physics.name=cart.name;Physics.location=cart.location;Physics.velocity=cart.velocity;Physics.orientation={x=1.;y=0.;z=0.};Physics.Friction=f;Physics.radius=r;Physics.density=d;Physics.age=cart.age;Physics.gRand=cart.gRand;Physics.freeze=freeze}
                    //Physics.Particle(cart.id,cart.name,cart.location,cart.velocity,{x=1.;y=0.;z=0.},f,r,d,cart.age,cart.gRand,freeze) //use arbitrary orientation for freeze particles
                    | _ -> {Physics.defaultParticle with  Physics.id=cart.id;Physics.name=cart.name;Physics.location=cart.location;Physics.velocity=cart.velocity;Physics.orientation=(Vector.randomDirectionUnitVector rng);Physics.Friction=f;Physics.radius=r;Physics.density=d;Physics.age=cart.age;Physics.gRand=cart.gRand;Physics.freeze=freeze}  
                    //Particle(cart.id,cart.name,cart.location,cart.velocity,(Vector.randomDirectionUnitVector rng),f,r,d,cart.age,cart.gRand,freeze)
                     ]
    let staticGrid = Physics.gridFill (List.filter (fun (p: Physics.Particle) -> p.freeze) uCart) Map.empty sOrigin 6.<Physics.um> 
    let qn = IO.bmaRead bmafile
    let machineCount = countCells 0 machName uCart
    let machineStates = Automata.spawnMachines qn machineCount rng machI0
    ({Physical=uCart;Formal=machineStates},{Topology=nbTypes;BMA=qn;machineName=machName;maxMove=(maxMove*1.<Physics.um>);Interface=interfaceTopology;systemOrigin=sOrigin;staticGrid=staticGrid})
    //(uCart, nbTypes, machineStates, qn, interfaceTopology, maxMove, sOrigin, staticGrid, machName)

let seed = ref 1982
let steps = ref 100
let dT = ref 1. //Minimum timestep
let xyz = ref ""
let pdb = ref ""
let top = ref ""
let bma = ref ""
let csv = ref ""
let reg = ref ""
let freq = ref 1 //Reporting frequency in steps
let mg = ref 1 //Machine time granularity- update every mg timesteps
let pg = ref 1 //Physical time granularity- update every pg timesteps
let ig = ref 1 //Interface time granularity- update every ig timesteps
let vdt = ref 0 //Number of levels of variable timestep depth to search (i.e. the minimum timestep is dt/2^vdt
let equil = ref 10 //Number of steepest descent steps at start, and cells don't respond. Used to equilibrate a starting system
let equillength = ref 0.1<Physics.um> //maximum length of steepest descent used in minimisation
let rec parse_args args = 
    match args with 
    | [] -> () 
    | "-steps" :: t    :: rest -> steps := (int)t;    parse_args rest
    | "-seed"  :: v0   :: rest -> seed  := int(v0);   parse_args rest
    | "-dt"    :: ts   :: rest -> dT    := float(ts); parse_args rest
    | "-pdb"   :: f    :: rest -> pdb   := f;         parse_args rest
    | "-xyz"   :: traj :: rest -> xyz   := traj;      parse_args rest
    | "-top"   :: topo :: rest -> top   := topo;      parse_args rest
    | "-report":: repo :: rest -> freq  := int(repo); parse_args rest
    | "-bma"   :: bck  :: rest -> bma   := bck;       parse_args rest
    | "-csv"   :: bck  :: rest -> csv   := bck;       parse_args rest
    | "-reg"   :: name :: rest -> reg   := name;      parse_args rest
    | "-equili_steps" :: bck :: rest -> equil := (int)bck; parse_args rest
    | "-equili_len  " :: bck :: rest -> equillength := (float)bck*1.<Physics.um>; parse_args rest
    | "-mg"    :: tmp :: rest -> mg     := (int)tmp;  parse_args rest
    | "-pg"    :: tmp :: rest -> pg     := (int)tmp;  parse_args rest
    | "-ig"    :: tmp :: rest -> ig     := (int)tmp;  parse_args rest
    | "-vdt"   :: tmp :: rest -> vdt    := (int)tmp;  parse_args rest
    | x::rest -> failwith (sprintf "Bad command line args: %s" x)

let rec equilibrate (system: Physics.Particle list) (topology) (steps: int) (maxlength: float<Physics.um>) (staticGrid:Map<int*int*int,Physics.Particle list>) (sOrigin:Vector.Vector3D<Physics.um>) =
    match steps with
    | 0 -> system
    | _ -> equilibrate (Physics.steep system (Physics.forceUpdate topology 6.<Physics.um> system staticGrid sOrigin (List.map (fun x -> {x=0.<Physics.zNewton>;y=0.<Physics.zNewton>;z=0.<Physics.zNewton>}) system) )  maxlength) topology (steps-1) maxlength staticGrid sOrigin

let standardOptions pdb bma top rand = 
    let cart = match !pdb with 
                    | "" -> 
                        failwith "No pdb input specified"
                    | _ ->
                        !pdb
    let topfile = match !top with
                    | "" ->
                        failwith "No top input specified"
                    | _ ->
                        !top
    let bmafile = match !bma with
                    | "" ->
                        failwith "No bma input specified"
                    | _ ->
                        !bma
    defineSystem cart topfile bmafile rand

let restart = 0


[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    let rand = System.Random(!seed)
    //let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>, true) ; Particle({x=0.5<um>;y=0.5<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>,false)]
//    let cart = match !pdb with 
//                    | "" -> 
//                        failwith "No pdb input specified"
//                    | _ ->
//                        !pdb
//    let topfile = match !top with
//                    | "" ->
//                        failwith "No top input specified"
//                    | _ ->
//                        !top
//    let bmafile = match !bma with
//                    | "" ->
//                        failwith "No bma input specified"
//                    | _ ->
//                        !bma
    //if checkpoint_restart = "" then (let (system, topology,machineStates,qn,iTop,maxMove,sOrigin,staticGrid,machName) = standardOptions pdb bma top rand) else restart
    //let (system, topology,machineStates,qn,iTop,maxMove,sOrigin,staticGrid,machName) = standardOptions pdb bma top rand
    let (state,definition) = standardOptions pdb bma top rand
    let trajout = match !xyz with 
                    | "" -> 
                        printfn "No xyz output (physics) specified"
                        IO.dropFrame
                    | _  -> 
                        IO.xyzWriteFrame (!xyz) definition.machineName
    let csvout = match !csv with
                    | "" ->
                        printfn "No csv output (state machines) specified"
                        IO.dropStates
                    | _ ->
                        IO.csvWriteStates !csv
    let eventout  = match !reg with
                    | "" ->
                        printfn "No births and deaths output (interface) specified"
                        IO.dropEvents
                    | _ ->
                        IO.interfaceEventWriteFrame !reg
    
    printfn "Initial system:"
    printfn "Particles: %A" state.Physical.Length //system.Length
    printfn "Machines:  %A" state.Formal.Length //machineStates.Length
    
    //printfn "Static grid: %A" staticGrid
    let (mSystem,sSystem) = List.partition (fun (p:Physics.Particle) -> not p.freeze) state.Physical
    printfn "Performing %A step steepest descent EM (max length %Aum)" !equil !equillength
    let eSystem = equilibrate mSystem definition.Topology !equil !equillength definition.staticGrid definition.systemOrigin
    printfn "Completed EM. Running %A seconds of simulation (%A steps)" (!dT*((float) !steps)) !steps
    printfn "Reporting every %A seconds (total frames = %A)" (!dT*((float) !freq)) ((!steps)/(!freq))
    printfn "Physical timestep: %A Machine timestep: %A Interface timestep: %A" (!dT*(float)!pg) (!dT*(float)!mg) (!dT*(float)!ig)
    let initialState = {state with Physical=eSystem}
    let recorders = {EventWriter=eventout;FrameWriter=trajout;StateWriter=csvout}
    let runInfo = {Temperature=298.<Physics.Kelvin>; Steps=(!steps); Time=0.<Physics.second>; TimeStep=(!dT*1.0<Physics.second>); InterfaceGranularity=(!ig); PhysicalGranularity=(!pg); MachineGranularity=(!mg); VariableTimestepDepth=(!vdt); ReportingFrequency=(!freq); EventLog=["Initialise system";]; NonBondedCutOff=6.0<Physics.um>}
    //simulate eSystem machineStates qn topology iTop !steps 298.<Physics.Kelvin> (!dT*1.0<Physics.second>) (maxMove*1.<Physics.um>) staticGrid sOrigin recorders !freq !mg !pg !ig !vdt rand 0.<Physics.second> ["Initialise system";]
    simulate initialState definition runInfo recorders rand
    0 // return an integer exit code
    