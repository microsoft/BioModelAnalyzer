// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Physics
open Automata
open Vector

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

let rec simulate (system: Particle list) (machineStates: Map<QN.var,int> list) (qn: QN.node list) (topology: Map<string,Map<string,Particle->Particle->Vector3D<zNewton>>>) (steps: int) (T: float<Kelvin>) (dT: float<second>) trajectory (freq: int) rand=
    let pUpdate (system: Particle list) (T: float<Kelvin>) (dT: float<second>) rand write =
        match write with
        | true -> trajectory system
        | _ -> ()
        bdSystemUpdate system (forceUpdate topology 6.<um> system) bdAtomicUpdate T dT rand
    let aUpdate (machineStates: Map<QN.var,int> list) (qn: QN.node list) write =
        updateMachines qn machineStates
    let write = match (steps%freq) with 
                | 0 -> true
                | _ -> false
    match steps with
    | 0 -> ()
    | _ -> simulate (pUpdate system T dT rand write) (aUpdate machineStates qn write) qn topology (steps-1) T dT trajectory freq rand

let defineSystem (cartFile:string) (topfile:string) (bmafile:string) (rng: System.Random) =
//    let combine (p1: Particle) (pTypes: Particle list) =
//        let remapped = [for p2 in pTypes do match p1 with
//                                                |p1 when System.String.Equals(p1.name,p2.name) -> yield Particle(p1.name,p1.location,p1.velocity,p2.Friction,p2.radius,p2.density,p2.freeze)
//                                                |_ -> () ]
//        match remapped.Length with
//        | 1 -> List.nth remapped 0
//        | _ -> failwith "Multiple definitions of the same particle type"
    //cartFile is (at present) a pdb with the initial cell positions
    //topology specifies the interactions and forcefield parameters
    let rec countCells acc (name: string) (s: Particle list) =
        match s with
        | head::tail -> 
                        if System.String.Equals(head.name,name) then countCells (acc+1) name tail
                        else countCells acc name tail
        | [] -> acc
    let positions = IO.pdbRead cartFile
    let (pTypes, nbTypes, (machName,machI0)) = IO.xmlTopRead topfile
    //combine the information from pTypes (on the cell sizes and density) with the postions
    //([for cart in positions -> combine cart pTypes ], nbTypes)
    let uCart = [for cart in positions -> 
                    let (f,r,d,freeze) = pTypes.[cart.name]
                    match freeze with
                    | true -> Particle(cart.name,cart.location,cart.velocity,{x=1.;y=0.;z=0.},f,r,d,freeze) //use arbitrary orientation for freeze particles
                    | _ -> Particle(cart.name,cart.location,cart.velocity,(randomDirectionUnitVector rng),f,r,d,freeze)
                     ]
    let qn = IO.bmaRead bmafile
    let machineCount = countCells 0 machName uCart
    let machineStates = spawnMachines qn machineCount rng machI0
    (uCart, nbTypes, machineStates, qn)

let seed = ref 1982
let steps = ref 100
let dT = ref 1.
let xyz = ref ""
let pdb = ref ""
let top = ref ""
let bma = ref ""
let freq = ref 1
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
    | _ -> failwith "Bad command line args" 


[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    let rand = System.Random(!seed)
    //let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>, true) ; Particle({x=0.5<um>;y=0.5<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>,false)]
    let trajout = match !xyz with 
                    | "" -> 
                        printfn "No xyz output specified"
                        IO.dropFrame
                    | _  -> 
                        IO.xyzWriteFrame (!xyz)
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
    let (system, topology,machineStates,qn) = defineSystem cart topfile bmafile rand
    simulate system machineStates qn topology !steps 298.<Kelvin> (!dT*1.0<second>) trajout !freq rand
    0 // return an integer exit code
    