module BMA {
    export module Presenters {
        export class FurtherTestingPresenter {

            private driver: BMA.UIDrivers.IFurtherTesting;
            private popupViewer: BMA.UIDrivers.IPopup;
            private ajax: BMA.UIDrivers.IServiceDriver;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private appModel: BMA.Model.AppModel;
            private num: number = 0;
            private data;
            private model;
            private result;
            private variables;

            constructor(
                appModel: BMA.Model.AppModel,
                driver: BMA.UIDrivers.IFurtherTesting,
                popupViewer: BMA.UIDrivers.IPopup,
                ajax: BMA.UIDrivers.IServiceDriver,
                messagebox: BMA.UIDrivers.IMessageServiсe,
                logService: BMA.ISessionLog
            ) {
                var that = this;
                this.appModel = appModel;
                this.driver = driver;
                this.popupViewer = popupViewer;
                this.ajax = ajax;
                this.messagebox = messagebox;


                window.Commands.On("ProofFailed", function (param: { Model; Variables; Res }) {
                    if (param !== undefined) {
                        that.driver.ShowStartFurtherTestingToggler();
                        that.model = param.Model;
                        that.result = param.Res;
                        that.variables = param.Variables;
                    }
                    else {
                        that.data = undefined;
                    }
                })

                function OnProofStarting() {
                    that.driver.ActiveMode();
                    that.driver.HideStartFurtherTestingToggler();
                    that.driver.HideResults();
                    that.data = undefined;
                }

                window.Commands.On("ProofStarting", function () {
                    OnProofStarting();
                })

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.result.length !== 0 && that.model !== undefined && that.result !== undefined && that.variables !== undefined) {
                        that.driver.StandbyMode();
                        logService.LogFurtherTestingRun();
                        var result = that.ajax.Invoke( {
                            Model: that.model,
                            Analysis: that.result,
                        })
                            .done(function (res2) {
                                that.driver.ActiveMode();
                                if (res2.CounterExamples !== null) {
                                    that.driver.HideStartFurtherTestingToggler();
                                    if (res2.CounterExamples.length === 0) {
                                        
                                    }
                                    else {
                                        var bif = null, osc = null, fix = null;
                                        for (var i = 0; i < res2.CounterExamples.length; i++) {
                                            switch (res2.CounterExamples[i].Status) {
                                                case "Bifurcation":
                                                    bif = res2.CounterExamples[i];
                                                    break;
                                                case "Cycle":
                                                    osc = res2.CounterExamples[i];
                                                    break;
                                                case "Fixpoint":
                                                    fix = res2.CounterExamples[i];
                                                    break;
                                            }
                                        }

                                        var data = [];
                                        var headers = [];
                                        var tabLabels = [];

                                        if (bif !== null) {
                                            var parseBifurcations = that.ParseBifurcations(bif.Variables);
                                            var bifurcationsView = that.CreateBifurcationsView(that.variables, parseBifurcations);
                                            data.push(bifurcationsView);
                                            headers.push(["Cell", "Name", "Calculated Bound", "Fix1", "Fix2"]);
                                            var label = $('<div></div>').addClass('bma-futhertesting-bifurcations-icon');
                                            tabLabels.push(label);
                                        }
                                        if (osc !== null) {
                                            var parseOscillations = that.ParseOscillations(osc.Variables);
                                            var oscillationsView = that.CreateOscillationsView(that.variables, parseOscillations);
                                            data.push(oscillationsView);
                                            headers.push(["Cell", "Name", "Calculated Bound", "Oscillation"]);
                                            var label = $('<div></div>').addClass('bma-futhertesting-oscillations-icon');
                                            tabLabels.push(label);
                                        }

                                        if (fix !== null && bif === null && osc === null) {
                                            var parseFix = that.ParseFixPoint(fix.Variables);
                                            window.Commands.Execute("ProofByFurtherTesting", {
                                                issucceeded: true,
                                                message: 'Further testing has been determined the model to be stable with the following stable state',
                                                fixPoint: parseFix
                                            });
                                            OnProofStarting();
                                        }
                                        else {

                                            that.data = { tabLabels: tabLabels, tableHeaders: headers, data: data };
                                            that.driver.ShowResults(that.data);
                                        }
                                    }
                                }
                                else {
                                    that.driver.ActiveMode();
                                    that.messagebox.Show(res2.Error);
                                }
                            })
                            .fail(function (XMLHttpRequest, textStatus, errorThrown) {
                                that.driver.ActiveMode();
                                that.messagebox.Show(errorThrown);
                            });

                    }
                    else that.messagebox.Show("No Variables");
                })

                window.Commands.On("Expand", (param) => {
                        switch (param) {
                            case "FurtherTesting":
                                that.driver.HideStartFurtherTestingToggler();
                                that.driver.HideResults();
                                var content = $('<div></div>').furthertesting();
                                content.furthertesting("SetData", that.data);
                                content.find("*").removeClass("scrollable-results");
                                var full = content.children().eq(1).children().eq(1).addClass("proof-expanded");
                                this.popupViewer.Show({ tab: param, content: full });
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

            public CreateOscillationsView(variables, results) {
                var that = this;
                var table = [];
                for (var i = 0; i < variables.length; i++) {
                    var resid = results[variables[i].Id];
                    table[i] = [];
                    table[i][0] = (function () {
                        var cont = that.appModel.Layout.GetContainerById(variables[i].ContainerId);
                        return cont !== undefined ? cont.Name : '';
                    })(); 
                    table[i][1] = variables[i].Name;
                    table[i][2] = resid.min + '-' + resid.max;
                    table[i][3] = resid.oscillations;
                }
                return table;
            }

            private CreateBifurcationsView(variables, results) {
                var that = this;
                var table = [];
                for (var i = 0; i < variables.length; i++) {
                    var resid = results[variables[i].Id];
                    table[i] = [];
                    table[i][0] = (function () {
                        var cont = that.appModel.Layout.GetContainerById(variables[i].ContainerId);
                        return cont !== undefined ? cont.Name : '';
                    })();
                    table[i][1] = variables[i].Name;
                    if (resid.min !== resid.max)
                        table[i][2] = resid.min + '-' + resid.max;
                    else
                        table[i][2] = resid.min;
                    table[i][3] = resid.Fix1;
                    table[i][4] = resid.Fix2;
                }
                return table;

            }

            private ParseBifurcations(variables) {
                var table = [];
                for (var j = 0; j < variables.length; j++) {
                    var parse = this.ParseId(variables[j].Id);
                    if (table[parseInt(parse[0])] === undefined)
                        table[parseInt(parse[0])] = [];
                    table[parseInt(parse[0])][0] = parseInt(variables[j].Fix1);
                    table[parseInt(parse[0])][1] = parseInt(variables[j].Fix2);
                }
                var result = [];
                for (var i = 0; i < table.length; i++) {
                    if (table[i] !== undefined) {
                        result[i] = {
                            min: Math.min(table[i][0], table[i][1]),
                            max: Math.max(table[i][0], table[i][1]),
                            Fix1: table[i][0],
                            Fix2: table[i][1]
                        };
                    }
                }
                return result;
            }

            private ParseFixPoint(variables) {
                var fixPoints = [];
                var that = this;
                variables.forEach((val,ind) => {
                    fixPoints.push({
                        "Id": that.ParseId(val.Id)[0],
                        "Value": val.Value
                    })
                })
                return fixPoints;
            }

            private ParseOscillations(variables) {
                var table = [];
                for (var j = 0; j < variables.length; j++) {
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
        }
    }
}