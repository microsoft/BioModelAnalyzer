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

            TestSVGPlotDriver.prototype.SetGrid = function (x0, y0, xStep, yStep) {
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

        var TestElementsPanel = (function () {
            function TestElementsPanel() {
            }
            TestElementsPanel.prototype.GetDragSubject = function () {
                return {
                    dragStart: function () {
                    },
                    drag: function () {
                    },
                    dragEnd: function () {
                    }
                };
            };
            return TestElementsPanel;
        })();
        Test.TestElementsPanel = TestElementsPanel;
    })(BMA.Test || (BMA.Test = {}));
    var Test = BMA.Test;
})(BMA || (BMA = {}));
