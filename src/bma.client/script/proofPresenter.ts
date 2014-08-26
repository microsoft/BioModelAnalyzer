module BMA {
    export module Presenters {
        export class ProofPresenter { 
            private appModel: BMA.Model.AppModel;
            private viewer: BMA.UIDrivers.IProofResultViewer;

            constructor(appModel: BMA.Model.AppModel, proofResultViewer: BMA.UIDrivers.IProofResultViewer) {
                this.appModel = appModel;

                window.Commands.On("ProofRequested", function (args) {
                    proofResultViewer.OnProofStarted();
                    $.ajax({
                        type: "POST",
                        url: "api/Analyze1",
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
            }
        }
    }
} 