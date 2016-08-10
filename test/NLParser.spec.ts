import * as chai from 'chai'
import NLParser from '../src/NLParser'
var expect = chai.expect;

describe('token parsing', function () {
    it('parse() should return an AST (Abstract Syntax Tree) for a correct set of input tokens', () => {
        var sentence = "a=1 and b=2"
        var parserResponse = NLParser.parse(sentence)
        var expected = {
            "type": "ltlFormula",
            "left": {
                "type": "ltlFormula",
                "left": {
                    "type": "expression",
                    "left": {
                        "type": "term",
                        "left": {
                            "type": "factor",
                            "left": {
                                "type": "factor",
                                "left": {
                                    "type": "relationalExpression",
                                    "left": "a",
                                    "right": 1
                                }
                            }
                        },
                        "right": {
                            "type": "term",
                            "left": {
                                "type": "factor",
                                "left": {
                                    "type": "factor",
                                    "left": {
                                        "type": "relationalExpression",
                                        "left": "b",
                                        "right": 2
                                    }
                                }
                            },
                            "right": null
                        },
                        "value": {
                            "type": "binaryOperator",
                            "value": {
                                "image": "and",
                                "offset": 4,
                                "startLine": 1,
                                "startColumn": 5,
                                "endLine": 1,
                                "endColumn": 7,
                                "isInsertedInRecovery": false
                            }
                        }
                    },
                    "right": null
                }
            }
        }
        expect(JSON.stringify(parserResponse.AST)).to.equal(JSON.stringify(expected))
    })
    it('parse() should prepend trailing unary operators maintaining their ordering', () => {
        var sentence = "a=1 and b=2 happen eventually"
        var parserResponse = NLParser.parse(sentence)
        var expected = {
            "type": "ltlFormula",
            "left": {
                "type": "unaryOperator",
                "value": {
                    "image": "eventu",
                    "offset": 19,
                    "startLine": 1,
                    "startColumn": 20,
                    "endLine": 1,
                    "endColumn": 25,
                    "isInsertedInRecovery": false
                },
                "left": {
                    "type": "ltlFormula",
                    "left": {
                        "type": "expression",
                        "left": {
                            "type": "term",
                            "left": {
                                "type": "factor",
                                "left": {
                                    "type": "factor",
                                    "left": {
                                        "type": "relationalExpression",
                                        "left": "a",
                                        "right": 1
                                    }
                                }
                            },
                            "right": {
                                "type": "term",
                                "left": {
                                    "type": "factor",
                                    "left": {
                                        "type": "factor",
                                        "left": {
                                            "type": "relationalExpression",
                                            "left": "b",
                                            "right": 2
                                        }
                                    }
                                },
                                "right": null
                            },
                            "value": {
                                "type": "binaryOperator",
                                "value": {
                                    "image": "and",
                                    "offset": 4,
                                    "startLine": 1,
                                    "startColumn": 5,
                                    "endLine": 1,
                                    "endColumn": 7,
                                    "isInsertedInRecovery": false
                                }
                            }
                        },
                        "right": null
                    }
                }
            }
        }
        expect(JSON.stringify(parserResponse.AST)).to.equal(JSON.stringify(expected))
    })
    it('parse() should return an error set for an invalid set of input tokens', () => {
        var sentence = "if a=1 and c=1"
        var parserResponse = NLParser.parse(sentence)
        var expected = '[{"name":"NoViableAltException","message":"Expecting: one of these possible Token sequences:\\n  <[GThan] ,[LThan] ,[GThanEq] ,[Eq] ,[LThanEq]>  but found: \'1\'","token":{"image":"1","offset":5,"startLine":1,"startColumn":6,"endLine":1,"endColumn":6,"isInsertedInRecovery":false},"resyncedTokens":[],"context":{"ruleStack":["formula","ifFormula","expression","term","factor","relationalExpression","relationalOperator"],"ruleOccurrenceStack":[1,1,1,1,1,1,1]}},{"name":"NoViableAltException","message":"Expecting: one of these possible Token sequences:\\n  <[GThan] ,[LThan] ,[GThanEq] ,[Eq] ,[LThanEq]>  but found: \'1\'","token":{"image":"1","offset":13,"startLine":1,"startColumn":14,"endLine":1,"endColumn":14,"isInsertedInRecovery":false},"resyncedTokens":[],"context":{"ruleStack":["formula","ifFormula","expression","term","factor","relationalExpression","relationalOperator"],"ruleOccurrenceStack":[1,1,1,1,2,1,1]}},{"name":"MismatchedTokenException","message":"Expecting token of type --> Then <-- but found --> \'\' <--","token":{"image":"","offset":-1,"startLine":-1,"startColumn":-1,"endLine":-1,"endColumn":-1,"isInsertedInRecovery":false},"resyncedTokens":[],"context":{"ruleStack":["formula","ifFormula"],"ruleOccurrenceStack":[1,1]}}]'
        expect(JSON.stringify(parserResponse.errors)).to.equal(expected)
    })
});
