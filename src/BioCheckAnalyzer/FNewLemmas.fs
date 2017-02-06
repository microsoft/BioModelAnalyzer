// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

module FNewLemmas

//
// Alg 4. Domain-specific fast lemma generation F-NewLemmas
//
open Microsoft.FSharp.Collections

/// Attempt to tighten the bounds of an expression.

let str_of_interval = Expr.str_of_interval 
let str_of_bounds (bounds : ((Expr.var*(int*int)) list)) =
    String.concat "," (List.map (fun (x,(lo,hi)) -> "var(" + (string)x + "):" + (str_of_interval lo hi)) bounds)

// Given a list of pairs [(x_0, y_0); ...; (x_k, y_k)],
// generate all maps M such that:
//   for 0 <= i <= k:
//      M[i] = j where x_i <= j <= y_i
let rec all_inputs vars =
    seq {
        match vars with
        | [] -> yield Map.empty
        | (var, (lower, upper)) :: more_vars ->
            for value in [lower .. upper] do
                for env in (all_inputs more_vars) do
                    yield Map.add var value env
    }

let tighten_internal node range expr lower upper input_bounds =
    // Find new upper and lower bounds by explicitly enumerating all the
    // possible input combinations and evaluating the function for each.
    let (new_lower, new_upper) =
        (Seq.fold
            (fun (lower, upper) env ->                                   
                let new_val = Expr.eval_expr node range expr env
                Log.log_debug ("tighten_internal: var:"+(string)node + " f:" + (Expr.str_of_expr expr) + " env:" + (Expr.str_of_env env) + " var':"+(string)new_val)
                let new_lower = min lower new_val
                let new_upper = max upper new_val
                (new_lower, new_upper))
            (upper, lower) // start with original swapped as (artificial) bounds 
            (all_inputs input_bounds))
    (new_lower, new_upper)

let find_max expr var bounds =
    let (lo, hi) = Map.find var bounds
    if (Expr.is_increasing expr var) then 
        (hi, hi)
    else if (Expr.is_decreasing expr var) then 
        (lo, lo)
    else (lo, hi)

let find_min expr var bounds =
    let (lo, hi) = Map.find var bounds
    if (Expr.is_increasing expr var) then 
        (lo, lo)
    else if (Expr.is_decreasing expr var) then 
        (hi, hi)
    else (lo, hi)
    
// node,expr,inputs - node that we're tightening for. 
// lower,upper      - node's current bounds 
// range       - range of each var.
// bounds      - current bounds  
let tighten_fast (node:QN.var) range expr lower upper inputs bounds =

    let current_bounds = [for input in inputs -> (input, Map.find input bounds)]
    //Log.log_debug ("FNewLemmas ( var(" + (string)node + "):" + (str_of_interval lower upper) + " applied to " + (str_of_bounds current_bounds))

    let max_bounds = [for input in inputs -> (input, find_max expr input bounds)]
    let min_bounds = [for input in inputs -> (input, find_min expr input bounds)]   
    let (new_lower, _) = tighten_internal node range expr lower upper min_bounds     
    let (_, new_upper) = tighten_internal node range expr lower upper max_bounds         

    //Log.log_debug ("           ) " + (str_of_interval lower upper) + "-->" + (str_of_interval new_lower new_upper))
    (new_lower, new_upper)


// Attempt to tighten the bounds on some expression by enumerating all the possible
// inputs to the expression (based on the currently known bounds for each input
// variable.)
let tighten_slow node range expr lower upper inputs bounds =
    let input_bounds = [for input in inputs -> (input, Map.find input bounds)]
    tighten_internal node range expr lower upper input_bounds 

let tighten = tighten_fast

