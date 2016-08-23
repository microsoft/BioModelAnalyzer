import { expect } from 'chai'
import { default as NLParser, ParserResponseType } from '../src/NLParser/NLParser'
import { toAPIString } from '../src/NLParser/ASTUtils'
import { ModelFile } from '../src/BMA'

describe('ASTUtils', () => {
    describe('#toAPIString', () => {
        it('returns correct API formula format', () => {
            var model = { "Model": { "Name": "model 1", "Variables": [{ "Name": "x", "Id": 1, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "y", "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "z", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }] } }
            var sentence = "give me some simulation where it is always the case that if x is 1 then y is 5 and followed by z is 25"
            var parserResponse = NLParser.parse(sentence, model)
            var expected = "(Always (Implies (= 1 1) (And (= 2 5) (Next (= 3 25)))))"
            expect(toAPIString(parserResponse.AST, model)).to.equal(expected)
        })
    })
})
