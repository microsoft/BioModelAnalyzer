/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model\biomodel.ts"/>
/// <reference path="model\model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>

interface JQuery {
    svg(): any;
    svg(options: any): any;
}

module BMA {
    export module Presenters {
        export class DesignSurfacePresenter {
            private appModel: BMA.Model.AppModel;

            private models: { model: BMA.Model.BioModel; layout: BMA.Model.Layout }[];
            private currentModelIndex: number = -1;

            private selectedType: string;
            private driver: BMA.UIDrivers.ISVGPlot;
            private svg: any;

            private undoButton: BMA.UIDrivers.ITurnableButton;
            private redoButton: BMA.UIDrivers.ITurnableButton;

            constructor(appModel: BMA.Model.AppModel,
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                undoButton: BMA.UIDrivers.ITurnableButton,
                redoButton: BMA.UIDrivers.ITurnableButton) {

                var that = this;
                this.appModel = appModel;
                this.undoButton = undoButton;
                this.redoButton = redoButton;

                this.driver = svgPlotDriver;
                this.models = [];


                window.Commands.On("AddElementSelect", (type: string) => {
                    this.selectedType = type;
                    this.driver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", (args: { x: number; y: number }) => {
                    if (that.selectedType !== undefined) {

                        var current = that.Current;
                        var model = current.model;
                        var layout = current.layout;

                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                        variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));

                        var newmodel = new BMA.Model.BioModel([], variables, []);
                        var newlayout = new BMA.Model.Layout([], variableLayouts);

                        that.Dup(newmodel, newlayout);
                    }
                });

                window.Commands.On("Undo", () => {
                    this.Undo();
                });

                window.Commands.On("Redo", () => {
                    this.Redo();
                });

                var svgCnt = $("<div></div>")
                svgCnt.svg({
                    onLoad: (svg) => {
                        this.svg = svg;
                        var drawingSvg = <SVGElement>this.CreateSvg();
                        this.driver.Draw(drawingSvg);
                    }
                });

                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }

            private OnModelUpdated() {
                this.undoButton.Turn(this.CanUndo);
                this.redoButton.Turn(this.CanRedo);

                this.appModel.BioModel = this.Current.model;
                this.appModel.Layout = this.Current.layout;

                var drawingSvg = <SVGElement>this.CreateSvg();
                this.driver.Draw(drawingSvg);
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

            private Dup(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
                this.Truncate();
                var current = this.Current;
                this.models[this.currentModelIndex] = { model: current.model.Clone(), layout: current.layout.Clone() };
                this.models.push({ model: m, layout: l });
                ++this.currentModelIndex;
                this.OnModelUpdated();
            }

            private get CanUndo(): boolean {
                return this.currentModelIndex > 0;
            }

            private get CanRedo(): boolean {
                return this.currentModelIndex < this.models.length - 1;
            }

            private Set(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
                this.models = [{ model: m, layout: l }];
                this.currentModelIndex = 0;
                this.OnModelUpdated();
            }

            private get Current(): { model: BMA.Model.BioModel; layout: BMA.Model.Layout } {
                return this.models[this.currentModelIndex];
            }

            private CreateSvg(): any {
                if (this.svg === undefined)
                    return undefined;

                this.svg.clear();

                var variables = this.appModel.BioModel.Variables;
                var variableLayouts = this.appModel.Layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    this.svg.add(element.RenderToSvg({ x: variableLayout.PositionX, y: variableLayout.PositionY, angle: variableLayout.Angle }));
                }

                return $(this.svg.toSVG()).children();
            }
        }
    }
} 