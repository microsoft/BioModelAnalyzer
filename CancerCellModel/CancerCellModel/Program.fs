// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module main
open Model

[<EntryPoint>]
let main argv = 
    let model = new Model()
    model.simulate(100) |> ignore
    0 // return an integer exit code