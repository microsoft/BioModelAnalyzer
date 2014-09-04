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
                            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === "Stabilizing", res.Time, res.Ticks);
                            var numericData = that.CreateTableView();
                            //var result = appModel.ProofResult;
                            //var data = { numericData: numericData, colorData: undefined };
                            proofResultViewer.SetData({ issucceeded: result.IsStable, time: result.Time, data: {numericData: numericData } });
                            proofResultViewer.ShowResult(appModel.ProofResult);
                        },
                        error: function (res) {
                            alert("Error: " + res.statusText);
                            proofResultViewer.OnProofFailed();
                        } 
                    });
                });

                window.Commands.On("Expand", (param) => {
                    var full = that.CreateFullResultTable(appModel.ProofResult.Ticks);
                    proofResultViewer.Hide({ tab: param });
                    popupViewer.Show({ tab: param, type: "coloredTable", content: full });
                });

                window.Commands.On("Collapse", (param) => {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }

            public CreateTableView() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = variables[i].Name;
                    table[i][1] = variables[i].Formula;
                    table[i][2] = variables[i].RangeFrom + ' - ' + variables[i].RangeTo;
                }
                return table;
            }

            public CreateFullResultTable(ticks) {
                
                var container = $('<div></div>');
                if (ticks === null) return container;
                var that = this;
                var biomodel = this.appModel.BioModel;
                var variables = biomodel.Variables;
                var table = [];
                var header = [];
                var l = ticks.length;
                header[0] = "Name";
                for (var i = 0; i < ticks.length; i++) {
                    header[i + 1] = "T = " + i;
                }

                for (var j = 0; j < variables.length; j++) {
                    table[j] = [];
                    table[j][0] = biomodel.GetVariableById(ticks[0].Variables[variables.length - 1 - j].Id).Name;
                    for (var i = 0; i < l; i++) {
                        var ij = ticks[i].Variables[variables.length-1-j];
                        table[j][i + 1] = ij.Lo === ij.Hi ? ij.Lo : ij.Lo + ' - ' + ij.Hi;

                    }
                }

                container.coloredtableviewer({ header: header, numericData: table});
                return container;
            }
        }
    }
}
