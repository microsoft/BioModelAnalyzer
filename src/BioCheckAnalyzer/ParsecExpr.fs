// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module ParsecExpr

open System
open System.Collections.Generic

// We're using fparsec #92e73db8cdff. 
open FParsec

(* 
This is the grammar we want to parse:


expr  : NUM                   
      | 'var' '(' NUM ')'
      | expr '+' expr | expr '-' expr  | expr '*' expr | expr '/' expr         
      | 'max' '(' expr ',' expr ')' | 'min' '(' expr ',' expr ')'
      | 'ceil' '(' expr ')' | 'floor' '(' expr ')'
      | 'avg' '(' expr_list ')'
      | '(' expr ')'
      ;

expr_list : expr 
          | expr_list ',' expr 
		  ;
*)

// Declare parser. 
let opp = new OperatorPrecedenceParser<Expr.expr,unit,unit>() 
let expr = opp.ExpressionParser 

// Space/string parsing. 
let ws = spaces // skips any whitespace 

let ch c = skipChar c >>. ws
let str s = skipString s >>. ws 

// Number parsing. 
let numberFormat = NumberLiteralOptions.AllowMinusSign
let numberLit = numberLiteral numberFormat "number" .>> ws

// "Grammar productions". 
let number = numberLit .>> ws |>> (fun x -> Expr.Const((int32)x.String))
let constant = str "const" .>> str "(" >>. numberLit .>> str ")" |>> (fun x -> Expr.Const((int32)x.String))
let ident  = str "var" >>. str "(" >>. numberLit .>> str ")" |>> (fun x -> Expr.Var((int32)x.String))
let max_expr  = (str "max" >>. str "(" >>. expr) .>>. (str "," >>. expr .>> str ")") |>> (fun e -> Expr.Max(e))
let min_expr  = (str "min" >>. str "(" >>. expr) .>>. (str "," >>. expr .>> str ")") |>> (fun e -> Expr.Min(e))
let ceil_expr  = str "ceil" >>. str "(" >>. expr .>> str ")" |>> Expr.Ceil
let floor_expr = str "floor" >>. str "(" >>. expr .>> str ")" |>> Expr.Floor
let abs_expr = str "abs" >>. str "(" >>. expr .>> str ")" |>> Expr.Abs
let avg_expr = str "avg" >>. str "(" >>. (sepBy expr (ch ',')) .>> str ")" |>> Expr.Ave

opp.TermParser <- choice [number; constant; ident; max_expr; min_expr; ceil_expr; abs_expr; floor_expr; avg_expr; between (ch '(') (ch ')') expr] 

// Operators. 
type Assoc = Associativity
opp.AddOperator(InfixOperator("+", ws, 1, Assoc.Left, fun x y -> Expr.Plus(x,y)))
opp.AddOperator(InfixOperator("-", ws, 1, Assoc.Left, fun x y -> Expr.Minus(x,y)))
opp.AddOperator(InfixOperator("*", ws, 2, Assoc.Left, fun x y -> Expr.Times(x,y)))
opp.AddOperator(InfixOperator("/", ws, 2, Assoc.Left, fun x y -> Expr.Div(x,y)))

// SI: do we need this?
// let completeExpression = ws >>. expr .>> eof


// The main exported function of this module is [parse_expr]. It returns a 
// ParseOK on successfully parsing [s]. Otherwise it returns a ParseErr. 
type perr = { line : int; col : int; msg: string } 
type parse_result = ParseOK of Expr.expr | ParseErr of perr

let parse_expr (s:string) = 
    match CharParsers.run expr s with
    | CharParsers.Success(f,_,pos) -> ParseOK(f)
    | CharParsers.Failure(msg,err,_) -> 
        let l,c = (int)err.Position.Line, (int)err.Position.Column
        ParseErr({line= l; col= c; msg= msg})

