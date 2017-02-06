// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      GGraph.fs
//
//  Abstract:
//
//      Set of useful graph algorithms
//
//  Contact:
//
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//
//  Notes:
//      This file implements the following algorithms over directed graphs
//      1. Tarjan's SCC Decomposition
//      2. Weak Topological Ordering using heirarchical Tarjan's SCC decomposition
//      3. Recursive Iteration Strategy based on WTOs

module GGraph

open System.Collections.Generic

/// type for a graph
/// every vertex must have a different label
type Graph<'label> = {    
    numNodes : int;
    vertices : Map<int, 'label>;
    edges : Set<int * int>
    }

/// return empty graph of specified type of vertices
let Empty<'label> = { 
    numNodes= 0;
    vertices= Map.empty<int, 'label>; 
    edges= Set.empty<int * int> 
    }

let AddVertex vertex graph =
    { graph with numNodes = graph.numNodes+1; vertices = Map.add graph.numNodes vertex graph.vertices }
    

let GetVertexId label graph =
    // get the internal number of a vertex from its label
    // raises KeyNotFoundException in case the label does not exist
    Map.findKey (fun _ lbl -> lbl = label) graph.vertices

let AddEdge src tgt graph= 
    let (s,t) =  try 
                    ((GetVertexId src graph), (GetVertexId tgt graph)) 
                  with 
                     | exn -> failwithf "Vertex %A or %A not found while adding edge" src tgt
                  in
        { graph with edges= Set.add (s,t) graph.edges } 

let AddEdgeInternal src tgt graph= 
    let (s,t) =  match (Map.tryFindKey (fun k v -> k=src) graph.vertices, Map.tryFindKey (fun k v -> k=tgt) graph.vertices) with
                     | (Some vsrc, Some vtgt) -> (vsrc, vtgt)  
                     | (_,_) -> failwithf "Vertex %A or %A not found while adding edge" src tgt
                  in
        { graph with edges= Set.add (s,t) graph.edges }

let IsEmpty graph = 
    graph.numNodes = 0

/// assumes graph has at least one vertex
/// need to generate a default label
let AddRoot graph = 
    let root = graph.numNodes
    let graphWithRootVertex = AddVertex graph.vertices.[0] graph
    let graphWithRoot = Map.fold
                            (fun graph idx _ -> AddEdgeInternal root idx graph)
                            graphWithRootVertex
                            graph.vertices
    (graphWithRoot, root)


let CreateDotFile (fname:string) graph = 
    // open file
    let dot = new System.IO.StreamWriter(fname)
    // write header
    Printf.fprintf dot "digraph Lemmas {\n" 

    // write vertices
    Map.iter 
        (fun idx data -> Printf.fprintf dot "%d [shape=box,fontname=\"Consolas\",label=\"%A\"]\n" idx data)
        graph.vertices

    // write edges
    Set.iter (fun (src, dest) -> Printf.fprintf dot "%d -> %d\n" src dest) graph.edges
        
    // write footer
    Printf.fprintf dot "}\n"
    // close file
    dot.Dispose ()
    dot.Close ()



/// Return the adjacency list of the graph
let GetAdjacencyList graph =
    Set.fold (fun map (s,t) -> 
                   Map.add s (t::map.[s]) map
                   )
        (Map.fold (fun map vertex _ ->
                    Map.add vertex [] map)
                Map.empty
                graph.vertices)
        graph.edges



/// Tarjan's SCC Decomposition from Wikipedia
let GetSCCDecomposition graph =
    let sccList = new List<List<'label>>()


    let index = ref 0
    let dfsStack = new Stack<int>()

    let depthIndex = Array.create graph.numNodes -1  //depth of -1 means depth is undefined
    let lowLink = Array.create graph.numNodes -1 
     
    let succ = GetAdjacencyList graph

    let rec strongConnect vertex =
        begin
            depthIndex.[vertex] <- !index
            lowLink.[vertex] <- !index
            index := !index + 1
            dfsStack.Push(vertex)

            for successor in succ.[vertex] do
                if depthIndex.[successor] = -1 then
                    strongConnect successor
                    lowLink.[vertex] <- min lowLink.[vertex] lowLink.[successor]
                elif dfsStack.Contains(successor) then
                    lowLink.[vertex] <- min lowLink.[vertex] depthIndex.[successor]
            
            if lowLink.[vertex] = depthIndex.[vertex] then
                let scc = new List<'label>()

                let mutable loopCondition = true
                while loopCondition do
                    let w = dfsStack.Pop()
                    scc.Add(graph.vertices.[w])
                    loopCondition <- not(w=vertex)

                sccList.Add(scc)
        end

    for KeyValue(idx,_) in graph.vertices do
        if depthIndex.[idx] = -1 then
            strongConnect idx

    sccList


/// Each component is either a terminal or a list of components
type Component<'label> = 
    | Term of 'label
    | NonTerm of Component<'label> list

/// Weak Topological Order from 
/// Efficient chaotic iteration strategies with widenings : Francois Bourdoncle
/// this implementation uses a lot of mutable data
/// to understand refer to the paper 
/// in the returned WTO a root node is added so this function is for internal use only
let GetWeakTopologicalOrderInternal graph =
    if IsEmpty graph then 
        (-1, NonTerm [])
    else
        let (graph, root) = AddRoot graph

        let index = ref 0
        let dfsStack = new Stack<int>()

        let depthIndex = Array.create graph.numNodes 0 
        let succ = GetAdjacencyList graph


        let rec VisitComponent vertex =
            let mutable (partition:Component<int>) = (NonTerm [])
            for successor in succ.[vertex] do
                if depthIndex.[successor] = 0 then
                    let (_, rpartition) = (Visit successor partition)
                    partition <- rpartition
            (match partition with
                    | Term(lblId) -> failwith "partition should have been a list"
                    | NonTerm(lst) -> NonTerm(Term(vertex) :: lst)
                    )

        and Visit vertex partition=
            dfsStack.Push(vertex)
            incr index
            depthIndex.[vertex] <- !index
            let mutable head = depthIndex.[vertex]
            let mutable loop = false
            let mutable min = System.Int32.MaxValue
            let mutable retPartition = partition

            for successor in succ.[vertex] do
                if depthIndex.[successor]  = 0 then
                    let (rmin, rretPartition) = (Visit successor retPartition)
                    min <- rmin
                    retPartition <- rretPartition
                else
                    min <- depthIndex.[successor]

                if min <= head then
                    head <- min
                    loop <- true

            if head = depthIndex.[vertex] then
                depthIndex.[vertex] <- System.Int32.MaxValue
                let mutable element = dfsStack.Pop()
            
           
                if loop then
                    while not (element = vertex) do
                        depthIndex.[element] <- 0
                        element <- dfsStack.Pop()
                
                    let mutable p = NonTerm []
                    retPartition <- (match retPartition with
                                        | Term(lblId) -> failwith "partition should have been a list"
                                        | NonTerm(lst) -> NonTerm((VisitComponent vertex):: lst)
                                        )
                else 
                    retPartition <- (match retPartition with
                                        | Term(lblId) -> failwith "partition should have been a list"
                                        | NonTerm(lst) -> NonTerm(Term(vertex) :: lst)
                                        )
            (head, retPartition)

        let mutable (partition:Component<int>) = NonTerm []
        let (_, rpartition) = (Visit root partition)
        partition <- rpartition
        (root, partition)

/// WTO for external use
let GetWeakTopologicalOrder graph =
    let rec ReLabel compLst =
        [for comp in compLst -> match comp with
                                | Term lblId -> Term graph.vertices.[lblId]
                                | NonTerm lst -> NonTerm (ReLabel lst)]
    let (root, wto) = GetWeakTopologicalOrderInternal graph
    match wto with
    | NonTerm [] -> []
    | NonTerm(Term(l)::tl) -> ReLabel tl
    | _ -> failwith "For non empty graph, WTO returned from WTOInternal must have a root as its first terminal"


let rec GetDepthOfVertexInWTO (vtx : 'label) (wto : Component<'label> list) =
    let rec GetDepth_h (vtx : 'label) (compLst : Component<'label> list) =
        List.fold 
            (fun acc comp ->
                let compDep =
                    match comp with
                    | Term lbl -> if lbl=vtx then Some 0 else None
                    | NonTerm lst -> let d : int option = (GetDepth_h vtx lst) in 
                                        if d.IsNone then None else Some(1 + Option.get d)
                if compDep.IsNone then acc else compDep)
            None
            compLst
    let depth = GetDepth_h vtx wto
    match depth with
    | None -> failwithf "Vertex %A not present in wto" vtx
    | Some d -> d
/// String representation of wto
/// Applying to_string to convert the labels to strings
let Stringify (wto : Component<'label> list) to_string=
    let rec ToString compLst =
        List.map 
            (fun comp ->
                 match comp with
                    | Term lbl -> to_string lbl
                    | NonTerm lst -> "[ " + (ToString lst) + " ]")
            compLst
        |> String.concat ", "
    ToString wto
    
type Strategy<'label> = {
    next : 'label option;
    exit : 'label option;
    isHead : bool
    }



let GetTrivialStrategy (graph : Graph<'lbl>)= 
    if graph.numNodes = 0 then Map.empty, None
    else
        let firstNode = graph.vertices.[0]
        let lastNode = graph.vertices.[graph.numNodes-1]
        let mutable stgyMap = Map.empty
        
        stgyMap <- Map.add firstNode 
                            { next = Some graph.vertices.[1] ; exit = None; isHead = true}
                            stgyMap
        for nodeNum in 1 .. graph.numNodes-2 do
            stgyMap <- Map.add graph.vertices.[nodeNum] 
                               { next = Some graph.vertices.[nodeNum+1] ; exit = Some graph.vertices.[nodeNum-1]; isHead = true}
                               stgyMap
        stgyMap <- Map.add lastNode
                            { next = Some lastNode ; exit = Some graph.vertices.[graph.numNodes-2]; isHead = true}
                            stgyMap
        (stgyMap, Some graph.vertices.[0])

/// create the recursive strategy from a wto as described in 
/// Efficient chaotic iteration strategies with widenings : Francois Bourdoncle
/// returns a Map<'label, Strategy<'label>
/// strategy has a next field which denote the next vertex in order
/// and a exit field which has Some value only for the vertices which are heads
/// it denotes the next vertex in order after that component (of which it is the head) reaches a fp
/// isHead is true iff the vertex is the head of some component
/// exit is non-None only when isHead is true

/// example: 1 [ 2 [ 3 4 ] ] ] 5 would return
/// 1.next = 2,    1.exit = None, 1.isHead = false
/// 2.next = 3,    2.exit = 5,    2.isHead = true
/// 3.next = 4,    3.exit = 2,    3.isHead = true
/// 4.next = 3,    4.exit = None, 4.isHead = false 
/// 5.next = None, 5.exit = None, 5.isHead = false
let GetRecursiveStrategy graph =
    let (_, wto) = GetWeakTopologicalOrderInternal graph

    /// loopBack is true iff this list of components is closed within another component 
    /// In other words, loopBack is false only when we start with the outermost list when it is not
    /// a compnent in itself. For eg. in case of [1 [ 2 3 ] ] loopBack will be true and 
    /// for 1 [ 2 3 ] loopBack will be false
    let rec CreateStrategy (compLst : Component<int> list) =
        let head = 
            match compLst with
                | Term(lblId) :: rest -> lblId
                | _ -> failwith "Component must have a head"

        ((List.foldBack 
            (fun subcomp (stgyMap, prev) ->
                // stgyMap contains the strategy map constructed so far and prev stores the vertex next in order in the WTO
                // if this subComp is a terminal, then its next field should point to prev
                // if this subComp is a non-terminal, then the exit field of head of subComp would be prev
                match subcomp with
                    | Term(lblId) -> (Map.add lblId { next= prev; exit= None; isHead = (lblId=head) } stgyMap, Some lblId)
                    | NonTerm(lst) ->   let (subStgyMap, subHead) = (CreateStrategy lst)
                                        // combine the maps stgyMap and subStgyMap
                                        // replace the exit field of the head of the subComp to point to prev
                                        (Util.MergeMaps stgyMap subStgyMap
                                            |> Map.add subHead { subStgyMap.[subHead] with exit = prev}, 
                                          Some subHead)
            )
            compLst
            (Map.empty<int, Strategy<int>>, Some head)) |> fst,
            head)

    let DerootStrategy ((strategy : Map<int, Strategy<int>>), (root : int)) =
        let derooted =  
            Map.fold
                (fun map vtx sgy ->
                    if vtx = root then
                        map
                    else
                        let newSgy = 
                            { next = match sgy.next with
                                     | None -> None
                                     | Some v -> if v = root then None else Some graph.vertices.[v]
                              exit = match sgy.exit with
                                     | None -> None
                                     | Some v -> if v = root then None else Some graph.vertices.[v]
                              isHead = sgy.isHead }
                        Map.add graph.vertices.[vtx] newSgy map)
                (Map.empty)
                strategy
        (derooted, Some graph.vertices.[Option.get strategy.[root].next])

   
    
    match wto with
        | Term(lblId) -> failwith "WTO cannot be a terminal"
        | NonTerm [] -> Map.empty, None
        | NonTerm(lst) -> (DerootStrategy (CreateStrategy lst))  
                             

(*
let testGraph1 = Empty<string> |> AddVertex "Alice" |> AddVertex "Bob" |> AddVertex "Charlie" |> AddEdge "Alice" "Bob" |>
                    AddEdge "Bob" "Alice" |> AddEdge "Bob" "Charlie"
let testGraph2 = Empty<int> |> AddVertex 11 |> AddVertex 12 |> AddVertex 13 |> AddVertex 14 |> 
                    AddVertex 15 |> AddVertex 16 |> AddVertex 17 |> AddVertex 18|> AddEdge 11 12 |> 
                        AddEdge 12 13 |> AddEdge 13 14 |> AddEdge 14 15 |> AddEdge 15 16 |> AddEdge 16 17 |>
                            AddEdge 17 18 |> AddEdge 12 18 |> AddEdge 14 17 |> AddEdge 16 15 |> AddEdge 17 13
*)
