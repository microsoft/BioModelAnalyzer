(* Copyright (c) Microsoft Corporation. All rights reserved. *)
//Begin <-- Added by Qinsi Wang
//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"


module Z3_old

(*

/// Z3 related stuff.
// treat range as a pair

open Microsoft.Z3

open Expr

let rec expr_to_z3 (node:QN.node) range expr (var_names : Map<int, string>) time (z : Context) =
    match expr with
    | Var v ->
        //let input_var = var_names.Item v
        let node_min,node_max = Map.find node.var range 
        let v_min,v_max = Map.find v range 
        // QSW: need to make sure that "v_max - v_min" is not equal to 0
        let scale = if (v_max - v_min) <> 0 then z.MkRealNumeral( ((node_max - node_min) / (v_max - v_min)):int )
                    else z.MkRealNumeral 0
        let displacement = z.MkRealNumeral( (node_min - v_min):int ) 
        let input_var = 
            let v_t = sprintf "%s^%d" (Map.find v var_names) time
            z.MkToReal(z.MkConst(z.MkSymbol v_t, z.MkIntSort()))
        z.MkMul(z.MkAdd(input_var,displacement), scale)
    | Const c -> z.MkRealNumeral c
    | Plus(e1, e2) -> z.MkAdd(expr_to_z3 node range e1 var_names time z, expr_to_z3 node range e2 var_names time z)
    | Minus(e1, e2) -> z.MkSub(expr_to_z3 node range e1 var_names time z, expr_to_z3 node range e2 var_names time z)
    | Times(e1, e2) -> z.MkMul(expr_to_z3 node range e1 var_names time z, expr_to_z3 node range e2 var_names time z)
    | Div(e1, e2) -> z.MkDiv(expr_to_z3 node range e1 var_names time z, expr_to_z3 node range e2 var_names time z)
    | Max(e1, e2) ->
        let z1 = expr_to_z3 node range e1 var_names time z
        let z2 = expr_to_z3 node range e2 var_names time z
        let is_gt = z.MkGt(z1, z2)
        z.MkIte(is_gt, z1, z2)
    | Min(e1, e2) ->
        let z1 = expr_to_z3 node range e1 var_names time z
        let z2 = expr_to_z3 node range e2 var_names time z
        let is_lt = z.MkLt(z1, z2)
        z.MkIte(is_lt, z1, z2)
    | Ceil e1 ->
        let z1 = expr_to_z3 node range e1 var_names time z
        //(*
        // QSW: the input format is not correct. Instead of using "0.5", we use "99/100". 
        let half = z.MkRealNumeral "99 / 100"
        let z1_half = z.MkAdd(half, z1)
        z.MkToReal (z.MkToInt z1_half )
        
    | Floor e1 ->
        let z1 = expr_to_z3 node range e1 var_names time z
        z.MkToReal (z.MkToInt z1)
    | Ave es ->
        let sum = List.fold
                    (fun ast e1 -> match ast with
                                   | None -> Some(expr_to_z3 node range e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 node range e1 var_names time z
                                                Some(z.MkAdd(z0, z1)))
                    None
                    es
        let cnt = z.MkRealNumeral (List.length es)

        match sum with
        | None -> z.MkRealNumeral 0
        | Some s -> z.MkDiv(s, cnt)
    | Sum es ->
        let sum = List.fold
                    (fun ast e1 -> match ast with
                                   | None -> Some(expr_to_z3 node range e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 node range e1 var_names time z
                                                Some(z.MkAdd(z0, z1)))
                    None
                    es
        let cnt = z.MkRealNumeral 1

        match sum with
        | None -> z.MkRealNumeral 0
        | Some s -> z.MkDiv(s, cnt)


let assert_target_function (node: QN.node) var_names range start_time end_time (z : Context) =
    let current_state_id = sprintf "%s^%d" node.name start_time
    let current_state = z.MkConst(z.MkSymbol current_state_id, z.MkIntSort())

    let next_state_id = sprintf "%s^%d" node.name end_time
    let next_state = z.MkConst(z.MkSymbol next_state_id, z.MkIntSort())

    let T_applied = z.MkToInt(expr_to_z3 node range node.f var_names start_time z)

    // QSW: have not considered these two situations: the current value is max in the range, but and the same
    // time, the value of its target function is greater than its current value; and, the current value is min
    // in the range, but at the same time, the value of its target function is less than its current value.
    // In either case, we need to keep its current value. 

    let (lower: int, upper: int) = Map.find node.var range

     
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

    let cnstr = z.MkOr([|up;same;dn|])
    Log.log_debug (z.ToString cnstr)
    z.AssertCnstr cnstr



let assert_bound (node : QN.node) (lower : int, upper: int) (value : int) time (z : Context) =
    let var_name = sprintf "%s^%d" node.name time
    let v = z.MkConst(z.MkSymbol var_name, z.MkIntSort())
    let lower_bound = z.Simplify (z.MkGe(v, z.MkIntNumeral lower))
    let upper_bound = z.Simplify (z.MkLe(v, z.MkIntNumeral upper))
    
    Log.log_debug (z.ToString lower_bound)
    z.AssertCnstr lower_bound

    Log.log_debug (z.ToString upper_bound)
    z.AssertCnstr upper_bound

//(*
let assert_query (node : QN.node) (value : int) time (z : Context) = 
    let var_name = sprintf "%s^%d" node.name time
    let v = z.MkConst(z.MkSymbol var_name, z.MkIntSort())
    let query = z.MkEq (v, z.MkIntNumeral value)
    z.AssertCnstr query
//*)
        
let unroll_qn_bounds qn range var_names start_time end_time z =
    // Assert the target functions...
    for node in qn do
        assert_target_function node var_names range start_time end_time z

    // Now assert the bounds...
    for node in qn do
        assert_bound node (Map.find node.var range) 2 start_time z
        // assert_bound node (Map.find node.var range) end_time z

let rec build_var_name_map (qn : QN.node list) =
    match qn with
    | [] -> Map.empty
    | node :: nodes -> Map.add node.var node.name (build_var_name_map nodes)


let qn_to_z3 (qn : QN.node list) range step (z : Context) =
    // Build a mapping from variable IDs to their names...
    let var_names = build_var_name_map qn

    // if steps = 0 then
    unroll_qn_bounds qn range var_names step (step+1) z
    (*
    else
        for time in [0..steps-1] do
            unroll_qn_bounds qn range var_names time (time+1) z
    *)

let find_paths (network : QN.node list) step range =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    qn_to_z3 network range 0 ctx
    
   
    //
    qn_to_z3 network range 1 ctx
    
    //    
    qn_to_z3 network range 2 ctx
     (*
    //    
    qn_to_z3 network range 3 ctx
     
    //    
    qn_to_z3 network range 4 ctx
    
    //       
    qn_to_z3 network range 5 ctx
    
    //    
    qn_to_z3 network range 6 ctx
    
    //    
    qn_to_z3 network range 7 ctx
    
    //    
    qn_to_z3 network range 8 ctx
    
    //    
    qn_to_z3 network range 9 ctx
    
    //    
    qn_to_z3 network range 10 ctx
    
    *)

    //(*
    for node in network do
        
        let (lower: int, upper: int) = Map.find node.var range
        
        for i in [lower .. upper] do
            ctx.Push()
            let model = ref null
            assert_query (node : QN.node) (i : int) (step+1) ctx
            let sat = ctx.CheckAndGetModel (model)
            let res = if sat = LBool.True then
                            printfn "%s equals to %d can be satisfied!" node.name i
                           // printfn "Yes"
                      else 
                            printfn "You find mew info! %s equals to %d cannot be satisfied!" node.name i
                            //printfn "No"

            if (!model) <> null then (!model).Dispose()
            ctx.Pop()

    
    ctx.Dispose()
    cfg.Dispose()

*)
*) 
*)


//End <-- Can be ignored since there is stepZ3rangelist

