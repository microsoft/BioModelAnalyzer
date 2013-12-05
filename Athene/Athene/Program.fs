

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

let rec simulate (system: Physics.Particle list) (machineStates: Map<QN.var,int> list) (qn: QN.node list) (topology: Map<string,Map<string,Physics.Particle->Physics.Particle->Vector.Vector3D<Physics.zNewton>>>) (intTop: Interface.interfaceTopology) (steps: int) (T: float<Physics.Kelvin>) (dT: float<Physics.second>) (maxMove: float<Physics.um>) staticGrid sOrigin trajectory csvout (freq: int) mg pg ig rand =
    let pUpdate (system: Physics.Particle list) staticGrid (machineForces: Vector.Vector3D<Physics.zNewton> list) (T: float<Physics.Kelvin>) (dT: float<Physics.second>) rand write =
        match write with
        | true -> trajectory system
        | _ -> ()
        let F = Physics.forceUpdate topology 6.<Physics.um> system staticGrid sOrigin machineForces
        //bdSystemUpdate system [for (sF,mF) in List.zip F machineForces -> sF+mF ] bdOrientedAtomicUpdate T dT rand maxMove
        Physics.bdSystemUpdate system F Physics.bdOrientedAtomicUpdate T dT rand maxMove
    let aUpdate (machineStates: Map<QN.var,int> list) (qn: QN.node list) write =
        match write with
        | true -> csvout machineStates
        | _ -> ()
        Automata.updateMachines qn machineStates
    let write = match (steps%freq) with 
                | 0 -> true
                | _ -> false
    match steps with
    | 0 -> ()
    | _ -> 
            match (steps%mg,steps%pg,steps%ig) with 
            | (0,0,0) ->  let (nSystem, nMachineStates, machineForces) = Interface.interfaceUpdate system machineStates dT intTop
                          let p = pUpdate nSystem staticGrid machineForces T dT rand write
                          let a = aUpdate nMachineStates qn write
                          simulate p a qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (x,0,0) when x > 0 -> let (nSystem, nMachineStates, machineForces) = Interface.interfaceUpdate system machineStates dT intTop
                                    simulate (pUpdate nSystem staticGrid machineForces T dT rand write) nMachineStates qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (0,x,0) when x > 0 -> let (nSystem, nMachineStates, machineForces) = Interface.interfaceUpdate system machineStates dT intTop
                                    simulate nSystem (aUpdate nMachineStates qn write) qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (0,0,x) when x > 0 -> simulate (pUpdate system staticGrid [for p in system-> {x=0.<Physics.zNewton>;y=0.<Physics.zNewton>;z=0.<Physics.zNewton>}] T dT rand write) (aUpdate machineStates qn write) qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (x,y,0) when x > 0 && y > 0 ->    let (nSystem, nMachineStates, machineForces) = Interface.interfaceUpdate system machineStates dT intTop
                                                simulate nSystem nMachineStates qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (x,0,y) when x > 0 && y > 0 ->    simulate (pUpdate system staticGrid [for p in system-> {x=0.<Physics.zNewton>;y=0.<Physics.zNewton>;z=0.<Physics.zNewton>}] T dT rand write) machineStates qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (0,x,y) when x > 0 && y > 0 ->    simulate system (aUpdate machineStates qn write) qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
            | (_,_,_) -> simulate system machineStates qn topology intTop (steps-1) T dT maxMove staticGrid sOrigin trajectory csvout freq mg pg ig rand
                       
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
                    | true -> Physics.Particle(cart.id,cart.name,cart.location,cart.velocity,{x=1.;y=0.;z=0.},f,r,d,cart.age,cart.gRand,freeze) //use arbitrary orientation for freeze particles
                    | _ -> Physics.Particle(cart.id,cart.name,cart.location,cart.velocity,(Vector.randomDirectionUnitVector rng),f,r,d,cart.age,cart.gRand,freeze)
                     ]
    let staticGrid = Physics.gridFill (List.filter (fun (p: Physics.Particle) -> p.freeze) uCart) Map.empty sOrigin 6.<Physics.um> 
    let qn = IO.bmaRead bmafile
    let machineCount = countCells 0 machName uCart
    let machineStates = Automata.spawnMachines qn machineCount rng machI0
    (uCart, nbTypes, machineStates, qn, interfaceTopology, maxMove, sOrigin, staticGrid, machName)

let seed = ref 1982
let steps = ref 100
let dT = ref 1. //Minimum timestep
let xyz = ref ""
let pdb = ref ""
let top = ref ""
let bma = ref ""
let csv = ref ""
let freq = ref 1 //Reporting frequency in steps
let mg = ref 1 //Machine time granularity- update every mg timesteps
let pg = ref 1 //Physical time granularity- update every pg timesteps
let ig = ref 1 //Interface time granularity- update every ig timesteps
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
    | "-equili_steps" :: bck :: rest -> equil := (int)bck; parse_args rest
    | "-equili_len  " :: bck :: rest -> equillength := (float)bck*1.<Physics.um>; parse_args rest
    | "-mg"    :: tmp :: rest -> mg     := (int)tmp;  parse_args rest
    | "-pg"    :: tmp :: rest -> pg     := (int)tmp;  parse_args rest
    | "-ig"    :: tmp :: rest -> ig     := (int)tmp;  parse_args rest
    | _ -> failwith "Bad command line args" 

let rec equilibrate (system: Physics.Particle list) (topology) (steps: int) (maxlength: float<Physics.um>) (staticGrid:Map<int*int*int,Physics.Particle list>) (sOrigin:Vector.Vector3D<Physics.um>) =
    match steps with
    | 0 -> system
    | _ -> equilibrate (Physics.steep system (Physics.forceUpdate topology 6.<Physics.um> system staticGrid sOrigin (List.map (fun x -> {x=0.<Physics.zNewton>;y=0.<Physics.zNewton>;z=0.<Physics.zNewton>}) system) ) maxlength) topology (steps-1) maxlength staticGrid sOrigin

[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    let rand = System.Random(!seed)
    //let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>, true) ; Particle({x=0.5<um>;y=0.5<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>,false)]
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
    let csvout = match !csv with
                    | "" ->
                        printfn "No csv output (state machines) specified"
                        IO.dropStates
                    | _ ->
                        IO.csvWriteStates !csv
    let (system, topology,machineStates,qn,iTop,maxMove,sOrigin,staticGrid,machName) = defineSystem cart topfile bmafile rand
    let trajout = match !xyz with 
                    | "" -> 
                        printfn "No xyz output specified"
                        IO.dropFrame
                    | _  -> 
                        IO.xyzWriteFrame (!xyz) machName
    printfn "Initial system:"
    printfn "Particles: %A" system.Length
    printfn "Machines:  %A" machineStates.Length
    //printfn "Static grid: %A" staticGrid
    let (mSystem,sSystem) = List.partition (fun (p:Physics.Particle) -> not p.freeze) system
    printfn "Performing %A step steepest descent EM (max length %Aum)" !equil !equillength
    let eSystem = equilibrate mSystem topology !equil !equillength staticGrid sOrigin
    printfn "Completed EM. Running %A seconds of simulation (%A steps)" (!dT*((float) !steps)) !steps
    printfn "Reporting every %A seconds (total frames = %A)" (!dT*((float) !freq)) ((!steps)/(!freq))
    simulate eSystem machineStates qn topology iTop !steps 298.<Physics.Kelvin> (!dT*1.0<Physics.second>) (maxMove*1.<Physics.um>) staticGrid sOrigin trajout csvout !freq !mg !pg !ig rand
    0 // return an integer exit code
    