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
        }

        export class TestUndoRedoButton implements BMA.UIDrivers.ITurnableButton {
            public Turn(isOn: boolean) {
            }
        }
    }
} 