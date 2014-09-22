module SystemSimulator

open System
open System.Collections.Generic
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
    { left_external_val : Map<string, 'a>
      right_external_val : Map<string, 'a>
      left_internal_val : Map<string, 'a>
      right_internal_val : Map<string, 'a>
      middle_vals : Set<Map<string, 'a> list>
    }
    override this.ToString() = 
        "left_val:\t" + this.left_external_val.ToString() + 
          "\nright_val:\t" + this.right_external_val.ToString() +
          //"\nint_left_val:\t" + this.left_internal_val.ToString() + 
          //"\nint_right_val:\t" + this.right_internal_val.ToString() +
          "\nmid:\t" + String.Join(",", this.middle_vals)

type Params = {
        show_intermediate_steps : bool
        show_composition_steps : bool
        bound : int
        cheap_answer : bool
        really_cheap_answer : bool
        no_compress : bool
        binary_combine : bool
        barrier : bool
    }
let default_params b = 
    {
        show_intermediate_steps = false
        show_composition_steps = false
        bound = b
        cheap_answer = true
        really_cheap_answer = true 
        no_compress = true 
        binary_combine = false
        barrier = true
    }

let run (init_form, trans_form) edge_values comms fates (inputs : int[]) (p : Params) = 
    let show_intermediate_steps = p.show_intermediate_steps
    let show_composition_steps = p.show_composition_steps
    let bound = p.bound
    let cheap_answer = p.cheap_answer
    let really_cheap_answer = p.really_cheap_answer && cheap_answer
    let no_compress = cheap_answer || p.no_compress
    let binary_combine = p.binary_combine



    let comms = Set.ofSeq comms
    let left_comms = Set.ofSeq (Seq.map (fun x -> "left_" + x) comms)
    let right_comms = Set.ofSeq (Seq.map (fun x -> "right_" + x) comms)
    let fates = Set.ofSeq fates
    
    let no_of_cells = inputs.Length
    //Set inputs
    let b = [| for i in inputs do yield Map.add "input" i Map.empty |]

    let sim = Simulator.sim init_form trans_form

    let map_filter p m =
        Map.fold (fun newm k v -> if p k then Map.add k v newm else newm) Map.empty m 
    let map_filter_map p f m =
        Map.fold (fun newm k v -> if p k then Map.add (f k) v newm else newm) Map.empty m 

    let setcontains s x = Set.contains x s

    let simstep rely = 
        //Simulate
        let sim = sim rely 
        if show_intermediate_steps then show_automata sim
        //Remove the bits not involved in interference
        let sim_smaller = compressedMapAutomata(sim, fun _ m -> map_filter (setcontains comms) (fst m))
        //show_automata sim
        if show_intermediate_steps then show_automata sim_smaller
        //Introduces Bounded asynchony
        let final = 
            if p.barrier then 
                let sim_BA = new BarrierBoundedAutomata<int,Simulator.interp> (bound, sim_smaller, true) :> Automata<_,_>
                if show_intermediate_steps then show_automata sim_BA
                //Compress
                compressedMapAutomata(sim_BA, fun _ m -> m)
            else
                let sim_BA = new SlidingWindowBoundedAutomata<int,Simulator.interp> (bound, sim_smaller, true) :> Automata<_,_>
                if show_intermediate_steps then show_automata sim_BA
                //Compress
                compressedMapAutomata(sim_BA, fun _ m -> m)
            
        if show_intermediate_steps then show_automata final
        show_automata final
        final


    let reach_repeatedly (auto : SimpleAutomata<_,_>) (f : 'a -> System.Collections.Generic.ISet<'b> -> unit) =
        let result = new System.Collections.Generic.Dictionary<_,ISet<'b>>()
        let scc_map = Automata.scc
                          (fun reps -> 
                                    let res = new HashSet<_>()
                                    if reps.Length > 1 then 
                                        List.iter (fun y -> f y res) reps 
                                    res :> ISet<_>)
                             auto.next auto.pred auto.initialstates

        Automata.dfs_iter auto.next Automata.skip 
            (fun x -> 
                result.Add(x, 
                                    let nexts = auto.next x 
                                    let res : HashSet<'b> = new HashSet<_>() 
                                    if (*nexts.Count = 0 ||*) nexts.Contains x then 
                                        f x res
                                    res.UnionWith (scc_map.[x])
                                    for n in nexts do
                                        match result.TryGetValue n with 
                                        | true, x -> res.UnionWith x
                                        | false, _ -> ()
                                    res)
            )
            Automata.skip
            auto.initialstates |> ignore
        fun x -> result.[x]

    let finalsimstep rely = 
        //Simulate
        let sim = sim rely 
        sim.RemoveNoEdges true
        //Remove the bits not involved in interference
        let f = fun x (set : ISet<_>) -> set.Add( [map_filter (setcontains fates) (fst (sim.value x))]) |> ignore
        let reach_rep = if no_compress then fun x -> let hs = (new HashSet<_>()) in f x hs; hs :> ISet<_> else reach_repeatedly sim f
        let auto = compressedMapAutomata(sim, fun s m -> { left_external_val =  match snd m with | None -> dont_care | Some m -> map_filter_map (setcontains left_comms) (fun (s : string) -> s.Substring 5) (rely.value m)
                                                           right_external_val = match snd m with | None -> dont_care | Some m -> map_filter_map (setcontains right_comms) (fun (s : string) -> s.Substring 6) (rely.value m)                                                           
                                                           left_internal_val = map_filter (setcontains comms) (fst m)
                                                           right_internal_val = map_filter (setcontains comms) (fst m)
                                                           middle_vals = (reach_rep s) |> Set.ofSeq
                                                         })
        //show_automata auto
        if cheap_answer then 
            //show_automata auto
            auto.RemoveNoEdges false
            //Make all states initial as we have lost the various starts of the system
            for s in auto.states do
                auto.addInitialState s
        auto

    //The universal rely
    // The rely for context is trivial for calculating the universal rely.
    let unitAuto = new SimpleAutomata<int,Simulator.interp>()
    unitAuto.addInitialState 0
    unitAuto.addState(0,Map.empty)
    unitAuto.addEdge(0,0)

    let univ_rely_pre = Simulator.sim trans_form trans_form unitAuto
    let univ_rely = Automata.compressedMapAutomata (univ_rely_pre, (fun _ (y,_) -> map_filter (setcontains comms) y))
    
    //show_automata univ_rely

    //Gets updated in place as we refine the components.
    let relies = 
        [|
            let edge = new SimpleAutomata<int,Simulator.interp>()
            edge.addInitialState 0
            edge.addState(0,edge_values)
            edge.addEdge(0,0)
            yield edge
            
            for c = 1 to no_of_cells  do
                yield univ_rely
            
            let edge = new SimpleAutomata<int, Simulator.interp>()
            edge.addInitialState 0
            edge.addState(0,edge_values)
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
    
    let normalize = (fun (x,y) -> x @ y) //Simulator.normalize_gen ()

    let bound = ref bound
    let auto = 
        let temp_auto = 
            [|
               for i = 1 to no_of_cells do
                  let sim = finalsimstep (rely i)
                  yield sim
            |]

        //Calculate if we can fudge the bound as they all have 1 length loops at the end
        if really_cheap_answer then
            let bound_orig = !bound
            bound := 0
            for sim in temp_auto do
                scc (fun x -> if x.Length > 1 then bound := bound_orig) sim.next sim.pred sim.states |> ignore


        [| 
            for sim in temp_auto do
                let sim = 
                    if !bound = 0 then
                        let res = compressedMapAutomata(sim, fun _ m -> (m,false))
                        res
                    else
                        //TODO need to do something in the case where p.barrier=false
                        let ba = new NstepBarrierAutomata<_,_>(!bound, sim)
                        let res = compressedMapAutomata(ba, fun _ m -> m)
                        res
                let sim = productFilter (unitAutomata) sim (fun _ y -> Some y) (fun _ y -> [y])
                yield sim
        |]


    printfn "Begin compositions"

    let set_cartesian op S1 S2 =
        Set.fold (fun RS y -> Set.fold (fun RS x -> Set.add (op x y) RS) RS S1) Set.empty S2

    let calc in1 in2 out1 = 
        printfn "Next product start: %O"  (new System.TimeSpan (System.DateTime.Now.Ticks - start))
        printfn "Calc (%d,%d) -> %d" (in1) (in2) out1
        auto.[out1] <- 
            composeFilter auto.[in1] auto.[in2] 
                    ( fun x bx y by ->
                        if (snd x = snd y) then
                            let b = snd x
                            let x = fst x
                            let y = fst y
                            Some ({ left_external_val = if bx then x.left_external_val else dont_care
                                    right_external_val = if by then y.right_external_val else dont_care
                                    left_internal_val = x.left_internal_val 
                                    right_internal_val = y.right_internal_val
                                    middle_vals = set_cartesian (@) x.middle_vals y.middle_vals}, b)
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
                    (!bound = 0)
                    show_automata
        //show_automata auto.[out1]
        auto.[out1].RemoveNoEdges true
        let f = fun m (hs : ISet<_>) -> hs.UnionWith ((fst (auto.[out1].value m)).middle_vals)
        let reach_rep = if no_compress then fun x -> let hs = new HashSet<_>() in f x hs; hs :> ISet<_> else reach_repeatedly auto.[out1] f
        let newauto =  compressedMapAutomata(auto.[out1], fun x y -> {fst y with middle_vals = reach_rep x |> Set.ofSeq }, snd y )
        let newauto =  productFilter (unitAutomata) newauto (fun _ y -> Some y) (fun _ y -> [y])
        auto.[out1] <- newauto
        if show_composition_steps then show_automata auto.[out1] 

    if binary_combine then
        let mutable steps = no_of_cells
        while steps > 1 do
            let carryover = steps % 2 = 1 
            steps <- (steps >>> 1 ) 
            for c = 0 to steps - 1 do
                calc (c*2) (c*2+1) c
                //show_automata auto.[c]
            if carryover then 
                printfn "Carry over %d -> %d" (steps*2) steps
                auto.[steps] <- auto.[steps * 2 ]
                steps <- steps + 1
    else
        for c = 1 to no_of_cells - 1 do
            //Use 0 entry as the accumulater
            calc 0 c 0         

    printfn "Pre tidy Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))
    //show_automata auto.[0]

    let tidy_auto = compressedMapAutomata (auto.[0], fun _ x -> String.Join(",\n ", Set.map (fun (x : Map<_,_> list) -> "[" + (String.Join(";", x)) + "]") ((fst x).middle_vals)))

    printfn "Time:  %O" (new System.TimeSpan (System.DateTime.Now.Ticks - start))

    //show_automata tidy_auto
    for s in tidy_auto.states do
        printfn "State: %O" (tidy_auto.value s)

