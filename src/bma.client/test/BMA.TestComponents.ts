/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\script\uidrivers.interfaces.ts"/>

module BMA {
    export module Test {
        export class TestSVGPlotDriver implements BMA.UIDrivers.ISVGPlot {
            private svg: SVGElement[];

            public get SVGs() {
                return this.svg; 
            }

            constructor() {
                this.svg = [];
            }

            public Draw(svg: SVGSVGElement) {
                this.svg.push(svg);
            }

            public TurnNavigation(isOn: boolean) {
            }

            public SetGrid(x0: number, y0: number, xStep: number, yStep: number) {
            }
        }

        export class TestUndoRedoButton implements BMA.UIDrivers.ITurnableButton {
            public Turn(isOn: boolean) {
            }
        }

        export class TestElementsPanel implements BMA.UIDrivers.IElementsPanel {
            GetDragSubject() { return {
                dragStart: { subscribe: function () { }},
                drag: { subscribe: function () { } },
                dragEnd: { subscribe: function () { } }
            } }
        }

        export class TestVariableEditor implements BMA.UIDrivers.IVariableEditor {

            GetVariableProperties(): { name: string; formula: string; rangeFrom: number; rangeTo: number; } {
                return { name: "testname", formula: "testformula", rangeFrom: 0, rangeTo: 100 }
            }

            Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) { }

            Show(x: number, y: number) { }

            Hide() { }
        }

    }
} 