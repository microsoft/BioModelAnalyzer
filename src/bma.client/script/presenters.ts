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

            private variableIndex = 0;

            private stagingLine = undefined;

            constructor(appModel: BMA.Model.AppModel,
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                dragService: BMA.UIDrivers.IElementsPanel,
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
                    this.stagingLine = undefined;
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

                                if (that.GetContainerFromGridCell(gridCell) === undefined && that.GetConstantsFromGridCell(gridCell).length === 0) {

                                    containers.push(new BMA.Model.Container(0));
                                    containerLayouts.push(new BMA.Model.ContainerLayout(0, 1, gridCell.x, gridCell.y));
                                    var newmodel = new BMA.Model.BioModel(containers, model.Variables, model.Relationships);
                                    var newlayout = new BMA.Model.Layout(containerLayouts, layout.Variables);
                                    that.Dup(newmodel, newlayout);

                                }

                                break;
                            case "Constant":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);

                                var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("Constant")).GetBoundingBox(args.x, args.y);
                                var gridCell = that.GetGridCell(args.x, args.y);

                                if (that.GetContainerFromGridCell(gridCell) !== undefined || !this.Contains(gridCell, bbox)) {
                                    return;
                                }

                                for (var i = 0; i < variableLayouts.length; i++) {
                                    var variable = variables[i];
                                    var variableLayout = variableLayouts[i];
                                    var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                                    if (this.Intersects(bbox, elementBBox))
                                        return;
                                }

                                variables.push(new BMA.Model.Variable(this.variableIndex, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);

                                break;
                            case "Default":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);

                                var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("Constant")).GetBoundingBox(args.x, args.y);
                                var gridCell = that.GetGridCell(args.x, args.y);
                                var container = that.GetContainerFromGridCell(gridCell);

                                if (container === undefined ||
                                    !(<BMA.Elements.BorderContainerElement>window.ElementRegistry.GetElementByType("Container"))
                                        .ContainsBBox(bbox, (container.layout.PositionX + 0.5) * this.xStep, (container.layout.PositionY + 0.5) * this.yStep)) {
                                    return;
                                }

                                for (var i = 0; i < variableLayouts.length; i++) {
                                    var variable = variables[i];
                                    var variableLayout = variableLayouts[i];
                                    var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                                    if (this.Intersects(bbox, elementBBox))
                                        return;
                                }

                                variables.push(new BMA.Model.Variable(this.variableIndex, container.container.Id, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);
                                break;
                            case "MembraneReceptor":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);

                                var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("Constant")).GetBoundingBox(args.x, args.y);
                                var gridCell = that.GetGridCell(args.x, args.y);
                                var container = that.GetContainerFromGridCell(gridCell);

                                if (container === undefined ||
                                    !(<BMA.Elements.BorderContainerElement>window.ElementRegistry.GetElementByType("Container"))
                                        .IntersectsBorder(args.x, args.y, (container.layout.PositionX + 0.5) * this.xStep, (container.layout.PositionY + 0.5) * this.yStep)) {
                                    return;
                                }

                                for (var i = 0; i < variableLayouts.length; i++) {
                                    var variable = variables[i];
                                    var variableLayout = variableLayouts[i];
                                    var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                                    if (this.Intersects(bbox, elementBBox))
                                        return;
                                }

                                var v = {
                                    x: (args.x - ((gridCell.x + 0.5) * this.xStep + this.xOrigin)),
                                    y: -(args.y - ((gridCell.y + 0.5) * this.yStep + this.yOrigin))
                                };
                                var len = Math.sqrt(v.x * v.x + v.y * v.y);
                                var angle = v.x / Math.abs(v.x) * Math.acos(v.y / len) / Math.PI * 180;

                                variables.push(new BMA.Model.Variable(this.variableIndex, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, args.x, args.y, 0, 0, angle));
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

                var dragSubject = dragService.GetDragSubject()

                dragSubject.dragStart.subscribe(
                    (gesture) => {
                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor")) {
                            var id = this.GetVariableAtPosition(gesture.x, gesture.y);
                            if (id !== undefined) {
                                this.stagingLine = {};
                                this.stagingLine.id = id;
                                this.stagingLine.x0 = gesture.x;
                                this.stagingLine.y0 = gesture.y;
                                return;
                            }
                        }
                        this.stagingLine = undefined;
                    });


                dragSubject.drag.subscribe(
                    (gesture) => {
                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && that.stagingLine !== undefined) {
                            that.stagingLine.x1 = gesture.x1;
                            that.stagingLine.y1 = gesture.y1;

                            //Redraw only svg for better performance
                            if (that.svg !== undefined) {

                                if (that.stagingLine.svg !== undefined) {
                                    that.svg.remove(that.stagingLine.svg);
                                }

                                that.stagingLine.svg = that.svg.line(
                                    that.stagingLine.x0,
                                    that.stagingLine.y0,
                                    that.stagingLine.x1,
                                    that.stagingLine.y1,
                                    {
                                        stroke: "#808080",
                                        strokeWidth: 2,
                                        fill: "#808080",
                                        "marker-end": "url(#" + that.selectedType + ")",
                                        id: "stagingLine"
                                    });

                                that.driver.Draw(<SVGElement>that.GetCurrentSVG(that.svg));
                            }

                            return;
                        }

                        //this.stagingLine = undefined;
                    });

                dragSubject.dragEnd.subscribe(
                    (gesture) => {
                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && this.stagingLine !== undefined) {
                            this.TryAddStagingLineAsLink();
                            this.stagingLine = undefined;
                            this.OnModelUpdated();
                        }
                    });


                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }

            private GetCurrentSVG(svg): any {
                return $(svg.toSVG()).children();
            }

            private GetVariableAtPosition(x: number, y: number): number {
                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    if (element.Contains(x, y, variableLayout.PositionX, variableLayout.PositionY)) {
                        return variable.Id;
                    }
                }

                return undefined;
            }

            private Intersects(
                a: { x: number; y: number; width: number; height: number },
                b: { x: number; y: number; width: number; height: number }): boolean {

                return (Math.abs(a.x - b.x) * 2 <= (a.width + b.width)) && (Math.abs(a.y - b.y) * 2 <= (a.height + b.height));
            }

            private Contains(
                gridCell: { x: number; y: number },
                bbox: { x: number; y: number; width: number; height: number }) {

                return bbox.width < this.xStep && bbox.height < this.yStep &&
                    bbox.x > gridCell.x * this.xStep + this.xOrigin &&
                    bbox.x + bbox.width < (gridCell.x + 1) * this.xStep + this.xOrigin &&
                    bbox.y > gridCell.y * this.yStep + this.yOrigin &&
                    bbox.y + bbox.height < (gridCell.y + 1) * this.yStep + this.yOrigin;
            }

            private TryAddStagingLineAsLink() {
                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    if (element.Contains(this.stagingLine.x1, this.stagingLine.y1, variableLayout.PositionX, variableLayout.PositionY)) {

                        var current = this.Current;
                        var model = current.model;
                        var layout = current.layout;
                        var relationships = model.Relationships.slice(0);
                        relationships.push(new BMA.Model.Relationship(this.stagingLine.id, variable.Id, this.selectedType));
                        var newmodel = new BMA.Model.BioModel(model.Containers, model.Variables, relationships);
                        this.Dup(newmodel, layout);

                        return;
                    }
                }
            }

            private GetGridCell(x: number, y: number): { x: number; y: number } {
                var cellX = Math.ceil((x - this.xOrigin) / this.xStep) - 1;
                var cellY = Math.ceil((y - this.yOrigin) / this.yStep) - 1;
                return { x: cellX, y: cellY };
            }

            private GetContainerFromGridCell(gridCell: { x: number; y: number }): { container: BMA.Model.Container; layout: BMA.Model.ContainerLayout } {
                var current = this.Current;

                var layouts = current.layout.Containers;
                for (var i = 0; i < layouts.length; i++) {
                    if (layouts[i].PositionX === gridCell.x && layouts[i].PositionY === gridCell.y) {
                        return { container: current.model.Containers[i], layout: layouts[i] };
                    }
                }

                return undefined;
            }

            private GetConstantsFromGridCell(gridCell: { x: number; y: number }): { container: BMA.Model.Variable; layout: BMA.Model.VarialbeLayout }[] {
                var result = [];
                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var vGridCell = this.GetGridCell(variableLayout.PositionX, variableLayout.PositionY);

                    if (gridCell.x === vGridCell.x && gridCell.y === vGridCell.y) {
                        result.push({ variable: variable, variableLayout: variableLayout });
                    }
                }
                return result;
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

            private get Grid(): { x0: number; y0: number; xStep: number; yStep: number } {
                return { x0: this.xOrigin, y0: this.yOrigin, xStep: this.xStep, yStep: this.yStep };
            }

            private GetVariableById(layout: BMA.Model.Layout, id: number) {
                var variableLayouts = layout.Variables;
                for (var i = 0; i < variableLayouts.length; i++) {
                    var variableLayout = variableLayouts[i];
                    if (variableLayout.Id === id) {
                        return variableLayout;
                    }
                }

                throw "No such variable in model";
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
                    svgElements.push(element.RenderToSvg(this.svg, { layout: containerLayout, grid: this.Grid }));
                }

                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    this.svg.clear();
                    svgElements.push(element.RenderToSvg(this.svg, { layout: variableLayout, grid: this.Grid }));
                }

                var relationships = this.Current.model.Relationships;
                for (var i = 0; i < relationships.length; i++) {
                    var relationship = relationships[i];
                    var element = window.ElementRegistry.GetElementByType(relationship.Type);

                    var start = this.GetVariableById(this.Current.layout, relationship.FromVariableId);
                    var end = this.GetVariableById(this.Current.layout, relationship.ToVariableId);

                    svgElements.push(element.RenderToSvg(this.svg, {
                        layout: { start: start, end: end },
                        grid: this.Grid
                    }));
                }

                //constructing final svg image
                this.svg.clear();

                var defs = this.svg.defs("bmaDefs");
                var activatorMarker = this.svg.marker(defs, "Activator", 4, 0, 8, 8, "auto", { viewBox: "0 -4 4 8" });
                this.svg.polyline(activatorMarker, [[0, 4], [4, 0], [0, -4]], { fill: "none", stroke: "#808080", strokeWidth: "1px" });
                var inhibitorMarker = this.svg.marker(defs, "Inhibitor", 0, 0, 2, 6, "auto", { viewBox: "0 -3 2 6" });
                this.svg.line(inhibitorMarker, 0, 3, 0, -3, { fill: "none", stroke: "#808080", strokeWidth: "2px" });

                for (var i = 0; i < svgElements.length; i++) {
                    this.svg.add(svgElements[i]);
                }

                /*
                if (this.stagingLine !== undefined) {
                    this.svg.line(
                        this.stagingLine.x0,
                        this.stagingLine.y0,
                        this.stagingLine.x1,
                        this.stagingLine.y1,
                        { stroke: "black", strokeWidth: 2, fill: "black", "marker-end": "url(#" + this.selectedType + ")" });
                }
                */

                return $(this.svg.toSVG()).children();
            }
        }
    }
} 