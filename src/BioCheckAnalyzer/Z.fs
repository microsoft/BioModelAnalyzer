// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Z

/// Z3 related stuff.

open Microsoft.Z3
open VariableEncoding

open Expr

// Z.expr_to_z3 should be similar to Expr.eval_expr_int.
let expr_to_z3 (qn:QN.node list) (node:QN.node) expr time (z : Context) : BoolExpr list * RealExpr =
    let asReal (e:Expr) : RealExpr = e :?> RealExpr
    let node_min,node_max = node.range

    let rec tr expr : BoolExpr list * RealExpr =
        match expr with
        | Var v ->
            // Use the node's original range
            //let node_min,node_max = node.range
            let v_defn = List.find (fun (n:QN.node) -> n.var = v) qn
            let v_min,v_max = v_defn.range
            // Don't scale/displace constants.
            let scale,displacement =
                if (v_min<>v_max) then
                    let t = z.MkReal(node_max - node_min)
                    let b = z.MkReal(v_max - v_min)
                    (z.MkDiv(t,b) |> asReal, z.MkReal( (node_min - v_min):int ))
                else (upcast z.MkReal 1, z.MkReal 0)

            let input_var =
                let v_t = enc_z3_int_var_at_time v_defn time
                z.MkInt2Real(make_z3_int_var v_t z)
            ([], z.MkAdd(z.MkMul(input_var,scale), displacement) |> asReal)
        | Const c -> ([],upcast z.MkReal c)
        | Plus(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkAdd(z1,z2) |> asReal)
        | Minus(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkSub(z1,z2) |> asReal)
        | Times(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkMul(z1,z2) |> asReal)
        | Div(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            (a1@a2, z.MkDiv(z1,z2) |> asReal)
        | Max(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            let is_gt = z.MkGt(z1, z2)
            (a1@a2, z.MkITE(is_gt, z1, z2) |> asReal)
        | Min(e1, e2) ->
            let (a1,z1) = tr e1
            let (a2,z2) = tr e2
            let is_lt = z.MkLt(z1, z2)
            (a1@a2, z.MkITE(is_lt, z1, z2) |> asReal)
        // x:Real, m,n:Int. If x-1 < m <= x <= n < x+1. Then floor(x)=m and ceil(x)=n.
(* Nir's code:
        | Ceil e1 ->
            let z1 = tr e1
            let floor = z.MkToReal( z.MkReal2Int(z1))
            let is_int = z.MkEq (floor, z1)
            let floor_plus_one = z.MkAdd(floor, z.MkReal(1))
            let ceil_assert = z.MkTrue
            z.MkIte(is_int,floor,floor_plus_one)
        | Floor e1 ->
            let z1 = tr e1
            let floor_assert = z.MkTrue
            z.MkToReal (z.MkReal2Int z1)
*)
        | Ceil e1 -> // TODO: CHECK WITH SAMIN
            let (a1,x) = tr e1
            let x' = z.MkAdd(x, z.MkReal(1))
            let n =
                let n = gensym "ceil"
                z.MkInt2Real(make_z3_int_var n z)
            let x_leq_n = z.MkLe(x, n)
            let n_lt_x' = z.MkLt(n, x')
            let ceil_assert = z.MkAnd([|x_leq_n;n_lt_x'|])
            (ceil_assert::a1, n)
        | Floor e1 -> // TODO: CHECK WITH SAMIN
            let (a1,x) = tr e1
            let x' = z.MkSub(x, z.MkReal(1))
            let m =
                let m = gensym "floor"
                z.MkInt2Real(make_z3_int_var m z)
            let x'_lt_m = z.MkLt(x', m)
            let m_le_x = z.MkLe(m, x)
            let floor_assert = z.MkAnd([|x'_lt_m; m_le_x|])
            (floor_assert::a1, m)
        | Abs e1 ->
            let (a1,x) = tr e1
            let zero = z.MkReal(0)
            let x_neg = z.MkSub(zero,x)
//            let l = 
//                let l = gensym "abs"
//                z.MkToReal(z.MkConst(z.MkSymbol l, z.MkRealSort() ))
//            let zero_lt_l = z.MkLt(z.MkReal(0),l)
//            let both = z.MkOr([|x;x'|]) // Problem -> Or applied to reals
//            let abs_assert = z.MkAnd([|zero_lt_l;both|])
            let x_gt_zero = z.MkGt(x,zero)
            let x_lt_zero = z.MkLt(x,zero)
            let abs = z.MkITE(x_gt_zero,x,x_neg)


            let a' = z.MkInt2Real(make_z3_int_var (gensym "absolute") z)
            let a_eq_abs = z.MkEq(a',abs)
//
//            let zero_lt_l = z.MkLt(z.MkReal(0),l)
//            let both = z.MkSetAdd(x,x')
//            let abs_assert = z.MkAnd([|zero_lt_l;both|])
            (a_eq_abs::a1, a')//broken- need to manipulate a1? BH
        | Ave es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some (a0,z0) ->
                                                let (a1,z1) = tr e1
                                                Some(a0@a1, z.MkAdd(z0, z1) |> asReal))
                        None
                        es
            let cnt = z.MkReal (List.length es)
            match sum with
              | None -> ([], upcast z.MkReal 0)
              | Some (a,s) -> (a, z.MkDiv(s, cnt) |> asReal)


        | Sum es ->
            let sum = List.fold
                        (fun ast e1 -> match ast with
                                       | None -> Some(tr e1)
                                       | Some (a0,z0) ->
                                                    let (a1,z1) = tr e1
                                                    Some(a0@a1, z.MkAdd(z0, z1) |> asReal))
                        None
                        es
            match sum with
              | None -> ([],upcast z.MkReal 0)
              | Some (a,s) -> (a,s)

    // SI: Garvit's rounding code (see eval_expr too). 
    let (extra_asserts, ze) = tr expr
    (extra_asserts, z.MkAdd(ze, z.MkDiv(z.MkReal(1), z.MkReal(2))) |> asReal)

///////////////////////////////////////////////////////////////////////////////
// unroll_qn
///////////////////////////////////////////////////////////////////////////////

//assert  (v_t+1 = (v_t + 1) /\ T(v_t) > v_t) \/
//        (v_t+1 = (v_t)     /\ T(v_t) = v_t) \/
//        (v_t+1 = (v_t - 1) /\ T(v_t) < v_t)
let assert_target_function qn (node: QN.node)  bounds start_time end_time (z : Context) (s : Solver) =
    // SI: should be able to use [get_z3_int_var_at_time node] for this too, like we do for next_state_id. 
    let current_state_id = enc_z3_int_var_at_time node start_time
    let current_state = make_z3_int_var current_state_id z

    let next_state_id = enc_z3_int_var_at_time node end_time
    let next_state = make_z3_int_var next_state_id z

    let (extra_asserts,z_of_f) = expr_to_z3 qn node node.f start_time z
    let T_applied = z.MkReal2Int(z_of_f)

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

    let cnstr = z.MkOr([|up;same;dn|])

    List.iter
        (fun c -> Log.log_debug ("T_" + (string)node.var + "_xtra_assrt: " + c.ToString()))
        extra_asserts
    s.Assert (z.MkAnd((Array.ofList extra_asserts)))

    Log.log_debug ("T_" + (string)node.var + ":" + cnstr.ToString())
    s.Assert cnstr

// assert  lower <= v_t <= upper
let assert_bound (node : QN.node) ((lower,upper) : (int*int)) time (z : Context) (s : Solver) =
    let var_name = enc_z3_int_var_at_time node time
    let v = make_z3_int_var var_name z

    let simplify =  id // z.Simplify
    let lower_bound = simplify (z.MkGe(v, z.MkInt lower))
    let upper_bound = simplify (z.MkLe(v, z.MkInt upper))

    Log.log_debug ("// " + var_name + "_lower_bound >= " + (string)lower)
    Log.log_debug (var_name + "_lower_bound:" + lower_bound.ToString() )
    s.Assert lower_bound

    Log.log_debug ("// " + var_name + "_upper_bound <=" + (string)upper)
    Log.log_debug (var_name + "_upper_bound:" + upper_bound.ToString())
    s.Assert upper_bound

let unroll_qn qn bounds start_time end_time z s =
    // Assert the target functions...
    for node in qn do
        assert_target_function qn node  bounds start_time end_time z s

    // Now assert the bounds...
    for node in qn do
        assert_bound node (Map.find node.var bounds) start_time z s
        assert_bound node (Map.find node.var bounds) end_time z s



///////////////////////////////////////////////////////////////////////////////
// model to fixpoint
///////////////////////////////////////////////////////////////////////////////

// Given the naming convention for Z3 vars, use [get_qn_var_at_t_from_z3_var]
// to filter out only valid QN vars. 
let fixpoint_to_env (fixpoint : Map<string, int>) =
    Map.fold
        (fun newMap name value ->
            try 
                let (id,t) = dec_qn_var_at_t_from_z3_var name
                Map.add (enc_for_env_qn_id_at_t id t) value newMap
            with
                | exn -> newMap )
        Map.empty
        fixpoint

//returns a list of QNs for simulation as int,int maps
let env_to_QNState_list (env:Map<string,int>) =
    let env' = Map.toList env
    let time_ordered = env' |> List.sortBy (fun item -> fst item |> (fun id_time -> int (id_time.Split([|'^'|]).[1]) )  )
    let times = env' |> List.map (fun assignment -> int ((fst assignment).Split([|'^'|]).[1] )) |> Seq.ofList |> Seq.distinct |> List.ofSeq
    let varNum = (List.length time_ordered)/(List.length times)
    let rename (vs:string*int) =    let vn = (fst(vs)).Split([|'^'|]).[0]
                                    (int(vn),snd(vs))
    List.init (List.length times) (fun t -> List.init varNum (fun varid -> rename time_ordered.[varid+t*varNum]) )
    |> List.map (Map.ofList)

//Assumption- there is a state where time = t
let env_to_QNState_t t (env:Map<string,int>) =
    //Doing this as a list as we want to change the keys, which map.map doesn't let us do
    Map.toList env
                                |> List.filter (fun (varid,value) -> (sprintf "%d" t) = (varid.Split([|'^'|])).[1]  ) 
                                |> List.map (fun (varid,value) -> (int((varid.Split([|'^'|])).[0]),value)) 
                                |> Map.ofList

//Assumption- there is a state where time = 0
let env_to_QNState (env:Map<string,int>) = 
    env_to_QNState_t 0 env

let format_endComponent n = 
    List.mapi ( fun i state -> Map.fold (fun (acc:Map<string,int>) key value -> acc.Add((sprintf "%d^%d" key i),value) ) Map.empty state ) n
                                |> List.fold (fun acc state -> Map.fold (fun (a:Map<string,int>) k v -> a.Add(k,v) ) acc state ) Map.empty
                        

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

let convertMapToInt map =
    map |> Map.map(fun _ -> System.Int32.Parse)

let find_fixpoint (network : QN.node list) range =
    // time "0" is just an arbitrary time here.
    Z3Util.find_fixpoint (unroll_qn network range 0 0)
    |> Option.map convertMapToInt
    |> Option.map fixpoint_to_env


///////////////////////////////////////////////////////////////////////////////
// bifurcation
///////////////////////////////////////////////////////////////////////////////

let find_bifurcation (network : QN.node list) range =
    Z3Util.find_bifurcation (unroll_qn network range 0 0) 
    |> Option.map(fun (fix1, fix2) -> 
        (fix1 |> convertMapToInt |> fixpoint_to_env), (fix2 |> convertMapToInt |> fixpoint_to_env))


///////////////////////////////////////////////////////////////////////////////
// cycle
///////////////////////////////////////////////////////////////////////////////

let find_cycle (network: QN.node list) bounds length =
    Z3Util.find_cycle
        length
        (unroll_qn network bounds)
        (Z3Util.condition_states_equal network)
    |> Option.map convertMapToInt
    |> Option.map fixpoint_to_env


let find_cycle_steps network diameter bounds =

    // ...We want the first iteration of our loop to have k=2...
    let mutable k = 1
    let mutable cycle = None

    while bigint.Compare(diameter, bigint(k)) > 0 && cycle = None do
        k <- k*2
        Log.log_debug ( "find_cycle_steps " + (string)k )
        cycle <- find_cycle network bounds k

    cycle

// let assert_multivariable_change qn start_time end_time (z:Context) =
//     let mutable one_change = z.MkFalse()
//     let mutable this_change = z.MkTrue()

//     for node in qn do
//         let current_state_id = enc_z3_int_var_at_time node start_time
//         let current_state = make_z3_int_var current_state_id z

//         let next_state_id = enc_z3_int_var_at_time node end_time
//         let next_state = make_z3_int_var next_state_id z

//         this_change <- z.MkNot(z.MkEq(current_state,next_state))
//         //everything else stays the same
//         for node2 in qn do
//             if node2<>node then
//                 let current_state_id2 = enc_z3_int_var_at_time node start_time
//                 let current_state2 = make_z3_int_var current_state_id2 z

//                 let next_state_id2 = enc_z3_int_var_at_time node end_time
//                 let next_state2 = make_z3_int_var next_state_id2 z

//                 this_change <- z.MkAnd(this_change,z.MkEq(current_state2,next_state2))

//         one_change <- z.MkOr(one_change,this_change)
    
//     z.AssertCnstr (z.MkNot(one_change))

//Takes a set of paths and looks to see if their starts are diverted away
//from a endpoint in async space
//Assumes that the context initially passed has the entrypoint
//of the endpoint (cycle state or fixpoint) specified as state 0
//and that state -1 is not part of the endpoint
// let rec frustrated_endpoint network (ctx: Context) (t:int) =
//     //Add an extra constraint to limit the search to paths starting with multivariable transitions
//     assert_multivariable_change network t (t+1) ctx
//     let model = ref null
//     let sat = ctx.CheckAndGetModel (model)

//     // Did we find a transition?
//     if sat = LBool.True then
//         let state = fixpoint_to_env (model_to_fixpoint !model) |> env_to_QNState_t t
//         Log.log_debug (sprintf "Found some transitions %A" (fixpoint_to_env (model_to_fixpoint !model)) )
//         match (Simulate.dfsAsyncFixPoint network state) with
//         | Simulate.FixPoint(n) ->       Log.log_debug "Found a fix point- moving onto next"
//                                         assert_not_model !model ctx
//                                         if (!model) <> null then (!model).Dispose()
//                                         frustrated_endpoint network ctx t
//         | Simulate.EndComponent(n) ->   Log.log_debug "Found an end component"
//                                         if (!model) <> null then (!model).Dispose()
//                                         Some(format_endComponent n) 
//     else
//         Log.log_debug "Found no transitions"
//         if (!model) <> null then (!model).Dispose()
//         None

// let initiate_paths_to_fixpoint network bounds ctx =
//     // get some simulations
//     unroll_qn network bounds 0 0 ctx
//     unroll_qn network bounds -1 0 ctx
//     //assert that the state before the fixpoint is not a fixpoint
//     let not_fixpoint = ctx.MkNot (assert_states_equal network -1 0 ctx)
//     ctx.AssertCnstr not_fixpoint

// let initiate_paths_to_cycle states network bounds ctx =
//     assert_any_one_of_these_states states ctx 0
//     unroll_qn network bounds -1 0 ctx
//     assert_none_of_these_states states ctx -1

// Finds states that lead to a fixpoint in synchronous space
// but don't in asynchronous space (hence "frustrated")
// Works as follows
// Find states that transition to a fixpoint in sync space
// Test if they transition to a fixpoint in async space
// Returns either None or an endcomponent
// let find_frustrated_endpoints network bounds initiator =
//     let mutable endComponent = None
//     let cfg = new Config()
//     cfg.SetParamValue("MODEL", "true")
//     let ctx = new Context(cfg)
    
//     let rec find_path_to_fixpoint_of_length network bounds length (ctx:Context) =
//         //first check to see if theres an endcomponent in the last round of transitions
//         let init = 1 - length
//         ctx.Push()
//         Log.log_debug (sprintf "Looking for frustrated fp in paths from t=%d to t=0" init)
//         let res = frustrated_endpoint network ctx init
//         ctx.Pop()

//         match res with
//         | Some(n) -> Some(n)
//         | None ->   //No endcomponents found- add an extra step to the simulation
//                     Log.log_debug (sprintf "Found no fix points- looking for longer paths (length %d)" length)
//                     unroll_qn network bounds (init-1) init ctx
//                     //Do any new simulations of this length exist?
//                     let model = ref null
//                     let sat = ctx.CheckAndGetModel (model)
//                     if sat = LBool.True then 
//                         Log.log_debug "Found longer paths- continuing search"
//                         //Simulations exist- test them for frustrated fp and try increase 
//                         //path length
//                         if (!model) <> null then (!model).Dispose()
//                         find_path_to_fixpoint_of_length network bounds (length+1) ctx
//                     else 
//                         Log.log_debug "No longer simulations available- there are no frustrated fp"
//                         //No more simulations to be found
//                         if (!model) <> null then (!model).Dispose()
//                         None

//     initiator network bounds ctx 
//     //Could add an additional constraint that >1 variable must change

//     endComponent <- find_path_to_fixpoint_of_length network bounds 2 ctx
//     ctx.Dispose()
//     cfg.Dispose()
//     endComponent


// Finds a cycle and returns it. If no cycle is found returns None
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
let find_cycle_steps_optimized network bounds = 

    let rec find_cycle_of_length length (ctx : Context) (s : Solver) =      
        // add to solver the constraints to increase the path to 0..length:

        // Unroll the model (length/2)-times
        for time = (length/2) to (length-1) do
            unroll_qn network bounds (time) (time+1) ctx s

        // Check that the last step is different than the one before it (fix point would come up here)
        let last_identical = Z3Util.condition_states_equal network (length-1) length ctx
        let last_not_identical = ctx.MkNot(last_identical)
        s.Assert last_not_identical

        match s.Check() with
        // A path of the requested length does not exists in the model, may stop the earsch now
        | Status.UNSATISFIABLE ->                 
            None
        // A path of the requested length exists, Check for a cycle in the range: length/2..length  
        | Status.SATISFIABLE -> 

            s.Push()
           
            // Assert that we get a repetition (cycle) in the range : length/2..length  
            let mutable loop_condition = ctx.MkFalse()
            for time in [0..((length/2)-1)] do 
                let k_loop = Z3Util.condition_states_equal network time length ctx
                loop_condition <- ctx.MkOr(loop_condition, k_loop)
                
            s.Assert loop_condition

            // Now go find that cycle            
            let sat = s.Check()
            s.Pop()
        
            match sat with
            | Status.UNSATISFIABLE -> 
                find_cycle_of_length (length*2) ctx s
            | Status.SATISFIABLE -> 
                use model = s.Model
                // update cycle with the information from model
                let env = Z3Util.model_to_fixpoint model |> convertMapToInt |> fixpoint_to_env
                let smallenv = extract_cycle_from_model env
                Some smallenv
            | Status.UNKNOWN -> None
        | Status.UNKNOWN -> None
     
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    
    use ctx = new Context(cfg)
    use s = ctx.MkSolver()

    // Prepare the first step - cannot contain a loop yet, and a fix point will be found via states 0==2
    let length=1    
    
    unroll_qn network bounds 0 length ctx s 

    let cycle = find_cycle_of_length (length+1) ctx s
    cycle
