﻿module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.ISimulationViewer;
            private full: BMA.UIDrivers.ISimulationFull;
            private data;
            private colors;
            private initValues;

            constructor(appModel: BMA.Model.AppModel, simulationFull: BMA.UIDrivers.ISimulationFull, simulationViewer: BMA.UIDrivers.ISimulationViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;
                this.viewer = simulationViewer;
                this.full = simulationFull;
                this.data = [];
                this.colors = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {
                    that.colors[param.ind].Seen = param.check;
                    that.viewer.ChangeVisibility(param);
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.data = [];
                    that.ClearColors();
                    that.initValues = param.data;
                    var stableModel = that.appModel.BioModel.GetJSON();
                    var variables = that.ConvertParam(param.data);
                    that.StartSimulation({ model: stableModel, variables: variables, num: param.num});
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.ClearColors();
                    that.CreateColors();
                    var variables = that.CreateVariablesView();
                    //var prmin = that.CreateProgressionMinTable();
                    that.viewer.SetData({ data: { variables: variables, colorData: undefined } });
                });

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        var variables = this.appModel.BioModel.Variables;
                        switch (param) {
                            case "SimulationVariables":
                                //that.ClearColors();
                                that.full.Set({ variables: variables, colors: that.colors, init: that.initValues });
                                full = that.full.GetViewer();//$('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable(), interval: that.CreateInterval(), init: that.initValues, data: that.data } });
                                break;
                            case "SimulationPlot":
                                full = $('<div id="SimulationPlot"></div>').height(500).simulationplot({colors: that.colors});
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
                    var variables = that.CreateVariablesView();
                    var colorData = that.CreateProgressionMinTable();
                    that.viewer.SetData({ data: { variables: variables, colorData: colorData }, plot: that.colors });
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
                                //window.Commands.Execute("AddResult", that.ConvertResult(res));
                                that.full.AddResult(res);
                                var d = that.ConvertResult(res);
                                that.addData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            }
                            else alert(res.ErrorMessages);
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
                    this.data[this.data.length] = d;
                    var variables = this.appModel.BioModel.Variables;
                    for (var i = 0; i < d.length; i++) {
                        var color = this.findColorById(variables[i].Id);
                        color.Plot[color.Plot.length] = d[i];
                    }
                }
            }

           

            public CreateColors() {
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    if (this.findColorById(variables[i].Id) === undefined) 
                    this.colors[i] =  {
                        Id: variables[i].Id,
                        Color: this.getRandomColor(),
                        Seen: true,
                        Plot: []
                    }
                }
            }

            public ClearColors() {
                for (var i = 0; i < this.colors.length; i++) {
                    this.colors[i].Plot = [];
                }
            }

            public findColorById(id) {
                for (var i = 0; i < this.colors.length; i++)
                    if (id === this.colors[i].Id)
                        return this.colors[i];
                return undefined;
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
                if (this.data.length < 1) return;
                for (var i = 0; i < this.data[0].length; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    for (var j = 1; j < this.data.length; j++) {
                        table[i][j] = this.data[j][i] !== this.data[j-1][i];
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
            public ConvertResult(res) {

                var data = [];
                if (res.Variables !== undefined && res.Variables !== null)
                    data = [];
                for (var i = 0; i < res.Variables.length; i++)
                    data[i] = res.Variables[i].Value;
                return data;
            }

            
        }
    }
}
 