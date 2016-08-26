export const TerminalTypes = [
    'binaryTemporalOperator', 'relationalOperator', 'unaryOperator',
    'modelVariable', 'integerLiteral'
]

export const BinaryExpressionTypes = [
    'conditionalsExpression',
    'disjunctionExpression',
    'conjunctionExpression',
    'temporalExpression',
    'relationalExpression'
]

export const UnaryExpressionTypes = [
    'unaryExpression'
]

export type NodeType = 
    'conditionalsExpression' |
    'impliesOperator' |
    'disjunctionExpression' |
    'conjunctionExpression' |
    'temporalExpression' |
    'atomicExpression' |
    'unaryExpression' |
    'unaryOperator' |
    'binaryTemporalOperator' |
    'relationalExpression' |
    'relationalOperator' |
    'modelVariable' |
    'integerLiteral'

export interface Node<L extends Node<any,any>, R extends Node<any,any>> {
    type: NodeType
    value?: any
    left?: L
    right?: R
}

export interface ImpliesOperator extends Node<any,any> {
    type: 'impliesOperator'
    value: 'implies'
}

export interface ConditionalsExpression extends Node<DisjunctionExpression, DisjunctionExpression> {
    type: 'conditionalsExpression'
    value: ImpliesOperator
    left: DisjunctionExpression
    right: DisjunctionExpression
}

export interface DisjunctionExpression extends Node<ConjunctionExpression, ConjunctionExpression> {
    type: 'disjunctionExpression'
    left: ConjunctionExpression
}

export interface ConjunctionExpression extends Node<TemporalExpression, TemporalExpression> {
    type: 'conjunctionExpression'
    left: TemporalExpression
}

export interface TemporalExpression extends Node<AtomicExpression,AtomicExpression> {
    type: 'temporalExpression'
    left: AtomicExpression
    value?: BinaryTemporalOperator
}

export interface AtomicExpression extends Node<any,any> {
    type: 'atomicExpression'
    left: RelationalExpression | UnaryExpression
}

export type UnaryOperatorSymbol =
    'not' | 'next' | 'always' | 'eventually'

export interface UnaryOperator extends Node<any,any> {
    type: 'unaryOperator'
    value: UnaryOperatorSymbol
}

export interface UnaryExpression extends Node<Node<any,any>,any> {
    type: 'unaryExpression'
    value: UnaryOperator
    left: Node<any,any>
}

export interface RelationalExpression extends Node<ModelVariable, IntegerLiteral> {
    type: 'relationalExpression'
    value: RelationalOperator
    left: ModelVariable
    right: IntegerLiteral
}

export type BinaryTemporalOperatorSymbol =
    'until' | 'weak until' | 'release' | 'upto'

export interface BinaryTemporalOperator extends Node<any,any> {
    type: 'binaryTemporalOperator'
    value: BinaryTemporalOperatorSymbol
}

export type RelationalOperatorSymbol = 
    '=' | '>' | '<' | '<=' | '>=' | '!='

export interface RelationalOperator extends Node<any,any> {
    type: 'relationalOperator'
    value: RelationalOperatorSymbol
}

export interface ModelVariable extends Node<any, any> {
    type: 'modelVariable'
    value: number
}

export interface IntegerLiteral extends Node<any, any> {
    type: 'integerLiteral'
    value: number
}