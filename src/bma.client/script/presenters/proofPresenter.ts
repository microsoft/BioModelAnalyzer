module BMA {
    export module Presenters {
        export class ProofPresenter { 
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.IProofResultViewer;

            constructor(appModel: BMA.Model.AppModel, proofResultViewer: BMA.UIDrivers.IProofResultViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;
                var that = this;

                window.Commands.On("ProofRequested", function (args) {
                    proofResultViewer.OnProofStarted();
                    var proofInput = appModel.BioModel.GetJSON()

                    $.ajax({
                        type: "POST",
                        url: "api/Analyze",
                        data: proofInput,
                        success: function (res) {
                            //else window.Commands.Execute("ProofSucceeded", {});
                            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === 4, res.Time, res.Ticks);
                            //if (res.Ticks !== null)
                            if (res.Status === 5)
                                window.Commands.Execute("ProofFailed", { Model: proofInput, Res: res, Variables: that.appModel.BioModel.Variables });
                            var st = that.Stability(res.Ticks);
                            var variablesData = that.CreateTableView(st.variablesStability);
                            var colorData = that.CreateColoredTable(res.Ticks);
                            //var result = appModel.ProofResult;
                            //var data = { numericData: numericData, colorData: undefined };
                            window.Commands.Execute("DrawingSurfaceSetProofResults", st);
                            proofResultViewer.SetData({ issucceeded: result.IsStable, time: result.Time, data: { numericData: variablesData.numericData, colorVariables: variablesData.colorData,  colorData: colorData } });
                            proofResultViewer.ShowResult(appModel.ProofResult);
                        },
                        error: function (res) {
                            alert("Error: " + res.statusText);
                            proofResultViewer.OnProofFailed();
                        } 
                    });
                });

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        switch (param) {
                            case "ProofPropagation":
                                if (this.appModel.ProofResult.Ticks !== null)
                                    full = that.CreateExpandedResultTable(appModel.ProofResult.Ticks);
                                break;
                            case "ProofVariables":
                                var variablesData = that.CreateTableView(appModel.ProofResult.Ticks);
                                full = $('<div></div>').coloredtableviewer({ numericData: variablesData.numericData, colorData: variablesData.colorData, header: ["Name", "Formula", "Range"] });
                                full.find("td").eq(0).width(150);
                                full.find("td").eq(2).width(150);
                                break;
                            default:
                                full = undefined;
                                proofResultViewer.Show({ tab: undefined });
                                break;
                        }
                        if (full !== undefined) {
                            proofResultViewer.Hide({ tab: param });
                            popupViewer.Show({ tab: param, content: full });
                        }
                    }
                });

                window.Commands.On("Collapse", (param) => {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }

            public Stability(ticks) {
                var containers = [];
                if (ticks === null) return undefined;
                var variables = this.appModel.BioModel.Variables;
                var stability = [];
                for (var i = 0; i < variables.length; i++) {
                    var ij = ticks[0].Variables[variables.length - 1 - i];
                    var c = ij.Lo === ij.Hi;
                    var range = '';
                    if (c) {
                        range = ij.Lo;

                    }
                    else {
                        range = ij.Lo + ' - ' + ij.Hi;
                    }
                    stability[i] = { state: c, range: range };
                    var id = ticks[0].Variables[variables.length - 1 - i].Id;
                    var v = this.appModel.BioModel.GetVariableById(id);
                    if (v.ContainerId !== undefined &&  (!c || containers[v.ContainerId] === undefined)) 
                            containers[v.ContainerId] = c;
                        
                }
                return {variablesStability: stability, containersStability: containers};
            }


            public CreateTableView(stability) {
                var table = [];
                if (stability === undefined) return { numericData: undefined, colorData: undefined };
                var variables = this.appModel.BioModel.Variables;
                var color = [];
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    color[i] = [];
                    table[i][0] = variables[i].Name;
                    table[i][1] = variables[i].Formula;
                    var range = '';
                    //var ij = ticks[0].Variables[variables.length - 1 - i];
                    var c = stability[i].state;
                    if (!c) {
                        for (var j = 0; j < 3; j++)
                            color[i][j] = c;
                    }
                    
                    table[i][2] = stability[i].range;
                    
                }
                return {numericData: table, colorData:color};
            }

            public CreateColoredTable(ticks): any {
                var that = this;
                if (ticks === null) return;
                var color = [];
                var t = ticks.length;
                var v = ticks[0].Variables.length;
                for (var i = 0; i < v; i++) {
                    color[i] = [];
                    for (var j = 0; j < t; j++) {
                        var ij = ticks[t-j-1].Variables[v - 1 - i];
                        color[i][j] = ij.Hi === ij.Lo;
                    }
                }
                return color;
            }

            public CreateExpandedResultTable(ticks) {
                
                var container = $('<div></div>');
                if (ticks === null) return container;
                var that = this;
                var biomodel = this.appModel.BioModel;
                var variables = biomodel.Variables;
                var table = [];
                var color = [];
                var header = [];
                var l = ticks.length;
                header[0] = "Name";
                for (var i = 0; i < ticks.length; i++) {
                    header[i + 1] = "T = " + i;
                }
                for (var j = 0; j < variables.length; j++) {
                    table[j] = [];
                    color[j] = [];
                    table[j][0] = biomodel.GetVariableById(ticks[0].Variables[variables.length - 1 - j].Id).Name;
                    for (var i = 0; i < l; i++) {
                        var ij = ticks[l-i-1].Variables[variables.length - 1 - j];
                        if (ij.Lo === ij.Hi) {
                            table[j][i + 1] = ij.Lo;
                            color[j][i + 1] = true;
                        }
                        else {
                            table[j][i + 1] = ij.Lo + ' - ' + ij.Hi;
                            color[j][i + 1] = false;
                        }
                    }
                }

                container.coloredtableviewer({ header: header, numericData: table, colorData: color });

                container.find("td").eq(0).width(150);
                return container;
            }
        }
    }
}
