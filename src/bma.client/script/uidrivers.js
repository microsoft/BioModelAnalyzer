/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="widgets\drawingsurface.ts"/>
var BMA;
(function (BMA) {
    (function (UIDrivers) {
        var SVGPlotDriver = (function () {
            function SVGPlotDriver(svgPlotDiv) {
                this.svgPlotDiv = svgPlotDiv;
            }
            SVGPlotDriver.prototype.Draw = function (svg) {
                this.svgPlotDiv.drawingsurface({ svg: svg });
            };

            SVGPlotDriver.prototype.TurnNavigation = function (isOn) {
                this.svgPlotDiv.drawingsurface({ isNavigationEnabled: isOn });
            };

            SVGPlotDriver.prototype.SetGrid = function (x0, y0, xStep, yStep) {
                this.svgPlotDiv.drawingsurface({ grid: { x0: x0, y0: y0, xStep: xStep, yStep: yStep } });
            };

            SVGPlotDriver.prototype.GetDragSubject = function () {
                return this.svgPlotDiv.drawingsurface("getDragSubject");
            };
            return SVGPlotDriver;
        })();
        UIDrivers.SVGPlotDriver = SVGPlotDriver;

        var TurnableButtonDriver = (function () {
            function TurnableButtonDriver(button) {
                this.button = button;
            }
            TurnableButtonDriver.prototype.Turn = function (isOn) {
                this.button.button("option", "disabled", !isOn);
            };
            return TurnableButtonDriver;
        })();
        UIDrivers.TurnableButtonDriver = TurnableButtonDriver;

        var VariableEditorDriver = (function () {
            function VariableEditorDriver(variableEditor) {
                this.variableEditor = variableEditor;
                this.variableEditor.bmaeditor();
                this.variableEditor.hide();

                this.variableEditor.click(function (e) {
                    e.stopPropagation();
                });
            }
            VariableEditorDriver.prototype.GetVariableProperties = function () {
                return {
                    name: this.variableEditor.bmaeditor('option', 'name'),
                    formula: this.variableEditor.bmaeditor('option', 'formula'),
                    rangeFrom: this.variableEditor.bmaeditor('option', 'rangeFrom'),
                    rangeTo: this.variableEditor.bmaeditor('option', 'rangeTo')
                };
            };

            VariableEditorDriver.prototype.Initialize = function (variable, model) {
                this.variableEditor.bmaeditor('option', 'name', variable.Name);
                this.variableEditor.bmaeditor('option', 'formula', variable.Formula);
                this.variableEditor.bmaeditor('option', 'rangeFrom', variable.RangeFrom);
                this.variableEditor.bmaeditor('option', 'rangeTo', variable.RangeTo);

                var options = [];
                var id = variable.Id;
                for (var i = 0; i < model.Relationships.length; i++) {
                    var rel = model.Relationships[i];
                    if (rel.ToVariableId === id) {
                        options.push(model.GetVariableById(rel.FromVariableId).Name);
                    }
                }
                this.variableEditor.bmaeditor('option', 'inputs', options);
            };

            VariableEditorDriver.prototype.Show = function (x, y) {
                this.variableEditor.show();
            };

            VariableEditorDriver.prototype.Hide = function () {
                this.variableEditor.hide();
            };
            return VariableEditorDriver;
        })();
        UIDrivers.VariableEditorDriver = VariableEditorDriver;

        var ProofViewer = (function () {
            function ProofViewer(proofAccordion, proofContentViewer) {
                this.proofAccordion = proofAccordion;
                this.proofContentViewer = proofContentViewer;
            }
            ProofViewer.prototype.SetData = function (params) {
                this.proofContentViewer.proofresultviewer({ issucceeded: params.issucceeded, time: params.time, data: params.data });
            };

            ProofViewer.prototype.ShowResult = function (result) {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } });
            };

            ProofViewer.prototype.OnProofStarted = function () {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
            };

            ProofViewer.prototype.OnProofFailed = function () {
                $("#icon1").click();
            };

            ProofViewer.prototype.Show = function (params) {
                this.proofContentViewer.proofresultviewer("show", params.tab);
            };

            ProofViewer.prototype.Hide = function (params) {
                this.proofContentViewer.proofresultviewer("hide", params.tab);
            };

            ProofViewer.prototype.DataToCompactMode = function (data) {
            };
            ProofViewer.prototype.DataToFullMode = function (data) {
            };
            return ProofViewer;
        })();
        UIDrivers.ProofViewer = ProofViewer;

        var PopupDriver = (function () {
            function PopupDriver(popupWindow) {
                this.popupWindow = popupWindow;
            }
            PopupDriver.prototype.Show = function (params) {
                var that = this;

                //this.createResultView(params);
                this.popupWindow.resultswindowviewer({ header: params.tab, content: params.content });
                this.popupWindow.show();
            };

            PopupDriver.prototype.Hide = function () {
                this.popupWindow.hide();
            };

            PopupDriver.prototype.createResultView = function (params) {
                if (params.type === "coloredTable") {
                }
            };
            return PopupDriver;
        })();
        UIDrivers.PopupDriver = PopupDriver;

        var ModelFileLoader = (function () {
            function ModelFileLoader(fileInput) {
                this.currentPromise = undefined;
                var that = this;
                this.fileInput = fileInput;

                fileInput.change(function (arg) {
                    var e = arg;
                    if (e.target.files !== undefined && e.target.files.length == 1 && that.currentPromise !== undefined) {
                        that.currentPromise.resolve(e.target.files[0]);
                        that.currentPromise = undefined;
                        fileInput.val("");
                    }
                });
            }
            ModelFileLoader.prototype.OpenFileDialog = function () {
                var deferred = $.Deferred();
                this.currentPromise = deferred;
                this.fileInput.click();
                return deferred.promise();
            };

            ModelFileLoader.prototype.OnCheckFileSelected = function () {
                return false;
            };
            return ModelFileLoader;
        })();
        UIDrivers.ModelFileLoader = ModelFileLoader;
    })(BMA.UIDrivers || (BMA.UIDrivers = {}));
    var UIDrivers = BMA.UIDrivers;
})(BMA || (BMA = {}));
//# sourceMappingURL=uidrivers.js.map
