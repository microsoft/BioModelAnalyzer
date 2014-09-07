var BMA;
(function (BMA) {
    (function (Presenters) {
        var ProofPresenter = (function () {
            function ProofPresenter(appModel, proofResultViewer, popupViewer) {
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
                            var result = appModel.ProofResult = new BMA.Model.ProofResult(res.Status === "Stabilizing", res.Time, res.Ticks);
                            var numericData = that.CreateTableView();
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
                    var full;
                    if (param === "Proof Propagation")
                        full = that.CreateFullResultTable(appModel.ProofResult.Ticks);

                    //if (param === "Variables")
                    //    full = that.
                    if (full !== undefined) {
                        proofResultViewer.Hide({ tab: param });
                        popupViewer.Show({ tab: param, type: "coloredTable", content: full });
                    }
                });

                window.Commands.On("Collapse", function (param) {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }
            ProofPresenter.prototype.CreateTableView = function () {
                var table = [];
                var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = variables[i].Name;
                    table[i][1] = variables[i].Formula;
                    table[i][2] = variables[i].RangeFrom + ' - ' + variables[i].RangeTo;
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
                return container;
            };
            return ProofPresenter;
        })();
        Presenters.ProofPresenter = ProofPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=proofpresenter.js.map
