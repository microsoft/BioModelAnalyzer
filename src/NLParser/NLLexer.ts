import * as chevrotain from 'chevrotain'
import NLToken from './NLToken'
import * as natural from 'natural'
import * as _ from 'underscore'

var extendToken = chevrotain.extendToken;
var Lexer = chevrotain.Lexer

//literals
var IntegerLiteral = extendToken("IntegerLiteral", /\d+/);
var Identifier = extendToken("Identifier", /\w+/);
//Ignore
var WhiteSpace = extendToken("WhiteSpace", /\s+/);
WhiteSpace.GROUP = Lexer.SKIPPED

export default class NLLexer {
    allowedTokens: NLToken[]
    constructor(dictionary) {
        this.allowedTokens = [new NLToken("WhiteSpace", WhiteSpace), new NLToken("IntegerLiteral", IntegerLiteral)].concat(_.map(dictionary, (v: [string], k) => new NLToken(k, extendToken(k, RegExp(v.join('|'), "i")))).concat(new NLToken("Identifier", Identifier)));
    }

    lex(sentence: string) {
        var lexer = new Lexer(this.allowedTokens.map((nlt) => nlt.token))

        return {
            output: lexer.tokenize(sentence),
            tokensAcceptedByParser: this.allowedTokens
        }
    }
}
