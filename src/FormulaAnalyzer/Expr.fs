// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Expr

type expr =
    | Ident of string
    | Const of int
    | Pos 
    | Neg 
    | Plus of expr * expr
    | Minus of expr * expr
    | Times of expr * expr
    | Div of expr * expr
    | Max of expr * expr
    | Min of expr * expr
    | Ceil of expr
    | Floor of expr
    | Avg of expr list


let rec str_of_expr e = 
    match e with 
    | Ident(s) -> "Ident(" + s + ")"
    | Const(i) -> "const(" + (string)i + ")"
    | Pos -> "pos"
    | Neg -> "neg" 
    | Plus(e,f) -> "(" + str_of_expr e + "+" + str_of_expr f + ")"
    | Minus(e,f) -> "(" + str_of_expr e + "-" + str_of_expr f + ")"
    | Times(e,f) -> "(" + str_of_expr e +  "*" + str_of_expr f +   ")"
    | Div(e,f) -> "(" + str_of_expr e + "/" + str_of_expr f + ")"
    | Max(e,f) -> "max(" + str_of_expr e + "," + str_of_expr f + ")"
    | Min(e,f) -> "min(" + str_of_expr e +  "," + str_of_expr f + ")"
    | Ceil(e) -> "ceil(" + str_of_expr e + ")"
    | Floor(e) -> "floor(" + str_of_expr e + ")"
    | Avg(ee) ->  "avg(" + (String.concat "," (List.map (fun e -> str_of_expr e) ee)) + ")"

