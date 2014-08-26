/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="widgets\drawingsurface.ts"/>

module BMA {
    export module UIDrivers {
        export class SVGPlotDriver implements ISVGPlot, IElementsPanel {
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

            public Initialize(variable: BMA.Model.Variable) {
                this.variableEditor.bmaeditor('option', 'name', variable.Name);
                this.variableEditor.bmaeditor('option', 'formula', variable.Formula);
                this.variableEditor.bmaeditor('option', 'rangeFrom', variable.RangeFrom);
                this.variableEditor.bmaeditor('option', 'rangeTo', variable.RangeTo);
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

                $("#icon1").click(function () {
                    var isHidden = $("#icon1").next().attr("aria-hidden");
                    console.log(isHidden);
                    if (isHidden === "true") {
                        window.Commands.Execute("ProofRequested", undefined);
                    }
                });
            }

            public ShowResult(result: BMA.Model.ProofResult) {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } });
            }

            public OnProofStarted() {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
            }
        }
    }
} 