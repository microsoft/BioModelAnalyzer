var BMA;
(function (BMA) {
    (function (Presenters) {
        var ProofPresenter = (function () {
            function ProofPresenter(appModel, proofResultViewer, popupViewer) {
                var _this = this;
                this.appModel = appModel;
                var that = this;

                window.Commands.On("ProofRequested", function (args) {
                    proofResultViewer.OnProofStarted();

                    var proofInput = appModel.BioModel.GetJSON();

                    $.ajax({
                        type: "POST",
                        url: "api/Analyze",
                        data: proofInput,
                        success: function (res) {
                            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === 4, res.Time, res.Ticks);
                            var numericData = that.CreateTableView(res.Ticks);
                            var colorData = that.CreateColoredTable(res.Ticks);

                            //var result = appModel.ProofResult;
                            //var data = { numericData: numericData, colorData: undefined };
                            proofResultViewer.SetData({ issucceeded: result.IsStable, time: result.Time, data: { numericData: numericData, colorData: colorData } });
                            proofResultViewer.ShowResult(appModel.ProofResult);
                        },
                        error: function (res) {
                            alert("Error: " + res.statusText);
                            proofResultViewer.OnProofFailed();
                        }
                    });
                });

                window.Commands.On("Expand", function (param) {
                    if (_this.appModel.BioModel.Variables.length !== 0) {
                        var full;
                        if (param === "ProofPropagation" && _this.appModel.ProofResult.Ticks !== null)
                            full = that.CreateFullResultTable(appModel.ProofResult.Ticks);
                        if (param === "ProofVariables") {
                            full = $('<div></div>').coloredtableviewer({ numericData: that.CreateTableView(appModel.ProofResult.Ticks), header: ["Name", "Formula", "Range"] });
                            full.find("td").eq(0).width(150);
                            full.find("td").eq(2).width(150);
                        }
                        if (full !== undefined) {
                            proofResultViewer.Hide({ tab: param });
                            popupViewer.Show({ tab: param, content: full });
                        }
                    }
                });

                window.Commands.On("Collapse", function (param) {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }
            ProofPresenter.prototype.CreateTableView = function (ticks) {
                var table = [];
                if (ticks === null)
                    return;
                var variables = this.appModel.BioModel.Variables;
                var color = [];
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    color[i] = [];
                    table[i][0] = variables[i].Name;
                    table[i][1] = variables[i].Formula;
                    var range;
                    var ij = ticks[ticks.length - 1].Variables[i];
                    var c = ij.Lo === ij.Hi;
                    if (c)
                        range = ij.Lo;
                    else
                        range = ij.Lo + ' - ' + ij.Hi;

                    table[i][2] = range;
                    for (var j = 0; j < 3; j++)
                        color[i][j] = c;
                }
                return table;
            };

            ProofPresenter.prototype.CreateColoredTable = function (ticks) {
                var that = this;
                if (ticks === null)
                    return;
                var color = [];
                var t = ticks.length;
                var v = ticks[0].Variables.length;
                for (var i = 0; i < v; i++) {
                    color[i] = [];
                    for (var j = 0; j < t; j++) {
                        var ij = ticks[j].Variables[v - 1 - i];
                        color[i][j] = ij.Hi === ij.Lo;
                    }
                }
                return color;
            };

            ProofPresenter.prototype.CreateFullResultTable = function (ticks) {
                var container = $('<div></div>');
                if (ticks === null)
                    return container;
                var that = this;
                var biomodel = this.appModel.BioModel;
                var variables = biomodel.Variables;
                var table = [];
                var color = [];
                var header = [];
                var l = ticks.length;
                header[0] = "Name";
                for (var i = 0; i < ticks.length; i++) {
                    header[i + 1] = "T = " + i;
                }
                for (var j = 0; j < variables.length; j++) {
                    table[j] = [];
                    color[j] = [];
                    table[j][0] = biomodel.GetVariableById(ticks[0].Variables[variables.length - 1 - j].Id).Name;
                    for (var i = 0; i < l; i++) {
                        var ij = ticks[i].Variables[variables.length - 1 - j];
                        if (ij.Lo === ij.Hi) {
                            table[j][i + 1] = ij.Lo;
                            color[j][i + 1] = true;
                        } else {
                            table[j][i + 1] = ij.Lo + ' - ' + ij.Hi;
                            color[j][i + 1] = false;
                        }
                    }
                }

                container.coloredtableviewer({ header: header, numericData: table, colorData: color });

                container.find("td").eq(0).width(150);
                return container;
            };
            return ProofPresenter;
        })();
        Presenters.ProofPresenter = ProofPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=proofpresenter.js.map
