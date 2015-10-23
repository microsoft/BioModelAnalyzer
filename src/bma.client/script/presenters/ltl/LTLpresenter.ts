module BMA {
    export module Presenters {
        export class LTLPresenter {

            keyframescompact: BMA.UIDrivers.IKeyframesList;
            appModel: BMA.Model.AppModel;
            currentdraggableelem: any;
            expandedResults: JQuery;

            tppresenter: BMA.LTL.TemporalPropertiesPresenter;
            statespresenter: BMA.LTL.StatesPresenter;

            constructor(
                commands: BMA.CommandRegistry,
                appModel: BMA.Model.AppModel,
                statesEditorDriver: BMA.UIDrivers.IStatesEditor,
                temporlapropertieseditor: BMA.UIDrivers.ITemporalPropertiesEditor,
                ltlviewer: BMA.UIDrivers.ILTLViewer,
                ltlresultsviewer: BMA.UIDrivers.ILTLResultsViewer,
                ajax: BMA.UIDrivers.IServiceDriver,
                popupViewer: BMA.UIDrivers.IPopup,
                exportService: BMA.UIDrivers.IExportService,
                fileLoaderDriver: BMA.UIDrivers.IFileLoader
                ) {

                var that = this;
                this.appModel = appModel;

                this.statespresenter = new BMA.LTL.StatesPresenter(commands, this.appModel, statesEditorDriver, ltlviewer.GetStatesViewer());

                temporlapropertieseditor.SetStates(appModel.States);
                commands.On("KeyframesChanged",(args) => {
                    temporlapropertieseditor.SetStates(args.states);
                });

                statesEditorDriver.SetModel(appModel.BioModel, appModel.Layout);
                window.Commands.On("AppModelChanged",(args) => {
                    statesEditorDriver.SetModel(appModel.BioModel, appModel.Layout);
                });

                
                //commands.On("LTLRequested", function (param: { formula }) {

                //    //var f = BMA.Model.MapVariableNames(param.formula, name => that.appModel.BioModel.GetIdByName(name));
                    
                //    var model = BMA.Model.ExportBioModel(appModel.BioModel);
                //    var proofInput = {
                //        "Name": model.Name,
                //        "Relationships": model.Relationships,
                //        "Variables": model.Variables,
                //        "Formula": param.formula,
                //        "Number_of_steps": 10
                //    }

                //    var result = ajax.Invoke(proofInput)
                //        .done(function (res) {
                //        if (res.Ticks == null) {
                //            alert(res.Error);
                //        }
                //        else {
                //            alert(res.Status);

                //            //if (res.Status == "True") {
                //                //var restbl = that.CreateColoredTable(res.Ticks);
                //                //ltlviewer.SetResult(restbl);
                //                //that.expandedResults = that.CreateExpanded(res.Ticks, restbl);
                //            //}
                //            //else {
                //                //ltlviewer.SetResult(undefined);
                //                //alert(res.Status);
                //            //}
                //        }
                //        })
                //        .fail(function () {
                //            alert("LTL failed");
                //        })
                //});
                

                window.Commands.On("Expand",(param) => {
                    switch (param) {
                        case "LTLStates":
                            statesEditorDriver.Show();
                            break;

                        case "LTLTempProp":
                            temporlapropertieseditor.Show();

                            commands.Execute("TemporalPropertiesEditorExpanded", {});

                            if (this.tppresenter === undefined) {
                                this.tppresenter = new BMA.LTL.TemporalPropertiesPresenter(
                                    commands,
                                    appModel,
                                    ajax,
                                    temporlapropertieseditor,
                                    this.statespresenter);
                            }

                            break;
                        default:
                            ltlviewer.Show(undefined);
                            break;
                    }
                });

                window.Commands.On("Collapse",(param) => {
                    temporlapropertieseditor.Hide();
                    statesEditorDriver.Hide();
                    ltlresultsviewer.Hide();
                    popupViewer.Hide();
                });

                commands.On("TemporalPropertiesOperationsChanged",(args) => {
                    ltlviewer.GetTemporalPropertiesViewer().SetOperations(args);
                });

                var ltlDataToExport = undefined;
                commands.On("ShowLTLResults",(args) => {
                    ltlDataToExport = {
                        ticks: args.ticks,
                        model: appModel.BioModel.Clone(),
                        layout: appModel.Layout.Clone()
                    };
                    ltlresultsviewer.SetData(appModel.BioModel, appModel.Layout, args.ticks, appModel.States);
                    ltlresultsviewer.Show();
                });

                ltlresultsviewer.SetOnExportCSV(function () {
                    if (ltlDataToExport !== undefined) {
                        exportService.Export(that.CreateCSV(ltlDataToExport, ","), "ltl", "csv");
                    }
                });

                commands.On("ExportLTLFormula",(args) => {
                    if (args.operation !== undefined) {
                        exportService.Export(JSON.stringify(args.operation), "operation", "txt");
                    }
                });

                commands.On("ImportLTLFormula",(args) => {
                    fileLoaderDriver.OpenFileDialog().done(function (fileName) {
                        var fileReader: any = new FileReader();
                        fileReader.onload = function () {
                            var fileContent = fileReader.result;
                            var obj = JSON.parse(fileContent);
                            var operation = that.DeserializeOperation(obj);

                            if (operation instanceof BMA.LTLOperations.Operation) {
                                var op = <BMA.LTLOperations.Operation>operation;
                                var states = that.GetStates(op);
                                var merged = that.MergeStates(that.appModel.States, states);
                                that.appModel.States = merged.states;
                                that.UpdateOperationStates(op, merged.map);
                                that.statespresenter.UpdateStatesFromModel();
                                that.tppresenter.AddOperation(op, args.position);
                            }

                        };
                        fileReader.readAsText(fileName);

                    });
                });
            }

            private UpdateOperationStates(operation: BMA.LTLOperations.Operation, map: any) {
                var that = this;
                for (var i = 0; i < operation.Operands.length; i++) {
                    var op = operation.Operands[i];
                    if (op instanceof BMA.LTLOperations.Keyframe) {
                        (<BMA.LTLOperations.Keyframe>op).Name = map[(<BMA.LTLOperations.Keyframe>op).Name];
                    } else if (op instanceof BMA.LTLOperations.Operation) {
                        that.UpdateOperationStates(<BMA.LTLOperations.Operation>op, map);
                    }
                }
            }

            private GetStates(operation: BMA.LTLOperations.Operation): BMA.LTLOperations.Keyframe[]{
                var that = this;
                var result = [];

                for (var i = 0; i < operation.Operands.length; i++) {
                    var op = operation.Operands[i];
                    if (op !== undefined && op !== null) {
                        if (op instanceof BMA.LTLOperations.Keyframe) {
                            result.push(<BMA.LTLOperations.Keyframe>op.Clone());
                        } else if (op instanceof BMA.LTLOperations.Operation) {
                            var states = that.GetStates(<BMA.LTLOperations.Operation>op);
                            for (var j = 0; j < states.length; j++) {
                                result.push(states[j]);
                            }
                        }
                    }
                }

                return result;
            }

            private MergeStates(currentStates: BMA.LTLOperations.Keyframe[], newStates: BMA.LTLOperations.Keyframe[]): { states: BMA.LTLOperations.Keyframe[]; map: any } {
                var result = {
                    states: [],
                    map: {}
                };

                result.states = currentStates;

                for (var i = 0; i < newStates.length; i++) {
                    var newState = newStates[i];
                    var exist = false;
                    for (var j = 0; j < currentStates.length; j++) {
                        var curState = currentStates[j];
                        if (curState.GetFormula() === newState.GetFormula()) {
                            exist = true;
                            result.map[newState.Name] = curState.Name;
                        }
                    }
                    if (!exist) {
                        var addedState = newState.Clone();
                        addedState.Name = String.fromCharCode(65 + result.states.length);
                        result.states.push(addedState); 
                        result.map[newState.Name] = addedState.Name;
                    }
                }

                return result;
            }

            private DeserializeOperation(obj): BMA.LTLOperations.IOperand {
                var that = this;
                if (obj === undefined)
                    throw "Invalid LTL Formula";

                switch (obj._type) {
                    case "NameOperand":
                        return new BMA.LTLOperations.NameOperand(obj.name);
                        break;
                    case "ConstOperand":
                        return new BMA.LTLOperations.ConstOperand(obj.const);
                        break;
                    case "KeyframeEquation":
                        var leftOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>that.DeserializeOperation(obj.leftOperand);
                        var rightOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>that.DeserializeOperation(obj.rightOperand);
                        var operator = <string>obj.operator;
                        return new BMA.LTLOperations.KeyframeEquation(leftOperand, operator, rightOperand);
                        break;
                    case "DoubleKeyframeEquation":
                        var leftOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>that.DeserializeOperation(obj.leftOperand);
                        var middleOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>that.DeserializeOperation(obj.middleOperand);
                        var rightOperand = <BMA.LTLOperations.NameOperand | BMA.LTLOperations.ConstOperand>that.DeserializeOperation(obj.rightOperand);
                        var leftOperator = <string>obj.leftOperator;
                        var rightOperator = <string>obj.rightOperator;
                        return new BMA.LTLOperations.DoubleKeyframeEquation(leftOperand, leftOperator, middleOperand, rightOperator, rightOperand);
                        break;
                    case "Keyframe":
                        var operands = [];
                        for (var i = 0; i < obj.operands.length; i++) {
                            operands.push(that.DeserializeOperation(obj.operands[i]));
                        }
                        return new BMA.LTLOperations.Keyframe(obj.name, obj.description, operands);
                        break;
                    case "Operation":
                        var operands = [];
                        for (var i = 0; i < obj.operands.length; i++) {
                            var operand = obj.operands[i];
                            if (operand === undefined || operand === null) {
                                operands.push(undefined);
                            } else {
                                operands.push(that.DeserializeOperation(operand));
                            }
                        }
                        var op = new BMA.LTLOperations.Operation();
                        op.Operands = operands;
                        op.Operator = window.OperatorsRegistry.GetOperatorByName(obj.operator.name);
                        return op;
                        break;
                    default:
                        break;
                }

                return undefined;
            }

            public CreateCSV(ltlDataToExport, sep): string {
                var csv = '';
                var that = this;

                var variables = (<BMA.Model.BioModel>ltlDataToExport.model).Variables;
                var ticks = ltlDataToExport.ticks.sort((x, y) => {
                    return x.Time < y.Time ? -1 : 1;
                });


                for (var i = 0; i < variables.length; i++) {

                    var variable = variables[i];
                    var cont = that.appModel.Layout.GetContainerById(variable.ContainerId);

                    if (cont !== undefined) {
                        csv += cont.Name + sep;
                    } else {
                        csv += '' + sep;
                    }

                    csv += variable.Name + sep;

                    for (var j = 0; j < ticks.length; j++) {
                        var tick = ticks[j].Variables;
                        for (var k = 0; k < tick.length; k++) {
                            var ij = tick[k];
                            if (ij.Id === variable.Id) {
                                if (ij.Lo === ij.Hi) {
                                    csv += ij.Lo + sep;
                                }
                                else {
                                    csv += ij.Lo + ' - ' + ij.Hi + sep;
                                }

                                break;
                            }
                        }
                    }

                    csv += "\n";
                }

                return csv;
            }
            

            /*
            public CreateColoredTable(ticks): any {
                var that = this;
                if (ticks === null) return undefined;
                var color = [];
                var t = ticks.length;
                var v = ticks[0].Variables.length;
                for (var i = 0; i < v; i++) {
                    color[i] = [];
                    for (var j = 1; j < t; j++) {
                        var ij = ticks[j].Variables[i];
                        var pr = ticks[j-1].Variables[i];
                        color[i][j] = pr.Hi === ij.Hi;
                    }
                }
                return color;
            }

            public CreateExpanded(ticks, color) {
                var container = $('<div></div>');
                if (ticks === null) return container;
                var that = this;
                var biomodel = this.appModel.BioModel;
                var variables = biomodel.Variables;
                var table = [];
                var colortable  = [];
                var header = [];
                var l = ticks.length;
                header[0] = "Name";
                for (var i = 0; i < ticks.length; i++) {
                    header[i + 1] = "T = " + ticks[i].Time;
                }
                for (var j = 0, len = ticks[0].Variables.length; j < len; j++) {
                    table[j] = [];
                    colortable[j] = [];
                    table[j][0] = biomodel.GetVariableById(ticks[0].Variables[j].Id).Name;
                    var v = ticks[0].Variables[j];
                    colortable[j][0] = undefined;
                    for (var i = 1; i < l+1; i++) {
                        var ij = ticks[i - 1].Variables[j];
                        colortable[j][i] = color[j][i - 1];
                        if (ij.Lo === ij.Hi) {
                            table[j][i] = ij.Lo;
                        }
                        else {
                            table[j][i] = ij.Lo + ' - ' + ij.Hi;
                        }
                    }
                }
                container.coloredtableviewer({ header: header, numericData: table, colorData: colortable });
                container.addClass('scrollable-results');
                container.children('table').removeClass('variables-table').addClass('proof-propagation-table ltl-result-table');
                container.find('td.propagation-cell-green').removeClass("propagation-cell-green");
                container.find('td.propagation-cell-red').removeClass("propagation-cell-red").addClass("change");
                container.find("td").eq(0).width(150);
                return container;
            }
            */
        }
    }
} 