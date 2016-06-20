module BMA {
    export module ModelHelper {
        export function CreateClipboardContent(model: BMA.Model.BioModel, layout: BMA.Model.Layout, contextElement: { id: number; type: string }): {
            Container: BMA.Model.ContainerLayout;
            Variables: {
                m: BMA.Model.Variable;
                l: BMA.Model.VariableLayout
            }[];
            Realtionships: BMA.Model.Relationship[]
        } {

            var result = undefined;

            if (contextElement.type === "variable") {
                var v = model.GetVariableById(contextElement.id);
                var l = layout.GetVariableById(contextElement.id);

                if (v !== undefined && l !== undefined) {
                    result = {
                        Container: undefined,
                        Realtionships: undefined,
                        Variables: [{ m: v, l: l }]
                    };
                }

            } else if (contextElement.type === "container") {
                var id = contextElement.id;
                var cnt = layout.GetContainerById(id);
                if (cnt !== undefined) {
                    var clipboardVariables = [];

                    var variables = model.Variables;
                    var variableLayouts = layout.Variables;

                    for (var i = 0; i < variables.length; i++) {
                        var variable = variables[i];
                        if (variable.ContainerId === id) {
                            clipboardVariables.push({ m: variable, l: variableLayouts[i] });
                        }
                    }

                    var clipboardRelationships = [];
                    var relationships = model.Relationships;

                    for (var i = 0; i < relationships.length; i++) {
                        var rel = relationships[i];
                        var index = 0;
                        for (var j = 0; j < clipboardVariables.length; j++) {
                            var cv = clipboardVariables[j];

                            if (rel.FromVariableId === cv.m.Id) {
                                index++;
                            }

                            if (rel.ToVariableId === cv.m.Id) {
                                index++;
                            }

                            if (index == 2)
                                break;
                        }

                        if (index === 2) {
                            clipboardRelationships.push(rel);
                        }
                    }

                    result = {
                        Container: cnt,
                        Realtionships: clipboardRelationships,
                        Variables: clipboardVariables
                    };
                }
            }

            return result;
        }

        export function GetGridCell(x: number, y: number, grid: { xOrigin: number; yOrigin: number; xStep: number; yStep: number }): { x: number; y: number } {
            var cellX = Math.ceil((x - grid.xOrigin) / grid.xStep) - 1;
            var cellY = Math.ceil((y - grid.yOrigin) / grid.yStep) - 1;
            return { x: cellX, y: cellY };
        }

        /*
        * Retrun cells which will occupied by container after its resize to @containerSize
        */
        export function GetContainerExtraCells(container: BMA.Model.ContainerLayout, containerSize: number): { x: number; y: number }[] {
            var result = [];

            if (container !== undefined) {
                var currentSize = container.Size;
                if (containerSize > currentSize) {
                    var diff = containerSize - currentSize;
                    for (var i = 0; i < container.Size; i++) {
                        for (var j = 0; j < diff; j++) {
                            result.push({ x: container.PositionX + container.Size + j, y: container.PositionY + i });
                        }
                    }
                    for (var i = 0; i < diff; i++) {
                        for (var j = 0; j < containerSize; j++) {
                            result.push({ x: container.PositionX + j, y: container.PositionY + container.Size + i });
                        }
                    }
                }
            }

            return result;
        }

        export function IsGridCellEmpty(gridCell: { x: number; y: number }, model: BMA.Model.BioModel, layout: BMA.Model.Layout, id: number, grid: { xOrigin: number; yOrigin: number; xStep: number; yStep: number }): boolean {
            var layouts = layout.Containers;
            for (var i = 0; i < layouts.length; i++) {
                if (layouts[i].Id === id)
                    continue;

                if (layouts[i].PositionX <= gridCell.x && layouts[i].PositionX + layouts[i].Size > gridCell.x &&
                    layouts[i].PositionY <= gridCell.y && layouts[i].PositionY + layouts[i].Size > gridCell.y) {
                    return false;
                }
            }

            var result = [];
            var variables = model.Variables;
            var variableLayouts = layout.Variables;
            for (var i = 0; i < variables.length; i++) {
                var variable = variables[i];
                var variableLayout = variableLayouts[i];

                if (variable.Type !== "Constant")
                    continue;

                var vGridCell = GetGridCell(variableLayout.PositionX, variableLayout.PositionY, grid);

                if (gridCell.x === vGridCell.x && gridCell.y === vGridCell.y) {
                    return false;
                }
            }

            return true;
        }

        export function ResizeContainer(model: BMA.Model.BioModel, layout: BMA.Model.Layout, containerId: number, containerSize: number, grid: { xOrigin: number; yOrigin: number; xStep: number; yStep: number }): {
            model: BMA.Model.BioModel;
            layout: BMA.Model.Layout
        } {
            var container = layout.GetContainerById(containerId);
            if (container !== undefined) {
                var sizeDiff = containerSize - container.Size;
                var shouldMove = sizeDiff > 0;

                
                var containerLayouts = layout.Containers;
                var variables = model.Variables;
                var variableLayouts = layout.Variables;

                if (shouldMove) {
                    //Check if there is enough size for extending without replacing other contents
                    var wishfulCells = GetContainerExtraCells(container, containerSize);
                    var hasNonEmpty = false;
                    for (var i = 0; i < wishfulCells.length; i++) {
                        if (!IsGridCellEmpty(wishfulCells[i], model, layout, containerId, grid)) {
                            hasNonEmpty = true;
                            break;
                        }
                    }
                    shouldMove = hasNonEmpty;
                }

                var newCnt = [];
                for (var i = 0; i < containerLayouts.length; i++) {
                    var cnt = containerLayouts[i];
                    if (cnt.Id === container.Id) {
                        newCnt.push(new BMA.Model.ContainerLayout(cnt.Id, cnt.Name, containerSize, cnt.PositionX, cnt.PositionY));
                    } else if (shouldMove && (cnt.PositionX > container.PositionX || cnt.PositionY > container.PositionY)) {
                        newCnt.push(new BMA.Model.ContainerLayout(cnt.Id, cnt.Name, cnt.Size, cnt.PositionX > container.PositionX ? cnt.PositionX + sizeDiff : cnt.PositionX,
                            cnt.PositionY > container.PositionY ? cnt.PositionY + sizeDiff : cnt.PositionY));
                    } else
                        newCnt.push(cnt);
                }

                var cntX = container.PositionX * grid.xStep + grid.xOrigin;
                var cntY = container.PositionY * grid.yStep + grid.yOrigin;
                var newVL = [];
                for (var i = 0; i < variableLayouts.length; i++) {
                    var v = variables[i];
                    var vl = variableLayouts[i];
                    if (variables[i].ContainerId === container.Id) {
                        newVL.push(new BMA.Model.VariableLayout(vl.Id, cntX + (vl.PositionX - cntX) * containerSize / container.Size, cntY + (vl.PositionY - cntY) * containerSize / container.Size, 0, 0, vl.Angle));
                    } else {
                        if (shouldMove) {
                            if (v.Type === "Constant") {
                                newVL.push(new BMA.Model.VariableLayout(vl.Id,
                                    vl.PositionX > cntX + grid.xStep ? vl.PositionX + sizeDiff * grid.xStep : vl.PositionX,
                                    vl.PositionY > cntY + grid.yStep ? vl.PositionY + sizeDiff * grid.yStep : vl.PositionY,
                                    0, 0, vl.Angle));
                            } else {
                                var vCnt = layout.GetContainerById(v.ContainerId);
                                var vCntX = vCnt.PositionX * grid.xStep + grid.xOrigin;
                                var vCntY = vCnt.PositionY * grid.yStep + grid.yOrigin;

                                var unsizedVposX = (vl.PositionX - vCntX) / vCnt.Size + vCntX;
                                var unsizedVposY = (vl.PositionY - vCntY) / vCnt.Size + vCntY;

                                newVL.push(new BMA.Model.VariableLayout(vl.Id,
                                    unsizedVposX > cntX + grid.xStep ? vl.PositionX + sizeDiff * grid.xStep : vl.PositionX,
                                    unsizedVposY > cntY + grid.yStep ? vl.PositionY + sizeDiff * grid.yStep : vl.PositionY,
                                    0, 0, vl.Angle));
                            }
                        } else {
                            newVL.push(vl);
                        }
                    }
                }

                var newlayout = new BMA.Model.Layout(newCnt, newVL);
                var newModel = new BMA.Model.BioModel(model.Name, model.Variables, model.Relationships);

                return { model: newModel, layout: newlayout };
            }
        }

        export function GetModelBoundingBox(model: BMA.Model.Layout, grid: { xOrigin: number; yOrigin: number; xStep: number; yStep: number }): { x: number; y: number; width: number; height: number } {
            var bottomLeftCell = { x: Number.POSITIVE_INFINITY, y: Number.POSITIVE_INFINITY };
            var topRightCell = { x: Number.NEGATIVE_INFINITY, y: Number.NEGATIVE_INFINITY };


            var cells = model.Containers;
            for (var i = 0; i < cells.length; i++) {
                var cell = cells[i];
                if (cell.PositionX < bottomLeftCell.x) {
                    bottomLeftCell.x = cell.PositionX;
                }
                if (cell.PositionY < bottomLeftCell.y) {
                    bottomLeftCell.y = cell.PositionY;
                }
                if (cell.PositionX + cell.Size - 1 > topRightCell.x) {
                    topRightCell.x = cell.PositionX + cell.Size - 1;
                }
                if (cell.PositionY + cell.Size - 1 > topRightCell.y) {
                    topRightCell.y = cell.PositionY + cell.Size - 1;
                }
            }

            var variables = model.Variables;

            var getGridCell = function(x,y) {
                var cellX = Math.ceil((x - grid.xOrigin) / grid.xStep) - 1;
                var cellY = Math.ceil((y - grid.yOrigin) / grid.yStep) - 1;
                return { x: cellX, y: cellY };
            }

            for (var i = 0; i < variables.length; i++) {
                var variable = variables[i];
                var gridCell = getGridCell(variable.PositionX, variable.PositionY);
                if (gridCell.x < bottomLeftCell.x) {
                    bottomLeftCell.x = gridCell.x;
                }
                if (gridCell.y < bottomLeftCell.y) {
                    bottomLeftCell.y = gridCell.y;
                }
                if (gridCell.x > topRightCell.x) {
                    topRightCell.x = gridCell.x;
                }
                if (gridCell.y > topRightCell.y) {
                    topRightCell.y = gridCell.y;
                }
            }


            if (cells.length === 0 && variables.length === 0) {
                return {
                    x: 0,
                    y: 0,
                    width: 5 * grid.xStep,
                    height: 4 * grid.yStep
                };
            } else {
                return {
                    x: bottomLeftCell.x * grid.xStep + grid.xOrigin,
                    y: bottomLeftCell.y * grid.yStep + grid.yOrigin,
                    width: (topRightCell.x - bottomLeftCell.x + 1) * grid.xStep,
                    height: (topRightCell.y - bottomLeftCell.y + 1) * grid.yStep
                };
            }
        }
        /**
         * Calculate updated states array according to model and layout
         * 1) If variable was renamed, corresponding state would be updated
         * 2) If variable was removed, corresponding state would be deleted
         * 3) If name operands in state have no ids, they would receive them from model
         * 4) If name operand in state has no id and there are multiple variables with same name in model, it would get first founded id 
         * and it would be flagged in "shouldNotify"
         * 5) If there were any changes from initial states array, it would be flagged in "isChanged"
         * @param model
         * @param layout
         * @param states
         */
        export function UpdateStatesWithModel(model: BMA.Model.BioModel, layout: BMA.Model.Layout, states: BMA.LTLOperations.Keyframe[]):
            { states: BMA.LTLOperations.Keyframe[], isChanged: boolean, shouldNotify: boolean } {
            
            var isChanged = false;
            var shouldNotify = false;
            var newStates = [];
            for (var i = 0; i < states.length; i++) {
                var state = states[i];
                var operands = [];
                var isActual = true;
                for (var j = 0; j < state.Operands.length; j++) {
                    var operand = state.Operands[j];
                    var variable;
                    if (operand instanceof BMA.LTLOperations.KeyframeEquation) {
                        variable = operand.LeftOperand;
                    } else if (operand instanceof BMA.LTLOperations.DoubleKeyframeEquation) {
                        variable = operand.MiddleOperand;
                    }
                    if (variable instanceof BMA.LTLOperations.NameOperand) {
                        var variableId = variable.Id;
                        if (variableId === undefined) {
                            var id = model.GetIdByName(variable.Name);
                            if (id.length == 0) {
                                isActual = false;
                                isChanged = true;
                                break;
                            }
                            if (id.length > 1)
                                shouldNotify = true;
                            variableId = parseFloat(id[0]);
                            isChanged = true;
                        }

                        var variableInModel = model.GetVariableById(variableId);
                        if (variableInModel === undefined/* || !variableInModel.Name*/) {
                            isActual = false;
                            isChanged = true;
                            break;
                        }
                        if (variable.Name != variableInModel.Name)
                            isChanged = true;
                        variable = new BMA.LTLOperations.NameOperand(variableInModel.Name, variableInModel.Id);
                        
                        var newOperand;
                        if (operand instanceof BMA.LTLOperations.KeyframeEquation) {
                            newOperand = new BMA.LTLOperations.KeyframeEquation(variable, operand.Operator, operand.RightOperand);
                        } else if (operand instanceof BMA.LTLOperations.DoubleKeyframeEquation) {
                            newOperand = new BMA.LTLOperations.DoubleKeyframeEquation(operand.LeftOperand, operand.LeftOperator, variable, operand.RightOperator, operand.RightOperand);
                        }
                        operands.push(newOperand);
                    }
                }
                if (isActual && operands.length != 0)
                    newStates.push(new BMA.LTLOperations.Keyframe(state.Name, state.Description, operands));
            }

            return {
                states: newStates,
                isChanged: isChanged,
                shouldNotify: shouldNotify
            };
        }

        export function UpdateFormulasAfterVariableChanged(variableId: number, oldModel: BMA.Model.BioModel, newModel: BMA.Model.BioModel) {

            if (variableId !== undefined && newModel) {
                var variables = oldModel.Variables;
                var editingVariableIndex = -1;
                for (var i = 0; i < variables.length; i++) {
                    if (variables[i].Id === variableId) {
                        editingVariableIndex = i;
                        break;
                    }
                }
                
                var editedVariableIndex = -1;
                for (var j = 0; j < newModel.Variables.length; j++) {
                    if (newModel.Variables[j].Id === variableId) {
                        editedVariableIndex = j;
                        break;
                    }
                }
                
                if (editingVariableIndex != -1 && editedVariableIndex != -1) {
                    var oldName = variables[editingVariableIndex].Name;
                    var newName = newModel.Variables[editedVariableIndex].Name
                    if (oldName != newName) {
                        var ids = BMA.ModelHelper.FindAllRelationships(variableId, newModel.Relationships);
                        var newVariables = [];
                        for (var j = 0; j < newModel.Variables.length; j++) {
                            var variable = newModel.Variables[j];
                            var oldFormula = variable.Formula;
                            var newFormula = undefined;
                            for (var k = 0; k < ids.length; k++) {
                                if (variable.Id == ids[k]) {
                                    newFormula = oldFormula.replace(new RegExp("var\\(" + oldName + "\\)", 'g'),
                                        "var(" + newName + ")");
                                    break;
                                }
                            }
                            newVariables.push(new BMA.Model.Variable(
                                variable.Id,
                                variable.ContainerId,
                                variable.Type,
                                variable.Name,
                                variable.RangeFrom,
                                variable.RangeTo,
                                newFormula === undefined ? oldFormula : newFormula)
                            );
                        }

                        var newRelations = [];
                        for (var j = 0; j < newModel.Relationships.length; j++) {
                            newRelations.push(new BMA.Model.Relationship(
                                newModel.Relationships[j].Id,
                                newModel.Relationships[j].FromVariableId,
                                newModel.Relationships[j].ToVariableId,
                                newModel.Relationships[j].Type)
                            );
                        }
                        newModel = new BMA.Model.BioModel(newModel.Name, newVariables, newRelations);
                    }
                } else if (editingVariableIndex != -1) {
                    var oldName = variables[editingVariableIndex].Name;
                    var ids = BMA.ModelHelper.FindAllRelationships(variableId, oldModel.Relationships);
                    var newVariables = [];
                    for (var j = 0; j < newModel.Variables.length; j++) {
                        var variable = newModel.Variables[j];
                        var oldFormula = variable.Formula;
                        var newFormula = undefined;
                        for (var k = 0; k < ids.length; k++) {
                            if (variable.Id == ids[k]) {
                                newFormula = oldFormula.replace(new RegExp("var\\(" + oldName + "\\)", 'g'), "");
                                break;
                            }
                        }
                        newVariables.push(new BMA.Model.Variable(
                            variable.Id,
                            variable.ContainerId,
                            variable.Type,
                            variable.Name,
                            variable.RangeFrom,
                            variable.RangeTo,
                            newFormula === undefined ? oldFormula : newFormula)
                        );
                    }
                    var newRelations = [];
                    for (var j = 0; j < newModel.Relationships.length; j++) {
                        newRelations.push(new BMA.Model.Relationship(
                            newModel.Relationships[j].Id,
                            newModel.Relationships[j].FromVariableId,
                            newModel.Relationships[j].ToVariableId,
                            newModel.Relationships[j].Type)
                        );
                    }
                    newModel = new BMA.Model.BioModel(newModel.Name, newVariables, newRelations);
                }
            }

            return newModel;
        }

        export function FindAllRelationships(id: number, relationships: BMA.Model.Relationship[]) {
            var variableIds = [];
            for (var i = 0; i < relationships.length; i++) {
                if (relationships[i].FromVariableId === id)
                    variableIds.push(relationships[i].ToVariableId)
            }
            return variableIds.sort((x, y) => {
                return x < y ? -1 : 1;
            });;
        }

        export function GenerateStateName(states: BMA.LTLOperations.Keyframe[], newState: BMA.LTLOperations.Keyframe): string {
            var k = states.length;
            var lastStateName = "A";
            for (var i = 0; i < k; i++) {
                var lastStateIdx = (lastStateName && lastStateName.length > 1) ? parseFloat(lastStateName.slice(1)) : 0;
                var stateName = states[i].Name ? states[i].Name : "A"; 
                var stateIdx = stateName.length > 1 ? parseFloat(stateName.slice(1)) : 0;

                if (stateIdx >= lastStateIdx) {
                    lastStateName = (lastStateName && stateIdx == lastStateIdx
                        && lastStateName.charAt(0) > stateName.charAt(0)) ?
                        lastStateName : stateName;
                }
            }

            var newStateName = newState && newState.Name ? newState.Name : "A";
            var newStateIdx = (newStateName && newStateName.length > 1) ? parseFloat(newStateName.slice(1)) : 0;
            
            if (lastStateName && ((lastStateIdx == newStateIdx && lastStateName.charAt(0) >= newStateName.charAt(0))
                || lastStateIdx > newStateIdx)) {
                
                var charCode = lastStateName ? lastStateName.charCodeAt(0) : 65;
                var n = (lastStateName && lastStateName.length > 1) ? parseFloat(lastStateName.slice(1)) : 0;
                
                if (charCode >= 90) {
                    n++;
                    charCode = 65;
                } else if (lastStateName) charCode++;
               

                newStateName = n ? String.fromCharCode(charCode) + n : String.fromCharCode(charCode);
            }
            return newStateName;
        }

        export function GetScrollBarSize(): { width: number, height: number } {
            var $outer = $('<div>').css({ visibility: 'hidden', width: 100, height: 100, overflow: 'scroll' }).appendTo('body'),
                widthWithScroll = $('<div>').css({ width: '100%' }).appendTo($outer).outerWidth(),
                heightWithScroll = $('<div>').css({ height: '100%' }).appendTo($outer).outerHeight();
            $outer.remove();
            var width =  100 - widthWithScroll;
            var height = 100 - heightWithScroll;

            return { width: width, height: height };
        }

        export function ConvertFormulaToOperation(formula: string, states: BMA.LTLOperations.Keyframe[]): BMA.LTLOperations.Operation {
            var parsedFormula;
            try {
                var parsedFormula = BMA.parser.parse(formula);
                var operation = ConvertToOperation(parsedFormula, states);
                if (operation instanceof BMA.LTLOperations.Operation) return operation;
            } catch (ex) {
                alert(ex);
            }
            return undefined;
        }

        export function ConvertToOperation(formula: any, states: BMA.LTLOperations.Keyframe[]): BMA.LTLOperations.IOperand  {
            if (!formula) throw "Nothing to import";
            if (formula.state && states) {
                for (var i = 0; i < states.length; i++) {
                    if (states[i].Name == formula.state)
                        return states[i].Clone();
                }
                if (formula.state.toUpperCase() == "OSCILLATION")
                    return new BMA.LTLOperations.OscillationKeyframe();
                if (formula.state.toUpperCase() == "SELFLOOP")
                    return new BMA.LTLOperations.SelfLoopKeyframe();
                if (formula.state.toUpperCase() == "TRUE")
                    return new BMA.LTLOperations.TrueKeyframe();
                return undefined;
            } else {
                if (formula.operator) {
                    var operation = new BMA.LTLOperations.Operation();
                    var operands = [];
                    var operator = window.OperatorsRegistry.GetOperatorByName(formula.operator.toUpperCase());
                    if (operator === undefined) throw "Operator doesn't exist";
                    for (i = 0; i < formula.operands.length; i++) {
                        operands.push(ConvertToOperation(formula.operands[i], states));
                    }
                    operation.Operator = operator;
                    operation.Operands = operands;
                    return operation;
                }
            }
            throw "Operation was not found";
        }
    }
} 