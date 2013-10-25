// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Physics
open Vector

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

let rec simulate (system: Particle list) (steps: int) (T: float<Kelvin>) (dT: float<second>) trajectory rand=
    let Update (system: Particle list) (T: float<Kelvin>) (dT: float<second>) rand =
        //printfn "%A" system.Length ; printfn "BD";
        trajectory system
        //let out = [for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        bdSystemUpdate system (forceUpdate system 6.<um>) bdAtomicUpdate T dT rand
    match steps with
    | 0 -> ()
    | _ -> simulate (Update system T dT rand) (steps-1) T dT trajectory rand

let defineSystem (cartFile:string) (topfile:string) =
    let combine (p1: Particle) (PTypes: Particle list) =
        let remapped = [for p2 in PTypes do match p1 with
                                                |p1 when System.String.Equals(p1.name,p2.name) -> yield Particle(p1.name,p1.location,p1.velocity,p2.Friction,p2.radius,p2.density,p2.freeze)
                                                |_ -> () ]
        match remapped.Length with
        | 1 -> List.nth remapped 0
        | _ -> failwith "Multiple definitions of the same particle type"
    //cartFile is (at present) a pdb with the initial cell positions
    //topology specifies the interactions and forcefield parameters
    let positions = IO.pdbRead cartFile
    let pTypes = IO.topRead topfile
    //combine the information from pTypes (on the cell sizes and density) with the postions
    [for cart in positions -> combine cart pTypes ]

let seed = ref 1982
let steps = ref 100
let dT = ref 1.
let xyz = ref ""
let pdb = ref ""
let top = ref ""
let rec parse_args args = 
    match args with 
    | [] -> () 
    | "-steps" :: t    :: rest -> steps := (int)t;    parse_args rest
    | "-seed"  :: v0   :: rest -> seed  := int(v0);   parse_args rest
    | "-dt"    :: ts   :: rest -> dT    := float(ts); parse_args rest
    | "-pdb"   :: f    :: rest -> pdb   := f;         parse_args rest
    | "-xyz"   :: traj :: rest -> xyz   := traj;      parse_args rest
    | "-top"   :: topo :: rest -> top   := topo;      parse_args rest
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
    let topology = match !top with
                    | "" ->
                        failwith "No top input specified"
                    | _ ->
                        !top
    let system = defineSystem cart topology 
    simulate system !steps 298.<Kelvin> (!dT*1.0<second>) trajout rand
    0 // return an integer exit code
    