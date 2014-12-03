module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private compactViewer: BMA.UIDrivers.ISimulationViewer;
            private expandedViewer: BMA.UIDrivers.ISimulationExpanded;
            private ajax: BMA.UIDrivers.IServiceDriver;
            private colors;
            private initValues;
            private dataForPlot;
            private results;
            private expandedSimulationVariables: JQuery;
            private expandedSimulationPlot: JQuery;
            private currentModel: BMA.Model.BioModel;

            constructor(appModel: BMA.Model.AppModel, simulationExpanded: BMA.UIDrivers.ISimulationExpanded, simulationViewer: BMA.UIDrivers.ISimulationViewer, popupViewer: BMA.UIDrivers.IPopup, ajax: BMA.UIDrivers.IServiceDriver) {
                this.appModel = appModel;
                this.compactViewer = simulationViewer;
                this.expandedViewer = simulationExpanded;
                this.ajax = ajax;
                this.colors = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {
                    that.colors[param.ind].Seen = param.check;
                    that.compactViewer.ChangeVisibility(param);
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.expandedViewer.StandbyMode();
                    that.results = [];
                    that.initValues = param.data;
                    that.ClearColors();
                    var stableModel = that.appModel.BioModel.GetJSON();
                    var variables = that.ConvertParam(param.data);
                    that.StartSimulation({ model: stableModel, variables: variables, num: param.num});
                });

                window.Commands.On("SimulationRequested", function (args) {
                    if (that.CurrentModelChanged()) {
                        that.initValues = [];
                        that.results = [];
                        that.expandedSimulationVariables = undefined;
                        that.CreateColors();
                        that.ClearColors();
                        that.dataForPlot = that.CreateDataForPlot(that.colors, that.appModel.BioModel.Variables);
                        var variables = that.CreateVariablesView();
                        that.compactViewer.SetData({ data: { variables: variables, colorData: undefined }, plot: undefined });
                    }
                });

                window.Commands.On("Expand", (param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full: JQuery = undefined;
                        var variables = this.appModel.BioModel.Variables.sort((x, y) => {
                            return x.Id < y.Id ? -1 : 1;
                        });
                        switch (param) {
                            case "SimulationVariables":
                                if (that.expandedSimulationVariables !== undefined) {
                                    full = that.expandedSimulationVariables;
                                }
                                else {
                                    that.expandedViewer.Set({ variables: variables, colors: that.dataForPlot, init: that.initValues });
                                    full = that.expandedViewer.GetViewer();
                                }
                                break;
                            case "SimulationPlot":
                                full = $('<div id="SimulationPlot"></div>').height('100%').simulationplot({ colors: that.dataForPlot });
                                break;
                            default:
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


            public CurrentModelChanged() {
                if (this.currentModel === undefined) {
                    this.Snapshot();
                    return true;
                }
                else
                    return JSON.stringify(this.currentModel.GetJSON()) !== JSON.stringify(this.appModel.BioModel.GetJSON());
            }

            public Snapshot() {
                this.currentModel = this.appModel.BioModel.Clone();
            }

            public StartSimulation(param) {
                var that = this;
                if (param.num === undefined || param.num === 0) {
                    var variables = that.CreateVariablesView();
                    var colorData = that.CreateProgressionMinTable();
                    that.dataForPlot = that.CreateDataForPlot(that.colors, that.appModel.BioModel.Variables);
                    that.compactViewer.SetData({ data: { variables: variables, colorData: colorData }, plot: that.dataForPlot });
                    that.expandedViewer.ActiveMode();
                    that.expandedSimulationVariables = that.expandedViewer.GetViewer();
                    return;
                }
                var simulate = {
                    "Model": param.model,
                    "Variables": param.variables
                }

                if (param.variables !== undefined && param.variables !== null)

                    var result = that.ajax.Invoke(simulate)
                        .done(function (res) {
                            if (res.Variables !== null) {
                                that.results.push(res);
                                that.expandedViewer.AddResult(res);
                                var d = that.ConvertResult(res);
                                that.AddData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            }
                            else {
                                that.expandedViewer.ActiveMode();
                                alert ("Simulation Error: " + res.ErrorMessages);
                            }
                        })
                        .fail(function (XMLHttpRequest, textStatus, errorThrown) {
                            console.log(textStatus);
                            that.expandedViewer.ActiveMode();
                            alert("Simulate error: " + errorThrown);
                            return;
                        });
                else return;
            }

            public AddData(d) {
                if (d !== null) {
                    var variables = this.appModel.BioModel.Variables;
                    for (var i = 0; i < d.length; i++) {
                        var color = this.colors[this.GetColorById(variables[i].Id)];
                        color.Plot.push(d[i]);
                    }
                }
                return color;
            }

            public CreateDataForPlot(colors: { Id; Colors; Seen; Plot }, variables: BMA.Model.Variable[]) {
                var result = [];
                for (var i = 0; i < variables.length; i++) {
                    result.push(colors[this.GetColorById(variables[i].Id)]);
                }
                return result;
            }

            public CreateColors() {
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    if (this.GetColorById(variables[i].Id) === undefined)
                        this.colors.push({
                            Id: variables[i].Id,
                            Name: variables[i].Name,
                            Color: this.getRandomColor(),
                            Seen: true,
                            Plot: []
                        })
                }
            }

            public ClearColors() {
                for (var i = 0; i < this.colors.length; i++) {
                    this.colors[i].Plot = [];
                }
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var color = this.GetColorById(variables[i].Id);
                    this.colors[color].Plot = [];
                    this.colors[color].Name = variables[i].Name;
                    if (this.initValues[i] !== undefined)
                        this.colors[color].Plot[0] = this.initValues[i];
                }
            }

            public GetColorById(id) {
                for (var i = 0; i < this.colors.length; i++)
                    if (id === this.colors[i].Id)
                        return i;
                return undefined;
            }

            public getRandomColor() {
                var r = this.GetRandomInt(0, 255);
                var g = this.GetRandomInt(0, 255);
                var b = this.GetRandomInt(0, 255);
                return "rgb(" + r + ", " + g + ", " + b + ")";
            }

            public GetRandomInt (min, max) {
                return Math.floor(Math.random() * (max - min + 1) + min);
            }

            public CreateProgressionMinTable() {
                var table = [];
                if (this.results.length < 1) return;
                for (var i = 0; i < this.results[0].Variables.length; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    var l = this.results.length;
                    for (var j = 1; j < l; j++) {
                        table[i][j] = this.results[j].Variables[i].Value !== this.results[j-1].Variables[i].Value;
                    }
                }
                return table;
            }

            public CreateVariablesView() {
                var that = this;
                var table = [];
                var variables = this.appModel.BioModel.Variables.sort((x, y) => {
                    return x.Id < y.Id ? -1 : 1;
                });
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.colors[this.GetColorById(variables[i].Id)].Color; // color should be there
                    table[i][1] = (function () {
                        var cont = that.appModel.Layout.GetContainerById(variables[i].ContainerId);
                        return cont !== undefined ? cont.Name : '';
                    })();
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
 