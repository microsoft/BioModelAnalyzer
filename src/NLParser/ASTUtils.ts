import * as _ from 'underscore'
import * as AST from './AST'
import * as BMA from '../BMA'

/**
 * Return a human readable formula string of an AST generated by the parser.
 * This string is in a specific format that can be read by the BMA tool.
 * 
 * @param node The AST.
 * @param bmaModel The BMA model.
 */
export function toHumanReadableString (node: AST.Node<any,any>, bmaModel: BMA.ModelFile) {
    let varName = id => _.find(bmaModel.Model.Variables, v => v.Id === id).Name

    let left = node.left ? toHumanReadableString(node.left, bmaModel) : null
    let right = node.right ? toHumanReadableString(node.right, bmaModel) : null

    if (node.type === AST.Type.ModelVariable) {
        let name = varName(node.value)
        return name.indexOf(' ') >= 0 ? '(' + name + ')' : name
    } else if (_.contains(AST.TerminalTypes, node.type)) {
        return node.value
    } else if (node.type === AST.Type.RelationalExpression) {
        return left + (<AST.RelationalExpression>node).value.value + right
    } else if (_.contains(AST.BinaryExpressionTypes, node.type)) {
        return '(' + left + ' ' + (<AST.BinaryExpression>node).value.value + ' ' + right + ')'
    } else if (_.contains(AST.UnaryExpressionTypes, node.type)) {
        return (<AST.UnaryExpression>node).value.value + '(' + left + ')'
    } else {
        throw new Error('Unknown node type: ' + node.type)
    }
}

/**
 * Return a BMA REST API-compatible formula string of an AST generated by the parser.
 * 
 * @param node The AST.
 * @param bmaModel The BMA model.
 */
export function toAPIString (node: AST.Node<any,any>, bmaModel: BMA.ModelFile) {
    let varName = id => _.find(bmaModel.Model.Variables, v => v.Id === id).Name
    let upper = (s: string) => s[0].toUpperCase() + s.substr(1)

    let left = node.left ? toAPIString(node.left, bmaModel) : null
    let right = node.right ? toAPIString(node.right, bmaModel) : null

    if (_.contains(AST.TerminalTypes, node.type)) {
        return node.value
    } else if (node.type === AST.Type.RelationalExpression) {
        return '(' + (<AST.RelationalExpression>node).value.value + ' ' + left + ' ' + right + ')'
    } else if (_.contains(AST.BinaryExpressionTypes, node.type)) {
        return '(' + upper((<AST.BinaryExpression>node).value.value) + ' ' + left + ' ' + right + ')'
    } else if (_.contains(AST.UnaryExpressionTypes, node.type)) {
        return '(' + upper((<AST.UnaryExpression>node).value.value) + ' ' + left + ')'
    } else {
        throw new Error('Unknown node type: ' + node.type)
    }
}

export interface Clamping {
    variable: BMA.Variable
    originalValue: number
    clampedValue: number
}

export function clampVariables (node: AST.Node<any,any>, bmaModel: BMA.ModelFile) {
    let getVariable = id => _.find(bmaModel.Model.Variables, v => v.Id === id)

    let clampings: Clamping[] = []

    /** clamps in-place */
    function doClamp (node) {
        if (node.type === AST.Type.RelationalExpression) {
            let variable = getVariable(node.left.value)
            let value = node.right.value
            if (value < variable.RangeFrom) {
                node.right.value = variable.RangeFrom
                clampings.push({
                    variable,
                    originalValue: value,
                    clampedValue: variable.RangeFrom
                })
            } else if (value > variable.RangeTo) {
                node.right.value = variable.RangeTo
                clampings.push({
                    variable,
                    originalValue: value,
                    clampedValue: variable.RangeTo
                })
            }
        } else {
            if (node.left) {
                doClamp(node.left)
            }
            if (node.right) {
                doClamp(node.right)
            }            
        }
    }

    let nodeCopy = JSON.parse(JSON.stringify(node))
    doClamp(nodeCopy)

    return {
        AST: nodeCopy,
        clampings
    }
}

/**
 * Separates an AST into an LTL formula and states.  
 * The result can be used within a BMA model file. New state names are guaranteed to not
 * collide with names of the input model.
 */
export function toStatesAndFormula (node: AST.Node<any,any>, bmaModel: BMA.ModelFile): BMA.Ltl {
    let varName = id => _.find(bmaModel.Model.Variables, v => v.Id === id).Name

    // A-Z
    let letters: string[] = Array.apply(0, Array(26)).map((x, y) => String.fromCharCode(65 + y))

    let states = bmaModel.ltl.states.slice()
    let unusedStateName = () => _.find(letters, letter => !states.some(state => state.name === letter))

    function toCompactRelationalExpression (node: AST.RelationalExpression): BMA.LtlCompactStateRelationalExpression {
        return {
            variableName: varName(node.left.value),
            variableId: node.left.value,
            operator: node.value.value,
            value: node.right.value
        }
    }

    function walk (node, states: BMA.LtlState[]): BMA.LtlFormula  {
        if (node.type === AST.Type.RelationalExpression) {
            let stateName = unusedStateName()
            let relationalExpression = toCompactRelationalExpression(node)
            states.push(new BMA.LtlStateImpl(stateName, [relationalExpression]))
            return new BMA.LtlNameStateReferenceImpl(stateName)
        } else if (_.contains(AST.BinaryExpressionTypes, node.type)) {
            // check for AND-nested relationalExpression tree
            // this becomes a state
            
            if (node.type === AST.Type.ConjunctionExpression && node.left.type === AST.Type.RelationalExpression) {
                let relExprNodes = [node.left]
                let curNode = node.right
                while (curNode.type === AST.Type.ConjunctionExpression && curNode.left.type === AST.Type.RelationalExpression) {
                    relExprNodes.push(curNode.left)
                    curNode = curNode.right
                }
                let stateName = unusedStateName()

                if (curNode.type === AST.Type.RelationalExpression) {
                    // no more nodes, we can collapse the tree of relational expressions to a single name state (see if-else below)
                    relExprNodes.push(curNode)
                }
                let relationalExpressions = relExprNodes.map(toCompactRelationalExpression)

                states.push(new BMA.LtlStateImpl(stateName, relationalExpressions))
                let stateNameRef = new BMA.LtlNameStateReferenceImpl(stateName)

                if (curNode.type === AST.Type.RelationalExpression) {
                    // tree is collapsed to a single state name reference
                    return stateNameRef
                } else {
                    // more nodes that are not relational expressions are coming
                    // we create a state and <rest>
                    return new BMA.LtlOperationImpl(node.value.value, [stateNameRef, walk(curNode, states)])
                }
            } else {
                return new BMA.LtlOperationImpl(node.value.value, [walk(node.left, states), walk(node.right, states)])
            }
        } else if (_.contains(AST.UnaryExpressionTypes, node.type)) {
            return new BMA.LtlOperationImpl(node.value.value, [walk(node.left, states)])
        } else {
            throw new Error('Unknown node type: ' + node.type)
        }
    }

    let formula = walk(node, states)
    
    // The following cast from LtlFormula to LtlOperation is a hack.
    // The BMA JSON format doesn't strictly allow it, but it still works in the tool (apart from some error messages and not being able to save).
    // TODO should a formula that is not an operation be wrapped in an Eventually operator? (is that equivalent?)
    let operations = [formula] as BMA.LtlOperation[]

    return {
        operations,
        states
    }
}