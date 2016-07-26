/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\model\biomodel.ts"/>
/// <reference path="..\model\model.ts"/>
/// <reference path="..\uidrivers\commondrivers.ts"/>
/// <reference path="..\commands.ts"/>

interface JQuery {
    svg(): any;
    svg(options: any): any;
}

module BMA {
    export module Presenters {
        export class DesignSurfacePresenter {
            private appModel: BMA.Model.AppModel;
            private undoRedoPresenter: BMA.Presenters.UndoRedoPresenter;
            private editingModel: BMA.Model.BioModel;
            private editingLayout: BMA.Model.Layout;

            private selectedType: string;
            private driver: BMA.UIDrivers.ISVGPlot;
            private navigationDriver: BMA.UIDrivers.INavigationPanel;
            private variableEditor: BMA.UIDrivers.IVariableEditor;
            private containerEditor: BMA.UIDrivers.IContainerEditor;
            private exportservice: BMA.UIDrivers.IExportService;
            private svg: any;

            private xOrigin = 0;
            private yOrigin = 0;
            private xStep = 250;
            private yStep = 280;

            private variableIndex = 1;

            private stagingLine = undefined;
            private stagingGroup = undefined;
            private stagingVariable: { model: BMA.Model.Variable; layout: BMA.Model.VariableLayout; } = undefined;
            private stagingContainer: any = undefined;

            private editingId = undefined;

            private contextMenu: BMA.UIDrivers.IContextMenu;
            private contextElement;
            
            private variableEditedId = undefined;
            private prevVariablesOptions = undefined;
            private isContainerEdited: boolean;

            private clipboard: {
                Container: BMA.Model.ContainerLayout;
                Variables: {
                    m: BMA.Model.Variable;
                    l: BMA.Model.VariableLayout
                }[];
                Realtionships: BMA.Model.Relationship[]
            };

            constructor(appModel: BMA.Model.AppModel,
                undoRedoPresenter: BMA.Presenters.UndoRedoPresenter,
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                navigationDriver: BMA.UIDrivers.INavigationPanel,
                dragService: BMA.UIDrivers.IElementsPanel,
                variableEditorDriver: BMA.UIDrivers.IVariableEditor,
                containerEditorDriver: BMA.UIDrivers.IContainerEditor,
                contextMenu: BMA.UIDrivers.IContextMenu,
                exportservice: BMA.UIDrivers.IExportService,
                dragndropExtender: BMA.UIDrivers.IDragnDropExtender
            ) {

                var that = this;
                this.appModel = appModel;
                this.undoRedoPresenter = undoRedoPresenter;

                this.driver = svgPlotDriver;
                this.navigationDriver = navigationDriver;
                this.variableEditor = variableEditorDriver;
                this.containerEditor = containerEditorDriver;
                this.contextMenu = contextMenu;
                this.exportservice = exportservice;
                
                that.isContainerEdited = false;

                svgPlotDriver.SetGrid(this.xOrigin, this.yOrigin, this.xStep, this.yStep);

                window.Commands.On('SaveSVG', () => {
                    that.exportservice.Export(that.driver.GetSVG(), appModel.BioModel.Name, 'svg');
                });

                window.Commands.On("AddElementSelect", (type: string) => {

                    that.selectedType = type;
                    that.navigationDriver.TurnNavigation(type === undefined);
                    that.stagingLine = undefined;
                    //this.selectedType = this.selectedType === type ? undefined : type;
                    //this.driver.TurnNavigation(this.selectedType === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", (args: { x: number; y: number; screenX: number; screenY: number }) => {
                    if (that.selectedType !== undefined) {
                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor")) {
                            var id = that.GetVariableAtPosition(args.x, args.y);
                            if (id !== undefined) {
                                if (this.stagingLine === undefined) {
                                    this.stagingLine = {};
                                    this.stagingLine.id = id;
                                    this.stagingLine.x0 = args.x;
                                    this.stagingLine.y0 = args.y;
                                    return;
                                }
                                else {
                                    this.stagingLine.x1 = args.x;
                                    this.stagingLine.y1 = args.y;
                                    that.TryAddStagingLineAsLink();
                                    this.stagingLine = undefined;
                                    that.RefreshOutput();
                                    return;
                                }
                            }
                            else {
                                this.stagingLine = undefined;
                            }
                        }
                        else {
                            that.TryAddVariable(args.x, args.y, that.selectedType, undefined);
                        }
                    } else {
                        var id = that.GetVariableAtPosition(args.x, args.y);
                        if (id !== undefined) {

                            if (that.editingId == that.variableEditedId)
                                that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                            that.editingId = id;
                            that.variableEditor.Initialize(that.GetVariableById(that.undoRedoPresenter.Current.layout,
                                that.undoRedoPresenter.Current.model, id).model, that.undoRedoPresenter.Current.model, that.undoRedoPresenter.Current.layout);
                            that.variableEditor.Show(args.screenX, args.screenY);
                            window.Commands.Execute("DrawingSurfaceVariableEditorOpened", undefined);
                            //if (that.isVariableEdited) {
                            //    that.undoRedoPresenter.Dup(that.editingModel, appModel.Layout);
                            //    that.editingModel = undefined;
                            //    that.isVariableEdited = false;
                            //}
                            //that.RefreshOutput();
                        } else {
                            var cid = that.GetContainerAtPosition(args.x, args.y);
                            if (cid !== undefined) {
                                if (that.editingId == that.variableEditedId)
                                    that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                                that.editingId = cid;
                                that.containerEditor.Initialize(that.undoRedoPresenter.Current.layout.GetContainerById(cid));
                                that.containerEditor.Show(args.screenX, args.screenY);
                                window.Commands.Execute("DrawingSurfaceContainerEditorOpened", undefined);
                                //if (that.isVariableEdited) {
                                //    //TODO: update appModel threw undoredopresenter
                                //    that.undoRedoPresenter.Dup(that.editingModel, appModel.Layout);
                                //    that.editingModel = undefined;
                                //    that.isVariableEdited = false;
                                //}
                                //that.RefreshOutput();
                            }
                        }
                    }
                });

                //window.Commands.On("VariableEdited", () => {
                //    var that = this;
                //    if (that.editingId !== undefined) {
                //        var model = this.undoRedoPresenter.Current.model;//add editingmodel
                //        var layout = this.undoRedoPresenter.Current.layout;
                //        var variables = model.Variables;
                //        var editingVariableIndex = -1;
                //        for (var i = 0; i < variables.length; i++) {
                //            if (variables[i].Id === that.editingId) {
                //                editingVariableIndex = i;
                //                break;
                //            }
                //        }
                //        if (editingVariableIndex !== -1) {
                //            var params = that.variableEditor.GetVariableProperties();
                //            //model.SetVariableProperties(variables[i].Id, params.name, params.rangeFrom, params.rangeTo, params.formula);//to editingmodel
                //            var newVariables = [];
                //            var newVariablesLayout = [];
                //            var newRelations = [];
                //            for (var j = 0; j < model.Variables.length; j++) {
                //                if (model.Variables[j].Id === variables[i].Id) {
                //                    newVariables.push(new BMA.Model.Variable(
                //                        model.Variables[j].Id,
                //                        model.Variables[j].ContainerId,
                //                        model.Variables[j].Type,
                //                        params.name === undefined ? model.Variables[j].Name : params.name,
                //                        isNaN(params.rangeFrom) ? model.Variables[j].RangeFrom : params.rangeFrom,
                //                        isNaN(params.rangeTo) ? model.Variables[j].RangeTo : params.rangeTo,
                //                        params.formula === undefined ? model.Variables[j].Formula : params.formula)
                //                    );

                //                } else {
                //                    newVariables.push(new BMA.Model.Variable(
                //                        model.Variables[j].Id,
                //                        model.Variables[j].ContainerId,
                //                        model.Variables[j].Type,
                //                        model.Variables[j].Name,
                //                        model.Variables[j].RangeFrom,
                //                        model.Variables[j].RangeTo,
                //                        model.Variables[j].Formula)
                //                    );
                //                }
                //            }

                //            for (var j = 0; j < model.Relationships.length; j++) {
                //                newRelations.push(new BMA.Model.Relationship(
                //                    model.Relationships[j].Id,
                //                    model.Relationships[j].FromVariableId,
                //                    model.Relationships[j].ToVariableId,
                //                    model.Relationships[j].Type)
                //                );
                //            }

                //            for (var j = 0; j < layout.Variables.length; j++) {
                //                newVariablesLayout.push(new BMA.Model.VariableLayout(
                //                    layout.Variables[j].Id,
                //                    layout.Variables[j].PositionX,
                //                    layout.Variables[j].PositionY,
                //                    layout.Variables[j].CellX,
                //                    layout.Variables[j].CellY,
                //                    layout.Variables[j].Angle,
                //                    (j == editingVariableIndex && params.TFdescription !== undefined) ?
                //                        params.TFdescription : layout.Variables[j].TFDescription)
                //                );
                //            }

                //            if (!(model.Variables[editingVariableIndex].Name === newVariables[editingVariableIndex].Name
                //                && model.Variables[editingVariableIndex].RangeFrom === newVariables[editingVariableIndex].RangeFrom
                //                && model.Variables[editingVariableIndex].RangeTo === newVariables[editingVariableIndex].RangeTo
                //                && model.Variables[editingVariableIndex].Formula === newVariables[editingVariableIndex].Formula)) {
                //                that.editingModel = new BMA.Model.BioModel(model.Name, newVariables, newRelations);
                //                that.variableEditedId = that.editingId;
                //                that.isVariableEdited = true;
                //            }
                //            if (!(layout.Variables[editingVariableIndex].TFDescription == newVariablesLayout[editingVariableIndex].TFDescription)) {
                //                that.editingLayout = new BMA.Model.Layout(layout.Containers, newVariablesLayout);
                //                that.variableEditedId = that.editingId;
                //                that.isVariableEdited = true;
                //            }

                //            that.RefreshOutput(that.editingModel, that.editingLayout);
                //        }
                //    }
                //});

                window.Commands.On("ContainerNameEdited", () => {
                    var that = this;

                    if (that.editingId !== undefined) {
                        var layout = this.undoRedoPresenter.Current.layout;

                        var cnt = layout.GetContainerById(that.editingId);
                        if (cnt !== undefined) {

                            var variablesLayout = [];
                            var containersLayout = [];
                            for (var i = 0; i < layout.Variables.length; i++) {
                                variablesLayout.push(
                                    new BMA.Model.VariableLayout(
                                        layout.Variables[i].Id,
                                        layout.Variables[i].PositionX,
                                        layout.Variables[i].PositionY,
                                        layout.Variables[i].CellX,
                                        layout.Variables[i].CellY,
                                        layout.Variables[i].Angle,
                                        layout.Variables[i].TFDescription
                                    )
                                );
                            }

                            for (var i = 0; i < layout.Containers.length; i++) {
                                containersLayout.push(
                                    new BMA.Model.ContainerLayout(
                                        layout.Containers[i].Id,
                                        (layout.Containers[i].Id === that.editingId) ?
                                            that.containerEditor.GetContainerName() : layout.Containers[i].Name,
                                        layout.Containers[i].Size,
                                        layout.Containers[i].PositionX,
                                        layout.Containers[i].PositionY
                                    )
                                );
                            }

                            that.editingLayout = new BMA.Model.Layout(containersLayout, variablesLayout);
                            that.isContainerEdited = true;
                            that.RefreshOutput(undefined, that.editingLayout);
                        }
                    }
                });

                window.Commands.On("DrawingSurfaceContextMenuOpening", (args) => {
                    var x = that.driver.GetPlotX(args.left);
                    var y = that.driver.GetPlotY(args.top);

                    var id = that.GetVariableAtPosition(x, y);
                    var containerId = that.GetContainerAtPosition(x, y);
                    var relationshipId = that.GetRelationshipAtPosition(x, y, 3 * that.driver.GetPixelWidth());
                    var cntSize = containerId !== undefined ? that.undoRedoPresenter.Current.layout.GetContainerById(containerId).Size : undefined;

                    var showPaste = that.clipboard !== undefined;
                    if (showPaste === true) {

                        if (that.clipboard.Container !== undefined) {
                            showPaste = that.CanAddContainer(that.clipboard.Container.Id, x, y, that.clipboard.Container.Size);
                        } else {
                            var variable = that.clipboard.Variables[0];
                            showPaste = that.CanAddVariable(x, y, variable.m.Type, undefined);
                        }
                    }

                    var canPaste = true;
                    if (showPaste !== true && id === undefined && containerId === undefined && relationshipId === undefined) {
                        showPaste = true;
                        canPaste = false;
                    }

                    that.contextMenu.ShowMenuItems([
                        { name: "Cut", isVisible: id !== undefined || containerId !== undefined },
                        { name: "Copy", isVisible: id !== undefined || containerId !== undefined },
                        { name: "Paste", isVisible: showPaste },
                        { name: "Delete", isVisible: id !== undefined || containerId !== undefined || relationshipId !== undefined },
                        { name: "Size", isVisible: containerId !== undefined },
                        { name: "ResizeCellTo1x1", isVisible: true },
                        { name: "ResizeCellTo2x2", isVisible: true },
                        { name: "ResizeCellTo3x3", isVisible: true },
                        { name: "Edit", isVisible: id !== undefined || containerId !== undefined }
                    ]);

                    that.contextMenu.EnableMenuItems([
                        { name: "Paste", isEnabled: canPaste }
                    ]);

                    that.contextElement = { x: x, y: y, screenX: args.left, screenY: args.top };

                    if (id !== undefined) {
                        that.contextElement.id = id;
                        that.contextElement.type = "variable";
                    } else if (containerId !== undefined) {
                        that.contextElement.id = containerId;
                        that.contextElement.type = "container";
                    } else if (relationshipId !== undefined) {
                        that.contextElement.id = relationshipId;
                        that.contextElement.type = "relationship";
                    }

                });

                window.Commands.On("DrawingSurfaceDelete", (args) => {
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

                window.Commands.On("DrawingSurfaceCopy", (args) => {
                    that.CopyToClipboard(false);
                });

                window.Commands.On("DrawingSurfaceCut", (args) => {
                    that.CopyToClipboard(true);
                });

                window.Commands.On("DrawingSurfacePaste", (args) => {
                    if (that.clipboard !== undefined) {
                        if (that.clipboard.Container !== undefined) {
                            var model = that.undoRedoPresenter.Current.model;
                            var layout = that.undoRedoPresenter.Current.layout;
                            var idDic = {};
                            var clipboardContainer = that.clipboard.Container;
                            var variables = model.Variables.slice(0);
                            var variableLayouts = layout.Variables.slice(0);
                            var containerLayouts = layout.Containers.slice(0);
                            var relationships = model.Relationships.slice(0);

                            var newContainerId = that.variableIndex++;
                            var gridCell = that.GetGridCell(that.contextElement.x, that.contextElement.y);
                            containerLayouts.push(new BMA.Model.ContainerLayout(newContainerId, BMA.Model.GenerateNewContainerName(containerLayouts), clipboardContainer.Size, gridCell.x, gridCell.y));

                            var oldContainerOffset = {
                                x: clipboardContainer.PositionX * that.Grid.xStep + that.Grid.x0,
                                y: clipboardContainer.PositionY * that.Grid.yStep + that.Grid.y0,
                            };

                            var newContainerOffset = {
                                x: gridCell.x * that.Grid.xStep + that.Grid.x0,
                                y: gridCell.y * that.Grid.yStep + that.Grid.y0,
                            };

                            for (var i = 0; i < that.clipboard.Variables.length; i++) {
                                var variable = that.clipboard.Variables[i].m;
                                var variableLayout = that.clipboard.Variables[i].l;
                                idDic[variable.Id] = that.variableIndex;
                                var offsetX = variableLayout.PositionX - oldContainerOffset.x;
                                var offsetY = variableLayout.PositionY - oldContainerOffset.y;
                                variables.push(new BMA.Model.Variable(that.variableIndex, newContainerId, variable.Type, variable.Name, variable.RangeFrom, variable.RangeTo, variable.Formula));
                                variableLayouts.push(new BMA.Model.VariableLayout(that.variableIndex++, newContainerOffset.x + offsetX, newContainerOffset.y + offsetY, 0, 0, variableLayout.Angle, variableLayout.TFDescription));
                            }

                            for (var i = 0; i < that.clipboard.Realtionships.length; i++) {
                                var relationship = that.clipboard.Realtionships[i];
                                relationships.push(new BMA.Model.Relationship(that.variableIndex++, idDic[relationship.FromVariableId], idDic[relationship.ToVariableId], relationship.Type));
                            }

                            var newmodel = new BMA.Model.BioModel(model.Name, variables, relationships);
                            var newlayout = new BMA.Model.Layout(containerLayouts, variableLayouts);
                            that.undoRedoPresenter.Dup(newmodel, newlayout);

                        } else {
                            var variable = that.clipboard.Variables[0].m;
                            var variableLayout = that.clipboard.Variables[0].l;
                            var model = that.undoRedoPresenter.Current.model;
                            var layout = that.undoRedoPresenter.Current.layout;
                            var variables = model.Variables.slice(0);
                            var variableLayouts = layout.Variables.slice(0);
                            var gridCell = that.GetGridCell(that.contextElement.x, that.contextElement.y);
                            var container = that.GetContainerFromGridCell(gridCell);
                            variables.push(new BMA.Model.Variable(that.variableIndex, container && container.Id ? container.Id : 0, variable.Type, variable.Name, variable.RangeFrom, variable.RangeTo, variable.Formula));
                            variableLayouts.push(new BMA.Model.VariableLayout(that.variableIndex++, that.contextElement.x, that.contextElement.y, 0, 0, variableLayout.Angle, variableLayout.TFDescription));
                            var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                            var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                            that.undoRedoPresenter.Dup(newmodel, newlayout);
                        }
                    }

                    //that.clipboard = undefined;
                });

                window.Commands.On("DrawingSurfaceResizeCell", (args) => {
                    if (that.contextElement !== undefined && that.contextElement.type === "container") {
                        var resized = ModelHelper.ResizeContainer(undoRedoPresenter.Current.model, undoRedoPresenter.Current.layout, that.contextElement.id, args.size, { xOrigin: that.xOrigin, yOrigin: that.yOrigin, xStep: that.xStep, yStep: that.yStep });
                        this.undoRedoPresenter.Dup(resized.model, resized.layout);
                    }
                });

                window.Commands.On("DrawingSurfaceEdit", () => {
                    if (that.contextElement !== undefined && that.contextElement.type === "variable") {
                        var id = that.contextElement.id;
                        if (that.editingId == that.variableEditedId)
                            that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                        that.editingId = id;
                        that.variableEditor.Initialize(that.GetVariableById(that.undoRedoPresenter.Current.layout, that.undoRedoPresenter.Current.model, id).model,
                            that.undoRedoPresenter.Current.model, that.undoRedoPresenter.Current.layout);
                        that.variableEditor.Show(that.contextElement.screenX, that.contextElement.screenY);
                        window.Commands.Execute("DrawingSurfaceVariableEditorOpened", undefined);
                        //that.RefreshOutput();
                    } else if (that.contextElement !== undefined && that.contextElement.type === "container") {
                        var id = that.contextElement.id;
                        if (that.editingId == that.variableEditedId)
                            that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                        that.editingId = id;
                        that.containerEditor.Initialize(that.undoRedoPresenter.Current.layout.GetContainerById(id));
                        that.containerEditor.Show(that.contextElement.screenX, that.contextElement.screenY);
                        window.Commands.Execute("DrawingSurfaceContainerEditorOpened", undefined);
                        //that.RefreshOutput();
                    }

                    that.contextElement = undefined;
                });

                window.Commands.On("DrawingSurfaceRefreshOutput", (args) => {
                    if (this.undoRedoPresenter.Current !== undefined) {

                        if (args !== undefined) {
                            var bbox = BMA.ModelHelper.GetModelBoundingBox(this.undoRedoPresenter.Current.layout, { xOrigin: this.Grid.x0, yOrigin: this.Grid.y0, xStep: this.Grid.xStep, yStep: this.Grid.yStep });
                            var screenRect = { x: 0, y: 0, left: 0, top: 0, width: plotHost.host.width(), height: plotHost.host.height() };
                            var cs = new InteractiveDataDisplay.CoordinateTransform({ x: bbox.x, y: bbox.y, width: bbox.width, height: bbox.height }, screenRect, plotHost.aspectRatio);
                            var actualRect = cs.getPlotRect(screenRect);
                            bbox.width = actualRect.width;
                            bbox.height = actualRect.height;
                            var oldMaxWidth = window.PlotSettings.MaxWidth;
                            window.Commands.Execute('SetPlotSettings', { MaxWidth: Math.max(3200, bbox.width * 1.1) });

                            if (args.status === "Undo" || args.status === "Redo" || args.status === "Set") {
                                this.variableEditor.Hide();
                                this.variableEditedId = undefined;
                                this.prevVariablesOptions = undefined;
                                this.editingId = undefined;
                            }

                            if (args.status === "Set") {
                                this.ResetVariableIdIndex();
                                var center = this.GetLayoutCentralPoint();

                                this.driver.SetVisibleRect(bbox);
                                //this.navigationDriver.SetCenter(center.x, center.y);
                            } else {
                                if (oldMaxWidth > window.PlotSettings.MaxWidth) {
                                    this.driver.SetVisibleRect(bbox);
                                }
                            }
                        }

                        if (that.editingId !== undefined) {
                            var v = that.undoRedoPresenter.Current.model.GetVariableById(that.editingId);
                            if (v !== undefined) {
                                //if (that.editingId == that.variableEditedId)
                                //    that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                                that.variableEditor.Initialize(that.GetVariableById(that.undoRedoPresenter.Current.layout, that.undoRedoPresenter.Current.model, that.editingId).model,
                                    that.undoRedoPresenter.Current.model, that.undoRedoPresenter.Current.layout);
                            } else {
                                //if (that.editingId == that.variableEditedId)
                                //    that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                                that.containerEditor.Initialize(that.undoRedoPresenter.Current.layout.GetContainerById(that.editingId));
                            }
                        }

                        that.RefreshOutput();
                    }
                });

                window.Commands.On("ModelFitToView", (args) => {
                    if (this.undoRedoPresenter.Current !== undefined) {
                        var bbox = BMA.ModelHelper.GetModelBoundingBox(this.undoRedoPresenter.Current.layout, { xOrigin: this.Grid.x0, yOrigin: this.Grid.y0, xStep: this.Grid.xStep, yStep: this.Grid.yStep });

                        var screenRect = { x: 0, y: 0, left: 0, top: 0, width: plotHost.host.width(), height: plotHost.host.height() };
                        var cs = new InteractiveDataDisplay.CoordinateTransform({ x: bbox.x, y: bbox.y, width: bbox.width, height: bbox.height }, screenRect, plotHost.aspectRatio);
                        var actualRect = cs.getPlotRect(screenRect);
                        bbox.width = actualRect.width;
                        bbox.height = actualRect.height;

                        window.Commands.Execute('SetPlotSettings', { MaxWidth: Math.max(3200, bbox.width * 1.1) });
                        this.driver.SetVisibleRect(bbox);
                    }
                });

                window.Commands.On("DrawingSurfaceSetProofResults", (args) => {
                    if (this.svg !== undefined && this.undoRedoPresenter.Current !== undefined) {
                        var drawingSvg = <SVGElement>this.CreateSvg(args);
                        this.driver.Draw(drawingSvg);
                    }
                });

                window.Commands.On("DrawingSurfaceVariableEditorOpened", (args) => {
                    this.containerEditor.Hide();
                    if (that.variableEditedId !== undefined) {
                        that.VariableEdited();
                        that.editingModel = BMA.ModelHelper.UpdateFormulasAfterVariableChanged(that.variableEditedId,
                            that.undoRedoPresenter.Current.model, that.editingModel);
                        if (that.editingModel || that.editingLayout)
                            that.undoRedoPresenter.Dup(that.editingModel ? that.editingModel : appModel.BioModel,
                            that.editingLayout ? that.editingLayout : appModel.Layout);
                        //that.variableEditedId = undefined;
                        that.editingModel = undefined;
                    }
                    if (that.isContainerEdited) {
                        that.undoRedoPresenter.Dup(appModel.BioModel, that.editingLayout);
                        that.editingLayout = undefined;
                        that.isContainerEdited = false;
                    }
                    that.variableEditedId = that.editingId;
                    that.prevVariablesOptions = undefined;
                });

                window.Commands.On("DrawingSurfaceContainerEditorOpened", (args) => {
                    this.variableEditor.Hide();
                    if (that.variableEditedId !== undefined) {
                        that.VariableEdited();
                        that.editingModel = BMA.ModelHelper.UpdateFormulasAfterVariableChanged(that.variableEditedId,
                            that.undoRedoPresenter.Current.model, that.editingModel);
                        if (that.editingModel || that.editingLayout)
                            that.undoRedoPresenter.Dup(that.editingModel ? that.editingModel : appModel.BioModel,
                            that.editingLayout ? that.editingLayout : appModel.Layout);
                        that.variableEditedId = undefined;
                        that.prevVariablesOptions = undefined;
                        that.editingModel = undefined;
                    }
                    if (that.isContainerEdited) {
                        that.undoRedoPresenter.Dup(appModel.BioModel, that.editingLayout);
                        that.editingLayout = undefined;
                        that.isContainerEdited = false;
                    }
                });


                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: (svg) => {
                        this.svg = svg;
                        that.RefreshOutput();
                    }
                });

                var dragSubject = dragService.GetDragSubject();

                window.Commands.On("ZoomSliderChanged", (args) => {
                    if (args.isExternal !== true) {
                        var value = args.value * 24 + 800;
                        navigationDriver.SetZoom(value);
                    }
                });

                window.Commands.On("HighlightContent", (args) => {
                    if (this.svg !== undefined && this.undoRedoPresenter.Current !== undefined) {
                        var drawingSvg = <SVGElement>this.CreateSvg(args);
                        this.driver.Draw(drawingSvg);
                    }
                });

                window.Commands.On("UnhighlightContent", (args) => {
                    that.RefreshOutput();
                });

                var plotHost = (<any>this.navigationDriver.GetNavigationSurface()).master;
                svgPlotDriver.SetConstraintFunc((plotRect) => {

                    var screenRect = { x: 0, y: 0, left: 0, top: 0, width: plotHost.host.width(), height: plotHost.host.height() };
                    var minCS = new InteractiveDataDisplay.CoordinateTransform({ x: 0, y: 0, width: window.PlotSettings.MinWidth, height: 1 }, screenRect, plotHost.aspectRatio);
                    var actualMinRect = minCS.getPlotRect(screenRect);
                    var maxCS = new InteractiveDataDisplay.CoordinateTransform({ x: 0, y: 0, width: window.PlotSettings.MaxWidth, height: 1 }, screenRect, plotHost.aspectRatio);
                    var actualMaxRect = maxCS.getPlotRect(screenRect);

                    var resultPR = { x: 0, y: 0, width: 0, height: 0 };
                    var center = {
                        x: plotRect.x + plotRect.width / 2,
                        y: plotRect.y + plotRect.height / 2
                    }

                    if (plotRect.width < actualMinRect.width) {
                        resultPR.x = center.x - actualMinRect.width / 2;
                        resultPR.width = actualMinRect.width;
                    } else if (plotRect.width > actualMaxRect.width) {
                        resultPR.x = center.x - actualMaxRect.width / 2;
                        resultPR.width = actualMaxRect.width;
                    } else {
                        resultPR.x = plotRect.x;
                        resultPR.width = plotRect.width;
                    }

                    if (plotRect.height < actualMinRect.height) {
                        resultPR.y = center.y - actualMinRect.height / 2;
                        resultPR.height = actualMinRect.height;
                    } else if (plotRect.height > actualMaxRect.height) {
                        resultPR.y = center.y - actualMaxRect.height / 2;
                        resultPR.height = actualMaxRect.height;
                    } else {
                        resultPR.y = plotRect.y;
                        resultPR.height = plotRect.height;
                    }

                    return resultPR;
                });


                window.Commands.On("VisibleRectChanged", function (param) {
                    //if (param < window.PlotSettings.MinWidth) {
                    //    param = window.PlotSettings.MinWidth;
                    //    navigationDriver.SetZoom(param);
                    //}
                    //if (param > window.PlotSettings.MaxWidth) {
                    //    param = window.PlotSettings.MaxWidth;
                    //    navigationDriver.SetZoom(param);
                    //}
                    var zoom = (param - window.PlotSettings.MinWidth) / 24;
                    window.Commands.Execute("ZoomSliderBind", zoom);
                });

                window.Commands.On("AccordeonTabOpening", () => {
                    variableEditorDriver.Hide();
                });

                variableEditorDriver.SetOnClosingCallback(() => {
                    that.prevVariablesOptions = that.variableEditor.GetVariableProperties();
                    if (that.variableEditedId !== undefined) {
                        that.VariableEdited();
                        that.editingModel = BMA.ModelHelper.UpdateFormulasAfterVariableChanged(that.variableEditedId,
                            that.undoRedoPresenter.Current.model, that.editingModel);
                        if (that.editingModel || that.editingLayout)
                            that.undoRedoPresenter.Dup(that.editingModel ? that.editingModel : appModel.BioModel,
                            that.editingLayout ? that.editingLayout : appModel.Layout);
                        that.editingModel = undefined;
                        that.variableEditedId = undefined;
                    }
                });

                //variableEditorDriver.SetOnVariableEditedCallback(() => {
                //    //if (that.editingId !== undefined) {
                //    //    var model = that.undoRedoPresenter.Current.model;//add editingmodel
                //    //    var layout = that.undoRedoPresenter.Current.layout;
                //    //    var variables = model.Variables;
                //    //    var editingVariableIndex = -1;
                //    //    for (var i = 0; i < variables.length; i++) {
                //    //        if (variables[i].Id === that.editingId) {
                //    //            editingVariableIndex = i;
                //    //            break;
                //    //        }
                //    //    }
                //    //    if (editingVariableIndex !== -1) {
                //    //        var params = that.variableEditor.GetVariableProperties();
                //    //        //model.SetVariableProperties(variables[i].Id, params.name, params.rangeFrom, params.rangeTo, params.formula);//to editingmodel
                //    //        var newVariables = [];
                //    //        var newVariablesLayout = [];
                //    //        var newRelations = [];
                //    //        for (var j = 0; j < model.Variables.length; j++) {
                //    //            if (model.Variables[j].Id === variables[i].Id) {
                //    //                newVariables.push(new BMA.Model.Variable(
                //    //                    model.Variables[j].Id,
                //    //                    model.Variables[j].ContainerId,
                //    //                    model.Variables[j].Type,
                //    //                    params.name === undefined ? model.Variables[j].Name : params.name,
                //    //                    isNaN(params.rangeFrom) ? model.Variables[j].RangeFrom : params.rangeFrom,
                //    //                    isNaN(params.rangeTo) ? model.Variables[j].RangeTo : params.rangeTo,
                //    //                    params.formula === undefined ? model.Variables[j].Formula : params.formula)
                //    //                );

                //    //            } else {
                //    //                newVariables.push(new BMA.Model.Variable(
                //    //                    model.Variables[j].Id,
                //    //                    model.Variables[j].ContainerId,
                //    //                    model.Variables[j].Type,
                //    //                    model.Variables[j].Name,
                //    //                    model.Variables[j].RangeFrom,
                //    //                    model.Variables[j].RangeTo,
                //    //                    model.Variables[j].Formula)
                //    //                );
                //    //            }
                //    //        }

                //    //        for (var j = 0; j < model.Relationships.length; j++) {
                //    //            newRelations.push(new BMA.Model.Relationship(
                //    //                model.Relationships[j].Id,
                //    //                model.Relationships[j].FromVariableId,
                //    //                model.Relationships[j].ToVariableId,
                //    //                model.Relationships[j].Type)
                //    //            );
                //    //        }

                //    //        for (var j = 0; j < layout.Variables.length; j++) {
                //    //            newVariablesLayout.push(new BMA.Model.VariableLayout(
                //    //                layout.Variables[j].Id,
                //    //                layout.Variables[j].PositionX,
                //    //                layout.Variables[j].PositionY,
                //    //                layout.Variables[j].CellX,
                //    //                layout.Variables[j].CellY,
                //    //                layout.Variables[j].Angle,
                //    //                (j == editingVariableIndex && params.TFdescription !== undefined) ?
                //    //                    params.TFdescription : layout.Variables[j].TFDescription)
                //    //            );
                //    //        }

                //    //        if (!(model.Variables[editingVariableIndex].Name === newVariables[editingVariableIndex].Name
                //    //            && model.Variables[editingVariableIndex].RangeFrom === newVariables[editingVariableIndex].RangeFrom
                //    //            && model.Variables[editingVariableIndex].RangeTo === newVariables[editingVariableIndex].RangeTo
                //    //            && model.Variables[editingVariableIndex].Formula === newVariables[editingVariableIndex].Formula)) {
                //    //            that.editingModel = new BMA.Model.BioModel(model.Name, newVariables, newRelations);
                //    //            that.variableEditedId = that.editingId;
                //    //            that.isVariableEdited = true;
                //    //        }
                //    //        if (!(layout.Variables[editingVariableIndex].TFDescription == newVariablesLayout[editingVariableIndex].TFDescription)) {
                //    //            that.editingLayout = new BMA.Model.Layout(layout.Containers, newVariablesLayout);
                //    //            that.variableEditedId = that.editingId;
                //    //            that.isVariableEdited = true;
                //    //        }

                //    //        that.RefreshOutput(that.editingModel, that.editingLayout);
                //    //    }
                //    //}
                //});

                if (containerEditorDriver !== undefined) {
                    containerEditorDriver.SetOnClosingCallback(() => {
                        if (that.isContainerEdited) {
                            that.undoRedoPresenter.Dup(appModel.BioModel, that.editingLayout);
                            that.editingLayout = undefined;
                            that.isContainerEdited = false;
                        }
                    });
                }

                dragSubject.dragStart.subscribe(
                    (gesture) => {
                        navigationDriver.MoveDraggableOnTop();

                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor")) {
                            var id = that.GetVariableAtPosition(gesture.x, gesture.y);
                            if (id !== undefined) {
                                this.stagingLine = {};
                                this.stagingLine.id = id;
                                this.stagingLine.x0 = gesture.x;
                                this.stagingLine.y0 = gesture.y;
                                return;
                            }
                        } else if (that.selectedType === undefined) {
                            var id = this.GetVariableAtPosition(gesture.x, gesture.y);
                            var containerId = this.GetContainerAtPosition(gesture.x, gesture.y);
                            if (id !== undefined) {
                                that.navigationDriver.TurnNavigation(false);
                                var vl = that.GetVariableById(that.undoRedoPresenter.Current.layout, that.undoRedoPresenter.Current.model, id);
                                that.stagingVariable = { model: vl.model, layout: vl.layout };
                            } else if (containerId !== undefined) {
                                that.navigationDriver.TurnNavigation(false);
                                var cl = that.undoRedoPresenter.Current.layout.GetContainerById(containerId);
                                that.stagingContainer = { container: cl };
                            } else {
                                that.navigationDriver.TurnNavigation(true);
                            }
                        }
                        this.stagingLine = undefined;
                    });

                dragSubject.drag.subscribe(
                    (gesture) => {


                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && that.stagingLine !== undefined) {
                            this.stagingLine.x1 = gesture.x1;
                            this.stagingLine.y1 = gesture.y1;

                            //Redraw only svg for better performance
                            if (that.svg !== undefined) {
                                that.driver.DrawLayer2(<SVGElement>that.CreateStagingSvg());
                            }

                            return;
                        } else if (that.stagingVariable !== undefined) {
                            that.stagingVariable.layout = new BMA.Model.VariableLayout(that.stagingVariable.layout.Id, gesture.x1, gesture.y1, 0, 0, 0, that.stagingVariable.layout.TFDescription);

                            if (that.svg !== undefined) {
                                that.driver.DrawLayer2(<SVGElement>that.CreateStagingSvg());
                            }
                        } else if (this.stagingContainer !== undefined) {
                            that.stagingContainer.position = { x: gesture.x1, y: gesture.y1 };

                            if (that.svg !== undefined) {
                                that.driver.DrawLayer2(<SVGElement>that.CreateStagingSvg());
                            }
                        }

                    });

                dragSubject.dragEnd.subscribe(
                    (gesture) => {
                        navigationDriver.MoveDraggableOnBottom();

                        that.driver.DrawLayer2(undefined);

                        if ((that.selectedType === "Activator" || that.selectedType === "Inhibitor") && that.stagingLine !== undefined && that.stagingLine.x1 !== undefined) {
                            that.TryAddStagingLineAsLink();
                            that.stagingLine = undefined;
                            that.RefreshOutput();
                        }

                        if (that.stagingVariable !== undefined) {
                            var x = that.stagingVariable.layout.PositionX;
                            var y = that.stagingVariable.layout.PositionY;
                            var type = that.stagingVariable.model.Type;
                            var id = that.stagingVariable.model.Id;
                            that.stagingVariable = undefined;

                            //var top = svgPlotDriver.GetTop(-gesture.y);
                            //var left = svgPlotDriver.GetLeft(gesture.x);

                            if (dragndropExtender === undefined || !dragndropExtender.HandleDrop({ x: gesture.pageX, y: gesture.pageY }, { type: "variable", id: id })) {
                                if (!that.TryAddVariable(x, y, type, id)) {
                                    that.RefreshOutput();
                                }
                            } else {
                                that.RefreshOutput();
                            }
                        }

                        if (that.stagingContainer !== undefined) {
                            var cx = that.stagingContainer.position.x - that.stagingContainer.container.Size * that.Grid.xStep / 3;
                            var cy = that.stagingContainer.position.y - that.stagingContainer.container.Size * that.Grid.yStep / 3;
                            
                            var cid = that.stagingContainer.container.Id;
                            that.stagingContainer = undefined;
                            if (!that.TryAddVariable(cx, cy, "Container", cid)) {
                                that.RefreshOutput();
                            }
                        }
                    });
            }

            private RefreshOutput(model: BMA.Model.BioModel = undefined, layout: BMA.Model.Layout = undefined) {
                if (this.svg !== undefined && this.undoRedoPresenter.Current !== undefined) {
                    var drawingSvg = <SVGElement>this.CreateSvg(undefined, model, layout);
                    this.driver.Draw(drawingSvg);
                }
            }

            private CopyToClipboard(remove: boolean) {
                var that = this;
                if (that.contextElement !== undefined) {
                    that.clipboard = ModelHelper.CreateClipboardContent(that.undoRedoPresenter.Current.model, that.undoRedoPresenter.Current.layout, that.contextElement);
                    if (remove) {
                        if (that.contextElement.type === "variable") {
                            that.RemoveVariable(that.contextElement.id);
                        } else if (that.contextElement.type === "container") {
                            that.RemoveContainer(that.contextElement.id);
                        }
                    }
                    that.contextElement = undefined;
                }
            }

            private GetLayoutCentralPoint(): { x: number; y: number } {
                var layout = this.undoRedoPresenter.Current.layout;
                var model = this.undoRedoPresenter.Current.model;

                var result = { x: 0, y: 0 };
                var count = 0;

                var containers = layout.Containers;

                for (var i = 0; i < containers.length; i++) {
                    result.x += containers[i].PositionX;
                    result.y += containers[i].PositionY;
                    count++;
                }

                var variables = layout.Variables;
                var gridCells = [];

                var existGS = function (gridCell) {
                    for (var i = 0; i < gridCells.length; i++) {
                        if (gridCell.x === gridCells[i].x && gridCell.y === gridCells[i].y) {
                            return true;
                        }
                    }
                    return false;
                }

                for (var i = 0; i < variables.length; i++) {
                    if (model.Variables[i].Type === "Constant") {
                        var gridCell = this.GetGridCell(variables[i].PositionX, variables[i].PositionY);
                        if (!existGS(gridCell)) {
                            gridCells.push(gridCell);
                            result.x += gridCell.x;
                            result.y += gridCell.y;
                            count++;
                        }
                    }
                }

                if (count > 0) {
                    result.x = (result.x / count + 0.5) * this.xStep + this.xOrigin;
                    result.y = -(result.y / count + 0.5) * this.yStep + this.yOrigin;
                }

                return result;
            }


            private GetCurrentSVG(svg): any {
                return $(svg.toSVG()).children();
            }

            private RemoveVariable(id: number) {
                if (this.editingId === id) {
                    this.editingId = undefined;
                }

                var wasRemoved = false;

                var model = this.undoRedoPresenter.Current.model;
                var layout = this.undoRedoPresenter.Current.layout;

                var variables = this.undoRedoPresenter.Current.model.Variables;
                var variableLayouts = this.undoRedoPresenter.Current.layout.Variables;

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

                var relationships = this.undoRedoPresenter.Current.model.Relationships;

                var newRels = [];

                for (var i = 0; i < relationships.length; i++) {
                    if (relationships[i].FromVariableId !== id &&
                        relationships[i].ToVariableId !== id) {
                        newRels.push(relationships[i]);
                    }
                }


                if (wasRemoved === true) {
                    var newmodel = new BMA.Model.BioModel(model.Name, newVars, newRels);
                    newmodel = BMA.ModelHelper.UpdateFormulasAfterVariableChanged(id, model, newmodel);
                    var newlayout = new BMA.Model.Layout(layout.Containers, newVarLs);
                    this.undoRedoPresenter.Dup(newmodel, newlayout);
                }
            }

            private RemoveContainer(id: number) {
                if (this.editingId === id) {
                    this.editingId = undefined;
                }

                var wasRemoved = false;

                var model = this.undoRedoPresenter.Current.model;
                var layout = this.undoRedoPresenter.Current.layout;

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
                            if (this.editingId === variables[i].Id) {
                                this.editingId = undefined;
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
                    for (var i = 0; i < removed.length; i++) {
                        newmodel = BMA.ModelHelper.UpdateFormulasAfterVariableChanged(removed[i], model, newmodel);
                    }
                    var newlayout = new BMA.Model.Layout(newCnt, newVL);
                    this.undoRedoPresenter.Dup(newmodel, newlayout);
                }
            }

            private RemoveRelationship(id: number) {
                var wasRemoved = false;

                var model = this.undoRedoPresenter.Current.model;
                var layout = this.undoRedoPresenter.Current.layout;

                var relationships = this.undoRedoPresenter.Current.model.Relationships;

                var newRels = [];
                var fromId = undefined;
                var toId = undefined;

                for (var i = 0; i < relationships.length; i++) {
                    if (relationships[i].Id !== id) {
                        newRels.push(relationships[i]);
                    } else {
                        wasRemoved = true;
                        //
                        fromId = relationships[i].FromVariableId;
                        toId = relationships[i].ToVariableId
                    }
                }

                if (wasRemoved === true) {
                    //updating formula
                    var fromVariable = model.GetVariableById(fromId);
                    var newVars = [];
                    for (var i = 0; i < model.Variables.length; i++) {
                        var oldFormula = model.Variables[i].Formula;
                        var newFormula = undefined;
                        if (model.Variables[i].Id == toId) {
                            newFormula = oldFormula.replace(new RegExp("var\\(" + fromVariable.Name + "\\)", 'g'), "");
                        }
                        newVars.push(new BMA.Model.Variable(
                            model.Variables[i].Id,
                            model.Variables[i].ContainerId,
                            model.Variables[i].Type,
                            model.Variables[i].Name,
                            model.Variables[i].RangeFrom,
                            model.Variables[i].RangeTo,
                            newFormula === undefined ? oldFormula : newFormula)
                        );
                    }

                    var newmodel = new BMA.Model.BioModel(model.Name, newVars, newRels);
                    var newlayout = new BMA.Model.Layout(layout.Containers, layout.Variables);
                    this.undoRedoPresenter.Dup(newmodel, newlayout);
                }
            }

            private GetVariableAtPosition(x: number, y: number): number {
                var variables = this.undoRedoPresenter.Current.model.Variables;
                var variableLayouts = this.undoRedoPresenter.Current.layout.Variables;
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

            private GetContainerAtPosition(x: number, y: number): number {
                var containers = this.undoRedoPresenter.Current.layout.Containers;
                var element = <BMA.Elements.BorderContainerElement>window.ElementRegistry.GetElementByType("Container");
                var grid = this.Grid;
                for (var i = 0; i < containers.length; i++) {
                    var containerLayout = containers[i];
                    if (element.IntersectsBorder(x, y, (containerLayout.PositionX + 0.5) * grid.xStep + grid.x0, (containerLayout.PositionY + 0.5) * grid.yStep + grid.y0, { Size: containerLayout.Size, xStep: grid.xStep / 2, yStep: grid.yStep / 2 })) {
                        return containerLayout.Id;
                    }
                }

                return undefined;
            }

            private GetRelationshipAtPosition(x: number, y: number, pixelWidth: number): number {
                var relationships = this.undoRedoPresenter.Current.model.Relationships;
                var layout = this.undoRedoPresenter.Current.layout;

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
                var variables = this.undoRedoPresenter.Current.model.Variables;
                var variableLayouts = this.undoRedoPresenter.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    if (element.Contains(this.stagingLine.x1, this.stagingLine.y1, variableLayout.PositionX, variableLayout.PositionY)) {

                        var current = this.undoRedoPresenter.Current;
                        var model = current.model;
                        var layout = current.layout;
                        var relationships = model.Relationships.slice(0);

                        var containsSameRelationship = false;
                        for (var j = 0; j < model.Relationships.length; j++) {
                            var rel = model.Relationships[j];
                            if (rel.FromVariableId === this.stagingLine.id && rel.ToVariableId === variable.Id) {
                                containsSameRelationship = true;
                                break;
                            }
                        }

                        if (!containsSameRelationship) {
                            relationships.push(new BMA.Model.Relationship(this.variableIndex++, this.stagingLine.id, variable.Id, this.selectedType));
                            var newmodel = new BMA.Model.BioModel(model.Name, model.Variables, relationships);
                            this.undoRedoPresenter.Dup(newmodel, layout);
                        }

                        return;
                    }
                }
            }

            private CanAddContainer(id: number, x: number, y: number, size: number): boolean {
                var that = this;
                var gridCell = that.GetGridCell(x, y);

                for (var i = 0; i < size; i++) {
                    for (var j = 0; j < size; j++) {
                        var cellForCheck = { x: gridCell.x + i, y: gridCell.y + j };
                        var cnt = that.GetContainerFromGridCell(cellForCheck);
                        var checkCell = (cnt === undefined || cnt.Id === id) && that.GetConstantsFromGridCell(cellForCheck).length === 0;
                        if (checkCell !== true)
                            return false;
                    }
                }

                return true;
            }

            private CanAddVariable(x: number, y: number, type: string, id: number): boolean {
                var that = this;
                var gridCell = that.GetGridCell(x, y);
                var variables = that.undoRedoPresenter.Current.model.Variables.slice(0);
                var variableLayouts = that.undoRedoPresenter.Current.layout.Variables.slice(0);

                switch (type) {
                    case "Constant":
                        var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("Constant")).GetBoundingBox(x, y);
                        var canAdd = that.GetContainerFromGridCell(gridCell) === undefined && that.Contains(gridCell, bbox);

                        if (canAdd === true) {
                            for (var i = 0; i < variableLayouts.length; i++) {
                                var variable = variables[i];

                                if (id !== undefined && id === variable.Id)
                                    continue;

                                var variableLayout = variableLayouts[i];
                                var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                                if (this.Intersects(bbox, elementBBox))
                                    return false;
                            }
                        }

                        return canAdd;

                    case "Default":
                        var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("Default")).GetBoundingBox(x, y);
                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

                        if (container === undefined ||
                            !(<BMA.Elements.BorderContainerElement>window.ElementRegistry.GetElementByType("Container"))
                                .ContainsBBox(bbox, (container.PositionX + 0.5) * that.xStep, (container.PositionY + 0.5) * that.yStep, { Size: container.Size, xStep: that.Grid.xStep / 2, yStep: that.Grid.yStep / 2 })) {
                            return false;
                        }

                        for (var i = 0; i < variableLayouts.length; i++) {
                            var variable = variables[i];

                            if (id !== undefined && id === variable.Id)
                                continue;

                            var variableLayout = variableLayouts[i];
                            var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                            if (that.Intersects(bbox, elementBBox))
                                return false;
                        }

                        return true;

                    case "MembraneReceptor":
                        var bbox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType("MembraneReceptor")).GetBoundingBox(x, y);
                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

                        if (container === undefined ||
                            !(<BMA.Elements.BorderContainerElement>window.ElementRegistry.GetElementByType("Container"))
                                .IntersectsBorder(x, y, (container.PositionX + 0.5) * that.xStep, (container.PositionY + 0.5) * that.yStep, { Size: container.Size, xStep: that.Grid.xStep / 2, yStep: that.Grid.yStep / 2 })) {
                            return false;
                        }

                        for (var i = 0; i < variableLayouts.length; i++) {
                            var variable = variables[i];

                            if (id !== undefined && id === variable.Id)
                                continue;

                            var variableLayout = variableLayouts[i];
                            var elementBBox = (<BMA.Elements.BboxElement>window.ElementRegistry.GetElementByType(variable.Type)).GetBoundingBox(variableLayout.PositionX, variableLayout.PositionY);
                            if (that.Intersects(bbox, elementBBox))
                                return false;
                        }

                        return true;
                }

                throw "Unknown Variable type";
            }

            private TryAddVariable(x: number, y: number, type: string, id: number): boolean {
                var that = this;
                var current = that.undoRedoPresenter.Current;
                var model = current.model;
                var layout = current.layout;


                switch (type) {
                    case "Container":
                        var containerLayouts = layout.Containers.slice(0);
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        var gridCell = that.GetGridCell(x, y);
                        var container = layout.GetContainerById(id);

                        if (that.CanAddContainer(id, x, y, container === undefined ? 1 : container.Size) === true) {

                            if (id !== undefined) {
                                for (var i = 0; i < containerLayouts.length; i++) {
                                    if (containerLayouts[i].Id === id) {

                                        var oldContainerOffset = {
                                            x: containerLayouts[i].PositionX * that.Grid.xStep + that.Grid.x0,
                                            y: containerLayouts[i].PositionY * that.Grid.yStep + that.Grid.y0,
                                        };

                                        containerLayouts[i] = new BMA.Model.ContainerLayout(id, containerLayouts[i].Name, containerLayouts[i].Size, gridCell.x, gridCell.y);

                                        var newContainerOffset = {
                                            x: gridCell.x * that.Grid.xStep + that.Grid.x0,
                                            y: gridCell.y * that.Grid.yStep + that.Grid.y0,
                                        };

                                        for (var j = 0; j < variableLayouts.length; j++) {
                                            if (variables[j].ContainerId === id) {
                                                var vlX = variableLayouts[j].PositionX;
                                                var vlY = variableLayouts[j].PositionY;

                                                variableLayouts[j] = new BMA.Model.VariableLayout(variableLayouts[j].Id,
                                                    vlX - oldContainerOffset.x + newContainerOffset.x,
                                                    vlY - oldContainerOffset.y + newContainerOffset.y, 0, 0, variableLayouts[j].Angle, variableLayouts[j].TFDescription);
                                            }
                                        }
                                    }
                                }
                            } else {
                                containerLayouts.push(new BMA.Model.ContainerLayout(that.variableIndex++, BMA.Model.GenerateNewContainerName(containerLayouts), 1, gridCell.x, gridCell.y));
                            }

                            var newmodel = new BMA.Model.BioModel(model.Name, model.Variables, model.Relationships);
                            var newlayout = new BMA.Model.Layout(containerLayouts, variableLayouts);
                            that.undoRedoPresenter.Dup(newmodel, newlayout);
                            return true;
                        }

                        break;
                    case "Constant":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        if (that.CanAddVariable(x, y, "Constant", id) !== true)
                            return false;

                        if (id !== undefined) {
                            for (var i = 0; i < variables.length; i++) {
                                if (variables[i].Id === id) {
                                    variableLayouts[i] = new BMA.Model.VariableLayout(id, x, y, 0, 0, 0, variableLayouts[i].TFDescription);
                                }
                            }
                        } else {
                            variables.push(new BMA.Model.Variable(this.variableIndex, 0, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VariableLayout(this.variableIndex++, x, y, 0, 0, 0));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.undoRedoPresenter.Dup(newmodel, newlayout);
                        return true;
                    case "Default":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        if (that.CanAddVariable(x, y, "Default", id) !== true)
                            return false;

                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

                        if (id !== undefined) {
                            for (var i = 0; i < variables.length; i++) {
                                if (variables[i].Id === id) {
                                    var vrbl = variables[i];
                                    if (vrbl.ContainerId !== container.Id) {
                                        variables[i] = new BMA.Model.Variable(vrbl.Id, container.Id, vrbl.Type, vrbl.Name, vrbl.RangeFrom, vrbl.RangeTo, vrbl.Formula);
                                    }
                                    variableLayouts[i] = new BMA.Model.VariableLayout(id, x, y, 0, 0, 0, variableLayouts[i].TFDescription);
                                }
                            }
                        } else {
                            variables.push(new BMA.Model.Variable(this.variableIndex, container.Id, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VariableLayout(this.variableIndex++, x, y, 0, 0, 0));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.undoRedoPresenter.Dup(newmodel, newlayout);
                        return true;
                    case "MembraneReceptor":
                        var variables = model.Variables.slice(0);
                        var variableLayouts = layout.Variables.slice(0);

                        if (that.CanAddVariable(x, y, "MembraneReceptor", id) !== true)
                            return false;

                        var gridCell = that.GetGridCell(x, y);
                        var container = that.GetContainerFromGridCell(gridCell);

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
                                    var vrbl = variables[i];
                                    if (vrbl.ContainerId !== container.Id) {
                                        variables[i] = new BMA.Model.Variable(vrbl.Id, container.Id, vrbl.Type, vrbl.Name, vrbl.RangeFrom, vrbl.RangeTo, vrbl.Formula);
                                    }
                                    variableLayouts[i] = new BMA.Model.VariableLayout(id, x, y, 0, 0, angle, variableLayouts[i].TFDescription);
                                }
                            }
                        } else {
                            var pos = SVGHelper.GeEllipsePoint(containerX + 2.5 * container.Size, containerY, 107 * container.Size, 127 * container.Size, x, y);
                            variables.push(new BMA.Model.Variable(this.variableIndex, container.Id, type, "", 0, 1, ""));
                            variableLayouts.push(new BMA.Model.VariableLayout(this.variableIndex++, pos.x, pos.y, 0, 0, angle));
                        }

                        var newmodel = new BMA.Model.BioModel(model.Name, variables, model.Relationships);
                        var newlayout = new BMA.Model.Layout(layout.Containers, variableLayouts);
                        that.undoRedoPresenter.Dup(newmodel, newlayout);
                        return true;
                }

                return false;
            }

            private GetGridCell(x: number, y: number): { x: number; y: number } {
                var cellX = Math.ceil((x - this.xOrigin) / this.xStep) - 1;
                var cellY = Math.ceil((y - this.yOrigin) / this.yStep) - 1;
                return { x: cellX, y: cellY };
            }

            private GetContainerFromGridCell(gridCell: { x: number; y: number }): BMA.Model.ContainerLayout {
                var current = this.undoRedoPresenter.Current;

                var layouts = current.layout.Containers;
                for (var i = 0; i < layouts.length; i++) {
                    if (layouts[i].PositionX <= gridCell.x && layouts[i].PositionX + layouts[i].Size > gridCell.x &&
                        layouts[i].PositionY <= gridCell.y && layouts[i].PositionY + layouts[i].Size > gridCell.y) {
                        return layouts[i];
                    }
                }

                return undefined;
            }

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

            private GetConstantsFromGridCell(gridCell: { x: number; y: number }): { container: BMA.Model.Variable; layout: BMA.Model.VariableLayout }[] {
                var result = [];
                var variables = this.undoRedoPresenter.Current.model.Variables;
                var variableLayouts = this.undoRedoPresenter.Current.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    if (variable.Type !== "Constant")
                        continue;

                    var vGridCell = this.GetGridCell(variableLayout.PositionX, variableLayout.PositionY);

                    if (gridCell.x === vGridCell.x && gridCell.y === vGridCell.y) {
                        result.push({ variable: variable, variableLayout: variableLayout });
                    }
                }
                return result;
            }

            private ResetVariableIdIndex() {
                this.variableIndex = 1;

                var m = this.undoRedoPresenter.Current.model;
                var l = this.undoRedoPresenter.Current.layout;

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
            }


            private get Grid(): { x0: number; y0: number; xStep: number; yStep: number } {
                return { x0: this.xOrigin, y0: this.yOrigin, xStep: this.xStep, yStep: this.yStep };
            }

            private GetVariableById(layout: BMA.Model.Layout, model: BMA.Model.BioModel, id: number): { model: BMA.Model.Variable; layout: BMA.Model.VariableLayout } {
                var variableLayouts = layout.Variables;
                var variables = model.Variables;
                for (var i = 0; i < variableLayouts.length; i++) {
                    var variableLayout = variableLayouts[i];
                    if (variableLayout.Id === id) {
                        return { model: variables[i], layout: variableLayout };
                    }
                }

                throw "No such variable in model";
            }

            private GetVariableColorByStatus(status): string {
                if (status)
                    return "green";//"#D9FFB3";
                else
                    return "red";
            }

            private GetContainerColorByStatus(status): string {
                if (status)
                    return "#E9FFCC";
                else
                    return "#FFDDDB";
            }

            private GetItemById(arr, id) {
                for (var i = 0; i < arr.length; i++) {
                    if (arr[i].id === id)
                        return arr[i];
                }

                return undefined;
            }

            private CreateSvg(args: any, model: BMA.Model.BioModel = undefined, layout: BMA.Model.Layout = undefined): any {
                if (this.svg === undefined)
                    return undefined;
                if (model === undefined)
                    model = this.undoRedoPresenter.Current.model;
                if (layout === undefined)
                    layout = this.undoRedoPresenter.Current.layout;
                //Generating svg elements from model and layout
                var svgElements = [];

                var containerLayouts = layout.Containers;//this.undoRedoPresenter.Current.layout.Containers;
                for (var i = 0; i < containerLayouts.length; i++) {
                    var containerLayout = containerLayouts[i];
                    var element = window.ElementRegistry.GetElementByType("Container");

                    var isHighlighted = undefined;
                    if (args !== undefined && args.containerHighlightIds !== undefined) {
                        isHighlighted = false;
                        for (var j = 0; j < args.containerHighlightIds.length; j++) {
                            if (containerLayout.Id === args.containerHighlightIds[j]) {
                                isHighlighted = true;
                                break;
                            }
                        }
                    }

                    svgElements.push(element.RenderToSvg({
                        layout: containerLayout,
                        grid: this.Grid,
                        background: args === undefined || args.containersStability === undefined ? undefined : this.GetContainerColorByStatus(args.containersStability[containerLayout.Id]),
                        isHighlighted: isHighlighted
                    }));
                }

                var variables = model.Variables;//this.undoRedoPresenter.Current.model.Variables;
                var variableLayouts = layout.Variables;//this.undoRedoPresenter.Current.layout.Variables;

                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];
                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    var additionalInfo = args === undefined || args.variablesStability === undefined ? undefined : this.GetItemById(args.variablesStability, variable.Id);

                    var isHighlighted = undefined;
                    if (args !== undefined && args.variableHighlightIds !== undefined) {
                        isHighlighted = false;
                        for (var j = 0; j < args.variableHighlightIds.length; j++) {
                            if (variable.Id === args.variableHighlightIds[j]) {
                                isHighlighted = true;
                                break;
                            }
                        }
                        if (!isHighlighted) {
                            for (var j = 0; j < args.containerHighlightIds.length; j++) {
                                if (variable.ContainerId === args.containerHighlightIds[j]) {
                                    isHighlighted = true;
                                    break;
                                }
                            }
                        }
                    }

                    var container: any = variable.Type === "MembraneReceptor" ? /*this.undoRedoPresenter.Current.layout*/layout.GetContainerById(variable.ContainerId) : undefined;
                    var sizeCoef = undefined;
                    var gridCell = undefined;
                    if (container !== undefined) {
                        sizeCoef = container.Size;
                        gridCell = { x: container.PositionX, y: container.PositionY };
                    }
                    svgElements.push(element.RenderToSvg({
                        model: variable,
                        layout: variableLayout,
                        grid: this.Grid,
                        gridCell: gridCell,
                        sizeCoef: sizeCoef,
                        valueText: additionalInfo === undefined ? undefined : additionalInfo.range,
                        labelColor: additionalInfo === undefined ? undefined : this.GetVariableColorByStatus(additionalInfo.state),
                        isHighlighted: isHighlighted
                    }));
                }

                var relationships = model.Relationships;//this.undoRedoPresenter.Current.model.Relationships;
                for (var i = 0; i < relationships.length; i++) {
                    var relationship = relationships[i];
                    var element = window.ElementRegistry.GetElementByType(relationship.Type);

                    var start = this.GetVariableById(layout/*this.undoRedoPresenter.Current.layout*/, model/*this.undoRedoPresenter.Current.model*/, relationship.FromVariableId).layout;
                    var end = this.GetVariableById(layout/*this.undoRedoPresenter.Current.layout*/, model/*this.undoRedoPresenter.Current.model*/, relationship.ToVariableId).layout;

                    svgElements.push(element.RenderToSvg({
                        layout: { start: start, end: end },
                        grid: this.Grid
                    }));
                }



                //constructing final svg image
                this.svg.clear();
                var defs = this.svg.defs("bmaDefs");
                var activatorMarker = this.svg.marker(defs, "Activator", 4, 0, 8, 4, "auto", { viewBox: "0 -2 4 4" });
                this.svg.polyline(activatorMarker, [[0, 2], [4, 0], [0, -2]], { fill: "none", stroke: "#808080", strokeWidth: "1px" });
                var inhibitorMarker = this.svg.marker(defs, "Inhibitor", 0, 0, 2, 6, "auto", { viewBox: "0 -3 2 6" });
                this.svg.line(inhibitorMarker, 0, 3, 0, -3, { fill: "none", stroke: "#808080", strokeWidth: "2px" });

                for (var i = 0; i < svgElements.length; i++) {
                    this.svg.add(svgElements[i]);
                }

                return $(this.svg.toSVG()).children();
            }

            private CreateStagingSvg(): any {
                if (this.svg === undefined)
                    return undefined;

                this.svg.clear();
                var defs = this.svg.defs("bmaDefs");
                var activatorMarker = this.svg.marker(defs, "Activator", 4, 0, 8, 4, "auto", { viewBox: "0 -2 4 4" });
                this.svg.polyline(activatorMarker, [[0, 2], [4, 0], [0, -2]], { fill: "none", stroke: "#808080", strokeWidth: "1px" });
                var inhibitorMarker = this.svg.marker(defs, "Inhibitor", 0, 0, 2, 6, "auto", { viewBox: "0 -3 2 6" });
                this.svg.line(inhibitorMarker, 0, 3, 0, -3, { fill: "none", stroke: "#808080", strokeWidth: "2px" });

                if (this.stagingLine !== undefined) {
                    this.svg.line(
                        this.stagingLine.x0,
                        this.stagingLine.y0,
                        this.stagingLine.x1,
                        this.stagingLine.y1,
                        {
                            stroke: "#808080",
                            strokeWidth: 2,
                            fill: "#808080",
                            "marker-end": "url(#" + this.selectedType + ")",
                            id: "stagingLine"
                        });
                }

                if (this.stagingVariable !== undefined) {
                    var element = window.ElementRegistry.GetElementByType(this.stagingVariable.model.Type);
                    this.svg.add(element.RenderToSvg({ model: this.stagingVariable.model, layout: this.stagingVariable.layout, grid: this.Grid }));
                }

                if (this.stagingContainer !== undefined) {
                    var element = window.ElementRegistry.GetElementByType("Container");

                    var x = (this.stagingContainer.container.PositionX + 0.5) * this.Grid.xStep + (this.stagingContainer.container.Size - 1) * this.Grid.xStep / 2;
                    var y = (this.stagingContainer.container.PositionY + 0.5) * this.Grid.yStep + (this.stagingContainer.container.Size - 1) * this.Grid.yStep / 2;

                    this.svg.add(element.RenderToSvg({
                        layout: this.stagingContainer.container,
                        grid: this.Grid,
                        background: "none",
                        translate: {
                            x: this.stagingContainer.position.x - x,
                            y: this.stagingContainer.position.y - y
                        }
                    }));
                }

                return $(this.svg.toSVG()).children();
            }

            private VariableEdited() {
                var that = this;
                if (that.variableEditedId !== undefined) {
                        var model = that.undoRedoPresenter.Current.model;//add editingmodel
                        var layout = that.undoRedoPresenter.Current.layout;
                        var variables = model.Variables;
                        var editingVariableIndex = -1;
                        for (var i = 0; i < variables.length; i++) {
                            if (variables[i].Id === that.variableEditedId) {
                                editingVariableIndex = i;
                                break;
                            }
                        }
                        if (editingVariableIndex !== -1) {
                            var params = that.prevVariablesOptions;
                            //model.SetVariableProperties(variables[i].Id, params.name, params.rangeFrom, params.rangeTo, params.formula);//to editingmodel
                            var newVariables = [];
                            var newVariablesLayout = [];
                            var newRelations = [];
                            for (var j = 0; j < model.Variables.length; j++) {
                                if (model.Variables[j].Id === variables[i].Id) {
                                    newVariables.push(new BMA.Model.Variable(
                                        model.Variables[j].Id,
                                        model.Variables[j].ContainerId,
                                        model.Variables[j].Type,
                                        params.name === undefined ? model.Variables[j].Name : params.name,
                                        isNaN(params.rangeFrom) ? model.Variables[j].RangeFrom : params.rangeFrom,
                                        isNaN(params.rangeTo) ? model.Variables[j].RangeTo : params.rangeTo,
                                        params.formula === undefined ? model.Variables[j].Formula : params.formula)
                                    );

                                } else {
                                    newVariables.push(new BMA.Model.Variable(
                                        model.Variables[j].Id,
                                        model.Variables[j].ContainerId,
                                        model.Variables[j].Type,
                                        model.Variables[j].Name,
                                        model.Variables[j].RangeFrom,
                                        model.Variables[j].RangeTo,
                                        model.Variables[j].Formula)
                                    );
                                }
                            }

                            for (var j = 0; j < model.Relationships.length; j++) {
                                newRelations.push(new BMA.Model.Relationship(
                                    model.Relationships[j].Id,
                                    model.Relationships[j].FromVariableId,
                                    model.Relationships[j].ToVariableId,
                                    model.Relationships[j].Type)
                                );
                            }

                            for (var j = 0; j < layout.Variables.length; j++) {
                                newVariablesLayout.push(new BMA.Model.VariableLayout(
                                    layout.Variables[j].Id,
                                    layout.Variables[j].PositionX,
                                    layout.Variables[j].PositionY,
                                    layout.Variables[j].CellX,
                                    layout.Variables[j].CellY,
                                    layout.Variables[j].Angle,
                                    (j == editingVariableIndex && params.TFdescription !== undefined) ?
                                        params.TFdescription : layout.Variables[j].TFDescription)
                                );
                            }

                            if (!(model.Variables[editingVariableIndex].Name === newVariables[editingVariableIndex].Name
                                && model.Variables[editingVariableIndex].RangeFrom === newVariables[editingVariableIndex].RangeFrom
                                && model.Variables[editingVariableIndex].RangeTo === newVariables[editingVariableIndex].RangeTo
                                && model.Variables[editingVariableIndex].Formula === newVariables[editingVariableIndex].Formula)) {
                                that.editingModel = new BMA.Model.BioModel(model.Name, newVariables, newRelations);
                                //that.variableEditedId = that.editingId;
                                //that.isVariableEdited = true;
                            }
                            if (!(layout.Variables[editingVariableIndex].TFDescription == newVariablesLayout[editingVariableIndex].TFDescription)) {
                                that.editingLayout = new BMA.Model.Layout(layout.Containers, newVariablesLayout);
                                //that.variableEditedId = that.editingId;
                                //that.isVariableEdited = true;
                            }

                            that.RefreshOutput(that.editingModel, that.editingLayout);
                        }
                    }
            }
        }
    }
} 