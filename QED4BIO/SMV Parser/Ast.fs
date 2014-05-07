module Ast

type types = Set of string list
           | Module of string * (string list) list option 
           | Range of int64 * int64

type expr = 
    | Next of expr
    | Not of expr 
    | Ident of string
    | And of expr * expr
    | Or of expr * expr
    | Imp of expr * expr
    | Lt of expr * expr
    | Le of expr * expr
    | Ge of expr * expr
    | Gt of expr * expr
    | Eq of expr * expr
    | Neq of expr * expr 
    | Add of expr * expr 
    | Int of int64
    | Cases of (expr * expr) list

type assign = 
    | InitAssign of string * expr
    | NextAssign of string * expr

type section = 
    | Assigns of assign list
    | Init of expr
    | Trans of expr
    | Var of (string * types) list
    | Bounded of (string * types) list

type smv_module = 
    {name : string
     parameters : string list
     sections : section list
    }