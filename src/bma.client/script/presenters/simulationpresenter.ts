module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.ISimulationViewer;
            private data;
            private colors;
            private initValues;

            constructor(appModel: BMA.Model.AppModel, simulationViewer: BMA.UIDrivers.ISimulationViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;
                this.viewer = simulationViewer;
                this.data = [];
                this.colors = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {

                    //if (param.check && param.ind !== undefined) {
                    //    var plot = [];
                    //    plot[0] = that.data[param.ind];
                    //    simulationViewer.SetData({ plot: plot });
                    //}
                    that.colors[param.ind].Seen = param.check;

                });

                window.Commands.On("RunSimulation", function (param) {
                    that.data = [];
                    for (var i = 0; i < param.data.length; i++)
                        that.data[i] = [];
                    var stableModel = that.ConvertModel();
                    that.StartSimulation({model: stableModel, variables: that.ConvertParam(param.data), num: param.num});
                    that.initValues = param.data;
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.CreateColors();
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView(), colorData: that.CreateProgressionMinTable() } });
                });

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        switch (param) {
                            case "SimulationVariables":
                                full = $('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable(), interval: that.CreateInterval(), init: that.initValues, data: that.data } });
                                break;
                            case "SimulationPlot":
                                full = $('<div id="SimulationPlot"></div>').text("Plot will be there");
                                break;
                            default:
                                full = undefined;
                                simulationViewer.Show({ tab: undefined });
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

            public StartSimulation(param) {
                var that = this;
                if (param.num === undefined || param.num === 0) {
                    //alert(that.data[0].length);
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView(), colorData: that.CreateProgressionMinTable() }, plot: { data: that.data, colors: that.colors } });
                    return;
                }
                var simulate = {
                    "Model": param.model,
                    "Variables": param.variables
                }

                if (param.variables !== undefined && param.variables !== null)
                    $.ajax({
                        type: "POST",
                        url: "api/Simulate",
                        data: simulate,
                        success: function (res) {
                            if (res.Variables !== null) {
                                window.Commands.Execute("AddResult", that.ConvertResult(res));
                                var d = that.ConvertResult(res);
                                that.addData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            }
                            else alert("No relationships in the model");
                            //$("#log").append("Simulate success. Result variable count: " + res.Variables.Length + "<br/>");
                        },
                        error: function (res) {
                            console.log(res.statusText);
                            return;
                            //$("#log").append("Simulate error: " + res.statusText + "<br/>");
                        }
                    });
                else return;
            }

            public addData(d) {
                if (d !== null) {
                    if (this.data.length !== d.length)
                        alert("Error add results");
                    for (var i = 0; i < d.length; i++) {
                        this.data[i][this.data[i].length] = d[i];
                    }
                }
            }

            public CreatePlotView() {
            }

            public CreateColors() {
                this.colors = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    this.colors[i] =  {
                        Id: variables[i].Id,
                        Color: this.getRandomColor(),
                        Seen: true
                }
                }
            }

            public findColorById(id) {
                for (var i = 0; i < this.colors.length; i++)
                    if (id === this.colors[i].Id)
                        return this.colors[i];
            }

            public getRandomColor () {
                var letters = '0123456789ABCDEF'.split('');
                var color = '#';
                for (var i = 0; i < 6; i++) {
                    color += letters[Math.floor(Math.random() * 16)];
                }
                return color;
            }

            public CreateProgressionMinTable() {
                var table = [];
                for (var i = 0; i < this.data.length; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    for (var j = 1; j < this.data[i].length; j++) {
                        table[i][j] = this.data[i][j] !== this.data[i][j - 1];
                    }
                }
                
                return table;
            }

            public CreateVariablesView() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.findColorById(variables[i].Id).Color; // color should be there
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

            public ConvertResult(res) {

                var data = [];
                if (res.Variables !== undefined && res.Variables !== null)
                    data = [];
                    for (var i = 0; i < res.Variables.length; i++)
                        data[i] = res.Variables[i].Value;
                return data;
            }

            public ConvertParam(arr) {
                var res = [];
                for (var i = 0; i < arr.length; i++) {
                    res[i] = {
                        "Id": this.appModel.BioModel.Variables[i].Id,
                        "Value": arr[i]
                    }
                }
                return res;
            }

            public ConvertModel() {

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


                return stableModel;
            }

            public CreateFullTable() {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.findColorById(variables[i].Id).Color;
                    table[i][1] = this.findColorById(variables[i].Id).Seen;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom
                    table[i][4] = variables[i].RangeTo;
                }
                return table;
            }
        }
    }
}
 