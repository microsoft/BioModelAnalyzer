// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      Shrink.fs
//
//  Abstract:
//
//      Shrink a given region. Updates based on weak topological order.
//
//  Contact:
//
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//

module Shrink

open System

/// Return updated bounds of var using curBounds of its inputs
let rec UpdateVarBounds (node: QN.node) ranges (curBounds: Map<QN.var, int*int>) =
    let nodeLower = (fst curBounds.[node.var]) 
    let nodeUpper = (snd curBounds.[node.var])
    let WithinOld (newLower, newUpper) =
        ((max (min newLower nodeUpper) nodeLower), (max (min newUpper nodeUpper) nodeLower))
    if (fst curBounds.[node.var]) < (snd curBounds.[node.var]) then
        Map.add node.var
                (WithinOld 
                    (FNewLemmas.tighten node.var ranges node.f nodeLower nodeUpper node.inputs curBounds))
               // ((Oracle.GetNewLowerBound node nodeLower ranges curBounds),
                //    (Oracle.GetNewUpperBound node nodeUpper ranges curBounds))// already within [nodeLower, nodeUpper]
                curBounds
    else
        curBounds


let NoChangeInBoundsForNode (node : QN.node) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    oldBounds.[node.var] = newBounds.[node.var]

let NoChangeInBounds (qn : QN.node list) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    List.forall (fun node -> NoChangeInBoundsForNode node oldBounds newBounds) qn




/// qnStrategy is the update order determined based on WTO. Supplied by Prover module
/// qnStartPoint is the start point of the strategy
let rec Shrink (qn : QN.node list) ranges inputs outputs qnStrategy qnStartPoint startFrontier startBounds =

    let mutable bounds = startBounds
    
    let mutable frontier:Set<QN.var> = startFrontier

    

    let mutable curNodeVarOption = qnStartPoint
    let mutable visitedHead = Map.filter (fun _ (strategy : GGraph.Strategy<QN.var>) -> strategy.isHead) qnStrategy
                                |> Map.fold (fun map nodeVar _ -> Map.add nodeVar false map) Map.empty
    let updateCounter = ref 0
    while not (Option.isNone curNodeVarOption) do
        
        let curNodeVar = (Option.get curNodeVarOption)
        let curNode = QN.get_node_from_var curNodeVar qn

        let mutable noChange = true

        if frontier.Contains curNode.var then 
            //if Log.level(2) then Log.log_debug("Updating node" + (QN.str_of_node curNode) )
            //if Log.level(2) then Log.log_debug("Bounds={" + (QN.str_of_range qn bounds) + "}")
            incr updateCounter
            let newBounds = UpdateVarBounds curNode ranges bounds 
                        
            noChange <- NoChangeInBoundsForNode curNode bounds newBounds


            let newFrontier = 
                if noChange then
                    (Set.remove curNode.var frontier)
                else 
                    Set.fold (fun fr o -> Set.add o fr) (Set.remove curNode.var frontier) (Map.find curNode.var outputs)
            bounds <- newBounds
            frontier <- newFrontier
        
        
        
        let mutable newCurNodeVarOption = qnStrategy.[curNodeVar].next
        if qnStrategy.[curNodeVar].isHead then
            let visited = match Map.tryFind curNodeVar visitedHead with
                            | None -> failwithf "Head %A should have been found in the visitedHead map" curNodeVar 
                            | Some value -> value
            if visited then     
                if noChange then
                    newCurNodeVarOption <- qnStrategy.[curNodeVar].exit
                    visitedHead <- Map.add curNodeVar false visitedHead
            else
                    visitedHead <- Map.add curNodeVar true visitedHead
        
        curNodeVarOption <- newCurNodeVarOption
    
        
    bounds
    

