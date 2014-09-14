﻿module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.ISimulationViewer;

            constructor(appModel: BMA.Model.AppModel, simulationViewer: BMA.UIDrivers.ISimulationViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;
                this.viewer = simulationViewer;
                var that = this;

                

                window.Commands.On("SimulationRequested", function (args) {
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView() } });
                });
                //window.Commands.On("ProofRequested", function (args) {
                //    //simulationViewer.OnProofStarted();

                //    var proofInput = appModel.BioModel.GetJSON()

                //    $.ajax({
                //        type: "POST",
                //        url: "api/Analyze",
                //        data: proofInput,
                //        success: function (res) {
                //            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === 4, res.Time, res.Ticks);
                //            var numericData = that.CreateTableView();
                //            var colorData = that.CreateColoredTable(res.Ticks);
                //            //var result = appModel.ProofResult;
                //            //var data = { numericData: numericData, colorData: undefined };
                //            simulationViewer.SetData({ issucceeded: result.IsStable, time: result.Time, data: { numericData: numericData, colorData: colorData } });
                //            //simulationViewer.ShowResult(appModel.ProofResult);
                //        },
                //        error: function (res) {
                //            alert("Error: " + res.statusText);
                //            //simulationViewer.OnProofFailed();
                //        }
                //    });
                //});

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        if (param === "SimulationVariables")
                            full = $('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable()}});//that.CreateFullResultTable(appModel.ProofResult.Ticks);
                        if (param === "SimulationPlot") {
                            full = $('<div id="SimulationPlot"></div>').text("In Plot We Trust");
                            //full = $('<div></div>').coloredtableviewer({ numericData: that.CreateTableView(), header: ["Name", "Formula", "Range"] });
                            //full.find("td").eq(0).width(150);
                            //full.find("td").eq(2).width(150);
                        }
                        if (full !== undefined) {
                            simulationViewer.Hide({ tab: param });
                            popupViewer.Show({ tab: param, content: full });
                        }
                    }
                });

                window.Commands.On("Collapse", (param) => {
                    simulationViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }

            public CreateVariablesView() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = ""//variables[i].Name;
                    table[i][1] = ""//variables[i].Formula;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom + ' - ' + variables[i].RangeTo;
                }
                return table;
            }

            //public CreateColoredTable(ticks): any {
            //    var that = this;
            //    if (ticks === null) return;
            //    var color = [];
            //    var t = ticks.length;
            //    var v = ticks[0].Variables.length;
            //    for (var i = 0; i < v; i++) {
            //        color[i] = [];
            //        for (var j = 0; j < t; j++) {
            //            var ij = ticks[j].Variables[v - 1 - i];
            //            color[i][j] = ij.Hi === ij.Lo;
            //        }
            //    }
            //    return color;
            //}

            public CreateFullTable() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = ""//variables[i].Name;
                    table[i][1] = variables[i].Name;
                    table[i][2] = variables[i].RangeFrom
                    table[i][3] = variables[i].RangeTo;
                }
                return table;
            }
        }
    }
}
 