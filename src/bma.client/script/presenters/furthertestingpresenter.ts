module BMA {
    export module Presenters {
        export class FurtherTestingPresenter {

            private driver: BMA.UIDrivers.IFurtherTesting;
            private popupViewer: BMA.UIDrivers.IPopup;
            private num: number = 0;
            private data;
            private model;
            private result;
            private variables;

            constructor(driver: BMA.UIDrivers.IFurtherTesting, popupViewer: BMA.UIDrivers.IPopup) {
                var that = this;
                this.driver = driver;
                this.popupViewer = popupViewer;

                window.Commands.On("ProofFailed", function (param: { Model; Variables; Res }) {
                    that.driver.ShowStartToggler();
                    that.model = param.Model;
                    that.result = param.Res;
                    that.variables = param.Variables;
                })

                window.Commands.On("ProofRequested", function () {
                    that.driver.HideStartToggler();
                    that.driver.HideResults();
                })

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.result.length !== 0 && that.model !== undefined && that.result !== undefined && that.variables !== undefined) {
                        that.driver.StandbyMode();
                        $.ajax({
                            type: "POST",
                            url: "api/FurtherTesting",
                            data: {
                                Model: that.model,
                                Analysis: that.result,
                            },
                            success: function (res2) {
                                that.driver.ActiveMode();
                                if (res2.CounterExamples !== null) {
                                    that.driver.HideStartToggler();
                                    that.data = res2.CounterExamples;
                                    //$("#log").append("FurtherTesting success. " + res2.Status + "<br/>");
                                    var result = that.ConvertCounterExamlpes(res2.CounterExamples);
                                    var table = that.CreateVariablesView(that.variables, result);
                                    that.driver.ShowResults(table);
                                    that.data = table;
                                }
                                else alert(res2.Error);
                            },
                            error: function (res2) {
                                that.driver.ActiveMode();
                                alert(res2.statusText);
                                //$("#log").append("FurtherTesting error: " + res2.statusText + "<br/>");
                            }
                        });
                    }
                    else alert("No Variables");
                })

                window.Commands.On("Expand", (param) => {
                        switch (param) {
                            case "FurtherTesting":
                                that.driver.HideStartToggler();
                                that.driver.HideResults();
                                var content = $('<div></div>').coloredtableviewer({ numericData: that.data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                                this.popupViewer.Show({ tab: param, content: content });
                                break;
                            default:
                                that.driver.ShowResults(that.data);
                                break;
                        }
                })

                window.Commands.On("Collapse", (param) => {
                    switch (param) {
                        case "FurtherTesting":
                            that.driver.ShowResults(that.data);
                            this.popupViewer.Hide();
                            break;
                    }
                })
            }

            public CreateVariablesView(variables, results) {
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
            }

            private ConvertCounterExamlpes(ex) {
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
                    if (table[i] !== undefined) {
                        result[i] = { min: table[i][0], max: table[i][0],oscillations:""};
                        for (var j = 0; j < table[i].length - 1; j++) {
                            if (table[i][j] < result[i].min) result[i].min = table[i][j];
                            if (table[i][j] > result[i].max) result[i].max = table[i][j];
                            result[i].oscillations += table[i][j] + ",";
                        }
                        result[i].oscillations += table[i][table[i].length - 1];
                    }
                }
                return result;
            }

            private ParseId(id) {
                var parse = id.split('^');
                return parse;
            }

            private FurtherTestingImitation(num) {
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
            }
        }
    }
}