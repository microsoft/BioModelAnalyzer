(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Expr

open Util

/// An arithmetic expression.
type var = int
type expr =
    | Var of var
    | Const of int
    | Plus of expr * expr
    | Minus of expr * expr
    | Times of expr * expr
    | Div of expr * expr
    | Max of expr * expr
    | Min of expr * expr
    | Ceil of expr
    | Floor of expr
    | Ave of expr list
    | Sum of expr list  //QSW

let rec print_expr e =
    match e with
    | Var(v) -> printf "var(%d)" v
    | Const(i) -> printf "%d" i
    | Plus(e,f) -> printf "("; print_expr e; printf "+"; print_expr f; printf ")"
    | Minus(e,f) -> printf "("; print_expr e; printf "-"; print_expr f; printf ")"
    | Times(e,f) -> printf "("; print_expr e; printf "*"; print_expr f; printf ")";
    | Div(e,f) -> printf "("; print_expr e; printf "/"; print_expr f; printf ")";
    | Max(e,f) -> printf "max("; print_expr e; printf ","; print_expr f; printf ")";
    | Min(e,f) -> printf "min("; print_expr e; printf ","; print_expr f; printf ")";
    | Ceil(e) -> printf "ceil("; print_expr e; printf ")"
    | Floor(e) -> printf "floor("; print_expr e; printf ")"
    | Ave(ee) -> printf "ave(";
                 List.iter (fun e -> print_expr e; printf ",") ee
                 printf ")"
    | Sum(ee) -> printf "sum(";
                 List.iter (fun e -> print_expr e; printf ",") ee
                 printf ")" //QSW

let rec str_of_expr e =
    match e with
    | Var(v) -> "var(" + (string)v + ")"
    | Const(i) -> (string)i
    | Plus(e,f) -> "(" + str_of_expr e + "+" + str_of_expr f + ")"
    | Minus(e,f) -> "(" + str_of_expr e + "-" + str_of_expr f + ")"
    | Times(e,f) -> "(" + str_of_expr e +  "*" + str_of_expr f +   ")"
    | Div(e,f) -> "(" + str_of_expr e + "/" + str_of_expr f + ")"
    | Max(e,f) -> "max(" + str_of_expr e + "," + str_of_expr f + ")"
    | Min(e,f) -> "min(" + str_of_expr e +  "," + str_of_expr f + ")"
    | Ceil(e) -> "ceil(" + str_of_expr e + ")"
    | Floor(e) -> "floor(" + str_of_expr e + ")"
    | Ave(ee) ->  "ave(" + (String.concat "," (List.map (fun e -> str_of_expr e) ee)) + ")"
    | Sum(ee) -> "sum(" + (String.concat "," (List.map (fun e -> str_of_expr e) ee)) + ")" //QSW

let str_of_interval (lo:int) (hi:int) = "[" + (string)lo + "," + (string)hi + "]"
let str_of_env (env:Map<var,int>) = String.concat "," (Map.fold (fun ss x n -> (string)x+":"+(string)n :: ss) [] env)

/// FV
let rec fv e =
    match e with
    | Var(v) -> Set.singleton v
    | Const(_) -> Set.empty
    | Plus(e1,e2)
    | Minus(e1,e2)
    | Times(e1,e2)
    | Div(e1,e2)
    | Max(e1,e2)
    | Min(e1,e2) -> Set.union (fv e1) (fv e2)
    | Ceil(e)
    | Floor(e) -> fv e
    | Ave(ee) -> List.fold (fun ff e -> Set.union (fv e) ff) Set.empty ee
    | Sum(ee) -> List.fold (fun ff e -> Set.union (fv e) ff) Set.empty ee //QSW


/// is_a_const
let is_a_const range e =
    let rec is_a_const_int e =
        match e with
        | Var _ -> false
        | Const _ -> true
        | Plus(e1, e2) | Minus(e1, e2) | Times(e1, e2) | Div(e1, e2) | Max(e1, e2) | Min(e1, e2) ->
            (is_a_const_int e1) && (is_a_const_int e2)
        | Ceil(e') | Floor(e') -> (is_a_const_int e')
        | Ave(es) -> List.forall (is_a_const_int) es
        | Sum(es) -> List.forall is_a_const_int es 
    let is_a_const_var (range:Map<var,int*int>) e  =
        match e with
        | Var v ->
            let v_min,v_max = Map.find v range
            v_min = v_max
        | _ -> false
    is_a_const_var range e || is_a_const_int e

/// Evaluate an arithmetic expression at [node]
let rec eval_expr_int (node:var) (range:Map<var,int*int>) (e : expr) (env : Map<var, int>) =

    let node_min, node_max = Map.find node range

    let rec eval_expr_int e env =
        match e with
        | Var v ->
            // Adjust v so that it lies within node's range
            let v_min,v_max = Map.find v range
            // Special case to deal with constants
            if v_min = v_max then
                let c = v_min
                //Log.log_debug ("eval_expr var constant"+(string)c)
                if (c > node_max) then float(node_max)
                else if (node_max >= c && c >= node_min) then float(c)
                else if (c < node_min) then float(node_min)
                else failwith("bug in scaling constant")
            else
                let scale = (node_max - node_min) / (v_max - v_min)
                let displacement = node_min - v_min
                match Map.tryFind v env with
                | Some x ->
                    float( (x + displacement) * scale ) //float(x)
                | None ->
                    float(node_min) // 0.0

        | Const c -> float(c)
        | Plus(e1, e2) -> (eval_expr_int e1 env) + (eval_expr_int e2 env)
        | Minus(e1, e2) -> (eval_expr_int e1 env) - (eval_expr_int e2 env)
        | Times(e1, e2) -> (eval_expr_int e1 env) * (eval_expr_int e2 env)
        | Div(e1, e2) -> (eval_expr_int e1 env) / (eval_expr_int e2 env)
        | Max(e1, e2) -> max (eval_expr_int e1 env) (eval_expr_int e2 env)
        | Min(e1, e2) -> min (eval_expr_int e1 env) (eval_expr_int e2 env)
        | Ceil(e) -> ceil (eval_expr_int e env)
        | Floor(e) -> floor (eval_expr_int e env)
        | Ave([]) -> float(0)
        | Ave(es) ->
            let total = List.fold (fun x e -> x + (eval_expr_int e env)) 0.0 es
            let len = (List.length es)
            total / float(len)
        | Sum(es) ->
            let total = List.fold (fun x e -> x + (eval_expr_int e env)) 0.0 es
            total  //QSW


    // SI, Nir: Lucinda's Excel model takes the ceiling of the float computation.
    // We need to convert the float to the int. But via which function (ceil, floor, round)?
    // floor seems to force a fast stabilization to 0 for Lucinda's model.
    let convert = id // round, ceil???
    let res = int (convert (eval_expr_int e env))
    // Keep res in range
    let node_lo,node_hi = Map.find node range
    if res < node_lo then node_lo else
        if res > node_hi then node_hi else
            res

let eval_expr = eval_expr_int

/// Symbolically partially differentiate an expression with respect to
/// one of its free variables.  Returns a new expression, which should
/// probably be simplified before use.
let rec differentiate_expr (e : expr) x =
    match e with
        | Var v -> if v = x then Some(Const 1) else Some(Var v)
        | Const _ -> Some(Const 0)
        | Plus(e1, e2) ->
            let d1 = differentiate_expr e1 x;
            let d2 = differentiate_expr e2 x;

            match (d1, d2) with
            | (Some f1, Some f2) -> Some(Plus(f1, f2))
            | _ -> None
        | Minus(e1, e2) ->
            let d1 = differentiate_expr e1 x;
            let d2 = differentiate_expr e2 x

            match (d1, d2) with
            | (Some f1, Some f2) -> Some(Minus(f1, f2))
            | _ -> None
        | Times(e1, e2) ->
            let d1 = differentiate_expr e1 x;
            let d2 = differentiate_expr e2 x;

            match (d1, d2) with
            | (Some f1, Some f2) -> Some(Plus(Times(f1, e2), Times(e1, f2)))
            | _ -> None
        // Can't differentiate non-continuous functions like min, max...
        | _ -> None

/// Simplify an expression of the form e1 + e2 by doing constant propagation and applying the
/// following identities:
///    0 + x = x
///    x + 0 = x
let simplify_plus e1 e2 =
    match e1 with
        | Const 0 -> e2
        | Const c ->
            match e2 with
            | Const d -> Const (c + d)
            | _ -> Plus(e1, e2)
        | _ ->
            match e2 with
            | Const 0 -> e1
            | _ -> Plus(e1, e2)

/// Simplify an expression of the form e1 + e2 by doing constant propagation and applying the
/// following identity:
///    x - 0 = x
let simplify_minus e1 e2 =
    match e1 with
        | Const c ->
            match e2 with
            | Const d -> Const(c - d)
            | _ -> Minus(e1, e2)
        | _ ->
            match e2 with
            | Const 0 -> e1
            | _ -> Minus(e1, e2)

/// Simplify an expression of the form e1 * e2 by doing constant propagation and applying the
/// following identities:
///   0 * x = 0
///   x * 0 = 0
///   1 * x = x
///   x * 1 = x
let simplify_times e1 e2 =
    match e1 with
    | Const 0 -> Const 0
    | Const 1 -> e2
    | Const c ->
        match e2 with
        | Const d -> Const(c * d)
        | _ -> Times(e1, e2)
    | _ ->
        match e2 with
        | Const 0 -> Const 0
        | Const 1 -> e1
        | _ -> Times(e1, e2)

/// Do basic simplification of an arithmetic expression
let rec simplify_expr e =
    match e with
    | Plus(e1, e2) ->
        let s1 = simplify_expr e1
        let s2 = simplify_expr e2

        simplify_plus s1 s2
    | Minus(e1, e2) ->
        let s1 = simplify_expr e1
        let s2 = simplify_expr e2

        simplify_minus s1 s2
    | Times(e1, e2) ->
        let s1 = simplify_expr e1
        let s2 = simplify_expr e2

        simplify_times s1 s2
    | _ -> e

type sign = Pos | Neg | Zero | Unk

let sign_neg s =
    match s with
    | Pos -> Neg
    | Neg -> Pos
    | Zero -> Zero
    | Unk -> Unk

let sign_plus s1 s2 =
    if s1 = Pos && s2 = Pos then Pos
    else if s1 = Neg && s2 = Neg then Neg
    else if s1 = Zero then s2
    else if s2 = Zero then s1
    else Unk

let sign_minus s1 s2 =
    sign_plus s1 (sign_neg s2)

let npos s = if s = Pos then 1 else 0
let nneg s = if s = Neg then 1 else 0
let nzer s = if s = Zero then 1 else 0
let nunk s = if s = Unk then 1 else 0

let sign_times s1 s2 =
    let pos_cnt = (npos s1) + (npos s2)
    let neg_cnt = (nneg s1) + (nneg s2)
    let zer_cnt = (nzer s1) + (nzer s2)

    if pos_cnt = 2 || neg_cnt = 2 then Pos
    else if pos_cnt = 1 && neg_cnt = 1 then Neg
    else if zer_cnt > 0 then Zero
    else Unk

let sign_max s1 s2 =
    let pos_cnt = (npos s1) + (npos s2)
    let neg_cnt = (nneg s1) + (nneg s2)
    let zer_cnt = (nzer s1) + (nzer s2)

    if pos_cnt > 0 then Pos
    else if zer_cnt > 0 then Zero
    else if neg_cnt = 2 then Neg
    else Unk

let sign_min s1 s2 =
    let pos_cnt = (npos s1) + (npos s2)
    let neg_cnt = (nneg s1) + (nneg s2)
    let zer_cnt = (nzer s1) + (nzer s2)

    if neg_cnt > 0 then Neg
    else if zer_cnt > 0 then Zero
    else if pos_cnt = 2 then Pos
    else Unk


let rec sign_int f =
    match f with
    | Const(c) when c > 0 -> Pos
    | Const(c) when c < 0 -> Neg
    | Const(c) when c = 0 -> Zero
    | Const(c) -> Unk       // Just to keep the compiler happy... :-/
    | Var(_) -> Pos
    | Plus(e1, e2) ->
        sign_plus (sign e1) (sign e2)
    | Minus(e1, e2) ->
        sign_minus (sign e1) (sign e2)
    | Times(e1, e2)
    | Div(e1, e2) ->
        sign_times (sign e1) (sign e2)
    | Max(e1, e2) ->
        sign_max (sign e1) (sign e2)
    | Min(e1, e2) ->
        sign_min (sign e1) (sign e2)
    | Ceil(e) -> sign e
    | Floor(e) -> sign e
    | Ave(es) ->
        // Just treat this like a big sum, so add together all the signs.
        List.fold (fun s e -> sign_plus s (sign e)) Zero es
    | Sum(es) ->
        // This is the big Sum!
        List.fold (fun s e -> sign_plus s (sign e)) Zero es //QSW
and sign = memoize sign_int


let rec is_increasing_int f var =
    match f with
    | Const(_)
    | Var(_) -> true
    | Plus(e1, e2)
    | Min(e1, e2)
    | Max(e1, e2) -> (is_increasing e1 var) && (is_increasing e2 var)
    | Ceil(e)
    | Floor(e) -> is_increasing e var
    | Minus(e1, e2) -> (is_increasing e1 var) && (is_decreasing e2 var)
    | Times(e1, e2)
    | Div(e1, e2) ->
        let inc1 = if (sign e1) = Pos then (is_increasing e2 var)
                   else if (sign e1) = Neg then (is_decreasing e2 var)
                   else false
        let inc2 = if (sign e2) = Pos then (is_increasing e1 var)
                   else if (sign e2) = Neg then (is_decreasing e1 var)
                   else false
        inc1 && inc2
    | Ave(es) -> List.forall (fun e -> is_increasing e var) es
    | Sum(es) -> List.forall (fun e -> is_increasing e var) es //QSW

and is_decreasing_int f var =
    match f with
    | Const(_) -> true
    | Var(v) when v = var -> false
    | Var(_) -> true
    | Plus(e1, e2)
    | Min(e1, e2)
    | Max(e1, e2) -> (is_decreasing e1 var) && (is_decreasing e2 var)
    | Ceil(e)
    | Floor(e) -> is_decreasing e var
    | Minus(e1, e2) -> (is_decreasing e1 var) && (is_increasing e2 var)
    | Times(e1, e2)
    | Div(e1, e2) ->
        let dec1 = if (sign e1) = Pos then (is_decreasing e2 var)
                   else if (sign e1) = Neg then (is_increasing e2 var)
                   else false
        let dec2 = if (sign e2) = Pos then (is_decreasing e1 var)
                   else if (sign e2) = Neg then (is_increasing e1 var)
                   else false
        dec1 && dec2
    | Ave(es) -> List.forall (fun e -> is_decreasing e var) es
    | Sum(es) -> List.forall (fun e -> is_decreasing e var) es //QSW
and is_increasing = is_increasing_int
and is_decreasing = is_decreasing_int

let register_tests () =
    let f = Var(0)
    Test.register_test true (fun () -> is_increasing f 0)
    Test.register_test false (fun () -> is_decreasing f 0)
    Test.register_test true (fun () -> (sign f) = Pos)

    let f = Minus(Var(0), Var(1))
    Test.register_test true (fun () -> is_increasing f 0)
    Test.register_test false (fun () -> is_decreasing f 0)
    Test.register_test false (fun () -> is_increasing f 1)
    Test.register_test true (fun () -> is_decreasing f 1)
    Test.register_test true (fun () -> (sign f) = Unk)

