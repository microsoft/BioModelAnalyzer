/*
TODO:
4) parenthesis the output
10) pretty print the output
15) check operator precedence
16) prepend with a missing eventually if no temporal operator provided
18) handle variables with space in them
19) convert variable usage to notation
20) make sure variables are not stemmed
21) later = eventually next (composite)
22) check operator precedence
23) word substrings are matching to te operators
*/

import * as chevrotain from 'chevrotain'
import * as _ from 'underscore'
import * as natural from 'natural'
import * as ASTUtils from './ASTUtils'
import { Parser, Token, IParserConfig, Lexer, TokenConstructor } from 'chevrotain'
var extendToken = chevrotain.extendToken;

export enum TokenType {
    LOGICAL_BINARY,
    LOGICAL_UNARY,
    OTHER
}

/**
 *  GRAMMAR TOKENS
 */
let If = generateStemmedTokenDefinition("If", ["if"],TokenType.OTHER,Keyword)
let Then = generateStemmedTokenDefinition("Then", ["then"],TokenType.OTHER,Keyword)
/**
 * Arithmetic operator tokens
 */
let GThan = generateStemmedTokenDefinition("GThan", [">", "greater than", "bigger than"],TokenType.OTHER,Keyword)
let LThan = generateStemmedTokenDefinition("LThan", ["<", "less than", "smaller than"],TokenType.OTHER,Keyword)
let GThanEq = generateStemmedTokenDefinition("GThanEq", [">=", "greater than or equal to", "bigger than or equal to"],TokenType.OTHER,Keyword)
let LThanEq = generateStemmedTokenDefinition("LThanEq", ["<=", "less than or equal to", "smaller than or equal to"],TokenType.OTHER,Keyword)
let Eq = generateStemmedTokenDefinition("Eq", ["=", "is equal to", "is same as", "equal", "is"],TokenType.OTHER,Keyword)
let NotEq = generateStemmedTokenDefinition("NotEq", ["!=", "is not equal to", "is not same as", "not equal", "is not"],TokenType.OTHER,Keyword)
/** 
 *  Boolean operator tokens
 */
let And = generateStemmedTokenDefinition("And", ["and", "conjunction", "as well as", "also", "along with", "in conjunction with", "plus", "together with"],TokenType.LOGICAL_BINARY,Keyword)
let Or = generateStemmedTokenDefinition("Or", ["or", "either"],TokenType.LOGICAL_BINARY,Keyword)
let Implies = generateStemmedTokenDefinition("Implies", ["implies", "means"],TokenType.LOGICAL_BINARY,Keyword)
/**
 *  Temporal operator tokens
 */
let Eventually = generateStemmedTokenDefinition("Eventually", ["eventually", "finally", "in time", "ultimately", "after all", "at last", "some point", "in the long run", "in a while", "soon", "at the end"],TokenType.LOGICAL_UNARY,Keyword)
let Always = generateStemmedTokenDefinition("Always", ["always", "invariably", "perpetually", "forever", "constantly"],TokenType.LOGICAL_UNARY,Keyword)
let Next = generateStemmedTokenDefinition("Next", ["next", "after", "then", "consequently", "afterwards", "subsequently", "followed by", "after this"],TokenType.LOGICAL_UNARY,Keyword)
let Not = generateStemmedTokenDefinition("Not", ["not", "never"],TokenType.LOGICAL_UNARY,Keyword)
let Upto = generateStemmedTokenDefinition("Upto", ["upto"],TokenType.LOGICAL_BINARY,Keyword)
let Until = generateStemmedTokenDefinition("Until", ["until"],TokenType.LOGICAL_BINARY,Keyword)
let WUntil = generateStemmedTokenDefinition("WUntil", ["weak until"],TokenType.LOGICAL_BINARY,Keyword)
let Release = generateStemmedTokenDefinition("Release", ["release"],TokenType.LOGICAL_BINARY,Keyword)
/**
 * literals (no stemming required)
 */
let IntegerLiteral = extendToken("IntegerLiteral", /\d+/);
let Identifier = extendToken("Identifier", /\w+/);
/**
 *  Ignored tokens
 */
let WhiteSpace = extendToken("WhiteSpace", /\s+/);
WhiteSpace.GROUP = Lexer.SKIPPED

var Keyword = extendToken("Keyword", Lexer.NA);
// LONGER_ALT will make the Lexer perfer a longer Identifier over a Keyword.
Keyword.LONGER_ALT = Identifier;

/**
 *  Explicit Token Precedence for Lexer (tokens with lower index have higher priority)
 */
let allowedTokens = [WhiteSpace, IntegerLiteral, Identifier,Keyword,If, Then, GThan, LThan, GThanEq, LThanEq, NotEq, Eq, And, Or, Implies, Eventually, Always, Next, Not, Upto, Until, WUntil, Release]

export enum ParserResponseType {
    SUCCESS,
    NO_PARSABLE_TOKEN_FOUND,
    PARSE_ERROR,
    UNKNOWN_VARIABLES_FOUND
}

export interface ParserResponse {
    responseType: ParserResponseType
    humanReadableFormula?:string
    errors?: any
    AST?: any
}


const configuration: IParserConfig = {
    recoveryEnabled: true
}

function generateStemmedTokenDefinition(id: string, synonyms: string[],tokenType: TokenType,alternative?:TokenConstructor) {
    var tokenFunction = extendToken(id, RegExp(synonyms.map(natural.PorterStemmer.stem).join('|'), "i"),alternative);
    tokenFunction.LABEL = synonyms[0]
    tokenFunction.TOKEN_TYPE = tokenType
    tokenFunction.SYNONYMS = synonyms
    return tokenFunction
}


export default class NLParser extends Parser {

    private formula = this.RULE("formula", () => {
        let ltlFormula, tree = {
            type: "ltlFormula",
            left: null
        }
        var subtree = tree
        this.MANY(() => {
            subtree.left = this.SUBRULE(this.unaryOperator)
            subtree = subtree.left
        })
        subtree.left = this.OR([{
            ALT: () => {
                return this.SUBRULE(this.expression)
            }
        }, {
            ALT: () => {
                return this.SUBRULE(this.ifFormula)
            }
        }]);

        var trailingTree, lastTrailingNode

        //handle trailing operators
        this.MANY2(() => {
            var subTrailingTree = trailingTree
            var operator = this.SUBRULE2(this.unaryOperator)
            if (subTrailingTree) {
                subTrailingTree.left = operator
                subTrailingTree = subTrailingTree.left
                lastTrailingNode = subTrailingTree
            } else {
                trailingTree = operator
                lastTrailingNode = operator
            }
        })
        var resultTree
        if (lastTrailingNode) {
            lastTrailingNode.left = tree
            resultTree = trailingTree
        } else {
            resultTree = tree
        }
        return {
            type: "ltlFormula",
            left: resultTree
        }
    })

    private ifFormula = this.RULE("ifFormula", () => {
        let conditionClause, body
        this.CONSUME(If)
        conditionClause = this.SUBRULE(this.expression)
        this.CONSUME(Then)
        body = this.SUBRULE2(this.expression)

        return {
            type: "expression",
            value: { type: "binaryOperator", value: Implies },
            left: conditionClause,
            right: body
        }
    })


    private expression = this.RULE("expression", () => {
        let tree, factors = [],
            operators = []

        factors.push(this.SUBRULE(this.factor));
        this.MANY(() => {
            operators.push(this.SUBRULE(this.binaryOperator))
            factors.push(this.SUBRULE2(this.factor))
        })

        tree = {
            type: "expression",
            left: factors[0],
            right: null
        }
        if (factors.length > 1) {
            var subtree = tree;
            for (var i = 1; i < factors.length; i++) {
                subtree.value = operators[i - 1];
                subtree.right = {
                    type: "expression",
                    left: factors[i],
                    right: null
                }
                subtree = subtree.right
            }
        }
        return tree;
    })

    private factor = this.RULE("factor", () => {
        var lastNode, tree = {
            type: "factor",
            left: null
        }

        this.MANY(() => {
            var operator = this.SUBRULE(this.unaryOperator)
            var subTree = tree
            subTree.left = operator
            subTree = subTree.left
            lastNode = subTree
        })
        var relationalExpression = this.SUBRULE(this.relationalExpression)
        if (lastNode) {
            lastNode.left = relationalExpression
        } else {
            tree.left = relationalExpression
        }
        return {
            type: "factor",
            left: tree
        }
    })

    private relationalExpression = this.RULE("relationalExpression", () => {
        let identifier, relationalOperator, integerLiteral
        identifier = this.CONSUME(Identifier).image
        relationalOperator = this.SUBRULE(this.relationalOperator)
        integerLiteral = parseInt(this.CONSUME(IntegerLiteral).image)
        return {
            type: "relationalExpression",
            value: relationalOperator,
            left: identifier,
            right: integerLiteral
        }
    })

    private relationalOperator = this.RULE("relationalOperator", () => {
        return {
            type: "relationalOperator",
            value: this.OR([{
                ALT: () => {
                    this.CONSUME(GThan)
                    return GThan.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(LThan)
                    return LThan.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(GThanEq)
                    return GThanEq.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Eq)
                    return Eq.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(LThanEq)
                    return LThanEq.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(NotEq)
                    return NotEq.LABEL
                }
            }])
        }
    });

    private binaryOperator = this.RULE("binaryOperator", () => {
        return {
            type: "binaryOperator",
            value: this.OR([{
                ALT: () => {
                    this.CONSUME(And)
                    return And
                }
            }, {
                ALT: () => {
                    this.CONSUME(Or)
                    return Or
                }
            }, {
                ALT: () => {
                    this.CONSUME(Implies)
                    return Implies
                }
            }, {
                ALT: () => {
                    this.CONSUME(Upto)
                    return Upto
                }
            }, {
                ALT: () => {
                    this.CONSUME(Until)
                    return Until
                }
            }, {
                ALT: () => {
                    this.CONSUME(Release)
                    return Release
                }
            }, {
                ALT: () => {
                    this.CONSUME(WUntil)
                    return WUntil
                }
            }])
        }
    });

    private unaryOperator = this.RULE("unaryOperator", () => {
        return {
            type: "unaryOperator",
            value: this.OR([{
                ALT: () => {
                    this.CONSUME(Not)
                    return Not
                }
            }, {
                ALT: () => {
                    this.CONSUME(Next)
                    return Next
                }
            }, {
                ALT: () => {
                    this.CONSUME(Always)
                    return Always
                }
            }, {
                ALT: () => {
                    this.CONSUME(Eventually)
                    return Eventually
                }
            }])
        }
    });

    //for internal purpose only
    private constructor(inputTokens: Token[]) {
        super(inputTokens, allowedTokens, configuration)
        // very important to call this after all the rules have been defined.
        // otherwise the parser may not work correctly as it will lack information
        // derived during the self analysis phase.
        Parser.performSelfAnalysis(this);
    }

    static parse(sentence: string, bmaModel): ParserResponse {
        //partition tokens that are neither operators nor model variables
        let modelVariables = _.pluck(bmaModel.Model.Variables, "Name")
        let tokensLessIdentifier = _.initial(allowedTokens)
        sentence = sentence.toLowerCase().split(" ").map((t)=>_.contains(modelVariables,t) ? t : natural.PorterStemmer.stem(t)).join(" ")
        let lexedTokens = (new Lexer(allowedTokens, true)).tokenize(sentence).tokens
        let partitionedTokens = _.partition(lexedTokens, (t) => tokensLessIdentifier.some((r) => _.contains(r.SYNONYMS,t.image)) || _.contains(modelVariables, t.image) || IntegerLiteral.PATTERN.test(t.image))
        //return with an error if unknown variables found in the token stream
        let tokens = {
            accepted: partitionedTokens[0],
            rejected: partitionedTokens[1]
        }
        if (!_.isEmpty(tokens.accepted)) {
            let arithmeticOperators = [Eq, NotEq, LThanEq, GThanEq, GThan, LThan]
            //filter accepted tokens to reject relational operators that do not have any operands
            var filteredTokens = []
            for (var i = 0; i < tokens.accepted.length; i++) {
                if (!(arithmeticOperators.some((op) => new RegExp(op.PATTERN).test(tokens.accepted[i].image)) && (i < 1 || i > tokens.accepted.length - 1 || !new RegExp(IntegerLiteral.PATTERN).test(tokens.accepted[i + 1].image) || (!new RegExp(Identifier.PATTERN).test(tokens.accepted[i - 1].image))))) {
                    filteredTokens.push(tokens.accepted[i])
                }
            }

            var parser = new NLParser(filteredTokens)
            var AST = parser.formula()
            if (AST) {
                return {
                    responseType: ParserResponseType.SUCCESS,
                    humanReadableFormula: ASTUtils.toHumanReadableString(AST)
                }
            } else {
                return {
                    responseType: ParserResponseType.PARSE_ERROR,
                    errors: parser.errors,
                }
            }
        } else {
            return {
                responseType: ParserResponseType.NO_PARSABLE_TOKEN_FOUND,
                errors: parser.errors,
            }
        }
    }
}

