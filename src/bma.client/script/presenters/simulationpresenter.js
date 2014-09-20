var BMA;
(function (BMA) {
    (function (Presenters) {
        var SimulationPresenter = (function () {
            function SimulationPresenter(appModel, simulationViewer, popupViewer) {
                var _this = this;
                this.appModel = appModel;
                this.viewer = simulationViewer;
                this.data = [];
                this.colors = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {
                    that.colors[param.ind].Seen = param.check;
                    that.viewer.SetData({
                        plot: that.colors
                    });
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.data = [];
                    that.initValues = param.data;
                    var stableModel = that.ConvertModel();
                    that.StartSimulation({ model: stableModel, variables: that.ConvertParam(param.data), num: param.num });
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.CreateColors();
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView(), colorData: that.CreateProgressionMinTable() } });
                });

                window.Commands.On("Expand", function (param) {
                    if (_this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        switch (param) {
                            case "SimulationVariables":
                                full = $('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable(), interval: that.CreateInterval(), init: that.initValues, data: that.data } });
                                break;
                            case "SimulationPlot":
                                full = $('<div id="SimulationPlot"></div>').simulationplot({ colors: that.colors });
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
                    //alert(that.data[0].length);
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView(), colorData: that.CreateProgressionMinTable() }, plot: that.colors });
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
                                window.Commands.Execute("AddResult", that.ConvertResult(res));
                                var d = that.ConvertResult(res);
                                that.addData(d);
                                that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                            } else
                                alert("No relationships in the model");
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

            SimulationPresenter.prototype.addData = function (d) {
                if (d !== null) {
                    this.data[this.data.length] = d;
                    var variables = this.appModel.BioModel.Variables;
                    for (var i = 0; i < d.length; i++) {
                        var color = this.findColorById(variables[i].Id);
                        color.Plot[color.Plot.length] = d[i];
                    }
                }
            };

            SimulationPresenter.prototype.CreatePlotView = function () {
                var data = [];
                for (var i = 0; i < this.colors.length; i++) {
                    data[i] = this.colors[i].Plot;
                }
                return data;
            };

            SimulationPresenter.prototype.CreateColors = function () {
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    if (this.findColorById(variables[i].Id) === undefined)
                        this.colors[i] = {
                            Id: variables[i].Id,
                            Color: this.getRandomColor(),
                            Seen: true,
                            Plot: []
                        };
                }
            };

            SimulationPresenter.prototype.findColorById = function (id) {
                for (var i = 0; i < this.colors.length; i++)
                    if (id === this.colors[i].Id)
                        return this.colors[i];
                return undefined;
            };

            SimulationPresenter.prototype.getRandomColor = function () {
                var letters = '0123456789ABCDEF'.split('');
                var color = '#';
                for (var i = 0; i < 6; i++) {
                    color += letters[Math.floor(Math.random() * 16)];
                }
                return color;
            };

            SimulationPresenter.prototype.CreateProgressionMinTable = function () {
                var table = [];
                for (var i = 0; i < this.data.length; i++) {
                    table[i] = [];
                    table[i][0] = false;
                    for (var j = 1; j < this.data[i].length; j++) {
                        table[i][j] = this.data[i][j] !== this.data[i][j - 1];
                    }
                }

                return table;
            };

            SimulationPresenter.prototype.CreateVariablesView = function () {
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
            };
            SimulationPresenter.prototype.CreateInterval = function () {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = variables[i].RangeFrom;
                    table[i][1] = variables[i].RangeTo;
                }
                return table;
            };

            SimulationPresenter.prototype.ConvertResult = function (res) {
                var data = [];
                if (res.Variables !== undefined && res.Variables !== null)
                    data = [];
                for (var i = 0; i < res.Variables.length; i++)
                    data[i] = res.Variables[i].Value;
                return data;
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

            SimulationPresenter.prototype.ConvertModel = function () {
                var relationships = this.appModel.BioModel.Relationships;
                var rel = [];
                for (var i = 0; i < relationships.length; i++) {
                    rel[i] = {
                        "Id": i,
                        "FromVariableId": relationships[i].FromVariableId,
                        "ToVariableId": relationships[i].ToVariableId,
                        "Type": relationships[i].Type
                    };
                }
                var variables = this.appModel.BioModel.Variables;
                var vars = [];
                for (var i = 0; i < variables.length; i++) {
                    vars[i] = {
                        "Id": variables[i].Id,
                        "Name": variables[i].Name,
                        "RangeFrom": variables[i].RangeFrom,
                        "RangeTo": variables[i].RangeTo,
                        "Function": variables[i].Formula
                    };
                }

                var stableModel = {
                    "ModelName": this.appModel.BioModel.Name,
                    "Engine": "VMCAI",
                    "Variables": vars,
                    "Relationships": rel
                };

                return stableModel;
            };

            SimulationPresenter.prototype.CreateFullTable = function () {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.findColorById(variables[i].Id).Color;
                    table[i][1] = this.findColorById(variables[i].Id).Seen;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom;
                    table[i][4] = variables[i].RangeTo;
                }
                return table;
            };
            return SimulationPresenter;
        })();
        Presenters.SimulationPresenter = SimulationPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=simulationpresenter.js.map
