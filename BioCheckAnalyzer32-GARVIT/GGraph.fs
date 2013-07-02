////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) 2013  Microsoft Corporation
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
//      1. Weak Topological Ordering using Tarjan's SCC decomposition
//      2. SCC Decomposition


module GGraph

open System.Collections.Generic

/// type for a graph
type Graph<'T when 'T : comparison> = {    
    numNodes : int;
    vertices : Map<'T, int>;
    edges : Set<int * int>
    }

/// return empty graph of specified type of vertices
let Empty<'T when 'T : comparison> = { 
    numNodes= 0;
    vertices= Map.empty<'T, int>; 
    edges= Set.empty<int * int> 
    }

let AddVertex vertex graph =
    { graph with numNodes = graph.numNodes+1; vertices = Map.add vertex graph.numNodes graph.vertices }


let AddEdge src tgt graph= 
    let (s,t) =  try 
                    (graph.vertices.[src], graph.vertices.[tgt]) 
                  with 
                     | exn -> failwithf "Vertex %A or %A not found while adding edge" src tgt
                  in
        { graph with edges= Set.add (s,t) graph.edges } 


let CreateDotFile (fname:string) graph = 
    // open file
    let dot = new System.IO.StreamWriter(fname)
    // write header
    Printf.fprintf dot "digraph Lemmas {\n" 

    // write vertices
    Map.iter 
        (fun data idx -> Printf.fprintf dot "%d [shape=box,fontname=\"Consolas\",label=\"%A\"]\n" idx data)
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
        (Map.fold (fun map _ vertex ->
                    Map.add vertex [] map)
                Map.empty
                graph.vertices)
        graph.edges


let GetSCCDecomposition graph =
    let sccList = new List<List<int>>()


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
                let scc = new List<int>()

                let mutable loopCondition = true
                while loopCondition do
                    let w = dfsStack.Pop()
                    scc.Add(w)
                    loopCondition <- not(w=vertex)

                sccList.Add(scc)
        end


    for KeyValue(data, idx) in graph.vertices do
        if depthIndex.[idx] = -1 then
            strongConnect idx

    sccList

    