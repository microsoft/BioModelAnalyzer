// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      Cut.fs
//
//  Abstract:
//
//      Find good cuts across a region in the interval domain
//
//  Contact:
//
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//



module Cut

open Microsoft.Z3
open System

type nature = OneWayInc | OneWayDec | TwoWay | ZeroWay

let ExistsEdgeAcrossCut (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) (cutNode : QN.node) (cutFrom : int) (cutTo : int) =
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    use ctx = new Context(cfg)
    use s = ctx.MkSolver()

    for node in qn do
        Z.assert_target_function qn cutNode ranges 0 1 ctx s

    for node in qn do
        if node.var = cutNode.var then
            Z.assert_bound node (cutFrom, cutFrom) 0 ctx s
            Z.assert_bound node (cutTo, cutTo) 1 ctx s
        else 
            Z.assert_bound node bounds.[node.var] 0 ctx s
            Z.assert_bound node bounds.[node.var] 1 ctx s

    match s.Check() with
    | Status.SATISFIABLE -> true
    | _ -> false

let CanIncreaseAcrossCut (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) (cutNode : QN.node) (cutAt : int) =
    ExistsEdgeAcrossCut qn ranges bounds cutNode cutAt (cutAt+1)

let CanDecreaseAcrossCut (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) (cutNode : QN.node) (cutAt : int) =
    ExistsEdgeAcrossCut qn ranges bounds cutNode (cutAt+1) cutAt


let FindCutScores (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) =
    seq { for cutNode in qn do 
            let hi = (snd bounds.[cutNode.var])
            let lo = (fst bounds.[cutNode.var])
            for cutPoint in [lo .. hi-1] do     
                let cutSize = (min (cutPoint-lo+1) (hi-cutPoint))
                if CanIncreaseAcrossCut qn ranges bounds cutNode cutPoint then 
                    if CanDecreaseAcrossCut qn ranges bounds cutNode cutPoint then
                        Log.log_debug(sprintf "Node: %d, cutPoint: %d, TwoWay" cutNode.var cutPoint)
                        yield ((cutNode, cutPoint, TwoWay), (0, cutSize))
                    else
                        Log.log_debug(sprintf "Node: %d, cutPoint: %d, OneWay" cutNode.var cutPoint)
                        yield ((cutNode, cutPoint, OneWayInc), (1, cutSize)) 
                elif CanDecreaseAcrossCut qn ranges bounds cutNode cutPoint then
                    Log.log_debug(sprintf "Node: %d, cutPoint: %d, OneWay" cutNode.var cutPoint)
                    yield ((cutNode, cutPoint, OneWayDec), (1, cutSize)) 
                else
                    Log.log_debug(sprintf "Node: %d, cutPoint: %d, ZeroWay" cutNode.var cutPoint)
                    yield ((cutNode, cutPoint, ZeroWay), (2, cutSize)) 
        }

let FindBestCut (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) =
    let scores = Seq.cache (FindCutScores qn ranges bounds)

    // find the first non-twoWay cut and return that as the best cut
    let ntwCut = Seq.tryFind (fun ((_,_,cutNature), _) -> cutNature<>TwoWay) scores
    
    match ntwCut with
    | Some (cut, _) -> cut
    // if no zeroWay cut found, return the one with the max score
    | None -> scores |> Seq.maxBy (fun (_, score) -> score) |> fst
