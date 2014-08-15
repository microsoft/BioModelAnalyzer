﻿// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.


[<EntryPoint>]
let main argv = 
    let inputs = [| 0;1 |]
    
    SystemSimulator.run Simulator.test_automata5_forms (Map.add "path" 0 Map.empty) ["path"] ["path"] inputs

    printfn "%A" argv
    0 // return an integer exit code
