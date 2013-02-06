module ExprParser
(*
open FParsec

let ws = spaces // skips any whitespace

let str_ws s = pstring s >>. ws

// we calculate with double precision floats
let number = pfloat .>> ws

// we set up an operator precedence parser for parsing the arithmetic expressions
let opp = new OperatorPrecedenceParser<float,unit,unit>()
let expr = opp.ExpressionParser
opp.TermParser <- number <|> between (str_ws "(") (str_ws ")") expr

// operator definitions follow the schema
// operator type, string, trailing whitespace parser, precedence, associativity, function to apply

opp.AddOperator(InfixOperator("+", ws, 1, Associativity.Left, (+)))
opp.AddOperator(InfixOperator("-", ws, 1, Associativity.Left, (-)))
opp.AddOperator(InfixOperator("*", ws, 2, Associativity.Left, (*)))
opp.AddOperator(InfixOperator("/", ws, 2, Associativity.Left, (/)))
opp.AddOperator(InfixOperator("^", ws, 3, Associativity.Right, fun x y -> System.Math.Pow(x, y)))
opp.AddOperator(PrefixOperator("-", ws, 4, true, fun x -> -x))

// we also want to accept the operators "exp" and "log", but we don't want to accept
// expressions like "logexp" 2, so we require that non-symbolic operators are not
// followed by letters

let ws1 = nextCharSatisfiesNot isLetter >>. ws
opp.AddOperator(PrefixOperator("log", ws1, 4, true, System.Math.Log))
opp.AddOperator(PrefixOperator("exp", ws1, 4, true, System.Math.Exp))

let completeExpression = ws >>. expr .>> eof // we append the eof parser to make
                                            // sure all input is consumed

// running and testing the parser
/////////////////////////////////

let calculate s = run completeExpression s

let equals expectedValue r =
    match r with
    | Success (v, _, _) when v = expectedValue -> ()
    | Success (v, _, _)     -> failwith "Math is hard, let's go shopping!"
    | Failure (msg, err, _) -> printf "%s" msg; failwith msg

let test() =
    calculate "10.5 + 123.25 + 877"  |> equals 1010.75
    calculate "10/2 + 123.125 + 877" |> equals 1005.125
    calculate "(123 + log 1 + 877) * 9/3" |> equals 3000.
    calculate " ( ( exp 0 + (6 / ( 1 +2 ) )- 123456 )/ 2+123 + 877) * 3^2 / 3" |> equals (-182179.5)
    printfn "No errors"

// currently the program only executes some tests
//do test()

*)







open System
open System.Collections.Generic

open FParsec
open Expr

// some lexical definitions
///////////////////////////

let ws  = spaces // skips any whitespace

let str s = pstring s >>. ws

// identifiers are strings of lower ascii chars that are not keywords
let identifierString = many1Satisfy isLower .>> ws // [a-z]+
let keywords = ["while"; "begin"; "end"; "do"; "if"; "then"; "else"; "print"; "decr"]
let keywordsSet = new HashSet<string>(keywords)
let isKeyword str = keywordsSet.Contains(str)

//open FParsec.StaticMapping
//let isKeyword = createStaticStringMapping false [for kw in keywords -> (kw, true)]

let identifier : Parser<string, unit> =
    let expectedIdentifier = expected "identifier"
    fun stream ->
        let state = stream.State
        let reply = identifierString stream
        if reply.Status <> Ok || not (isKeyword reply.Result) then reply
        else // result is keyword, so backtrack to before the string
            stream.BacktrackTo(state)
            Reply(Error, expectedIdentifier)


let numberFormat =     NumberLiteralOptions.AllowMinusSign
                   ||| NumberLiteralOptions.AllowFraction
                   ||| NumberLiteralOptions.AllowExponent
let numberLit = numberLiteral numberFormat "number" .>> ws


// parsers for the original grammar productions
///////////////////////////////////////////////

let pval = identifier |>> Ident

let number =
    numberLit
    |>> fun nl -> Const (int32 nl.String)

// expr and decr are mutually recursive grammar grammar productions.
// In order to break the cyclic dependency, we make expr a parser that
// forwards all calls to a parser in a reference cell.
let expr, exprRef = createParserForwardedToRef() // initially exprRef holds a reference to a dummy parser

// Use parser hierarchy to deal with op precedence. 

let pdiv   = expr >>. str "/" >>. expr |>> Div
let pminus = expr >>. str "-" >>. expr |>> Minus
let pplus  = expr >>. str "+" >>. expr |>> Plus
let ptimes = expr >>. str "*" >>. expr |>> Times

//let pdecr = str "decr" >>. str "(" >>. expr .>> str ")" |>> Decr

// replace dummy parser reference in exprRef
do exprRef:= choice [pval; number; pdiv; pminus; pplus; ptimes] 


let stmt, stmtRef = createParserForwardedToRef()

let stmtList = sepBy1 stmt (str ";")

let assign =
    pipe2 identifier (str ":=" >>. expr) (fun id e -> Assign(id, e))

let print = str "print" >>. expr |>> Print

let pwhile =
    pipe2 (str "while" >>. expr) (str "do" >>. stmt) (fun e s -> While(e, s))

let seq =
    str "begin" >>. stmtList .>> str "end" |>> Seq

let ifthen =
    pipe3 (str "if" >>. expr) (str "then" >>. stmt) (opt (str "else" >>. stmt))
          (fun e s1 optS2 ->
               match optS2 with
               | None    -> IfThen(e, s1)
               | Some s2 -> IfThenElse(e, s1, s2))

do stmtRef:= choice [assign; ifthen; pwhile; seq; print] // try assign first, so that an
                                                         // identifier starting with a
                                                         // keyword doesn't trigger an error
let prog =
    ws >>. stmtList .>> eof |>> Prog


