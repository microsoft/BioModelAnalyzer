var BMA;
(function (BMA) {
    (function (Presenters) {
        var FurtherTestingPresenter = (function () {
            function FurtherTestingPresenter(driver, popupViewer) {
                var _this = this;
                this.num = 0;
                var that = this;
                this.driver = driver;
                this.popupViewer = popupViewer;

                window.Commands.On("ProofFailed", function (param) {
                    that.driver.ShowStartToggler();
                    that.model = param.Model;
                    that.result = param.Res;
                    that.variables = param.Variables;
                });

                window.Commands.On("ProofRequested", function () {
                    that.driver.HideStartToggler();
                    that.driver.HideResults();
                });

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.result.length !== 0 && that.model !== undefined && that.result !== undefined && that.variables !== undefined) {
                        that.driver.StandbyMode();

                        $.ajax({
                            type: "POST",
                            url: "api/FurtherTesting",
                            data: {
                                Model: that.model,
                                Analysis: that.result
                            },
                            success: function (res2) {
                                that.driver.ActiveMode();
                                that.driver.HideStartToggler();
                                that.data = res2.CounterExamples;

                                //$("#log").append("FurtherTesting success. " + res2.Status + "<br/>");
                                var result = that.ConvertCounterExamlpes(res2.CounterExamples);
                                var table = that.CreateVariablesView(that.variables, result);
                                that.driver.ShowResults(table);
                                that.data = table;
                            },
                            error: function (res2) {
                                that.driver.ActiveMode();
                                //$("#log").append("FurtherTesting error: " + res2.statusText + "<br/>");
                            }
                        });
                    } else
                        alert("No Variables");
                });

                window.Commands.On("Expand", function (param) {
                    switch (param) {
                        case "FurtherTesting":
                            that.driver.HideStartToggler();
                            that.driver.HideResults();
                            var content = $('<div></div>').coloredtableviewer({ numericData: that.data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                            _this.popupViewer.Show({ tab: param, content: content });
                            break;
                        default:
                            that.driver.ShowResults(that.data);
                            break;
                    }
                });

                window.Commands.On("Collapse", function (param) {
                    switch (param) {
                        case "FurtherTesting":
                            that.driver.ShowResults(that.data);
                            _this.popupViewer.Hide();
                            break;
                    }
                });
            }
            FurtherTestingPresenter.prototype.CreateVariablesView = function (variables, results) {
                var table = [];
                for (var i = 0; i < variables.length; i++) {
                    var resid = results[variables[i].Id];
                    table[i] = [];
                    table[i][0] = variables[i].ContainerId; // color should be there
                    table[i][1] = variables[i].Name;
                    table[i][2] = resid.min + '-' + resid.max;
                    table[i][3] = resid.oscillations;
                }
                return table;
            };

            FurtherTestingPresenter.prototype.ConvertCounterExamlpes = function (ex) {
                var variables = ex[0].Variables;
                var table = [];
                for (var j = 0; j < variables.length; j++) {
                    //table[i][j] = ex[i].Variables[j].Id + " " + ex[i].Variables[j].Value;
                    var parse = this.ParseId(variables[j].Id);
                    if (table[parseInt(parse[0])] === undefined)
                        table[parseInt(parse[0])] = [];

                    table[parseInt(parse[0])][parseInt(parse[1])] = variables[j].Value;
                }
                var result = [];
                for (var i = 0; i < table.length; i++) {
                    result[i] = { min: table[i][0], max: table[i][0], oscillations: "" };
                    if (table[i] !== undefined) {
                        for (var j = 0; j < table[i].length - 1; j++) {
                            if (table[i][j] < result[i].min)
                                result[i].min = table[i][j];
                            if (table[i][j] > result[i].max)
                                result[i].max = table[i][j];
                            result[i].oscillations += table[i][j] + ",";
                        }
                        result[i].oscillations += table[i][table[i].length - 1];
                    }
                }
                return result;
            };

            FurtherTestingPresenter.prototype.ParseId = function (id) {
                var parse = id.split('^');
                return parse;
            };

            FurtherTestingPresenter.prototype.FurtherTestingImitation = function (num) {
                var data = [];
                for (var i = 0; i < num; i++) {
                    data[i] = [];
                    data[i][0] = Math.round(Math.random());
                    data[i][1] = Math.round(Math.random());
                    data[i][2] = Math.round(Math.random());
                    data[i][3] = '';
                    for (var j = 0; j < 6; j++)
                        data[i][3] += Math.round(Math.random()) + ' ';
                }
                return data;
            };
            return FurtherTestingPresenter;
        })();
        Presenters.FurtherTestingPresenter = FurtherTestingPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=furthertestingpresenter.js.map
