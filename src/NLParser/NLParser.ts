/// <reference path="../../node_modules/chevrotain/lib/chevrotain.d.ts" />

/**
 *  Please read ./NLParserDocumentation.md for a high level explaination of the parser
 */
import { Parser, Token, IParserConfig, Lexer, TokenConstructor, extendToken } from 'chevrotain'
import * as _ from 'underscore'
import * as natural from 'natural'
import * as AST from './AST'
import * as ASTUtils from './ASTUtils'
/** 
 *  Parser response structure
 */
export enum ParserResponseType {
    SUCCESS,
    PARSE_ERROR,
    UNKNOWN_VARIABLES_FOUND
}

export interface ParserResponse {
    responseType: ParserResponseType
    humanReadableFormula?: string
    errors?: any
    AST?: any
}
/**
 *  Enumeration of the possible non-literal tokens in the grammar]
 */
export enum TokenType {
    /** variables found in the passed in model, encoded as: MODELVAR(K) where K is the variable id */
    MODELVAR,
    /** operators with 2 arguments eg: a=1 and b=2 */
    BINARY_OPERATOR,
    /** operators with 1 argument eg: eventually(a=1) */
    UNARY_OPERATOR,
    /** operators comparing values eg: a > 1 */
    ARITHMETIC_OPERATOR,
    /** grammar specefic tokens eg: if/then  */
    GRAMMAR_CONSTRUCT
}
/*
 *  GRAMMAR TOKENS: The set of tokens that are accepted by the grammar of our language,
 *  where each terminal token is augmented with a set of synonyms that can also be matched in the input token stream
 */
let If = generateStemmedTokenDefinition("If", "if", ["if"], TokenType.GRAMMAR_CONSTRUCT)
let Then = generateStemmedTokenDefinition("Then", "then", ["then"], TokenType.GRAMMAR_CONSTRUCT)

//Arithmetic operator tokens
let GThan = generateStemmedTokenDefinition("GThan", ">", [">", "greater than", "bigger than"], TokenType.ARITHMETIC_OPERATOR)
let LThan = generateStemmedTokenDefinition("LThan", "<", ["<", "less than", "smaller than"], TokenType.ARITHMETIC_OPERATOR)
let GThanEq = generateStemmedTokenDefinition("GThanEq", ">=", [">=", "greater than or equal to", "bigger than or equal to"], TokenType.ARITHMETIC_OPERATOR)
let LThanEq = generateStemmedTokenDefinition("LThanEq", "<=", ["<=", "less than or equal to", "smaller than or equal to"], TokenType.ARITHMETIC_OPERATOR)
let Eq = generateStemmedTokenDefinition("Eq", "=", ["=", "is equal to", "is same as", "equal", "is"], TokenType.ARITHMETIC_OPERATOR)
let NotEq = generateStemmedTokenDefinition("NotEq", "!=", ["!=", "is not equal to", "is not same as", "not equal", "is not"], TokenType.ARITHMETIC_OPERATOR)

//Boolean operator tokens
let And = generateStemmedTokenDefinition("And", "and", ["and", "conjunction", "as well as", "also", "along with", "in conjunction with", "plus", "together with"], TokenType.BINARY_OPERATOR)
let Or = generateStemmedTokenDefinition("Or", "or", ["or"], TokenType.BINARY_OPERATOR)
let Implies = generateStemmedTokenDefinition("Implies", "implies", ["implies"], TokenType.BINARY_OPERATOR)
let Not = generateStemmedTokenDefinition("Not", "not", ["not"], TokenType.UNARY_OPERATOR)

// Temporal operator tokens
let Eventually = generateStemmedTokenDefinition("Eventually", "eventually", ["eventually", "finally", "in time", "ultimately", "after all", "at last", "some point", "in the long run", "in a while", "soon", "at the end", "sometime"], TokenType.UNARY_OPERATOR)
let Always = generateStemmedTokenDefinition("Always", "always", ["always", "invariably", "perpetually", "forever", "constantly"], TokenType.UNARY_OPERATOR)
let Next = generateStemmedTokenDefinition("Next", "next", ["next", "after", "then", "consequently", "afterwards", "subsequently", "followed by", "after this"], TokenType.UNARY_OPERATOR)
let Upto = generateStemmedTokenDefinition("Upto", "upto", ["upto"], TokenType.BINARY_OPERATOR)
let Until = generateStemmedTokenDefinition("Until", "until", ["until"], TokenType.BINARY_OPERATOR)
let WUntil = generateStemmedTokenDefinition("WUntil", "weak until", ["weak until"], TokenType.BINARY_OPERATOR)
let Release = generateStemmedTokenDefinition("Release", "release", ["release"], TokenType.BINARY_OPERATOR)

/**  literals (no stemming required) */
class IntegerLiteral extends Token {
    static PATTERN = /\d+/
}

/**  Model variable token: in the form MODELVAR(varId) (no stemming required) */
class ModelVariable extends Token {
    static PATTERN = /(MODELVAR)(\()(\d+)(\))/
    static TokenType = TokenType.MODELVAR
}

/**  Ignored tokens : We ignore whitespaces as token boundaries are defined using the token set and can be processed by the lexer accordingly */
class WhiteSpace extends Token {
    static PATTERN = /\s+/
    static GROUP = Lexer.SKIPPED
}

/**
 *  Token groups for accessibility
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
let ALLOWED_TOKENS = (<typeof Token[]> IGNORE)
    .concat(LITERALS)
    .concat(CONSTRUCTS)
    .concat(ARITHMETIC_OPERATORS)
    .concat(BOOLEAN_OPERATORS)
    .concat(TEMPORAL_OPERATORS)

/**
 *  Token Stemming: As part of initialisation, each token is stemmed as we perform stemming on the input sentence. 
 *  This allows input tokens such as : "eventual","eventually" to be matched with the same token "eventually". Example execution: 
 * 
 *  input: let Eventually = generateStemmedTokenDefinition("Eventually", "eventually", ["eventually", "finally", ...],TokenType.UNARY_OPERATOR)
 *  1) each synonym in the synonym set is stemmed 
 *  2) the synonym set is mapped to a regex pattern ie: (eventu | final ...)
 *  3) we use the chevrotain function extendToken to generate a TokenConstructor
 *  4) we augment the generated TokenConstructor with static properties that are used in later processing
 */
function generateStemmedTokenDefinition(id: string, label: string, synonyms: string[], tokenType: TokenType): typeof Token {
    let stemmedSynonyms = synonyms.map(s => s.split(" ").map(natural.PorterStemmer.stem).join(" "))
    //We require explicit token boundaries on binary tokens to ensure input strings do not get match with tokens that are substrings eg: notch and not
    let pattern = RegExp(tokenType == TokenType.BINARY_OPERATOR ? "(\\b)(" + stemmedSynonyms.join('|') + ")(\\b)" : stemmedSynonyms.join('|'), "i")

    let tokenClass = class extends Token {
        static PATTERN = pattern
        static LABEL = label

        // custom properties
        static TOKEN_TYPE = tokenType
        static NON_STEMMED_SYNONYMS = synonyms
    }
    Object.defineProperty(tokenClass.prototype.constructor, 'name', {value: id})
    return tokenClass
}
/**
 *  NLParser: This class implicitly defines the grammar of the language based on the structure of the RULE,MANY,OR,SUBRULE and CONSUME operations.
 *  The operator precedence is explicitly defined by the level at which the assosiated rule is defined in the hierarchy
 */
export default class NLParser extends Parser {

    /** Base entry rule of the grammar */
    private formula = this.RULE("formula", () => {
        let ltlFormula
        let tree = {
            type: "ltlFormula",
            left: null
        }
        var subtree = tree

        /** Zero or one unary operators */
        this.MANY(() => {
            subtree.left = this.SUBRULE(this.unaryOperator)
            subtree = subtree.left
        })

        /** First child production */
        subtree.left = this.OR([{
            ALT: () => this.SUBRULE(this.conditionalsExpression)
        }, {
            ALT: () => this.SUBRULE(this.disjunctionExpression)
        }])

        var trailingTree, lastTrailingNode

        //TODO: MOVE TO ASTUTILS as this can be done more easily when parsing the AST instead of at construction time
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
    }, {     /**
             *  Resync Root: This is invoked whenever an unexpected token is encountered, the parser returns a set of "resynched" tokens 
             *  that are possible tokens less error token encountered that could be successfully parsed
             */
            resyncEnabled: true, recoveryValueFunc: () => {
                let error: any = _.first(this.errors)
                return { resyncedToken: _.first(error.resyncedTokens), errorToken: error.token }
            }
        })

    /**
     *  Conditional Expression example: if x=1 then z=2
     */
    private conditionalsExpression = this.RULE<AST.ConditionalsExpression>("conditionalsExpression", () => {
        let conditionClause, body
        this.CONSUME(If)
        conditionClause = this.SUBRULE(this.disjunctionExpression)
        this.CONSUME(Then)
        body = this.SUBRULE2(this.disjunctionExpression)

        return {
            type: "conditionalsExpression",
            value: { type: 'impliesOperator', value: Implies.LABEL},
            left: conditionClause,
            right: body
        }
    })
    /**
     *  Base rule for all expressions as it has the lowest precedence.
     *  E.g.: (a=1 and b=1) (this is still a disjunctionExpression with an implicit disjunction)
     *        (a=1 or (a=1 and a=2))
     */
    private disjunctionExpression = this.RULE<AST.DisjunctionExpression>("disjunctionExpression", () => {
        let rhs: AST.ConjunctionExpression[] = []
        let operators = []

        let lhs = this.SUBRULE(this.conjunctionExpression)
        this.MANY(() => {
            this.CONSUME(Or)
            operators.push(Or)
            rhs.push(this.SUBRULE2(this.conjunctionExpression))
        })
        return NLParser.appendRHSOperatorTreeToExistingTree<AST.DisjunctionExpression>({
            type: "disjunctionExpression",
            left: lhs
        }, rhs, operators, "conjunctionExpression")
    })

    /**
     *  Conjunction expressions have higher precedence than disjunction expressions hence their order in the tree.
     *  These are of the form: (a=1 and b=2)
     */
    private conjunctionExpression = this.RULE<AST.ConjunctionExpression>("conjunctionExpression", () => {
        let rhs: AST.TemporalExpression[] = []
        let operators = []

        let lhs = this.SUBRULE(this.temporalExpression)
        this.MANY(() => {
            this.CONSUME(And)
            operators.push(And)
            rhs.push(this.SUBRULE2(this.temporalExpression))
        })
        return NLParser.appendRHSOperatorTreeToExistingTree<AST.ConjunctionExpression>({
            type: "conjunctionExpression",
            left: lhs
        }, rhs, operators, "temporalExpression")
    })

    /**
     *  Temporal expressions can be of the form: always(x=1), (always(x=1) until eventually(k=2)).
     *  Binary temporal operators have a higher precedence than logical binary operators.
     */
    private temporalExpression = this.RULE<AST.TemporalExpression>("temporalExpression", () => {
        let rhs: AST.AtomicExpression[] = []
        let operators = []

        let lhs = this.SUBRULE(this.atomicExpression)
        this.MANY(() => {
            operators.push(this.SUBRULE(this.binaryTemporalOperator))
            rhs.push(this.SUBRULE2(this.atomicExpression))
        })
        return NLParser.appendRHSOperatorTreeToExistingTree<AST.TemporalExpression>({
            type: "temporalExpression",
            left: lhs
        }, rhs, operators, "atomicExpression")
    })

    /**
     *  Atomic expressions eg: eventually(a=1)
     */
    private atomicExpression = this.RULE<AST.AtomicExpression>("atomicExpression", () => {
        let lastNode
        let tree: AST.AtomicExpression = {
            type: "atomicExpression",
            left: null
        }

        this.MANY(() => {
            let operator = this.SUBRULE(this.unaryOperator)
            let subTree = tree
            subTree.left = operator
            subTree = subTree.left
            lastNode = subTree
        })
        let relationalExpression = this.SUBRULE(this.relationalExpression)

        if (lastNode) {
            lastNode.left = relationalExpression
        } else {
            tree.left = relationalExpression
        }
        return tree
    })

    /**
     *  A single unit eg:  MODELVAR(1) = 1 where MODELVAR(1) is the encoding of the actual variable with id=1
     */
    private relationalExpression = this.RULE<AST.RelationalExpression>("relationalExpression", () => {
        // consume the model variable token ie:  MODELVAR(variableId)
        let image = this.CONSUME(ModelVariable).image
        // The model variables are always encoded in the form MODELVAR(variableId), 
        // which means the variable id will always be found at the 4th group in the RegExp.match results
        let modelVariableId = parseInt(image.match(new RegExp(ModelVariable.PATTERN))[3])
        // deconstruct the matched pattern to extract the variable id
        let relationalOperator = this.SUBRULE(this.relationalOperator)
        let integerLiteral = parseInt(this.CONSUME(IntegerLiteral).image)
        return {
            type: "relationalExpression",
            value: relationalOperator,
            left: { type: "modelVariable", value: modelVariableId },
            right: { type: "integerLiteral", value: integerLiteral }
        }
    })

    private binaryTemporalOperator = this.RULE<AST.BinaryTemporalOperator>("binaryTemporalOperator", () => {
        return {
            type: "binaryTemporalOperator",
            value: this.OR([{
                ALT: () => {
                    this.CONSUME(Until)
                    return Until.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(WUntil)
                    return WUntil.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Release)
                    return Release.LABEL
                }
            }, {
                ALT: () => {
                    this.CONSUME(Upto)
                    return Upto.LABEL
                }
            }]) as AST.BinaryTemporalOperatorSymbol
        }
    });
    private relationalOperator = this.RULE<AST.RelationalOperator>("relationalOperator", () => {
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
            }]) as AST.RelationalOperatorSymbol
        }
    });

    private unaryOperator = this.RULE<AST.UnaryOperator>("unaryOperator", () => {
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
            }]) as AST.UnaryOperatorSymbol
        }
    });

    //for internal purpose only
    private constructor(inputTokens: Token[]) {
        super(inputTokens, ALLOWED_TOKENS)
        // very important to call this after all the rules have been defined.
        // otherwise the parser may not work correctly as it will lack information
        // derived during the self analysis phase.
        Parser.performSelfAnalysis(this);
    }

    /**
     * Helper fuction to traverse and append the RHS to the existing tree when constructing the expression trees
     */
    private static appendRHSOperatorTreeToExistingTree<T extends AST.Node<any,any>> (tree: T, rhs: AST.Node<any,any>[], operators, childExpressionName: AST.NodeType): T {
        if (!_.isEmpty(rhs)) {
            let subtree = tree
            for (let i = 0; i < rhs.length; i++) {
                subtree.value = { value: operators[i].LABEL }
                subtree.right = {
                    type: childExpressionName,
                    left: rhs[i]
                }
                subtree = subtree.right
            }
        }
        return tree
    }

    /**
     *  Detects variable usage in the string using the supplied model, e.g. "a is 1",
     *  and encodes the variables as MODELVAR(k) where k is the id of the variable a
     *  and then performs stemming on the remaining tokens.
     */
    private static applyPreprocessing(sentence: string, bmaModel): string {
        var modelVariables = bmaModel.Model.Variables
        var modelVariableRelationOpRegex = new RegExp(
            "(" + _.pluck(modelVariables, "Name").join("|") + 
            ")(\\s*)(" + ARITHMETIC_OPERATORS.map((op) => op.NON_STEMMED_SYNONYMS.join("|")).join("|") +
            ")(\\s*\\d+)", "ig")
        var matchedGroups, variableTokens = [];
        while ((matchedGroups = modelVariableRelationOpRegex.exec(sentence)) !== null) {
            //The variable will always on the 1st index as the 0th index is the entire group and the variable is matched in the 1st group of the regex expression
            variableTokens.push({ offset: matchedGroups.index, name: matchedGroups[1], id: _.find(bmaModel.Model.Variables, (v: any) => v.Name === matchedGroups[1]).Id })
        }
        variableTokens.forEach(t => sentence = sentence.replace(new RegExp("\\b" + t.name + "\\b", "ig"), "MODELVAR(" + t.id + ")"))
        //stem the sentence
        return sentence.split(" ").map((t) => ModelVariable.PATTERN.test(t) ? t : natural.PorterStemmer.stem(t)).join(" ")
    }

    /**
     *  Main Parse routine: 
     */
    static parse(sentence: string, bmaModel, didResynchedBefore?: boolean): ParserResponse {
        sentence = NLParser.applyPreprocessing(sentence, bmaModel)
        //lex the sentence to get token stream where illegal tokens are ignored and returns a token stream
        let lexedTokens = (new Lexer(ALLOWED_TOKENS, true)).tokenize(sentence).tokens
        var parser = new NLParser(lexedTokens)
        //We perform parsing by execute the root rule
        var parserResponse = parser.formula()
        //handle parse response
        if (parserResponse.AST) {
            return {
                responseType: ParserResponseType.SUCCESS,
                humanReadableFormula: ASTUtils.toHumanReadableString(parserResponse.AST, bmaModel),
                AST: parserResponse.AST
            }
        } else if (parserResponse.resyncedToken) {
            //the parser failed to parse a token and return the set of tokens less the error token that can possibly be parsed 
            return handleResynchedTokens(parserResponse.resyncedToken, didResynchedBefore)
        } else {
            return createParseError()
        }

        function createParseError() {
            return {
                responseType: ParserResponseType.PARSE_ERROR,
                errors: parser.errors,
            }
        }

        /**
         *  We continue parsing the resynched tokens until we find a good parse, no tokens are left or no new resynched tokens are generated
         */
        function handleResynchedTokens(resyncedTokens: string, didResynchedBefore?: boolean): ParserResponse {
            //extract the part of the sentance starting from the first resynched token
            var currentResynched = sentence.substring(parserResponse.resyncedToken.offset, sentence.length);
            //check the newly generated suffix with the previously generated suffix in order to prevent an infinite loop
            if (didResynchedBefore) {
                return NLParser.parse(sentence.substring(0, parserResponse.errorToken.offset) + currentResynched, bmaModel, didResynchedBefore)
            } else {
                return NLParser.parse(currentResynched, bmaModel, true)
            }
        }
    }
}

