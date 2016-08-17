import * as config from 'config'
import * as Promise from 'promise'
import * as request from 'request'

const BACKEND_URL = config.get('BMA_BACKEND_URL')
 
/**
 * 
 * 
 * @param model The "Model" part of a BmaJsonModel.
 * @param formula An expanded formula to run, e.g. "(Eventually (And (= 10 1) SelfLoop))".
 */
export function runFastSimulation (model: BmaModel, formula: string, stepCount: number): Promise.IThenable<BmaJsonAnalyzeLTLSimulationResponse> {
    let url = BACKEND_URL + 'AnalyzeLTLSimulation'
    let req: BmaJsonAnalyzeLTLSimulationRequest = {
        Formula: formula,
        Number_of_steps: stepCount,
        Name: model.Name,
        Relationships: model.Relationships,
        Variables: model.Variables
    }

    return new Promise((resolve, reject) => {
        request.post(url, {
            body: JSON.stringify(req),
            json: true
        }, (error, response, body) => {
            if (error) {
                reject(error)
                return
            }
            let resp = body as BmaJsonAnalyzeLTLSimulationResponse
            resolve(resp)
        })
    })
}

export function runThoroughSimulation(model: BmaModel, formula: string, stepCount: number,
        fastSimulationResponse: BmaJsonAnalyzeLTLSimulationResponse): Promise.IThenable<BmaJsonAnalyzeLTLPolarityResponse> {
    let url = BACKEND_URL + 'AnalyzeLTLPolarity'
    let req: BmaJsonAnalyzeLTLPolarityRequest = {
        Formula: formula,
        Polarity: !fastSimulationResponse.Status,
        Number_of_steps: stepCount,
        Name: model.Name,
        Relationships: model.Relationships,
        Variables: model.Variables
    }

    return new Promise((resolve, reject) => {
        request.post(url, {
            body: JSON.stringify(req),
            json: true
        }, (error, response, body) => {
            if (error) {
                reject(error)
                return
            }
            let resp = body as BmaJsonAnalyzeLTLSimulationResponse
            resolve(resp)
        })
    })
}

// TODO unit test

/** Returns a formula in expanded string format that can be used in API calls. */
export function getExpandedFormula (model: BmaModel, states: BmaLtlState[], formula: BmaLtlOperation) {
    let op = new BmaLtlOperationImpl(formula)
    return '(' + doGetExpandedFormula(model, states, op) + ')'
}

function doGetExpandedFormula (model: BmaModel, states: BmaLtlState[], formula: BmaLtlFormulaImpl) {
    if (formula instanceof BmaLtlSelfLoopStateImpl) {
        return 'SelfLoop'
    } else if (formula instanceof BmaLtlOscillationStateImpl) {
        return 'Oscillation'
    } else if (formula instanceof BmaLtlTrueStateImpl) {
        return 'True'
    } else if (formula instanceof BmaLtlStateNameReferenceImpl) {
        let stateName = formula.name
        let state = states.filter(state => state.name === stateName)[0]
        let ops = state.operands
        let varId = name => model.Variables.filter(variable => variable.Name === name)[0].Id
        return ops.slice(1).reduce((l,r) =>
            `AND (${l}) (${r.operator} ${varId(r.leftOperand.name)} ${r.rightOperand['const']})`,
            `${ops[0].operator} ${varId(ops[0].leftOperand.name)} ${ops[0].rightOperand['const']}`)
    } else if (formula instanceof BmaLtlOperationImpl) {
        let ops = formula.operands
            .map(op => '(' + doGetExpandedFormula(model, states, op) + ')')
            .join(' ')
        return formula.operator.name + ops
    }
}

/**
 * The JSON format for an AnalyzeLTLSimulation API request.
 * 
 * This request is typically very fast (few seconds) and only
 * takes longer (1-2min) in certain edge cases.
 * 
 * The response of this request (BmaJsonAnalyzeLTLSimulationResponse)
 * can only assert one of the following two:
 * - The formula is true for some simulations.
 * - the formula is false for some simulations.
 * It does not say anything about the opposite case.
 * For that, a more thorough analysis has to be made with the
 * AnalyzeLTLPolarity request. This thorough analysis times out after
 * 1-2min in which case the only information available would come
 * from the AnalyzeLTLSimulation request.
 */
interface BmaJsonAnalyzeLTLSimulationRequest {
    /** LTL expanded formula, e.g. "(Eventually (And (= 10 1) SelfLoop))" */
    Formula: string

    /** integer */
    Number_of_steps: number

    // the following properties are an inlined BmaModel
    Name: string
    Variables: BmaVariable[]
    Relationships: BmaVariableRelationship[]
}

interface BmaJsonAnalyzeLTLSimulationResponse {
    /** 
     * True, if the formula is true for some simulations.
     * False, if the formula is false for some simulations.
     * 
     * The boolean does not imply anything about the opposite.
     * A AnalyzeLTLPolarity API request has to be made to determine
     * whether the formula is true (false) for all simulations,
     * or whether it is true for some and false for other simulations. 
     */
    Status: boolean

    /** If an error occurs, then this contains the error message. */
    Error?: string
    
    Ticks: BmaSimulationTick[]
    /** Not sure what this is used for. It's null even on errors. */
    
    ErrorMessages?: string[]
    DebugMessages?: string[]

    /** integer, not sure what this is */
    Loop: number
}

/**
 * The JSON format for an AnalyzeLTLPolarity API request.
 */
interface BmaJsonAnalyzeLTLPolarityRequest {
    /** LTL expanded formula, e.g. "(Eventually (And (= 10 1) SelfLoop))" */
    Formula: string

    /** integer */
    Number_of_steps: number

    /** 
     * This is the negated "Status" of the AnalyzeLTLSimulation response.
     * It determines what to check for.
     * 
     * If the initial result (AnalyzeLTLSimulation) was a trace that falsifies
     * then it is left to check whether it can be true and the other way around.
     */
    Polarity: boolean

    // the following properties are an inlined BmaModel
    Name: string
    Variables: BmaVariable[]
    Relationships: BmaVariableRelationship[]
}

interface BmaJsonAnalyzeLTLPolarityResponse extends BmaJsonAnalyzeLTLSimulationResponse {
    /**
     * See BmaJsonAnalyzeLTLSimulationResponse docs.
     */
    status: boolean
}

interface BmaSimulationTick {
    /** integer, first tick starts at 0 */
    Time: number

    Variables: BmaSimulationTickVariable[]
}

interface BmaSimulationTickVariable {
    /** Variable ID */
    Id: number

    /** integer */
    Lo: number

    /** integer */
    Hi: number
}

/**
 * The JSON model format of the BMA tool.
 * Note that this is different to the JSON format used for API calls. 
 */
interface BmaJsonModel {
    Model: BmaModel

    /** Model layout */
    Layout: any

    ltl: BmaLtl
}

interface BmaModel {
    Name: string
    Variables: BmaVariable[]
    Relationships: BmaVariableRelationship[]
}

interface BmaVariable {
    Id: number

    Name: string    

    /** Integer >= 0 */
    RangeFrom: number

    /** Integer >= 0 and >= RangeFrom */
    RangeTo: number

    /** Target function */
    Formula: string
}

interface BmaVariableRelationship {
    id: number

    /** Variable ID */
    FromVariable: number

    /** Variable ID */
    ToVariable: number

    /** "Activator" or "Inhibitor" */
    Type: string
}

interface BmaLtl {
    states: BmaLtlState[]
    operations: BmaLtlOperation[]
}

interface BmaLtlState {
    /** Keyframe */
    _type: string

    name: string
    description: string    
    operands: BmaLtlStateEquation[]
}

interface BmaLtlStateEquation {
    /** "KeyframeEquation" */
    _type: string

    leftOperand: BmaLtlStateNameOperand

    /** =, <, ... */
    operator: string

    rightOperand: BmaLtlStateConstOperand
}

interface BmaLtlStateNameOperand {
    /** "NameOperand" */
    _type: string

    /** Variable name */
    name: string
}

interface BmaLtlStateConstOperand {
    /** "ConstOperand" */
    _type: string

    /** An integer. */
    const: number
}

/** Root interface for all formula interfaces */
interface BmaLtlFormula {
    _type: string
}

/** An LTL formula made up of operators and state references. */
interface BmaLtlOperation extends BmaLtlFormula {
    /** "Operation" */
    _type: string

    operator: BmaLtlOperationOperator

    operands: BmaLtlFormula[]
}

interface BmaLtlOperationOperator {
    /** "ALWAYS", "NOT", "AND", "EVENTUALLY", ... */
    name: string

    /** Number of operands the operator accepts, 
     *  equals array length of "operands" in BmaLtlOperation */
    operandsCount: number
}

interface BmaLtlStateNameReference extends BmaLtlFormula {
    /** "Keyframe" */
    _type: string

    /** State name */
    name: string
}

interface BmaLtlSelfLoopState extends BmaLtlFormula {
    /** "SelfLoopKeyframe" */
    _type: string
}

interface BmaLtlOscillationState extends BmaLtlFormula {
    /** "OscillationKeyframe" */
    _type: string
}

interface BmaLtlTrueState extends BmaLtlFormula {
    /** "TrueKeyframe" */
    _type: string
}

/** This exists purely because interfaces do not exist at runtime */
class BmaLtlFormulaImpl implements BmaLtlFormula {
    _type: string
    constructor (formula: BmaLtlFormula) {
        this._type = formula._type
    }
}

/**
 * Wraps an untyped BmaLtlOperation object into a typed object.
 * This makes it easier to write algorithms on it which walk the nested tree
 */
class BmaLtlOperationImpl extends BmaLtlFormulaImpl implements BmaLtlOperation {
    operator: BmaLtlOperationOperator
    operands: BmaLtlFormulaImpl[]
    constructor (operation: BmaLtlOperation) {
        super(operation)
        this.operator = operation.operator
        this.operands = operation.operands.map(op => {
            switch (op._type) {
                case 'Operation': return new BmaLtlOperationImpl(op as BmaLtlOperation)
                case 'Keyframe': return new BmaLtlStateNameReferenceImpl(op as BmaLtlStateNameReference)
                case 'SelfLoopKeyframe': return new BmaLtlSelfLoopStateImpl(op as BmaLtlSelfLoopState)
                case 'OscillationKeyframe': return new BmaLtlOscillationStateImpl(op as BmaLtlOscillationState)
                case 'TrueKeyframe': return new BmaLtlTrueStateImpl(op as BmaLtlTrueState)
                default: throw new Error('Not implemented: ' + op._type)
            }
        })
    }
}

class BmaLtlStateNameReferenceImpl extends BmaLtlFormulaImpl implements BmaLtlStateNameReference {
    name: string
    constructor (state: BmaLtlStateNameReference) {
        super(state)
        this.name = state.name
    }
}

class BmaLtlSelfLoopStateImpl extends BmaLtlFormulaImpl implements BmaLtlSelfLoopState {}

class BmaLtlOscillationStateImpl extends BmaLtlFormulaImpl implements BmaLtlOscillationState {}

class BmaLtlTrueStateImpl extends BmaLtlFormulaImpl implements BmaLtlTrueState {}
