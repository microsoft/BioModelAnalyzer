/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\script\uidrivers.interfaces.ts"/>

module BMA {
    export module Test {
        export class TestSVGPlotDriver implements BMA.UIDrivers.ISVGPlot, BMA.UIDrivers.IElementsPanel, BMA.UIDrivers.INavigationPanel {
            
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

            public GetPlotX(left: number) {
                return this.svgPlotDiv.drawingsurface("getPlotX", left);
            }

            public GetPlotY(top: number) {
                return this.svgPlotDiv.drawingsurface("getPlotY", top);
            }

            public GetPixelWidth() {
                return this.svgPlotDiv.drawingsurface("getPixelWidth");
            }

            public SetGridVisibility(isOn: boolean) {
                this.svgPlotDiv.drawingsurface({ gridVisibility: isOn });
            }

            public HighlightAreas(areas: { x: number; y: number; width: number; height: number; fill: string }[]) {
                this.svgPlotDiv.drawingsurface({ rects: areas });
            }

            public SetCenter(x: number, y: number) {

            }
            
            //private svg: SVGElement[];

            //public get SVGs() {
            //    return this.svg; 
            //}

            //constructor() {
            //    this.svg = [];
            //}

            //public SetZoom(zoom: number){}

            //public GetDragSubject() {
            //    return {
            //        dragStart: null,//createDragStartSubject(that._plot.centralPart),
            //        drag: null, //createPanSubject(that._plot.centralPart),
            //        dragEnd: null, //createDragEndSubject(that._plot.centralPart)
            //    };
            //}

            //public Draw(svg: SVGSVGElement) {
            //    this.svg.push(svg);
            //}

            //public TurnNavigation(isOn: boolean) {
            //}

            //public SetGrid(x0: number, y0: number, xStep: number, yStep: number) {
            //}

            //public GetPlotX(left) {
            //    return 0;
            //}

            //public GetPlotY(left) {
            //    return 0;
            //}

            //public GetPixelWidth() {
            //    return 0;
            //}

            //public SetGridVisibility(isOn: boolean) {

            //}
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

            SetValidation(v: boolean) {
                return v;
            }

            Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) { }

            Show(x: number, y: number) { }

            Hide() { }
        }

    }
} 