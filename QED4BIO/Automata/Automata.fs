module Automata
open System.Collections.Generic

let dont_care = Map.empty

let cache (f : 'a -> 'b) =
    let cache = new System.Collections.Generic.Dictionary<'a, 'b>()
    fun x -> 
        match cache.TryGetValue(x) with
        | true, interps -> interps 
        | false, _ ->
            let v = f x 
            cache.Add(x,v)
            v

let set_collect (f : _ -> HashSet<_> -> unit) (s : IEnumerable<_>) = 
    let res = new HashSet<_>()
    for i in s do 
        f i res
    res

let skip = fun _ -> () in
    
//Stolen from SLAyer
let inline dfs_iter nexts pre post next_start starts =
        let visited = new HashSet<_>()
        in
        let rec dfs_visit u =
          if visited.Add u then 
            pre u 
            for v in nexts u do
              dfs_visit v
            post u
        in
        Seq.iter (fun start -> dfs_visit start ; next_start()) starts
        visited

//Stolen from SLAyer
let inline scc f nexts preds states = 
    let rev_postorder = ref [] in
    let add_to rl = fun v -> rl := v :: !rl in
    (* Get the finished times for each node *)
    dfs_iter nexts skip
      (add_to rev_postorder)
      skip
      states |> ignore
    (* Walk backwards in reverse finished time *)
    let current_scc = ref [] in
    let scc_map = new Dictionary<_,_>() in
    dfs_iter preds
      skip
      (add_to current_scc)
      (fun () ->
        (* Add each vertex in the scc to the map, with the whole SCC *)
        let res = f !current_scc
        List.iter
          (fun v ->
            scc_map.Add(v, res)
          )
          (!current_scc) ;
        (* Setup next scc *)
        current_scc := []
      ) 
      !rev_postorder |> ignore
    scc_map
 
/// Abstract class representing automata
[<AbstractClass>]
type Automata<'state , 'data> when 'state : comparison and 'data:equality  () = 
   
   abstract member next : 'state -> ISet<'state>
   abstract member value : 'state -> 'data
   abstract states : ICollection<'state> 
   abstract member initialstates: ISet<'state>


   member a.reachablestates () : ISet<'state> =
     dfs_iter a.next skip skip skip a.initialstates :> ISet<_>

   member a.Graph (graph : Microsoft.Msagl.Drawing.Graph) = 
        let add_node x =
            let node = new Microsoft.Msagl.Drawing.Node(x.ToString())
            let label = "(" + x.ToString() + "): " + ((a.value x).ToString())
            node.LabelText <- label
            if a.initialstates.Contains x then
                    node.Attr.FillColor <- Microsoft.Msagl.Drawing.Color.Beige
            else if (a.next x).Count = 0 then 
                    //printf "Warning node with no sucessors!\n %s" label
                    node.Attr.FillColor <- Microsoft.Msagl.Drawing.Color.RosyBrown
            graph.AddNode(node) |> ignore
      
        let add_edges x = 
            let next = a.next x
            if next.Count = 0 then 
                printfn "Warning node with no sucessors! %s" (x.ToString())
            for y in next do
                add_node y
                graph.AddEdge(x.ToString(),y.ToString()) |> ignore

        for i in a.initialstates do
            add_node i

        dfs_iter a.next add_edges skip skip a.initialstates

   
   member a.simulates<'state2 when 'state2 : comparison> (b : Automata<'state2, 'data>) : bool =
        let relation = new Dictionary<'state, Set<'state2>>()
        //initialise relation to relate things with same values.
        for x in a.states do
            relation.Add(x, Set.empty)
            for y in b.states do
                if a.value x = b.value y then
                   match relation.TryGetValue x with
                   | true, vs -> relation.[x] <- Set.add y vs
                   | false, _ -> relation.Add(x, Set.singleton y)
        
        //Build a simulation, by repeated refining
        let mutable change = true
        let mutable no_sim = false
        while change && not no_sim do
            change <- false
            for x in Array.ofSeq relation.Keys do
                for y in relation.[x] do 
                    if Seq.forall (fun nx -> Seq.exists (fun ny -> relation.[nx].Contains ny) (b.next y)) (a.next x) then
                        ()
                    else
                        change <- true
                        relation.[x] <- relation.[x].Remove y 
                if relation.[x].IsEmpty then no_sim <- true
        
        //Check initial states are in relation
        for ix in a.initialstates do
            if not (Set.exists  b.initialstates.Contains  (relation.[ix])) then 
               no_sim <- true

        not no_sim


type SimpleAutomata<'state, 'data when 'state : comparison and 'data : equality> () =
    inherit Automata<'state, 'data>() with 
    let mutable startSet = new HashSet<_>()
    let mutable nextMap = new Dictionary<'state, HashSet<'state>>()
    let mutable dataMap = new Dictionary<'state, 'data>()
    let mutable prevMap = new Dictionary<'state, HashSet<'state>>()
    

    //The method for the automata
    override this.next(s) = 
        match nextMap.TryGetValue(s) with
        | true, x -> x :> ISet<_>
        | false, _ -> 
            let set = new HashSet<_>()
            nextMap.Add(s, set) |> ignore
            set :> ISet<_>

    member this.pred(s) = 
        match prevMap.TryGetValue(s) with
        | true, x -> x :> ISet<_>
        | false, _ -> 
            let set = new HashSet<_>()
            prevMap.Add(s, set) |> ignore
            set :> ISet<_>

    override this.initialstates = startSet :> ISet<_>
    override this.states = dataMap.Keys :> ICollection<_>
    override this.value s = dataMap.[s]

    member this.addState(x,d) = 
        if dataMap.ContainsKey x then
            dataMap.[x] <- d
        else
            dataMap.Add(x,d)

    member this.addEdge(x,y) =
        //TODO Add some checks 
        match nextMap.TryGetValue x with
        | true, v ->  
            v.Add y |> ignore
        | false, _ ->  
            let set = new HashSet<_>()
            set.Add y |> ignore
            nextMap.Add(x, set) |> ignore
        match prevMap.TryGetValue y with
        | true, v -> 
            v.Add x |> ignore
        | false, _ -> 
            let set = new HashSet<_>()
            set.Add(x) |> ignore
            prevMap.Add(y, set) |> ignore

    ///  Remove nodes that have no edges leaving in a specified direction
    ///  if direction = true, then remove nodes with no successors
    ///  if direction = false, then remove nodes with no predecessors
    member this.RemoveNoEdges direction =
        let said_error = ref false
        let worklist = ref []
        let outs = if direction then this.next else this.pred
        let prop = if direction then this.pred else this.next
        let try_remove i =
            if (outs i).Count = 0 then 
                if !said_error then 
                    ()
                else
                    //Add this for checking stuff later
                    //printfn "Warning removing a node with no successors!"
                    said_error := true
                dataMap.Remove i |> ignore
                startSet.Remove i |> ignore
                for j in prop i do 
                    assert (j<>i)
                    if direction then nextMap.[j].Remove(i) |> ignore else prevMap.[j].Remove(i) |> ignore
                    worklist := j :: !worklist

        for i in this.states |> Array.ofSeq do
            try_remove i 

        let rec work () = 
            match !worklist with 
            | x :: rest -> 
                worklist := rest
                try_remove x 
                work ()               
            | [] -> ()
        work () 

    member this.addInitialState(x) =
        startSet.Add x |> ignore

type BoundedAutomata<'istate, 'data> when 'istate : comparison and 'data : equality
    (bound : int, 
     inner : Automata<'istate,'data>,
     allow_negative : bool )  = 
       inherit Automata<('istate * int), 'data>() with
          override this.next((s,i)) =
                seq {
                    //Can produce the same result
                    if i > -bound then 
                        yield (s, i - 1) 
                    //Produce all results skipping ahead up to the bound
                    let nexts = ref (inner.next(s))
                    for j = i to bound do
                       yield! Seq.map (fun (x : 'istate) -> (x, j)) !nexts
                       nexts := set_collect (fun x rs -> rs.UnionWith (inner.next x)) !nexts :> ISet<_>
                } |> fun x -> new HashSet<_>(x) :> ISet<_>

          override this.value((s,i)) = inner.value(s)

          override this.states = 
            seq {
                //All states of the inner automata with all bounds. 
                //Some states might not be reachable.
                for s in inner.states do
                    for i = -bound to bound do
                        yield (s,i)
            } |> Set.ofSeq :> ICollection<_>

          override this.initialstates =
            let mutable nreach = new HashSet<_>(inner.initialstates)
            let mutable result = new HashSet<_>()
            for i = 0 to (if allow_negative then bound else 0) do
                result.UnionWith (set_collect (fun x rs -> rs.Add (x,i) |> ignore) nreach)
                nreach <- set_collect (fun x rs -> rs.UnionWith (inner.next x)) nreach
            result :> ISet<_>

let opt_default d o = match o with | Some x -> x | None -> d

let find_refl m x = Map.tryFind x m |> opt_default x

let build_eq<'a, 'state3,'data3 when 'a : comparison and 'state3 : equality and 'state3 : comparison and 'data3 : comparison> 
    (inner : Automata<'state3, 'data3>) 
    (canonise : Dictionary<'state3,'state3> -> 'state3 -> 'a) 
    =
    let rec inner_loop eq_prev reps_count =
        let reps = new Dictionary<_,_>()
        let canons = new Dictionary<_,'state3>()
        let eq_new = new Dictionary<'state3,'state3>()
        for x in inner.states do
                let v = canonise eq_prev x
                match canons.TryGetValue( v) with
                | true, y -> 
                    eq_new.Add(x,y)
                | false, _ -> 
                    canons.Add (v,x)
                    reps.Add(x,true) |> ignore
                    eq_new.Add(x,x)
        if reps_count <> reps.Count then inner_loop eq_new reps.Count
        else eq_new, reps.Keys
    //Make all elements equal initially
    let enum = inner.states.GetEnumerator()
    assert (enum.MoveNext())
    let first_canon = enum.Current
    let first_eq = new Dictionary<'state3,'state3>()
    do Seq.iter (fun x -> first_eq.Add(x,first_canon)) inner.states
    inner_loop first_eq inner.states.Count

let compressedMapAutomata 
    (inner : Automata<'state,'data1>, 
     f : 'state -> 'data1 -> 'data2) : SimpleAutomata<int, 'data2> = 
        //Get initial sample of the data
        let next_eq (eq : Dictionary<_,_>) x = 
            Seq.fold (fun res x -> Set.add eq.[x] res) Set.empty (inner.next x)

        let canonise eq x = 
            let newv = f x (inner.value x)
            let newnext =  next_eq eq x
            (newv, newnext)

        let eq,reps = build_eq inner canonise

        let reps_arr = Seq.toArray reps
        let newAuto = new SimpleAutomata<int, 'data2>()
        let mutable reps_map = Map.empty
        for i = 0 to reps_arr.Length - 1 do 
            reps_map <- Map.add reps_arr.[i] i reps_map
            newAuto.addState(i, f reps_arr.[i] (inner.value reps_arr.[i]))
        for x in reps_arr do
            for y in next_eq eq x do 
                newAuto.addEdge((reps_map.TryFind x).Value,(reps_map.TryFind y).Value)

        for x in inner.initialstates do
            newAuto.addInitialState ((reps_map.TryFind (eq.[x])).Value)

        //Do some reverse normalisation now
        let pred_eq (eq : Dictionary<_,_>) x = Seq.fold (fun rs x -> Set.add eq.[x] rs) Set.empty (newAuto.pred x)
        let canonise2 eq x = (newAuto.value x, pred_eq eq x, newAuto.initialstates.Contains x)
        let eq, reps = build_eq newAuto canonise2
        let newAuto2 = new SimpleAutomata<int, 'data2>()
        for x in newAuto.states do
            newAuto2.addState(eq.[x], newAuto.value eq.[x])
            for y in newAuto.next x do
                newAuto2.addEdge(eq.[x], eq.[y])
        for x in newAuto.initialstates do
            newAuto2.addInitialState eq.[x]

        newAuto2

let productFilter 
    (left : Automata<'lstate, 'ldata>) 
    (right : Automata<'rstate, 'rdata>)
    (f : 'ldata -> 'rdata -> 'data option)
    (g : 'lstate -> 'rstate -> 'state)
    : SimpleAutomata<'state, 'data>
    =
    let result = new SimpleAutomata<'state, 'data>()

    let work_set = System.Collections.Concurrent.ConcurrentBag()
    
    let reached = new HashSet<_>()
    let add_node li ri pred_option =
        match f (left.value li) (right.value ri) with
        | None -> ()
        | Some d -> 
            result.addState(g li ri, d)

            if reached.Add( (li,ri) ) then 
                work_set.Add (li,ri)
                

            match pred_option with 
            | Some lri -> result.addEdge( lri,  g li ri)
            | None -> result.addInitialState (g li ri)
        
                
    for li in left.initialstates do
        for ri in right.initialstates do
            add_node li ri None

    let mutable more, liri = work_set.TryTake()
    while more do
        let li = fst liri
        let ri = snd liri
        let ln = left.next(li)
        let rn = right.next(ri)
        for lni in ln do
            for rni in rn do
                add_node lni rni (Some (g li ri))
        more <- work_set.TryTake(&(liri))
    result    

let composeFilter 
    (left : Automata<'lstate, 'ldata>) 
    (right : Automata<'rstate, 'rdata>)
    (f : 'ldata -> bool -> 'rdata -> bool -> 'data option)
    (left_allowed : 'ldata -> 'rdata -> bool)
    (right_allowed : 'rdata -> 'ldata -> bool)
    (g : 'lstate -> 'rstate -> 'state)
    (sync : bool)
    (display : SimpleAutomata<'state, 'data> -> unit)
    : SimpleAutomata<'state, 'data>
    =
    let result = new SimpleAutomata<'state, 'data>()

    let work_set = System.Collections.Concurrent.ConcurrentBag()
    
    let reached = new HashSet<_>()
    let add_node li bl ri br pred_option =
        match f (left.value li) bl (right.value ri) br with        
        | None -> ()
        | Some d -> 
            let s = g li ri
            result.addState(s, d)

            if reached.Add( (li,ri) ) then 
                work_set.Add (li,ri)
                

            match pred_option with 
            | Some lri -> result.addEdge( lri,  s)
            | None -> result.addInitialState (s)
        
//            display result
                
    for li in left.initialstates do
        for ri in right.initialstates do
            add_node li false ri false None

    let mutable more, liri = work_set.TryTake()
    while more do
        let li = fst liri
        let ri = snd liri
        let ln = left.next(li)
        let rn = right.next(ri)
        for lni in ln do
            if left_allowed (left.value lni) (right.value ri) then
                if not sync then add_node lni true ri false (Some (g li ri))
                for rni in rn do
                    if right_allowed (right.value rni) (left.value li) then
                        add_node lni true rni true (Some (g li ri))

        if not sync then 
            for rni in rn do
                if right_allowed (right.value rni) (left.value li) then
                    add_node li false rni true (Some (g li ri))
        //Add self loop, this will make more things equal later
        //add_node li false ri false (Some (g li ri))
        more <- work_set.TryTake(&(liri))
    result    

type NstepBarrierAutomata<'istate, 'data> when 'istate : comparison and 'data : equality
    (bound : int, 
     inner : Automata<'istate,'data>)  = 
     inherit Automata<('istate * int), ('data * bool)>() with      
       let InnerNext = 
        cache (
            fun  (s, i) ->
                seq {
                    //yield (s,i)
                    if i=bound then
                        yield (s,-1)
                    elif i = -1 then
                        yield (s,0)
                    else 
                        for n in inner.next s do
                            yield (n,i+1)
                } |> fun s -> new HashSet<_>(s) :> ISet<_>
        )


       override this.next x = InnerNext x

       override this.value ((s,i)) = 
            let v = inner.value s 
            if i = -1 then (v,true) 
            else (v,false)

       override this.states = 
            seq {
                //All states of the inner automata with all bounds. 
                //Some states might not be reachable.
                for s in inner.states do
                    for i = -1 to bound  do
                        yield (s,i)
            } |> Set.ofSeq :> ICollection<_>

       override this.initialstates = 
        seq {
            for s in inner.initialstates do
                yield (s,0)
        } |> fun s -> new HashSet<_>(s) :> ISet<_>

let unitAutomata =
    let a = new SimpleAutomata<string,string>()
    a.addState("","")
    a.addInitialState ""
    a.addEdge("", "")
    a
    