// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

//Begin <-- Added by Qinsi Wang

module stepZ3rangelist

// Z3 related stuff.
// the input rangelist is a list 
// Also (the difference between this and Z3rangelist)
// for each step, just push the transition constraints 
// related to the inputs of one variable.

open Microsoft.Z3
open VariableEncoding

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
               let t = z.MkReal(node_max - node_min)
               let b = z.MkReal(v_max - v_min)
               (z.MkDiv(t,b) , z.MkReal( (node_min - v_min):int ))
            else (z.MkReal 1, z.MkReal 0)

        let input_var = 
            let v_t = BioCheckZ3.get_z3_int_var_at_time v_defn time
            z.MkToReal(z.MkConst(z.MkSymbol v_t, z.MkIntSort()))
        z.MkMul(z.MkAdd(input_var,displacement), scale)

    | Const c -> z.MkReal c
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
        let half = z.MkReal "99 / 100"
        let z1_half = z.MkAdd(half, z1)
        z.MkToReal (z.MkReal2Int z1_half )
        
    | Floor e1 ->
        let z1 = expr_to_z3 qn node e1 var_names time z
        z.MkToReal (z.MkReal2Int z1)
    | Ave es ->
        let sum = List.fold
                    (fun ast e1 -> match ast with
                                   | None -> Some(expr_to_z3 qn node e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 qn node e1 var_names time z
                                                Some(z.MkAdd(z0, z1)))
                    None
                    es
        let cnt = z.MkReal (List.length es)

        match sum with
        | None -> z.MkReal 0
        | Some s -> z.MkDiv(s, cnt)
    | Sum es ->
        let sum = List.fold
                    (fun ast e1 -> match ast with
                                   | None -> Some(expr_to_z3 qn node e1 var_names time z)
                                   | Some z0 -> let z1 = expr_to_z3 qn node e1 var_names time z
                                                Some(z.MkAdd(z0, z1)))
                    None
                    es
        let cnt = z.MkReal 1

        match sum with
        | None -> z.MkReal 0
        | Some s -> z.MkDiv(s, cnt)
        *)

let assert_target_function (qn:QN.node list) (node: QN.node) var_names (rangelist : Map<QN.var,int list>) start_time end_time (z : Context) (s : Solver) =

    let current_state_id = enc_z3_int_var_at_time node start_time
    let current_state = make_z3_int_var current_state_id z

    let next_state_id = enc_z3_int_var_at_time node end_time
    let next_state = make_z3_int_var next_state_id z

    let z3_target_function = BioCheckZ3.expr_to_z3 qn node node.f start_time z
    // let T_applied = z.MkReal2Int(z3_target_function)    
    let tf_plus_half = z.MkAdd(z.MkReal(1,2),z3_target_function) :?> RealExpr
    let T_applied = z.MkReal2Int (tf_plus_half)

    let list = Map.find node.var rangelist
    let lower = List.min list
    let upper = List.max list
     
    // If the next value is greater than current value and current value is not
    // max the value needs to increase
    let up = z.MkEq(next_state, z.MkAdd(current_state, z.MkInt 1))
    let up = z.MkAnd(up, z.MkGt(T_applied, current_state))
    let up = z.MkAnd(up, z.MkGt(z.MkInt upper, current_state))

    // If the next value is the same as current value of the next value is 
    // greater than the current value but current value is max or
    // the next value is smaller than the current value but current value is min
    // the value needs to stay the same
    let same = z.MkEq(next_state, current_state)
    let tmpsame = z.MkEq(T_applied, current_state)
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkGt(T_applied, current_state), z.MkEq(z.MkInt upper, current_state)))
    let tmpsame = z.MkOr(tmpsame, z.MkAnd(z.MkLt(T_applied, current_state), z.MkEq(z.MkInt lower, current_state)))
    let same = z.MkAnd(same, tmpsame)

    let dn = z.MkEq(next_state, z.MkSub(current_state, z.MkInt 1))
    let dn = z.MkAnd(dn, z.MkLt(T_applied, current_state))
    let dn = z.MkAnd(dn, z.MkLt(z.MkInt lower, current_state))

    let cnstr = z.MkOr([|up;same;dn|])
    Log.log_debug ("Apply TF of " + node.name + " for step " + string(start_time) + " to " + string(end_time) + ":" + (cnstr.ToString()))
    s.Assert cnstr
    
let assert_query (node : QN.node) (value : int) time (z : Context) (s : Solver) = 
    let var_name = enc_z3_int_var_at_time node time
    let v = make_z3_int_var var_name z
    let query = z.MkEq (v, z.MkInt value)
    s.Assert query

let find_paths (network : QN.node list) step rangelist orbounds=
    let updatePossibleValues (node : QN.node) (ctx : Context, s : Solver) = function
        | [] -> failwith "There are no possible values"
        | [v] -> [v]
        | values ->
            values 
            |> List.choose(fun i ->
                s.Push()
                assert_query (node : QN.node) (i : int) (step+1) ctx s

                // SI: cons the i in all cases but false (UNSATISFIABLE)
                match s.Check() with
                | Status.UNSATISFIABLE -> 
                    Log.log_debug(sprintf "unsat")
                    s.Pop()
                    None
                | Status.SATISFIABLE ->
                    s.Pop()
                    Some i
                | Status.UNKNOWN ->
                    Log.log_debug (sprintf "z3 returned unknown - %s" s.ReasonUnknown)
                    s.Pop()
                    Some i
                    // Todo:
                    // Implement a model checker that evaluates the 
                    // value of the formula under the model 
                    // actually evaluates the formula to true
                | _ -> 
                    failwith "Unexpected check response")
            |> List.sort

    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    use ctx = new Context(cfg)
        
    network
    |> Seq.fold(fun nubounds node ->
        // For each action step, we only handle one variable and its inputs.
        use s = ctx.MkSolver()
            
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
            BioCheckZ3.assert_bound n (nMin , nMax) step ctx s

        // Then, push related transition constraints
        assert_target_function network node varnamenode orbounds step (step+1) ctx s

        // Then, we can begin to query about this node/variable
        let currentPossibleValues = Map.find node.var rangelist                   
        let newPossibleValues = currentPossibleValues |> updatePossibleValues node (ctx, s)                   
        nubounds |> Map.add node.var newPossibleValues) Map.empty
//End <-- For each time step, encode the system (model) in SMT format, and push into Z3 to compute possible values of each variables



// SI: rewritten find_paths to remove redundant code and to be more functional. 

let input_nodes (qn:QN.node list) (node:QN.node) = 
            List.concat
                [ for var in node.inputs do
                    yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) qn) ]

let check_ok (s:Solver) = 
    match s.Check() with
    | Status.SATISFIABLE
    | Status.UNKNOWN -> true
    | _ -> false
       
let reachability (qn:QN.node list) step range orig_range =
    // Z3 ctx management
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    use ctx = new Context(cfg)
    use s = ctx.MkSolver()
    s.Push()    
    
    let next_values (node : QN.node) vv =
        match vv with
        | [] -> failwith "values empty" 
        | [_] -> vv
        | _ -> 
            let vv' = List.fold 
                        (fun vv' v ->                     
                            s.Push()
                            assert_query node v (step+1) ctx s
                            let vv' = if check_ok s then v :: vv' else vv'
                            s.Pop()         
                            vv')
                        []
                        vv
            List.sort vv'

    let bounds' = 
        List.fold 
            (fun bounds' (node:QN.node) -> 
                s.Push()
                let nodes = node :: (input_nodes qn node)
                // Assert current range bounds 
                for n in nodes do
                    let range_n = Map.find n.var range
                    let min_n, max_n = List.min range_n, List.max range_n 
                    BioCheckZ3.assert_bound n (min_n,max_n) step ctx s
                // Assert transition constraints
                let var_to_name = BioCheckZ3.build_var_name_map nodes
                assert_target_function qn node var_to_name orig_range step (step+1) ctx s
                // Assert next range 
                let current_values = Map.find node.var range
                let bounds' = Map.add node.var (next_values node current_values) bounds'
                s.Pop()
                bounds' )
            Map.empty
            qn
    // result
    bounds'







