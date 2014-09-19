var BMA;
(function (BMA) {
    (function (Presenters) {
        var SimulationPresenter = (function () {
            function SimulationPresenter(appModel, simulationViewer, popupViewer) {
                var _this = this;
                this.appModel = appModel;
                this.viewer = simulationViewer;
                this.data = [];
                var that = this;

                window.Commands.On("ChangePlotVariables", function (param) {
                    //if (param.check && param.ind !== undefined) {
                    //    var plot = [];
                    //    plot[0] = that.data[param.ind];
                    //    simulationViewer.SetData({ plot: plot });
                    //}
                });

                window.Commands.On("RunSimulation", function (param) {
                    that.data = [];
                    for (var i = 0; i < param.data.length; i++)
                        that.data[i] = [];
                    var stableModel = that.ConvertModel();
                    that.StartSimulation({ model: stableModel, variables: that.ConvertParam(param.data), num: param.num });
                });

                window.Commands.On("SimulationRequested", function (args) {
                    that.viewer.SetData({ data: { variables: that.CreateVariablesView() } });
                });

                window.Commands.On("Expand", function (param) {
                    if (_this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        if (param === "SimulationVariables")
                            full = $('<div id="SimulationFull"></div>').simulationfull({ data: { variables: that.CreateFullTable(), interval: that.CreateInterval() } }); //that.CreateFullResultTable(appModel.ProofResult.Ticks);
                        if (param === "SimulationPlot") {
                            full = $('<div id="SimulationPlot"></div>').text("Plot will be there");
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
                if (param.num === undefined || param.num === 0)
                    return;
                var simulate = {
                    "Model": param.model,
                    "Variables": param.variables
                };

                $.ajax({
                    type: "POST",
                    url: "api/Simulate",
                    data: simulate,
                    success: function (res) {
                        window.Commands.Execute("AddResult", that.ConvertResult(res));
                        that.StartSimulation({ model: param.model, variables: res.Variables, num: param.num - 1 });
                        var d = that.ConvertResult(res);
                        that.addData(d);
                        //$("#log").append("Simulate success. Result variable count: " + res.Variables.Length + "<br/>");
                    },
                    error: function (res) {
                        console.log(res.statusText);
                        return;
                        //$("#log").append("Simulate error: " + res.statusText + "<br/>");
                    }
                });
            };

            SimulationPresenter.prototype.addData = function (d) {
                if (this.data.length !== d.length)
                    alert("Error");
                for (var i = 0; i < d.length; i++) {
                    this.data[i][this.data[i].length] = d[i];
                }
            };

            SimulationPresenter.prototype.CreateVariablesView = function () {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = "";
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
                    table[i][1] = variables[i].Name;
                    table[i][2] = variables[i].RangeFrom;
                    table[i][3] = variables[i].RangeTo;
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
