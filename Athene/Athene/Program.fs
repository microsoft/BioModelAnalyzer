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
        printfn "%A" system.Length ; printfn "BD";
        trajectory system
        //let out = [for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        bdSystemUpdate system (forceUpdate system 6.<um>) bdAtomicUpdate T dT rand
    match steps with
    | 0 -> ()
    | _ -> simulate (Update system T dT rand) (steps-1) T dT trajectory rand

let defineSystem coordinates topology =
    ()

let seed = ref 1982
let steps = ref 100
let dT = ref 1.
let xyz = ref ""
let rec parse_args args = 
    match args with 
    | [] -> () 
    | "-steps" :: t    :: rest -> steps := (int)t;    parse_args rest
    | "-seed"  :: v0   :: rest -> seed  := int(v0);   parse_args rest
    | "-dt"    :: ts   :: rest -> dT    := float(ts); parse_args rest
    | "-xyz"   :: traj :: rest -> xyz   := traj;      parse_args rest
    | _ -> failwith "Bad command line args" 


[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    let rand = System.Random(!seed)
    let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>, true) ; Particle({x=0.5<um>;y=0.5<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>,false)]
    let trajout = match !xyz with 
    | "" -> 
        printfn "No xyz output specified"
        IO.dropFrame
    | _  -> 
        IO.xyzWriteFrame (!xyz)
    simulate system !steps 298.<Kelvin> (!dT*1.0<second>) trajout rand
    0 // return an integer exit code
    