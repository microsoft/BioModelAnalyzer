﻿(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Z

/// Z3 related stuff.

open Microsoft.Z3

open Expr

///////////////////////////////////////////////////////////////////////////////
// Encode the target function in Z3
///////////////////////////////////////////////////////////////////////////////

// SI: not sure about the encoding of floor/ceil. 
let rec expr_to_z3 (qn:QN.node list) (node:QN.node) expr time (z : Context) =
    let node_min,node_max = node.range

    let rec tr expr = 
        match expr with
        | Var v ->
            // Use the node's original range 
            //let node_min,node_max = node.range
            let v_min,v_max = 
                let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
                v_defn.range
//            let scale = 
//                if (v_min<>v_max) then z.MkRealNumeral( ((node_max - node_min) / (v_max - v_min)):int ) 
//                else z.MkRealNumeral(1)
//            let displacement = z.MkRealNumeral( (node_min - v_min):int ) 
            // Don't scale/displace constants. 
            // SI: do the same Expr.eval. 
            let scale,displacement = 
                if (v_min<>v_max) then 
                    // (z.MkRealNumeral( ((node_max - node_min) / (v_max - v_min)):int ), z.MkRealNumeral( (node_min - v_min):int ))
                    let t = z.MkRealNumeral(node_max - node_min)
                    let b = z.MkRealNumeral(v_max - v_min)
                    (z.MkDiv(t,b) , z.MkRealNumeral( (node_min - v_min):int ))
                else (z.MkRealNumeral 1, z.MkRealNumeral 0)

            let input_var = 
                let v_t = sprintf "%d^%d" v time
                z.MkToReal(z.MkConst(z.MkSymbol v_t, z.MkIntSort()))
            ([], z.MkMul(z.MkAdd(input_var,displacement), scale))
        | Const c -> ([],z.MkRealNumeral c)
        | Plus(e1, e2) -> 
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkAdd(z1,z2))
        | Minus(e1, e2) -> 
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkSub(z1,z2))
        | Times(e1, e2) -> 
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkMul(z1,z2))
        | Div(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkDiv(z1,z2))
        | Max(e1, e2) -> 
            let (a1,z1) = tr e1 
            let (a2,z2) = tr e2
            let is_gt = z.MkGt(z1, z2)
            (a1@a2, z.MkIte(is_gt, z1, z2))
        | Min(e1, e2) ->
            let (a1,z1) = tr e1 
            let (a2,z2) = tr e2
            let is_lt = z.MkLt(z1, z2)
            (a1@a2, z.MkIte(is_lt, z1, z2))
        // x:Real, m,n:Int. If x-1 < m <= x <= n < x+1. Then floor(x)=m and ceil(x)=n. 
        | Ceil e1 ->        
            let (a1,z1) = tr e1
            //z.MkToReal (z.MkToInt (z.MkAdd(z.MkRealNumeral "99 / 100", z1)))
            let ceil_assert = z.MkTrue 
            (ceil_assert::a1, z.MkToReal (z.MkToInt (z.MkAdd(z.MkRealNumeral "0.5", z1))))
        | Floor e1 ->
            let (a1,z1) = tr e1 
            let floor_assert = z.MkTrue
            (floor_assert::a1, z.MkToReal (z.MkToInt z1))
        | Ave es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some (a0,z0) -> 
                                                let (a1,z1) = tr e1
                                                Some(a0@a1, z.MkAdd(z0, z1)))
                        None
                        es
            let cnt = z.MkRealNumeral (List.length es)
            match sum with
              | None -> ([], z.MkRealNumeral 0)
              | Some (a,s) -> (a, z.MkDiv(s, cnt))

    let (extra_asserts,z) = tr expr 
    (extra_asserts,z)

///////////////////////////////////////////////////////////////////////////////
// unroll_qn 
///////////////////////////////////////////////////////////////////////////////

//assert  (v_t+1 = (v_t + 1) /\ T(v_t) > v_t) \/ 
//        (v_t+1 = (v_t)     /\ T(v_t) = v_t) \/
//        (v_t+1 = (v_t - 1) /\ T(v_t) < v_t) 
let assert_target_function qn (node: QN.node)  bounds start_time end_time (z : Context) =
    let current_state_id = sprintf "%d^%d" node.var start_time
    let current_state = z.MkConst(z.MkSymbol current_state_id, z.MkIntSort())

    let next_state_id = sprintf "%d^%d" node.var end_time
    let next_state = z.MkConst(z.MkSymbol next_state_id, z.MkIntSort())
 
    // SI: float->int conversion. Should use ceil or floor...
    let (extra_asserts,z_of_f) = expr_to_z3 qn node node.f start_time z
    let T_applied = z.MkToInt(z_of_f)
    
    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkIntNumeral 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))

    let same = z.MkEq(next_state, current_state)
    let same = z.MkAnd(same, z.MkEq(T_applied, current_state))

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkIntNumeral 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))

    let cnstr = z.MkOr([|up;same;dn|])
    Log.log_debug ("T_" + (string)node.var + ":" + z.ToString cnstr)
    z.AssertCnstr cnstr

// assert  lower <= v_t <= upper
let assert_bound (node : QN.node) ((lower,upper) : (int*int)) time (z : Context) =
    let var_name = sprintf "%d^%d" node.var time
    let v = z.MkConst(z.MkSymbol var_name, z.MkIntSort())

    let simplify =  id // z.Simplify
    let lower_bound = simplify (z.MkGe(v, z.MkIntNumeral lower))
    let upper_bound = simplify (z.MkLe(v, z.MkIntNumeral upper))

    Log.log_debug ("// " + var_name + "_lower_bound >= " + (string)lower)
    Log.log_debug (var_name + "_lower_bound:" + z.ToString lower_bound)
    z.AssertCnstr lower_bound

    Log.log_debug ("// " + var_name + "_upper_bound <=" + (string)upper)
    Log.log_debug (var_name + "_upper_bound:" + z.ToString upper_bound)
    z.AssertCnstr upper_bound

let unroll_qn qn bounds start_time end_time z =
    // Assert the target functions...
    for node in qn do
        assert_target_function qn node  bounds start_time end_time z

    // Now assert the bounds...
    for node in qn do
        assert_bound node (Map.find node.var bounds) start_time z
        assert_bound node (Map.find node.var bounds) end_time z


//let rec build_var_name_map (qn : QN.node list) =
//    match qn with
//    | [] -> Map.empty
//    | node :: nodes -> Map.add node.var ((string)node.var) (build_var_name_map nodes)

// SI: this is only ever called with steps=0.
//     Can remove it, and replace calls to it by:
//         unroll_qn qn bounds (build_var_name_map qn) 0 0 z
//let qn_to_z3 (qn : QN.node list) bounds steps (z : Context) =
//    // Build a mapping from variable IDs to their names...
//    let var_names = build_var_name_map qn
//
//    if steps = 0 then
//        unroll_qn qn bounds var_names 0 0 z
//    else
//        for time in [0..steps-1] do
//            unroll_qn qn bounds var_names time (time+1) z


///////////////////////////////////////////////////////////////////////////////
// model to fixpoint 
///////////////////////////////////////////////////////////////////////////////

let model_to_fixpoint (model : Model) =
    let mutable fixpoint = Map.empty

    for var in model.GetModelConstants() do
        let lhs = var.GetDeclName()
        let rhs = model.Eval(var, Array.empty).GetNumeralString()
        let value = int rhs

        fixpoint <- Map.add lhs value fixpoint

    fixpoint



///////////////////////////////////////////////////////////////////////////////
// fixpoint
///////////////////////////////////////////////////////////////////////////////

let find_fixpoint (network : QN.node list) range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    // time "0" is just an arbitrary time here. 
    unroll_qn network range 0 0 ctx

    let model = ref null
    let sat = ctx.CheckAndGetModel (model)

    // Did we find a fixpoint?
    let res = if sat = LBool.True then
                    Some(model_to_fixpoint (!model))
              else
                    None

    if (!model) <> null then (!model).Dispose()
    ctx.Dispose()
    cfg.Dispose()

    res


///////////////////////////////////////////////////////////////////////////////
// bifurcation 
///////////////////////////////////////////////////////////////////////////////

let assert_not_model (model : Model) (z : Context) =
    let mutable not_model = z.MkFalse()

    for decl in model.GetModelConstants() do
        let lhs = z.MkConst decl
        let rhs = model.Eval(lhs)
        let new_val = z.MkNot (z.MkEq(lhs, rhs))
        not_model <- z.MkOr(not_model, new_val)

    Log.log_debug ("not_model: " + z.ToString not_model)
    z.AssertCnstr not_model

let find_bifurcation (network : QN.node list) range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    // time "0" is just an arbitrary time here. 
    unroll_qn network range 0 0 ctx

    let model = ref null
    let sat = ctx.CheckAndGetModel (model)

    // Did we find a fixpoint?
    if sat = LBool.True then
        assert_not_model !model ctx
    else
        Log.log_debug "No initial fixpoint when looking for bifurcation"

    let model2 = ref null
    let sat2 = ctx.CheckAndGetModel(model2)

    let res = if sat2 = LBool.True then
                let fix1 = model_to_fixpoint (!model)
                let fix2 = model_to_fixpoint (!model2)
                Some((fix1, fix2))
              else
                None

    if (!model) <> null then (!model).Dispose()
    if (!model2) <> null then (!model2).Dispose()
    ctx.Dispose()
    cfg.Dispose()

    res


///////////////////////////////////////////////////////////////////////////////
// cycle
///////////////////////////////////////////////////////////////////////////////

let assert_states_equal (qn : QN.node list) start_time end_time (ctx : Context) =
    let mutable equal_condition = ctx.MkTrue()

    for node in qn do
        let start_name = sprintf "%d^%d" node.var start_time
        let end_name = sprintf "%d^%d" node.var end_time
        let start_var = ctx.MkConst(start_name, ctx.MkIntSort())
        let end_var = ctx.MkConst(end_name, ctx.MkIntSort())
        let eq = ctx.MkEq(start_var, end_var)
        equal_condition <- ctx.MkAnd(equal_condition, eq)

    equal_condition

let find_cycle (network: QN.node list) bounds length =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
    //let var_names = build_var_name_map network

    Log.log_debug ("Searching for a cycle of length " + (string)length)

    // Unroll the model k-times
    for time in [0..length] do
        unroll_qn network bounds  time (time+1) ctx

    // Assert that we get a repetition somewhere...
    let mutable loop_condition = ctx.MkFalse()
    for time in [2..length] do
        let k_loop = assert_states_equal network 0 time ctx
        loop_condition <- ctx.MkOr(loop_condition, k_loop)

    //Log.log_debug ("loop_condition: " + ctx.ToString loop_condition)
    ctx.AssertCnstr loop_condition

    // Assert that the start of the loop is _not_ a fixpoint
    // SI: don't we mean not (s_i = S_{i+1}) ?
    let not_fixpoint = ctx.MkNot (assert_states_equal network 0 1 ctx)
    //Log.log_debug ("not_fixpoint: " + ctx.ToString not_fixpoint)
    ctx.AssertCnstr not_fixpoint

    // Now go find that cycle!
    let model = ref null

    let sat = ctx.CheckAndGetModel (model)

    // Did we find a loop?
    let res =
        if sat = LBool.True then
                 Some(model_to_fixpoint (!model))
        else
                 None

    if (!model) <> null then (!model).Dispose()
    ctx.Dispose()
    cfg.Dispose()

    res


let find_cycle_steps network diameter bounds =

    // ...We want the first iteration of our loop to have k=2...
    let mutable k = 1
    let mutable cycle = None

    //Log.log_debug ( "%A %A" diameter (bigint.Compare(diameter, bigint(k)))

    while bigint.Compare(diameter, bigint(k)) > 0 && cycle = None do
        k <- k*2
        Log.log_debug ( "find_cycle_steps " + (string)k )
        cycle <- find_cycle network bounds k

    cycle


// SI: Z has three entry points: find_bifurcation, find_cycle_steps, find_fixpoint. 
// _b and _f use range rather than bounds. Why? They should use bounds as that's 
// better information. (Unless, like Expr.eval, you really need the range at some
// function down the call chain. 
//     Also, there's no special casing for constants. That'll save some (small) 
// amount of work for Z3. 



///////////////////////////////////////////////////////////////////////////////
// simulate
///////////////////////////////////////////////////////////////////////////////

(*
1. unroll_qn 0 k
2. assign initial values to v_0. 
3. ask for value at t_k
*)
let simulate (network: QN.node list) (init:Map<QN.var,int>) bounds (length:int) =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    Log.log_debug ("Run a simulation of length " + (string)length + " in Z3")

    // Unroll the model length times. 
    for time in [0..length] do
        unroll_qn network bounds  time (time+1) ctx

    // Assign init values to the v_0's 


    // Extract (final) values from model 
    let model = ref null
    let sat = ctx.CheckAndGetModel (model)

    let res =
        if sat = LBool.True then
            Some(model_to_fixpoint (!model))
        else
            None

    if (!model) <> null then (!model).Dispose()
    ctx.Dispose()
    cfg.Dispose()

    res