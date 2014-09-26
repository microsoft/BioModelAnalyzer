module ExprParser

open System
open System.Collections.Generic

// We're using fparsec #92e73db8cdff. 
open FParsec

(* 
This is the grammar we want to parse:

expr  : NUM                   
      | 'var' '(' ident ')'
      | expr '+' expr | expr '-' expr  | expr '*' expr | expr '/' expr         
      | 'max' '(' expr ',' expr ')' | 'min' '(' expr ',' expr ')'
      | 'ceil' '(' expr ')' | 'floor' '(' expr ')'
      | 'avg' '(' POS ')' '-' 'avg' '(' 'neg' ')'
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

// Ident parsing, from the FParsec reference manual. 
let pythonIdentifier =  
   let isAsciiIdStart    = fun c -> isAsciiLetter c || c = '_'
   let isAsciiIdContinue = fun c -> isAsciiLetter c || isDigit c || c = '_' || c = '.' || c = '-'
   identifier (IdentifierOptions(
                    isAsciiIdStart = isAsciiIdStart,
                    isAsciiIdContinue = isAsciiIdContinue,
                    normalization = System.Text.NormalizationForm.FormKC,
                    normalizeBeforeValidation = true,
                    allowAllNonAsciiCharsInPreCheck = true))

// Number parsing. 
let numberFormat = NumberLiteralOptions.AllowMinusSign
let numberLit = numberLiteral numberFormat "number" .>> ws

// "Grammar productions". 
let number = numberLit .>> ws |>> (fun x -> Expr.Const((int32)x.String))
let ident  = str "var" >>. str "(" >>. pythonIdentifier .>> str ")" |>> Expr.Ident
let pos = stringReturn "pos" Expr.Pos
let neg = stringReturn "neg" Expr.Neg
let max_expr  = (str "max" >>. str "(" >>. expr) .>>. (str "," >>. expr .>> str ")") |>> (fun e -> Expr.Max(e))
let min_expr  = (str "min" >>. str "(" >>. expr) .>>. (str "," >>. expr .>> str ")") |>> (fun e -> Expr.Min(e))
let ceil_expr  = str "ceil" >>. str "(" >>. expr .>> str ")" |>> Expr.Ceil
let floor_expr = str "floor" >>. str "(" >>. expr .>> str ")" |>> Expr.Floor
let avg_expr = str "avg" >>. str "(" >>. (sepBy expr (ch ',')) .>> str ")" |>> Expr.Avg

opp.TermParser <- choice [number; pos; neg; ident; max_expr; min_expr; ceil_expr; floor_expr; avg_expr; between (ch '(') (ch ')') expr] 

// Operators. 
type Assoc = Associativity
opp.AddOperator(InfixOperator("+", ws, 1, Assoc.Left, fun x y -> Expr.Plus(x,y)))
opp.AddOperator(InfixOperator("-", ws, 1, Assoc.Left, fun x y -> Expr.Minus(x,y)))
opp.AddOperator(InfixOperator("*", ws, 2, Assoc.Left, fun x y -> Expr.Times(x,y)))
opp.AddOperator(InfixOperator("/", ws, 2, Assoc.Left, fun x y -> Expr.Div(x,y)))

// SI: do we need this?
let completeExpression = ws >>. expr .>> eof

