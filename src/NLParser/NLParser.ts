/// <reference path="../../node_modules/chevrotain/lib/chevrotain.d.ts" />

//1) handle error tokens in the rsynching logic

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
    errors?: any
    AST?: AST.Formula
}

export interface FormulaPointer {
    name: string
    id: number
}
/**
 *  Enumeration of the possible non-literal tokens in the grammar]
 */
export enum TokenType {
    /** formula poointer variables, encoded as: FORMULAPOINTER(K) where K is the variable id */
    FORMULAPOINTER,
    /** variables found in the passed in model, encoded as: MODELVAR(K) where K is the variable id */
    MODELVAR,
    /** operators with 2 arguments eg: a=1 and b=2 */
    BINARY_OPERATOR,
    /** operators with 1 argument eg: eventually(a=1) */
    UNARY_OPERATOR,
    /** operators comparing values eg: a > 1 */
    ARITHMETIC_OPERATOR,
    /** grammar specefic tokens eg: if/then  */
    GRAMMAR_CONSTRUCT,
    /** composite operator keyword eg: never => not(always(..))*/
    COMPOSITE_OPERATOR,
    /** developmental end state eg: self loop,oscillation*/
    DEVELOPMENTAL_END_STATE,
    /** activity state ie: active,on,off,high,low,maximum,minimum*/
    ACTIVITY_CLASS
}

/** Token class extended to support NLParser specefic properties */
class BaseToken extends Token {
    static PATTERN
    static LABEL
    // custom properties
    static TOKEN_TYPE
    static NON_STEMMED_SYNONYMS
}

class CompositeToken extends BaseToken {
    static REPLACEMENT_TOKENS
    static replacementTokensAsSubtrees = (tokenClass: typeof CompositeToken) => tokenClass.REPLACEMENT_TOKENS.map(rtokenClass => {
        return {
            type: AST.Type.UnaryOperator,
            value: rtokenClass.LABEL
        }
    })
}

/**  literals (no stemming required) */
class IntegerLiteral extends Token {
    static PATTERN = /\d+/
}

class TrueLiteral extends Token {
    static PATTERN = /true/
    static LABEL = "true"
}

class FalseLiteral extends Token {
    static PATTERN = /false/
}

/**  Model variable token: in the form MODELVAR(varId) (no stemming required) */
class ModelVariable extends Token {
    static PATTERN = /(MODELVAR)(\()(\d+)(\))/
    static TokenType = TokenType.MODELVAR
}

class FormulaPointerToken extends Token {
    static PATTERN = /(FORMULAPOINTER)(\()(\d+)(\))/
    static TokenType = TokenType.FORMULAPOINTER
}

/**  Ignored tokens : We ignore whitespaces as token boundaries are defined using the token set and can be processed by the lexer accordingly */
class WhiteSpace extends Token {
    static PATTERN = /\s+/
    static GROUP = Lexer.SKIPPED
}


/*
 *  GRAMMAR TOKENS: The set of tokens that are accepted by the grammar of our language,
 *  where each terminal token is augmented with a set of synonyms that can also be matched in the input token stream
 */
let If = generateStemmedTokenDefinition("If", "if", ["if"], TokenType.GRAMMAR_CONSTRUCT)
let Then = generateStemmedTokenDefinition("Then", "then", ["then"], TokenType.GRAMMAR_CONSTRUCT)

//Arithmetic operator tokens
let GThan = generateStemmedTokenDefinition("GThan", ">", [">", "is greater than", "is bigger than"], TokenType.ARITHMETIC_OPERATOR)
let LThan = generateStemmedTokenDefinition("LThan", "<", ["<", "is less than", "is smaller than"], TokenType.ARITHMETIC_OPERATOR)
let GThanEq = generateStemmedTokenDefinition("GThanEq", ">=", [">=", "is greater than or equal to", "is bigger than or equal to"], TokenType.ARITHMETIC_OPERATOR)
let LThanEq = generateStemmedTokenDefinition("LThanEq", "<=", ["<=", "is less than or equal to", "is smaller than or equal to"], TokenType.ARITHMETIC_OPERATOR)
let Eq = generateStemmedTokenDefinition("Eq", "=", ["=", "is equal to", "is same as", "equal", "is"], TokenType.ARITHMETIC_OPERATOR)
let NotEq = generateStemmedTokenDefinition("NotEq", "!=", ["!=", "is not equal", "is not same as", "not equal", "is not"], TokenType.ARITHMETIC_OPERATOR)

//Boolean operator tokens
let And = generateStemmedTokenDefinition("And", "and", ["and", "conjunction", "as well as", "also", "along with", "in conjunction with", "plus", "together with"], TokenType.BINARY_OPERATOR)
let Or = generateStemmedTokenDefinition("Or", "or", ["or"], TokenType.BINARY_OPERATOR)
let Implies = generateStemmedTokenDefinition("Implies", "implies", ["implies"], TokenType.BINARY_OPERATOR)
let Not = generateStemmedTokenDefinition("Not", "not", ["not"], TokenType.UNARY_OPERATOR)

// Temporal operator tokens
let Eventually = generateStemmedTokenDefinition("Eventually", "eventually", ["eventually", "finally", "ultimately", "after all", "at last", "at some point", "soon", "at the end", "sometime", "possible"], TokenType.UNARY_OPERATOR)
let Always = generateStemmedTokenDefinition("Always", "always", ["always", "invariably", "perpetually", "forever", "constantly"], TokenType.UNARY_OPERATOR)
let Next = generateStemmedTokenDefinition("Next", "next", ["next", "after", "then", "consequently", "afterwards", "subsequently", "followed by", "after this", "later", "thereafter", "directly after"], TokenType.UNARY_OPERATOR)
let Upto = generateStemmedTokenDefinition("Upto", "upto", ["upto"], TokenType.BINARY_OPERATOR)
let Until = generateStemmedTokenDefinition("Until", "until", ["until"], TokenType.BINARY_OPERATOR)
let WUntil = generateStemmedTokenDefinition("WUntil", "weak until", ["weak until"], TokenType.BINARY_OPERATOR)
let Release = generateStemmedTokenDefinition("Release", "release", ["release"], TokenType.BINARY_OPERATOR)

// Developmental end state tokens
let SelfLoop = generateStemmedTokenDefinition("SelfLoop", "SelfLoop", ["self loop", "stable loop", "fixed point", "fixpoint", "stable recursion", "end state", "stabilises"], TokenType.DEVELOPMENTAL_END_STATE)
let Oscillation = generateStemmedTokenDefinition("Oscillation", "Oscillation", ["loop", "oscillation", "unstable loop", "unstable recursion", "cycle"], TokenType.DEVELOPMENTAL_END_STATE)

//Composite tokens - these are replaced when parsing with the replacement array (where replacement is done based on the order of the items in the replacement array ie: Never => not(eventually(..)))
let Never = generateCompositeTokenDefinition("Never", "never", ["never", "impossible", "at no time"], TokenType.COMPOSITE_OPERATOR, [Always, Not])
let Later = generateCompositeTokenDefinition("Later", "later", ["later", "sometime in the future", "in the future", "sometime later", "after a while", "in the long run", "in a while"], TokenType.COMPOSITE_OPERATOR, [Next, Eventually])

//Activity classes
let Active = generateStemmedTokenDefinition("Active", "Active", ["active", "on"], TokenType.ACTIVITY_CLASS)
let InActive = generateStemmedTokenDefinition("InActive", "InActive", ["inactive", "off", "idle"], TokenType.ACTIVITY_CLASS)
let MaximumActivity = generateStemmedTokenDefinition("MaximumActivity", "MaximumActivity", ["most active", "most intense", "maximum activity", "maximally active", "extremely active", "most active", "most possible", "maximum", "max", "highest"], TokenType.ACTIVITY_CLASS)
let MinimumActivity = generateStemmedTokenDefinition("MinimumActivity", "MinimumActivity", ["least active", "least intense", "minimum activity", "minimally active", "least possible", "minimum", "min", "lowest"], TokenType.ACTIVITY_CLASS)
let HighActivity = generateStemmedTokenDefinition("HighActivity", "HighActivity", ["high activity", "highly active"], TokenType.ACTIVITY_CLASS)
let LowActivity = generateStemmedTokenDefinition("LowActivity", "LowActivity", ["low activity"], TokenType.ACTIVITY_CLASS)


/**
 *  Token groups for accessibility
 */
let IGNORE = [WhiteSpace]
let LITERALS = [FalseLiteral, TrueLiteral, ModelVariable, FormulaPointerToken, IntegerLiteral]
let DEVELOPMENTAL_END_STATES = [SelfLoop, Oscillation]
let CONSTRUCTS = [If, Then]
let ARITHMETIC_OPERATORS = [LThanEq, GThanEq, GThan, LThan, NotEq, Eq]
let BOOLEAN_OPERATORS = [And, Or, Implies, Not]
let TEMPORAL_OPERATORS = [Never, Later, Eventually, Always, Next, Upto, Until, WUntil, Release]
let ACTIVITY_CLASSES = [HighActivity, LowActivity, MinimumActivity, MaximumActivity, InActive, Active]
/**
 *  Explicit Token Precedence for Lexer (tokens with lower index have higher priority)
 */
let ALLOWED_TOKENS = (<typeof Token[]>IGNORE)
    .concat(LITERALS)
    .concat(ACTIVITY_CLASSES)
    .concat(CONSTRUCTS)
    .concat(DEVELOPMENTAL_END_STATES)
    .concat(ARITHMETIC_OPERATORS)
    .concat(BOOLEAN_OPERATORS)
    .concat(TEMPORAL_OPERATORS)

function generateCompositeTokenDefinition(id: string, label: string, synonyms: string[], tokenType: TokenType, replacementTokens?: typeof Token[]) {
    if (tokenType == TokenType.COMPOSITE_OPERATOR && (!replacementTokens || _.isEmpty(replacementTokens))) {
        throw Error("No replacement tokens found for composite token type")
    } else {
        let tokenClass = generateStemmedTokenDefinition(id, label, synonyms, tokenType)
        let compositeTokenClass = class extends CompositeToken {
            static REPLACEMENT_TOKENS = replacementTokens
        }
        for (var k in tokenClass) compositeTokenClass[k] = tokenClass[k];
        Object.defineProperty(compositeTokenClass.prototype.constructor, 'name', { value: id })
        return compositeTokenClass
    }
}

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

function generateStemmedTokenDefinition(id: string, label: string, synonyms: string[], tokenType): typeof BaseToken {
    let stemmedSynonyms = synonyms.map(s => s.split(" ").map(natural.PorterStemmer.stem).join(" "))
    //We require explicit token boundaries on binary tokens to ensure input strings do not get match with tokens that are substrings eg: notch and not
    let pattern = RegExp(tokenType == TokenType.BINARY_OPERATOR ? "(\\b)(" + stemmedSynonyms.join('|') + ")(\\b)" : stemmedSynonyms.join('|'), "i")

    let tokenClass = class extends BaseToken {
        static PATTERN = pattern
        static LABEL = label
        // custom properties
        static TOKEN_TYPE = tokenType
        static NON_STEMMED_SYNONYMS = synonyms
    }
    Object.defineProperty(tokenClass.prototype.constructor, 'name', { value: id })
    return tokenClass
}

/**
 *  NLParser: This class implicitly defines the grammar of the language based on the structure of the RULE,MANY,OR,SUBRULE and CONSUME operations.
 *  The operator precedence is explicitly defined by the level at which the assosiated rule is defined in the hierarchy
 */
export default class NLParser extends Parser {

    /** Base entry rule of the graar */
    private formula = this.RULE<AST.InternalFormula>("formula", () => {
        let ltlFormula
        let tree = {
            left: null
        }
        var subTree = tree
        /** Zero or one unary operators */
        this.MANY(() => {
            let unaryOperatorTree = NLParser.asUnaryExpressionNode(this.SUBRULE(this.compositeOperator))
            subTree.left = unaryOperatorTree.tree
            subTree = unaryOperatorTree.lastNode
        })
        /** First child production */
        subTree.left = this.OR<AST.ConditionalsExpression | AST.DisjunctionExpression | AST.DisjunctionExpressionChild>([{
            ALT: () => this.SUBRULE(this.conditionalsExpression)
        }, {
            ALT: () => this.SUBRULE(this.disjunctionExpression)
        }])

        // get rid of the first empty node
        tree = tree.left

        var trailingTree, lastTrailingNode

        //handle trailing operators
        this.MANY2(() => {
            var subTrailingTree = trailingTree
            let unaryOperatorTree = NLParser.asUnaryExpressionNode(this.SUBRULE2(this.compositeOperator))
            if (subTrailingTree) {
                subTrailingTree.left = unaryOperatorTree.tree
                subTrailingTree = unaryOperatorTree.lastNode
                lastTrailingNode = subTrailingTree
            } else {
                trailingTree = unaryOperatorTree.tree
                lastTrailingNode = unaryOperatorTree.lastNode
            }
        })
        let resultTree
        if (lastTrailingNode) {
            lastTrailingNode.left = tree
            resultTree = trailingTree
        } else {
            resultTree = tree
        }
        return {
            AST: resultTree
        }
    }, {     /**
             *  Resync Root: This is invoked whenever an unexpected token is encountered, the parser returns a set of "resynched" tokens 
             *  that are possible tokens less error token encountered that could be successfully parsed
             */
            resyncEnabled: true,
            recoveryValueFunc: () => {
                let error: any = _.first(this.errors)
                return { resyncedToken: _.first(error.resyncedTokens), errorToken: error.token }
            }
        })

    /**
     *  Conditional Expression example: if x=1 then z=2
     */
    private conditionalsExpression = this.RULE<AST.ConditionalsExpression>('conditionalsExpression', () => {
        let conditionClause, body
        this.CONSUME(If)
        conditionClause = this.SUBRULE(this.disjunctionExpression)
        this.CONSUME(Then)
        body = this.SUBRULE2(this.disjunctionExpression)

        return {
            type: AST.Type.ConditionalsExpression,
            value: { type: AST.Type.ImpliesOperator, value: Implies.LABEL as AST.ImpliesOperatorSymbol },
            left: conditionClause,
            right: body
        }
    })
    /**
     *  Base rule for all expressions as it has the lowest precedence.
     *  E.g.: (a=1 and b=1) (this is still a disjunctionExpression with an implicit disjunction)
     *        (a=1 or (a=1 and a=2))
     */
    private disjunctionExpression = this.RULE<AST.DisjunctionExpression | AST.DisjunctionExpressionChild>("disjunctionExpression", () => {
        let nodes: AST.DisjunctionExpressionChild[] = []
        let values = []

        nodes.push(this.SUBRULE(this.conjunctionExpression))
        this.MANY(() => {
            this.CONSUME(Or)
            values.push({
                type: AST.Type.DisjunctionOperator,
                value: Or.LABEL
            })
            nodes.push(this.SUBRULE2(this.conjunctionExpression))
        })
        return NLParser.asNestedTree<AST.DisjunctionExpression | AST.ConjunctionExpressionChild>("disjunctionExpression", nodes, values)
    })

    /**
     *  Conjunction expressions have higher precedence than disjunction expressions hence their order in the tree.
     *  These are of the form: (a=1 and b=2)
     */
    private conjunctionExpression = this.RULE<AST.ConjunctionExpression | AST.ConjunctionExpressionChild>("conjunctionExpression", () => {
        let nodes: AST.ConjunctionExpressionChild[] = []
        let values = []

        nodes.push(this.SUBRULE(this.temporalExpression))
        this.MANY(() => {
            this.CONSUME(And)
            values.push({
                type: AST.Type.ConjunctionOperator,
                value: And.LABEL
            })
            nodes.push(this.SUBRULE2(this.temporalExpression))
        })
        return NLParser.asNestedTree<AST.ConjunctionExpression | AST.ConjunctionExpressionChild>("conjunctionExpression", nodes, values)
    })

    /**
     *  Temporal expressions can be of the form: always(x=1), (always(x=1) until eventually(k=2)).
     *  Binary temporal operators have a higher precedence than logical binary operators.
     */
    private temporalExpression = this.RULE<AST.TemporalExpression | AST.AtomicExpression>("temporalExpression", () => {
        let nodes: AST.AtomicExpression[] = []
        let values = []

        nodes.push(this.SUBRULE(this.atomicExpression))
        this.MANY(() => {
            values.push(this.SUBRULE(this.binaryTemporalOperator))
            nodes.push(this.SUBRULE2(this.atomicExpression))
        })
        return NLParser.asNestedTree<AST.TemporalExpression | AST.AtomicExpression>("temporalExpression", nodes, values)
    })

    /**
     *  Atomic expressions eg: eventually(a=1)
     */
    private atomicExpression = this.RULE<AST.AtomicExpression>("atomicExpression", () => {
        let lastNode
        let tree = {
            left: null
        }
        let subTree = tree
        this.MANY(() => {
            let unaryOperatorTree = NLParser.asUnaryExpressionNode(this.SUBRULE(this.compositeOperator))
            subTree.left = unaryOperatorTree.tree
            subTree = unaryOperatorTree.lastNode
            lastNode = subTree
        })
        let rhs = this.OR<AST.ActivityExpression | AST.RelationalExpression | AST.FormulaPointer | AST.TrueLiteral | AST.UnaryExpression | AST.DevelopmentalEndState>([{
            ALT: () => this.SUBRULE(this.activityExpression)
        }, {
            ALT: () => this.SUBRULE(this.relationalExpression)
        }, {
            ALT: () => this.SUBRULE(this.formulaPointer)
        }, {
            ALT: () => this.SUBRULE(this.booleanLiteral)
        }, {
            ALT: () => this.SUBRULE(this.developmentalEndState)
        }])

        if (lastNode) {
            lastNode.left = rhs
            return tree.left
        } else {
            return rhs
        }
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
            type: AST.Type.RelationalExpression,
            value: relationalOperator,
            left: { type: AST.Type.ModelVariable, value: modelVariableId },
            right: { type: AST.Type.IntegerLiteral, value: integerLiteral }
        }
    })

    /**
    *  A single unit eg:  MODELVAR(1) = 1 where MODELVAR(1) is the encoding of the actual variable with id=1
    */
    private activityExpression = this.RULE<AST.ActivityExpression>("activityExpression", () => {
        // consume the model variable token ie:  MODELVAR(variableId)
        let image = this.CONSUME(ModelVariable).image
        // The model variables are always encoded in the form MODELVAR(variableId), 
        // which means the variable id will always be found at the 4th group in the RegExp.match results
        let modelVariableId = parseInt(image.match(new RegExp(ModelVariable.PATTERN))[3])
        //activity assignment ("only the 'is' makes sense, but support for the others is present nevertheless")
        this.OPTION(() => {
            this.CONSUME(Eq)
        })
        //activity classes
        let activityClass = this.OR([{
            ALT: () => {
                this.CONSUME(Active)
                return Active
            }
        }, {
            ALT: () => {
                this.CONSUME(InActive)
                return InActive
            }
        }, {
            ALT: () => {
                this.CONSUME(MaximumActivity)
                return MaximumActivity
            }
        }, {
            ALT: () => {
                this.CONSUME(MinimumActivity)
                return MinimumActivity
            }
        }, {
            ALT: () => {
                this.CONSUME(HighActivity)
                return HighActivity
            }
        }, {
            ALT: () => {
                this.CONSUME(LowActivity)
                return LowActivity
            }
        }]).LABEL

        return {
            type: AST.Type.ActivityExpression,
            value: activityClass,
            left: { type: AST.Type.ModelVariable, value: modelVariableId }
        }
    })


    /**
    *  A single unit eg:  FORMULAPOINTER(1) = 1 where FORMULAPOINTER(1) is the encoding of the actual variable with id=1
    */
    private formulaPointer = this.RULE<AST.FormulaPointer>("formulaPointer", () => {
        let image = this.CONSUME(FormulaPointerToken).image
        let formulaPointerId = parseInt(image.match(new RegExp(FormulaPointerToken.PATTERN))[3])
        return {
            type: AST.Type.FormulaPointer,
            value: formulaPointerId
        }
    })


    private booleanLiteral = this.RULE<AST.TrueLiteral | AST.UnaryExpression>("booleanLiteral", () => {
        let trueLiteralSubtree: AST.TrueLiteral = {
            type: AST.Type.TrueLiteral,
            value: TrueLiteral.LABEL as AST.TrueLiteralSymbol
        }
        let tokenClass = this.OR([{
            ALT: () => {
                this.CONSUME(TrueLiteral)
                return TrueLiteral
            }
        }, {
            ALT: () => {
                this.CONSUME(FalseLiteral)
                return FalseLiteral
            }
        }])
        if (tokenClass == TrueLiteral) {
            return trueLiteralSubtree
        } else {
            return {
                type: AST.Type.UnaryExpression,
                value: {
                    type: AST.Type.UnaryOperator,
                    value: Not.LABEL
                },
                left: trueLiteralSubtree
            }
        }
    })

    private developmentalEndState = this.RULE<AST.DevelopmentalEndState>("developmentalEndState", () => {
        let developmentalEndStateLabel = this.OR([{
            ALT: () => {
                this.CONSUME(SelfLoop)
                return SelfLoop
            }
        }, {
            ALT: () => {
                this.CONSUME(Oscillation)
                return Oscillation
            }
        }]).LABEL

        return {
            type: AST.Type.DevelopmentalEndState,
            value: developmentalEndStateLabel
        }
    })

    private binaryTemporalOperator = this.RULE<AST.BinaryTemporalOperator>("binaryTemporalOperator", () => {
        return {
            type: AST.Type.BinaryTemporalOperator,
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
            type: AST.Type.RelationalOperator,
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

    private compositeOperator = this.RULE("compositeOperator", () => {
        return this.OR([{
            ALT: () => [this.SUBRULE(this.unaryOperator)]
        }, {
            ALT: () => {
                this.CONSUME(Never)
                return CompositeToken.replacementTokensAsSubtrees(Never)
            }
        }, {
            ALT: () => {
                this.CONSUME(Later)
                return CompositeToken.replacementTokensAsSubtrees(Later)
            }
        }])
    });

    private unaryOperator = this.RULE<AST.UnaryOperator>("unaryOperator", () => {
        return {
            type: AST.Type.UnaryOperator,
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
                /** To handle the case where "then" is used as a unary operator eg: a=1 and then b=1 */
                ALT: () => {
                    this.CONSUME(Then)
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
            }]).LABEL as AST.UnaryOperatorSymbol
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

    private static asUnaryExpressionNode(unaryOperators) {
        let tree = {
            left: null
        }
        var lastNode = null
        var subtree = tree
        unaryOperators.forEach(op => {
            subtree.left = {
                type: AST.Type.UnaryExpression,
                value: op
            }
            subtree = subtree.left
            lastNode = subtree
        });
        //get rid of the first empty left node
        return { tree: tree.left, lastNode: lastNode }
    }

    /**
     * Helper fuction to traverse and append the RHS to the existing tree when constructing the expression trees
     */
    private static asNestedTree<T extends AST.Node<any, any>>(nodeType, nodes: AST.Node<any, any>[], operators): T {
        if (nodes.length === 1) {
            return nodes[0] as T
        }

        let tree = {
            type: nodeType,
            value: operators[0],
            left: nodes[0],
            right: null
        }
        let subtree = tree
        for (let i = 1; i < nodes.length; i++) {
            if (i === nodes.length - 1) {
                subtree.right = nodes[i]
            } else {
                subtree.right = {
                    type: nodeType,
                    value: operators[i],
                    left: nodes[i],
                    right: null
                }
            }
            subtree = subtree.right
        }

        return tree as any
    }

    /**
     *  Detects variable usage in the string using the supplied model, e.g. "a is 1",
     *  and encodes the variables as MODELVAR(k) where k is the id of the variable a
     *  and then performs stemming on the remaining tokens.
     */
    private static applySentencePreprocessing(sentence: string, bmaModel, formulaPointers?: FormulaPointer[]): string {
        let hasFormulaPointers = formulaPointers && !_.isEmpty(formulaPointers)
        let modelVariables = bmaModel.Model.Variables
        let modelVariableRelationOpRegex = "(" + _.pluck(modelVariables, "Name").join("|") +
            ")(\\s*)(" + ARITHMETIC_OPERATORS.map((op) => op.NON_STEMMED_SYNONYMS.join("|")).join("|") +
            ")(\\s*)"
        let modelVariableAndFormulaPointerRegex = new RegExp(hasFormulaPointers ? modelVariableRelationOpRegex + "|" + "\\b(" + _.pluck(formulaPointers, "name").join("|") + ")\\b" : modelVariableRelationOpRegex, "ig")

        var matchedGroups, variableTokens = [];
        while ((matchedGroups = modelVariableAndFormulaPointerRegex.exec(sentence)) !== null) {
            //The variable will always on the 1st index as the 0th index is the entire group and the variable is matched in the 1st group of the regex expression
            if (hasFormulaPointers && _.last(matchedGroups)) {
                let forumulaPointer = _.last(matchedGroups)
                variableTokens.push({ offset: matchedGroups.index, name: forumulaPointer, id: _.find(formulaPointers, (v: any) => v.name === forumulaPointer).id, type: FormulaPointerToken })
            } else {
                variableTokens.push({ offset: matchedGroups.index, name: matchedGroups[1], id: _.find(bmaModel.Model.Variables, (v: any) => v.Name.toLowerCase() === matchedGroups[1].toLowerCase()).Id, type: ModelVariable })
            }
        }
        //use the generated offsets to replace instances of variable usage with MODELVAR(k), where k is the model variable
        //variableTokens can be empty when processing a resynched token stream
        if (!_.isEmpty(variableTokens)) {
            var processedSentence
            for (var i = 0; i < variableTokens.length; i++) {
                let token = variableTokens[i]
                let encodedToken = token.type == FormulaPointerToken ? "FORMULAPOINTER(" + token.id + ")" : "MODELVAR(" + token.id + ")"
                if (i == 0) {
                    processedSentence = sentence.substring(0, token.offset) + encodedToken
                } else {
                    let prevToken = variableTokens[i - 1]
                    processedSentence += sentence.substring(prevToken.offset + prevToken.name.length, token.offset) + encodedToken
                }
            }
            //append the tail of the original sentence to the processed sentence
            let lastVariableToken = _.last(variableTokens)
            processedSentence += sentence.substring(lastVariableToken.offset + lastVariableToken.name.length, sentence.length)
            sentence = processedSentence
        }
        //stem the sentence
        return sentence.split(" ").map((t) => ModelVariable.PATTERN.test(t) || FormulaPointerToken.PATTERN.test(t) || TrueLiteral.PATTERN.test(t) || FalseLiteral.PATTERN.test(t) ? t : natural.PorterStemmer.stem(t)).join(" ")
    }

    /**
     *  Main Parse routine: 
     */
    static parse(sentence: string, bmaModel, formulaPointers?: FormulaPointer[], didResynchedBefore?: boolean): ParserResponse {
        sentence = NLParser.applySentencePreprocessing(sentence, bmaModel, formulaPointers)
        //lex the sentence to get token stream where illegal tokens are ignored and returns a token stream
        let lexedTokens = (new Lexer(ALLOWED_TOKENS, true)).tokenize(sentence).tokens
        var parser = new NLParser(lexedTokens)
        //We perform parsing by execute the root rule
        var parserResponse = parser.formula()
        //handle parse response
        if (parserResponse.AST) {
            return {
                responseType: ParserResponseType.SUCCESS,
                AST: parserResponse.AST
            }
        } else if (parserResponse.resyncedToken) {
            //the parser failed to parse a token and return the set of tokens less the error token that can possibly be parsed 
            return handleResynchedTokens(formulaPointers, didResynchedBefore)
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
        function handleResynchedTokens(formulaPointers?: FormulaPointer[], didResynchedBefore?: boolean): ParserResponse {
            //extract the part of the sentance starting from the first resynched token
            var currentResynched = sentence.substring(parserResponse.resyncedToken.offset, sentence.length);
            //check the newly generated suffix with the previously generated suffix in order to prevent an infinite loop
            if (didResynchedBefore) {
                return NLParser.parse(sentence.substring(0, parserResponse.errorToken.offset) + currentResynched, bmaModel, formulaPointers, didResynchedBefore)
            } else {
                return NLParser.parse(currentResynched, bmaModel, formulaPointers, true)
            }
        }
    }
}

