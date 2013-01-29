(* Copyright (c) Microsoft Corporation. All rights reserved. *)
//Begin <-- Added by Qinsi Wang
// This module is used to unroll the system (model) step by step
// and compute and record possible value range for all variable at each time step
// until the range does not change any more
module Paths

let output_paths (network : QN.node list) bounds naive_encoding=
    
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

let print_paths (network : QN.node list) (paths : Map<QN.var, int list> list) =
    let mutable all_vars = "time"
    for node in network do
        all_vars <- all_vars + "," + node.name

    printfn "%s" all_vars

    let i = ref 0
    for bound in paths do
        let mutable line = sprintf "%d" !i
        for node in network do
            line <- line + ",["
            let list_of_values = Map.find node.var bound
            let mutable first=true
            for value in list_of_values do
                if not first then line <- line + ":" else first <- false

                let value_string = sprintf "%d" value
                line <- line + value_string

            line <- line + "]"

        printfn "%s" line

        incr i

    ()

// Extend/truncate the list of paths to the required length
// If the list of paths is shorter than needed repeat the last element 
// If the list of paths is longer than needed remove the prefix of the list
let change_list_to_length (paths : Map<QN.var, int list> list) (length : int) =
    let changed_length_paths = 
        if (length < 0)
        then
            paths
        elif (length > paths.Length) 
        then 
            let mutable (temp_paths : Map<QN.var, int list> list) = paths
            while (length > temp_paths.Length) do
                temp_paths <- temp_paths @ [ List.head (List.rev temp_paths) ]
            temp_paths
        elif (length < paths.Length)
        then
            let last_elem = List.nth paths (paths.Length - 1)
            let mutable (temp_paths : Map<QN.var, int list> list) = paths
            while (length - 1 < temp_paths.Length) do
                temp_paths <- List.rev (List.tail (List.rev temp_paths))
            temp_paths <- temp_paths @ [ last_elem ]
            temp_paths
        else
            paths        

    changed_length_paths      
//End <-- The list of value range