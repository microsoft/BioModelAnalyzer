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
export function runFastSimulation (model: Model, formula: string, stepCount: number): Promise.IThenable<AnalyzeLTLSimulationResponse> {
    let url = BACKEND_URL + 'AnalyzeLTLSimulation'
    let req: AnalyzeLTLSimulationRequest = {
        Formula: formula,
        Number_of_steps: stepCount,
        Name: model.Name,
        Relationships: model.Relationships,
        Variables: model.Variables
    }

    return new Promise((resolve, reject) => {
        request.post(url, {
            body: req,
            json: true,
        }, (error, response, body) => {
            if (error) {
                reject(error)
                return
            }
            let resp = body as AnalyzeLTLSimulationResponse
            if (resp.Error) {
                reject('API error: ' + resp.Error)
                console.error(resp.ErrorMessages)
                return
            }
            resolve(resp)
        })
    })
}

export function runThoroughSimulation(model: Model, formula: string, stepCount: number,
        fastSimulationResponse: AnalyzeLTLSimulationResponse): Promise.IThenable<AnalyzeLTLPolarityResponse> {
    let url = BACKEND_URL + 'AnalyzeLTLPolarity'
    let req: AnalyzeLTLPolarityRequest = {
        Formula: formula,
        Polarity: !fastSimulationResponse.Status,
        Number_of_steps: stepCount,
        Name: model.Name,
        Relationships: model.Relationships,
        Variables: model.Variables
    }

    return new Promise((resolve, reject) => {
        request.post(url, {
            body: req,
            json: true
        }, (error, response, body) => {
            if (error) {
                reject(error)
                return
            }
            let resp = body as AnalyzeLTLSimulationResponse
            if (resp.Error) {
                reject('API error: ' + resp.Error)
                console.error(resp.ErrorMessages)
                return
            }
            resolve(resp)
        })
    })
}

/** Returns a formula in expanded string format that can be used in API calls. */
export function getExpandedFormula (model: Model, states: LtlState[], formula: LtlOperation) {
    let op = LtlOperationImpl.from(formula)
    return '(' + doGetExpandedFormula(model, states, op) + ')'
}

function doGetExpandedFormula (model: Model, states: LtlState[], formula: LtlFormula) {
    if (formula instanceof LtlSelfLoopStateImpl) {
        return 'SelfLoop'
    } else if (formula instanceof LtlOscillationStateImpl) {
        return 'Oscillation'
    } else if (formula instanceof LtlTrueStateImpl) {
        return 'True'
    } else if (formula instanceof LtlNameStateReferenceImpl) {
        let stateName = formula.name
        let state = states.filter(state => state.name === stateName)[0]
        let ops = state.operands
        let varId = name => model.Variables.filter(variable => variable.Name === name)[0].Id
        return ops.slice(1).reduce((l,r) =>
            `And (${l}) (${r.operator} ${varId(r.leftOperand.name)} ${r.rightOperand['const']})`,
            `${ops[0].operator} ${varId(ops[0].leftOperand.name)} ${ops[0].rightOperand['const']}`)
    } else if (formula instanceof LtlOperationImpl) {
        let ops = formula.operands
            .map(op => '(' + doGetExpandedFormula(model, states, op) + ')')
            .join(' ')
        let opName = formula.operator.name
        // the API expects 'And', whereas the model object has 'AND'
        let opNameApi = opName[0] + opName.substr(1).toLowerCase()
        return opNameApi + ' ' + ops
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
interface AnalyzeLTLSimulationRequest {
    /** LTL expanded formula, e.g. "(Eventually (And (= 10 1) SelfLoop))" */
    Formula: string

    /** integer */
    Number_of_steps: number

    // the following properties are an inlined BmaModel
    Name: string
    Variables: Variable[]
    Relationships: VariableRelationship[]
}

interface AnalyzeLTLSimulationResponse {
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
    
    Ticks: SimulationTick[]
    /** Not sure what this is used for. It's null even on errors. */
    
    ErrorMessages?: string[]
    DebugMessages?: string[]

    /** integer, not sure what this is */
    Loop: number
}

/**
 * The JSON format for an AnalyzeLTLPolarity API request.
 */
interface AnalyzeLTLPolarityRequest {
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
    Variables: Variable[]
    Relationships: VariableRelationship[]
}

interface AnalyzeLTLPolarityResponse extends AnalyzeLTLSimulationResponse {
    /**
     * See BmaJsonAnalyzeLTLSimulationResponse docs.
     */
    Status: boolean
}

interface SimulationTick {
    /** integer, first tick starts at 0 */
    Time: number

    Variables: SimulationTickVariable[]
}

interface SimulationTickVariable {
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
export interface ModelFile {
    Model: Model

    /** Model layout */
    Layout: any

    ltl: Ltl
}

export interface Model {
    Name: string
    Variables: Variable[]
    Relationships: VariableRelationship[]
}

export interface Variable {
    Id: number

    Name: string    

    /** Integer >= 0 */
    RangeFrom: number

    /** Integer >= 0 and >= RangeFrom */
    RangeTo: number

    /** Target function */
    Formula: string
}

export interface VariableRelationship {
    id: number

    /** Variable ID */
    FromVariable: number

    /** Variable ID */
    ToVariable: number

    /** "Activator" or "Inhibitor" */
    Type: string
}

export interface Ltl {
    states: LtlState[]
    operations: LtlOperation[]
}

const NameStateType = 'Keyframe'
export interface LtlState {
    /** Keyframe */
    _type: string

    name: string
    description: string    
    operands: LtlStateEquation[]
}

export class LtlStateImpl implements LtlState {
    _type = NameStateType
    description = ''
    operands: LtlStateEquation[]

    constructor (public name: string, equations: LtlCompactStateEquation[]) {
        this.operands = equations.map(eq => ({
            _type: 'KeyframeEquation',
            leftOperand: {
                _type: 'NameOperand',
                name: eq.variable
            },
            operator: eq.operator,
            rightOperand: {
                _type: 'ConstOperand',
                const: eq.value
            }
        }))
    }
}

export type LtlStateEquationOperatorSymbol =
    '=' | '>' | '<' | '<=' | '>=' | '!='

export interface LtlCompactStateEquation {
    variable: string
    operator: LtlStateEquationOperatorSymbol
    value: number
}

export interface LtlStateEquation {
    /** "KeyframeEquation" */
    _type: string

    leftOperand: LtlStateNameOperand

    /** =, <, ... */
    operator: LtlStateEquationOperatorSymbol

    rightOperand: LtlStateConstOperand
}

export interface LtlStateNameOperand {
    /** "NameOperand" */
    _type: string

    /** Variable name */
    name: string
}

export interface LtlStateConstOperand {
    /** "ConstOperand" */
    _type: string

    /** An integer. */
    const: number
}

/** Root interface for all formula interfaces */
export interface LtlFormula {
    _type: string
}

const OperationType = 'Operation'
/** An LTL formula made up of operators and state references. */
export interface LtlOperation extends LtlFormula {
    /** "Operation" */
    _type: string

    operator: LtlOperationOperator

    operands: LtlFormula[]
}

export type LtlOperatorName =
    'AND' | 'OR' | 'IMPLIES' | 'NOT' |
    'NEXT' | 'ALWAYS' | 'EVENTUALLY' | 'UPTO' |
    'WEAKUNTIL' | 'UNTIL' | 'RELEASE'

export interface LtlOperationOperator {
    name: LtlOperatorName

    /** Number of operands the operator accepts, 
     *  equals array length of "operands" in BmaLtlOperation */
    operandsCount: number
}

export interface LtlNameStateReference extends LtlFormula {
    /** "Keyframe" */
    _type: string

    /** State name */
    name: string
}

const SelfLoopStateType = 'SelfLoopKeyframe'
export interface LtlSelfLoopState extends LtlFormula {
    /** "SelfLoopKeyframe" */
    _type: string
}

const OscillationStateType = 'OscillationKeyframe'
export interface LtlOscillationState extends LtlFormula {
    /** "OscillationKeyframe" */
    _type: string
}

const TrueStateType = 'TrueKeyframe'
export interface LtlTrueState extends LtlFormula {
    /** "TrueKeyframe" */
    _type: string
}

/**
 * Wraps an untyped LtlOperation object into a typed object.
 * This makes it easier to write algorithms on it which walk the nested tree.
 */
export class LtlOperationImpl implements LtlOperation {
    _type = OperationType
    operator: LtlOperationOperator
    operands: LtlFormula[]

    static from (operation: LtlOperation) {
        return new LtlOperationImpl(operation.operator.name, operation.operands)
    }

    constructor (operator: LtlOperatorName, operands: LtlFormula[]) {
        this.operator = {
            name: operator,
            operandsCount: operands.length
        }
        this.operands = operands.map(op => {
            switch (op._type) {
                case OperationType: return LtlOperationImpl.from(op as LtlOperation)
                case NameStateType: return new LtlNameStateReferenceImpl((<LtlNameStateReference>op).name)
                case SelfLoopStateType: return new LtlSelfLoopStateImpl()
                case OscillationStateType: return new LtlOscillationStateImpl()
                case TrueStateType: return new LtlTrueStateImpl()
                default: throw new Error('Not implemented: ' + op._type)
            }
        })
    }
}

export class LtlNameStateReferenceImpl implements LtlNameStateReference {
    _type = NameStateType
    constructor (public name: string) {}
}

export class LtlSelfLoopStateImpl implements LtlSelfLoopState {
    _type = SelfLoopStateType
}

export class LtlOscillationStateImpl implements LtlOscillationState {
    _type = OscillationStateType
}

export class LtlTrueStateImpl implements LtlTrueState {
    _type = TrueStateType
}
