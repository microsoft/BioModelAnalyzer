(* Copyright (c) Microsoft Corporation. All rights reserved. *)
// License: MIT. See LICENSE
module Attractors

// SW: currently has duplicated code from BioCheckPlusZ3.fs

open System.Runtime.InteropServices
open FSharp.NativeInterop
open Expr

[<DllImport("AttractorsDLL.dll", CallingConvention=CallingConvention.Cdecl)>]
extern int attractors(int numVars, int[] ranges, int[] minValues, int[] numInputs, int[] inputVars, int[] numUpdates, int[] inputValues, int[] outputValues,
                      string proofOutput, int proofOutputLength, string csvHeader, int headerLength, int mode)

let private apply_target_function value target min max =
    if value < target && value < max then value + 1
    elif value > target && value > min then value - 1
    else value

let private my_round real_input = 
    let int_round = int (real_input + 0.5)
    int_round

// Convert an expression to a real value based on map from variables to values
let rec private expr_to_real (qn : QN.node list) (node : QN.node) expr var_values = 
    let rec tr expr = 
        match expr with
        | Var v -> 
            let node_min,node_max = node.range 
            let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
            let v_min,v_max = v_defn.range 
            let scale,displacement = 
                if (v_min <> v_max) then
                    let t = float (node_max - node_min)
                    let b = float (v_max - v_min)
                    (t/b,float (node_min-v_min))
                else (1.0,0.0)
            let var_value = Map.find v var_values
            (float (var_value))*scale + displacement
        | Const c -> float (c)
        | Plus(e1,e2) -> (tr e1) + (tr e2)
        | Minus(e1,e2) -> (tr e1) - (tr e2)
        | Times(e1,e2) -> (tr e1) * (tr e2)
        | Div(e1,e2) -> (tr e1) / (tr e2)
        | Max(e1,e2) -> 
            if (tr e1) > (tr e2) then (tr e1)
            else (tr e2) 
        | Min(e1,e2) -> 
            if (tr e1) < (tr e2) then (tr e1)
            else (tr e2) 
        | Ceil e1 -> ceil(tr e1)
        | Abs e1 -> abs(tr e1)
        | Floor e1 -> floor (tr e1)
        | Ave exprs ->
            let sum = List.fold (fun cur_sum e1 -> cur_sum + (tr e1)) 0.0 exprs
            float (sum) / (float (List.length exprs))
        | Sum exprs ->
            List.fold (fun cur_sum e1 -> cur_sum + (tr e1)) 0.0 exprs
    tr expr

let rec combs_with_rep k xxs =
  match k, xxs with
  | 0,  _ -> [[]]
  | _, [] -> []
  | k, x::xs ->
      List.map (fun ys -> x::ys) (combs_with_rep (k-1) xxs)
      @ combs_with_rep k xs

let private generateQNTable (qn:QN.node list) (ranges : Map<QN.var,int list>) (node : QN.node) =
    let inputnodes =
        node :: List.concat
                     [ for var in node.inputs do
                           yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) qn) ]
    let list_of_ranges = List.fold (fun acc (node : QN.node) -> (Map.find node.var ranges)::acc) [] inputnodes

    let list_of_possible_combinations = 
        List.rev list_of_ranges |> Seq.fold (fun acc xs -> [for x in xs do for ac in acc -> List.rev(x::(List.rev ac))]) [[]]  
    let create_map_from_var_to_values list_of_nodes (list_of_values : int list) = 
        let node_to_var (node : QN.node) = node.var
        let convert_node_list_to_var_list node_list = List.map node_to_var node_list
        List.zip (convert_node_list_to_var_list list_of_nodes) list_of_values |> Map.ofList
    let list_of_targets = 
        List.map (fun elem -> expr_to_real qn node node.f (create_map_from_var_to_values inputnodes elem)) list_of_possible_combinations 
    let list_of_int_targets = 
        List.map my_round list_of_targets    
    let list_of_actual_next_vals =
        let compute_actual_next_val target_val inputs_vals =
            let range = Map.find node.var ranges
            let min, max = List.head range, List.rev range |> List.head
            let temp = apply_target_function (List.head inputs_vals) target_val min max
            temp - min

        List.map2 compute_actual_next_val list_of_int_targets list_of_possible_combinations
    let temp = list_of_possible_combinations |> List.map (List.map2 (fun (input : QN.node) v -> 
                                                                         let min = Map.find input.var ranges |> List.head
                                                                         if v - min < 0 then printfn "%A" (Map.find input.var ranges)
                                                                         v - min) inputnodes)
    let list_of_possible_combinations = temp
    list_of_possible_combinations, list_of_actual_next_vals

let rangeToList _ (min, max) = [min .. max]

type Mode = Sync | Async

let findAttractors mode proof_output qn =
    printfn "Calling VMCAI stabilisation algorithm to narrow ranges..."
    let vmcai = match Stabilize.stabilization_prover qn true Counterexample.Synchronous |> fst with
                | Result.SRStabilizing history -> printfn "Stabilised."; history
                | Result.SRNotStabilizing history -> printfn "Failed to stabilise."; history
    let vmcaiBounds = List.head vmcai |> snd
    let ranges = Map.map rangeToList vmcaiBounds

    printfn "Building QN table..."
    let qn = qn |> List.sortBy (fun n -> n.var) // important to sort to match ranges map ordering
    let qnVars = qn |> List.map (fun n -> n.var)
    let inputValues, outputValues = qn |> List.map (generateQNTable qn ranges) |> List.unzip
    let inputVars = qn |> List.map (fun n -> n.var :: List.filter (fun x -> not (x = n.var)) n.inputs)
                       |> List.map (List.map (fun x -> List.findIndex ((=) x) qnVars)) // convert BMA index to 0-based index

    let numInputs = List.map List.length inputVars |> Array.ofList
    let inputVars' = List.reduce (@) inputVars |> Array.ofList
    let numUpdates = List.map List.length outputValues |> Array.ofList
    let inputValues' = List.reduce (@) inputValues |> List.reduce (@) |> Array.ofList
    let outputValues' = List.reduce (@) outputValues |> Array.ofList

    let minValues = Map.toArray ranges |> Array.map (fun (_, x) -> List.head x)
    let ranges' = Map.toArray ranges |> Array.map (fun (_, x) -> List.length x - 1)
    let variables = qn |> List.map (fun n -> n.name)

    printfn "variables: %A" variables
    printfn "ranges: %A" ranges'
    printfn "minValues: %A" minValues
    
    let header = List.reduce (fun x y -> x + "," + y) variables 
    let printRange min range =
        seq { for i in min .. min + range do yield string i } |> Seq.reduce (fun x y -> x + "; " + y)
    let vmcaiBounds = Array.zip minValues ranges' |> Array.map (fun (min, range) ->
                                                                    if range = 0 then string min
                                                                    else "[" + printRange min range + "]") |> Array.reduce (fun x y -> x + "," + y)
    System.IO.File.WriteAllLines(proof_output + "_VMCAI_bounds.csv", [| header; vmcaiBounds |])

    printfn "Calling DLL..."
    let mode = match mode with
               | Sync -> 0
               | Async -> 1
                                             
    attractors(List.length qn, ranges', minValues, numInputs, inputVars', numUpdates, inputValues', outputValues',
               proof_output, String.length proof_output, header, String.length header, mode) |> ignore
