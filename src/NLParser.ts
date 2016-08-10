/*
TODO:
2) map if -> then to implies
4) parenthesis the output
5) implement fault tolarance
7) add stemming
10) pretty print the output
11) comma sytax when dealing with variables
12) handle arbitary text anywhere in the string (use the lexer to just ignore the tokens), perhaps use the model to identify tokens that are neither variables nor tokens
13) automatically generate the stemmed tokens by passing in the lexer some settings or a factory approach
14) recursively action
15) check operator precedence
*/

import * as chevrotain from 'chevrotain'
import * as natural from 'natural'
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
let Eq = generateStemmedTokenDefinition("Eq", ["is equal to", "is same as", "equal", "is"])
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
let allowedTokens = [WhiteSpace, IntegerLiteral, If, Then, GThan, LThan, GThanEq, LThanEq, Eq, And, Or, Implies, Eventually, Always,Next, Not, Upto, Until, WUntil, Release, Identifier]

const configuration: IParserConfig = {
    recoveryEnabled: true
}

function generateStemmedTokenDefinition(id: string, synonyms: string[]): TokenConstructor {
    return extendToken(id, RegExp(synonyms.map(natural.PorterStemmer.stem).join('|'), "i"));
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
            type: "IfFormula",
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
            operator: relationalOperator,
            left: identifier,
            right: integerLiteral
        }
    })

    private relationalOperator = this.RULE("relationalOperator", () => {
        return {
            type: "relationalOperator",
            value: this.OR([{
                ALT: () => {
                    return this.CONSUME(GThan)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(LThan)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(GThanEq)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Eq)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(LThanEq)
                }
            }])
        }
    });

    private binaryOperator = this.RULE("binaryOperator", () => {
        return {
            type: "binaryOperator",
            value: this.OR([{
                ALT: () => {
                    return this.CONSUME(And)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Or)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Implies)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Upto)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Until)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Release)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(WUntil)
                }
            }])
        }
    });

    private unaryOperator = this.RULE("unaryOperator", () => {
        return {
            type: "unaryOperator",
            value: this.OR([{
                ALT: () => {
                    return this.CONSUME(Not)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Next)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Always)
                }
            }, {
                ALT: () => {
                    return this.CONSUME(Eventually)
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

    private preprocessSentence(sentence: string): string {
        // to lowercase and apply stemming
        return natural.PorterStemmer.stem(sentence.toLocaleLowerCase())
    }

    static parse(sentence: string) {
        sentence = natural.PorterStemmer.stem(sentence.toLocaleLowerCase())
        var lexedTokens = (new Lexer(allowedTokens)).tokenize(sentence).tokens
        var parser = new NLParser(lexedTokens)
        return {
            AST: parser.formula(),
            errors: parser.errors
        }
    }
}

