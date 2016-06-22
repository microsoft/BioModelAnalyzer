module BMA {
    export module Model {

        export function MapVariableNames(f: string, mapper: (string) => string[]) {
            var namestory = {};
            if (f !== undefined && f != null) {
                f = f.trim();
                // Convert default function to null
                if (f.toLowerCase() == "avg(pos)-avg(neg)")
                    return null;
                // Replace variable names with IDs
                var varPrefix = "var(";
                var startPos = 0;
                var index: number;
                while ((index = f.indexOf(varPrefix, startPos)) >= 0) {
                    var endIndex = f.indexOf(")", index);
                    if (endIndex < 0)
                        break;
                    var varName = f.substring(index + varPrefix.length, endIndex);
                    namestory[varName] = (namestory[varName] === undefined) ? 0 : namestory[varName] + 1;
                    var map = mapper(varName);

                    var m: any = undefined;
                    if (map instanceof Array) {
                        var ind = namestory[varName];
                        if (ind > map.length - 1)
                            ind = map.length - 1;
                        m = map[ind];
                    } else {
                        m = map;
                    }

                    f = f.substring(0, index + varPrefix.length) + m + f.substr(endIndex);
                    startPos = index + 1;
                }
            }
            return f;
        }

        // Returns object whose JSON representation matches external format:
        // 1) Variables in formulas are identified by IDs
        // 2) Default function avg(pos)-avg(neg) is replaced with null formula
        export function ExportBioModel(model: BioModel) {

            function GetIdByName(id: number, name: string): string[] {
                var results = model.Variables.filter(function (v2: Variable) {
                    return v2.Name == name &&
                        model.Relationships.some(function (r: Relationship) {
                            return r.ToVariableId == id && r.FromVariableId == v2.Id;
                            // || r.FromVariableId == id && r.ToVariableId == v2.Id
                        });
                });
                if (results.length == 0) {
                    var varName = "unnamed";
                    for (var ind = 0; ind < model.Variables.length; ind++) {
                        var vi = model.Variables[ind];
                        if (vi.Id === id) {
                            varName = vi.Name;
                            break;
                        }
                    }
                    if (varName === "")
                        varName = "''";
                    throw "Unknown variable " + name + " in formula for variable " + varName;
                }
                var res = [];
                res = res.concat(results.map(x => x.Id.toString()));
                return res;
            }

            return {
                Name: model.Name,
                Variables: model.Variables.map(v => {
                    return {
                        Name: v.Name, //this is required when ltl finds var by name
                        Id: v.Id,
                        RangeFrom: v.RangeFrom,
                        RangeTo: v.RangeTo,
                        Formula: MapVariableNames(v.Formula, name => GetIdByName(v.Id, name))
                    }
                }),
                Relationships: model.Relationships.map(r => {
                    return {
                        Id: r.Id,
                        FromVariable: r.FromVariableId,
                        ToVariable: r.ToVariableId,
                        Type: r.Type
                    }
                })
            }
        }

        export function ExportModelAndLayout(model: BioModel, layout: Layout) {
            return {
                Model: ExportBioModel(model),
                Layout: {
                    Variables: layout.Variables.map(v => {
                        var mv = model.GetVariableById(v.Id);
                        return {
                            Id: v.Id,
                            Name: mv.Name,
                            Type: mv.Type,
                            ContainerId: mv.ContainerId,
                            PositionX: v.PositionX,
                            PositionY: v.PositionY,
                            CellX: v.CellX,
                            CellY: v.CellY,
                            Angle: v.Angle,
                            Description: v.TFDescription,
                        }
                    }),
                    Containers: layout.Containers.map(c => {
                        return {
                            Id: c.Id,
                            Name: c.Name,
                            Size: c.Size,
                            PositionX: c.PositionX,
                            PositionY: c.PositionY
                        }
                    })
                }
            }
        }

        export function ImportModelAndLayout(json: any) {
            var id = {};
            json.Layout.Variables.forEach(v => {
                id[v.Id] = v;
            });

            var model = new BioModel(json.Model.Name,
                json.Model.Variables.map(v => new Variable(v.Id, id[v.Id].ContainerId, id[v.Id].Type, id[v.Id].Name, v.RangeFrom, v.RangeTo,
                    MapVariableNames(v.Formula, s => id[parseInt(s)].Name))),
                json.Model.Relationships.map(r => new Relationship(r.Id, r.FromVariable, r.ToVariable, r.Type)));


            var containers = json.Layout.Containers.map(c => new ContainerLayout(c.Id, c.Name, c.Size, c.PositionX, c.PositionY));
            for (var i = 0; i < containers.length; i++) {
                if (containers[i].Name === undefined || containers[i].Name === "") {
                    var newContainer = new BMA.Model.ContainerLayout(containers[i].Id, BMA.Model.GenerateNewContainerName(containers),
                        containers[i].Size, containers[i].PositionX, containers[i].PositionY);
                    containers[i] = newContainer;
                }
            }

            var layout = new Layout(containers,
                json.Layout.Variables.map(v => new VariableLayout(v.Id, v.PositionX, v.PositionY, v.CellX, v.CellY, v.Angle, v.Description)));


            return {
                Model: model,
                Layout: layout
            };
        }


        export function ExportState(state: BMA.LTLOperations.Keyframe | BMA.LTLOperations.KeyframeEquation | BMA.LTLOperations.DoubleKeyframeEquation | BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand) {
            if (state instanceof BMA.LTLOperations.NameOperand) {
                var nameOp = <BMA.LTLOperations.NameOperand>state;
                var result: any = {
                    _type: "NameOperand",
                    name: nameOp.Name
                };
                if (nameOp.Id !== undefined) result.id = nameOp.Id;
                return result;
            } else if (state instanceof BMA.LTLOperations.ConstOperand) {
                var constOp = <BMA.LTLOperations.ConstOperand>state;
                var result: any = {
                    _type: "ConstOperand",
                    "const": constOp.Value
                };
                return result;
            } else if (state instanceof BMA.LTLOperations.KeyframeEquation) {
                var ke = <BMA.LTLOperations.KeyframeEquation>state;
                var result: any = {
                    _type: "KeyframeEquation",
                    leftOperand: ExportState(ke.LeftOperand),
                    operator: ke.Operator,
                    rightOperand: ExportState(ke.RightOperand)
                };
                return result;
            } else if (state instanceof BMA.LTLOperations.DoubleKeyframeEquation) {
                var dke = <BMA.LTLOperations.DoubleKeyframeEquation>state;
                var result: any = {
                    _type: "DoubleKeyframeEquation",
                    leftOperand: ExportState(dke.LeftOperand),
                    leftOperator: dke.LeftOperator,
                    middleOperand: ExportState(dke.MiddleOperand),
                    rightOperator: dke.RightOperator,
                    rightOperand: ExportState(dke.RightOperand)
                }
                return result;
            } else if (state instanceof BMA.LTLOperations.Keyframe) {
                var kf = <BMA.LTLOperations.Keyframe>state;
                var result: any = {
                    _type: "Keyframe",
                    description: kf.Description,
                    name: kf.Name,
                    operands: []
                };

                for (var i = 0; i < kf.Operands.length; i++) {
                    result.operands.push(ExportState(kf.Operands[i]));
                }

                return result;
            }

            throw "Unsupported State Type";
        }

        export function ExportOperation(operation: BMA.LTLOperations.Operation, withStates: boolean) {
            var result: any = {};
            result["_type"] = "Operation";
            if (operation.Operator && operation.Operator.Name && operation.Operator.OperandsCount) {
                result.operator = {
                    name: operation.Operator.Name,
                    operandsCount: operation.Operator.OperandsCount
                };
            } else
                throw "Operation must have operator";

            result.operands = [];

            if (operation.Operands) {
                for (var i = 0; i < operation.Operands.length; i++) {
                    var op = operation.Operands[i];
                    if (op === undefined || op === null) {
                        result.operands.push(undefined);
                    } else if (op instanceof BMA.LTLOperations.Operation) {
                        result.operands.push(ExportOperation(<BMA.LTLOperations.Operation>op, withStates));
                    } else if (op instanceof BMA.LTLOperations.Keyframe) {
                        if (withStates) {
                            result.operands.push(ExportState(<BMA.LTLOperations.Keyframe>op));
                        } else {
                            result.operands.push({
                                _type: "Keyframe",
                                name: (<BMA.LTLOperations.Keyframe>op).Name
                            });
                        }
                    } else if (op instanceof BMA.LTLOperations.TrueKeyframe) {
                        result.operands.push({
                            _type: "TrueKeyframe",
                        });
                    } else if (op instanceof BMA.LTLOperations.OscillationKeyframe) {
                        result.operands.push({
                            _type: "OscillationKeyframe",
                        });
                    } else if (op instanceof BMA.LTLOperations.SelfLoopKeyframe) {
                        result.operands.push({
                            _type: "SelfLoopKeyframe",
                        });
                    } else {
                        //Unknown operand type
                        result.operands.push(undefined);
                    }
                }
            }

            return result;
        }

        export function ExportLTLContents(states: BMA.LTLOperations.Keyframe[], operations: BMA.LTLOperations.Operation[]): { states: any[]; operations: any[] } {
            var result = {
                states: [],
                operations: []
            };

            if (states) {
                for (var i = 0; i < states.length; i++) {
                    result.states.push(ExportState(states[i]));
                }
            }

            if (operations) {
                for (var i = 0; i < operations.length; i++) {
                    result.operations.push(ExportOperation(operations[i], false));
                }
            }

            return result;
        }

        export function ImportLTLContents(infoset: {
            states: any[];
            operations: any[]
        }): {
                states: BMA.LTLOperations.Keyframe[];
                operations: BMA.LTLOperations.Operation[]
            } {

            var result = {
                states: undefined,
                operations: undefined
            }

            if (infoset.states !== undefined && infoset.states.length > 0) {
                result.states = [];
                for (var i = 0; i < infoset.states.length; i++) {
                    result.states.push(<BMA.LTLOperations.Keyframe>ImportOperand(infoset.states[i], undefined));
                }
                for (var i = 0; i < result.states.length; i++) {
                    var currState = result.states[i];
                    var slicedStates = result.states.slice(0);
                    slicedStates = slicedStates.splice(0, i);
                    if (!currState.Name) {
                        var newName = BMA.ModelHelper.GenerateStateName(slicedStates, currState);
                        result.states[i] = new BMA.LTLOperations.Keyframe(newName, currState.Description, currState.Operands);
                    }
                }
            }

            if (infoset.operations !== undefined && infoset.operations.length > 0) {
                result.operations = [];
                for (var i = 0; i < infoset.operations.length; i++) {
                    result.operations.push(<BMA.LTLOperations.Operation>ImportOperand(infoset.operations[i], result.states));
                }
            }

            return result;
        }

        export function ImportOperand(obj, states: BMA.LTLOperations.Keyframe[]): BMA.LTLOperations.IOperand {
            if (obj === undefined)
                throw "Invalid LTL Operand";

            switch (obj._type) {
                case "NameOperand":
                    return new BMA.LTLOperations.NameOperand(obj.name, obj.id);
                    break;
                case "ConstOperand":
                    return new BMA.LTLOperations.ConstOperand(obj.const);
                    break;
                case "KeyframeEquation":
                    var leftOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>ImportOperand(obj.leftOperand, states);
                    var rightOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>ImportOperand(obj.rightOperand, states);
                    var operator = <string>obj.operator;
                    return new BMA.LTLOperations.KeyframeEquation(leftOperand, operator, rightOperand);
                    break;
                case "DoubleKeyframeEquation":
                    var leftOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>ImportOperand(obj.leftOperand, states);
                    var middleOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>ImportOperand(obj.middleOperand, states);
                    var rightOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>ImportOperand(obj.rightOperand, states);
                    var leftOperator = <string>obj.leftOperator;
                    var rightOperator = <string>obj.rightOperator;
                    return new BMA.LTLOperations.DoubleKeyframeEquation(leftOperand, leftOperator, middleOperand, rightOperator, rightOperand);
                    break;
                case "Keyframe":
                    if (states !== undefined) {
                        for (var i = 0; i < states.length; i++) {
                            var state = states[i];
                            if (state && state.Name === obj.name)
                                return state.Clone();
                        }
                        alert(obj.name);
                        throw "No suitable states found";//TODO: replace this by editing empty operation
                    } else {

                        var operands = [];
                        if (obj.operands) {
                            for (var i = 0; i < obj.operands.length; i++) {
                                operands.push(ImportOperand(obj.operands[i], states));
                            }
                        }
                        return new BMA.LTLOperations.Keyframe(obj.name, obj.description, operands);
                    }
                    break;
                case "Operation":
                    var operands = [];
                    if (obj.operands) {
                        for (var i = 0; i < obj.operands.length; i++) {
                            var operand = obj.operands[i];
                            if (operand === undefined || operand === null) {
                                operands.push(undefined);
                            } else {
                                operands.push(ImportOperand(operand, states));
                            }
                        }
                    }
                    var op = new BMA.LTLOperations.Operation();
                    op.Operands = operands;
                    //TODO: improve operator restoring
                    if (obj.operator && obj.operator.name)
                        op.Operator = window.OperatorsRegistry.GetOperatorByName(obj.operator.name);
                    else throw "Operation must have name of operator";
                    return op;                    
                    break;
                case "TrueKeyframe":
                    return new BMA.LTLOperations.TrueKeyframe();
                case "OscillationKeyframe":
                    return new BMA.LTLOperations.OscillationKeyframe();
                case "SelfLoopKeyframe":
                    return new BMA.LTLOperations.SelfLoopKeyframe();
                default:
                    break;
                
            }

            return undefined;
        }
    }
} 