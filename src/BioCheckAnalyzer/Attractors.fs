// Copyright (c) Microsoft Research 2017
// License: MIT. See LICENSE
module Attractors

open System.Runtime.InteropServices
open FSharp.NativeInterop
open Simulate
open BioCheckPlusZ3
open Paths

[<DllImport("Attractors.dll", CallingConvention=CallingConvention.Cdecl)>]
extern int attractors(int numVars, int[] ranges, int[] minValues, int[] numInputs, int[] inputVars, int[] numUpdates, int[] inputValues, int[] outputValues,
                      string proofOutput, int proofOutputLength, string csvHeader, int headerLength, int mode, string initialCsvFilename, int initialCsvFilenameLength)

type Mode = Sync | Async

let values = Map.toList >> List.unzip >> snd

let rangeToList (min, max) = [min .. max]

let collapseRange l1 l2 = [min (List.min l1) (List.min l2) ..  max (List.max l1) (List.max l2)]

let collapseRanges initial rest =
    let mutable ranges = initial
    for r in rest do
        for key, range in Map.toArray r do
            let current = Map.find key ranges
            ranges <- Map.add key (collapseRange current range) ranges
    ranges
    
// SW: currently has duplicated code from BioCheckPlusZ3.fs
let generateQNTable (qn:QN.node list) (ranges : Map<QN.var,int list>) (node : QN.node) =
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
    let list_of_possible_combinations = list_of_possible_combinations |> List.map (List.map2 (fun (input : QN.node) v -> 
                                                                                                let min = Map.find input.var ranges |> List.head
                                                                                                v - min) inputnodes)
    list_of_possible_combinations, list_of_actual_next_vals

let runVMCAI qn =
    printfn "Calling VMCAI stabilisation algorithm to narrow ranges..."
    let vmcai, stable = match Stabilize.stabilization_prover qn true Counterexample.Synchronous |> fst with
                        | Result.SRStabilizing history -> printfn "Stabilised."; history, true
                        | Result.SRNotStabilizing history -> printfn "Failed to stabilise."; history, false
    let vmcaiBounds = List.head vmcai |> snd
    let ranges = Map.map (fun _ x -> rangeToList x) vmcaiBounds
    ranges, stable

let runReachability qn initialRanges =
    printfn "Calling CAV reachability algorithm to narrow ranges..."
    let paths = output_paths qn initialRanges |> List.rev
    let stable = paths |> List.head |> values |> List.forall (fun l -> List.length l = 1)

    if stable then
        List.head paths, stable
    else
        let ranges = collapseRanges initialRanges paths    
        ranges, stable

let parseRange (s : string) =
    let s = s.Trim()
    if s.[0] <> '[' then
        let x = int s
        x, x
    else
        let s = s.Remove(s.Length-1, 1).Remove(0, 1)
        let a = s.Split(';') |> Array.map int
        Array.min a, Array.max a
        
let loadRangesFromCsv sortedQnVars filename =
    let split (s : string) = s.Split(',')
    let parse = Array.map (parseRange >> rangeToList)
    let lines = System.IO.File.ReadLines filename |> Seq.skip 1 // skip header
    let ranges = lines |> Seq.map (split >> parse >> Seq.zip sortedQnVars >> Map.ofSeq) |> List.ofSeq
    collapseRanges (List.head ranges) (List.tail ranges)

let findAttractors mode proof_output qn initialCsvFilename =
    let qn = qn |> List.sortBy (fun (n : QN.node) -> n.var) // important to sort to match ranges map ordering
    let qnVars = qn |> List.map (fun n -> n.var)
    let variables = qn |> List.map (fun n -> n.name)

    let ranges, stable = runVMCAI qn
    let ranges, stable = if initialCsvFilename = "" || stable then
                             ranges, stable
                         else
                             runReachability qn (loadRangesFromCsv qnVars initialCsvFilename)

    let initialCsvFilename = if stable then "" else initialCsvFilename

    let minValues = Map.toArray ranges |> Array.map (fun (_, x) -> List.head x)
    let ranges' = Map.toArray ranges |> Array.map (fun (_, x) -> List.length x - 1)
    let header = List.reduce (fun x y -> x + "," + y) variables 
    let printRange min range =
        seq { for i in min .. min + range do yield string i } |> Seq.reduce (fun x y -> x + "; " + y)
    let bounds = Array.zip minValues ranges' |> Array.map (fun (min, range) ->
                                                                    if range = 0 then string min
                                                                    else "[" + printRange min range + "]") |> Array.reduce (fun x y -> x + "," + y)
    System.IO.File.WriteAllLines(proof_output + "_bounds.csv", [| header; bounds |])

    printfn "Building QN table..."
    let inputValues, outputValues = qn |> List.map (generateQNTable qn ranges) |> List.unzip
    let inputVars = qn |> List.map (fun n -> n.var :: List.filter (fun x -> not (x = n.var)) n.inputs)
                        |> List.map (List.map (fun x -> List.findIndex ((=) x) qnVars)) // convert BMA index to 0-based index

    let numInputs = List.map List.length inputVars |> Array.ofList
    let inputVars' = List.reduce (@) inputVars |> Array.ofList
    let numUpdates = List.map List.length outputValues |> Array.ofList
    let inputValues' = List.reduce (@) inputValues |> List.reduce (@) |> Array.ofList
    let outputValues' = List.reduce (@) outputValues |> Array.ofList

    printfn "Calling DLL..."
    let mode = match mode with
                | Sync -> 0
                | Async -> 1
                                             
    attractors(List.length qn, ranges', minValues, numInputs, inputVars', numUpdates, inputValues', outputValues',
                proof_output, String.length proof_output, header, String.length header, mode, initialCsvFilename, String.length initialCsvFilename) |> ignore
