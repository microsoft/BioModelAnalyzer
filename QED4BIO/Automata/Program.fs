// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System
open System.Drawing
open System.Windows.Forms
open Microsoft.FSharp.Collections
open Automata

[<EntryPoint>]
let main argv = 
    let a = new SimpleAutomata<string>("a")
    let form = new Form(ClientSize=Size(800, 600))
    let gviewer = new Microsoft.Msagl.GraphViewerGdi.GViewer()
    let graph = new Microsoft.Msagl.Drawing.Graph()
//    a.addEdge(0,0)
    a.addState(1,"a")
    a.addEdge(0,1)
    a.addState(2,"d")
    a.addEdge(1,2)
    a.addState(3,"d")
    a.addEdge(2,3)
    a.addState(4,"d")
    a.addEdge(3,4)
    a.addEdge(4,4)

    a.Graph(graph) |> ignore
    gviewer.Graph <- graph
    form.Controls.Add(gviewer)
    do Application.Run(form)
    
    let b = new BoundedAutomata<int, string>(1, a)
    let graph = new Microsoft.Msagl.Drawing.Graph()
    b.Graph(graph) |> ignore
    let form = new Form(ClientSize=Size(800, 600))
    let gviewer = new Microsoft.Msagl.GraphViewerGdi.GViewer()
    gviewer.Graph <- graph
    form.Controls.Add(gviewer)
    do Application.Run(form)

    let b = compressedMapAutomata( b, fun x -> x)
    let graph = new Microsoft.Msagl.Drawing.Graph()
    b.Graph(graph) |> ignore
    let form = new Form(ClientSize=Size(800, 600))
    let gviewer = new Microsoft.Msagl.GraphViewerGdi.GViewer()
    gviewer.Graph <- graph
    form.Controls.Add(gviewer)
    do Application.Run(form)



    printfn "%A" argv
    0 // return an integer exit code
