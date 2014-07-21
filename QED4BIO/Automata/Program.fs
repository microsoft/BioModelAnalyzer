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
    a.Graph graph |> ignore
    gviewer.Graph <- graph
    gviewer.Dock <- DockStyle.Fill
    form.Controls.Add(gviewer)
    
    do Application.Run(form)


[<EntryPoint>]
let main argv = 
    let show_intermediate_steps = false
    let bound = 10
    let inputs = [| 1;0;1;0;0;1;0;1 |]
    let no_of_cells = inputs.Length
    //Set input high
    let b = [| for i in inputs do yield Map.add "input" i Map.empty |]
    

    let simstep rely = 
        //Simulate
        let sim = Simulator.test_automata5 rely
        if show_intermediate_steps then show_automata sim
        //Remove the bits not involved in interference
        let sim_smaller = compressedMapAutomata(sim, fun m -> Map.add "path" (fst m).["path"] Map.empty)
        if show_intermediate_steps then show_automata sim_smaller
        //Introduces Bounded asynchony
        let sim_BA = new BoundedAutomata<int,Simulator.interp> (bound, sim_smaller)
        if show_intermediate_steps then show_automata sim_BA
        //Compress
        compressedMapAutomata(sim_BA, fun m -> m), sim_BA

    let finalsimstep rely = 
        //Simulate
        let sim = Simulator.test_automata3 rely
        //Remove the bits not involved in interference
        compressedMapAutomata(sim, fun m -> Map.add "neighbour_path" ((rely.value (snd m)).["neighbour_path"]) ((fst m).Remove("signal")))

    //The universal rely
    let relies = 
        [|
            let edge = new SimpleAutomata<int,Simulator.interp>()
            edge.addInitialState 0
            edge.addState(0,Map.add "path" 0 Map.empty)
            edge.addEdge(0,0)
            yield edge
            
            for c = 1 to no_of_cells  do
                let a = new SimpleAutomata<int,Simulator.interp>()
                for i = 1 to 4 do
                    for j = 1 to 4 do
                        a.addInitialState i
                        a.addState(i,Map.add "path" i Map.empty)
                        a.addEdge(i,j)
                yield a
            
            let edge = new SimpleAutomata<int, Simulator.interp>()
            edge.addInitialState 0
            edge.addState(0,Map.add "path" 0 Map.empty)
            edge.addEdge(0,0)
            yield edge
        |]
    
    let changed = Array.map (fun _ -> true) relies
        
    let add_map s m1 m2 = 
        Map.fold (fun m k v -> Map.add (s + k) v m) m2 m1

    let round () = 
        for c = 1 to no_of_cells do
            if changed.[c-1] || changed.[c+1] then 
                printf "Recalculate %d" c
                let combine = Automata.productFilter relies.[c-1] relies.[c+1] (fun ld rd -> Some (add_map "right_" rd (add_map "left_" ld b.[c-1])))
                let guar, auto = simstep combine
                
                //show_automata combine
                //show_automata guar
                if relies.[c].simulates guar then
                    printfn " - No change"
                    changed.[c] <- false
                else
                    printfn " - Updated"
                    relies.[c] <- guar
                    changed.[c] <- true
            else changed.[c] <- false

    let start = System.DateTime.Now.Ticks 
  
    printfn "First Round"
    round()
    printfn "Ticks:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    changed.[0] <- false
    changed.[changed.Length - 1] <- false

    while Array.Exists(changed, fun x -> x) do
      printfn "Next Round"
      round()
      printfn "Ticks:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    printfn "%A" argv
    0 // return an integer exit code
