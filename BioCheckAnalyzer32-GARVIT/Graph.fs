(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Graph

// Basic implementation of a graph. 

type index = int 

type 'a graph = {    
    vertices : Map<index,'a>;
    edges    : Map<index,index> 
    }


let mk_graph _ = { vertices= Map.empty; edges= Map.empty }

let vertex_id = ref -1

let mk_vertex (g) label =
    incr vertex_id
    !vertex_id, {g with vertices = Map.add !vertex_id label g.vertices }

// type error? 
//let mk_vertex = 
//    let vertex_id = ref -1 
//    (fun (g:'a graph) (label:'a) ->
//        incr vertex_id
//        !vertex_id, {g with vertices = Map.add !vertex_id label g.vertices })


let mk_edge g (src:index) (tgt:index) = 
    { g with edges= Map.add src tgt g.edges } 


let dot_of_graph (fname:string) (g:graph<string>) = 
    // open file
    let dot = new System.IO.StreamWriter(fname)
    // write header
    Printf.fprintf dot "digraph Lemmas {\n" 

    // write vertices
    Map.iter 
        (fun idx label -> Printf.fprintf dot "%d [shape=box,fontname=\"Consolas\",label=\"%d\: %s\"]\n" idx idx ((string)label))
        g.vertices

    // write edges
    Map.iter (fun src dest -> Printf.fprintf dot "%d -> %d\n" src dest) g.edges
        
    // write footer
    Printf.fprintf dot "}\n"
    // close file
    dot.Dispose ()
    dot.Close ()