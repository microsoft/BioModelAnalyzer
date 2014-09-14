module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.ISimulationViewer;

            constructor(appModel: BMA.Model.AppModel, simulationViewer: BMA.UIDrivers.ISimulationViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;
                this.viewer = simulationViewer;
                var that = this;



                window.Commands.On("RunSimulation", function(param) {
                    var simulate = that.ConvertModel(param);
                    

                    $.ajax({
                        type: "POST",
                        url: "api/Simulate",
                        data: simulate,
                        success: function (res) {
                            window.Commands.Execute("AddResult", that.ConvertResult(res));
                            //$("#log").append("Simulate success. Result variable count: " + res.Variables.Length + "<br/>");
                        },
                        error: function (res) {
                            //$("#log").append("Simulate error: " + res.statusText + "<br/>");
                        }
                    });
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView() } });
                });

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        if (param === "SimulationVariables")
                            full = $('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable(), interval: that.CreateInterval()}});//that.CreateFullResultTable(appModel.ProofResult.Ticks);
                        if (param === "SimulationPlot") {
                            full = $('<div id="SimulationPlot"></div>').text("Plot");
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
                    table[i][0] = "" // color should be there
                    table[i][1] = variables[i].ContainerId;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom + ' - ' + variables[i].RangeTo;
                }
                return table;
            }
            public CreateInterval() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = variables[i].RangeFrom;
                    table[i][1] = variables[i].RangeTo;
                }
                return table;
            }

            public ConvertResult (res) {
                var data = [];
                for (var i = 0; i < res.Variables.length; i++)
                    data[i] = res.Variables[i].Value;
                return data;
            }

            public ConvertModel(param) {

                var relationships = this.appModel.BioModel.Relationships;
                var rel = [];
                for (var i = 0; i < relationships.length; i++) {
                    rel[i] = {
                        "Id": i,
                        "FromVariableId": relationships[i].FromVariableId,
                        "ToVariableId": relationships[i].ToVariableId,
                        "Type": relationships[i].Type
                    }
                }
                var variables = this.appModel.BioModel.Variables;
                var vars = [];
                for (var i = 0; i < variables.length; i++) {
                    vars[i] = {
                        "Id": variables[i].Id,
                        "Name": variables[i].Name,
                        "RangeFrom": variables[i].RangeFrom,
                        "RangeTo": variables[i].RangeTo,
                        "Function": variables[i].Formula,
                    }
                }


                var stableModel = {
                    "ModelName": this.appModel.BioModel.Name,
                    "Engine": "VMCAI",
                    "Variables": vars,
                    "Relationships": rel
                }

                var valuespair = [];
                for (var i = 0; i < param.length; i++) {
                    valuespair[i] = {
                        "Id": i,
                        "Value": param[i]
                    }
                }

                var simulate = {
                    "Model": stableModel,
                    "Variables": valuespair//this.appModel.BioModel.Variables
                }

                return simulate;
            }

            public CreateFullTable() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][1] = variables[i].Name;
                    table[i][2] = variables[i].RangeFrom
                    table[i][3] = variables[i].RangeTo;
                }
                return table;
            }
        }
    }
}
 