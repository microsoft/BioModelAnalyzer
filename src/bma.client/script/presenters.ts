/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>

interface JQuery {
    svg(): any;
    svg(options: any): any;
}

module BMA {
    export module Presenters {
        export class DesignSurfacePresenter {
            private model: BMA.Model.BioModel;
            private layout: BMA.Model.Layout;

            private models: { model: BMA.Model.BioModel; layout: BMA.Model.Layout }[];
            private currentModelIndex: number;

            private selectedType: string;
            private driver: BMA.UIDrivers.ISVGPlot;
            private svg: any;

            constructor(bioModel: BMA.Model.BioModel,
                layout: BMA.Model.Layout,
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                undoButton: BMA.UIDrivers.ITurnableButton,
                redoButton: BMA.UIDrivers.ITurnableButton) {

                var that = this;
                this.model = bioModel;
                this.layout = layout;
                this.driver = svgPlotDriver;
                this.models = [];

                window.Commands.On("AddElementSelect", (type: string) => {
                    this.selectedType = type;
                    this.driver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", (args: { x: number; y: number }) => {
                    if (that.selectedType !== undefined) {

                        var variables = that.model.Variables;
                        var variableLayouts = that.layout.Variables;

                        variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                        variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));

                        that.model = new BMA.Model.BioModel([], variables, []);
                        that.layout = new BMA.Model.Layout([], variableLayouts);

                        var drawingSvg = <SVGElement>that.CreateSvg();
                        that.driver.Draw(drawingSvg);
                    }
                });

                window.Commands.On("Undo", () => {

                });

                window.Commands.On("Redo", () => {

                });

                var svgCnt = $("<div></div>")
                svgCnt.svg({
                    onLoad: (svg) => {
                        this.svg = svg;
                        var drawingSvg = <SVGElement>this.CreateSvg();
                        this.driver.Draw(drawingSvg);
                    }
                });
            }

            private OnModelUpdated() {
                //todo: update application model
            }

            private Undo() {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    this.OnModelUpdated();
                }
            }

            private Redo() {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.OnModelUpdated();
                }
            }

            private Truncate() {
                this.models.length = this.currentModelIndex + 1;
            }

            private Dup() {
            }

            private get CanUndo(): boolean {
                return this.currentModelIndex > 0;
            }

            private get CanRedo(): boolean {
                return this.currentModelIndex <this.models.length - 1;
            }

            private CreateSvg(): any {
                if (this.svg === undefined)
                    return undefined;

                this.svg.clear();

                var variables = this.model.Variables;
                var variableLayouts = this.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    this.svg.add(element.RenderToSvg(variableLayout.PositionX, variableLayout.PositionY));
                }

                return $(this.svg.toSVG()).children();
            }
        }
    }

    export class ModelStack {
        private static models: { model: BMA.Model.BioModel; layout: BMA.Model.Layout }[] = [];
        private static index: number = -1;

        static get Current() { return ModelStack.models[ModelStack.index]; }
        static get HasModel() { return ModelStack.index >= 0; }

        static get CanUndo() { return ModelStack.index > 0; }
        static get CanRedo() { return ModelStack.index < ModelStack.models.length - 1; }

        static Undo() {
            if (ModelStack.CanUndo) {
                --ModelStack.index;
            }
        }

        static Redo() {
            if (ModelStack.CanRedo) {
                ++ModelStack.index;
            }
        }

        static Set(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
            ModelStack.models = [{ model: m, layout: l }];
            ModelStack.index = 0;
        }

        static Dup() {
            ModelStack.truncate();
            var orig = ModelStack.Current;
            ModelStack.models[ModelStack.index] = { model: orig.model.Clone(), layout: orig.layout.Clone() };
            ModelStack.models.push(orig);
            ++ModelStack.index;
        }

        static truncate() {
            ModelStack.models.length = ModelStack.index + 1;
        }

    }
} 