/*
TODO:
4) parenthesis the output
5) implement fault tolarance
7) add stemming
10) pretty print the output
11) comma sytax when dealing with variables
12) handle arbitary text anywhere in the string (use the lexer to just ignore the tokens), perhaps use the model to identify tokens that are neither variables nor tokens
13) automatically generate the stemmed tokens by passing in the lexer some settings or a factory approach
14) recursively action
15) check operator precedence
16) prepend with a missing eventually if no temporal operator provided
17) add not equal to param
18) handle variables with space in them
19) convert variable usage to notation
20) make sure variables are not stemmed
*/

import * as chevrotain from 'chevrotain'
import * as _ from 'underscore'
import * as natural from 'natural'
import { BMAModel } from '../BMAModel'
import { Parser, Token, IParserConfig, Lexer, TokenConstructor } from 'chevrotain'
var extendToken = chevrotain.extendToken;

/**
 *  GRAMMAR TOKENS
 */
let If = generateStemmedTokenDefinition("If", ["if"])
let Then = generateStemmedTokenDefinition("Then", ["then"])
/**
 * Arithmetic operator tokens
 */
let GThan = generateStemmedTokenDefinition("GThan", [">", "greater than", "bigger than"])
let LThan = generateStemmedTokenDefinition("LThan", ["<", "less than", "smaller than"])
let GThanEq = generateStemmedTokenDefinition("GThanEq", [">=", "greater than or equal to", "bigger than or equal to"])
let LThanEq = generateStemmedTokenDefinition("LThanEq", ["<=", "less than or equal to", "smaller than or equal to"])
let Eq = generateStemmedTokenDefinition("Eq", ["=", "is equal to", "is same as", "equal", "is"])
let NotEq = generateStemmedTokenDefinition("NotEq", ["!=", "is not equal to", "is not same as", "not equal", "is not"])
/** 
 *  Boolean operator tokens
 */
let And = generateStemmedTokenDefinition("And", ["and", "conjunction", "as well as", "also", "along with", "in conjunction with", "plus", "together with"])
let Or = generateStemmedTokenDefinition("Or", ["or", "either"])
let Implies = generateStemmedTokenDefinition("Implies", ["implies", "means"])
/**
 *  Temporal operator tokens
 */
let Eventually = generateStemmedTokenDefinition("Eventually", ["eventually", "finally", "in time", "ultimately", "after all", "at last", "sometime", "some point", "in the long run", "in a while", "soon", "at the end"])
let Always = generateStemmedTokenDefinition("Always", ["always", "invariably", "perpetually", "forever", "constantly"])
let Next = generateStemmedTokenDefinition("Next", ["next", "after", "then", "consequently", "afterwards", "subsequently", "followed by", "after this"])
let Not = generateStemmedTokenDefinition("Not", ["not", "never"])
let Upto = generateStemmedTokenDefinition("Upto", ["upto"])
let Until = generateStemmedTokenDefinition("Until", ["until"])
let WUntil = generateStemmedTokenDefinition("WUntil", ["weak until"])
let Release = generateStemmedTokenDefinition("Release", ["release"])
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
/**
 *  Explicit Token Precedence for Lexer (tokens with lower index have higher priority)
 */
let allowedTokens = [WhiteSpace, IntegerLiteral, If, Then, GThan, LThan, GThanEq, LThanEq, Eq, NotEq, And, Or, Implies, Eventually, Always, Next, Not, Upto, Until, WUntil, Release, Identifier]

export enum ParserResponseType {
    SUCCESS,
    NO_PARSABLE_TOKEN_FOUND,
    PARSE_ERROR,
    UNKNOWN_VARIABLES_FOUND
}

export interface ParserResponse {
    responseType: ParserResponseType
    errors?: any
    AST?: any
}


const configuration: IParserConfig = {
    recoveryEnabled: true
}

function generateStemmedTokenDefinition(id: string, synonyms: string[]) {
    var tokenFunction = extendToken(id, RegExp(synonyms.map(natural.PorterStemmer.stem).join('|'), "i"));
    tokenFunction.LABEL = synonyms[0]
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
            value: { type: "binaryOperator", value: Implies.LABEL },
            left: conditionClause,
            right: body
        }
    })

    private expression = this.RULE("expression", () => {
        return {
            type: "expression",
            left: this.SUBRULE(this.term),
            right: null
        };
    })

    private term = this.RULE("term", () => {
        let tree, factors = [],
            operators = []

        factors.push(this.SUBRULE(this.factor));
        this.MANY(() => {
            operators.push(this.SUBRULE(this.binaryOperator))
            factors.push(this.SUBRULE2(this.factor))
        })

        tree = {
            type: "term",
            left: factors[0],
            right: null
        }
        if (factors.length > 1) {
            var subtree = tree;
            for (var i = 1; i < factors.length; i++) {
                subtree.value = operators[i - 1];
                subtree.right = {
                    type: "term",
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
                    return And.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Or)
                    return Or.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Implies)
                    return Implies.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Upto)
                    return Upto.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Until)
                    return Until.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Release)
                    return Release.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(WUntil)
                    return WUntil.LABEL
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
                    return Not.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Next)
                    return Next.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Always)
                    return Always.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Eventually)
                    return Eventually.LABEL
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

    static parse(sentence: string, bmaModel: BMAModel): ParserResponse {
        let tokensLessIdentifier = _.initial(allowedTokens)
        sentence = sentence.toLowerCase().split(" ").map(natural.PorterStemmer.stem).join(" ")
        let lexedTokens = (new Lexer(allowedTokens, true)).tokenize(sentence).tokens
        //partition tokens that are neither operators nor model variables
        let modelVariables = _.pluck(bmaModel.variables, "Name")
        let partitionedTokens = _.partition(lexedTokens, (t) => tokensLessIdentifier.some((r) => new RegExp(r.PATTERN).test(t.image)) || _.contains(modelVariables, t.image))
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

            var parser = new NLParser(tokens.accepted)
            var AST = parser.formula()
            if (AST) {
                return {
                    responseType: ParserResponseType.SUCCESS,
                    AST: AST
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

