// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System
open System.Drawing
open System.Windows.Forms
open Microsoft.FSharp.Collections

let set_collect f s = Set.unionMany (Set.map f s) 

/// Abstract class representing automata
[<AbstractClass>]
type Automata<'state , 'data> when 'state : comparison () = 
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
     let rec rs_inner (workset : Set<'state>) reached = 
        let nextstates = 
             set_collect 
                (fun x -> 
                    let n = a.next(x)
                    let node = new Microsoft.Msagl.Drawing.Node(x.ToString())
                    let label = x.ToString() + "," + ((a.value x).ToString())
                    node.LabelText <- label
                    graph.AddNode(node) |> ignore
                    for y in n do 
                        let node = new Microsoft.Msagl.Drawing.Node(y.ToString())
                        let label = y.ToString() + "," + ((a.value y).ToString())
                        node.LabelText <- label
                        graph.AddNode(node) |> ignore
                        graph.AddEdge(x.ToString(),y.ToString()) |> ignore
                    n
                ) workset
        let unreached_nextstates = Set.difference nextstates reached
        if Set.isEmpty unreached_nextstates then 
            reached
        else
            rs_inner unreached_nextstates (Set.union reached unreached_nextstates)
     rs_inner a.initialstates a.initialstates
   
type SimpleAutomata<'data> (sd) =
    inherit Automata<int, 'data>() with 
    let start = 0
    let mutable statesSet = Set.singleton 0
    let mutable nextMap = Map.empty
    let mutable dataMap = Map.add 0 sd Map.empty


    //The method for the automata
    override this.next(s) = 
        match Map.tryFind s nextMap with
        | Some x -> x
        | None -> Set.empty
    override this.initialstates = Set.singleton 0
    override this.states = statesSet
    override this.value s = Map.find s dataMap

    member this.addState(x,d) = 
        statesSet <- Set.add x statesSet
        dataMap <- Map.add x d dataMap

    member this.addEdge(x,y) =
        //TODO Add some checks 
        nextMap <- Map.add x (Set.add y (this.next x)) nextMap


type BoundedAutomata<'istate, 'data> when 'istate : comparison 
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
            Set.map (fun x -> (x,0)) inner.initialstates

let opt_default d o = match o with | Some x -> x | None -> d

let find_refl m x = Map.tryFind x m |> opt_default x

let compressedMapAutomata 
    (inner : Automata<'state,'data1>, 
     f : 'data1 -> 'data2) : SimpleAutomata<'data2> = 
        //Get initial sample of the data
        let next_eq eq x = Set.map (find_refl eq) (inner.next x)
        let canonise eq x = (f (inner.value x), next_eq eq x)
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

        let eq,reps = inner_loop (Map.empty)
        
        let reps_arr = Set.toArray reps
        let newAuto = new SimpleAutomata<'data2>(f (inner.value reps_arr.[0]))
        let mutable reps_map = Map.add reps_arr.[0] 0 Map.empty
        for i = 1 to reps_arr.Length - 1 do 
            reps_map <- Map.add reps_arr.[i] i reps_map
            newAuto.addState(i, f (inner.value reps_arr.[i]))
        for x in reps_arr do
            for y in next_eq eq x do 
                newAuto.addEdge((reps_map.TryFind x).Value,(reps_map.TryFind y).Value)
        newAuto

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
