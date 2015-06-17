module BMA {
    export module Presenters {
        export class LTLPresenter {

            keyframescompact: BMA.UIDrivers.IKeyframesList;
            appModel: BMA.Model.AppModel;
            constructor(
                appModel: BMA.Model.AppModel,
                keyframescompact: BMA.UIDrivers.IKeyframesList,
                ajax: BMA.UIDrivers.IServiceDriver
                ) {

                var that = this;
                window.Commands.On("AddKeyframe", function () {
                    keyframescompact.Add("New");
                });

                window.Commands.On("ChangedKeyframeName", function (item: { ind; name} ) {
                    alert('ind=' + item.ind + ' name=' + item.name);
                });

                window.Commands.On("KeyframeSelected", function (item: { ind }) {
                    alert('selected ind=' + item.ind);
                });
                
                window.Commands.On("LTLRequested", function () {
                    var model = BMA.Model.ExportBioModel(appModel.BioModel);
                    var proofInput = {
                        "Model": model,
                        "Formula": "(True)",
                        "Number_of_steps": 100
                    }

                    var result = ajax.Invoke(proofInput)
                        .done(function (res) {
                            console.log(res.Loop);
                            alert(res.Status);
                        })
                })

            }
        }
    }
} 