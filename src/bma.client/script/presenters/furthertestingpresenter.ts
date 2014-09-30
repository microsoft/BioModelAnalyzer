module BMA {
    export module Presenters {
        export class FurtherTestingPresenter {

            private driver: BMA.UIDrivers.IFurtherTesting;
            private popupViewer: BMA.UIDrivers.IPopup;
            private num: number = 0;
            private data;
            private model;
            private result;

            constructor(driver: BMA.UIDrivers.IFurtherTesting, popupViewer: BMA.UIDrivers.IPopup) {
                var that = this;
                this.driver = driver;
                this.popupViewer = popupViewer;

                window.Commands.On("ProofFailed", function (param: { Model; Res }) {
                    that.driver.ShowStartToggler();
                    that.model = param.Model;
                    that.result = param.Res;
                    //that.driver.HideResults();
                    //that.num = variables.length;
                })

                window.Commands.On("ProofRequested", function () {
                    that.driver.HideStartToggler();
                    that.driver.HideResults();
                })

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.result.length !== 0) {
                        that.driver.HideStartToggler();
                        //$.ajax({
                        //    type: "POST",
                        //    url: "api/FurtherTesting",
                        //    data: {
                        //        Model: that.model,
                        //        Analysis: that.result,
                        //    },
                        //    success: function (res2) {
                        //        $("#log").append("FurtherTesting success. " + res2.Status + "<br/>");
                        //    },
                        //    error: function (res2) {
                        //        //$("#log").append("FurtherTesting error: " + res2.statusText + "<br/>");
                        //    }
                        //});
                        //that.driver.ShowResults(data);
                        //that.data = data;
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