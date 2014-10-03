module main

open Model
open MainForm

[<EntryPoint>]
let main argv = 
    let main_form = new MainForm()
    main_form.ShowDialog() |> ignore
    0 // return an integer exit code
