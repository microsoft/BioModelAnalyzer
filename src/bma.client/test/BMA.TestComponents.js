/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\script\uidrivers.interfaces.ts"/>
var BMA;
(function (BMA) {
    (function (Test) {
        var TestSVGPlotDriver = (function () {
            function TestSVGPlotDriver() {
                this.svg = [];
            }
            Object.defineProperty(TestSVGPlotDriver.prototype, "SVGs", {
                get: function () {
                    return this.svg;
                },
                enumerable: true,
                configurable: true
            });

            TestSVGPlotDriver.prototype.Draw = function (svg) {
                this.svg.push(svg);
            };

            TestSVGPlotDriver.prototype.TurnNavigation = function (isOn) {
            };
            return TestSVGPlotDriver;
        })();
        Test.TestSVGPlotDriver = TestSVGPlotDriver;

        var TestUndoRedoButton = (function () {
            function TestUndoRedoButton() {
            }
            TestUndoRedoButton.prototype.Turn = function (isOn) {
            };
            return TestUndoRedoButton;
        })();
        Test.TestUndoRedoButton = TestUndoRedoButton;
    })(BMA.Test || (BMA.Test = {}));
    var Test = BMA.Test;
})(BMA || (BMA = {}));
//# sourceMappingURL=BMA.TestComponents.js.map
