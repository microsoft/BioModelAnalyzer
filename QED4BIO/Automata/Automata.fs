module Automata


let set_collect f s = Set.unionMany (Set.map f s) 

 
/// Abstract class representing automata
[<AbstractClass>]
type Automata<'state , 'data> when 'state : comparison and 'data:equality  () = 
   
   abstract member next : 'state -> Set<'state>
   abstract member value : 'state -> 'data
   abstract states : Set<'state> 
   abstract member initialstates: Set<'state>

   member a.reachablestates () : Set<_> =
     let rec rs_inner (workset : Set<_>) reached = 
        let nextstates = set_collect (fun x -> a.next(x)) workset
        let unreached_nextstates = Set.difference nextstates reached
        if Set.isEmpty unreached_nextstates then 
            reached
        else
            rs_inner unreached_nextstates (Set.union reached unreached_nextstates)
     rs_inner a.initialstates a.initialstates

    member a.Graph (graph : Microsoft.Msagl.Drawing.Graph) = 
      let add_node x =
        let node = new Microsoft.Msagl.Drawing.Node(x.ToString())
        let label = "(" + x.ToString() + "): " + ((a.value x).ToString())
        node.LabelText <- label
        if Set.contains x a.initialstates then
                node.Attr.FillColor <- Microsoft.Msagl.Drawing.Color.Beige
        graph.AddNode(node) |> ignore

      let rec rs_inner (workset : Set<'state>) reached =
        let nextstates = 
             set_collect 
                (fun x -> 
                    let n = a.next(x)
                    add_node x
                    for y in n do 
                        add_node y
                        graph.AddEdge(x.ToString(),y.ToString()) |> ignore
                    n
                ) workset
        let unreached_nextstates = Set.difference nextstates reached
        if Set.isEmpty unreached_nextstates then 
            reached
        else
            rs_inner unreached_nextstates (Set.union reached unreached_nextstates)
      rs_inner a.initialstates a.initialstates
   
    member a.simulates<'state2 when 'state2 : comparison> (b : Automata<'state2, 'data>) : bool =
        let relation = new System.Collections.Generic.Dictionary<'state, Set<'state2>>()
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
                    if Set.forall (fun nx -> Set.exists (fun ny -> relation.[nx].Contains ny) (b.next y)) (a.next x) then
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
    let mutable startSet = Set.empty
    let mutable statesSet = Set.empty
    let mutable nextMap = System.Collections.Generic.Dictionary<'state, Set<'state>>()
    let mutable dataMap = System.Collections.Generic.Dictionary<'state, 'data>()


    //The method for the automata
    override this.next(s) = 
        match nextMap.TryGetValue(s) with
        | true, x -> x
        | false, _ -> Set.empty
    override this.initialstates = startSet
    override this.states = statesSet
    override this.value s = dataMap.[s]

    member this.addState(x,d) = 
        statesSet <- Set.add x statesSet
        if dataMap.ContainsKey x then
            dataMap.[x] <- d
        else
            dataMap.Add(x,d)

    member this.addEdge(x,y) =
        //TODO Add some checks 
        if nextMap.ContainsKey x then 
            nextMap.[x] <- Set.add y (this.next x) 
        else
            nextMap.Add(x, Set.singleton y)

    member this.addInitialState(x) =
        startSet <- Set.add x startSet

type BoundedAutomata<'istate, 'data> when 'istate : comparison and 'data : equality
    (bound : int, 
     inner : Automata<'istate,'data>)  = 
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
                       nexts := set_collect (fun x -> inner.next(x)) !nexts
                } |> Set.ofSeq

          override this.value((s,i)) = inner.value(s)

          override this.states = 
            seq {
                //All states of the inner automata with all bounds. 
                //Some states might not be reachable.
                for s in inner.states do
                    for i = -bound to bound do
                        yield (s,i)
            } |> Set.ofSeq

          override this.initialstates =
            let mutable nreach = inner.initialstates
            let mutable result = Set.empty
            for i = 0 to bound do
                result <- Set.union (Set.map (fun x -> (x,i)) nreach) result
                nreach <- set_collect (fun x -> inner.next(x)) nreach
            result

let opt_default d o = match o with | Some x -> x | None -> d

let find_refl m x = Map.tryFind x m |> opt_default x

let compressedMapAutomata 
    (inner : Automata<'state,'data1>, 
     f : 'state -> 'data1 -> 'data2) : SimpleAutomata<int, 'data2> = 
        //Get initial sample of the data
        let next_eq eq x = Set.map (find_refl eq) (inner.next x)
        let canonise eq x = (f x (inner.value x), next_eq eq x)
        let rec inner_loop eq_prev =
            let mutable reps = Set.empty
            let mutable canons = Map.empty
            let mutable eq_new = Map.empty
            for x in inner.states do
                 match Map.tryFind (canonise eq_prev x) canons with
                 | Some y -> 
                    eq_new <- Map.add x y eq_new
                 | None -> 
                    canons <- Map.add (canonise eq_prev x) x canons 
                    reps <- Set.add x reps
                    eq_new <- Map.add x x eq_new
            if eq_new <> eq_prev then inner_loop eq_new
            else eq_new, reps

        //Make all elements equal initially
        let first_canon = inner.states.MinimumElement
        let first_eq = Set.fold (fun m x -> Map.add x first_canon m) Map.empty inner.states
        let eq,reps = inner_loop first_eq
        
        let reps_arr = Set.toArray reps
        let newAuto = new SimpleAutomata<int, 'data2>()
        let mutable reps_map = Map.empty
        for i = 0 to reps_arr.Length - 1 do 
            reps_map <- Map.add reps_arr.[i] i reps_map
            newAuto.addState(i, f reps_arr.[i] (inner.value reps_arr.[i]))
        for x in reps_arr do
            for y in next_eq eq x do 
                newAuto.addEdge((reps_map.TryFind x).Value,(reps_map.TryFind y).Value)

        for x in inner.initialstates do
            newAuto.addInitialState ((reps_map.TryFind (Map.find x eq)).Value)
        newAuto

let productFilter 
    (left : Automata<'lstate, 'ldata>) 
    (right : Automata<'rstate, 'rdata>)
    (f : 'ldata -> 'rdata -> 'data option)
    : SimpleAutomata<'lstate * 'rstate, 'data>
    =
    let result = new SimpleAutomata<'lstate * 'rstate, 'data>()

    let work_set = ref Set.empty
    let reached = ref Set.empty
    let add_node li ri pred_option =
        match f (left.value li) (right.value ri) with
        | None -> ()
        | Some d -> 
            result.addState( (li,ri), d)

            if Set.contains (li,ri) !reached then 
                ()
            else
                work_set := (!work_set).Add (li,ri)
                reached := (!reached).Add (li,ri)

            match pred_option with 
            | Some lri -> result.addEdge( lri,  (li,ri))
            | None -> result.addInitialState (li,ri)
        
                
    for li in left.initialstates do
        for ri in right.initialstates do
            add_node li ri None

    while not (!work_set).IsEmpty do
        let li,ri = (!work_set).MaximumElement
        work_set := (!work_set).Remove ((li,ri))
        let ln = left.next(li)
        let rn = right.next(ri)
        for lni in ln do
            for rni in rn do
                add_node lni rni (Some (li,ri))
    
    result    

let unitAutomata =
    let a = new SimpleAutomata<unit,unit>()
    a.addState((),())
    a.addInitialState ()
    a.addEdge((), ())
    a
    