import * as assert from 'assert'
import * as BMA from '../src/BMA'
import * as BMAApi from '../src/BMAApi'

let testmodel: BMA.ModelFile = require('./data/testmodel.json')

let states1 = [
    new BMA.LtlStateImpl('A', [{
        variableName: 'x',
        variableId: 3,
        operator: '=',
        value: 0
    }, {
        variableName: 'y',
        variableId: 2,
        operator: '>=',
        value: 1
    }, {
        variableName: 'z',
        variableId: 5,
        operator: '!=',
        value: 1
    }])
]
let formula1 = new BMA.LtlOperationImpl('EVENTUALLY', [
    new BMA.LtlOperationImpl('ALWAYS', [
        new BMA.LtlNameStateReferenceImpl('A')
    ])
])
let formula2 = new BMA.LtlOperationImpl('NOT', [
    new BMA.LtlNameStateReferenceImpl('A')
])

describe('BMAApi module', () => {
    describe('#getExpandedFormula', () => {
        it('should expand correctly', () => {
            let exp1 = BMAApi.getExpandedFormula(testmodel.Model, states1, formula1)
            let exp2 = BMAApi.getExpandedFormula(testmodel.Model, states1, formula2)

            assert.equal(exp1, '(Eventually (Always (And (And (= 3 0) (>= 2 1)) (!= 5 1))))')
            assert.equal(exp2, '(Not (And (And (= 3 0) (>= 2 1)) (!= 5 1)))')
        })
    })
    describe('#runFastSimulation', () => {
        it('should return the correct simulation response (basic)', () => {
            return BMAApi.runFastSimulation(testmodel.Model, '(Eventually True)', 10).then(response => {
                assert.strictEqual(response.Status, true)
            })
        })
        it('should return the correct simulation response (advanced)', () => {
            return BMAApi.runFastSimulation(testmodel.Model, '(Eventually (Always (And (And (= 3 0) (>= 2 1)) (!= 5 1))))', 10).then(response => {
                assert.strictEqual(response.Status, false)
            })
        })
    })
    describe('#runThoroughSimulation', () => {
        it('should return the correct polarity response', () => {
            let formula = '(Eventually True)'
            return BMAApi.runFastSimulation(testmodel.Model, formula, 10).then(resp1 => {
                assert.strictEqual(resp1.Status, true)
                return BMAApi.runThoroughSimulation(testmodel.Model, formula, 10, resp1).then(resp2 => {
                    assert.strictEqual(resp2.Status, false)
                })
            })
        })
    })
})