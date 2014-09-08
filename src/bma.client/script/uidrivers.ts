/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="widgets\drawingsurface.ts"/>

module BMA {
    export module UIDrivers {
        export class SVGPlotDriver implements ISVGPlot, IElementsPanel, INavigationPanel {
            private svgPlotDiv: JQuery;

            constructor(svgPlotDiv: JQuery) {
                this.svgPlotDiv = svgPlotDiv;
            }

            public Draw(svg: SVGElement) {
                this.svgPlotDiv.drawingsurface({ svg: svg });
            }

            public TurnNavigation(isOn: boolean) {
                this.svgPlotDiv.drawingsurface({ isNavigationEnabled: isOn });
            }

            public SetGrid(x0: number, y0: number, xStep: number, yStep: number) {
                this.svgPlotDiv.drawingsurface({ grid: { x0: x0, y0: y0, xStep: xStep, yStep: yStep } });
            }

            public GetDragSubject() {
                return this.svgPlotDiv.drawingsurface("getDragSubject");
            }

            public SetZoom(zoom: number) {
                this.svgPlotDiv.drawingsurface({ zoom: zoom });
            }
        }

        export class TurnableButtonDriver implements ITurnableButton {
            private button: JQuery;

            constructor(button: JQuery) {
                this.button = button;
            }

            public Turn(isOn: boolean) {
                this.button.button("option", "disabled", !isOn);
            }

        }

        export class VariableEditorDriver implements IVariableEditor {
            private variableEditor: JQuery;

            constructor(variableEditor: JQuery) {
                this.variableEditor = variableEditor;
                this.variableEditor.bmaeditor();
                this.variableEditor.hide();

                this.variableEditor.click(function (e) { e.stopPropagation(); });
            }

            public GetVariableProperties(): { name: string; formula: string; rangeFrom: number; rangeTo: number } {
                return {
                    name: this.variableEditor.bmaeditor('option', 'name'),
                    formula: this.variableEditor.bmaeditor('option', 'formula'),
                    rangeFrom: this.variableEditor.bmaeditor('option', 'rangeFrom'),
                    rangeTo: this.variableEditor.bmaeditor('option', 'rangeTo')
                };
            }

            public Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) {
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
            }

            public Show(x: number, y: number) {
                this.variableEditor.show();
            }

            public Hide() {
                this.variableEditor.hide();
            }
        }

        export class ProofViewer implements IProofResultViewer {
            private proofAccordion: JQuery;
            private proofContentViewer: JQuery;

            constructor(proofAccordion, proofContentViewer) {
                this.proofAccordion = proofAccordion;
                this.proofContentViewer = proofContentViewer;
            }

            public SetData(params) {
                this.proofContentViewer.proofresultviewer({ issucceeded: params.issucceeded, time: params.time, data: params.data});
            }

            public ShowResult(result: BMA.Model.ProofResult) {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } });
            }

            public OnProofStarted() {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
            }

            public OnProofFailed() {
                $("#icon1").click();
            }

            public Show(params: any) {
                this.proofContentViewer.proofresultviewer("show",params.tab);
            }

            public Hide(params) {
                this.proofContentViewer.proofresultviewer("hide", params.tab);
            }

            private DataToCompactMode(data) { }
            private DataToFullMode(data) { }
            
        }

        export class PopupDriver implements IPopup {
            private popupWindow;
            constructor(popupWindow) {
                this.popupWindow = popupWindow;
            }

            public Show(params: any) {
                var that = this;
                //this.createResultView(params);
                this.popupWindow.resultswindowviewer({ header: params.tab, content: params.content, icon: "min" });
                this.popupWindow.show();
            }

            public Hide() {
                this.popupWindow.hide();
            }

            private createResultView(params) {
                if (params.type === "coloredTable") {
                }
            }
        }

        export class ModelFileLoader implements IFileLoader {
            private fileInput: JQuery;
            private currentPromise = undefined;

            constructor(fileInput: JQuery) {
                var that = this;
                this.fileInput = fileInput;

                fileInput.change(function (arg) {
                    var e: any = arg;
                    if (e.target.files !== undefined && e.target.files.length == 1 && that.currentPromise !== undefined) {
                        that.currentPromise.resolve(e.target.files[0]);
                        that.currentPromise = undefined;
                        fileInput.val("");
                    }
                });
            }

            public OpenFileDialog(): JQueryPromise<File> {
                var deferred = $.Deferred();
                this.currentPromise = deferred;
                this.fileInput.click();
                return deferred.promise();
            }

            private OnCheckFileSelected() : boolean {
                return false;
            }
        }
    }
} 