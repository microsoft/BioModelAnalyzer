// Signature file for parser generated by fsyacc
module ExprParse
type token = 
  | AVG
  | CEIL
  | FLOOR
  | MAX
  | MIN
  | VAR
  | DIV
  | MINUS
  | PLUS
  | TIMES
  | COMMA
  | LPAREN
  | RPAREN
  | EOF
  | NUM of (int)
type tokenId = 
    | TOKEN_AVG
    | TOKEN_CEIL
    | TOKEN_FLOOR
    | TOKEN_MAX
    | TOKEN_MIN
    | TOKEN_VAR
    | TOKEN_DIV
    | TOKEN_MINUS
    | TOKEN_PLUS
    | TOKEN_TIMES
    | TOKEN_COMMA
    | TOKEN_LPAREN
    | TOKEN_RPAREN
    | TOKEN_EOF
    | TOKEN_NUM
    | TOKEN_end_of_input
    | TOKEN_error
type nonTerminalId = 
    | NONTERM__startfunc
    | NONTERM_func
    | NONTERM_expr
    | NONTERM_expr_list
/// This function maps integers indexes to symbolic token ids
val tagOfToken: token -> int

/// This function maps integers indexes to symbolic token ids
val tokenTagToTokenId: int -> tokenId

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
val prodIdxToNonTerminal: int -> nonTerminalId

/// This function gets the name of a token as a string
val token_to_string: token -> string
val func : (Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> token) -> Microsoft.FSharp.Text.Lexing.LexBuffer<'cty> -> (expr) 
