﻿////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) 2013  Microsoft Corporation
//
//  Module Name:
//
//      Suggest.fs
//
//  Abstract:
//
//      Suggest edges using wto based heuristics to enforce stability
//
//  Contact:
//
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//

module Suggest

open System
open System.Collections.Generic

type Suggestion =
    | Edges of (QN.node*QN.node) list * QN.nature
    | Stable of Map<QN.var, int>
    | NoSuggestion of Map<QN.var, int*int>


// src number, dst number, list of pairs of positions of matching tags
type EdgeSign = QN.number * QN.number * ((QN.pos * QN.pos) list)

//higher scores are better
type SuggestionScore = double 


let edgelist_to_str (edges:(QN.node*QN.node) list) =
    String.concat ",\n\n\n" (List.map (fun (src, dst) -> (QN.str_of_node src) + "->" + (QN.str_of_node dst)) edges)

let MutateExpr (qn : QN.node list) (srcNodeVar : QN.var) (dstNodeVar : QN.var) (nature : QN.nature)=
    let dstNode = QN.get_node_from_var dstNodeVar qn

    if srcNodeVar=dstNodeVar || List.exists (fun x -> x = srcNodeVar) dstNode.inputs then
        // in case src node is same as dst or src is already an input to dst, no change
        dstNode.f
    else
        if dstNode.defualtF then
            // if dst node is default node, then just add to the pos/neg set acc to nature
            let posInputs = List.filter (fun inp -> dstNode.nature.[inp] = QN.Act) dstNode.inputs
            let negInputs = List.filter (fun inp -> dstNode.nature.[inp] = QN.Inh) dstNode.inputs

            let posInputs = if nature = QN.Act then srcNodeVar :: posInputs else posInputs
            let negInputs = if nature = QN.Inh then srcNodeVar :: negInputs else negInputs

            let (min, max) = dstNode.range

            let newExpr =
                match posInputs, negInputs with
                | [], [] -> Expr.Const(min)
                | [], _  -> Expr.Minus(Expr.Const(max), Expr.Ave(List.map (fun o -> Expr.Var(o)) negInputs))
                | _ , [] -> Expr.Ave(List.map (fun i -> Expr.Var(i)) posInputs)
                | _ , _  -> Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) posInputs),
                                        Expr.Ave(List.map (fun o -> Expr.Var(o)) negInputs))
            newExpr
        else
            // if not default function node then mutate the expr as follows
            // if the original expr is f(x_1, .. ,x_m) and you are adding input x_{m+1}
            // new expr will be (m*f(x_1, .. , x_m) + x_{m+1})/(m+1)
            let numInputs = List.length dstNode.inputs
            Expr.Div(Expr.Plus(Expr.Times(dstNode.f, Expr.Const(numInputs)), Expr.Var(srcNodeVar)), Expr.Const(numInputs+1))

                    


let AddEdgeInQN (src, dst, ntr) qn = 
    // add an edge in the qn
    let dstNode = QN.get_node_from_var dst qn
    let mutatedExpr = MutateExpr qn src dst ntr
    let qn = List.filter (fun (node : QN.node) -> not(node.var = dst)) qn
    let qn = 
        {dstNode with f=mutatedExpr; inputs = src::dstNode.inputs; nature= Map.add src ntr dstNode.nature} :: qn
    qn


let ComputeEdgeSign (src : QN.node) (dst : QN.node)=

    // given a tagged network, every edge has a signature
    // the edge (1, {A B C}) -> (2, {C A D E}) will have signature (1, 2, {(1, 2),(3, 1)}); A and C are common tags, (1,2) for A and (3,1) for C 
    let matchingPosPairs =
        List.fold
            (fun mpp ((pos_s, cell_s), (pos_t, cell_t)) ->
                if cell_s = cell_t then (pos_s, pos_t)::mpp else mpp)
            []
            [for pr_s in src.tags do for pr_t in dst.tags do yield (pr_s, pr_t)]
    (src.number, dst.number, matchingPosPairs)


let ComputeAllEdgesWithSign qn (sign : EdgeSign) =
    // find all edges in the qn which have the give signature
    List.filter
        (fun (src, dst) ->
            (ComputeEdgeSign src dst) = sign)
        [for src in qn do for dst in qn -> (src, dst)]



let GetQnGraph (qn : QN.node list) = 
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


let CallShrink (qn : QN.node list) ranges inputs outputs qnStrategyPair frontier bounds = 
    let ranges = match ranges with 
                 | None -> Map.ofList [for node in qn -> (node.var, node.range)]
                 | Some r -> r
    let inputs = match inputs with 
                 | None -> Map.ofList [for node in qn -> (node.var, node.inputs)]
                 | Some ii -> ii

     
    let outputs =
        match outputs with
        | None ->
            let mutable outputs' = Map.ofList [for node in qn -> (node.var, Set.empty)]
            for node in qn do
                for input in node.inputs do
                    let curr_outs = outputs'.[input]
                    outputs' <- Map.add input (curr_outs.Add node.var) outputs'
            outputs'
        | Some oo -> oo

    let (qnStrategy, qnStartPoint) = 
        match qnStrategyPair with
        | None -> let qnGraph = GetQnGraph qn
                  GGraph.GetRecursiveStrategy qnGraph
        | Some (x, y) -> (x, y)

    
    let initialFrontier = 
        match frontier with
        | None ->
            Set.ofList 
                (List.fold 
                    (fun bb (n:QN.node) -> 
                        if (Expr.is_a_const ranges n.f) then bb
                        else n.var :: bb)
                    []
                    qn)
        | Some f -> f


    let initialBounds = 
        match bounds with
        | None -> 
            Map.ofList 
                (List.fold 
                    (fun bb (n:QN.node) -> 
                        if (Expr.is_a_const ranges n.f) then 
                            let c = Expr.eval_expr n.var ranges n.f Map.empty
                            (n.var,(c,c)) :: bb
                        else (n.var, Map.find n.var ranges) :: bb)
                    []
                    qn)
        | Some b -> b
    Shrink.Shrink qn ranges inputs outputs qnStrategy qnStartPoint initialFrontier initialBounds

let ComputeShrinkCoeff (bounds : Map<QN.var, int*int>) (shrunkBounds : Map<QN.var, int*int>)=
    // size of space before shrinking/size of space after shrinking
    let allVars = [for KeyValue(k, v) in bounds -> k]
    List.fold
        (fun coeff var ->
            let (lo1, hi1) = bounds.[var]
            let (lo2, hi2) = shrunkBounds.[var]
            coeff * ((double)(hi1 - lo1 + 1)) / ((double)(hi2 - lo2 + 1))
        )
        1.0
        allVars


let NoPosMatch (edgeSign : EdgeSign) =
    // true is no tags at any positions match
    let (_, _, l) = edgeSign
    l.IsEmpty



let ApplySuggestion (qn : QN.node list) (edges : (QN.node*QN.node) list) (ntr : QN.nature)=
    // take the given edge set and add it to the qn
    List.fold
        (fun newQn ((srcNode, dstNode): (QN.node*QN.node)) ->
            AddEdgeInQN (srcNode.var, dstNode.var, ntr) newQn)
        qn
        edges

let FindSuggestionScores qn ranges inputs outputs (qnStrategy : Map<QN.var, GGraph.Strategy<QN.var>>) qnStartPoint
                 (nodes : QN.node list) bounds (knownScores : Dictionary<EdgeSign*QN.nature, SuggestionScore>) =
        let scores = knownScores


        for dst in nodes do 
            Log.log_debug(sprintf "Scoring edges to the node %s" (QN.str_of_node dst))
            for src in List.filter (fun node -> node <> dst && (not (List.exists (fun inp -> inp = node.var) dst.inputs))) qn do 
                Log.log_debug(sprintf "Scoring edges from the node var %d to the node var %d" src.var dst.var)
                for ntr in [QN.Act; QN.Inh] do
                    let edgeSign = ComputeEdgeSign src dst
                    
                    if NoPosMatch edgeSign || scores.ContainsKey(edgeSign, ntr) then
                        ()
                    else
                        let allEdges = ComputeAllEdgesWithSign qn edgeSign

                        let modQn =
                            ApplySuggestion qn allEdges ntr
                        let modInputs = 
                            List.fold
                                (fun inps ((s,t) : (QN.node*QN.node)) ->
                                    Map.add t.var (s.var::inps.[t.var]) inps
                                    )
                                inputs
                                allEdges
                        let modOutputs = 
                            List.fold
                                (fun outs ((s,t) : (QN.node*QN.node)) ->
                                    Map.add s.var (Set.add t.var outs.[s.var]) outs
                                    )
                                outputs
                                allEdges

                        // recompute wto only if some edge to a non-head is added
                        let modQnStrategy, modQnStartPoint =
                            if List.exists (fun (_, (t : QN.node)) -> not qnStrategy.[t.var].isHead) allEdges then
                                let qnGraph = GetQnGraph modQn
                                GGraph.GetRecursiveStrategy qnGraph
                            else
                                qnStrategy, qnStartPoint
                        
                        let modFrontier =
                            List.fold
                                (fun frntr (_, (t : QN.node)) ->
                                    Set.add t.var frntr)
                                Set.empty
                                allEdges

                            
                        let modShrunkBounds = CallShrink modQn (Some ranges) (Some modInputs) (Some modOutputs) 
                                                (Some (modQnStrategy, modQnStartPoint)) None None
                        
                        let shrinkCoeff = ComputeShrinkCoeff bounds modShrunkBounds
                        let shrinkPerEdge =  shrinkCoeff// / (float) allEdges.Length

                        Log.log_debug(sprintf "Added score %f for edge sign %A, nature %A" shrinkPerEdge edgeSign ntr)
                        scores.Add((edgeSign, ntr), shrinkPerEdge)
        scores



let GetSuggestionFromSign qn sgn =
    (ComputeAllEdgesWithSign qn sgn)

let GetMaxScore (scores : Dictionary<'T, SuggestionScore>) =
    List.max [for KeyValue(_, score) in scores -> score]

let SortScores qn (scores : Dictionary<EdgeSign*QN.nature, SuggestionScore>) =
    Log.log_debug(sprintf "Sorting %d scores" scores.Count)
    seq{ for KeyValue(k, score) in scores -> (k, score)}
    |> Seq.sortBy (fun (_, score) -> -score)


    // unused
let ScoresToList qn (scores : Dictionary<EdgeSign*QN.nature, SuggestionScore>) =
    [for KeyValue((sgn, ntr), score) in scores -> (((GetSuggestionFromSign qn sgn), ntr), score)]

    // unused
let rec FindMaxFromScoreList lst =
    match lst with
    | [] -> failwith "Cannot get max from empty list"
    | (x,hd)::[] -> (x,hd), []
    | (x,hd)::tl -> let (y,max), rest = FindMaxFromScoreList tl
                    if max>=hd then (y, max), (x, hd)::rest
                    else (x, hd), (y, max)::rest

let Suggest (qn : QN.node list) =
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


    let  initialBounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then 
                        let c = Expr.eval_expr n.var ranges n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var ranges) :: bb)
                []
                qn)
    
    let  initialFrontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then bb
                    else n.var :: bb)
                []
                qn)


    let shrunkBounds = CallShrink qn (Some ranges) (Some inputs) (Some outputs) (Some (qnStrategy, qnStartPoint)) (Some initialFrontier) (Some initialBounds)

    Log.log_debug("ShrunkBounds={" + (QN.str_of_range qn shrunkBounds) + "}")

    if Map.forall (fun _ (lower,upper) -> upper = lower) shrunkBounds then
        let stablePoint = (Map.fold (fun map v (lower, _) -> Map.add v lower map) Map.empty shrunkBounds)
        Log.log_debug(sprintf "Found a stable point %s" (Expr.str_of_env stablePoint))
        Stable(stablePoint)
    else
        let unstableNodes = List.filter (fun (node : QN.node) -> 
                                            let (lower, upper) = shrunkBounds.[node.var]
                                            upper>lower)
                                        qn
        let defaultFunctionNodes = List.filter (fun (node : QN.node) -> node.defualtF) unstableNodes
        let headNodes = List.filter (fun (node : QN.node) -> qnStrategy.[node.var].isHead) defaultFunctionNodes
        
        Log.log_debug("Trying to find good score for defualt function nodes which are heads")
        let mutable scores = FindSuggestionScores qn ranges inputs outputs qnStrategy qnStartPoint
                                headNodes shrunkBounds (new Dictionary<EdgeSign*QN.nature, SuggestionScore>())
        let mutable maxScore = GetMaxScore scores
        

        if maxScore <= (double) 1.0 then
            Log.log_debug("Trying to find good score for all defualt function nodes")
            scores <- FindSuggestionScores qn ranges inputs outputs qnStrategy qnStartPoint
                        defaultFunctionNodes shrunkBounds scores
            maxScore <- GetMaxScore scores

        if maxScore <= (double) 1.0 then
            Log.log_debug("Trying to find good score for all unstable nodes")
            scores <- FindSuggestionScores qn ranges inputs outputs qnStrategy qnStartPoint
                        unstableNodes shrunkBounds scores
            maxScore <- GetMaxScore scores
                
        if maxScore <= (double) 1.0 then
            NoSuggestion(shrunkBounds)
        else
            Log.log_debug("Now sorting scores.. ")

            let tmp =
                Seq.tryFind
                    (fun ((sign, ntr), score) ->
                        Log.log_debug(sprintf "Edges:\n %s \n\n\n Nature: %A Score: %f" (edgelist_to_str (GetSuggestionFromSign qn sign)) ntr score)
                        Log.log_debug("Accept this and proceed? Y/N")
                        let inp = Console.ReadLine()
                        inp="Y" || inp="y"
                        )
                    (SortScores qn scores)
                    
            match tmp with
            | None -> NoSuggestion(shrunkBounds)
            | Some((sign, ntr),_) -> Edges((GetSuggestionFromSign qn sign), ntr)
            
let rec SuggestLoop (qn : QN.node list) =
    let sug = Suggest qn

    match sug with
    | NoSuggestion(b) -> NoSuggestion(b)
    | Stable(p) -> Stable(p)
    | Edges(edges, ntr) -> SuggestLoop (ApplySuggestion qn edges ntr)

