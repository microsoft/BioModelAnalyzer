module BMA {
    export module Presenters {
        export class FurtherTestingPresenter {

            private driver: BMA.UIDrivers.IFurtherTesting;
            private popupViewer: BMA.UIDrivers.IPopup;
            private num: number = 0;
            private data;

            constructor(driver: BMA.UIDrivers.IFurtherTesting, popupViewer: BMA.UIDrivers.IPopup) {
                var that = this;
                this.driver = driver;
                this.popupViewer = popupViewer;

                window.Commands.On("ProofFailed", function (variables) {
                    that.driver.ShowStartToggler();
                    //that.driver.HideResults();
                    that.num = variables.length;
                })

                window.Commands.On("ProofRequested", function () {
                    that.driver.HideStartToggler();
                    that.driver.HideResults();
                })

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.num !== 0) {
                        that.driver.HideStartToggler();
                        var data = that.FurtherTestingImitation(that.num);
                        that.driver.ShowResults(data);
                        that.data = data;
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