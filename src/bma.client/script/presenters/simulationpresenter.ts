module BMA {
    export module Presenters {
        export class SimulationPresenter {
            private appModel: BMA.Model.AppModel;
            private compactViewer: BMA.UIDrivers.ISimulationViewer;
            private expandedViewer: BMA.UIDrivers.ISimulationExpanded;
            private ajax: BMA.UIDrivers.IServiceDriver;
            private initValues;
            private compactView;
            private expandedSimulationVariables: JQuery;
            private expandedSimulationPlot: JQuery;
            private currentModel: BMA.Model.BioModel;
            private logService: ISessionLog;
            private simulationAccordeon: JQuery;
            private messagebox: BMA.UIDrivers.IMessageServiсe;

            private variables: {
                Id: number;
                Color: string;
                Seen: boolean;
                Plot: number[];
                Init: number;
                Name: string;
            }[]

            constructor(
                appModel: BMA.Model.AppModel,
                simulationAccordeon: JQuery,
                simulationExpanded: BMA.UIDrivers.ISimulationExpanded,
                simulationViewer: BMA.UIDrivers.ISimulationViewer,
                popupViewer: BMA.UIDrivers.IPopup,
                ajax: BMA.UIDrivers.IServiceDriver,
                logService: BMA.ISessionLog,
                exportService: BMA.UIDrivers.ExportService,
                messagebox: BMA.UIDrivers.IMessageServiсe) {

                this.appModel = appModel;
                this.compactViewer = simulationViewer;
                this.expandedViewer = simulationExpanded;
                this.logService = logService;
                this.ajax = ajax;
                this.simulationAccordeon = simulationAccordeon;
                this.messagebox = messagebox;
                var that = this;
                this.initValues = [];


                window.Commands.On("ChangePlotVariables", function (param) {
                    that.variables[param.ind].Seen = param.check;
                    that.compactViewer.ChangeVisibility(param);
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.expandedViewer.StandbyMode();
                    that.ClearPlot(param.data);
                    try {
                        var stableModel = BMA.Model.ExportBioModel(that.appModel.BioModel);
                        var variables = that.ConvertParam(param.data);
                        logService.LogSimulationRun();
                        that.StartSimulation({ model: stableModel, variables: variables, num: param.num });
                    }
                    catch (ex) {
                        that.messagebox.Show(ex);
                        that.expandedViewer.ActiveMode();
                    }
                });

                window.Commands.On("SimulationRequested", function (args) {
                    if (that.CurrentModelChanged()) {

                        try {
                            var stableModel = BMA.Model.ExportBioModel(that.appModel.BioModel);
                        }
                        catch (ex) {
                            that.compactViewer.SetData({ data: undefined, plot: undefined, error: { title: "Invalid Model", message: ex } });
                            return;
                        }

                        that.simulationAccordeon.bmaaccordion({ contentLoaded: { ind: "#icon2", val: false } });
                        that.expandedSimulationVariables = undefined;
                        that.UpdateVariables();
                        that.compactView = that.CreateVariablesCompactView();
                        that.compactViewer.SetData({
                            data: {
                                variables: that.compactView,
                                colorData: undefined
                            },
                            plot: undefined,
                            error: undefined
                        });

                        var initValues = that.initValues;
                        that.expandedViewer.Set({
                            variables: that.GetSortedVars(),
                            colors: that.variables,
                            init: initValues
                        });
                        window.Commands.Execute("RunSimulation", { num: 10, data: initValues });
                    }
                    else {
                        var variables = that.CreateVariablesCompactView();
                        var colorData = that.CreateProgressionMinTable();
                        that.compactViewer.SetData({
                            data: { variables: variables, colorData: colorData },
                            plot: that.variables
                        });
                    }
                });

                window.Commands.On("Expand",(param) => {
                    if (this.appModel.BioModel.Variables.length !== 0) {
                        var full: JQuery = undefined;
                        switch (param) {
                            case "SimulationVariables":
                                if (that.expandedSimulationVariables !== undefined) {
                                    full = that.expandedSimulationVariables;
                                }
                                else {
                                    that.expandedViewer.Set({ variables: that.GetSortedVars(), colors: that.variables, init: that.initValues });
                                    full = that.expandedViewer.GetViewer();
                                }
                                break;
                            case "SimulationPlot":
                                full = $('<div></div>').simulationplot({ colors: that.variables });
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

                window.Commands.On("Collapse",(param) => {
                    simulationViewer.Show({ tab: param });
                    popupViewer.Hide();
                });

                window.Commands.On('ExportCSV', function () {
                    var csv = that.CreateCSV(',');
                    exportService.Export(csv, appModel.BioModel.Name, 'csv');
                });
            }

            public GetSortedVars() {
                var vars = this.appModel.BioModel.Variables.sort((x, y) => {
                    return x.Id < y.Id ? -1 : 1;
                });
                return vars;
            }

            public UpdateVariables() {
                var that = this;
                var vars = that.appModel.BioModel.Variables.sort((x, y) => {
                    return x.Id < y.Id ? -1 : 1;
                });
                that.variables = [];
                that.initValues = [];
                for (var i = 0; i < vars.length; i++) {
                    this.variables.push({
                        Id: vars[i].Id,
                        Color: this.getRandomColor(),
                        Seen: true,
                        Plot: [],
                        Init: vars[i].RangeFrom,
                        Name: vars[i].Name
                    });
                    this.variables[i].Plot[0] = this.variables[i].Init;
                    this.initValues[i] = this.variables[i].Init;
                }
            }

            public ClearPlot(init) {
                this.initValues = [];
                for (var i = 0; i < this.variables.length; i++) {
                    this.variables[i].Plot = [];
                    this.variables[i].Plot[0] = (init!==undefined) ? init[i] : this.variables[i].Init;
                    this.initValues.push(this.variables[i].Plot[0]);
                }
            }

            public GetById(arr, id) {
                for (var i = 0; i < arr.length; i++)
                    if (id === arr[i].Id)
                        return i;
                return undefined;
            }

            public CurrentModelChanged() {
                if (this.currentModel === undefined) {
                    this.Snapshot();
                    return true;
                }
                else {
                    try {
                        var q = JSON.stringify(BMA.Model.ExportBioModel(this.currentModel));
                        var w = JSON.stringify(BMA.Model.ExportBioModel(this.appModel.BioModel));
                        return q !== w;
                    }
                    catch (ex) {
                        this.Snapshot();
                        return true;
                    }
                }
            }

            public Snapshot() {
                this.currentModel = this.appModel.BioModel.Clone();
            }

            public StartSimulation(param) {
                var that = this;
                
                if (param.num === undefined || param.num === 0) {
                    var colorData = that.CreateProgressionMinTable();
                    that.compactViewer.SetData({
                        data: {
                            variables: that.compactView,
                            colorData: colorData
                        },
                        plot: that.variables,
                        error: undefined
                    });
                    that.expandedSimulationVariables = that.expandedViewer.GetViewer();
                    that.expandedViewer.ActiveMode();
                    that.Snapshot();
                    that.simulationAccordeon.bmaaccordion({ contentLoaded: { ind: "#icon2", val: true } });
                    return;
                }
                else {
                    var simulate = {
                        "Model": param.model,
                        "Variables": param.variables
                    }

                    if (param.variables !== undefined && param.variables !== null) {

                        var result = that.ajax.Invoke(simulate)
                            .done(function (res) {
                            if (res.Variables !== null) {
                                that.expandedViewer.AddResult(res);
                                var d = that.ConvertResult(res);
                                that.AddData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            }
                            else {
                                that.expandedViewer.ActiveMode();
                                alert("Simulation Error: " + res.ErrorMessages);
                            }
                        })
                            .fail(function (XMLHttpRequest, textStatus, errorThrown) {
                            this.logService.LogSimulationError();
                            console.log(textStatus);
                            that.expandedViewer.ActiveMode();
                            alert("Simulate error: " + errorThrown);
                            return;
                        });
                    }
                    else return;
                }
            }

            public AddData(d) {
                if (d !== null) {
                    for (var i = 0; i < d.length; i++) {
                        this.variables[i].Plot.push(d[i]);
                    }
                }
            }

            public CreateCSV(sep): string {
                var csv = '';
                var that = this;
                var data = this.variables;
                for (var i = 0, len = data.length; i < len; i++) {
                    var ivar = that.appModel.BioModel.GetVariableById(data[i].Id);
                    var contid = ivar.ContainerId;
                    var cont = that.appModel.Layout.GetContainerById(contid);
                    if (cont !== undefined) {
                        csv += cont.Name + sep;
                    }
                    else csv += '' + sep;
                    csv += ivar.Name + sep;
                    var plot = data[i].Plot;
                    for (var j = 0, plotl = plot.length; j < plotl; j++) {
                        csv += plot[j] + sep;
                    }
                    csv += "\n";
                }
                return csv;
            }

            public getRandomColor() {
                var r = this.GetRandomInt(0, 255);
                var g = this.GetRandomInt(0, 255);
                var b = this.GetRandomInt(0, 255);
                return "rgb(" + r + ", " + g + ", " + b + ")";
            }

            public GetRandomInt(min, max) {
                return Math.floor(Math.random() * (max - min + 1) + min);
            }

            public GetResults() {
                var res = [];
                for (var i = 0; i < this.variables.length; i++) {
                    res.push(this.variables[i].Plot);
                }
                return res;
            }

            public CreateProgressionMinTable() {
                var table = [];
                var res = this.GetResults();
                if (res.length < 1) return;
                for (var i = 0, len = res.length; i < len; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    var l = res[i].length;
                    for (var j = 1; j < l; j++) {
                        table[i][j] = res[i][j] !== res[i][j - 1];
                    }
                }
                return table;
            }

            public CreateVariablesCompactView() {
                var that = this;
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < this.variables.length; i++) {
                    var ivar = this.appModel.BioModel.GetVariableById(this.variables[i].Id);
                    table[i] = [];
                    table[i][0] = this.variables[i].Color; 
                    table[i][1] = (function () {
                        var cont = that.appModel.Layout.GetContainerById(ivar.ContainerId);
                        return cont !== undefined ? cont.Name : '';
                    })();
                    table[i][2] = ivar.Name;
                    table[i][3] = ivar.RangeFrom + ' - ' + ivar.RangeTo;
                }
                return table;
            }

            public ConvertParam(arr) {
                var res = [];
                for (var i = 0; i < arr.length; i++) {
                    res[i] = {
                        "Id": this.variables[i].Id,
                        "Value": arr[i]
                    }
                }
                return res;
            }
            public ConvertResult(res) {
                var data = [];
                if (res.Variables !== undefined && res.Variables !== null) {
                    for (var i = 0; i < res.Variables.length; i++)
                        data[i] = res.Variables[i].Value;
                }
                return data;
            }
        }
    }
}
 