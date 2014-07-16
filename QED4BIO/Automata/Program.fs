// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System
open System.Drawing
open System.Windows.Forms
open Microsoft.FSharp.Collections
open Automata

let show_automata (a : Automata<_,_>) = 
    let form = new Form(ClientSize=Size(800, 600))
    let gviewer = new Microsoft.Msagl.GraphViewerGdi.GViewer()
    let graph = new Microsoft.Msagl.Drawing.Graph()
    a.Graph(graph) |> ignore
    gviewer.Graph <- graph
    gviewer.Dock <- DockStyle.Fill
    form.Controls.Add(gviewer)
    
    do Application.Run(form)


[<EntryPoint>]
let main argv = 
//    let a = new SimpleAutomata<string>()
//    a.addState(0,"a")
//    a.addInitialState(0)
//    a.addState(1,"a")
//    a.addEdge(0,1)
//    a.addState(2,"d")
//    a.addEdge(1,2)
//    a.addState(3,"d")
//    a.addEdge(2,3)
//    a.addState(4,"d")
//    a.addEdge(3,4)
//    a.addEdge(4,4)
//
//    show_automata a
//    
//    let b = new BoundedAutomata<int, string>(1, a)
//    show_automata b
//
//    let b = compressedMapAutomata( b, fun x -> x)
//    show_automata b
//
//    let a = new SimpleAutomata<string>()
//    for i = 1 to 4 do
//        a.addInitialState i
//        a.addState(i,i.ToString())
//        if i<4 then a.addEdge(i,i+1)
//    show_automata a
//
//    let b = new BoundedAutomata<int, string>(6, a)
//    show_automata b
//
//    let b = compressedMapAutomata(new BoundedAutomata<int, string>(6, a), fun x -> x)
//    show_automata b
//
//    for i = 1 to 20 do
//       let start = System.DateTime.Now
//       compressedMapAutomata(new BoundedAutomata<int, string>(i, a), fun x -> x) |> ignore
//       let finish = System.DateTime.Now
//       let duration = finish.Ticks - start.Ticks
//       printfn "Round %d Ticks %d" i duration
//
//    show_automata (Simulator.test_automata ())
//
//    show_automata (Simulator.test_automata2 ())
//
    let a = new SimpleAutomata<Simulator.interp>()
    let b = Map.add "input" 1 Map.empty
    for i = 1 to 4 do
        for j = 1 to 4 do
            a.addInitialState i
            a.addState(i,Map.add "neighbour_path" i b)
            a.addEdge(i,j)
    
    show_automata a

    let t1 = Simulator.test_automata3 a

    show_automata t1
    
    let t2 = compressedMapAutomata(t1, fun m -> Map.add "neighbour_path" (fst m).["path"] b)

    show_automata t2

    let t2 = new BoundedAutomata<int,Simulator.interp> (1,t2)

    show_automata t2

    let t2 = compressedMapAutomata(t2, fun m -> m)

    show_automata t2
    

    let t3 = Simulator.test_automata3 t2

    show_automata t3


    printfn "%A" argv
    0 // return an integer exit code
