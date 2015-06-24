module BMA {
    export module Presenters {
        export class LTLPresenter {

            keyframescompact: BMA.UIDrivers.IKeyframesList;
            appModel: BMA.Model.AppModel;
            currentdraggableelem: any;
            


            constructor(
                appModel: BMA.Model.AppModel,
                keyframesfullDriver: BMA.UIDrivers.IKeyframesFull,
                keyframescompactDriver: BMA.UIDrivers.IKeyframesList,
                ltlviewer: BMA.UIDrivers.ILTLViewer,
                ajax: BMA.UIDrivers.IServiceDriver,
                popupViewer: BMA.UIDrivers.IPopup
                ) {

                var that = this;
                window.Commands.On("AddKeyframe", function () {
                    var newstate = 'new';
                    keyframescompactDriver.AddState(newstate);
                    keyframesfullDriver.AddState(newstate);
                });

                window.Commands.On("ChangedKeyframeName", function (item: { ind; name} ) {
                    //alert('ind=' + item.ind + ' name=' + item.name);
                });

                window.Commands.On("KeyframeSelected", function (item: { ind }) {
                    //alert('selected ind=' + item.ind);
                });
                
                window.Commands.On("LTLRequested", function (param: {formula}) {
                    var model = BMA.Model.ExportBioModel(appModel.BioModel);
                    var proofInput = {
                        "Name": model.Name,
                        "Relationships": model.Relationships,
                        "Variables": appModel.BioModel.Variables,
                        "Formula": param.formula,
                        "Number_of_steps": 10
                    }

                    var result = ajax.Invoke(proofInput)
                        .done(function (res) {
                        if (res.Ticks == null) {
                            alert(res.Error);
                        }
                        else {
                            if (res.Status == "True") {
                                var restbl = that.CreateColoredTable(res.Ticks);
                                ltlviewer.SetResult(restbl);
                            }
                            else {
                                ltlviewer.SetResult(undefined);
                                alert(res.Status);
                            }
                        }
                        })
                        .fail(function () {
                            alert("LTL failed");
                        })
                });

                window.Commands.On("Expand", (param) => {
                    switch (param) {
                        case "LTLStates":
                            var content = keyframesfullDriver.GetContent();
                            popupViewer.Show({ tab: param, content: content });
                            ltlviewer.Hide (param);
                            break;
                        default:
                            ltlviewer.Show(undefined);
                            break;
                    }
                });

                window.Commands.On("Collapse",(param) => {
                    ltlviewer.Show(param);
                    popupViewer.Hide();
                });

                window.Commands.On('KeyframeStartDrag',(param) => {
                    this.currentdraggableelem = param;
                });

                window.Commands.On("KeyframeDropped",(param: { location: JQuery;}) => {
                    var cl = window.KeyframesRegistry.Keyframes[this.currentdraggableelem];
                    var img = $('<img>').attr('src', cl.Icon);
                    img.appendTo(param.location);
                });

                window.Commands.On('RemoveKeyframe', function () {
                    keyframesfullDriver.RemovePart('','');
                });

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