// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module LTL

// New operators: UpTo 
// New state formulas: cycle, fixpoint

type LTLFormulaType = 
    // Each operator has a list of booleans memorizing its location
    // in the parse tree
    | Until of int list * LTLFormulaType * LTLFormulaType 
    | Wuntil of int list * LTLFormulaType * LTLFormulaType
    | Upto of int list * LTLFormulaType * LTLFormulaType
    | Release of int list * LTLFormulaType * LTLFormulaType
    | And of int list * LTLFormulaType * LTLFormulaType
    | Or of int list * LTLFormulaType * LTLFormulaType
    | Implies of int list * LTLFormulaType * LTLFormulaType
    | Not of int list * LTLFormulaType
    | Next of int list * LTLFormulaType
    | Always of int list * LTLFormulaType
    | Eventually of int list * LTLFormulaType
    | PropEq of int list * QN.node * int
    | PropNeq of int list * QN.node * int
    | PropGt of int list * QN.node * int 
    | PropGtEq of int list * QN.node * int
    | PropLt of int list * QN.node * int
    | PropLtEq of int list * QN.node * int
    | SelfLoop
    | Loop
    | Oscillation
    | False 
    | True
    | Error 

let left_formula phi =
    match phi with 
    | Until(_,phi0,_)
    | Wuntil (_,phi0,_)
    | Release(_,phi0,_)
    | Upto(_,phi0,_)
    | And(_,phi0,_) 
    | Or(_,phi0,_) 
    | Implies(_,phi0,_) 
    | Not(_,phi0)
    | Next(_,phi0)
    | Always(_,phi0)
    | Eventually(_,phi0) -> Some phi0
    | _ -> None

let right_formula phi = 
    match phi with 
    | Until (_,_,phi1)
    | Wuntil (_,_,phi1)
    | Release (_,_,phi1)
    | Upto (_,_,phi1)
    | And (_,_,phi1) 
    | Or (_,_,phi1) 
    | Implies (_,_,phi1) -> Some phi1
    | _ -> None

let formula_location phi = 
    match phi with
    | Until (loc, _, _)
    | Wuntil (loc, _,_)
    | Release (loc, _, _)
    | Upto (loc, _, _)
    | Next (loc, _) 
    | Always (loc, _) 
    | Eventually (loc, _) 
    | And (loc, _, _) 
    | Or (loc, _, _) 
    | Implies (loc, _, _) 
    | Not (loc, _) 
    | PropEq (loc, _, _)
    | PropNeq (loc, _, _)
    | PropGt (loc, _, _) 
    | PropGtEq (loc, _, _) 
    | PropLt (loc , _, _) 
    | PropLtEq (loc , _, _) -> Some loc
    | _ -> None 

let prop_var_range phi = 
    match phi with
    | PropEq (_, var, _)
    | PropNeq (_, var, _)
    | PropGt (_, var , _)
    | PropGtEq (_, var, _)
    | PropLt (_, var, _)
    | PropLtEq (_, var, _) -> Some var
    | _ -> None

// Printers 
let print_in_order(formula : LTLFormulaType) =
    let rec print (formula : LTLFormulaType) =
        let name =
            match formula with 
            | Until (_, _, _) -> "Until"
            | Wuntil (_,_,_) -> "Weak Until"
            | Release (_, _, _) -> "Release"
            | Upto (_, _, _) -> "Upto"
            | And (_, _, _) -> "And"
            | Or (_, _, _) -> "Or"
            | Implies (_, _, _) -> "Implies"
            | Not (_, _) -> "Not"
            | Next (_, _) -> "Next"
            | Always (_, _) -> "Always"
            | Eventually (_, _) -> "Eventually"
            | Loop -> "Loop"
            | SelfLoop -> "SelfLoop"
            | Oscillation -> "Oscillation"
            | False -> "FF"
            | True -> "TT"
            | _ -> "Err"

        let left = 
            match formula with
            | Until (_, l, _) 
            | Wuntil (_, l, _)
            | Release (_, l, _)
            | Upto (_, l, _)
            | And (_, l, _) 
            | Or (_, l, _) 
            | Implies (_, l, _) 
            | Not (_, l) 
            | Next (_, l)
            | Always (_, l) 
            | Eventually (_, l) ->
                print l 
            | _ -> ""
        let right = 
            match formula with
            | Until (_, _, r) 
            | Wuntil (_, _, r)
            | Release (_, _, r)
            | Upto (_, _, r)
            | And (_, _, r) 
            | Or (_, _, r) 
            | Implies (_, _, r) ->
                print r
            | _ -> ""
        let prop = 
            match formula with
            | PropEq (_, var, value) -> sprintf "%s=%d" var.name value
            | PropNeq (_, var, value) -> sprintf "%s!=%d" var.name value
            | PropGt (_, var, value) -> sprintf "%s>%d" var.name value
            | PropGtEq (_, var, value) -> sprintf "%s>=%d" var.name value
            | PropLt (_, var, value) -> sprintf "%s<%d" var.name value
            | PropLtEq (_, var, value) -> sprintf "%s<=%d" var.name value 
            | _ -> ""

        let result =
            match formula with
            | Until (_, _, _)
            | Wuntil (_, _, _)
            | Release (_, _, _)
            | Upto (_, _, _)
            | And (_, _, _) 
            | Or (_, _, _) 
            | Implies (_, _, _) ->
                sprintf "(%s %s %s)" left name right
            | Not (_, _) 
            | Next (_, _) 
            | Always (_, _) 
            | Eventually (_, _) ->
                sprintf "(%s %s)" name left
            | PropEq (_, _, _)
            | PropNeq (_, _, _)
            | PropGt (_, _, _) 
            | PropGtEq (_, _, _)
            | PropLt (_, _, _)
            | PropLtEq (_, _, _) -> 
                prop
            | _ -> 
                name
        result
    let string_res = print formula
    Log.log_debug string_res

// parsers
let string_to_LTL_formula (s : string) (network : QN.node list) (varbyid : bool) = 
    let until = "Until"
    let wuntil = "Weakuntil"
    let release = "Release"
    let upto = "Upto"
    let always = "Always"
    let eventually = "Eventually"
    let conjunction = "And"
    let disjunction = "Or"
    let implication = "Implies"
    let negation = "Not"
    let next = "Next"
    let loop_string = "Loop"
    let self_loop_string = "SelfLoop"
    let oscillation_string = "Oscillation"
    let true_string = "True"
    let false_string = "False"
    let eq = "="
    let neq = "!="
    let gt = ">"
    let gt_eq = ">="
    let lt = "<"
    let lt_eq = "<="
    let space = " "

    let length_of_until = until.Length
    let length_of_wuntil = wuntil.Length
    let length_of_release = release.Length
    let length_of_upto = upto.Length
    let length_of_and = conjunction.Length
    let length_of_or = disjunction.Length
    let length_of_implies = implication.Length
    let length_of_not = negation.Length
    let length_of_next = next.Length
    let length_of_always = always.Length
    let length_of_eventually = eventually.Length
    let length_of_Loop = loop_string.Length
    let length_of_self_loop = self_loop_string.Length
    let length_of_oscillation = oscillation_string.Length
    let length_of_true = true_string.Length
    let length_of_false = false_string.Length
    let length_of_prop_eq = eq.Length
    let length_of_prop_neq = neq.Length
    let length_of_prop_gt = gt.Length
    let length_of_prop_gt_eq = gt_eq.Length
    let length_of_prop_lt = lt.Length
    let length_of_prop_lt_eq = lt_eq.Length


    let IsUntil (s : string) = s.StartsWith(until + space)
    let IsWuntil (s : string) = s.StartsWith(wuntil + space)
    let IsRelease (s : string) = s.StartsWith(release + space)
    let IsUpto (s : string) = s.StartsWith(upto + space)
    let IsAnd (s : string) = s.StartsWith(conjunction + space)
    let IsOr (s : string) = s.StartsWith(disjunction + space)
    let IsImplies (s : string) = s.StartsWith(implication + space)
    let IsNot (s : string) = s.StartsWith(negation + space)
    let IsNext (s : string) = s.StartsWith(next + space)
    let IsEventually (s : string) = s.StartsWith(eventually + space)
    let IsAlways (s: string) = s.StartsWith(always + space)
    let IsPropEq (s : string) = s.StartsWith(eq + space)
    let IsPropNeq (s : string) = s.StartsWith(neq + space)
    let IsPropGt (s : string) = s.StartsWith(gt + space)
    let IsPropGtEq (s : string) = s.StartsWith(gt_eq + space)
    let IsPropLt (s : string) = s.StartsWith(lt + space)
    let IsPropLtEq (s : string) = s.StartsWith(lt_eq + space)
    let IsLoop (s : string) = s.Equals(loop_string)
    let IsSelfLoop (s : string) = s.Equals(self_loop_string)
    let IsOscillation (s : string) = s.Equals(oscillation_string)
    let IsTrue (s : string) = s.Equals(true_string)
    let IsFalse(s : string) = s.Equals(false_string)

    let remove_spaces (s : string) =
        let mutable temp = s
        while temp.StartsWith(" ") do
            temp <- temp.Substring(1)
        while temp.EndsWith(" ") do
            temp <- temp.Substring(0,temp.Length - 1)
        temp

    let partition_string_to_balanced_paren (s: string) = 
        let string_length = s.Length
        let paren_height = ref 0
        let mutable (res1, res2) = ("" , "") 
        for i in 0 .. (string_length - 1) do
            if (s.Chars(i) = '(') then
                incr paren_height
            elif (s.Chars(i) = ')') then
                decr paren_height 
            elif (s.Chars(i) = ' ' && !paren_height = 0) then
                res1 <- s.Substring(0, i)
                res2 <- s.Substring(i + 1)
        res1 <- remove_spaces res1
        res2 <- remove_spaces res2
        (res1, res2)

    let (non_digit : string) = ".)( abcdefghijklmnopqrstuvwxyzABCDEFGHIGJLMOPQRSTUVWZYZ,"

    let rec parse (s: string) (location : int list) = 
        let analyze_proposition (length_of_keyword : int) (s: string) (location : int list) =
            let substring = s.Substring((length_of_keyword + 1), (s.Length - length_of_keyword - 1))
            let first_space = substring.IndexOf(" ")
            // Strips out antideath in (> antideath 2)
            let var_name_str = substring.Substring(0, first_space)
            let var_number =
                match System.Int32.TryParse var_name_str with
                | true, n -> n
                | false, _ -> -1
            // Strips out 2 in (> antideath 2) 
            let value_string = substring.Substring(first_space + 1, substring.Length - first_space - 1)
            let notFound = (network.Head , -1)
            let findVar = match varbyid with
                          | true -> fun (n : QN.node) -> n.var = var_number
                          | false -> fun (n : QN.node) -> n.name = var_name_str
            match System.Int32.TryParse value_string with
            | true, value -> 
                match List.tryFind findVar network with
                | Some(var) -> (var,value)
                | None -> notFound
            | false, _ -> notFound
                             
        let analyze_two_operands (length_of_keyword : int) (s: string) (location : int list) =
            let substring = s.Substring((length_of_keyword + 1), (s.Length - length_of_keyword - 1))
            let (str_sub_formula1, str_sub_formula2) = partition_string_to_balanced_paren (substring)
            let sub_formula1 = parse str_sub_formula1 (0::location) 
            let sub_formula2 = parse str_sub_formula2 (1::location) 
            (sub_formula1, sub_formula2)

        let analyze_one_operand length_of_keyword (s: string) (location : int list) = 
            let str_sub_formula = s.Substring((length_of_keyword + 1),(s.Length - length_of_keyword - 1))
            let sub_formula = parse str_sub_formula (0::location) 
            sub_formula

        if (not(s.StartsWith("(")) || not(s.EndsWith(")"))) then
            if (IsLoop(s)) then
                Loop
            elif (IsSelfLoop(s)) then
                SelfLoop
            elif (IsOscillation(s)) then
                Oscillation
            elif (IsTrue(s)) then
                True
            elif (IsFalse(s)) then
                False
            else
                Error
        else
            let without_paren = s.Substring(1,s.Length - 2)
            if (IsUntil(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_until without_paren location 
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Until (location,sub_formula1,sub_formula2))
            elif (IsWuntil(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_wuntil without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Wuntil (location, sub_formula1, sub_formula2))
            elif (IsRelease(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_release without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Release (location, sub_formula1, sub_formula2))
            elif (IsUpto(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_upto without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Upto (location, sub_formula1, sub_formula2))
            elif (IsAnd(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_and without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (And (location, sub_formula1, sub_formula2))
            elif (IsOr(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_or without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Or (location, sub_formula1, sub_formula2))
            elif (IsImplies(without_paren)) then
                let (sub_formula1, sub_formula2) = analyze_two_operands length_of_implies without_paren location
                if (sub_formula1 = Error || sub_formula2 = Error) then
                    Error
                else
                    (Implies (location, sub_formula1, sub_formula2))
            elif (IsNot(without_paren)) then
                let sub_formula = analyze_one_operand length_of_not without_paren location
                if (sub_formula = Error) then
                    Error
                else
                    (Not (location, sub_formula))
            elif (IsNext(without_paren)) then
                let sub_formula = analyze_one_operand length_of_next without_paren location
                if (sub_formula = Error) then
                    Error
                else
                    (Next (location, sub_formula))
            elif (IsAlways(without_paren)) then
                let sub_formula = analyze_one_operand length_of_always without_paren location
                if (sub_formula = Error) then
                    Error
                else
                    (Always (location, sub_formula))
            elif (IsEventually(without_paren)) then
                let sub_formula = analyze_one_operand length_of_eventually without_paren location
                if (sub_formula = Error) then
                    Error
                else
                    (Eventually (location, sub_formula))
            elif (IsPropEq(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_eq without_paren location
                if (value < 0) then
                    Error
                else
                    (PropEq (location, var, value))
            elif (IsPropNeq(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_neq without_paren location
                if (value < 0) then
                    Error
                else
                    (PropNeq (location, var, value))
            elif (IsPropGt(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_gt without_paren location
                if (value < 0) then
                    Error
                else
                    (PropGt (location, var, value))
            elif (IsPropGtEq(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_gt_eq without_paren location
                if (value < 0) then
                    Error
                else
                    (PropGtEq (location, var, value))
            elif (IsPropLt(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_lt without_paren location
                if (value < 0) then
                    Error
                else
                    (PropLt (location, var, value))
            elif (IsPropLtEq(without_paren)) then
                let (var, value) = analyze_proposition length_of_prop_lt_eq without_paren location
                if (value < 0) then
                    Error
                else
                    (PropLtEq (location, var, value))
            elif (IsSelfLoop(without_paren)) then
                SelfLoop
            elif (IsLoop(without_paren)) then
                Loop
            elif (IsOscillation(without_paren)) then
                Oscillation
            elif (IsTrue(without_paren)) then
                True
            elif (IsFalse(without_paren)) then
                False
            else
                Error
                    
    parse s [] 

let unable_to_parse_formula =
    ignore (Log.log_debug "Was not able to parse the LTL formula!")

// This test returns normal formulas in a network that has
// v1 v2 and v3 as variables
// Otherwise, all formulas are errors)
let test_LTL_parser (network) =
    let formula_one_string = "(Until (Not (> a 5)) (> b 6))"
    let formula_one = string_to_LTL_formula formula_one_string  network
    let formula_two_string = "(Until (Release (Not (> a 5)) (Next (>= b 17))) (And (> c 6) (<= b 564)))"
    let formula_two = string_to_LTL_formula formula_two_string  network
    let formula_three_string = "(Always (Or (Not (Next (>= b 6344))) (Eventually (Next (<= c 343245)))))"
    let formula_three = string_to_LTL_formula formula_three_string network
    // This is formula three without some close parenthesis
    let formula_four_string = "(Always (Or (Not (Next (>= a 6344))) (Eventually (Next (<= c 343245))))"
    let formula_four = string_to_LTL_formula formula_four_string network
    // This is formula three without some open parenthesis
    let formula_five_string = "(Always (Or (Not (Next >= a 6344))) (Eventually (Next (<= a 343245)))))"
    let formula_five = string_to_LTL_formula formula_five_string network
    // This is formula three with = and != instead of >= and <=
    let formula_six_string = "(Always (Or (Not (Next (= b 6344))) (Eventually (Next (!= c 343245)))))"
    let formula_six = string_to_LTL_formula formula_six_string network
    // This is formula one with weakuntil replacing until 
    let formula_seven_string = "(Weakuntil (Not (> a 5)) (> b 6))"
    let formula_seven = string_to_LTL_formula formula_seven_string  network
    let formula_eight_string = "(Upto (Not (> a 5)) (> b 6))"
    let formula_eight = string_to_LTL_formula formula_eight_string  network
    let formula_nine_string = "(Always (Or (And (Not SelfLoop) Oscillation)) (Eventually (Or (Next (<= v1 343245) Loop)))))"
    let formula_nine = string_to_LTL_formula formula_nine_string network

    ignore(formula_three)
