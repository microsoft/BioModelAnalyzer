import * as config from 'config'
import * as Promise from 'promise'
import * as request from 'request'
import * as BMA from './BMA'

const BACKEND_URL = config.get('BMA_BACKEND_URL')
 
/**
 * Runs a fast simulation against the public BMA server API.
 * 
 * The result of the simulation asserts one of the two following:
 * - The formula is sometimes true. (response.Status == true)
 * - The formula is sometimes false. (response.Status == false)
 * To assert whether the formula is always true/false or has duality (sometimes true and sometimes false),
 * a thorough simulation has to be run with the result of the fast simulation.
 * See runThoroughSimulation() for details.
 * 
 * @param model The "Model" part of a BmaJsonModel.
 * @param formula An expanded formula to run, e.g. "(Eventually (And (= 10 1) SelfLoop))". See getExpandedFormula().
 * @param stepCount The maximum number of time steps to simulate.
 */
export function runFastSimulation (model: BMA.Model, formula: string, stepCount: number): Promise.IThenable<AnalyzeLTLSimulationResponse> {
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

/**
 * Runs a thorough/polarity simulation against the public BMA server API using the result of a fast simulation.
 * 
 * The result of the simulation asserts one of the following:
 * - The formula is always true (false). (response.Status == false)
 * - The formula is sometimes true and sometimes false. (response.Status == true)
 * 
 * @param model The "Model" part of a BmaJsonModel.
 * @param formula An expanded formula to run, e.g. "(Eventually (And (= 10 1) SelfLoop))". See getExpandedFormula().
 * @param stepCount The maximum number of time steps to simulate.
 * @param fastSimulationResponse The response of a fast simulation using the same model, formula, and steps.
 */
export function runThoroughSimulation(model: BMA.Model, formula: string, stepCount: number,
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
export function getExpandedFormula (model: BMA.Model, states: BMA.LtlState[], formula: BMA.LtlOperation) {
    let op = BMA.LtlOperationImpl.from(formula)
    return '(' + doGetExpandedFormula(model, states, op) + ')'
}

function doGetExpandedFormula (model: BMA.Model, states: BMA.LtlState[], formula: BMA.LtlFormula) {
    if (formula instanceof BMA.LtlSelfLoopStateImpl) {
        return 'SelfLoop'
    } else if (formula instanceof BMA.LtlOscillationStateImpl) {
        return 'Oscillation'
    } else if (formula instanceof BMA.LtlTrueStateImpl) {
        return 'True'
    } else if (formula instanceof BMA.LtlNameStateReferenceImpl) {
        let stateName = formula.name
        let state = states.filter(state => state.name === stateName)[0]
        let ops = state.operands
        let varId = name => model.Variables.filter(variable => variable.Name === name)[0].Id
        return ops.slice(1).reduce((l,r) =>
            `And (${l}) (${r.operator} ${varId(r.leftOperand.name)} ${r.rightOperand['const']})`,
            `${ops[0].operator} ${varId(ops[0].leftOperand.name)} ${ops[0].rightOperand['const']}`)
    } else if (formula instanceof BMA.LtlOperationImpl) {
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
    Variables: BMA.Variable[]
    Relationships: BMA.VariableRelationship[]
}

/**
 * The JSON format for an AnalyzeLTLSimulation API response.
 */
export interface AnalyzeLTLSimulationResponse {
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
    
    /** Detailed error messages. This may be null even if Error is defined. */
    ErrorMessages?: string[]

    /** Detailed debug messages. */
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
    Variables: BMA.Variable[]
    Relationships: BMA.VariableRelationship[]
}

/**
 * The JSON format for an AnalyzeLTLPolarity API response.
 */
export interface AnalyzeLTLPolarityResponse extends AnalyzeLTLSimulationResponse {
    /**
     * See BmaJsonAnalyzeLTLSimulationResponse docs.
     */
    Status: boolean
}

export interface SimulationTick {
    /** integer, first tick starts at 0 */
    Time: number

    Variables: SimulationTickVariable[]
}

export interface SimulationTickVariable {
    /** Variable ID */
    Id: number

    /** integer */
    Lo: number

    /** integer */
    Hi: number
}
