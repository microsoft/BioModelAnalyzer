// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
open System
open System.Collections.Generic
open System.IO

open IO

type stateReporter = {EventWriter:(string list->unit);StateWriter:(Map<QN.var,int> array->unit);FrameWriter:(Physics.Particle array->unit)}
//[<Serializable>]
//type systemState = {Physical: Physics.Particle list; Formal: Map<QN.var,int> list}
[<Serializable>]
type systemDefinition = {   Topology:Map<string,Map<string,Physics.Particle -> Physics.Particle -> Vector.Vector3D<Physics.zNewton>>>; 
                            BMA:QN.node list; 
                            Interface: Interface.interfaceTopology;
                            maxMove: float<Physics.um>;
                            systemOrigin: Vector.Vector3D<Physics.um>;
                            machineName: string;
                            staticGrid: Dictionary<int*int*int,Physics.Particle list>;
                            staticSystem: Physics.Particle array;
                            }//{system, topology,machineStates,qn,iTop,maxMove,sOrigin,staticGrid,machName}
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
                            Threads:                int;
                            CheckPointDisable:      bool;
                            CheckPointFreq:         int;
                            SearchType:             Physics.searchType;
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

    let (system', machineStates', machineForces, eventLog') = if (steps%ig=0) then (Interface.interfaceUpdate state.Physical state.Formal (runInfo.TimeStep*(float)ig) definition.Interface runInfo.Time eventLog ) else (state.Physical,state.Formal,(Array.map (fun x -> new Vector.Vector3D<Physics.zNewton>()) state.Physical),eventLog)
    let write = match (steps%freq) with 
                    | 0 ->  output.FrameWriter system'
                            output.StateWriter machineStates'
                            output.EventWriter eventLog'
                            ignore (if ((not runInfo.CheckPointDisable) && (steps%(runInfo.CheckPointFreq)=0) ) then (IO.dumpSystem "Checkpoint.txt" {Physical=system';Formal=machineStates'}) else ())
                            ()
                    | _ ->  ()
    let eventLog' = if (steps%freq=0) then [] else eventLog'
    let state' = match (steps%mg>0,steps%pg>0) with 
                        | (false,false)  ->    //let p = pUpdate nSystem staticGrid machineForces T dT rand 
                                               let p = Physics.integrate system' definition.Topology runInfo.SearchType definition.staticGrid definition.staticSystem definition.systemOrigin machineForces runInfo.Temperature (runInfo.TimeStep*(float)pg) definition.maxMove runInfo.VariableTimestepDepth runInfo.NonBondedCutOff 1 rand runInfo.Threads None
                                               let a = Automata.updateMachines definition.BMA machineStates' runInfo.Threads
                                               {Physical=p;Formal=a}
                        | (true,false)   ->    //let p = pUpdate system' staticGrid machineForces T dT rand 
                                               let p = Physics.integrate system' definition.Topology runInfo.SearchType definition.staticGrid definition.staticSystem definition.systemOrigin machineForces runInfo.Temperature (runInfo.TimeStep*(float)pg) definition.maxMove runInfo.VariableTimestepDepth runInfo.NonBondedCutOff 1 rand runInfo.Threads None
                                               {Formal=machineStates';Physical=p}
                        | (false,true)   ->    let a = Automata.updateMachines definition.BMA machineStates' runInfo.Threads
                                               {Formal=a;Physical=system'}
                        | (true,true)    ->    {Formal=machineStates';Physical=system'}

    if steps > 0 then simulate state' definition {runInfo with Steps=(steps-1); Time=(runInfo.Time+runInfo.TimeStep); EventLog=eventLog'} output rand else ()       

let defineSystem (cartFile:string) (topfile:string) (bmafile:string) =
    let (pTypes, nbTypes, (machName,machI0), interfaceTopology, (sOrigin,maxMove), rp, rng) = IO.xmlTopRead topfile
    let positions = IO.pdbRead cartFile rng
    // SI: consider defining a record type rather than tuple. 
    let uCart = [for cart in positions -> 
                    let (f,r,d,freeze) = pTypes.[cart.name]
                    match freeze with
                    | true -> {Physics.defaultParticle with Physics.id=cart.id;Physics.name=cart.name;Physics.location=cart.location;Physics.velocity=cart.velocity;Physics.orientation=new Vector.Vector3D<1>(1.,0.,0.);Physics.Friction=f;Physics.radius=r;Physics.density=d;Physics.age=cart.age;Physics.gRand=cart.gRand;Physics.freeze=freeze}
                    //Physics.Particle(cart.id,cart.name,cart.location,cart.velocity,{x=1.;y=0.;z=0.},f,r,d,cart.age,cart.gRand,freeze) //use arbitrary orientation for freeze particles
                    | _ -> {Physics.defaultParticle with  Physics.id=cart.id;Physics.name=cart.name;Physics.location=cart.location;Physics.velocity=cart.velocity;Physics.orientation=(Vector.randomDirectionUnitVector rng);Physics.Friction=f;Physics.radius=r;Physics.density=d;Physics.age=cart.age;Physics.gRand=cart.gRand;Physics.freeze=freeze}  
                    //Particle(cart.id,cart.name,cart.location,cart.velocity,(Vector.randomDirectionUnitVector rng),f,r,d,cart.age,cart.gRand,freeze)
                     ]
                     |> Array.ofList
    let staticSystem = Array.filter (fun (p: Physics.Particle) -> p.freeze) uCart
    let blankGrid = new Dictionary<int*int*int,Physics.Particle list>(HashIdentity.Structural)
    let staticGrid = Physics.gridFill staticSystem blankGrid sOrigin rp.nonBond 
    let qn = IO.bmaRead bmafile
    let machineCount = Array.length (Array.filter  (fun (p: Physics.Particle) -> not p.freeze) uCart) 
    let machineStates = Automata.spawnMachines qn machineCount rng machI0
    let runInfo = {Temperature=rp.temperature; Steps=rp.steps; Time=0.<Physics.second>; TimeStep=rp.timestep; InterfaceGranularity=rp.ig; PhysicalGranularity=rp.pg; MachineGranularity=rp.mg; VariableTimestepDepth=rp.vdt; ReportingFrequency=rp.report; EventLog=["Initialise system";]; NonBondedCutOff=rp.nonBond; Threads=0; CheckPointDisable=false; CheckPointFreq=rp.checkpointReport;SearchType=rp.searchType}

    ({Physical=uCart;Formal=machineStates},{Topology=nbTypes;BMA=qn;machineName=machName;maxMove=(maxMove*1.<Physics.um>);Interface=interfaceTopology;systemOrigin=sOrigin;staticGrid=staticGrid;staticSystem=staticSystem},runInfo,rng)
    //(uCart, nbTypes, machineStates, qn, interfaceTopology, maxMove, sOrigin, staticGrid, machName)

//let seed = ref 1982
//let steps = ref 100
let threads = ref (System.Environment.ProcessorCount)
//let dT = ref 1. //Minimum timestep
let xyz = ref ""
let pdb = ref ""
let top = ref ""
let bma = ref ""
let csv = ref ""
let reg = ref ""
let restart = ref ""
let disablecheckpoint = ref false
//let freq = ref 1 //Reporting frequency in steps
//let mg = ref 1 //Machine time granularity- update every mg timesteps
//let pg = ref 1 //Physical time granularity- update every pg timesteps
//let ig = ref 1 //Interface time granularity- update every ig timesteps
//let vdt = ref 0 //Number of levels of variable timestep depth to search (i.e. the minimum timestep is dt/2^vdt
let equil = ref 10 //Number of steepest descent steps at start, and cells don't respond. Used to equilibrate a starting system
let equillength = ref 0.1<Physics.um> //maximum length of steepest descent used in minimisation
let rec parse_args args = 
    match args with 
    | [] -> () 
    //| "-steps" :: t    :: rest -> steps := (int)t;    parse_args rest
    | "-threadlimit" :: t  :: rest -> threads := (int)t;    parse_args rest
    //| "-seed"  :: v0   :: rest -> seed  := int(v0);   parse_args rest
    //| "-dt"    :: ts   :: rest -> dT    := float(ts); parse_args rest
    | "-pdb"   :: f    :: rest -> pdb   := f;         parse_args rest
    | "-xyz"   :: traj :: rest -> xyz   := traj;      parse_args rest
    | "-top"   :: topo :: rest -> top   := topo;      parse_args rest
    //| "-report":: repo :: rest -> freq  := int(repo); parse_args rest
    | "-bma"   :: bck  :: rest -> bma   := bck;       parse_args rest
    | "-csv"   :: bck  :: rest -> csv   := bck;       parse_args rest
    | "-reg"   :: name :: rest -> reg   := name;      parse_args rest
    | "-equili_steps" :: bck :: rest -> equil := (int)bck; parse_args rest
    | "-equili_len  " :: bck :: rest -> equillength := (float)bck*1.<Physics.um>; parse_args rest
    //| "-mg"    :: tmp :: rest -> mg     := (int)tmp;  parse_args rest
    //| "-pg"    :: tmp :: rest -> pg     := (int)tmp;  parse_args rest
    //| "-ig"    :: tmp :: rest -> ig     := (int)tmp;  parse_args rest
    //| "-vdt"   :: tmp :: rest -> vdt    := (int)tmp;  parse_args rest
    | "-restart" :: tmp :: rest -> restart := tmp  ;  parse_args rest
    | x::rest -> failwith (sprintf "Bad command line args: %s" x)

let rec equilibrate (system: Physics.Particle array) (topology) (steps: int) (maxlength: float<Physics.um>) searchType (staticGrid:Dictionary<int*int*int,Physics.Particle list>) staticSystem (sOrigin:Vector.Vector3D<Physics.um>) (cutoff:float<Physics.um>) =
    match steps with
    | 0 -> system
    | _ ->  let zeroForces = Array.init (Array.length system) (fun index -> new Vector.Vector3D<Physics.zNewton>() ) 
            let forceEnv = (Physics.forceUpdate topology cutoff system searchType staticGrid staticSystem sOrigin zeroForces (!threads))
            let system' = (Physics.steep system forceEnv  maxlength)
            equilibrate system' topology (steps-1) maxlength searchType staticGrid staticSystem sOrigin cutoff

let standardOptions pdb bma top  = 
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
    defineSystem cart topfile bmafile 


[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    //let rand = System.Random(!seed)
    let (state,definition,runInfo,rand) = standardOptions pdb bma top
    //edit runinfo with details like threads and checkpointing
    let runInfo = {runInfo with Threads=(!threads); CheckPointDisable=(!disablecheckpoint)}
    if (!xyz="") then failwith "No xyz output (physics) specified" else ()
    use xyzFile = new StreamWriter(!xyz, true)
    let trajout = IO.xyzWriteFrame (xyzFile) definition.machineName

    if (!csv="") then failwith "No csv output (state machines) specified" else ()
    use csvFile = new StreamWriter(!csv, true)
    let csvout = IO.csvWriteStates csvFile

    if (!reg="") then failwith "No births and deaths output (interface) specified" else ()
    use regFile = new StreamWriter(!reg, true)
    let eventout = IO.interfaceEventWriteFrame regFile
    
    printfn "Initial system:"
    printfn "Particles: %A" state.Physical.Length //system.Length
    printfn "Machines:  %A" state.Formal.Length //machineStates.Length
    printfn "Maximum number of threads: %A" runInfo.Threads
    
    //printfn "Static grid: %A" staticGrid
    //Todo: define a discriminated union to test for mobile, static and unsorted systems to avoid repeated partitions/filters
    let (mSystem,sSystem) = Array.partition (fun (p:Physics.Particle) -> not p.freeze) state.Physical
    printfn "Performing %A step pseudo steepest descent (max length %Aum)" !equil !equillength
    let eSystem = equilibrate mSystem definition.Topology !equil !equillength runInfo.SearchType definition.staticGrid definition.staticSystem definition.systemOrigin runInfo.NonBondedCutOff
    printfn "Completed pseudo SD. Running %A seconds of simulation (%A steps)" (runInfo.TimeStep*(float runInfo.Steps)) runInfo.Steps // (!dT*((float) !steps)) !steps
    printfn "Reporting every %A seconds (total frames = %A)" (runInfo.TimeStep*(float runInfo.ReportingFrequency)) (runInfo.Steps/runInfo.ReportingFrequency)//(!dT*((float) !freq)) ((!steps)/(!freq))
    printfn "Physical timestep: %A Machine timestep: %A Interface timestep: %A" (runInfo.TimeStep*(float runInfo.PhysicalGranularity)) (runInfo.TimeStep*(float runInfo.MachineGranularity)) (runInfo.TimeStep * (float runInfo.InterfaceGranularity) )//(!dT*(float)!pg) (!dT*(float)!mg) (!dT*(float)!ig)
    let initialState = {state with Physical=eSystem}
    let recorders = {EventWriter=eventout;FrameWriter=trajout;StateWriter=csvout}
//    let runInfo = {Temperature=298.<Physics.Kelvin>; Steps=(!steps); Time=0.<Physics.second>; TimeStep=(!dT*1.0<Physics.second>); InterfaceGranularity=(!ig); PhysicalGranularity=(!pg); MachineGranularity=(!mg); VariableTimestepDepth=(!vdt); ReportingFrequency=(!freq); EventLog=["Initialise system";]; NonBondedCutOff=6.0<Physics.um>; Threads=(!threads); CheckPointDisable=(!disablecheckpoint)}
    //If a restart file is used: open it and replace the system and machines with this
    let initialState =  if (!restart <> "") then 
                                let io = (IO.readCheckpoint !restart)
                                {Physical=io.Physical;Formal=io.Formal}
                        else initialState
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    simulate initialState definition runInfo recorders rand
    stopWatch.Stop()
    printfn "Simulation time = %f seconds" stopWatch.Elapsed.TotalSeconds
    //Clean up and close files
    regFile.Close()
    xyzFile.Close()
    csvFile.Close()
    0 // return an integer exit code
    
