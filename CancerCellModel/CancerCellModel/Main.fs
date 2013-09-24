// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module main
open Model
open MainForm

[<EntryPoint>]
let main argv = 
    let cell_form = new CellStatisticsForm()
    cell_form.ShowDialog() |> ignore
    (*let model = new Model()
    model.simulate(1000) |> ignore*)
    0 // return an integer exit code