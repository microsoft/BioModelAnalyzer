// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//Begin <-- Added by Qinsi Wang
module Rangelist

open Expr

// Assign the constants fixed value instead of [0, N]
let nuRange network = List.fold (fun range (x:QN.node) -> 
                                    match x.f with 
                                    //| Const c -> Map.add x.var (c, c) range
                                    | _ -> Map.add x.var x.range range) Map.empty network

// Also treat the possible values of each variable as a list instead of a pair
// which can represent the situation that the mid value become impossible
// e.g. for variable v,
// at time t, its possible values are [0, 1, 2], which can be represented as both a pair (0,2) and a list [0, 1, 2]
// at time t+1, if its possible values become [0, 2], then we cannot describe it as a pair anymore.
let nuRangel network = List.fold (fun range (x:QN.node) -> 
                                    match x.f with 
                                    //| Const c -> Map.add x.var [c .. c] range
                                    | _ -> 
                                        let min,max = x.range
                                        Map.add x.var [min .. max] range) Map.empty network
//End <-- Assign initial possible values for different variables    
