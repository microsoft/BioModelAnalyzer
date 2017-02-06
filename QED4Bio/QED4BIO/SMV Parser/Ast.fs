// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Ast

type types = 
   | Set of string list
   | Module of string * (string list) list option 
   | Range of int64 * int64
    override this.ToString() =
        match this with
        | Set sl -> "{" + (String.concat ", " sl) + "}"
        | Module (name,None) -> name
        | Module (name, Some args) -> name + "(" + (String.concat ", " (List.map (String.concat ".") args)) + ")"
        | Range (lb,ub) -> lb.ToString() + ".." + ub.ToString()

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
    override this.ToString() =
        match this with
        | Next e -> "next(" + (e.ToString()) + ")"
        | Not e -> "!" + e.ToString()
        | Ident e -> e
        | And (e1,e2) -> "(" + e1.ToString() + " & "  + e2.ToString() + ")"
        | Or  (e1,e2) -> "(" + e1.ToString() + " | "  + e2.ToString() + ")"
        | Imp (e1,e2) -> "(" + e1.ToString() + " -> " + e2.ToString() + ")"
        | Lt  (e1,e2) -> "(" + e1.ToString() + " < "  + e2.ToString() + ")"
        | Le  (e1,e2) -> "(" + e1.ToString() + " <= " + e2.ToString() + ")"
        | Ge  (e1,e2) -> "(" + e1.ToString() + " >= " + e2.ToString() + ")"
        | Gt  (e1,e2) -> "(" + e1.ToString() + " > "  + e2.ToString() + ")"
        | Eq  (e1,e2) -> "(" + e1.ToString() + " = "  + e2.ToString() + ")"
        | Neq (e1,e2) -> "(" + e1.ToString() + " != " + e2.ToString() + ")"
        | Add (e1,e2) -> "(" + e1.ToString() + " + "  + e2.ToString() + ")"
        | Int i -> i.ToString()
        | Cases (es) -> 
            "\n\t\tcase\n\t\t\t"
            + String.concat "\n\t\t\t" (List.map (fun (e1,e2) -> e1.ToString() + ":" + e2.ToString() + ";") es)
            + "\n\t\t\tesac\n" 

type assign = 
    | InitAssign of string * expr
    | NextAssign of string * expr
    override this.ToString() = 
       match this with 
       | InitAssign (s,e) ->  "init(" + s + ") := " + e.ToString() + ";\n"
       | NextAssign (s,e) ->  "next(" + s + ") := " + e.ToString() + ";\n"

type section = 
    | Assigns of assign list
    | Init of expr
    | Trans of expr
    | Var of (string * types) list
  //  | Bounded of (string * types) list
    override this.ToString() = 
       match this with 
       | Assigns al -> "ASSIGN\n\t" + (String.concat "\t" (List.map (string) al)) + "\n\n"
       | Init e -> "INIT\n\t" + e.ToString() + ";\n\n"
       | Trans e -> "TRANS\n\t" + e.ToString() + ";\n\n"
       | Var vdl -> "VAR\n\t" + (String.concat "\t" (List.map (fun (v,t) -> v + " : " + t.ToString() + ";\n") vdl)) + "\n"
     //  | Bounded vdl -> "BOUNDED\n\t" + (String.concat "\t" (List.map (fun (v,t) -> v + " : " + t.ToString() + ";\n") vdl)) + "\n"


type smv_module = 
    {name : string
     parameters : string list
     sections : section list
    }
    override this.ToString() =
        "MODULE " + this.name + (match this.parameters with [] -> "" | _ -> "(" + (String.concat ", " (List.map string this.parameters))  + ")" )
        + "\n\n\n" + (String.concat "\n\n\n" (List.map string this.sections))

let mkModuleType (n, argls : System.Collections.Generic.List<System.Collections.Generic.List<string>>) : (string * (string list list option)) = 
    (n, Some (Seq.toList (Seq.map (Seq.toList) argls)))
