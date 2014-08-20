/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model\biomodel.ts"/>
/// <reference path="model\model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>

var BMA;
(function (BMA) {
    (function (Presenters) {
        var DesignSurfacePresenter = (function () {
            function DesignSurfacePresenter(appModel, svgPlotDriver, dragService, undoButton, redoButton) {
                var _this = this;
                this.currentModelIndex = -1;
                this.xOrigin = 0;
                this.yOrigin = 0;
                this.xStep = 250;
                this.yStep = 280;
                this.variableIndex = 0;
                this.stagingLine = undefined;
                var that = this;
                this.appModel = appModel;
                this.undoButton = undoButton;
                this.redoButton = redoButton;

                this.driver = svgPlotDriver;
                this.models = [];

                svgPlotDriver.SetGrid(this.xOrigin, this.yOrigin, this.xStep, this.yStep);

                window.Commands.On("AddElementSelect", function (type) {
                    _this.selectedType = type;
                    _this.driver.TurnNavigation(type === undefined);
                    _this.stagingLine = undefined;
                    //this.selectedType = this.selectedType === type ? undefined : type;
                    //this.driver.TurnNavigation(this.selectedType === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", function (args) {
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
                                variables.push(new BMA.Model.Variable(_this.variableIndex, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(_this.variableIndex++, args.x, args.y, 0, 0, 0));
                                var newmodel = new BMA.Model.BioModel(model.Containers, variables, model.Relationships);
                                var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                                that.Dup(newmodel, newlayout);
                                break;
                            case "Default":
                                var variables = model.Variables.slice(0);
                                var variableLayouts = layout.Variables.slice(0);
                                variables.push(new BMA.Model.Variable(_this.variableIndex, 0, that.selectedType, 0, 0, ""));
                                variableLayouts.push(new BMA.Model.VarialbeLayout(_this.variableIndex++, args.x, args.y, 0, 0, 0));
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

                window.Commands.On("Undo", function () {
                    _this.Undo();
                });

                window.Commands.On("Redo", function () {
                    _this.Redo();
                });

                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: function (svg) {
                        _this.svg = svg;

                        if (_this.Current !== undefined) {
                            var drawingSvg = _this.CreateSvg();
                            _this.driver.Draw(drawingSvg);
                        }
                    }
                });

                var dragSubject = dragService.GetDragSubject();

                dragSubject.dragStart.subscribe(function (gesture) {
                    if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor")) {
                        var id = _this.GetVariableAtPosition(gesture.x, gesture.y);
                        if (id !== undefined) {
                            _this.stagingLine = {};
                            _this.stagingLine.id = id;
                            _this.stagingLine.x0 = gesture.x;
                            _this.stagingLine.y0 = gesture.y;
                            return;
                        }
                    }
                    _this.stagingLine = undefined;
                });

                dragSubject.drag.subscribe(function (gesture) {
                    if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && that.stagingLine !== undefined) {
                        that.stagingLine.x1 = gesture.x1;
                        that.stagingLine.y1 = gesture.y1;

                        //Redraw only svg for better performance
                        if (that.svg !== undefined) {
                            if (that.stagingLine.svg !== undefined) {
                                that.svg.remove(that.stagingLine.svg);
                            }

                            that.stagingLine.svg = that.svg.line(that.stagingLine.x0, that.stagingLine.y0, that.stagingLine.x1, that.stagingLine.y1, {
                                stroke: "black",
                                strokeWidth: 2,
                                fill: "black",
                                "marker-end": "url(#" + that.selectedType + ")",
                                id: "stagingLine"
                            });

                            that.driver.Draw(that.GetCurrentSVG(that.svg));
                        }

                        return;
                    }
                    //this.stagingLine = undefined;
                });

                dragSubject.dragEnd.subscribe(function (gesture) {
                    if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && _this.stagingLine !== undefined) {
                        _this.TryAddStagingLineAsLink();
                        _this.stagingLine = undefined;
                        _this.OnModelUpdated();
                    }
                });

                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }
            DesignSurfacePresenter.prototype.GetCurrentSVG = function (svg) {
                return $(svg.toSVG()).children();
            };

            DesignSurfacePresenter.prototype.GetVariableAtPosition = function (x, y) {
                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    if (Math.abs(variableLayout.PositionX - x) < 20 && Math.abs(variableLayout.PositionY - y) < 20) {
                        return variable.Id;
                    }
                }

                return undefined;
            };

            DesignSurfacePresenter.prototype.TryAddStagingLineAsLink = function () {
                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    if (Math.abs(variableLayout.PositionX - this.stagingLine.x1) < 20 && Math.abs(variableLayout.PositionY - this.stagingLine.y1) < 20) {
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
            };

            DesignSurfacePresenter.prototype.GetGridCell = function (x, y) {
                var cellX = Math.ceil((x - this.xOrigin) / this.xStep) - 1;
                var cellY = Math.ceil((y - this.yOrigin) / this.yStep) - 1;
                return { x: cellX, y: cellY };
            };

            DesignSurfacePresenter.prototype.GetContainerFromGridCell = function (gridCell) {
                var current = this.Current;

                var layouts = current.layout.Containers;
                for (var i = 0; i < layouts.length; i++) {
                    if (layouts[i].PositionX === gridCell.x && layouts[i].PositionY === gridCell.y) {
                        return { container: current.model.Containers[i], layout: layouts[i] };
                    }
                }

                return undefined;
            };

            DesignSurfacePresenter.prototype.GetConstantsFromGridCell = function (gridCell) {
                return [];
            };

            DesignSurfacePresenter.prototype.OnModelUpdated = function () {
                this.undoButton.Turn(this.CanUndo);
                this.redoButton.Turn(this.CanRedo);

                this.appModel.BioModel = this.Current.model;
                this.appModel.Layout = this.Current.layout;

                var drawingSvg = this.CreateSvg();
                this.driver.Draw(drawingSvg);
            };

            DesignSurfacePresenter.prototype.Undo = function () {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    this.OnModelUpdated();
                }
            };

            DesignSurfacePresenter.prototype.Redo = function () {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.OnModelUpdated();
                }
            };

            DesignSurfacePresenter.prototype.Truncate = function () {
                this.models.length = this.currentModelIndex + 1;
            };

            DesignSurfacePresenter.prototype.Dup = function (m, l) {
                this.Truncate();
                var current = this.Current;
                this.models[this.currentModelIndex] = { model: current.model.Clone(), layout: current.layout.Clone() };
                this.models.push({ model: m, layout: l });
                ++this.currentModelIndex;
                this.OnModelUpdated();
            };

            Object.defineProperty(DesignSurfacePresenter.prototype, "CanUndo", {
                get: function () {
                    return this.currentModelIndex > 0;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(DesignSurfacePresenter.prototype, "CanRedo", {
                get: function () {
                    return this.currentModelIndex < this.models.length - 1;
                },
                enumerable: true,
                configurable: true
            });

            DesignSurfacePresenter.prototype.Set = function (m, l) {
                this.models = [{ model: m, layout: l }];
                this.currentModelIndex = 0;
                this.OnModelUpdated();
            };

            Object.defineProperty(DesignSurfacePresenter.prototype, "Current", {
                get: function () {
                    return this.models[this.currentModelIndex];
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(DesignSurfacePresenter.prototype, "Grid", {
                get: function () {
                    return { x0: this.xOrigin, y0: this.yOrigin, xStep: this.xStep, yStep: this.yStep };
                },
                enumerable: true,
                configurable: true
            });

            DesignSurfacePresenter.prototype.GetVariableById = function (layout, id) {
                var variableLayouts = layout.Variables;
                for (var i = 0; i < variableLayouts.length; i++) {
                    var variableLayout = variableLayouts[i];
                    if (variableLayout.Id === id) {
                        return variableLayout;
                    }
                }

                throw "No such variable in model";
            };

            DesignSurfacePresenter.prototype.CreateSvg = function () {
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
                this.svg.polyline(activatorMarker, [[0, 4], [4, 0], [0, -4]], { fill: "none", stroke: "black", strokeWidth: "1px" });
                var inhibitorMarker = this.svg.marker(defs, "Inhibitor", 0, 0, 2, 6, "auto", { viewBox: "0 -3 2 6" });
                this.svg.line(inhibitorMarker, 0, 3, 0, -3, { fill: "none", stroke: "black", strokeWidth: "2px" });

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
            };
            return DesignSurfacePresenter;
        })();
        Presenters.DesignSurfacePresenter = DesignSurfacePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=presenters.js.map
