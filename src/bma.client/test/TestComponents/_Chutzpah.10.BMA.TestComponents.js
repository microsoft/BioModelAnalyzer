/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\script\uidrivers.interfaces.ts"/>
var BMA;
(function (BMA) {
    (function (Test) {
        var TestSVGPlotDriver = (function () {
            function TestSVGPlotDriver(svgPlotDiv) {
                this.svgPlotDiv = svgPlotDiv;
            }
            TestSVGPlotDriver.prototype.Draw = function (svg) {
                this.svgPlotDiv.drawingsurface({ svg: svg });
            };

            TestSVGPlotDriver.prototype.DrawLayer2 = function (svg) {
                this.svgPlotDiv.drawingsurface({ lightSvg: svg });
            };

            TestSVGPlotDriver.prototype.TurnNavigation = function (isOn) {
                this.svgPlotDiv.drawingsurface({ isNavigationEnabled: isOn });
            };

            TestSVGPlotDriver.prototype.SetGrid = function (x0, y0, xStep, yStep) {
                this.svgPlotDiv.drawingsurface({ grid: { x0: x0, y0: y0, xStep: xStep, yStep: yStep } });
            };

            TestSVGPlotDriver.prototype.GetDragSubject = function () {
                return this.svgPlotDiv.drawingsurface("getDragSubject");
            };

            TestSVGPlotDriver.prototype.SetZoom = function (zoom) {
                this.svgPlotDiv.drawingsurface({ zoom: zoom });
            };

            TestSVGPlotDriver.prototype.GetPlotX = function (left) {
                return this.svgPlotDiv.drawingsurface("getPlotX", left);
            };

            TestSVGPlotDriver.prototype.GetPlotY = function (top) {
                return this.svgPlotDiv.drawingsurface("getPlotY", top);
            };

            TestSVGPlotDriver.prototype.GetPixelWidth = function () {
                return this.svgPlotDiv.drawingsurface("getPixelWidth");
            };

            TestSVGPlotDriver.prototype.SetGridVisibility = function (isOn) {
                this.svgPlotDiv.drawingsurface({ gridVisibility: isOn });
            };

            TestSVGPlotDriver.prototype.HighlightAreas = function (areas) {
                this.svgPlotDiv.drawingsurface({ rects: areas });
            };

            TestSVGPlotDriver.prototype.SetCenter = function (x, y) {
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

            TestVariableEditor.prototype.SetValidation = function (v) {
                return v;
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
