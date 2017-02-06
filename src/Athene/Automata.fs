// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Automata

open System
open System.Collections.Generic
open System.Collections.Concurrent

let spawnMachines (qn: QN.node list) (number:int) (rng:System.Random) (init0:string) =
    let rec maximum acc (s: int list) =
        match s with 
        | head::tail -> 
            if head > acc then maximum head tail
            else maximum acc tail
        | [] -> acc
    let max = maximum 0 [for v in qn ->
                            let (min,max) = v.range
                            max
                        ]
    
    let initState i0 = match i0 with
                        | "zero" -> List.fold (fun m (n:QN.node) -> Map.add n.var 0 m) Map.empty qn
                        | "random" -> List.fold (fun m (n:QN.node) -> Map.add n.var (rng.Next(0,(max+1))) m) Map.empty qn
                        | _ -> failwith "Cannot read the initial state of the machines"
    [for machine in [0..(number-1)] -> initState init0] |> Array.ofList

let rec qnHash : int -> Map<QN.var,int> -> string =
    let g = ref None
    //How many digits do I need?
    //let g = int( log10( float (granularity) )) + 1
    (fun granularity qnState -> 
    match !g with
    | None    -> g := Some(int( log10( float (granularity) )) + 1); qnHash granularity qnState
    | Some(1) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map (fun (x,y) -> sprintf "%+02d" y) |> List.fold (+) "" //|> (fun x -> int x)
    | Some(2) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map (fun (x,y) -> sprintf "%+03d" y) |> List.fold (+) "" //|> (fun x -> int x)
    | Some(3) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map (fun (x,y) -> sprintf "%+04d" y) |> List.fold (+) "" //|> (fun x -> int x)
    | _ -> failwith "Athene will not work with ranges greater than 10^3"
    )

let rec qnHash' : int -> Map<QN.var,int> -> int list -> string =
    let g = ref None
    //How many digits do I need?
    //let g = int( log10( float (granularity) )) + 1
    (fun granularity qnState minList-> 
    match !g with
    | None    -> g := Some(int( log10( float (granularity) )) + 1); qnHash' granularity qnState minList
    | Some(1) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map2 (fun min (x,y) -> sprintf "%01d" (y-min)) minList |> List.fold (+) "" //|> (fun x -> int x)
    | Some(2) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map2 (fun min (x,y) -> sprintf "%02d" (y-min)) minList |> List.fold (+) "" //|> (fun x -> int x)
    | Some(3) -> qnState |> Map.toList |> List.sortBy (fun (x,y) -> x) |> List.map2 (fun min (x,y) -> sprintf "%03d" (y-min)) minList |> List.fold (+) "" //|> (fun x -> int x)
    | _ -> failwith "Athene will not work with ranges greater than 10^3"
    )

let tickMemo'' =
    let cache = ConcurrentDictionary<_,Map<QN.var,int>>(HashIdentity.Structural)
    fun qn m g ml ->
        let hashQN = qnHash' g m ml
        //let hashQN = m
        if cache.ContainsKey(hashQN) then cache.[hashQN]
        else    let m' = Simulate.tick qn m
                cache.[hashQN] <- m'
                m'

let tickMemo' =
    let cache = Dictionary<_,Map<QN.var,int>>(HashIdentity.Structural)
    fun qn m g ->
        let hashQN = qnHash g m
        //let hashQN = m
        if cache.ContainsKey(hashQN) then cache.[hashQN]
        else    let m' = Simulate.tick qn m
                cache.[hashQN] <- m'
                m'

let tickMemo =
    let cache = Dictionary<_,Map<QN.var,int>>(HashIdentity.Structural)
    fun qn hashQN ->
        if cache.ContainsKey(hashQN) then cache.[hashQN]
        else    let m' = Simulate.tick qn hashQN
                cache.[hashQN] <- m'
                m'

let updateMachines =
    let maxCache = ref None
    let minListCache = ref None
    (
    fun (qn: QN.node list) (machines: Map<QN.var,int> array) (threads:int) -> 
    let minList = match !minListCache with
                    | Some(n) -> n
                    | None ->
                        let ml = qn |> List.sortBy (fun qn -> qn.var) |> List.map (fun qn -> fst(qn.range))
                        minListCache := Some(ml)
                        ml
    let max = match !maxCache with
                | Some(n) -> n
                | None    ->
                    let maxGranularity = List.map (fun (qnVar:QN.node) -> let min,max = qnVar.range
                                                                          max-min)  qn 
                                        |> List.max
                    maxCache := Some(maxGranularity)
                    maxGranularity
    Array.Parallel.map (fun (automata: Map<QN.var,int>) -> tickMemo'' qn automata max minList) machines
    )

let updateMachines' =
    let maxCache = ref None
    (
    fun (qn: QN.node list) (machines: Map<QN.var,int> array) (threads:int) -> 
    let max = match !maxCache with
                | Some(n) -> n
                | None    ->
                    let maxGranularity = List.map (fun (qnVar:QN.node) -> let min,max = qnVar.range
                                                                          max-min)  qn 
                                        |> List.max
                    maxCache := Some(maxGranularity)
                    maxGranularity
    Array.Parallel.map (fun (automata: Map<QN.var,int>) -> tickMemo' qn automata max) machines
    )

let updateMachines'' (qn: QN.node list) (machines: Map<QN.var,int> array) (threads:int) = 
    Array.Parallel.map (fun (automata: Map<QN.var,int>) -> tickMemo qn automata ) machines

let updateMachines''' (qn: QN.node list) (machines: Map<QN.var,int> array) (threads:int) = 
    Array.Parallel.map (fun (automata: Map<QN.var,int>) -> Simulate.tick qn automata ) machines
