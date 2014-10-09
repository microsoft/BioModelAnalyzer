/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\model\biomodel.ts"/>
/// <reference path="..\model\model.ts"/>
/// <reference path="..\uidrivers.ts"/>
/// <reference path="..\commands.ts"/>

var BMA;
(function (BMA) {
    (function (Presenters) {
        var DesignSurfacePresenter = (function () {
            function DesignSurfacePresenter(appModel, svgPlotDriver, highlightDriver, navigationDriver, dragService, undoButton, redoButton, variableEditorDriver, contextMenu) {
                var _this = this;
                this.currentModelIndex = -1;
                this.xOrigin = 0;
                this.yOrigin = 0;
                this.xStep = 250;
                this.yStep = 280;
                this.variableIndex = 1;
                this.stagingLine = undefined;
                this.stagingGroup = undefined;
                this.stagingVariable = undefined;
                this.editingVariableId = undefined;
                var that = this;
                this.appModel = appModel;
                this.undoButton = undoButton;
                this.redoButton = redoButton;

                this.driver = svgPlotDriver;
                this.highlightDriver = highlightDriver;
                this.navigationDriver = navigationDriver;
                this.variableEditor = variableEditorDriver;
                this.contextMenu = contextMenu;

                this.models = [];

                svgPlotDriver.SetGrid(this.xOrigin, this.yOrigin, this.xStep, this.yStep);

                window.Commands.On("AddElementSelect", function (type) {
                    that.selectedType = type;
                    that.navigationDriver.TurnNavigation(type === undefined);
                    that.stagingLine = undefined;
                    //this.selectedType = this.selectedType === type ? undefined : type;
                    //this.driver.TurnNavigation(this.selectedType === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", function (args) {
                    if (that.selectedType !== undefined) {
                        that.TryAddVariable(args.x, args.y, that.selectedType, undefined);
                    } else {
                        var id = that.GetVariableAtPosition(args.x, args.y);
                        if (id !== undefined) {
                            that.editingVariableId = id;
                            that.variableEditor.Initialize(that.GetVariableById(that.Current.layout, that.Current.model, id).model, that.Current.model);
                            that.variableEditor.Show(0, 0);
                        }
                    }
                });

                window.Commands.On("VariableEdited", function () {
                    var that = _this;
                    if (that.editingVariableId !== undefined) {
                        var model = _this.Current.model;
                        var variables = model.Variables;
                        var editingVariableIndex = -1;
                        for (var i = 0; i < variables.length; i++) {
                            if (variables[i].Id === that.editingVariableId) {
                                editingVariableIndex = i;
                                break;
                            }
                        }
                        if (editingVariableIndex !== -1) {
                            var params = that.variableEditor.GetVariableProperties();
                            variables[i] = new BMA.Model.Variable(variables[i].Id, variables[i].ContainerId, variables[i].Type, params.name, params.rangeFrom, params.rangeTo, params.formula);
                            that.OnModelUpdated();
                        }
                    }
                });

                window.Commands.On("ModelReset", function () {
                    _this.Set(_this.appModel.BioModel, _this.appModel.Layout);
                });

                window.Commands.On("Undo", function () {
                    _this.Undo();
                });

                window.Commands.On("Redo", function () {
                    _this.Redo();
                });

                window.Commands.On("DrawingSurfaceContextMenuOpening", function (args) {
                    var x = that.driver.GetPlotX(args.left);
                    var y = that.driver.GetPlotY(args.top);

                    var id = that.GetVariableAtPosition(x, y);
                    var containerId = that.GetContainerAtPosition(x, y);
                    var relationshipId = that.GetRelationshipAtPosition(x, y, that.driver.GetPixelWidth());
                    var cntSize = containerId !== undefined ? that.Current.layout.GetContainerById(containerId).Size : undefined;

                    var canPaste = id === undefined && containerId === undefined && that.clipboard !== undefined;

                    that.contextMenu.EnableMenuItems([
                        { name: "Copy", isVisible: id !== undefined || containerId !== undefined },
                        { name: "Paste", isVisible: canPaste },
                        { name: "Cut", isVisible: id !== undefined || containerId !== undefined },
                        { name: "Delete", isVisible: id !== undefined || containerId !== undefined || relationshipId !== undefined },
                        { name: "Size", isVisible: containerId !== undefined },
                        { name: "ResizeCellTo1x1", isVisible: true },
                        { name: "ResizeCellTo2x2", isVisible: true },
                        { name: "ResizeCellTo3x3", isVisible: true }
                    ]);

                    if (id !== undefined) {
                        that.contextElement = { id: id, type: "variable" };
                    } else if (containerId !== undefined) {
                        that.contextElement = { id: containerId, type: "container" };
                    } else if (relationshipId !== undefined) {
                        that.contextElement = { id: relationshipId, type: "relationship" };
                    }
                });

                window.Commands.On("DrawingSurfaceDelete", function (args) {
                    if (that.contextElement !== undefined) {
                        if (that.contextElement.type === "variable") {
                            that.RemoveVariable(that.contextElement.id);
                        } else if (that.contextElement.type === "relationship") {
                            that.RemoveRelationship(that.contextElement.id);
                        } else if (that.contextElement.type === "container") {
                            that.RemoveContainer(that.contextElement.id);
                        }

                        that.contextElement = undefined;
                    }
                });

                window.Commands.On("DrawingSurfaceCopy", function (args) {
                    if (that.contextElement !== undefined) {
                        if (that.contextElement.type === "variable") {
                            that.clipboard = that.contextElement;
                            that.RemoveVariable(that.contextElement.id);
                        } else if (that.contextElement.type === "container") {
                            that.RemoveContainer(that.contextElement.id);
                        }

                        that.contextElement = undefined;
                    }
                });

                window.Commands.On("DrawingSurfaceResizeCell", function (args) {
                    if (that.contextElement !== undefined && that.contextElement.type === "container") {
                        var container = that.Current.layout.GetContainerById(that.contextElement.id);
                        if (container !== undefined) {
                            var sizeDiff = args.size - container.Size;

                            var containerLayouts = that.Current.layout.Containers;
                            var variables = that.Current.model.Variables;
                            var variableLayouts = that.Current.layout.Variables;

                            var newCnt = [];
                            for (var i = 0; i < containerLayouts.length; i++) {
                                var cnt = containerLayouts[i];
                                if (cnt.Id === container.Id) {
                                    newCnt.push(new BMA.Model.ContainerLayout(cnt.Id, args.size, cnt.PositionX, cnt.PositionY));
                                } else if (cnt.PositionX > container.PositionX || cnt.PositionY > container.PositionY) {
                                    newCnt.push(new BMA.Model.ContainerLayout(cnt.Id, cnt.Size, cnt.PositionX > container.PositionX ? cnt.PositionX + sizeDiff : cnt.PositionX, cnt.PositionY > container.PositionY ? cnt.PositionY + sizeDiff : cnt.PositionY));
                                } else
                                    newCnt.push(cnt);
                            }

                            var cntX = container.PositionX * that.xStep + that.xOrigin;
                            var cntY = container.PositionY * that.yStep + that.yOrigin;
                            var newVL = [];
                            for (var i = 0; i < variableLayouts.length; i++) {
                                var vl = variableLayouts[i];
                                if (variables[i].ContainerId === container.Id) {
                                    newVL.push(new BMA.Model.VarialbeLayout(vl.Id, cntX + (vl.PositionX - cntX) * args.size / container.Size, cntY + (vl.PositionY - cntY) * args.size / container.Size, 0, 0, vl.Angle));
                                } else {
                                    newVL.push(new BMA.Model.VarialbeLayout(vl.Id, vl.PositionX > cntX + that.xStep ? vl.PositionX + sizeDiff * that.xStep : vl.PositionX, vl.PositionY > cntY + that.yStep ? vl.PositionY + sizeDiff * that.yStep : vl.PositionY, 0, 0, vl.Angle));
                                }
                            }

                            var newlayout = new BMA.Model.Layout(newCnt, newVL);
                            _this.Dup(new BMA.Model.BioModel(that.Current.model.Name, that.Current.model.Variables, that.Current.model.Relationships), newlayout);
                        }
                    }
                });

                window.Commands.On("DrawingSurfaceRefreshOutput", function () {
                    if (_this.Current !== undefined) {
                        var drawingSvg = _this.CreateSvg();
                        _this.highlightDriver.HighlightAreas(_this.PrepareHighlightAreas());
                        _this.driver.Draw(drawingSvg);
                    }
                });

                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: function (svg) {
                        _this.svg = svg;

                        if (_this.Current !== undefined) {
                            var drawingSvg = _this.CreateSvg();
                            _this.highlightDriver.HighlightAreas(_this.PrepareHighlightAreas());
                            _this.driver.Draw(drawingSvg);
                        }
                    }
                });

                var dragSubject = dragService.GetDragSubject();

                window.Commands.On("ZoomSliderChanged", function (args) {
                    if (args.isExternal !== true) {
                        var value = args.value * 24 + 800;
                        navigationDriver.SetZoom(value);
                    }
                });

                window.Commands.On("VisibleRectChanged", function (param) {
                    if (param < window.PlotSettings.MinWidth) {
                        param = window.PlotSettings.MinWidth;
                        navigationDriver.SetZoom(param);
                    }
                    if (param > window.PlotSettings.MaxWidth) {
                        param = window.PlotSettings.MaxWidth;
                        navigationDriver.SetZoom(param);
                    }
                    var zoom = (param - window.PlotSettings.MinWidth) / 24;
                    window.Commands.Execute("ZoomSliderBind", zoom);
                });

                /*
                window.Commands.On("DrawingSurfaceMouseMove", function (param) {
                
                var cnt = that.GetContainerAtPosition(param.x, param.y);
                that.driver.SetGridVisibility(cnt===undefined);
                });
                */
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
                    } else if (that.selectedType === undefined) {
                        var id = _this.GetVariableAtPosition(gesture.x, gesture.y);
                        console.log(id);
                        if (id !== undefined) {
                            that.navigationDriver.TurnNavigation(false);
                            var vl = that.GetVariableById(that.Current.layout, that.Current.model, id);
                            that.stagingVariable = { model: vl.model, layout: vl.layout };
                        } else {
                            that.navigationDriver.TurnNavigation(true);
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
                                stroke: "#808080",
                                strokeWidth: 2,
                                fill: "#808080",
                                "marker-end": "url(#" + that.selectedType + ")",
                                id: "stagingLine"
                            });

                            that.highlightDriver.HighlightAreas(that.PrepareHighlightAreas());
                            that.driver.Draw(that.GetCurrentSVG(that.svg));
                        }

                        return;
                    } else if (that.stagingVariable !== undefined) {
                        that.stagingVariable.layout = new BMA.Model.VarialbeLayout(that.stagingVariable.layout.Id, gesture.x1, gesture.y1, 0, 0, 0);
                        var drawingSvg = that.CreateSvg();
                        that.driver.Draw(drawingSvg);
                    }
                });

                dragSubject.dragEnd.subscribe(function (gesture) {
                    if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && _this.stagingLine !== undefined) {
                        _this.TryAddStagingLineAsLink();
                        _this.stagingLine = undefined;
                        _this.OnModelUpdated();
                    }

                    if (that.stagingVariable !== undefined) {
                        var x = that.stagingVariable.layout.PositionX;
                        var y = that.stagingVariable.layout.PositionY;
                        var type = that.stagingVariable.model.Type;
                        var id = that.stagingVariable.model.Id;
                        that.stagingVariable = undefined;
                        if (!that.TryAddVariable(x, y, type, id)) {
                            var drawingSvg = that.CreateSvg();
                            that.highlightDriver.HighlightAreas(that.PrepareHighlightAreas());
                            that.driver.Draw(drawingSvg);
                        }
                    }
                });

                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }
            DesignSurfacePresenter.prototype.GetCurrentSVG = function (svg) {
                return $(svg.toSVG()).children();
            };

            DesignSurfacePresenter.prototype.RemoveVariable = function (id) {
                if (this.editingVariableId === this.contextElement.id) {
                    this.editingVariableId = undefined;
                }

                var wasRemoved = false;

                var model = this.Current.model;
                var layout = this.Current.layout;

                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;

                var newVars = [];
                var newVarLs = [];

                for (var i = 0; i < variables.length; i++) {
                    if (variables[i].Id !== id) {
                        newVars.push(variables[i]);
                        newVarLs.push(variableLayouts[i]);
                    } else {
                        wasRemoved = true;
                    }
                }

                var relationships = this.Current.model.Relationships;

                var newRels = [];

                for (var i = 0; i < relationships.length; i++) {
                    if (relationships[i].FromVariableId !== id && relationships[i].ToVariableId !== id) {
                        newRels.push(relationships[i]);
                    }
                }

                if (wasRemoved === true) {
                    var newmodel = new BMA.Model.BioModel(model.Name, newVars, newRels);
                    var newlayout = new BMA.Model.Layout(layout.Containers, newVarLs);
                    this.Dup(newmodel, newlayout);
                }
            };

            DesignSurfacePresenter.prototype.RemoveContainer = function (id) {
                var wasRemoved = false;

                var model = this.Current.model;
                var layout = this.Current.layout;

                var containers = layout.Containers;
                var newCnt = [];

                for (var i = 0; i < containers.length; i++) {
                    var container = containers[i];
                    if (container.Id !== id) {
                        newCnt.push(container);
                    } else {
                        wasRemoved = true;
                    }
                }

                if (wasRemoved === true) {
                    var variables = model.Variables;
                    var variableLayouts = layout.Variables;

                    var newV = [];
                    var newVL = [];
                    var removed = [];

                    for (var i = 0; i < variables.length; i++) {
                        if (variables[i].Type === "Constant" || variables[i].ContainerId !== id) {
                            newV.push(variables[i]);
                            newVL.push(variableLayouts[i]);
                        } else {
                            removed.push(variables[i].Id);
                            if (this.editingVariableId === variables[i].Id) {
                                this.editingVariableId = undefined;
                            }
                        }
                    }

                    var relationships = model.Relationships;
                    var newRels = [];

                    for (var i = 0; i < relationships.length; i++) {
                        var r = relationships[i];
                        var shouldBeRemoved = false;
                        for (var j = 0; j < removed.length; j++) {
                            if (r.FromVariableId === removed[j] || r.ToVariableId === removed[j]) {
                                shouldBeRemoved = true;
                                break;
                            }
                        }

                        if (shouldBeRemoved === false) {
                            newRels.push(r);
                        }
                    }

                    var newmodel = new BMA.Model.BioModel(model.Name, newV, newRels);
                    var newlayout = new BMA.Model.Layout(newCnt, newVL);
                    this.Dup(newmodel, newlayout);
                }
            };

            DesignSurfacePresenter.prototype.RemoveRelationship = function (id) {
                var wasRemoved = false;

                var model = this.Current.model;
                var layout = this.Current.layout;

                var relationships = this.Current.model.Relationships;

                var newRels = [];

                for (var i = 0; i < relationships.length; i++) {
                    if (relationships[i].Id !== id) {
                        newRels.push(relationships[i]);
                    } else {
                        wasRemoved = true;
                    }
                }

                if (wasRemoved === true) {
                    var newmodel = new BMA.Model.BioModel(model.Name, model.Variables, newRels);
                    var newlayout = new BMA.Model.Layout(layout.Containers, layout.Variables);
                    this.Dup(newmodel, newlayout);
                }
            };

            DesignSurfacePresenter.prototype.GetVariableAtPosition = function (x, y) {
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
            };

            DesignSurfacePresenter.prototype.GetContainerAtPosition = function (x, y) {
                var containers = this.Current.layout.Containers;
                var element = window.ElementRegistry.GetElementByType("Container");
                var grid = this.Grid;
                for (var i = 0; i < containers.length; i++) {
                    var containerLayout = containers[i];
                    if (element.IntersectsBorder(x, y, (containerLayout.PositionX + 0.5) * grid.xStep + grid.x0, (containerLayout.PositionY + 0.5) * grid.yStep + grid.y0, { Size: containerLayout.Size, xStep: grid.xStep / 2, yStep: grid.yStep / 2 })) {
                        return containerLayout.Id;
                    }
                }

                return undefined;
            };

            DesignSurfacePresenter.prototype.GetRelationshipAtPosition = function (x, y, pixelWidth) {
                var relationships = this.Current.model.Relationships;
                var layout = this.Current.layout;

                for (var i = 0; i < relationships.length; i++) {
                    var relationship = relationships[i];
                    var var1 = layout.GetVariableById(relationship.FromVariableId);
                    var var2 = layout.GetVariableById(relationship.ToVariableId);

                    var elx = { x: var1.PositionX, y: var1.PositionY, pixelWidth: pixelWidth };
                    var ely = { x: var2.PositionX, y: var2.PositionY };

                    var elem = window.ElementRegistry.GetElementByType(relationship.Type);
                    if (elem.Contains(x, y, elx, ely)) {
                        return relationship.Id;
                    }
                }

                return undefined;
            };

            DesignSurfacePresenter.prototype.Intersects = function (a, b) {
                return (Math.abs(a.x - b.x) * 2 <= (a.width + b.width)) && (Math.abs(a.y - b.y) * 2 <= (a.height + b.height));
            };

            DesignSurfacePresenter.prototype.Contains = function (gridCell, bbox) {
                return bbox.width < this.xStep && bbox.height < this.yStep && bbox.x > gridCell.x * this.xStep + this.xOrigin && bbox.x + bbox.width < (gridCell.x + 1) * this.xStep + this.xOrigin && bbox.y > gridCell.y * this.yStep + this.yOrigin && bbox.y + bbox.height < (gridCell.y + 1) * this.yStep + this.yOrigin;
            };

            DesignSurfacePresenter.prototype.TryAddStagingLineAsLink = function () {
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
                        relationships.push(new BMA.Model.Relationship(this.variableIndex++, this.stagingLine.id, variable.Id, this.selectedType));
                        var newmodel = new BMA.Model.BioModel(model.Name, model.Variables, relationships);
                        this.Dup(newmodel, layout);

                        return;
                    }
                }
            };

            DesignSurfacePresenter.prototype.TryAddVariable = function (x, y, type, id) {
                var that = this;
                var current = that.Current;
                var model = current.model;
                var layout = current.layout;

                switch (type) {
                    case "Container":
                        var containerLayouts = layout.Containers.slice(0);

                        var gridCell = that.GetGridCell(x, y);

                        if (that.GetContainerFromGridCell(gridCell) === undefined && that.GetConstantsFromGridCell(gridCell).length === 0) {
                            if (id !== undefined) {
                                for (var i = 0; i < containerLayouts.length; i++) {
                                    if (containerLayouts[i].Id === id) {
                                        containerLayouts[i] = new BMA.Model.ContainerLayout(id, containerLayouts[i].Size, gridCell.x, gridCell.y);
                                    }
                                }
                            } else {
                                containerLayouts.push(new BMA.Model.ContainerLayout(that.variableIndex++, 1, gridCell.x, gridCell.y));
                            }

                            var newmodel = new BMA.Model.BioModel(model.Name, model.Variables, model.Relationships);
                            var newlayout = new BMA.Model.Layout(containerLayouts, layout.Variables);
                            that.Dup(newmodel, newlayout);
                            return true;
                        }

                        break;
                    case "Constant":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        var bbox = window.ElementRegistry.GetElementByType("Constant").GetBoundingBox(x, y);
                        var gridCell = that.GetGridCell(x, y);

                        if (that.GetContainerFromGridCell(gridCell) !== undefined || !this.Contains(gridCell, bbox)) {
                            return;
                        }

                        for (var i = 0; i < variableLayouts.length; i++) {
                            var variable = variables[i];
                            var variableLayout = variableLayouts[i];
                            var elementBBox = window.ElementRegistry.GetElementByType(variable.Type).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                            if (this.Intersects(bbox, elementBBox))
                                return;
                        }

                        if (id !== undefined) {
                            for (var i = 0; i < variables.length; i++) {
                                if (variables[i].Id === id) {
                                    variableLayouts[i] = new BMA.Model.VarialbeLayout(id, x, y, 0, 0, 0);
                                }
                            }
                        } else {
                            variables.push(new BMA.Model.Variable(this.variableIndex, 0, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, x, y, 0, 0, 0));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.Dup(newmodel, newlayout);
                        return true;
                        break;
                    case "Default":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        var bbox = window.ElementRegistry.GetElementByType("Constant").GetBoundingBox(x, y);
                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

                        if (container === undefined || !window.ElementRegistry.GetElementByType("Container").ContainsBBox(bbox, (container.PositionX + 0.5) * this.xStep, (container.PositionY + 0.5) * this.yStep, { Size: container.Size, xStep: that.Grid.xStep / 2, yStep: that.Grid.yStep / 2 })) {
                            return;
                        }

                        for (var i = 0; i < variableLayouts.length; i++) {
                            var variable = variables[i];
                            var variableLayout = variableLayouts[i];
                            var elementBBox = window.ElementRegistry.GetElementByType(variable.Type).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                            if (this.Intersects(bbox, elementBBox))
                                return;
                        }

                        if (id !== undefined) {
                            for (var i = 0; i < variables.length; i++) {
                                if (variables[i].Id === id) {
                                    var vrbl = variables[i];
                                    if (vrbl.ContainerId !== container.Id) {
                                        variables[i] = new BMA.Model.Variable(vrbl.Id, container.Id, vrbl.Type, vrbl.Name, vrbl.RangeFrom, vrbl.RangeTo, vrbl.Formula);
                                    }
                                    variableLayouts[i] = new BMA.Model.VarialbeLayout(id, x, y, 0, 0, 0);
                                }
                            }
                        } else {
                            variables.push(new BMA.Model.Variable(this.variableIndex, container.Id, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, x, y, 0, 0, 0));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.Dup(newmodel, newlayout);
                        return true;
                        break;
                    case "MembraneReceptor":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        var bbox = window.ElementRegistry.GetElementByType("Constant").GetBoundingBox(x, y);
                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

                        if (container === undefined || !window.ElementRegistry.GetElementByType("Container").IntersectsBorder(x, y, (container.PositionX + 0.5) * this.xStep, (container.PositionY + 0.5) * this.yStep, { Size: container.Size, xStep: that.Grid.xStep / 2, yStep: that.Grid.yStep / 2 })) {
                            return;
                        }

                        for (var i = 0; i < variableLayouts.length; i++) {
                            var variable = variables[i];
                            var variableLayout = variableLayouts[i];
                            var elementBBox = window.ElementRegistry.GetElementByType(variable.Type).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                            if (this.Intersects(bbox, elementBBox))
                                return;
                        }

                        var containerX = (container.PositionX + 0.5) * this.xStep + this.xOrigin + (container.Size - 1) * this.xStep / 2;
                        var containerY = (container.PositionY + 0.5) * this.yStep + this.yOrigin + (container.Size - 1) * this.yStep / 2;

                        var v = {
                            x: x - containerX,
                            y: y - containerY
                        };
                        var len = Math.sqrt(v.x * v.x + v.y * v.y);

                        v.x = v.x / len;
                        v.y = v.y / len;

                        var acos = Math.acos(-v.y);

                        var angle = acos * v.x / Math.abs(v.x);

                        angle = angle * 180 / Math.PI;
                        if (angle < 0)
                            angle += 360;

                        if (id !== undefined) {
                            for (var i = 0; i < variables.length; i++) {
                                if (variables[i].Id === id) {
                                    variableLayouts[i] = new BMA.Model.VarialbeLayout(id, x, y, 0, 0, angle);
                                }
                            }
                        } else {
                            var pos = BMA.SVGHelper.GeEllipsePoint(containerX + 2.5 * container.Size, containerY, 107 * container.Size, 127 * container.Size, x, y);
                            variables.push(new BMA.Model.Variable(this.variableIndex, container.Id, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VarialbeLayout(this.variableIndex++, pos.x, pos.y, 0, 0, angle));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.Dup(newmodel, newlayout);
                        return true;
                        break;
                }

                return false;
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
                    if (layouts[i].PositionX <= gridCell.x && layouts[i].PositionX + layouts[i].Size > gridCell.x && layouts[i].PositionY <= gridCell.y && layouts[i].PositionY + layouts[i].Size > gridCell.y) {
                        return layouts[i];
                    }
                }

                return undefined;
            };

            //private GetContainerGridCells(containerLayout: BMA.Model.ContainerLayout): { x: number; y: number }[] {
            //    var result = [];
            //    var size = containerLayout.Size;
            //    for (var i = 0; i < size; i++) {
            //        for (var j = 0; j < size; j++) {
            //            result.push({ x: i + containerLayout.PositionX, y: j + containerLayout.PositionY });
            //        }
            //    }
            //    return result;
            //}
            DesignSurfacePresenter.prototype.GetConstantsFromGridCell = function (gridCell) {
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
            };

            DesignSurfacePresenter.prototype.OnModelUpdated = function () {
                this.undoButton.Turn(this.CanUndo);
                this.redoButton.Turn(this.CanRedo);

                if (this.editingVariableId !== undefined) {
                    this.variableEditor.Initialize(this.GetVariableById(this.Current.layout, this.Current.model, this.editingVariableId).model, this.Current.model);
                }

                this.appModel.BioModel = this.Current.model;
                this.appModel.Layout = this.Current.layout;

                var drawingSvg = this.CreateSvg();
                this.highlightDriver.HighlightAreas(this.PrepareHighlightAreas());
                this.driver.Draw(drawingSvg);
            };

            DesignSurfacePresenter.prototype.PrepareHighlightAreas = function () {
                var result = [];
                var containers = this.Current.layout.Containers;
                var grid = this.Grid;
                for (var i = 0; i < containers.length; i++) {
                    var container = containers[i];
                    if (container.Size > 1) {
                        var containerX = container.PositionX * this.xStep + this.xOrigin;
                        var containerY = (container.PositionY + container.Size) * this.yStep + this.yOrigin;
                        result.push({
                            x: containerX + 1,
                            y: -containerY - 1,
                            width: this.xStep * container.Size - 2,
                            height: this.yStep * container.Size - 2,
                            fill: "white"
                        });
                    }
                }

                return result;
            };

            DesignSurfacePresenter.prototype.Undo = function () {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    this.variableEditor.Hide();
                    this.editingVariableId = undefined;
                    this.OnModelUpdated();
                }
            };

            DesignSurfacePresenter.prototype.Redo = function () {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.variableEditor.Hide();
                    this.editingVariableId = undefined;
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

            DesignSurfacePresenter.prototype.ResetVariableIdIndex = function () {
                this.variableIndex = 1;

                var m = this.Current.model;
                var l = this.Current.layout;

                for (var i = 0; i < m.Variables.length; i++) {
                    if (m.Variables[i].Id >= this.variableIndex)
                        this.variableIndex = m.Variables[i].Id + 1;
                }

                for (var i = 0; i < l.Containers.length; i++) {
                    if (l.Containers[i].Id >= this.variableIndex) {
                        this.variableIndex = l.Containers[i].Id + 1;
                    }
                }

                for (var i = 0; i < m.Relationships.length; i++) {
                    if (m.Relationships[i].Id >= this.variableIndex) {
                        this.variableIndex = m.Relationships[i].Id + 1;
                    }
                }
            };

            DesignSurfacePresenter.prototype.Set = function (m, l) {
                this.models = [{ model: m, layout: l }];
                this.currentModelIndex = 0;
                this.ResetVariableIdIndex();
                this.variableEditor.Hide();
                this.editingVariableId = undefined;
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

            DesignSurfacePresenter.prototype.GetVariableById = function (layout, model, id) {
                var variableLayouts = layout.Variables;
                var variables = model.Variables;
                for (var i = 0; i < variableLayouts.length; i++) {
                    var variableLayout = variableLayouts[i];
                    if (variableLayout.Id === id) {
                        return { model: variables[i], layout: variableLayout };
                    }
                }

                throw "No such variable in model";
            };

            DesignSurfacePresenter.prototype.CreateSvg = function () {
                if (this.svg === undefined)
                    return undefined;

                //Generating svg elements from model and layout
                var svgElements = [];

                var containerLayouts = this.Current.layout.Containers;
                for (var i = 0; i < containerLayouts.length; i++) {
                    var containerLayout = containerLayouts[i];
                    var element = window.ElementRegistry.GetElementByType("Container");
                    svgElements.push(element.RenderToSvg({ layout: containerLayout, grid: this.Grid }));
                }

                var variables = this.Current.model.Variables;
                var variableLayouts = this.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];
                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    svgElements.push(element.RenderToSvg({ model: variable, layout: variableLayout, grid: this.Grid }));
                }

                var relationships = this.Current.model.Relationships;
                for (var i = 0; i < relationships.length; i++) {
                    var relationship = relationships[i];
                    var element = window.ElementRegistry.GetElementByType(relationship.Type);

                    var start = this.GetVariableById(this.Current.layout, this.Current.model, relationship.FromVariableId).layout;
                    var end = this.GetVariableById(this.Current.layout, this.Current.model, relationship.ToVariableId).layout;

                    svgElements.push(element.RenderToSvg({
                        layout: { start: start, end: end },
                        grid: this.Grid
                    }));
                }

                if (this.stagingVariable !== undefined) {
                    var element = window.ElementRegistry.GetElementByType(this.stagingVariable.model.Type);
                    svgElements.push(element.RenderToSvg({ model: this.stagingVariable.model, layout: this.stagingVariable.layout, grid: this.Grid }));
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

                return $(this.svg.toSVG()).children();
            };
            return DesignSurfacePresenter;
        })();
        Presenters.DesignSurfacePresenter = DesignSurfacePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=presenters.js.map
