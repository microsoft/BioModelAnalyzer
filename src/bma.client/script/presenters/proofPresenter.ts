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
                            if (res.Status !== 4) window.Commands.Execute("ProofFailed", that.appModel.BioModel.Variables);
                            //else window.Commands.Execute("ProofSucceeded", {});
                            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === 4, res.Time, res.Ticks);
                            //if (res.Ticks !== null)
                            var variablesData = that.CreateTableView(res.Ticks);
                            var colorData = that.CreateColoredTable(res.Ticks);
                            //var result = appModel.ProofResult;
                            //var data = { numericData: numericData, colorData: undefined };
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
                                    full = that.CreateFullResultTable(appModel.ProofResult.Ticks);
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

            public CreateTableView(ticks) {
                var table = [];
                if (ticks === null) return { numericData: undefined, colorData: undefined };
                var variables = this.appModel.BioModel.Variables;
                var color = [];
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    color[i] = [];
                    table[i][0] = variables[i].Name;
                    table[i][1] = variables[i].Formula;
                    var range;
                    var ij = ticks[ticks.length - 1].Variables[variables.length - 1 - i];
                    var c = ij.Lo === ij.Hi;
                    if (c) {
                        range = ij.Lo;

                    }
                    else {
                        range = ij.Lo + ' - ' + ij.Hi;
                        for (var j = 0; j < 3; j++)
                            color[i][j] = c;
                    }
                    
                    table[i][2] = range;
                    
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
                        var ij = ticks[j].Variables[v - 1 - i];
                        color[i][j] = ij.Hi === ij.Lo;
                    }
                }
                return color;
            }

            public CreateFullResultTable(ticks) {
                
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
                        var ij = ticks[i].Variables[variables.length - 1 - j];
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
