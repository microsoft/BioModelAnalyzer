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

/** The value of the "Model" property of a JSON model file. */
export interface Model {
    Name: string
    Variables: Variable[]
    Relationships: VariableRelationship[]
}

/** An array item within the "Model.Variables" array of a JSON model file. */
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

/** An array item within the "Model.Relationships" array of a JSON model file. */
export interface VariableRelationship {
    id: number

    /** Variable ID */
    FromVariable: number

    /** Variable ID */
    ToVariable: number

    /** "Activator" or "Inhibitor" */
    Type: string
}

/** The value of the "Ltl" property of a JSON model file. */
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
    operands: LtlStateRelationalExpression[]
}

export class LtlStateImpl implements LtlState {
    _type = NameStateType
    description = ''
    operands: LtlStateRelationalExpression[]

    constructor (public name: string, equations: LtlCompactStateRelationalExpression[]) {
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

export type LtlStateRelationalOperatorSymbol =
    '=' | '>' | '<' | '<=' | '>=' | '!='

export interface LtlCompactStateRelationalExpression {
    variable: string
    operator: LtlStateRelationalOperatorSymbol
    value: number
}

export interface LtlStateRelationalExpression {
    /** "KeyframeEquation" */
    _type: string

    leftOperand: LtlStateNameOperand

    /** =, <, ... */
    operator: LtlStateRelationalOperatorSymbol

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
