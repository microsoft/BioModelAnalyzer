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

type summary<'a> =
    { left_val : 'a
      right_val : 'a
      middle_vals : 'a list
      range : int * int  }
    override this.ToString() = 
        "left_val:\t" + this.left_val.ToString() + 
          "\nright_val:\t" + this.right_val.ToString() +
          "\nrange:\t" + this.range.ToString() +
          "\nmid:\t" + String.Join(",", this.middle_vals)


[<EntryPoint>]
let main argv = 
    let show_intermediate_steps = false
    let bound = 1
    let inputs = [| 1;1;1;1;1|]
    let no_of_cells = inputs.Length
    //Set input high
    let b = [| for i in inputs do yield Map.add "input" i Map.empty |]
    

    let simstep rely = 
        //Simulate
        let sim = Simulator.test_automata5 rely
        if show_intermediate_steps then show_automata sim
        //Remove the bits not involved in interference
        let sim_smaller = compressedMapAutomata(sim, fun _ m -> Map.add "path" (fst m).["path"] Map.empty)
        if show_intermediate_steps then show_automata sim_smaller
        //Introduces Bounded asynchony
        let sim_BA = new BoundedAutomata<int,Simulator.interp> (bound, sim_smaller, true)
        if show_intermediate_steps then show_automata sim_BA
        //Compress
        compressedMapAutomata(sim_BA, fun _ m -> m)

    let finalsimstep rely = 
        //Simulate
        let sim = Simulator.test_automata5 rely
        //Remove the bits not involved in interference
        let auto = compressedMapAutomata(sim, fun _ m -> { left_val = ((rely.value (snd m)).["left_path"])
                                                           right_val = ((rely.value (snd m)).["right_path"])
                                                           middle_vals = [(fst m).["path"]]
                                                           range = (0,0)
                                                         })
        let ba = new BoundedAutomata<_,_>(bound, auto, true)                                  
        compressedMapAutomata(ba, fun (_,i) m -> { m with range=(i,i)})

    //The universal rely
    //Gets updated in place as we refine the components.
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

    let add_map s m1 m2 = 
        Map.fold (fun m k v -> Map.add (s + k) v m) m2 m1

    let rely c = 
        Automata.productFilter relies.[c-1] relies.[c+1] 
            (fun ld rd -> Some (add_map "right_" rd (add_map "left_" ld b.[c-1])))
            (fun x y -> (x,y))

    let changed = Array.map (fun _ -> true) relies
        

    let round () = 
        for c = 1 to no_of_cells do
            if changed.[c-1] || changed.[c+1] then 
                printf "Recalculate %d" c

                let guar = simstep (rely c)
                
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

    printfn "Build big composition..."
    
    let mutable auto = finalsimstep (rely 1)
    let normalize = (@)

    let mutable auto = productFilter unitAutomata auto (fun _ x -> Some x) (fun _ x -> [x])

    //show_automata auto 


    for c = 2 to no_of_cells do
        let right = finalsimstep (rely c)
        //show_automata right
        auto <- 
            productFilter auto right 
                ( fun x y ->
                    let newrangel = if fst x.range < fst y.range then fst x.range else fst y.range
                    let newranger = if snd x.range > snd y.range then snd x.range else snd y.range
                    if x.middle_vals.[x.middle_vals.Length - 1] = y.left_val
                        && y.middle_vals.Head = x.right_val 
                        && newranger - newrangel <= bound 
                    then 
                        Some { left_val = x.left_val 
                               right_val = y.right_val
                               range = (newrangel, newranger)
                               middle_vals = x.middle_vals @ y.middle_vals}
                    else 
                        None
                )         
                (fun x y -> normalize x [y])
    //show_automata auto

    let tidy_auto = compressedMapAutomata (auto, fun _ x -> String.Join(", ", x.middle_vals))

    printfn "Ticks:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    show_automata tidy_auto


    printfn "%A" argv
    0 // return an integer exit code
