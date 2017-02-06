// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module internal Z3Util 

open Microsoft.Z3
open VariableEncoding

/// The Z3 bug causes extra consts to appear in the model and this function filters them out.
/// See http://stackoverflow.com/questions/10983067/z3-4-0-extra-output-in-model
let internal getConstDecls (model : Model) =
    model.ConstDecls
    |> Seq.filter(fun decl -> decl.Name.ToString().StartsWith("z3name!") |> not)

/// Return [model] constants as a Map<string,int>.
let internal model_to_fixpoint (model : Model) =
    let mutable fixpoint = Map.empty

    for var in getConstDecls(model) do
        let lhs = var.Name.ToString() 
        let expr = model.ConstInterp(var)
        let rhs = expr.ToString() 
        fixpoint <- fixpoint |> Map.add lhs rhs 

    fixpoint


let find_fixpoint (makeAssertions : Context -> Solver -> unit) =
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")

    use ctx = new Context(cfg)
    use s = ctx.MkSolver()

    makeAssertions ctx s

    // Did we find a fixpoint?
    match s.Check() with
    | Status.SATISFIABLE ->
        use model = s.Model                    
        Some(model_to_fixpoint (model))
    | _ ->
        None

let internal assert_not_model (model : Model) (z : Context) (s : Solver) =
    let mutable not_model = z.MkFalse()

    for decl in getConstDecls(model) do
        let lhs = z.MkConst decl
        let rhs = model.Eval(lhs)
        let new_val = z.MkNot (z.MkEq(lhs, rhs))
        not_model <- z.MkOr(not_model, new_val)

    Log.log_debug ("not_model: " + not_model.ToString())
    s.Assert not_model

let find_bifurcation (makeAssertions : Context -> Solver -> unit) =
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    
    use ctx = new Context(cfg)
    use s = ctx.MkSolver()
    
    makeAssertions ctx s

    // Did we find a fixpoint?
    match s.Check() with
    | Status.SATISFIABLE ->
        use model1 = s.Model
        assert_not_model model1 ctx s

        match s.Check() with
        | Status.SATISFIABLE ->
            use model2 = s.Model
            let fix1 = model_to_fixpoint model1
            let fix2 = model_to_fixpoint model2
            Some (fix1, fix2)
        | _ -> None
    | _ ->
        Log.log_debug "No initial fixpoint when looking for bifurcation"
        None

let condition_states_equal (qn : QN.node list) start_time end_time (ctx : Context) =
    let mutable equal_condition = ctx.MkTrue()

    for node in qn do
        let start_name = enc_z3_int_var_at_time node start_time
        let end_name = enc_z3_int_var_at_time node end_time
        let start_var = make_z3_int_var start_name ctx 
        let end_var = make_z3_int_var end_name ctx 
        let eq = ctx.MkEq(start_var, end_var)
        equal_condition <- ctx.MkAnd(equal_condition, eq)

    equal_condition

let find_cycle length (makeAssertions: (*startTime:*)int -> (*endTime:*)int -> Context -> Solver -> unit) (areStatesEqual : int -> int -> Context -> BoolExpr) =
    Log.log_debug ("Searching for a cycle of length " + (string)length)
    
    find_fixpoint(fun ctx s ->
        // Unroll the model k-times
        for time in [0..length] do
            makeAssertions time (time+1) ctx s

        // Assert that we get a repetition somewhere...
        let mutable loop_condition = ctx.MkFalse()
        for time in [2..length] do
            let k_loop = areStatesEqual 0 time ctx
            loop_condition <- ctx.MkOr(loop_condition, k_loop)

        s.Assert loop_condition

        // Assert that the start of the loop is _not_ a fixpoint
        // SI: don't we mean not (s_i = S_{i+1}) ?
        let not_fixpoint = ctx.MkNot (areStatesEqual 0 1 ctx)
        s.Assert not_fixpoint)
