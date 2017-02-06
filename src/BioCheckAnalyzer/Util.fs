// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Util

open System.Collections.Generic

/// Sin bin for random "utility" code.

// Take n items from list l.
// Is this really not a standard F# function?
let rec take n l =
    match (n, l) with
    | (0, _) -> []
    | (_, []) -> []
    | (n, l) -> l.Head :: (take (n-1) l.Tail)

let memoize f =
    let dict = Dictionary<_, _>()

    fun x ->
        if dict.ContainsKey x then dict.[x]
        else let res = f x
             dict.[x] <- res
             res

let memoize2 f =
    let dict = Dictionary<_, _>()

    fun x y ->
        if dict.ContainsKey (x, y) then dict.[(x, y)]
        else let res = f x y
             dict.[(x, y)] <- res
             res

let memoize3 f =
    let dict = Dictionary<_, _>()

    fun x y z ->
        if dict.ContainsKey (x, y, z) then dict.[(x, y, z)]
        else let res = f x y z
             dict.[(x, y, z)] <- res
             res

/// merge two maps into one
/// if duplicate keys are present then the value in map2 survives
let MergeMaps map1 map2 =
    Map.fold
        (fun map k v ->
            Map.add k v map)
        map1
        map2
