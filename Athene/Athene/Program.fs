// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Physics
open Vector

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

let rec simulate (system: Particle list) (steps: int) (T: float<Kelvin>) (dT: float<second>) rand=
    let Update (system: Particle list) (T: float<Kelvin>) (dT: float<second>) rand =
        printfn "%A" system.Length ; printfn "BD";
        let out = [for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        bdSystemUpdate system [{x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>}] bdAtomicUpdate T dT rand
    match steps with
    | 0 -> ()
    | _ -> simulate (Update system T dT rand) (steps-1) T dT rand

let seed = ref 1982
let steps = ref 100
let rec parse_args args = 
    match args with 
    | [] -> () 
    | "-steps" :: t :: rest -> steps := (int)t; parse_args rest
    | "-seed" :: v0 :: rest -> seed := int(v0); parse_args rest
    | _ -> failwith "Bad command line args" 


[<EntryPoint>]
let main argv = 
    parse_args (List.ofArray argv)
    let rand = System.Random(!seed)
    let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>)]
    simulate system !steps 298.<Kelvin> 1.<second> rand
    0 // return an integer exit code
    