// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module LTLParser

open System
open System.Collections.Generic
open FParsec

//
// Parsing
//

// Flattened to confuse expr/terms. 
type Prop = 
    | PUntil of Prop * Prop 
    | PWuntil of Prop * Prop
    | PRelease of Prop * Prop
    | PUpto of Prop * Prop
    | PAnd of Prop * Prop 
    | POr of Prop * Prop 
    | PImplies of Prop * Prop 
    | PNot of Prop 
    | PNext of Prop 
    | PAlways of Prop 
    | PEventually of Prop
    | PGt of Prop * Prop   // type-check that these are PGt(PId(x),PNum(n))
    | PGtEq of Prop * Prop //
    | PLt of Prop * Prop   //
    | PLtEq of Prop * Prop //
    | PEq of Prop * Prop
    | PNeq of Prop * Prop
    | PLoop
    | PSelfLoop
    | POscillation
    | PFalse
    | PTrue
    | PNum of int
    | PId of string

// Space/string parsing. 
let ws = spaces // skips any whitespace 

let ch c = skipChar c >>. ws
let str s = skipString s >>. ws 

// Declare parser. 
let opp = new OperatorPrecedenceParser<Prop,unit,unit>() 
let expr = opp.ExpressionParser 

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
let number = numberLit .>> ws |>> (fun x -> PNum((int32)x.String))
let ident  = str "var" >>. str "(" >>. pythonIdentifier .>> str ")" |>> (fun x -> PId(x))
let (ll : Parser<Prop,unit>) = str "Loop" .>> ws |>> (fun _x -> PLoop)
let (sl : Parser<Prop,unit>) = str "SelfLoop" .>> ws |>> (fun _x -> PSelfLoop)
let (os : Parser<Prop,unit>) = str "Oscillation" .>> ws |>> (fun _x -> POscillation)
let tt = str "True" .>> ws |>> (fun _x -> PTrue)
let ff = str "False" .>> ws |>> (fun _x -> PFalse)
let paren = between (str "(") (str ")") expr 
opp.TermParser <- choice [tt; ff; number; ident; paren]

// Operators. 
type Assoc = Associativity
opp.AddOperator(InfixOperator("until",   ws, 1, Assoc.Left, fun x y -> PUntil(x,y)))
opp.AddOperator(InfixOperator("weakuntil",   ws, 1, Assoc.Left, fun x y -> PWuntil(x,y)))
opp.AddOperator(InfixOperator("release", ws, 1, Assoc.Left, fun x y -> PRelease(x,y)))
opp.AddOperator(InfixOperator("upto", ws, 1, Assoc.Left, fun x y -> PUpto(x,y)))
opp.AddOperator(PrefixOperator("next",   ws, 1, true, fun x -> PNext(x)))
opp.AddOperator(PrefixOperator("always", ws, 1, true, fun x -> PAlways(x)))
opp.AddOperator(PrefixOperator("eventually", ws, 1, true, fun x -> PEventually(x)))
opp.AddOperator(InfixOperator("&&", ws, 2, Assoc.Left, fun x y -> PAnd(x,y)))
opp.AddOperator(InfixOperator("||", ws, 2, Assoc.Left, fun x y -> POr(x,y)))
opp.AddOperator(InfixOperator("=>", ws, 2, Assoc.Left, fun x y -> PImplies(x,y)))
opp.AddOperator(PrefixOperator("!", ws, 4, true, fun x -> PNot(x)))
opp.AddOperator(InfixOperator("=",ws,8,Assoc.Left, fun x y -> PEq(x,y)))
opp.AddOperator(InfixOperator("!=", ws, 8, Assoc.Left, fun x y -> PNeq(x,y)))
opp.AddOperator(InfixOperator(">", ws, 8, Assoc.Left, fun x y -> PGt(x,y)))
opp.AddOperator(InfixOperator(">=", ws, 8, Assoc.Left, fun x y -> PGtEq(x,y)))
opp.AddOperator(InfixOperator("<", ws, 8, Assoc.Left, fun x y -> PLt(x,y)))
opp.AddOperator(InfixOperator("<=", ws, 8, Assoc.Left, fun x y -> PLtEq(x,y)))

let completeProp = ws >>. expr .>> eof



//
// Translate to LTL.LTLFormulaType
//
let find_node_by_name qn name = failwith "Unimplemented"

let rec ltl_of_p qn p loc = 
    match p with 
    | PUntil(p0,p1) -> LTL.Until(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PWuntil(p0,p1) -> LTL.Wuntil(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PRelease(p0,p1) -> LTL.Release(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PUpto(p0,p1) -> LTL.Upto(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PAnd(p0,p1) -> LTL.And(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | POr(p0,p1) -> LTL.Or(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PImplies(p0,p1) -> LTL.Implies(loc, ltl_of_p qn p0 (0::loc), ltl_of_p qn p1 (1::loc))
    | PNot(p0) -> LTL.Not(loc, ltl_of_p qn p0 (0::loc))
    | PNext(p0) -> LTL.Next(loc, ltl_of_p qn p0 (0::loc))
    | PAlways(p0) -> LTL.Always(loc, ltl_of_p qn p0 (0::loc))
    | PEventually(p0) -> LTL.Eventually(loc, ltl_of_p qn p0 (0::loc))
    | PEq (PId(x), PNum(n)) -> LTL.PropEq (loc, find_node_by_name qn x, n)
    | PEq (_,_) -> LTL.Error
    | PNeq (PId(x), PNum(n)) -> LTL.PropNeq (loc, find_node_by_name qn x, n)
    | PNeq (_,_) -> LTL.Error
    | PGt(PId(x),PNum(n)) -> LTL.PropGt(loc, find_node_by_name qn x, n)
    | PGt(_,_) -> LTL.Error
    | PGtEq(PId(x),PNum(n)) -> LTL.PropGtEq(loc, find_node_by_name qn x, n)
    | PGtEq(_,_) -> LTL.Error
    | PLt(PId(x),PNum(n)) -> LTL.PropLt(loc, find_node_by_name qn x, n)
    | PLt(_,_) -> LTL.Error
    | PLtEq(PId(x),PNum(n)) -> LTL.PropLtEq(loc, find_node_by_name qn x, n)
    | PLtEq(_,_) -> LTL.Error
    | PLoop -> LTL.Loop
    | PSelfLoop -> LTL.SelfLoop
    | POscillation -> LTL.Oscillation
    | PFalse -> LTL.False
    | PTrue -> LTL.True
    | PNum(_) 
    | PId(_) -> LTL.Error
    
let ltl_of_prop qn p = ltl_of_p qn p []


//
// Parser Tests
//
type perr = { line : int; col : int; msg: string } 
type parse_result = ParseOK of Prop | ParseErr of perr

let check_syntax (s:string) = 
    match FParsec.CharParsers.run completeProp s with
    | FParsec.CharParsers.Success(f,_,pos) -> ParseOK(f)
    | FParsec.CharParsers.Failure(msg,err,_) -> 
        let l,c = (int)err.Position.Line, (int)err.Position.Column
        ParseErr({line= l; col= c; msg= msg})

// An example use of check_syntax: unit testing the parser. 
let check (s:string) (expected:parse_result) : unit = 
    match (check_syntax s), expected with
    | ParseOK(f), ParseOK(f') -> 
        if f = f' then Printf.printf "OK.\n" 
        else Printf.printf "Not OK, got a %A instead of a %A\n" f' f 
    | ParseErr({line=line; col=col; msg=msg}), ParseErr(_) -> 
        Printf.printf "Parse failed as expected, at line %d, col %d, msg=%s.\n" line col msg
    | _ -> Printf.printf "!!Didn't get what I expected!\n"

let unit_tests _ = 
    let a_err = ParseErr({line=0; col=0; msg=""})
    check   "((~ (var(v1) > 5)) until (var(v2) > 6))" 
            (ParseOK(PUntil (PNot (PGt (PId("x1"),PNum(5))),PGt (PId("v2"),PNum(6)))))

    check   "((!(var(v1) > 5)) release (next (var(v2) >= 17))) until ((var(v2) > 6) && (var(v2) <= 564))" 
            (ParseOK(PUntil (PRelease (PNot (PGt (PId "v1",PNum 5)),PNext (PGtEq (PId "v2",PNum 17))),PAnd (PGt (PId "v2",PNum 6),PLtEq (PId "v2",PNum 564)))))

    check   "((!(var(v1) = 5)) release (next (var(v2) >= 17))) until ((var(v2) != 6) && (var(v2) <= 564))" 
            (ParseOK(PUntil (PRelease (PNot (PEq (PId "v1",PNum 5)),PNext (PGtEq (PId "v2",PNum 17))),PAnd (PNeq (PId "v2",PNum 6),PLtEq (PId "v2",PNum 564)))))

    check   "((!(var(v1) > 5)) release (next (var(v2) >= 17))) weakuntil ((var(v2) > 6) && (var(v2) <= 564))" 
            (ParseOK(PWuntil (PRelease (PNot (PGt (PId "v1",PNum 5)),PNext (PGtEq (PId "v2",PNum 17))),PAnd (PGt (PId "v2",PNum 6),PLtEq (PId "v2",PNum 564)))))

    check   "((!(var(v1) > 5)) upto (next (var(v2) >= 17))) weakuntil ((var(v2) > 6) && (var(v2) <= 564))" 
            (ParseOK(PWuntil (PUpto (PNot (PGt (PId "v1",PNum 5)),PNext (PGtEq (PId "v2",PNum 17))),PAnd (PGt (PId "v2",PNum 6),PLtEq (PId "v2",PNum 564)))))

    check   "((!(var(v1) > 5)) upto (next Oscillation)) weakuntil (Loop && SelfLoop)" 
            (ParseOK(PWuntil (PUpto (PNot (PGt (PId "v1",PNum 5)),PNext (POscillation)),PAnd (PLoop,PSelfLoop))))

//    let formula_three_string = "(Always (Or (Not (Next (>= v2 6344))) (Eventually (Next (<= v3 343245)))))"
//    let formula_three = string_to_LTL_formula formula_three_string network
//    // This is formula three without some close parenthesis
//    let formula_four_string = "(Always (Or (Not (Next (>= v1 6344))) (Eventually (Next (<= v3 343245))))"
//    let formula_four = string_to_LTL_formula formula_four_string network
//    // This is formula three without some open parenthesis
//    let formula_five_string = "(Always (Or (Not (Next >= v1 6344))) (Eventually (Next (<= v1 343245)))))"
//    let formula_five = string_to_LTL_formula formula_five_string network
