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

            TestSVGPlotDriver.prototype.SetZoom = function (zoom) {
            };

            TestSVGPlotDriver.prototype.GetDragSubject = function () {
            };

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
                    dragStart: { subscribe: function () {
                        } },
                    drag: { subscribe: function () {
                        } },
                    dragEnd: { subscribe: function () {
                        } }
                };
            };
            return TestElementsPanel;
        })();
        Test.TestElementsPanel = TestElementsPanel;

        var TestVariableEditor = (function () {
            function TestVariableEditor() {
            }
            TestVariableEditor.prototype.GetVariableProperties = function () {
                return { name: "testname", formula: "testformula", rangeFrom: 0, rangeTo: 100 };
            };

            TestVariableEditor.prototype.Initialize = function (variable, model) {
            };

            TestVariableEditor.prototype.Show = function (x, y) {
            };

            TestVariableEditor.prototype.Hide = function () {
            };
            return TestVariableEditor;
        })();
        Test.TestVariableEditor = TestVariableEditor;
    })(BMA.Test || (BMA.Test = {}));
    var Test = BMA.Test;
})(BMA || (BMA = {}));
//# sourceMappingURL=BMA.TestComponents.js.map
