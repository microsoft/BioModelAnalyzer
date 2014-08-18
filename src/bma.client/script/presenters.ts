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

            private xOrigin = 0;
            private yOrigin = 0;
            private xStep = 250;
            private yStep = 280;

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

                svgPlotDriver.SetGrid(this.xOrigin, this.yOrigin, this.xStep, this.yStep);

                window.Commands.On("AddElementSelect", (type: string) => {

                    this.selectedType = type;
                    this.driver.TurnNavigation(type === undefined);
                    //this.selectedType = this.selectedType === type ? undefined : type;
                    //this.driver.TurnNavigation(this.selectedType === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", (args: { x: number; y: number }) => {
                    if (that.selectedType !== undefined) {

                        var current = that.Current;
                        var model = current.model;
                        var layout = current.layout;


                        switch (that.selectedType) {
                            case "Container":
                                var containers = model.Containers.slice(0);
                                var containerLayouts = layout.Containers.slice(0);

                                var gridCell = that.GetGridCell(args.x, args.y);

                                containers.push(new BMA.Model.Container(0));
                                containerLayouts.push(new BMA.Model.ContainerLayout(0, 1, (gridCell.x + 0.5) * that.xStep, (gridCell.y + 0.5) * that.yStep));
                                var newmodel = new BMA.Model.BioModel(containers, model.Variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(containerLayouts, layout.Variables);
                                that.Dup(newmodel, newlayout);
                                break;
                            case "Constant":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);
                                variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);
                                break;
                            case "Default":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);
                                variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);
                                break;
                            case "MembraneReceptor":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);
                                variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);
                                break;

                        }
                    }
                });

                window.Commands.On("Undo", () => {
                    this.Undo();
                });

                window.Commands.On("Redo", () => {
                    this.Redo();
                });

                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: (svg) => {
                        this.svg = svg;

                        if (this.Current !== undefined) {
                            var drawingSvg = <SVGElement>this.CreateSvg();
                            this.driver.Draw(drawingSvg);
                        }
                    }
                });

                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }

            private GetGridCell(x: number, y: number): { x: number; y: number } {
                var cellX = Math.ceil((x - this.xOrigin) / this.xStep) - 1;
                var cellY = Math.ceil((y - this.yOrigin) / this.yStep) - 1;

                return { x: cellX, y: cellY };
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

                //Generating svg elements from model and layout
                this.svg.clear();
                var svgElements = [];

                var containers = this.Current.model.Containers;
                var containerLayouts = this.Current.layout.Containers;
                for (var i = 0; i < containers.length; i++) {
                    var container = containers[i];
                    var containerLayout = containerLayouts[i];

                    var element = window.ElementRegistry.GetElementByType("Container");
                    this.svg.clear();
                    svgElements.push(element.RenderToSvg(this.svg, containerLayout));
                }

                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    this.svg.clear();
                    svgElements.push(element.RenderToSvg(this.svg, variableLayout));
                }

                //constructing final svg image
                this.svg.clear();
                for (var i = 0; i < svgElements.length; i++) {
                    this.svg.add(svgElements[i]);
                }
                return $(this.svg.toSVG()).children();
            }
        }
    }
} 