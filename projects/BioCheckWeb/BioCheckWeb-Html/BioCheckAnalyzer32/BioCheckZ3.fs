(* Copyright (c) Microsoft Corporation. All rights reserved. *)

(* This file contains functions from BioCheck that manipulate Z3 *)

module BioCheckZ3

open Microsoft.Z3

open Expr

// Naming convension for Z3 variables
let get_z3_int_var_at_time (node : QN.node) time = sprintf "%s^%d" node.name time

let rec expr_to_z3 (qn:QN.node list) (node:QN.node) expr time (z : Context) =
    let node_min,node_max = node.range

    let rec tr expr = 
        match expr with
        | Var v ->
            // Use the node's original range 
            let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
            let v_min,v_max = v_defn.range
            let scale,displacement = 
                if (v_min<>v_max) then 
                    let t = z.MkRealNumeral(node_max - node_min)
                    let b = z.MkRealNumeral(v_max - v_min)
                    (z.MkDiv(t,b) , z.MkRealNumeral( (node_min - v_min):int ))
                else (z.MkRealNumeral 1, z.MkRealNumeral 0)

            let input_var = 
                let v_t = get_z3_int_var_at_time v_defn time
                z.MkToReal(z.MkConst(z.MkSymbol v_t, z.MkIntSort()))
            z.MkAdd(z.MkMul(input_var,scale),displacement)
            
        | Const c -> z.MkRealNumeral c
        | Plus(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkAdd(z1,z2)
        | Minus(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkSub(z1,z2)
        | Times(e1, e2) -> 
            let z1 = tr e1
            let z2 = tr e2
            z.MkMul(z1,z2)
        | Div(e1, e2) ->
            let z1 = tr e1
            let z2 = tr e2
            z.MkDiv(z1,z2)
        | Max(e1, e2) -> 
            let z1 = tr e1 
            let z2 = tr e2
            let is_gt = z.MkGt(z1, z2)
            z.MkIte(is_gt, z1, z2)
        | Min(e1, e2) ->
            let z1 = tr e1 
            let z2 = tr e2
            let is_lt = z.MkLt(z1, z2)
            z.MkIte(is_lt, z1, z2)
        // x:Real, m,n:Int. If x-1 < m <= x <= n < x+1. Then floor(x)=m and ceil(x)=n. 
        | Ceil e1 ->        
            let z1 = tr e1
            let floor = z.MkToReal( z.MkToInt(z1))
            let is_int = z.MkEq (floor, z1)
            let floor_plus_one = z.MkAdd(floor, z.MkRealNumeral(1))
            let ceil_assert = z.MkTrue 
            z.MkIte(is_int,floor,floor_plus_one)
        | Floor e1 ->
            let z1 = tr e1 
            let floor_assert = z.MkTrue
            z.MkToReal (z.MkToInt z1)
        | Abs e1 ->
            let z1 = tr e1
            let zero = z.MkRealNumeral(0)
            let z2 = z.MkSub(zero,z1)
            let is_gt_zero = z.MkGt(z1,zero)
            z.MkIte(is_gt_zero,z1,z2)
            //BH
        | Ave es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some z0 -> 
                                                let z1 = tr e1
                                                Some(z.MkAdd(z0, z1)))
                        None
                        es
            let cnt = z.MkRealNumeral (List.length es)
            match sum with
              | None -> (z.MkRealNumeral 0)
              | Some s -> (z.MkDiv(s, cnt))

        | Sum es -> // Sum() is needed to implement multiple activators and (or) inhibitors
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some z0 -> let z1 = tr e1 
                                                    Some(z.MkAdd(z0, z1)))
                        None
                        es
            match sum with
              | None -> (z.MkRealNumeral 0)
              | Some s -> s

    tr expr 


let assert_target_function (node: QN.node) qn bounds start_time end_time (z : Context) =
    let current_state_id = get_z3_int_var_at_time node start_time
    let current_state = z.MkConst(z.MkSymbol current_state_id, z.MkIntSort())

    let next_state_id = get_z3_int_var_at_time node end_time
    let next_state = z.MkConst(z.MkSymbol next_state_id, z.MkIntSort())

    let T_applied = z.MkToInt(expr_to_z3 qn node node.f start_time z)

    //Begin <-- Edit by Qinsi Wang

    // QSW: have not considered these two situations: the current value is max in the range, but and the same
    // time, the value of its target function is greater than its current value; and, the current value is min
    // in the range, but at the same time, the value of its target function is less than its current value.
    // In either case, we need to keep its current value. 

    let (lower: int, upper: int) = Map.find node.var bounds

    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkIntNumeral 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))
    let up = z.MkAnd(up, z.MkGt(z.MkIntNumeral upper, current_state))

    let same = z.MkEq(next_state, current_state)
    let tmpsame = z.MkEq(T_applied, current_state)
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkGt(T_applied, current_state), z.MkEq(z.MkIntNumeral upper, current_state)))
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkLt(T_applied, current_state), z.MkEq(z.MkIntNumeral lower, current_state)))
    let same = z.MkAnd(same, tmpsame)

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkIntNumeral 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))
    let dn = z.MkAnd(dn, z.MkLt(z.MkIntNumeral lower, current_state))

    (*
    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkIntNumeral 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))

    let same = z.MkEq(next_state, current_state)
    let same = z.MkAnd(same, z.MkEq(T_applied, current_state))

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkIntNumeral 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))
    *)
    // End 

    let cnstr = z.MkOr([|up;same;dn|])
    Log.log_debug (z.ToString cnstr)
    z.AssertCnstr cnstr


let assert_bound (node : QN.node) (lower : int , upper : int) (time : int) (z : Context) =
    let var_name = get_z3_int_var_at_time node time
    let v = z.MkConst(z.MkSymbol var_name, z.MkIntSort())
    let lower_bound = z.Simplify (z.MkGe(v, z.MkIntNumeral lower))
    let upper_bound = z.Simplify (z.MkLe(v, z.MkIntNumeral upper))

    Log.log_debug (z.ToString lower_bound)
    z.AssertCnstr lower_bound

    Log.log_debug (z.ToString upper_bound)
    z.AssertCnstr upper_bound

let unroll_qn_bounds qn bounds var_names start_time end_time z =
    // Assert the target functions...
    for node in qn do
        assert_target_function node qn bounds start_time end_time z

    // Now assert the bounds...
    for node in qn do
        assert_bound node (Map.find node.var bounds) start_time z
        assert_bound node (Map.find node.var bounds) end_time z

let rec build_var_name_map (qn : QN.node list) =
    match qn with
    | [] -> Map.empty
    | node :: nodes -> Map.add node.var node.name (build_var_name_map nodes)

// SI: this is only ever called with steps=0.
//     Can remove it, and replace calls to it by:
//         unrollw_qn qn bounds (build_var_name_map qn) 0 0 z
let qn_to_z3 (qn : QN.node list) bounds steps (z : Context) =
    // Build a mapping from variable IDs to their names...
    let var_names = build_var_name_map qn

    if steps = 0 then
        unroll_qn_bounds qn bounds var_names 0 0 z
    else
        for time in [0..steps-1] do
            unroll_qn_bounds qn bounds var_names time (time+1) z

let model_to_fixpoint (model : Model) =
    let mutable fixpoint = Map.empty

    for var in model.GetModelConstants() do
        let lhs = var.GetDeclName()
        let rhs = model.Eval(var, Array.empty).GetNumeralString()
        let value = int rhs

        fixpoint <- Map.add lhs value fixpoint

    fixpoint


let find_fixpoint (network : QN.node list) range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    qn_to_z3 network range 0 ctx
    let model = ref null

    let sat = ctx.CheckAndGetModel (model)

    // Did we find a fixpoint?
    // SI: not so much fixpoint as a solution to constraints imposed by qn_to_z3.
    let res = if sat = LBool.True then
                    Some(model_to_fixpoint (!model))
              else
                    None

    if (!model) <> null then (!model).Dispose()
    ctx.Dispose()
    cfg.Dispose()

    res

let assert_not_model (model : Model) (z : Context) =
    let mutable not_model = z.MkFalse()

    for decl in model.GetModelConstants() do
        let lhs = z.MkConst decl
        let rhs = model.Eval(lhs)
        let new_val = z.MkNot (z.MkEq(lhs, rhs))
        not_model <- z.MkOr(not_model, new_val)

    z.AssertCnstr not_model

let find_bifurcation (network : QN.node list) range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    qn_to_z3 network range 0 ctx
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

let assert_states_equal (qn : QN.node list) start_time end_time (ctx : Context) =
    let mutable equal_condition = ctx.MkTrue()

    for node in qn do
        let start_name = get_z3_int_var_at_time node start_time
        let end_name = get_z3_int_var_at_time node end_time
        let start_var = ctx.MkConst(start_name, ctx.MkIntSort())
        let end_var = ctx.MkConst(end_name, ctx.MkIntSort())
        let eq = ctx.MkEq(start_var, end_var)
        equal_condition <- ctx.MkAnd(equal_condition, eq)

    equal_condition

let find_cycle (network: QN.node list) bounds length range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
    let var_names = build_var_name_map network

    Log.log_debug ("Finding cycle of length " + (string)length)

    Log.log_debug ("Diameter of system is " + (string)length)

    // Unroll the model k-times
    for time in [0..length] do
        unroll_qn_bounds network bounds var_names time (time+1) ctx

    // Assert that we get a repetition somewhere...
    let mutable loop_condition = ctx.MkFalse()
    for time in [2..length] do
        let k_loop = assert_states_equal network 0 time ctx
        loop_condition <- ctx.MkOr(loop_condition, k_loop)

    ctx.AssertCnstr loop_condition

    // Assert that the start of the loop is _not_ a fixpoint
    // SI: don't we mean not (s_i = S_{i+1}) ?
    let not_fixpoint = ctx.MkNot (assert_states_equal network 0 1 ctx)
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