// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

(* This file contains functions from BioCheck that manipulate Z3 *)

module BioCheckZ3

open Microsoft.Z3

open VariableEncoding

open Expr
open System.Collections.Generic

let rec expr_to_z3 (qn:QN.node list) (node:QN.node) expr time (z : Context) =
    let node_min,node_max = node.range

    let rec tr expr : RealExpr = 
        let asReal (e:Expr) : RealExpr = e :?> RealExpr

        match expr with
        | Var v ->
            // Use the node's original range 
            let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
            let v_min,v_max = v_defn.range
            let scale,displacement = 
                if (v_min<>v_max) then 
                    let t = z.MkReal(node_max - node_min)
                    let b = z.MkReal(v_max - v_min)
                    (z.MkDiv(t,b), z.MkReal( (node_min - v_min):int ))
                else (upcast z.MkReal 1, z.MkReal 0)

            let input_var = 
                let v_t = enc_z3_int_var_at_time v_defn time
                let z_v_t = make_z3_int_var v_t z
                z.MkInt2Real(z_v_t)
            z.MkAdd(z.MkMul(input_var,scale),displacement) |> asReal
            
        | Const c -> upcast z.MkReal c
        | Plus(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkAdd(z1,z2) |> asReal
        | Minus(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkSub(z1,z2) |> asReal
        | Times(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkMul(z1,z2) |> asReal
        | Div(e1, e2) ->
            let z1 = tr e1
            let z2 = tr e2
            z.MkDiv(z1,z2) |> asReal
        | Max(e1, e2) -> 
            let z1 = tr e1 
            let z2 = tr e2
            let is_gt = z.MkGt(z1, z2)
            z.MkITE(is_gt, z1, z2) |> asReal
        | Min(e1, e2) ->
            let z1 = tr e1 
            let z2 = tr e2
            let is_lt = z.MkLt(z1, z2)
            z.MkITE(is_lt, z1, z2) |> asReal
        // x:Real, m,n:Int. If x-1 < m <= x <= n < x+1. Then floor(x)=m and ceil(x)=n. 
        | Ceil e1 ->        
            let z1 = tr e1
            let floor = z.MkInt2Real(z.MkReal2Int(z1))
            let is_int = z.MkEq (floor, z1)
            let floor_plus_one = z.MkAdd(floor, z.MkReal(1))
            let ceil_assert = z.MkTrue 
            z.MkITE(is_int,floor,floor_plus_one) |> asReal
        | Floor e1 ->
            let z1 = tr e1 
            let floor_assert = z.MkTrue
            z.MkInt2Real (z.MkReal2Int(z1))
        | Abs e1 ->
            let z1 = tr e1
            let zero = z.MkReal(0)
            let z2 = z.MkSub(zero,z1)
            let is_gt_zero = z.MkGt(z1,zero)
            z.MkITE(is_gt_zero,z1,z2) |> asReal
            //BH
        | Ave es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some z0 -> 
                                                let z1 = tr e1
                                                Some(z.MkAdd(z0, z1) |> asReal))
                        None
                        es
            let cnt = z.MkReal (List.length es)
            match sum with
              | None -> upcast (z.MkReal 0)
              | Some s -> z.MkDiv(s, cnt) |> asReal

        | Sum es -> // Sum() is needed to implement multiple activators and (or) inhibitors
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some z0 -> let z1 = tr e1 
                                                    Some(z.MkAdd(z0, z1) |> asReal))
                        None
                        es
            match sum with
              | None -> upcast (z.MkReal 0)
              | Some s -> s

    tr expr 


let assert_target_function (node: QN.node) qn bounds start_time end_time (z : Context) (s : Solver) =
    let current_state_id = enc_z3_int_var_at_time node start_time
    let current_state = make_z3_int_var current_state_id z

    let next_state_id = enc_z3_int_var_at_time node end_time
    let next_state = make_z3_int_var next_state_id z

    let T_applied = z.MkReal2Int(expr_to_z3 qn node node.f start_time z)

    //Begin <-- Edit by Qinsi Wang

    // QSW: have not considered these two situations: the current value is max in the range, but and the same
    // time, the value of its target function is greater than its current value; and, the current value is min
    // in the range, but at the same time, the value of its target function is less than its current value.
    // In either case, we need to keep its current value. 

    let (lower: int, upper: int) = Map.find node.var bounds

    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkInt 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))
    let up = z.MkAnd(up, z.MkGt(z.MkInt upper, current_state))

    let same = z.MkEq(next_state, current_state)
    let tmpsame = z.MkEq(T_applied, current_state)
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkGt(T_applied, current_state), z.MkEq(z.MkInt upper, current_state)))
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkLt(T_applied, current_state), z.MkEq(z.MkInt lower, current_state)))
    let same = z.MkAnd(same, tmpsame)

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkInt 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))
    let dn = z.MkAnd(dn, z.MkLt(z.MkInt lower, current_state))

    (*
    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkInt 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))

    let same = z.MkEq(next_state, current_state)
    let same = z.MkAnd(same, z.MkEq(T_applied, current_state))

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkInt 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))
    *)
    // End 

    let cnstr = z.MkOr([|up;same;dn|])
    Log.log_debug (cnstr.ToString())
    s.Assert cnstr


let assert_bound (node : QN.node) (lower : int , upper : int) (time : int) (z : Context) (s : Solver) =
    let var_name = enc_z3_int_var_at_time node time
    let v = make_z3_int_var var_name z
    let lower_bound = z.MkGe(v, z.MkInt lower)
    let upper_bound = z.MkLe(v, z.MkInt upper)

    Log.log_debug ("Asserting lower bound: " + (lower_bound.ToString()))
    s.Assert lower_bound

    Log.log_debug ("Asserting upper bound: " + (upper_bound.ToString()))
    s.Assert upper_bound

let unroll_qn_bounds qn bounds var_names start_time end_time z s =
    // Assert the target functions...
    for node in qn do
        assert_target_function node qn bounds start_time end_time z s

    // Now assert the bounds...
    for node in qn do
        assert_bound node (Map.find node.var bounds) start_time z s
        assert_bound node (Map.find node.var bounds) end_time z s

let rec build_var_name_map (qn : QN.node list) =
    match qn with
    | [] -> Map.empty
    | node :: nodes -> Map.add node.var node.name (build_var_name_map nodes)

// SI: this is only ever called with steps=0.
//     Can remove it, and replace calls to it by:
//         unrollw_qn qn bounds (build_var_name_map qn) 0 0 z
let qn_to_z3 (qn : QN.node list) bounds steps (z : Context) (s : Solver) =
    // Build a mapping from variable IDs to their names...
    let var_names = build_var_name_map qn

    if steps = 0 then
        unroll_qn_bounds qn bounds var_names 0 0 z s
    else
        for time in [0..steps-1] do
            unroll_qn_bounds qn bounds var_names time (time+1) z s



let find_fixpoint (network : QN.node list) range =
    // SI: not so much fixpoint as a solution to constraints imposed by qn_to_z3.
    Z3Util.find_fixpoint (qn_to_z3 network range 0)

let find_bifurcation (network : QN.node list) range =
    Z3Util.find_bifurcation (qn_to_z3 network range 0)


let assert_states_equal (qn : QN.node list) start_time end_time (ctx : Context) =
    let mutable equal_condition = ctx.MkTrue()

    for node in qn do
        let start_name = enc_z3_int_var_at_time node start_time
        let end_name = enc_z3_int_var_at_time node end_time
        let start_var = make_z3_int_var start_name ctx 
        let end_var = make_z3_int_var end_name ctx 
        let eq = ctx.MkEq(start_var, end_var)
        equal_condition <- ctx.MkAnd(equal_condition, eq)

    equal_condition

let find_cycle (network: QN.node list) bounds length range =
    let var_names = build_var_name_map network
    Z3Util.find_cycle 
        length 
        (unroll_qn_bounds network bounds var_names)
        (Z3Util.condition_states_equal network)


// SI: this is only ever called with length=-1, and only by one caller.
//     Better to work out diameter outside.
let find_cycle_steps network bounds length range =
    // Calculate a diameter if we need to...
    let diameter =
        if length = -1 then
            Map.fold (fun total _ (lo: int, hi: int) -> let res = bigint.Multiply(total, bigint.Subtract(bigint.Add(bigint.One, bigint(hi)), bigint(lo)))
                                                        res)
                     bigint.One
                     bounds
        else
            bigint(length)

    // ...We want the first iteration of our loop to have k=2...
    let mutable k = 1
    let mutable cycle = None

    while bigint.Compare(diameter, bigint(k)) > 0 && cycle = None do
        k <- k*2
        Log.log_debug ((string)k)
        cycle <- find_cycle network bounds k range

    cycle
