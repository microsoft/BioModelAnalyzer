module UIExpr

open Expr

// The main exported function of this solution is [check_syntax]. It returns a 
// ParseOK on successfully parsing [s]. Otherwise it returns a ParseErr. 
type perr = { line : int; col : int; msg: string } 
type parse_result = ParseOK of expr | ParseErr of perr

let check_syntax (s:string) = 
    match FParsec.CharParsers.run ExprParser.expr s with
    | FParsec.CharParsers.Success(f,_,pos) -> ParseOK(f)
    | FParsec.CharParsers.Failure(msg,err,_) -> 
        let l,c = (int)err.Position.Line, (int)err.Position.Column
        ParseErr({line= l; col= c; msg= msg})
    


// An example use of check_syntax: unit testing the parser. 
let check (s:string) (expected:parse_result) : unit = 
    match (check_syntax s), expected with
    | ParseOK(f), ParseOK(f') -> 
        if f = f' then Printf.printf "OK.\n" 
        else Printf.printf "Not OK, got a %s instead of a %s\n" (str_of_expr f') (str_of_expr f)
    | ParseErr({line=line; col=col; msg=msg}), ParseErr(_) -> 
        Printf.printf "Parse failed as expected, at line %d, col %d, msg=%s.\n" line col msg
    | _ -> Printf.printf "!!Didn't get what I expected!\n"

let _ = 
    let a_err = ParseErr({line=0; col=0; msg=""})
    check "var(C1.x)" (ParseOK(Ident("C1.x"))) 
    check "var(C1.x.y)" (ParseOK(Ident("C1.x.y"))) // Shouldn't be allowed for now. But is. 
    check "-10+var(x)" (ParseOK(Plus(Const(-10),Ident("x"))))
    check "var(Xx23_xLT)" (ParseOK(Ident("Xx23_xLT")))
    check "4-2 * var(x) " (ParseOK(Minus(Const(4),(Times(Const(2),Ident("x"))))))
    check "max (               2,3)" (ParseOK(Max(Const(2),Const(3))))
    check "min(var(x),   var(y))  / 2" (ParseOK(Div(Min(Ident("x"),Ident("y")),Const(2))))
    check "avg(1,2,3,4)" (ParseOK((Avg([Const(1);Const(2);Const(3);Const(4)]))))
    check "0" (ParseOK(Const(0)))
    check 
        "max( (avg(var(x),var(y),var(z),var(w),var(v),var(u)) - avg(var(a),var(b),var(c),var(d))), 0)"
        (let fst_avg = Avg([Ident("x");Ident("y");Ident("z");Ident("w");Ident("v");Ident("u")])
         let snd_avg = Avg([Ident("a");Ident("b");Ident("c");Ident("d")])
         (ParseOK(Max(Minus(fst_avg,snd_avg),Const(0)))))
    check "( ( ((  ((((2 )" a_err
    check "avg(neg) - (pos) " (ParseOK(Minus(Avg([Neg]),Pos))) // OK grammatically. (We don't really check for wff.)
    check "max( 2, 3 " a_err // Not OK, missing closing bracket
    check "222 + $$$ " a_err // Not OK, junk after the +. 
    check "max 2,3)" a_err // Not OK, missing opening bracket. 
