// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Physics
open Vector

let rec listLinePrint l =
    match l with
    | head::tail -> printfn "%A" head; listLinePrint tail
    | [] -> ()

[<EntryPoint>]
let main argv = 
    let rand = System.Random(1982)
    //let numbers = PRNG.nGaussianRandomMP rand 1. 1. 100000 
    //listLinePrint numbers
    let system = [ Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.000000001<second>, 1.<um>, 1.<pg um^-3>) ]
    let atom = Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.000000001<second>, 1.<um>, 1.<pg um^-3>)
    let F = {x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>}
    let Fs = [{x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>}] 
    printfn "%A" atom.location
    let newatom = bdAtomicUpdate atom F 298.<Kelvin> 1.<second> rand
    printfn "%A" newatom.location
    let trajectory = [for i in [0..999] -> bdSystemUpdate system Fs bdAtomicUpdate 298.<Kelvin> 1.<second> rand]
    //let none = [for item in trajectory -> printfn "%A" item.location]
    //printfn "%A" trajectory
    //printfn "%A" (List.nth (List.nth trajectory 0) 0).location
    let out = [for step in trajectory -> printfn "%A" (List.nth step 0).location]
    printfn "%A" argv
    0 // return an integer exit code
