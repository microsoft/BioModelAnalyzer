module BMA {
    export module Presenters {
        export class ProofPresenter { 
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.IProofResultViewer;

            constructor(appModel: BMA.Model.AppModel, proofResultViewer: BMA.UIDrivers.IProofResultViewer, popupViewer: BMA.UIDrivers.IPopup) {
                this.appModel = appModel;


                window.Commands.On("ProofRequested", function (args) {
                    proofResultViewer.OnProofStarted();

                    var proofInput = appModel.BioModel.GetJSON()

                    $.ajax({
                        type: "POST",
                        url: "api/Analyze",
                        data: proofInput,
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

                window.Commands.On("Expand", (param) => {
                    proofResultViewer.Hide({ tab: param });
                    popupViewer.Show({ tab: param, type: "coloredTable" });
                });

                window.Commands.On("Collapse", (param) => {
                    proofResultViewer.Show({ tab: param });
                    popupViewer.Hide();
                });
            }
        }
    }
} 