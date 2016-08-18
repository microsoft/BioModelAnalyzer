import * as assert from 'assert'
import * as BMA from '../src/BMA'

let testmodel: BMA.ModelFile = require('./data/testmodel.json')

describe('BMA module', () => {
    describe('#getExpandedFormula', () => {
        it('should expand correctly', () => {
            let states = [
                new BMA.LtlStateImpl('A', [{
                    variable: 'x',
                    operator: '=',
                    value: 0
                }, {
                    variable: 'y',
                    operator: '>=',
                    value: 1
                }, {
                    variable: 'z',
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
            let exp1 = BMA.getExpandedFormula(testmodel.Model, states, formula1)
            let exp2 = BMA.getExpandedFormula(testmodel.Model, states, formula2)

            assert.equal(exp1, '(EVENTUALLY (ALWAYS (AND (AND (= 3 0) (>= 2 1)) (!= 5 1))))')
            assert.equal(exp2, '(NOT (AND (AND (= 3 0) (>= 2 1)) (!= 5 1)))')
        })
    })
})