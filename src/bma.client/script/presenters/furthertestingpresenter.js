var BMA;
(function (BMA) {
    (function (Presenters) {
        var FurtherTestingPresenter = (function () {
            function FurtherTestingPresenter(driver, popupViewer) {
                var _this = this;
                this.num = 0;
                var that = this;
                this.driver = driver;
                this.popupViewer = popupViewer;

                window.Commands.On("ProofFailed", function (param) {
                    that.model = param.Model;
                    that.result = param.Res;
                });

                window.Commands.On("ProofRequested", function () {
                    that.driver.HideStartToggler();
                    that.driver.HideResults();
                });

                window.Commands.On("FurtherTestingRequested", function () {
                    if (that.result.length !== 0) {
                        that.driver.HideStartToggler();
                    } else
                        alert("No Variables");
                });

                window.Commands.On("Expand", function (param) {
                    switch (param) {
                        case "FurtherTesting":
                            that.driver.HideStartToggler();
                            that.driver.HideResults();
                            var content = $('<div></div>').coloredtableviewer({ numericData: that.data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                            _this.popupViewer.Show({ tab: param, content: content });
                            break;
                    }
                });

                window.Commands.On("Collapse", function (param) {
                    switch (param) {
                        case "FurtherTesting":
                            that.driver.ShowResults(that.data);
                            _this.popupViewer.Hide();
                            break;
                    }
                });
            }
            FurtherTestingPresenter.prototype.FurtherTestingImitation = function (num) {
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
            };
            return FurtherTestingPresenter;
        })();
        Presenters.FurtherTestingPresenter = FurtherTestingPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
