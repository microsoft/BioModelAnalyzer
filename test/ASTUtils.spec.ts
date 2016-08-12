import * as chai from 'chai'
import NLParser from '../src/NLParser/NLParser'
var expect = chai.expect;

describe('parse AST to human readable string', function () {
    it('toHumanReadableString() should return a human readable formula that represents the AST', () => {
        var sentence = "show me a simulation where if x is 1 and y is equal to 1 then z is 1 never happen in the long run"
        var model = { "Model": { "Name": "model 1", "Variables": [{ "Name": "x", "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "y", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "z", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }] } }
        var parserResponse = NLParser.parse(sentence, model)
        var expected = "not(eventually(((x=1 and y=1) implies z=1)))"
        expect(JSON.stringify(parserResponse.humanReadableFormula)).to.equal(JSON.stringify(expected))
    })
});
