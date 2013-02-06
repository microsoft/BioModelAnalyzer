(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//Begin <-- Added by Qinsi Wang

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"


module stepZ3rangelist

// Z3 related stuff.
// the input rangelist is a list 
// Also (the difference between this and Z3rangelist)
// for each step, just push the transition constraints 
// related to the inputs of one variable.

open Microsoft.Z3

open Expr
(*
let rec expr_to_z3 (qn : QN.node list) (node:QN.node) expr (var_names : Map<int, string>) time (z : Context) =
    match expr with
    | Var v ->
        // Use the node's original range 
        let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
        let node_min,node_max = (node.min,node.max)
        let v_min,v_max = (v_defn.min,v_defn.max)
        // Don't scale/displace constants. 
        // SI: do the same Expr.eval. 
        let scale,displacement = 
            if (v_min<>v_max) then 
               let t = z.MkRealNumeral(node_max - node_min)
               let b = z.MkRealNumeral(v_max - v_min)
               (z.MkDiv(t,b) , z.MkRealNumeral( (node_min - v_min):int ))
            else (z.MkRealNumeral 1, z.MkRealNumeral 0)

        let input_var = 
            let v_t = BioCheckZ3.get_z3_int_var_at_time v_defn time
            z.MkToReal(z.MkConst(z.MkSymbol v_t, z.MkIntSort()))
        z.MkMul(z.MkAdd(input_var,displacement), scale)

    | Const c -> z.MkRealNumeral c
    | Plus(e1, e2) -> z.MkAdd(expr_to_z3 qn node e1 var_names time z, expr_to_z3 qn node e2 var_names time z)
    | Minus(e1, e2) -> z.MkSub(expr_to_z3 qn node e1 var_names time z, expr_to_z3 qn node e2 var_names time z)
    | Times(e1, e2) -> z.MkMul(expr_to_z3 qn node e1 var_names time z, expr_to_z3 qn node e2 var_names time z)
    | Div(e1, e2) -> z.MkDiv(expr_to_z3 qn node e1 var_names time z, expr_to_z3 qn node e2 var_names time z)
    | Max(e1, e2) ->
        let z1 = expr_to_z3 qn node e1 var_names time z
        let z2 = expr_to_z3 qn node e2 var_names time z
        let is_gt = z.MkGt(z1, z2)
        z.MkIte(is_gt, z1, z2)
    | Min(e1, e2) ->
        let z1 = expr_to_z3 qn node e1 var_names time z
        let z2 = expr_to_z3 qn node e2 var_names time z
        let is_lt = z.MkLt(z1, z2)
        z.MkIte(is_lt, z1, z2)
    | Ceil e1 ->
        let z1 = expr_to_z3 qn node e1 var_names time z
        let half = z.MkRealNumeral "99 / 100"
        let z1_half = z.MkAdd(half, z1)
        z.MkToReal (z.MkToInt z1_half )
        
    | Floor e1 ->
        let z1 = expr_to_z3 qn node e1 var_names time z
        z.MkToReal (z.MkToInt z1)
    | Ave es ->
        let sum = List.fold
                    (fun ast e1 -> match ast with
                                   | None -> Some(expr_to_z3 qn node e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 qn node e1 var_names time z
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
                                   | None -> Some(expr_to_z3 qn node e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 qn node e1 var_names time z
                                                Some(z.MkAdd(z0, z1)))
                    None
                    es
        let cnt = z.MkRealNumeral 1

        match sum with
        | None -> z.MkRealNumeral 0
        | Some s -> z.MkDiv(s, cnt)
        *)

let assert_target_function qn (node: QN.node) var_names (rangelist : Map<QN.var,int list>) start_time end_time (z : Context) =
    let current_state_id = BioCheckPlusZ3.get_z3_int_var_at_time node start_time
    let current_state = z.MkConst(z.MkSymbol current_state_id, z.MkIntSort())

    let next_state_id = BioCheckPlusZ3.get_z3_int_var_at_time node end_time
    let next_state = z.MkConst(z.MkSymbol next_state_id, z.MkIntSort())

    let z3_target_function = BioCheckZ3.expr_to_z3 qn node node.f start_time z
    // let T_applied = z.MkToInt(z3_target_function)
    let half = z.MkDiv(z.MkRealNumeral(1),z.MkRealNumeral(2))
    let tf_plus_half = z.MkAdd(half,z3_target_function)
    let T_applied = z.MkToInt (tf_plus_half)

    let list = Map.find node.var rangelist
    let lower = List.min list
    let upper = List.max list
     
    // If the next value is greater than current value and current value is not
    // max the value needs to increase
    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkIntNumeral 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))
    let up = z.MkAnd(up, z.MkGt(z.MkIntNumeral upper, current_state))

    // If the next value is the same as current value of the next value is 
    // greater than the current value but current value is max or
    // the next value is smaller than the current value but current value is min
    // the value needs to stay the same
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
    
let assert_query (node : QN.node) (value : int) time (z : Context) = 
    let var_name = BioCheckPlusZ3.get_z3_int_var_at_time node time
    let v = z.MkConst(z.MkSymbol var_name, z.MkIntSort())
    let query = z.MkEq (v, z.MkIntNumeral value)
    z.AssertCnstr query

let find_paths (network : QN.node list) step rangelist orbounds=
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
    
    ctx.Push()
    
    // use to record new bounds on variables
    let mutable nubounds = Map.empty

    let UpdatePossibleValues (node : QN.node) oldPossibleValues =
            
        let mutable newPossibleValues : int list = List.empty

        if List.length oldPossibleValues = 1 
        then 
            newPossibleValues <- oldPossibleValues
        else             
            for i in oldPossibleValues do
                    
                let (model : ref<Model>) = ref null

                ctx.Push()
                assert_query (node : QN.node) (i : int) (step+1) ctx
                let sat = ctx.CheckAndGetModel (model)
                        
            
                if sat = LBool.True then
                    newPossibleValues <- i :: newPossibleValues
                elif sat = LBool.Undef then
                    if (!model = null) then
                        printfn "z3 returned unknown"
                        newPossibleValues <- i :: newPossibleValues
                    else
                        // (!model).Eval find_the_conjunction_of_assertions
                        // Todo:
                        // Implement a model checker that evaluates the 
                        // value of the formula under the model 
                        // actually evaluates the formula to true
                        newPossibleValues <- i :: newPossibleValues


                ctx.Pop()         
                if (!model) <> null then (!model).Dispose()
            
        if (newPossibleValues.Length = 0) then
            // Something went wrong! Throw an exception at some point
            // when a real software engineer looks on this.
            newPossibleValues <- []
 
        List.sort newPossibleValues
            

    // For each action step, we only handle one variable and its inputs.
    for node in network do
            
        ctx.Push()
            
        // then find the correponding nodes with these node.var(s)
        // network is a list, and each member has four elements
        // we need to return the whole node according to .var element
        let nodeinputlist =
            List.concat
                [ for var in node.inputs do
                    yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) network) ]
        let nodelist = node :: nodeinputlist
        let varnamenode = BioCheckZ3.build_var_name_map nodelist
        for n in nodelist do
            let rangeOfN = Map.find n.var rangelist
            let nMin = List.min rangeOfN
            let nMax = List.max rangeOfN
            BioCheckZ3.assert_bound n (nMin , nMax) step ctx
        // Then, push related transition constraints
        assert_target_function network node varnamenode orbounds step (step+1) ctx
        // Then, we can begin to query about this node/variable
        let currentPossibleValues = Map.find node.var rangelist
                   
        nubounds <- Map.add node.var (UpdatePossibleValues node currentPossibleValues) nubounds
        ctx.Pop()
            
    ctx.Pop()

    ctx.Dispose()
    cfg.Dispose()

    nubounds
//End <-- For each time step, encode the system (model) in SMT format, and push into Z3 to compute possible values of each variables