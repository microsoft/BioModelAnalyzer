// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module Parser 

exception ParseException of string

open FParsec.CharParsers
open FParsec
open Ast
type SMV() = 
    let ws : Parser<unit,unit> = many 
                                    (skipString "--" .>> (skipRestOfLine true)
                                    <|> spaces1) >>% ()

    //let skipStringNS = skipString
    let skipString a =  skipString a .>> ws
    let pint64 = pint64 .>> ws


    let kwModule = skipString "MODULE"
    let kwVar = skipString "VAR"
    let kwBounded = skipString  "BOUNDED"
    let kwAssign = skipString "ASSIGN"
    let kwinit = skipString "init"
    let kwnext = skipString "next"
    let kwInit = skipString "INIT"
    let kwTrans = skipString "TRANS"
    let kwcase = skipString "case"
    let kwesac = skipString "esac"

    let keywords = ["MODULE"; "VAR"; "BOUNDED";"ASSIGN"; "init"; "next"; "INIT"; "TRANS"; "case"; "esac"]

    let pident : Parser<string,unit> = attempt (many1Satisfy2 (fun c -> isLetter c) (fun c -> isLetter c || isDigit c || '_' = c ) >>= fun s -> if List.exists ( (=) s) keywords then pzero else preturn s) .>> ws  

    let plist b e s pp = between (skipString b) (skipString e) (sepBy pp (skipString s))

    let parguments pp = plist "(" ")" "," pp

    let parguments_opt pp =  opt (parguments pp)

    let pidentexpr = sepBy pident (skipString ".")

    let ptype = 
        (plist "{" "}" "," pident |>> Set)
        <|> (pident .>>. parguments_opt pidentexpr |>> Module)
        <|> (pint64 .>> skipString ".." .>>. pint64 |>> Range)

    let pvardecl = ((pident .>> skipString ":") .>>. ptype) .>> skipString ";"

    let pvardecls : Parser<(string * types) list,unit> = many1 pvardecl

    let pbounddecl = ((pident .>> skipString ":") .>>. ptype) .>> skipString ";"

    let pbounddecls : Parser<(string * types) list,unit> = many1 pbounddecl


    let opp = new OperatorPrecedenceParser<expr,unit,unit>()
    let pexpr = opp.ExpressionParser .>> ws

    (* TODO What is the actual operator precedence for SMV *)
    let _ = opp.AddOperator(PrefixOperator("!", ws, 6, false, Not))
    let _ = opp.AddOperator(InfixOperator("|", ws, 2, Associativity.Left, fun x y-> Or(x,y)))
    let _ = opp.AddOperator(InfixOperator("&", ws, 3, Associativity.Left, fun x y-> And(x,y)))
    let _ = opp.AddOperator(InfixOperator("->", ws, 1, Associativity.Left, fun x y-> Imp(x,y)))


    let _ = opp.AddOperator(InfixOperator("<", ws, 4, Associativity.Left, fun x y-> Lt(x,y)))
    let _ = opp.AddOperator(InfixOperator("<=", ws, 4, Associativity.Left, fun x y-> Le(x,y)))
    let _ = opp.AddOperator(InfixOperator(">", ws, 4, Associativity.Left, fun x y-> Gt(x,y)))
    let _ = opp.AddOperator(InfixOperator(">=", ws, 4, Associativity.Left, fun x y-> Ge(x,y)))
    let _ = opp.AddOperator(InfixOperator("=", ws, 4, Associativity.Left, fun x y-> Eq(x,y)))
    let _ = opp.AddOperator(InfixOperator("!=", ws, 4, Associativity.Left, fun x y-> Neq(x,y)))

    let _ = opp.AddOperator(InfixOperator("+", ws, 6, Associativity.Left, fun x y-> Add(x,y)))

    let pcases =
        between kwcase kwesac (many1 (pexpr .>> skipString ":" .>>. pexpr .>> skipString ";"))
            |>> Cases

    let pnext = kwnext >>. between (skipString "(") (skipString ")") pexpr |>> Next

    let pparen = between (skipString "(") (skipString ")") pexpr

    let pint : Parser<expr, unit> = pint64 |>> Int

    let _ = opp.TermParser <- pint <|> pnext <|> pparen <|> pcases <|> (pident |>> Ident)

    let pdefn pkw pbody =
        pkw >>. skipString "(" >>. pident .>> skipString ")" .>> 
            skipString ":=" .>>. pbody .>> skipString ";"

    let pinitvar = pdefn kwinit pexpr |>> InitAssign

    let pupdatevar = pdefn kwnext pexpr |>> NextAssign

    let pupdate = 
          pinitvar
          <|> pupdatevar  

    let pVarSec = kwVar >>. pvardecls |>> Var
    let pBoundSec = kwBounded >>. pbounddecls |>> Bounded
    let pTransSec = kwTrans >>. pexpr |>> Trans
    let pInitSec = kwInit >>. pexpr |>> Init
    let pAssignSec = kwAssign >>. many pupdate |>> Assigns

    let pSecs = pVarSec <|> pBoundSec <|> pTransSec <|> pInitSec <|> pAssignSec

    let pModule =
        kwModule >>. pident .>>. (parguments_opt pident) .>>. many pSecs 
            |>> fun ((name,ps),ss) -> 
                let ps = match ps with None -> [] | Some ps -> ps
                {name=name; parameters=ps; sections=ss}

    let pSmv = (ws >>. many pModule .>> eof)


    member this.parser_smv file = 
        let res = runParserOnFile  pSmv () file  System.Text.Encoding.Default 
        match res with 
        | Success(r,_,_) -> r
        | Failure(errormsg, _, _)  -> raise (ParseException(errormsg))

