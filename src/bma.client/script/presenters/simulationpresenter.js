var BMA;
(function (BMA) {
    (function (Presenters) {
        var SimulationPresenter = (function () {
            function SimulationPresenter(appModel, simulationExpanded, simulationViewer, popupViewer) {
                var _this = this;
                this.appModel = appModel;
                this.compactViewer = simulationViewer;
                this.expandedViewer = simulationExpanded;
                this.data = [];
                this.colors = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {
                    that.colors[param.ind].Seen = param.check;
                    that.compactViewer.ChangeVisibility(param);
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.expandedViewer.StandbyMode();
                    that.data = [];
                    that.ClearColors();
                    that.initValues = param.data;
                    var stableModel = that.appModel.BioModel.GetJSON();
                    var variables = that.ConvertParam(param.data);
                    that.StartSimulation({ model: stableModel, variables: variables, num: param.num });
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.initValues = [];
                    that.ClearColors();
                    that.CreateColors();
                    var variables = that.CreateVariablesView();

                    //var prmin = that.CreateProgressionMinTable();
                    that.compactViewer.SetData({ data: { variables: variables, colorData: undefined }, plot: undefined });
                });

                window.Commands.On("Expand", function (param) {
                    if (_this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        var variables = _this.appModel.BioModel.Variables;
                        switch (param) {
                            case "SimulationVariables":
                                //that.ClearColors();
                                that.expandedViewer.Set({ variables: variables, colors: that.colors, init: that.initValues });
                                full = that.expandedViewer.GetViewer(); //$('<div id="SimulationExpanded"></div>').simulationexpanded({ data: { variables: that.CreateExpandedTable(), interval: that.CreateInterval(), init: that.initValues, data: that.data } });
                                break;
                            case "SimulationPlot":
                                full = $('<div id="SimulationPlot"></div>').height(500).simulationplot({ colors: that.colors });
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

                window.Commands.On("Collapse", function (param) {
                    simulationViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }
            SimulationPresenter.prototype.StartSimulation = function (param) {
                var that = this;
                if (param.num === undefined || param.num === 0) {
                    var variables = that.CreateVariablesView();
                    var colorData = that.CreateProgressionMinTable();
                    that.compactViewer.SetData({ data: { variables: variables, colorData: colorData }, plot: that.colors });
                    that.expandedViewer.ActiveMode();
                    return;
                }
                var simulate = {
                    "Model": param.model,
                    "Variables": param.variables
                };

                if (param.variables !== undefined && param.variables !== null)
                    $.ajax({
                        type: "POST",
                        url: "api/Simulate",
                        data: simulate,
                        success: function (res) {
                            if (res.Variables !== null) {
                                //window.Commands.Execute("AddResult", that.ConvertResult(res));
                                that.expandedViewer.AddResult(res);
                                var d = that.ConvertResult(res);
                                that.AddData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            } else
                                alert(res.ErrorMessages);
                            //$("#log").append("Simulate success. Result variable count: " + res.Variables.Length + "<br/>");
                        },
                        error: function (res) {
                            console.log(res.statusText);
                            return;
                            //$("#log").append("Simulate error: " + res.statusText + "<br/>");
                        }
                    });
                else
                    return;
            };

            SimulationPresenter.prototype.AddData = function (d) {
                if (d !== null) {
                    this.data[this.data.length] = d;
                    var variables = this.appModel.BioModel.Variables;
                    for (var i = 0; i < d.length; i++) {
                        var color = this.GetColorById(variables[i].Id);
                        color.Plot[color.Plot.length] = d[i];
                    }
                }
            };

            SimulationPresenter.prototype.CreateColors = function () {
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    if (this.GetColorById(variables[i].Id) === undefined)
                        this.colors.push({
                            Id: variables[i].Id,
                            Color: this.getRandomColor(),
                            Seen: true,
                            Plot: []
                        });
                }
            };

            SimulationPresenter.prototype.ClearColors = function () {
                for (var i = 0; i < this.colors.length; i++) {
                    this.colors[i].Plot = [];
                }
            };

            SimulationPresenter.prototype.GetColorById = function (id) {
                for (var i = 0; i < this.colors.length; i++)
                    if (id === this.colors[i].Id)
                        return this.colors[i];
                return undefined;
            };

            SimulationPresenter.prototype.getRandomColor = function () {
                var r = this.GetRandomInt(0, 255);
                var g = this.GetRandomInt(0, 255);
                var b = this.GetRandomInt(0, 255);
                return "rgb(" + r + ", " + g + ", " + b + ")";
            };

            SimulationPresenter.prototype.GetRandomInt = function (min, max) {
                return Math.floor(Math.random() * (max - min + 1) + min);
            };

            SimulationPresenter.prototype.CreateProgressionMinTable = function () {
                var table = [];
                if (this.data.length < 1)
                    return;
                for (var i = 0; i < this.data[0].length; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    for (var j = 1; j < this.data.length; j++) {
                        table[i][j] = this.data[j][i] !== this.data[j - 1][i];
                    }
                }
                return table;
            };

            SimulationPresenter.prototype.CreateVariablesView = function () {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.GetColorById(variables[i].Id).Color; // color should be there
                    table[i][1] = variables[i].ContainerId;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom + ' - ' + variables[i].RangeTo;
                }
                return table;
            };

            SimulationPresenter.prototype.ConvertParam = function (arr) {
                var res = [];
                for (var i = 0; i < arr.length; i++) {
                    res[i] = {
                        "Id": this.appModel.BioModel.Variables[i].Id,
                        "Value": arr[i]
                    };
                }
                return res;
            };
            SimulationPresenter.prototype.ConvertResult = function (res) {
                var data = [];
                if (res.Variables !== undefined && res.Variables !== null)
                    data = [];
                for (var i = 0; i < res.Variables.length; i++)
                    data[i] = res.Variables[i].Value;
                return data;
            };
            return SimulationPresenter;
        })();
        Presenters.SimulationPresenter = SimulationPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=simulationpresenter.js.map
