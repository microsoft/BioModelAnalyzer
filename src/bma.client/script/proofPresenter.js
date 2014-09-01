var BMA;
(function (BMA) {
    (function (Presenters) {
        var ProofPresenter = (function () {
            function ProofPresenter(appModel, proofResultViewer, popupViewer) {
                this.appModel = appModel;

                window.Commands.On("ProofRequested", function (args) {
                    proofResultViewer.OnProofStarted();
                    $.ajax({
                        type: "POST",
                        url: "api/Analyze",
                        data: appModel.BioModel.GetJSON(),
                        success: function (res) {
                            appModel.ProofResult = new BMA.Model.ProofResult(res.Status === "Stabilizing", 0);
                            proofResultViewer.ShowResult(appModel.ProofResult);
                        },
                        error: function (res) {
                            alert("Error: " + res.statusText);
                            proofResultViewer.OnProofFailed();
                        }
                    });
                });

                window.Commands.On("Expand", function (param) {
                    proofResultViewer.Hide({ tab: param });
                    popupViewer.Show({ tab: param, type: "coloredTable" });
                });

                window.Commands.On("Collapse", function (param) {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }
            return ProofPresenter;
        })();
        Presenters.ProofPresenter = ProofPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=proofpresenter.js.map
