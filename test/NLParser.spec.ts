import * as chai from 'chai'
import NLParser from '../src/NLParser/NLParser'
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
                                    "value": { type: "relationalOperator", value: "=" },
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
                                        "value": { type: "relationalOperator", value: "=" },
                                        "left": "b",
                                        "right": 2
                                    }
                                }
                            },
                            "right": null
                        },
                        "value": {
                            "type": "binaryOperator",
                            "value": "and"
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
                "value": "eventually",
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
                                        "value": { type: "relationalOperator", value: "=" },
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
                                            "value": { type: "relationalOperator", value: "=" },
                                            "left": "b",
                                            "right": 2
                                        }
                                    }
                                },
                                "right": null
                            },
                            "value": {
                                "type": "binaryOperator",
                                "value": "and"
                            }
                        },
                        "right": null
                    }
                }
            }
        }
        expect(JSON.stringify(parserResponse.AST)).to.equal(JSON.stringify(expected))
    })
    it('parse() should re-write if (expression) then (expression) to:  expression implies expression', () => {
        var sentence = "eventually if a=1 and c=1 then d=1"
        var parserResponse = NLParser.parse(sentence)
        var expected = {
            "type": "ltlFormula",
            "left": {
                "type": "ltlFormula",
                "left": {
                    "type": "unaryOperator",
                    "value": "eventually",
                    "left": {
                        "type": "expression",
                        "value": {
                            "type": "binaryOperator",
                            "value": "implies"
                        },
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
                                            "value": {
                                                "type": "relationalOperator",
                                                "value": "="
                                            },
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
                                                "value": {
                                                    "type": "relationalOperator",
                                                    "value": "="
                                                },
                                                "left": "c",
                                                "right": 1
                                            }
                                        }
                                    },
                                    "right": null
                                },
                                "value": {
                                    "type": "binaryOperator",
                                    "value": "and"
                                }
                            },
                            "right": null
                        },
                        "right": {
                            "type": "expression",
                            "left": {
                                "type": "term",
                                "left": {
                                    "type": "factor",
                                    "left": {
                                        "type": "factor",
                                        "left": {
                                            "type": "relationalExpression",
                                            "value": {
                                                "type": "relationalOperator",
                                                "value": "="
                                            },
                                            "left": "d",
                                            "right": 1
                                        }
                                    }
                                },
                                "right": null
                            },
                            "right": null
                        }
                    }
                }
            }
        }
        expect(JSON.stringify(parserResponse.AST)).to.equal(JSON.stringify(expected))
    })
    it('parse() should return an error set for an invalid set of input tokens', () => {
        var sentence = "if a=1 and c=1"
        var parserResponse = NLParser.parse(sentence)
        var expected = '[{"name":"MismatchedTokenException","message":"Expecting --> then <-- but found --> \'\' <--","token":{"image":"","offset":-1,"startLine":-1,"startColumn":-1,"endLine":-1,"endColumn":-1,"isInsertedInRecovery":false},"resyncedTokens":[],"context":{"ruleStack":["formula","ifFormula"],"ruleOccurrenceStack":[1,1]}}]'
        expect(JSON.stringify(parserResponse.errors)).to.equal(expected)
    })
});
