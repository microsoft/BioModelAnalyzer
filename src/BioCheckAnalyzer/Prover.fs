// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      Prover.fs
//
//  Abstract:
//
//      Top level prover module to run the shrink-cut-merge method
//
//  Contact:
//
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//

module Prover


open System
open VariableEncoding

type CEX =
    | Bifurcation of Map<QN.var, int> * Map<QN.var, int>
    | Cycle of Map<QN.var, int> * int


/// starts simulation from the current point, returns 
/// Some (cyclestart, length) length can be 1
/// should never return (_,0)
/// should always terminate because the network is finite
let SimulateForCycle qn point =
    Log.log_debug "SimulateForCycle { "
    let mutable allEnvs = Map.add point 0 Map.empty
    let mutable env = point
    let mutable i = 0
    let mutable stop = false

    let mutable cycleData = (point, 0) /// this never be the return value, it will change when the loop terminates
    
    while not stop do
        ///Log.log_debug (sprintf "Step: %d, Env %s" i (Expr.str_of_env env))
        env <- (Simulate.tick qn env)
        i <- i+1
        if allEnvs.ContainsKey env then /// found cycle
            Log.log_debug (sprintf "Step %d, Back To %d, LOOP FOUND at %s" i allEnvs.[env] (Expr.str_of_env env))
            cycleData <- (env, i-allEnvs.[env])
            stop <- true
        else
            allEnvs <- Map.add env i allEnvs

    Log.log_debug "SimulateForCycle } "
    cycleData

/// BH
/// checks to see if a simulation can cross the frontier
/// if it doesn't go either cutAt -> cutAt+1 or cutAt+1 -> cutAt
/// then we shouldn't bother running a complete simulation
/// Returns true for traces which cross frontier, false otherwise

let doesSimulationCrossFrontier (qn:QN.node list) (state:Map<QN.var,int>) (cutNode:QN.var) (cutAt:int) = 
    let s = state.[cutNode]
    let s' = Simulate.individualVariableTick qn state cutNode
    let ds = s' - s
    match (s,s') with
    | (a,b) when a = cutAt && b = cutAt+1-> true
    | (b,a) when a = cutAt && b = cutAt+1 -> true
    | _ ->  Log.log_debug "Skipping non-frontier state";
            false

let ProveStability (qn : QN.node list) =

    let timer = new System.Diagnostics.Stopwatch()
    timer.Start()

    let ranges = Map.ofList [for node in qn -> (node.var, node.range)]
    let inputs = Map.ofList [for node in qn -> (node.var, node.inputs)]


    let mutable outputs' = Map.ofList [for node in qn -> (node.var, Set.empty)]
    for node in qn do
        for input in node.inputs do
            let curr_outs = outputs'.[input]
            outputs' <- Map.add input (curr_outs.Add node.var) outputs'
    let outputs = outputs'


    let qnGraph =
        List.fold
            (fun graph (node : QN.node) ->
                List.fold
                    (fun graph input ->
                        GGraph.AddEdge input node.var graph)
                    graph
                    node.inputs)
            (List.fold
                    (fun graph (node : QN.node) ->
                    GGraph.AddVertex node.var graph)
                    GGraph.Empty<QN.var>
                    qn)
            qn

    let (qnStrategy, qnStartPoint) = 
            GGraph.GetRecursiveStrategy qnGraph
    
    
    let qnWTO = GGraph.GetWeakTopologicalOrder qnGraph
    Log.log_debug("WTO:" + GGraph.Stringify (qnWTO) string)


    let mutable initialBounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then 
                        let c = Expr.eval_expr n.var ranges n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var ranges) :: bb)
                []
                qn)
    
    let mutable initialFrontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then bb
                    else n.var :: bb)
                []
                qn)
    
    
    Log.log_debug("InitialBounds={" + (QN.str_of_range qn initialBounds) + "}")


    let rec FindStablePoints  frontier bounds =
        let shrunkBounds = Shrink.Shrink qn ranges inputs outputs qnStrategy qnStartPoint frontier bounds 
        
        if Map.forall (fun _ (lower,upper) -> upper = lower) shrunkBounds then
            let stablePoint = (Map.fold (fun map v (lower, _) -> Map.add v lower map) Map.empty shrunkBounds)
            let stablePoint' = (Simulate.tick qn stablePoint)
            if stablePoint' = stablePoint then
                Log.log_debug(sprintf "Found Stable Point %s" (Expr.str_of_env stablePoint))
                (Some stablePoint, None)
            else 
                Log.log_debug(sprintf "Found fake Stable Point %s" (Expr.str_of_env stablePoint'))
                (None, None)

        else 
            Log.log_debug("Trying to cut.. ")
            let (cutNode, cutAt, cutNature) = Cut.FindBestCut qn ranges shrunkBounds
            let cutNodeVar = cutNode.var
            Log.log_debug (sprintf "CutNode %d, CutPoint %d, CutNature %A" cutNodeVar cutAt cutNature)

            let newFrontier = (Map.find cutNodeVar outputs)
            
            Log.log_debug("Entering first half..")
            let (stablePoint1, cex1) = FindStablePoints newFrontier (Map.add cutNodeVar ((fst shrunkBounds.[cutNodeVar]), cutAt) shrunkBounds) 
            if Option.isSome cex1 then 
                (None, cex1)
            else 
                Log.log_debug("Entering second half..")
                let (stablePoint2, cex2) = FindStablePoints newFrontier (Map.add cutNodeVar (cutAt+1, (snd shrunkBounds.[cutNodeVar]))shrunkBounds) 
                if Option.isSome cex2 then 
                    (None, cex2)
                else
                    Log.log_debug("Trying to merge two halves, since no cex found..")
                    let OneWayResult =
                        match (stablePoint1, stablePoint2) with
                        |(Some a, Some b) -> (None, Some (Bifurcation(a,b)))
                        |(Some a, None) -> (Some a, None)
                        |(None, Some a) -> (Some a, None)
                        | _ -> (None, None)
                                

                    if cutNature <> Cut.TwoWay then
                        OneWayResult
                    else
                        Log.log_debug("Trying to find cycles across two way cut..")
                        let cycle =
                            (FNewLemmas.all_inputs [for node in qn -> if node.var = cutNode.var then (node.var, (cutAt, cutAt+1)) else (node.var, bounds.[node.var])])
                            /// BH: filter out all the states which don't cross the frontier
                            |> Seq.filter (fun state -> doesSimulationCrossFrontier qn state cutNode.var cutAt)
                            |> Seq.map (fun point -> (SimulateForCycle qn point))
                            |> Seq.tryFind (fun cycleData -> match cycleData with 
                                                                | (_, 0) -> failwith "Bad length of cycle returned"
                                                                | (_, 1) -> false
                                                                | _ -> true)
                        Log.log_debug "Got cycle result"
                        match cycle with
                        | Some c -> (None, Some (Cycle(c)))
                        | None -> OneWayResult
                  
    
    printfn "Elapsed time before calling shrink is %i" timer.ElapsedMilliseconds

    let initialShrunkBounds = Shrink.Shrink qn ranges inputs outputs qnStrategy qnStartPoint initialFrontier initialBounds

    printfn "Elapsed time after calling shrink the first time is %i" timer.ElapsedMilliseconds

    let results =
        if Map.forall (fun _ (lower,upper) -> upper = lower) initialShrunkBounds then
            (Some ((Map.fold (fun map v (lower, _) -> Map.add v lower map) Map.empty initialShrunkBounds)), None)
        else
            Log.log_debug "Trying to find bifurcation.."
            let z_bifur = Z.find_bifurcation qn initialShrunkBounds

            let bifur = 
                match z_bifur with
                | Some((fix1, fix2)) -> 
                    // id^t --> id
                    let parse s = 
                        let (id,t) = 
                            try dec_qn_var_at_t_from_z3_var s 
                            with exn -> failwith "Failed to parse bifurcation id"
                        id
                    let fix1 = 
                        Map.fold
                            (fun newMap name value ->
                                let (id,t) = dec_qn_var_at_t_from_z3_var name
                                Map.add id value newMap)
                            Map.empty
                            fix1
                    let fix2 = 
                        Map.fold
                            (fun newMap name value ->
                                let (id,t) = dec_qn_var_at_t_from_z3_var name
                                Map.add id value newMap)
                            Map.empty
                            fix2
                    Some (Bifurcation(fix1, fix2))
                | None -> None
                
            //printfn "Elapsed time after trying to find bifurcation is %i" timer.ElapsedMilliseconds

            if Option.isNone bifur then 
                Log.log_debug "No bifurcation found. Will now attempt to cut."
                FindStablePoints Set.empty initialShrunkBounds 
            else
                (None, bifur)
           
          
    printfn "Elapsed time until finish is %i" timer.ElapsedMilliseconds
    results
