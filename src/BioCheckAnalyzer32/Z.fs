(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Z

/// Z3 related stuff.

open Microsoft.Z3

open Expr

///////////////////////////////////////////////////////////////////////////////
// Encode the target function in Z3
///////////////////////////////////////////////////////////////////////////////

let gensym =
    let counter = ref 0
    (fun s -> incr counter; s + ((string)!counter))

// Naming convention for Z3 variables
// The same functions appear in BioCheckZ3 and BioCheckPlusZ3
let get_z3_int_var_at_time (node : QN.node) time = sprintf "v%d^%d" node.var time
let make_z3_int_var (name : string) (z : Context) = z.MkConst(z.MkSymbol(name),z.MkIntSort())


let get_qn_var_from_z3_var (name : string) =
    let parts = name.Split[|'^'|]
    let id = (parts.[0]).Substring 1
    ((int id) : QN.var)

let get_qn_var_at_t_from_z3_var (name : string) =
    let parts = name.Split[|'^'|]
    let id = (parts.[0]).Substring 1
    ((int id),(int parts.[1]) : QN.var * int)

// The env encoding is different as it does not include the v before the %d^%d!
let enc_for_env_qn_id_string_at_t (id : string) time =
    (id +  "^" + ((string)time))

let enc_for_env_qn_id_at_t (id : QN.var) time =
    enc_for_env_qn_id_string_at_t ((string) id) time

let dec_from_env_qn_id_at_t (name : string) = 
    let parts = name.Split[|'^'|]
    let id = parts.[0]
    ((id),((int)parts.[1]))


// Z.expr_to_z3 should be similar to Expr.eval_expr_int.
let expr_to_z3 (qn:QN.node list) (node:QN.node) expr time (z : Context) =
    let node_min,node_max = node.range

    let rec tr expr =
        match expr with
        | Var v ->
            // Use the node's original range
            //let node_min,node_max = node.range
            let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
            let v_min,v_max = v_defn.range
            // Don't scale/displace constants.
            let scale,displacement =
                if (v_min<>v_max) then
                    let t = z.MkRealNumeral(node_max - node_min)
                    let b = z.MkRealNumeral(v_max - v_min)
                    (z.MkDiv(t,b) , z.MkRealNumeral( (node_min - v_min):int ))
                else (z.MkRealNumeral 1, z.MkRealNumeral 0)

            let input_var =
                let v_t = get_z3_int_var_at_time v_defn time
                z.MkToReal(make_z3_int_var v_t z)
            ([], z.MkAdd(z.MkMul(input_var,scale), displacement))
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
(* Nir's code:
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
*)
        | Ceil e1 -> // TODO: CHECK WITH SAMIN
            let (a1,x) = tr e1
            let x' = z.MkAdd(x, z.MkRealNumeral(1))
            let n =
                let n = gensym "ceil"
                z.MkToReal(make_z3_int_var n z)
            let x_leq_n = z.MkLe(x, n)
            let n_lt_x' = z.MkLt(n, x')
            let ceil_assert = z.MkAnd([|x_leq_n;n_lt_x'|])
            (ceil_assert::a1, n)
        | Floor e1 -> // TODO: CHECK WITH SAMIN
            let (a1,x) = tr e1
            let x' = z.MkSub(x, z.MkRealNumeral(1))
            let m =
                let m = gensym "floor"
                z.MkToReal(make_z3_int_var m z)
            let x'_lt_m = z.MkLt(x', m)
            let m_le_x = z.MkLe(m, x)
            let floor_assert = z.MkAnd([|x'_lt_m; m_le_x|])
            (floor_assert::a1, m)
        | Abs e1 ->
            let (a1,x) = tr e1
            let zero = z.MkRealNumeral(0)
            let x_neg = z.MkSub(zero,x)
//            let l = 
//                let l = gensym "abs"
//                z.MkToReal(z.MkConst(z.MkSymbol l, z.MkRealSort() ))
//            let zero_lt_l = z.MkLt(z.MkRealNumeral(0),l)
//            let both = z.MkOr([|x;x'|]) // Problem -> Or applied to reals
//            let abs_assert = z.MkAnd([|zero_lt_l;both|])
            let x_gt_zero = z.MkGt(x,zero)
            let x_lt_zero = z.MkLt(x,zero)
            let abs = z.MkIte(x_gt_zero,x,x_neg)


            let a' = z.MkToReal(make_z3_int_var (gensym "absolute") z)
            let a_eq_abs = z.MkEq(a',abs)
//
//            let zero_lt_l = z.MkLt(z.MkRealNumeral(0),l)
//            let both = z.MkSetAdd(x,x')
//            let abs_assert = z.MkAnd([|zero_lt_l;both|])
            (a_eq_abs::a1, a')//broken- need to manipulate a1? BH
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


        | Sum es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some (a0,z0) ->
                                                    let (a1,z1) = tr e1
                                                    Some(a0@a1, z.MkAdd(z0, z1)))
                        None
                        es
            match sum with
              | None -> ([],z.MkRealNumeral 0)
              | Some (a,s) -> (a,s)

    // SI: Garvit's rounding code (see eval_expr too). 
    let (extra_asserts, ze) = tr expr
    (extra_asserts, z.MkAdd(ze, z.MkDiv(z.MkRealNumeral(1), z.MkRealNumeral(2))))

///////////////////////////////////////////////////////////////////////////////
// unroll_qn
///////////////////////////////////////////////////////////////////////////////

//assert  (v_t+1 = (v_t + 1) /\ T(v_t) > v_t) \/
//        (v_t+1 = (v_t)     /\ T(v_t) = v_t) \/
//        (v_t+1 = (v_t - 1) /\ T(v_t) < v_t)
let assert_target_function qn (node: QN.node)  bounds start_time end_time (z : Context) =
    // SI: should be able to use [get_z3_int_var_at_time node] for this too, like we do for next_state_id. 
    let current_state_id = get_z3_int_var_at_time node start_time
    let current_state = make_z3_int_var current_state_id z

    let next_state_id = get_z3_int_var_at_time node end_time
    let next_state = make_z3_int_var next_state_id z

    let (extra_asserts,z_of_f) = expr_to_z3 qn node node.f start_time z
    let T_applied = z.MkToInt(z_of_f)

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

    let cnstr = z.MkOr([|up;same;dn|])

    List.iter
        (fun c -> Log.log_debug ("T_" + (string)node.var + "_xtra_assrt: " + z.ToString c))
        extra_asserts
    z.AssertCnstr (z.MkAnd((Array.ofList extra_asserts)))

    Log.log_debug ("T_" + (string)node.var + ":" + z.ToString cnstr)
    z.AssertCnstr cnstr

// assert  lower <= v_t <= upper
let assert_bound (node : QN.node) ((lower,upper) : (int*int)) time (z : Context) =
    let var_name = get_z3_int_var_at_time node time
    let v = make_z3_int_var var_name z

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



///////////////////////////////////////////////////////////////////////////////
// model to fixpoint
///////////////////////////////////////////////////////////////////////////////

// Return [model] constants as a Map<string,int>.
let model_to_fixpoint (model : Model) =
    let mutable fixpoint = Map.empty

    for var in model.GetModelConstants() do
        let lhs = var.GetDeclName()
        let rhs = model.Eval(var, Array.empty).GetNumeralString()
        let value = int rhs

        fixpoint <- Map.add lhs value fixpoint

    fixpoint

// Given the naming convention for Z3 vars, use [get_qn_var_at_t_from_z3_var]
// to filter out only valid QN vars. 
let fixpoint_to_env (fixpoint : Map<string, int>) =
    Map.fold
        (fun newMap name value ->
            try 
                let (id,t) = get_qn_var_at_t_from_z3_var name
                Map.add (enc_for_env_qn_id_at_t id t) value newMap
            with
                | exn -> newMap )
        Map.empty
        fixpoint

// Assumption:
// There is a state repeating twice in the map
// If there is no such state then there is a problem
let extract_cycle_from_model (env : Map<string, int>) =

    let value_for_name_in_cyclepoint_is_not_equivalent_to_value_for_name_in_lastpoint cycletime lastpointmap =
        (fun name value -> 
            let (id,time) = dec_from_env_qn_id_at_t name
            if (time <> cycletime) then false
            else 
                let value_in_lastpoint = 
                    try 
                        Map.find id lastpointmap
                    with
                        | exn -> value + 1
                if (value_in_lastpoint <> value) then true
                else false
         )


//    let mintime = Map.fold 
//                        (fun oldmin name value ->
//                            let (name, time) = dec_from_env_qn_id_at_t name
//                            if (time < oldmin) then time
//                            else oldmin
//                        )
//                        1000
//                        env
    let maxtime = Map.fold
                        (fun oldmax name value -> 
                            let (name, time) = dec_from_env_qn_id_at_t name
                            if (time > oldmax) then time
                            else oldmax
                        )
                        0
                        env

    let lastpoint = Map.fold
                        (fun oldmap name value -> 
                            let (id,time) = dec_from_env_qn_id_at_t name
                            if (time = maxtime) then Map.add id value oldmap
                            else oldmap 
                        )
                        Map.empty
                        env
    

    let mutable cyclepoint = 0
    while (Map.exists (value_for_name_in_cyclepoint_is_not_equivalent_to_value_for_name_in_lastpoint cyclepoint lastpoint) env) do
        cyclepoint <- cyclepoint + 1

    let cyclepointcopy = cyclepoint
    Map.fold 
        (fun oldmap name value ->
            let (id, time) = dec_from_env_qn_id_at_t name
            if (time < cyclepointcopy) then oldmap
            else Map.add (enc_for_env_qn_id_string_at_t id (time - cyclepointcopy)) value oldmap
        )
        Map.empty
        env

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
                    let env = fixpoint_to_env (model_to_fixpoint !model)
                    Some(env)
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
                let env1 = fixpoint_to_env (model_to_fixpoint !model) 
                let env2 = fixpoint_to_env (model_to_fixpoint !model2)
                Some((env1, env2))
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
        let start_name = get_z3_int_var_at_time node start_time
        let end_name = get_z3_int_var_at_time node end_time
        let start_var = make_z3_int_var start_name ctx
        let end_var = make_z3_int_var end_name ctx
        let eq = ctx.MkEq(start_var, end_var)
        equal_condition <- ctx.MkAnd(equal_condition, eq)

    equal_condition

let find_cycle (network: QN.node list) bounds length =
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    Log.log_debug ("Searching for a cycle of length " + (string)length)

    // Unroll the model k-times
    for time in [0..length] do
        unroll_qn network bounds  time (time+1) ctx

    // Assert that we get a repetition somewhere...
    let mutable loop_condition = ctx.MkFalse()
    for time in [2..length] do
        let k_loop = assert_states_equal network 0 time ctx
        loop_condition <- ctx.MkOr(loop_condition, k_loop)

    ctx.AssertCnstr loop_condition

    // Assert that the start of the loop (at an arbitrary time 0 and 1) is _not_ a fixpoint
    let not_fixpoint = ctx.MkNot (assert_states_equal network 0 1 ctx)
    ctx.AssertCnstr not_fixpoint

    // Now go find that cycle!
    let model = ref null

    let sat = ctx.CheckAndGetModel (model)

    // Did we find a loop?
    let res =
        if sat = LBool.True then
            let env = fixpoint_to_env (model_to_fixpoint !model)
            Some(env)
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

    while bigint.Compare(diameter, bigint(k)) > 0 && cycle = None do
        k <- k*2
        Log.log_debug ( "find_cycle_steps " + (string)k )
        cycle <- find_cycle network bounds k

    cycle


// Finds a cycle and returns it. I f no cycle is found returns None
// 
// The search for cycles proceeds as follows:
// For increasing lengths, make sure that there is a path of this length
// If not, the search for a cycle failed.
// If a path of a certain length is possible then search for a cycle
// of the same length where a cycle is characterized by the last state
// on the path being equivalent to one of the states in the first half of the
// path.
// If no cycle is found try again with a path twice the length.
// 
// Postcondition:
// The returned cycle has the last state and one of the states in the first
// half of the path the same!
let find_cycle_steps_optimized network bounds synchronous = 
    let mutable cycle = None
    let rec find_cycle_of_length length (ctx : Context) =      
        let model = ref null

        // add to ctx the constratints to increase the path to 0..length:

        // Unroll the model (length/2)-times
        for time = (length/2) to (length-1) do
            unroll_qn network bounds (time) (time+1) ctx

        // Check that the last step is different than the one before it (fix point would come up here)
        let last_identical = assert_states_equal network (length-1) length ctx
        let last_not_identical = ctx.MkNot(last_identical)
        ctx.AssertCnstr last_not_identical

        let sat = ctx.CheckAndGetModel (model)
        if (!model) <> null then (!model).Dispose()

        match (sat,synchronous) with
        // A path of the requested length does not exists in the model, may stop the search now
        | (LBool.False,_) ->                 
                None
        // A path of the requested length exists, Check for a cycle in the range: length/2..length  
        | (LBool.True,true) -> 

            ctx.Push()
           
            // Assert that we get a repetition (cycle) in the range : length/2..length  
            let mutable loop_condition = ctx.MkFalse()
            for time in [0..((length/2)-1)] do 
                let k_loop = assert_states_equal network time length ctx
                loop_condition <- ctx.MkOr(loop_condition, k_loop)
                
            ctx.AssertCnstr loop_condition

            // Now go find that cycle
            let model = ref null
            let sat = ctx.CheckAndGetModel (model)
            ctx.Pop()
        
            match sat with
            | LBool.False -> 
                find_cycle_of_length (length*2) ctx
            | LBool.True -> 
                // update cycle with the information from model
                let env = fixpoint_to_env (model_to_fixpoint !model)
                let smallenv = extract_cycle_from_model env
                let res = Some(smallenv)
                if (!model) <> null then (!model).Dispose()
                res
        | (LBool.True,false) -> 

            ctx.Push()
           
            // Assert that we get a repetition (cycle) in the range : length/2..length  
            let mutable loop_condition = ctx.MkFalse()
            for time in [0..((length/2)-1)] do 
                let k_loop = assert_states_equal network time length ctx
                loop_condition <- ctx.MkOr(loop_condition, k_loop)
                
            ctx.AssertCnstr loop_condition

            // Now go find that cycle
            
            let rec asyncLoop (ctx:Context) =
                let model = ref null
                let sat = ctx.CheckAndGetModel (model)
                match sat with
                | LBool.False -> 
                    ctx.Pop()
                    find_cycle_of_length (length*2) ctx
                | LBool.True ->
                    //There is a cycle, but is does it collapse to fix point?
                    Log.log_debug "Testing for fix point"
                    //Convert the env to a state and perform a simulation in async space
                    let env = fixpoint_to_env (model_to_fixpoint !model)
                    assert_not_model !model ctx
                    if (!model) <> null then (!model).Dispose()
                    let smallenv = extract_cycle_from_model env
                    let state = Map.toList smallenv
                                |> List.filter (fun (varid,value) -> "0" = (varid.Split([|'^'|])).[1]  ) 
                                |> List.map (fun (varid,value) -> (int((varid.Split([|'^'|])).[0]),value)) 
                                |> Map.ofList
                    Log.log_debug (sprintf "Cycle start %A" state)
                    let endstate = Simulate.dfsAsyncFixPoint network state
                    match endstate with 
                    | Simulate.EndComponent(n) -> 
                    //This cycle leads to an endpoint- return the endpoint
                        let r = List.mapi ( fun i state -> Map.fold (fun (acc:Map<string,int>) key value -> acc.Add((sprintf "%d^%d" key i),value) ) Map.empty state ) n
                                |> List.fold (fun acc state -> Map.fold (fun (a:Map<string,int>) k v -> a.Add(k,v) ) acc state ) Map.empty
                        Some(r)
                    //This cycle collapses- try again until there are no more loops
                    | Simulate.FixPoint(n) -> 
                        Log.log_debug "Cycle collapsed- trying again"
                        Log.log_debug (sprintf "Fixpoint %A" n)
                        if (!model) <> null then (!model).Dispose()
                        asyncLoop ctx

            //failwith "Incomplete endpoint checking"
            asyncLoop ctx
        
//            match sat with
//            | LBool.False -> 
//                find_cycle_of_length (length*2) ctx
//            | LBool.True -> 
//                // update cycle with the information from model
//                let env = fixpoint_to_env (model_to_fixpoint !model)
//                let smallenv = extract_cycle_from_model env
//                let res = Some(smallenv)
//                if (!model) <> null then (!model).Dispose()
//                res
        
        | (LBool.Undef,_) -> None
     
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)

    // Prepare the first step - cannot contain a loop yet, and a fix point will be found via states 0==2
    let length=1    
    unroll_qn network bounds 0 length ctx   
    cycle <- find_cycle_of_length (length+1) ctx

    ctx.Dispose()
    cfg.Dispose()

    cycle


