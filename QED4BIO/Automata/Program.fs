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
    let show_intermediate_steps = false
    let bound = 1
    let no_of_cells = 2

    //Set input high
    let b = [| Map.add "input" 1 Map.empty; Map.add "input" 0 Map.empty|]
    

    let simstep rely i = 
        //Simulate
        let sim = Simulator.test_automata3 rely
        if show_intermediate_steps then show_automata sim
        //Remove the bits not involved in interference
        let sim_smaller = compressedMapAutomata(sim, fun m -> Map.add "neighbour_path" (fst m).["path"] b.[i])
        if show_intermediate_steps then show_automata sim_smaller
        //Introduces Bounded asynchony
        let sim_BA = new BoundedAutomata<int,Simulator.interp> (bound, sim_smaller)
        if show_intermediate_steps then show_automata sim_BA
        //Compress
        compressedMapAutomata(sim_BA, fun m -> m)

    let finalsimstep rely = 
        //Simulate
        let sim = Simulator.test_automata3 rely
        //Remove the bits not involved in interference
        compressedMapAutomata(sim, fun m -> Map.add "neighbour_path" ((rely.value (snd m)).["neighbour_path"]) ((fst m).Remove("signal")))
        
    //The universal rely
    let a = 
        [|
            for c = 0 to no_of_cells - 1 do
                let a = new SimpleAutomata<Simulator.interp>()
                for i = 1 to 4 do
                    for j = 1 to 4 do
                        a.addInitialState i
                        a.addState(i,Map.add "neighbour_path" i b.[c])
                        a.addEdge(i,j)
                yield a
        |]
    
    show_automata a.[0]
    show_automata a.[1]

    let step1a = simstep a.[1] 0
    let step1b = simstep a.[0] 1

    show_automata step1a
    show_automata step1b

    let step2a = simstep step1b 0
    let step2b = simstep step1a 1

    show_automata step2a
    show_automata step2b

    let finalstepa = finalsimstep step2b
    let finalstepb = finalsimstep step2a

    show_automata finalstepa
    show_automata finalstepb


    (*
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
    *)

    printfn "%A" argv
    0 // return an integer exit code
