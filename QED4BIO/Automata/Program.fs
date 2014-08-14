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
    
    //System.Threading.Tasks.Task.Factory.StartNew((fun () -> Application.Run(form))) |> ignore
    do Application.Run(form)

type summary<'a> when 'a : comparison =
    { left_external_val : 'a
      right_external_val : 'a
      left_internal_val : 'a
      right_internal_val : 'a
      middle_vals : 'a list
    }
    override this.ToString() = 
        "left_val:\t" + this.left_external_val.ToString() + 
          "\nright_val:\t" + this.right_external_val.ToString() +
          //"\nint_left_val:\t" + this.left_internal_val.ToString() + 
          //"\nint_right_val:\t" + this.right_internal_val.ToString() +
          "\nmid:\t" + String.Join(",", this.middle_vals)


[<EntryPoint>]
let main argv = 
    let show_intermediate_steps = false
    let bound = 4
    let inputs = [| 0;0;0;1;0;0 |]
    let no_of_cells = inputs.Length
    //Set input high
    let b = [| for i in inputs do yield Map.add "input" i Map.empty |]
    

    let simstep rely = 
        //Simulate
        let sim = Simulator.simple_automata_B_1 rely //BH: Hardcode a model here
        if show_intermediate_steps then show_automata sim
        //Remove the bits not involved in interference
        let sim_smaller = compressedMapAutomata(sim, fun _ m -> Map.add "path" (fst m).["path"] Map.empty)
        //show_automata sim
        if show_intermediate_steps then show_automata sim_smaller
        //Introduces Bounded asynchony
        let sim_BA = new BoundedAutomata<int,Simulator.interp> (bound, sim_smaller, true)
        if show_intermediate_steps then show_automata sim_BA
        //Compress
        let final = compressedMapAutomata(sim_BA, fun _ m -> m)
        if show_intermediate_steps then show_automata final
        //show_automata final
        final

    let reach_repeatedly (auto : Automata<_,_>) =
        let body f s seen result =
            //Quick hack : TODO better 
            if Set.contains s seen then 
               Set.add s result 
            else 
               Seq.fold (fun rs n -> Set.union rs (f n (Set.add s seen) result)) Set.empty (auto.next s)
        let rec g s seen result = cache (body g) s seen result
        fun s -> g s Set.empty Set.empty

    let finalsimstep rely = 
        //Simulate
        let sim = Simulator.simple_automata_B_1 rely //BH: Hardcode a different model here
        //Remove the bits not involved in interference
        let reach_rep = reach_repeatedly sim
        let auto = compressedMapAutomata(sim, fun s m -> { left_external_val =  match snd m with | None -> dont_care | Some m -> ((rely.value m).["left_path"])
                                                           right_external_val = match snd m with | None -> dont_care | Some m -> ((rely.value m).["right_path"])
                                                           left_internal_val = ((fst m).["path"])
                                                           right_internal_val = ((fst m).["path"])
                                                           middle_vals = [(fst m).["path"]]
                                                         })
        //show_automata auto
        let ba = new NstepBarrierAutomata<_,_>(bound, auto)
        let res = compressedMapAutomata(ba, fun _ m -> m)
        res
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
                for i = 0 to 2 do
                    for j = 0 to 2 do
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
                
                //show_automata guar
                if relies.[c].simulates guar then
                    printfn " - No change"
                    changed.[c] <- false
                else
                    printfn " - Updated"
                    relies.[c] <- guar
                    changed.[c] <- true
                    //show_automata guar
            else changed.[c] <- false

    let start = System.DateTime.Now.Ticks 
  
    printfn "First Round"
    round()
    printfn "Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    changed.[0] <- false
    changed.[changed.Length - 1] <- false

    while Array.Exists(changed, fun x -> x) do
      printfn "Next Round"
      round()
      printfn "Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    printfn "Build big composition..."
    
    let normalize = Simulator.normalize_gen ()

    let auto = 
        [|
           for i = 1 to no_of_cells do
              let sim = finalsimstep (rely i)
              //show_automata sim
              //let sim = productFilter (unitAutomata) sim (fun _ y -> Some y) (fun _ y -> [y])
              yield sim
        |]

    printfn "Begin compositions"

    let set_cartesian op S1 S2 =
        Set.fold (fun RS y -> Set.fold (fun RS x -> Set.add (op x y) RS) RS S1) Set.empty S2

    let mutable steps = no_of_cells
    while steps > 1 do
        let carryover = steps % 2 = 1 
        steps <- (steps >>> 1 ) 
        for c = 0 to steps - 1 do
            printfn "Next product start: %O"  (new System.TimeSpan (System.DateTime.Now.Ticks - start))
            printfn "Calc (%d,%d) -> %d" (c*2) (c*2+1) c
            auto.[c] <- 
                composeFilter auto.[c*2] auto.[c*2+1] 
                    ( fun x bx y by ->
                        if (snd x = snd y) then
                            let b = snd x
                            let x = fst x
                            let y = fst y
                            Some ({left_external_val = if bx then x.left_external_val else dont_care
                                   right_external_val = if by then y.right_external_val else dont_care
                                   left_internal_val = x.left_internal_val 
                                   right_internal_val = y.right_internal_val
                                   middle_vals = x.middle_vals @ y.middle_vals}, b)
                        else None
                    )         
                    (fun l r -> 
                        (fst l).right_external_val = (fst r).left_internal_val 
                        || (fst l).right_external_val = dont_care 
                        || (snd l) || (snd r)
                    )
                    (fun r l -> 
                        (fst r).left_external_val = (fst l).right_internal_val                        
                        || (fst r).left_external_val = dont_care
                        || (snd l) || (snd r)
                    )
                    (fun x y -> normalize (x , y))
                    show_automata
            //show_automata auto.[c]
            auto.[c] <- compressedMapAutomata(auto.[c], fun x y -> y)
        if carryover then 
            printfn "Carry over %d -> %d" (steps*2) steps
            auto.[steps] <- auto.[steps * 2 ]
            steps <- steps + 1
             

    printfn "Pre tidy Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))
    //show_automata auto.[0]

    let tidy_auto = compressedMapAutomata (auto.[0], fun _ x -> String.Join(", ", (fst x).middle_vals))

    printfn "Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    show_automata tidy_auto


    printfn "%A" argv
    0 // return an integer exit code
