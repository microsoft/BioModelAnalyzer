/*
TODO:

21) later = eventually next (composite)
22) check operator precedence
25) blows up with empty string
26) check for variable usage outside range
27) support on/off for boolean variables 
28) limit post fixing to the closest expression
29) if/then implies check
30) redundandant operators eventually eventually 
*/

import * as chevrotain from 'chevrotain'
import * as _ from 'underscore'
import * as natural from 'natural'
import * as ASTUtils from './ASTUtils'
import { Parser, Token, IParserConfig, Lexer, TokenConstructor } from 'chevrotain'
var extendToken = chevrotain.extendToken;

export enum TokenType {
    MODELVAR,
    BINARY,
    UNARY,
    OTHER
}

/**
 *  GRAMMAR TOKENS
 */
let If = generateStemmedTokenDefinition("If", ["if"], TokenType.OTHER)
let Then = generateStemmedTokenDefinition("Then", ["then"], TokenType.OTHER)
/**
 * Arithmetic operator tokens
 */
let GThan = generateStemmedTokenDefinition("GThan", [">", "greater than", "bigger than"], TokenType.OTHER)
let LThan = generateStemmedTokenDefinition("LThan", ["<", "less than", "smaller than"], TokenType.OTHER)
let GThanEq = generateStemmedTokenDefinition("GThanEq", [">=", "greater than or equal to", "bigger than or equal to"], TokenType.OTHER)
let LThanEq = generateStemmedTokenDefinition("LThanEq", ["<=", "less than or equal to", "smaller than or equal to"], TokenType.OTHER)
let Eq = generateStemmedTokenDefinition("Eq", ["=", "is equal to", "is same as", "equal", "is"], TokenType.OTHER)
let NotEq = generateStemmedTokenDefinition("NotEq", ["!=", "is not equal to", "is not same as", "not equal", "is not"], TokenType.OTHER)
/** 
 *  Boolean operator tokens
 */
let And = generateStemmedTokenDefinition("And", ["and", "conjunction", "as well as", "also", "along with", "in conjunction with", "plus", "together with"], TokenType.BINARY)
let Or = generateStemmedTokenDefinition("Or", ["or", "either"], TokenType.BINARY)
let Implies = generateStemmedTokenDefinition("Implies", ["implies", "means"], TokenType.BINARY)
let Not = generateStemmedTokenDefinition("Not", ["not", "never"], TokenType.UNARY)
/**
 *  Temporal operator tokens
 */
let Eventually = generateStemmedTokenDefinition("Eventually", ["eventually", "finally", "in time", "ultimately", "after all", "at last", "some point", "in the long run", "in a while", "soon", "at the end", "sometime"], TokenType.UNARY)
let Always = generateStemmedTokenDefinition("Always", ["always", "invariably", "perpetually", "forever", "constantly"], TokenType.UNARY)
let Next = generateStemmedTokenDefinition("Next", ["next", "after", "then", "consequently", "afterwards", "subsequently", "followed by", "after this"], TokenType.UNARY)
let Upto = generateStemmedTokenDefinition("Upto", ["upto"], TokenType.BINARY)
let Until = generateStemmedTokenDefinition("Until", ["until"], TokenType.BINARY)
let WUntil = generateStemmedTokenDefinition("WUntil", ["weak until"], TokenType.BINARY)
let Release = generateStemmedTokenDefinition("Release", ["release"], TokenType.BINARY)
/**
 * literals (no stemming required)
 */
let IntegerLiteral = extendToken("IntegerLiteral", /\d+/);
let ModelVariable = extendToken("ModelVariable", new RegExp("(MODELVAR)" + "(\\()" + "(\\d+)" + "(\\))"));
ModelVariable.TokenType = TokenType.MODELVAR
/**
 *  Ignored tokens
 */
let WhiteSpace = extendToken("WhiteSpace", /\s+/);
WhiteSpace.GROUP = Lexer.SKIPPED

/**
 *  Token groups
 */
let IGNORE = [WhiteSpace]
let LITERALS = [ModelVariable, IntegerLiteral]
let CONSTRUCTS = [If, Then]
let ARITHMETIC_OPERATORS = [Eq, NotEq, LThanEq, GThanEq, GThan, LThan]
let BOOLEAN_OPERATORS = [And, Or, Implies, Not]
let TEMPORAL_OPERATORS = [Eventually, Always, Next, Upto, Until, WUntil, Release]
/**
 *  Explicit Token Precedence for Lexer (tokens with lower index have higher priority)
 */
let ALLOWED_TOKENS = IGNORE
    .concat(LITERALS)
    .concat(CONSTRUCTS)
    .concat(ARITHMETIC_OPERATORS)
    .concat(BOOLEAN_OPERATORS)
    .concat(TEMPORAL_OPERATORS)

export enum ParserResponseType {
    SUCCESS,
    NO_PARSABLE_TOKEN_FOUND,
    PARSE_ERROR,
    UNKNOWN_VARIABLES_FOUND
}

export interface ParserResponse {
    responseType: ParserResponseType
    humanReadableFormula?: string
    errors?: any
    AST?: any
}

function generateStemmedTokenDefinition(id: string, synonyms: string[], tokenType: TokenType, alternative?: TokenConstructor) {
    let stemmedSynonyms = synonyms.map((s) => s.split(" ").map(natural.PorterStemmer.stem).join(" "))
    var tokenFunction = extendToken(id, RegExp(tokenType === TokenType.BINARY ? "(\\b)(" + stemmedSynonyms.join('|') + ")(\\b)" : stemmedSynonyms.join('|'), "i"), alternative);
    tokenFunction.LABEL = synonyms[0]
    tokenFunction.TokenType = tokenType
    tokenFunction.NON_STEMMED_SYNONYMS = synonyms
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
                return this.SUBRULE(this.conditionalsExpression)
            }
        }, {
            ALT: () => {
                return this.SUBRULE(this.disjunctionExpression)
            }
        }])

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
            AST: {
                type: "ltlFormula",
                left: resultTree
            }
        }
    }, {
            resyncEnabled: true, recoveryValueFunc: () => {
                return { resyncedTokens: _.first(this.errors).resyncedTokens }
            }
        })

    private conditionalsExpression = this.RULE("conditionalsExpression", () => {
        let conditionClause, body
        this.CONSUME(If)
        conditionClause = this.SUBRULE(this.disjunctionExpression)
        this.CONSUME(Then)
        body = this.SUBRULE2(this.disjunctionExpression)

        return {
            type: "conditionalsExpression",
            value: Implies,
            left: conditionClause,
            right: body
        }
    })

    private disjunctionExpression = this.RULE("disjunctionExpression", () => {
        let tree, lhs, rhs = [], operators = []

        lhs = this.SUBRULE(this.conjunctionExpression);
        this.MANY(() => {
            this.CONSUME(Or)
            operators.push(Or)
            rhs.push(this.SUBRULE2(this.conjunctionExpression));
        })

        tree = {
            type: "disjunctionExpression",
            left: lhs,
            right: null
        }
        if (!_.isEmpty(rhs)) {
            var subtree = tree;
            for (var i = 0; i < rhs.length; i++) {
                subtree.value = operators[i];
                subtree.right = {
                    type: "conjunctionExpression",
                    left: rhs[i],
                    right: null
                }
                subtree = subtree.right
            }
        }
        return tree;
    })

    private conjunctionExpression = this.RULE("conjunctionExpression", () => {
        let tree, lhs, rhs = [], operators = []

        lhs = this.SUBRULE(this.temporalExpression);
        this.MANY(() => {
            this.CONSUME(And)
            operators.push(And)
            rhs.push(this.SUBRULE2(this.temporalExpression));
        })

        tree = {
            type: "conjunctionExpression",
            left: lhs,
            right: null
        }
        if (!_.isEmpty(rhs)) {
            var subtree = tree;
            for (var i = 0; i < rhs.length; i++) {
                subtree.value = operators[i];
                subtree.right = {
                    type: "atomicExpression",
                    left: rhs[i],
                    right: null
                }
                subtree = subtree.right
            }
        }
        return tree;
    })

    private temporalExpression = this.RULE("temporalExpression", () => {
        let tree, lhs, rhs = [], operators = []

        lhs = this.SUBRULE(this.atomicExpression);
        this.MANY(() => {
            operators.push(this.SUBRULE(this.binaryTemporalOperators))
            rhs.push(this.SUBRULE2(this.atomicExpression));
        })

        tree = {
            type: "temporalExpression",
            left: lhs,
            right: null
        }
        if (!_.isEmpty(rhs)) {
            var subtree = tree;
            for (var i = 0; i < rhs.length; i++) {
                subtree.value = operators[i];
                subtree.right = {
                    type: "temporalExpression",
                    left: rhs[i],
                    right: null
                }
                subtree = subtree.right
            }
        }
        return tree;
    })

    private atomicExpression = this.RULE("atomicExpression", () => {
        var lastNode, tree = {
            type: "atomicExpression",
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
            type: "atomicExpression",
            left: tree
        }
    })

    private relationalExpression = this.RULE("relationalExpression", () => {
        let modelVariableTokens, relationalOperator, integerLiteral
        //deconstruct the matched pattern to extract the variable id
        modelVariableTokens = this.CONSUME(ModelVariable).image.match(ModelVariable.PATTERN)
        relationalOperator = this.SUBRULE(this.relationalOperator)
        integerLiteral = parseInt(this.CONSUME(IntegerLiteral).image)
        return {
            type: "relationalExpression",
            value: relationalOperator,
            left: { type: TokenType.MODELVAR, id: parseInt(modelVariableTokens[3]) },
            right: integerLiteral
        }
    })


    private binaryTemporalOperators = this.RULE("binaryTemporalOperators", () => {
        return {
            type: "binaryTemporalOperators",
            value: this.OR([{
                ALT: () => {
                    this.CONSUME(Until)
                    return GThan.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(WUntil)
                    return LThan.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Release)
                    return GThanEq.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Upto)
                    return Eq.LABEL
                }
            }])
        }
    });
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
        super(inputTokens, ALLOWED_TOKENS, {
            recoveryEnabled: true
        })
        // very important to call this after all the rules have been defined.
        // otherwise the parser may not work correctly as it will lack information
        // derived during the self analysis phase.
        Parser.performSelfAnalysis(this);
    }

    private static applyPreprocessing(sentence: string, bmaModel): string {
        var processedSentence
        var modelVariables = bmaModel.Model.Variables
        var modelVariableRelationOpRegex = new RegExp("(" + _.pluck(modelVariables, "Name").join("|") + ")(\\s*)(" + ARITHMETIC_OPERATORS.map((op) => op.NON_STEMMED_SYNONYMS.join("|")).join("|") + ")(\\s*\\d+)", "ig");
        var matchedGroups, variableTokens = [];
        while ((matchedGroups = modelVariableRelationOpRegex.exec(sentence)) !== null) {
            //The variable will always on the 1st index as the 0th index is the entire group and the variable is matched in the 1st group of the regex expression
            variableTokens.push({ offset: matchedGroups.index, name: matchedGroups[1], id: _.find(bmaModel.Model.Variables, (v: any) => v.Name === matchedGroups[1]).Id })
        }
        variableTokens.forEach(t => sentence = sentence.replace(new RegExp("\\b" + t.name + "\\b", "ig"), "MODELVAR(" + t.id + ")"))
        //stem the sentence
        return sentence.split(" ").map((t) => ModelVariable.PATTERN.test(t) ? t : natural.PorterStemmer.stem(t)).join(" ")
    }

    static parse(sentence: string, bmaModel): ParserResponse {
        //parse the input string to find and identify model variables to denote them so there are no lexing conflicts
        sentence = this.applyPreprocessing(sentence, bmaModel)
        //lex the sentence to get token stream where illegal tokens are ignored
        let lexedTokens = (new Lexer(ALLOWED_TOKENS, true)).tokenize(sentence).tokens
        //parse the token stream
        var parser = new NLParser(lexedTokens)
        var parserResponse = parser.formula()
        if (parserResponse.AST) {
            return {
                responseType: ParserResponseType.SUCCESS,
                humanReadableFormula: ASTUtils.toHumanReadableString(parserResponse.AST, bmaModel),
                AST: parserResponse.AST
            }
        } else if (parserResponse.resyncedTokens) {
            return this.parse(sentence.substring(_.first(parserResponse.resyncedTokens).offset, sentence.length), bmaModel)
        } else {
            return {
                responseType: ParserResponseType.PARSE_ERROR,
                errors: parser.errors,
            }
        }
    }
}