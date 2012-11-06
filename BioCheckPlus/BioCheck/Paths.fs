(* Copyright (c) Microsoft Corporation. All rights reserved. *)
//Begin <-- Added by Qinsi Wang
// This module is used to unroll the system (model) step by step
// and compute and record possible value range for all variable at each time step
// until the range does not change any more
module Paths

let OutputPaths network bounds naive_encoding=
    
    let mutable paths = [bounds]
    
    let mutable step = 0
    
    let mutable pathLength = 0

    let nubounds = ref bounds
        
    let mutable con = true 
    while con && not (naive_encoding) do
            
        nubounds := stepZ3rangelist.find_paths network step !nubounds bounds
        con <- List.forall (fun elem -> elem <> !nubounds) paths 
            
        if con then

            paths <- paths @ [!nubounds]
            pathLength <- step
            step <- step + 1
               
    paths

// Extend/truncate the list of paths to the required length
// If the list of paths is shorter than needed repeat the last element 
// If the list of paths is longer than needed remove the prefix of the list
let change_list_to_length (paths : Map<QN.var, int list> list) (length : int) =
    let changed_length_paths = 
        if (length > paths.Length) 
        then 
            let mutable (temp_paths : Map<QN.var, int list> list) = paths
            while (length > temp_paths.Length) do
                temp_paths <- temp_paths @ [ List.head (List.rev temp_paths) ]
            temp_paths
        elif (length < paths.Length)
        then
            let mutable (temp_paths : Map<QN.var, int list> list) = paths
            while (length < temp_paths.Length) do
                temp_paths <- List.rev (List.tail (List.rev temp_paths))
            temp_paths
        else
            paths        

    changed_length_paths      
//End <-- The list of value range