module BMA {
    export module Presenters {
        export class LTLPresenter {

            keyframescompact: BMA.UIDrivers.IKeyframesList;
            appModel: BMA.Model.AppModel;
            constructor(
                appModel: BMA.Model.AppModel,
                keyframescompactDriver: BMA.UIDrivers.IKeyframesList,
                resultsDriver: BMA.UIDrivers.ILTLResultsViewer,
                ajax: BMA.UIDrivers.IServiceDriver
                ) {

                var that = this;
                window.Commands.On("AddKeyframe", function () {
                    keyframescompactDriver.Add("New");
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
                        "Name": model.Name,
                        "Relationships": model.Relationships,
                        "Variables": model.Variables,
                        "Formula": "(True)",
                        "Number_of_steps": 10
                    }

                    var result = ajax.Invoke(proofInput)
                        .done(function (res) {
                            alert(res.Loop);
                            var restbl = that.CreateColoredTable(res.Ticks);
                            resultsDriver.Set(restbl);
                        })
                        .fail(function () {
                            alert("LTL failed");
                        })
                })

            }

            public CreateColoredTable(ticks): any {
                var that = this;
                if (ticks === null) return undefined;
                var color = [];
                var t = ticks.length;
                var v = ticks[0].Variables.length;
                for (var i = 0; i < v; i++) {
                    color[i] = [];
                    for (var j = 1; j < t; j++) {
                        var ij = ticks[j].Variables[i];
                        var pr = ticks[j-1].Variables[i];
                        color[i][j] = pr.Hi === ij.Hi;
                    }
                }
                return color;
            }
        }
    }
} 