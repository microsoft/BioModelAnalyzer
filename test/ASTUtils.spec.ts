import { expect } from 'chai'
import { default as NLParser, ParserResponseType } from '../src/NLParser/NLParser'
import { toAPIString, toStatesAndFormula, simplifyAST } from '../src/NLParser/ASTUtils'
import { ModelFile, Ltl } from '../src/BMA'

let testModel: ModelFile = require('./data/testmodel.json')
let ltlMultipleStates: Ltl = require('./data/ltl-multiple-states.json')
let ltlMultipleVariables: Ltl = require('./data/ltl-multiple-variables.json')

describe('ASTUtils', () => {
    describe('#toAPIString', () => {
        it('returns correct API formula format', () => {        
            var sentence = "give me some simulation where it is always the case that if x is 1 then y is 5 and followed by z is 25"
            var parserResponse = NLParser.parse(sentence, testModel)
            var expected = "(Always (Implies (= 3 1) (And (= 2 5) (Next (= 5 25)))))"
            expect(toAPIString(parserResponse.AST, testModel)).to.equal(expected)
        })
    })

    describe('#toStatesAndFormula', () => {
        it('generates multiple states', () => {
            var sentence = "give me some simulation where it is always the case that if x is 1 then y is 5 and followed by z is 25"
            var parserResponse = NLParser.parse(sentence, testModel)
            let ltl = toStatesAndFormula(parserResponse.AST, testModel)
            expect(ltl).to.deep.equal(ltlMultipleStates)

            // debug output
            /*
            let newModel: ModelFile = JSON.parse(JSON.stringify(testModel))
            newModel.ltl = ltl
            console.log(JSON.stringify(newModel, null, 2))
            process.exit()
            */
        })
        it('generates states with multiple variables', () => {
            var sentence = "give me some simulation where it is always the case that x is 1 and y is 5"
            var parserResponse = NLParser.parse(sentence, testModel)
            var expected = "(Always (And (= 3 1) (= 2 5)))"
            expect(toAPIString(parserResponse.AST, testModel)).to.equal(expected)

            let ltl = toStatesAndFormula(parserResponse.AST, testModel)
            expect(ltl).to.deep.equal(ltlMultipleVariables)
        })
    })
})
