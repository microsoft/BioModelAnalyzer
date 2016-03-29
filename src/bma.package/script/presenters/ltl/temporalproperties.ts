/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\model\biomodel.ts"/>
/// <reference path="..\..\model\model.ts"/>
/// <reference path="..\..\uidrivers\commoninterfaces.ts"/>
/// <reference path="..\..\uidrivers\ltlinterfaces.ts"/>
/// <reference path="..\..\model\operation.ts"/>
/// <reference path="..\..\commands.ts"/>

module BMA {
    export module LTL {
        export class TemporalPropertiesPresenter {
            private operations: BMA.LTLOperations.OperationLayout[];
            private keyframes: BMA.LTLOperations.Keyframe[];
            private activeOperation: BMA.LTLOperations.Operation;
            private stagingOperation: {
                operation: BMA.LTLOperations.OperationLayout;
                originRef: BMA.LTLOperations.OperationLayout;
                originIndex: number;
                isRoot: boolean;
                parentoperation: BMA.LTLOperations.Operation;
                parentoperationindex: number;
                fromclipboard: boolean;
            };
            private elementToAdd: { type: string; name: string };

            private tpEditorDriver: BMA.UIDrivers.ITemporalPropertiesEditor;
            private driver: BMA.UIDrivers.ISVGPlot;
            private navigationDriver: BMA.UIDrivers.INavigationPanel;
            private dragService: BMA.UIDrivers.IElementsPanel;

            private operatorRegistry: BMA.LTLOperations.OperatorsRegistry;
            private previousHighlightedOperation: BMA.LTLOperations.OperationLayout;

            private clipboard: any;
            private contextElement: any;

            private commands: BMA.CommandRegistry;

            private controlPanelPadding = 3;

            private appModel: BMA.Model.AppModel;
            private ajax: BMA.UIDrivers.IServiceDriver;

            private ltlcompactviewfactory: BMA.UIDrivers.ILTLResultsViewerFactory;
            private isUpdateControlRequested = false;

            private statesPresenter: BMA.LTL.StatesPresenter;

            private zoomConstraints = {
                minWidth: 100,
                maxWidth: 1000
            };

            private states = [];

            private isInitialized = false;

            private plotConstraints = {
                minWidth: 400,
                minHeight: 200,
                maxWidth: Number.POSITIVE_INFINITY,
                maxHeight: Number.POSITIVE_INFINITY
            };

            private log: ISessionLog;

            constructor(
                commands: BMA.CommandRegistry,
                appModel: BMA.Model.AppModel,
                ajax: BMA.UIDrivers.IServiceDriver,
                tpEditorDriver: BMA.UIDrivers.ITemporalPropertiesEditor,
                statesPresenter: BMA.LTL.StatesPresenter,
                logService: ISessionLog) {

                var that = this;
                this.appModel = appModel;
                this.ajax = ajax;
                this.tpEditorDriver = tpEditorDriver;
                this.driver = tpEditorDriver.GetSVGDriver();
                this.navigationDriver = tpEditorDriver.GetNavigationDriver();
                this.dragService = tpEditorDriver.GetDragService();
                this.commands = commands;
                this.statesPresenter = statesPresenter;
                this.ltlcompactviewfactory = new BMA.UIDrivers.LTLResultsViewerFactory();

                this.operatorRegistry = new BMA.LTLOperations.OperatorsRegistry();
                this.operations = [];

                this.log = logService;

                var contextMenu = tpEditorDriver.GetContextMenuDriver();

                tpEditorDriver.SetCopyZoneVisibility(false);
                tpEditorDriver.SetDeleteZoneVisibility(false);

                var plotHost = (<any>this.navigationDriver.GetNavigationSurface()).master;
                tpEditorDriver.GetSVGDriver().SetConstraintFunc((plotRect) => {

                    var screenRect = { x: 0, y: 0, left: 0, top: 0, width: plotHost.host.width(), height: plotHost.host.height() };
                    var minCS = new InteractiveDataDisplay.CoordinateTransform({ x: 0, y: 0, width: that.plotConstraints.minWidth, height: that.plotConstraints.minHeight }, screenRect, plotHost.aspectRatio);
                    var actualMinRect = minCS.getPlotRect(screenRect);
                    var maxCS = new InteractiveDataDisplay.CoordinateTransform({ x: 0, y: 0, width: that.plotConstraints.maxWidth, height: that.plotConstraints.maxHeight }, screenRect, plotHost.aspectRatio);
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


                commands.On("AddOperatorSelect", (operatorName: string) => {
                    that.elementToAdd = { type: "operator", name: operatorName };
                });

                commands.On("AddStateSelect", (stateName: string) => {
                    that.elementToAdd = { type: "state", name: stateName };
                });

                commands.On("DrawingSurfaceDrop", (args: { x: number; y: number; screenX: number; screenY: number }) => {
                    if (that.elementToAdd !== undefined) {
                        var position = { x: args.x, y: args.y };
                        var operation = that.GetOperationAtPoint(args.x, args.y);
                        var emptyCell = undefined;
                        if (operation !== undefined) {
                            emptyCell = operation.GetEmptySlotAtPosition(position.x, position.y);
                        }

                        if (that.elementToAdd.type === "operator") {
                            var registry = this.operatorRegistry;

                            var op = new BMA.LTLOperations.Operation();
                            op.Operator = registry.GetOperatorByName(that.elementToAdd.name);
                            op.Operands = op.Operator.OperandsCount > 1 ? [undefined, undefined] : [undefined];


                            if (operation !== undefined) {
                                if (emptyCell !== undefined) {
                                    emptyCell.operation.Operands[emptyCell.operandIndex] = op;
                                    operation.Refresh();
                                }
                            } else {
                                var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), op, position);
                                if (that.HasIntersections(operationLayout)) {
                                    operationLayout.IsVisible = false;
                                } else {
                                    that.InitializeOperationTag(operationLayout);
                                    that.operations.push(operationLayout);
                                }
                            }
                        } else if (that.elementToAdd.type === "state") {
                            var state = statesPresenter.GetStateByName(that.elementToAdd.name);
                            if (operation !== undefined && emptyCell !== undefined) {
                                emptyCell.operation.Operands[emptyCell.operandIndex] = state;
                                operation.Refresh();
                            }
                        }

                        this.OnOperationsChanged();
                    }
                });

                commands.On("DrawingSurfaceClick", (args) => {
                    for (var i = 0; i < this.operations.length; i++) {
                        if (this.operations[i].Tag !== undefined && this.operations[i].Tag.driver !== undefined) {
                            this.operations[i].Tag.driver.Collapse();
                        }
                    }
                    (<any>this.navigationDriver.GetNavigationSurface()).updateLayout();
                });

                commands.On("TemporalPropertiesEditorContextMenuOpening", (args) => {
                    var x = that.driver.GetPlotX(args.left);
                    var y = that.driver.GetPlotY(args.top);

                    this.contextElement = {
                        x: x,
                        y: y
                    };

                    //that.driver.GetLightSVGRef().rect(x - 5, y - 5, 10, 10, { stroke: "red", fill: "transparent" });

                    var canPaste = this.clipboard !== undefined;

                    var stagingOp = this.GetOperationAtPoint(x, y);
                    if (stagingOp !== undefined) {
                        var emptyCell = stagingOp.GetEmptySlotAtPosition(x, y);

                        this.contextElement.operationlayoutref = stagingOp;
                        this.contextElement.emptyslot = emptyCell;

                        contextMenu.ShowMenuItems([
                            { name: "Cut", isVisible: true },
                            { name: "Copy", isVisible: true },
                            { name: "Paste", isVisible: emptyCell !== undefined },
                            { name: "Delete", isVisible: true },
                            { name: "Export", isVisible: true },
                            { name: "Import", isVisible: false },
                        ]);

                        contextMenu.EnableMenuItems([
                            { name: "Cut", isEnabled: emptyCell === undefined },
                            { name: "Copy", isEnabled: emptyCell === undefined },
                            { name: "Delete", isEnabled: emptyCell === undefined },
                            { name: "Paste", isEnabled: canPaste },
                            { name: "Export", isEnabled: true },

                        ]);

                    } else {

                        if (this.clipboard !== undefined) {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), this.clipboard.operation, { x: x, y: y });
                            canPaste = !this.HasIntersections(operationLayout) && this.clipboard.operation.operator !== undefined;
                            operationLayout.IsVisible = false;
                        }

                        contextMenu.ShowMenuItems([
                            { name: "Cut", isVisible: false },
                            { name: "Copy", isVisible: false },
                            { name: "Paste", isVisible: true },
                            { name: "Delete", isVisible: false },
                            { name: "Export", isVisible: false },
                            { name: "Import", isVisible: true },
                        ]);

                        contextMenu.EnableMenuItems([
                            { name: "Paste", isEnabled: canPaste }
                        ]);
                    }
                });

                commands.On("TemporalPropertiesEditorExport", (args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        var operationDescr = this.contextElement.operationlayoutref.PickOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = operationDescr !== undefined ? operationDescr.operation.Clone() : undefined;
                        commands.Execute("ExportLTLFormula", { operation: clonned });
                    }
                });

                commands.On("TemporalPropertiesEditorImport", (args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        commands.Execute("ImportLTLFormula", {
                            position: { x: this.contextElement.x, y: this.contextElement.y }
                        });
                    }
                });

                commands.On("TemporalPropertiesEditorCut", (args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        this.ResetOperation(this.contextElement.operationlayoutref);
                        //this.contextElement.operationlayoutref.AnalysisStatus = "nottested";

                        var unpinned = this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = unpinned.operation !== undefined ? unpinned.operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned,
                        };
                        this.tpEditorDriver.SetCopyZoneVisibility(true);
                        this.tpEditorDriver.SetCopyZoneIcon(clonned);

                        if (unpinned.isRoot) {
                            this.ClearOperationTag(this.contextElement.operationlayoutref, true);
                            this.operations.splice(this.operations.indexOf(this.contextElement.operationlayoutref), 1);
                            this.contextElement.operationlayoutref.IsVisible = false;
                        }

                        tpEditorDriver.SetCopyZoneVisibility(this.clipboard !== undefined);
                        this.OnOperationsChanged();
                    }
                });

                commands.On("TemporalPropertiesEditorCopy", (args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        var operationDescr = this.contextElement.operationlayoutref.PickOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = operationDescr !== undefined ? operationDescr.operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned
                        };
                        this.tpEditorDriver.SetCopyZoneVisibility(true);
                        this.tpEditorDriver.SetCopyZoneIcon(clonned);

                        tpEditorDriver.SetCopyZoneVisibility(this.clipboard !== undefined);
                    }
                });

                commands.On("TemporalPropertiesEditorPaste", (args: { top: number; left: number }) => {
                    var x = this.contextElement.x;
                    var y = this.contextElement.y;

                    if (this.clipboard !== undefined) {
                        var op = this.clipboard.operation.Clone();

                        if (this.contextElement.emptyslot !== undefined) {
                            var emptyCell = this.contextElement.emptyslot;
                            emptyCell.operation.Operands[emptyCell.operandIndex] = op;
                            this.contextElement.operationlayoutref.Refresh();
                        } else {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), op, { x: x, y: y });
                            that.InitializeOperationTag(operationLayout);
                            this.operations.push(operationLayout);
                        }

                        this.OnOperationsChanged();
                    }
                });

                commands.On("TemporalPropertiesEditorDelete", (args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        this.ResetOperation(this.contextElement.operationlayoutref);
                        //this.contextElement.operationlayoutref.AnalysisStatus = "nottested";
                        //this.ClearOperationTag(this.contextElement.operationlayoutref);

                        var op = this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        if (op.isRoot) {
                            this.ClearOperationTag(this.contextElement.operationlayoutref, true);
                            var ind = this.operations.indexOf(this.contextElement.operationlayoutref);
                            this.contextElement.operationlayoutref.IsVisible = false;
                            this.operations.splice(ind, 1);
                        }
                        this.OnOperationsChanged();
                    }
                });

                commands.On("KeyframesChanged", (args: { states: BMA.LTLOperations.Keyframe[] }) => {
                    if (this.CompareStatesToLocal(args.states)) {
                        this.states = args.states;
                        tpEditorDriver.SetStates(args.states);
                        for (var i = 0; i < this.operations.length; i++) {
                            var op = this.operations[i];
                            op.RefreshStates(args.states);
                        }

                        this.FitToView();
                        this.OnOperationsChanged(true, true);
                        that.isUpdateControlRequested = true;
                        this.RunAllQueries();
                    }
                });

                commands.On("TemporalPropertiesEditorExpanded", (args) => {

                    for (var i = 0; i < that.operations.length; i++) {
                        that.operations[i].Refresh();
                    }

                    //if (that.isUpdateControlRequested) {
                    that.UpdateControlPanels();
                    that.isUpdateControlRequested = false;
                    //}
                });

                window.Commands.On("ModelReset", (args) => {
                    for (var i = 0; i < this.operations.length; i++) {
                        this.operations[i].IsVisible = false;
                        this.ClearOperationTag(this.operations[i], true);
                    }
                    this.operations = [];
                    this.LoadFromAppModel();
                    this.clipboard = undefined;
                    this.tpEditorDriver.SetCopyZoneIcon(undefined);
                    this.tpEditorDriver.SetCopyZoneVisibility(false);
                });

                window.Commands.On("AppModelChanged", (args) => {
                    if (this.CompareStatesToLocal(appModel.States) || args.isMajorChange) {
                        this.states = appModel.States;
                        tpEditorDriver.SetStates(appModel.States);
                        for (var i = 0; i < this.operations.length; i++) {
                            var op = this.operations[i];
                            op.RefreshStates(appModel.States);
                            if (args.isMajorChange) {
                                this.ResetOperation(op);
                                //op.AnalysisStatus = "nottested";
                            }
                        }

                        this.FitToView();
                        this.OnOperationsChanged(true, false);
                        that.isUpdateControlRequested = true;
                        this.RunAllQueries();
                    }
                });

                tpEditorDriver.SetFitToViewCallback(() => {
                    that.FitToView();
                });

                this.InitializeDragndrop();
                this.CreateSvgHeaders();
                this.LoadFromAppModel();
            }

            private InitializeDragndrop() {
                if (!this.isInitialized) {

                    var that = this;
                    var tpEditorDriver = that.tpEditorDriver;

                    that.dragService.GetMouseMoves().subscribe(
                        (gesture) => {
                            if (that.previousHighlightedOperation !== undefined) {
                                that.previousHighlightedOperation.Refresh();
                                that.previousHighlightedOperation = undefined;
                            }

                            var staginOp = that.GetOperationAtPoint(gesture.x, gesture.y);
                            if (staginOp !== undefined) {
                                staginOp.HighlightAtPosition(gesture.x, gesture.y);
                                that.previousHighlightedOperation = staginOp;
                            }

                            if (this.clipboard !== undefined) {
                                var copyZoneBB = tpEditorDriver.GetCopyZoneBBox();
                                if (that.Intersects(gesture, copyZoneBB)) {
                                    tpEditorDriver.HighlightCopyZone(true);
                                } else {
                                    tpEditorDriver.HighlightCopyZone(false);
                                }
                            }
                        });


                    var dragSubject = that.dragService.GetDragSubject();
                    dragSubject.dragStart.subscribe(
                        (gesture) => {
                            var copyZoneBbox = that.tpEditorDriver.GetCopyZoneBBox();
                            if (that.clipboard !== undefined && that.Intersects(gesture, copyZoneBbox)) {
                                that.navigationDriver.TurnNavigation(false);
                                this.stagingOperation = {
                                    operation: new BMA.LTLOperations.OperationLayout(that.driver.GetLightSVGRef(), this.clipboard.operation, gesture),
                                    originRef: undefined,
                                    originIndex: undefined,
                                    isRoot: undefined,
                                    parentoperation: undefined,
                                    parentoperationindex: undefined,
                                    fromclipboard: true
                                };

                            } else {
                                var staginOp = this.GetOperationAtPoint(gesture.x, gesture.y);
                                if (staginOp !== undefined) {
                                    //if (staginOp.AnalysisStatus !== "processing") {
                                    //staginOp.AnalysisStatus = "nottested";
                                    //staginOp.Tag = undefined;
                                    //}

                                    that.navigationDriver.MoveDraggableOnTop();
                                    that.navigationDriver.TurnNavigation(false);


                                    //Can't drag parts of processing operations
                                    var picked = staginOp.PickOperation(gesture.x, gesture.y);
                                    if (staginOp.AnalysisStatus === "processing" && picked !== undefined && !picked.isRoot) {
                                        this.stagingOperation = undefined;
                                    } else {
                                        if (!picked.isRoot) {
                                            this.ResetOperation(staginOp);
                                            //staginOp.AnalysisStatus = "nottested";
                                            //this.ClearOperationTag(staginOp);
                                            //staginOp.Tag = undefined;
                                        }

                                        tpEditorDriver.SetCopyZoneVisibility(true);
                                        tpEditorDriver.SetDeleteZoneVisibility(true);

                                        var unpinned = staginOp.UnpinOperation(gesture.x, gesture.y);
                                        this.stagingOperation = {
                                            operation: new BMA.LTLOperations.OperationLayout(that.driver.GetLightSVGRef(), unpinned.operation, gesture),
                                            originRef: staginOp,
                                            originIndex: this.operations.indexOf(staginOp),
                                            isRoot: unpinned.isRoot,
                                            parentoperation: unpinned.parentoperation,
                                            parentoperationindex: unpinned.parentoperationindex,
                                            fromclipboard: false
                                        };

                                        if (staginOp.Tag !== undefined && staginOp.Tag.dommarker !== undefined) {
                                            staginOp.Tag.dommarker.hide();
                                        }

                                        staginOp.IsVisible = !unpinned.isRoot;
                                    }
                                }
                            }
                        });

                    dragSubject.drag.subscribe(


                        (gesture) => {
                            if (this.stagingOperation !== undefined) {
                                var bbox = this.stagingOperation.operation.BoundingBox;
                                this.stagingOperation.operation.Position = { x: <number>gesture.x1 + this.stagingOperation.operation.Scale.x * bbox.width / 2, y: <number>gesture.y1 + this.stagingOperation.operation.Scale.y * bbox.height / 2 };

                                var copyZoneBB = tpEditorDriver.GetCopyZoneBBox();
                                var deleteZoneBB = tpEditorDriver.GetDeleteZoneBBox();
                                var position = {
                                    x: gesture.x1,
                                    y: gesture.y1
                                };

                                if (this.Intersects(position, copyZoneBB)) {
                                    tpEditorDriver.HighlightCopyZone(true);
                                    tpEditorDriver.HighlightDeleteZone(false);
                                } else if (this.Intersects(position, deleteZoneBB)) {
                                    tpEditorDriver.HighlightCopyZone(false);
                                    tpEditorDriver.HighlightDeleteZone(true);
                                } else {
                                    tpEditorDriver.HighlightCopyZone(false);
                                    tpEditorDriver.HighlightDeleteZone(false);
                                }


                            }
                        });

                    dragSubject.dragEnd.subscribe(
                        (gesture) => {
                            that.navigationDriver.MoveDraggableOnBottom();

                            if (this.stagingOperation !== undefined) {
                                that.navigationDriver.TurnNavigation(true);
                                this.stagingOperation.operation.IsVisible = false;

                                var bbox = this.stagingOperation.operation.BoundingBox;
                                var position = {
                                    x: this.stagingOperation.operation.Position.x - this.stagingOperation.operation.Scale.x * bbox.width / 2,
                                    y: this.stagingOperation.operation.Position.y - this.stagingOperation.operation.Scale.y * bbox.height / 2,
                                };
                                this.stagingOperation.operation.Position = {
                                    x: position.x + bbox.width / 2,
                                    y: position.y + bbox.height / 2
                                };

                                tpEditorDriver.HighlightCopyZone(false);
                                tpEditorDriver.HighlightDeleteZone(false);
                                var copyZoneBB = tpEditorDriver.GetCopyZoneBBox();
                                var deleteZoneBB = tpEditorDriver.GetDeleteZoneBBox();
                                if (that.Intersects(position, copyZoneBB) && !this.stagingOperation.fromclipboard) {

                                    this.clipboard = {
                                        operation: this.stagingOperation.operation.Operation.Clone()
                                    };
                                    this.tpEditorDriver.SetCopyZoneIcon(this.clipboard.operation);

                                    //Operation should stay in its origin place
                                    if (this.stagingOperation.isRoot) {
                                        this.stagingOperation.originRef.IsVisible = true;
                                    } else {
                                        this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                        this.stagingOperation.originRef.Refresh();
                                    }


                                } else if (that.Intersects(position, deleteZoneBB) && !this.stagingOperation.fromclipboard) {

                                    if (this.stagingOperation.isRoot) {
                                        this.ClearOperationTag(this.stagingOperation.originRef, true);
                                        this.operations.splice(this.stagingOperation.originIndex, 1);
                                    }

                                } else {


                                    if (!this.HasIntersections(this.stagingOperation.operation)) {
                                        if (this.stagingOperation.fromclipboard) {
                                            if (this.stagingOperation.operation.IsOperation) {
                                                var newOp = new BMA.LTLOperations.OperationLayout(
                                                    that.driver.GetSVGRef(),
                                                    this.stagingOperation.operation.Operation.Clone(),
                                                    this.stagingOperation.operation.Position);
                                                that.InitializeOperationTag(newOp);
                                                this.operations.push(newOp);
                                            }
                                        } else {
                                            if (this.stagingOperation.operation.IsOperation) {
                                                if (this.stagingOperation.isRoot) {
                                                    this.stagingOperation.originRef.Position = this.stagingOperation.operation.Position;
                                                    this.stagingOperation.originRef.IsVisible = true;
                                                } else {
                                                    var newOp = new BMA.LTLOperations.OperationLayout(
                                                        that.driver.GetSVGRef(),
                                                        this.stagingOperation.operation.Operation,
                                                        this.stagingOperation.operation.Position);
                                                    that.InitializeOperationTag(newOp);
                                                    this.operations.push(newOp);
                                                }

                                            } else {
                                                //State should state in its origin place
                                                this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                                this.stagingOperation.originRef.Refresh();
                                            }
                                        }
                                    } else {
                                        var operation = this.GetOperationAtPoint(position.x, position.y);
                                        if (operation !== undefined) {
                                            if (operation.AnalysisStatus === "processing") {
                                                if (!this.stagingOperation.fromclipboard) {
                                                    //Operation should stay in its origin place bacuse editing of processing operations is not allowed
                                                    if (this.stagingOperation.isRoot) {
                                                        this.stagingOperation.originRef.IsVisible = true;
                                                    } else {
                                                        this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                                        this.stagingOperation.originRef.Refresh();
                                                    }
                                                }
                                            } else {

                                                var emptyCell = undefined;
                                                emptyCell = operation.GetEmptySlotAtPosition(position.x, position.y);
                                                if (emptyCell !== undefined) {
                                                    //emptyCell.opLayout = operation;
                                                    emptyCell.operation.Operands[emptyCell.operandIndex] = this.stagingOperation.operation.Operation.Clone();
                                                    operation.Refresh();
                                                    this.ResetOperation(operation);
                                                    //operation.AnalysisStatus = "nottested";
                                                    //this.ClearOperationTag(operation);
                                                    //operation.Tag = undefined;

                                                    if (this.stagingOperation.isRoot) {
                                                        this.ClearOperationTag(this.stagingOperation.originRef, true);
                                                        this.operations[this.stagingOperation.originIndex].IsVisible = false;
                                                        this.operations.splice(this.stagingOperation.originIndex, 1);

                                                    }
                                                } else {
                                                    if (!this.stagingOperation.fromclipboard) {
                                                        //Operation should stay in its origin place
                                                        if (this.stagingOperation.isRoot) {
                                                            this.stagingOperation.originRef.IsVisible = true;
                                                        } else {
                                                            this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                                            this.stagingOperation.originRef.Refresh();
                                                        }
                                                    }
                                                }
                                            }
                                        } else {
                                            if (!this.stagingOperation.fromclipboard) {
                                                //Operation should stay in its origin place
                                                if (this.stagingOperation.isRoot) {
                                                    this.stagingOperation.originRef.IsVisible = true;
                                                } else {
                                                    this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                                    this.stagingOperation.originRef.Refresh();
                                                }
                                            }
                                        }
                                    }
                                }

                                this.stagingOperation.operation.IsVisible = false;
                                this.stagingOperation = undefined;

                                tpEditorDriver.SetCopyZoneVisibility(this.clipboard !== undefined);
                                tpEditorDriver.SetDeleteZoneVisibility(false);

                                this.OnOperationsChanged();

                            }
                        });


                    this.isInitialized = true;
                }
            }

            private ResetOperation(operation: BMA.LTLOperations.OperationLayout) {
                operation.AnalysisStatus = "nottested";
                if (operation.Tag !== undefined && operation.Tag.driver !== undefined) {
                    operation.Tag.driver.SetStatus("nottested", undefined);
                }
            }

            private CreateSvgHeaders() {
                var svg = this.driver.GetSVGRef();
                var defs = svg.defs("ltlBmaDefs");

                var pattern = svg.pattern(defs, "pattern-stripe", 0, 0, 8, 4, {
                    patternUnits: "userSpaceOnUse",
                    patternTransform: "rotate(45)"
                });
                svg.rect(pattern, 0, 0, 4, 4, {
                    transform: "translate(0,0)",
                    fill: "white"
                });

                var mask = svg.mask(defs, "mask-stripe");
                svg.rect(mask, "-50%", "-50%", "100%", "100%", {
                    fill: "url(#pattern-stripe)"
                });
            }

            private CompareStatesToLocal(states: BMA.LTLOperations.Keyframe[]) {
                if (states.length !== this.states.length)
                    return true;
                else {
                    for (var i = 0; i < states.length; i++) {
                        var st = states[i];
                        var appst = this.states[i];
                        if (st.Name !== appst.Name || st.GetFormula() !== appst.GetFormula() || st.Description !== appst.Description)
                            return true;
                    }

                    return false;
                }
            }

            private FitToView() {
                if (this.operations.length < 1)
                    this.driver.SetVisibleRect({ x: 0, y: 0, width: 800, height: 600 });
                else {
                    var bbox = this.CalcOperationsBBox();

                    var size = Math.max(bbox.width, bbox.height);
                    var center = {
                        x: bbox.x + bbox.width / 2,
                        y: bbox.y + bbox.height / 2
                    };

                    if (size < this.zoomConstraints.minWidth)
                        this.zoomConstraints.minWidth = size;
                    else if (size > this.zoomConstraints.maxWidth)
                        this.zoomConstraints.maxWidth = size;

                    bbox = {
                        x: center.x - size / 2,
                        y: center.y - size / 2,
                        width: size,
                        height: size
                    }
                    this.driver.SetVisibleRect(bbox);
                }
            }

            private CalcOperationsBBox() {
                if (this.operations.length < 1)
                    return undefined;

                var bbox = this.operations[0].BoundingBox;
                for (var i = 1; i < this.operations.length; i++) {
                    var unitBbbox = this.operations[i].BoundingBox;
                    var x = Math.min(bbox.x, unitBbbox.x);
                    var y = Math.min(bbox.y, unitBbbox.y);
                    bbox = {
                        x: x,
                        y: y,
                        width: Math.max(bbox.x + bbox.width, unitBbbox.x + unitBbbox.width) - x,
                        height: Math.max(bbox.y + bbox.height, unitBbbox.y + unitBbbox.height) - y
                    };
                }

                return bbox;
            }

            private LoadFromAppModel() {

                var appModel = this.appModel;
                var height = 0;
                var padding = 5;

                var checkAppearance = appModel.OperationAppearances !== undefined && appModel.OperationAppearances.length > 0 && appModel.OperationAppearances.length === appModel.Operations.length;

                if (appModel.Operations !== undefined && appModel.Operations.length > 0) {
                    for (var i = 0; i < appModel.Operations.length; i++) {
                        var position = { x: 0, y: 0 };
                        if (checkAppearance) {
                            var opAppearance = appModel.OperationAppearances[i];
                            if (opAppearance.x !== undefined) {
                                position.x = opAppearance.x;
                            }
                            if (opAppearance.y !== undefined) {
                                position.y = opAppearance.y;
                            }
                        }

                        var newOp = new BMA.LTLOperations.OperationLayout(this.driver.GetSVGRef(), appModel.Operations[i], position);
                        this.InitializeOperationTag(newOp);

                        if (!checkAppearance) {
                            height += newOp.BoundingBox.height / 2 + padding;
                            newOp.Position = { x: 0, y: height };
                            height += newOp.BoundingBox.height / 2 + padding;
                        }

                        this.operations.push(newOp);
                    }
                }

                this.states = appModel.States;
                this.tpEditorDriver.SetStates(appModel.States);
                for (var i = 0; i < this.operations.length; i++) {
                    var op = this.operations[i];
                    op.RefreshStates(appModel.States);
                }

                this.FitToView();
                this.OnOperationsChanged(true, false);
                this.RunAllQueries();
            }

            private GetOperationAtPoint(x: number, y: number) {
                var that = this;
                var operations = this.operations;
                for (var i = 0; i < operations.length; i++) {
                    if (!operations[i].IsVisible)
                        continue;

                    var bbox = operations[i].BoundingBox;

                    if (this.Intersects({ x: x, y: y }, bbox)) {
                        return operations[i];
                    }
                }

                return undefined;
            }

            private HasIntersections(operation: BMA.LTLOperations.OperationLayout): boolean {
                var that = this;
                var operations = this.operations;
                var opBbox = operation.BoundingBox;
                for (var i = 0; i < operations.length; i++) {
                    if (!operations[i].IsVisible)
                        continue;

                    var bbox = operations[i].BoundingBox;

                    if (this.HasIntersection(opBbox, bbox))
                        return true;
                }

                return false;
            }

            private HasIntersection(bbox, bbox2) {
                var isXIntersects = bbox2.x <= bbox.x + bbox.width && bbox2.x + bbox2.width >= bbox.x;
                var isYIntersects = bbox2.y <= bbox.y + bbox.height && bbox2.y + bbox2.height >= bbox.y;
                return isXIntersects && isYIntersects;
            }

            private Intersects(point, bbox) {
                return bbox.x <= point.x && (bbox.x + bbox.width) >= point.x && bbox.y <= point.y && (bbox.y + bbox.height) >= point.y;
            }

            private PerformLTL(operation: BMA.LTLOperations.OperationLayout) {
                var that = this;
                var domplot: any = this.navigationDriver.GetNavigationSurface();

                if (operation.Tag === undefined || operation.Tag.driver === undefined) {
                    console.log("Unable to perform LTL request. No driver assosiated with requested operation");
                    return;
                }

                var driver = operation.Tag.driver;
                if (operation.IsCompleted) {

                    this.log.LogLTLRequest();

                    operation.AnalysisStatus = "processing";
                    driver.SetStatus("processing", undefined);
                    domplot.updateLayout();

                    var formula = operation.Operation.GetFormula();

                    var model;
                    try {
                        model = BMA.Model.ExportBioModel(that.appModel.BioModel);
                    }
                    catch (exc) {
                        driver.SetStatus("nottested", "Incorrect Model: " + exc);
                        operation.AnalysisStatus = "nottested";
                        operation.Tag.data = undefined;
                        operation.Tag.negdata = undefined;
                        operation.Tag.steps = driver.GetSteps();
                        domplot.updateLayout();
                        that.OnOperationsChanged(false);

                        return;
                    }

                    var proofInput = {
                        "Name": model.Name,
                        "Relationships": model.Relationships,
                        "Variables": model.Variables,
                        "Formula": formula,
                        "Number_of_steps": driver.GetSteps()
                    }

                    var result = that.ajax.Invoke(proofInput)
                        .done(function (res) {
                            if (res.Ticks == null) {
                                that.log.LogLTLError();

                                if (res.Status === "Error" && res.Error.indexOf("Operation is not completed in") > -1)
                                    driver.SetStatus("nottested", "Timed out");
                                else
                                    driver.SetStatus("nottested", "Server error");

                                operation.AnalysisStatus = "nottested";
                                operation.Tag.data = undefined;
                                operation.Tag.negdata = undefined;
                                operation.Tag.steps = driver.GetSteps();
                                domplot.updateLayout();
                                that.OnOperationsChanged(false);
                            }
                            else {
                                if (res.Status === "True") {

                                    driver.SetShowResultsCallback(function () {
                                        that.commands.Execute("ShowLTLResults", {
                                            ticks: res.Ticks
                                        });
                                    });

                                    driver.SetStatus("success");
                                    //driver.Expand();
                                    operation.AnalysisStatus = "success";
                                    operation.Tag.data = res.Ticks;
                                    operation.Tag.negdata = undefined;
                                    operation.Tag.steps = driver.GetSteps();

                                } else if (res.Status === "PartiallyTrue") {

                                    driver.SetShowResultsCallback(function (showpositive) {
                                        that.commands.Execute("ShowLTLResults", {
                                            ticks: showpositive ? res.Ticks : res.NegTicks
                                        });
                                    });

                                    driver.SetStatus("partialsuccess");
                                    //driver.Expand();
                                    operation.AnalysisStatus = "partialsuccess";
                                    operation.Tag.data = res.Ticks;
                                    operation.Tag.negdata = res.NegTicks;
                                    operation.Tag.steps = driver.GetSteps();


                                } else {
                                    driver.SetShowResultsCallback(function (showpositive) {
                                        that.commands.Execute("ShowLTLResults", {
                                            ticks: res.NegTicks
                                        });
                                    });

                                    driver.SetStatus("fail");
                                    //driver.Expand();
                                    operation.AnalysisStatus = "fail";
                                    operation.Tag.data = undefined;
                                    operation.Tag.negdata = res.NegTicks;
                                    operation.Tag.steps = driver.GetSteps();
                                }

                                domplot.updateLayout();
                                that.OnOperationsChanged(false);

                                //if (res.Status == "True") {
                                //var restbl = that.CreateColoredTable(res.Ticks);
                                //ltlviewer.SetResult(restbl);
                                //that.expandedResults = that.CreateExpanded(res.Ticks, restbl);
                                //}
                                //else {
                                //ltlviewer.SetResult(undefined);
                                //alert(res.Status);
                                //}
                            }
                        })
                        .fail(function (xhr, textStatus, errorThrown) {
                            that.log.LogLTLError();
                            driver.SetStatus("nottested", "Server Error" + (errorThrown !== undefined && errorThrown !== "" ? ": " + errorThrown : ""));
                            operation.AnalysisStatus = "nottested";
                            operation.Tag.data = undefined;
                            operation.Tag.negdata = undefined;
                            operation.Tag.steps = driver.GetSteps();
                            domplot.updateLayout();
                            that.OnOperationsChanged(false);
                        })


                    //that.commands.Execute("LTLRequested", { formula: formula });
                } else {
                    operation.HighlightEmptySlots("red");
                    driver.SetStatus("nottested");
                    operation.AnalysisStatus = "nottested";
                    operation.Tag.data = undefined;
                    operation.Tag.negdata = undefined;
                    operation.Tag.steps = driver.GetSteps();
                    domplot.updateLayout();
                }
            }

            private UpdateControlPanels() {
                var that = this;
                var dom = this.navigationDriver.GetNavigationSurface();

                for (var i = 0; i < this.operations.length; i++) {
                    var op = this.operations[i];
                    var bbox = op.BoundingBox;
                    var driver = op.Tag.driver;
                    driver.SetStatus(op.AnalysisStatus);
                    driver.SetSteps(op.Tag.steps);
                    (<any>dom).set(op.Tag.dommarker[0], bbox.x + bbox.width + this.controlPanelPadding, -op.Position.y, 0, 0 /*40 * 57.28 / 27, 40*/);
                    op.Tag.dommarker.show();
                }
            }

            private OnOperationsChanged(updateControls: boolean = true, updateAppModel: boolean = true) {
                var that = this;

                var ops = [];
                var operations = [];
                var appearances = [];
                for (var i = 0; i < this.operations.length; i++) {
                    operations.push(this.operations[i].Operation.Clone());
                    ops.push({ operation: this.operations[i].Operation.Clone(), status: this.operations[i].AnalysisStatus, steps: this.operations[i].Tag.steps });
                    appearances.push({
                        x: this.operations[i].Position.x,
                        y: this.operations[i].Position.y
                    });
                }

                if (updateControls) {
                    this.UpdateControlPanels();
                }

                if (updateAppModel) {
                    this.appModel.Operations = operations;
                    this.appModel.OperationAppearances = appearances;
                }

                var bbox = that.CalcOperationsBBox();
                if (bbox !== undefined) {
                    that.plotConstraints.maxWidth = Math.max(400 * 3, bbox.width * 1.2);
                    that.plotConstraints.maxHeight = Math.max(200 * 3, bbox.height * 1.2);
                }

                this.commands.Execute("TemporalPropertiesOperationsChanged", ops);
            }

            public AddOperation(operation: BMA.LTLOperations.Operation, position: { x: number; y: number }) {
                var that = this;
                var newOp = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), operation, position);
                newOp.RefreshStates(this.appModel.States);
                that.InitializeOperationTag(newOp);
                this.operations.push(newOp);
                this.OnOperationsChanged(true);
            }

            public UpdateStatesFromModel() {
                this.states = this.appModel.States;
                this.tpEditorDriver.SetStates(this.appModel.States);
            }

            private ClearOperationTag(operation: BMA.LTLOperations.OperationLayout, isCompleteClear: boolean) {
                if (operation.Tag !== undefined) {
                    if (operation.Tag.data !== undefined) {
                        operation.Tag.data = undefined;
                    }

                    if (operation.Tag.negdata !== undefined) {
                        operation.Tag.negdata = undefined;
                    }

                    if (isCompleteClear) {
                        if (operation.Tag.driver !== undefined) {
                            operation.Tag.driver.SetOnExpandedCallback(undefined);
                            operation.Tag.driver.SetLTLRequestedCallback(undefined);
                            operation.Tag.driver.SetShowResultsCallback(undefined);
                            operation.Tag.driver.Destroy();
                            operation.Tag.driver = undefined;
                        }
                    }

                    if (operation.Tag.dommarker !== undefined) {
                        if (isCompleteClear) {
                            var dom = this.navigationDriver.GetNavigationSurface();
                            dom.remove(operation.Tag.dommarker);
                        } else {
                            operation.Tag.dommarker.hide();
                        }
                    }
                }
            }

            private InitializeOperationTag(operation: BMA.LTLOperations.OperationLayout) {
                var that = this;
                var defaultSteps = 10;
                var opDiv = $("<div></div>");
                var driver = new BMA.UIDrivers.LTLResultsCompactViewer(opDiv);
                driver.SetSteps(defaultSteps);
                driver.SetStatus(operation.AnalysisStatus);

                var dom = this.navigationDriver.GetNavigationSurface();

                driver.SetLTLRequestedCallback(() => {
                    that.PerformLTL(operation);
                    that.OnOperationsChanged(false, false);
                });

                driver.SetOnExpandedCallback(() => {
                    (<any>dom).updateLayout();
                });

                driver.SetShowResultsCallback(function (showpositive) {
                    if (operation.Tag.data !== undefined) {
                        that.commands.Execute("ShowLTLResults", {
                            ticks: showpositive === undefined || showpositive === true ? operation.Tag.data : operation.Tag.negdata
                        });
                    }
                });

                driver.SetOnExpandedCallback(() => {
                    for (var i = 0; i < this.operations.length; i++) {
                        if (this.operations[i].Tag !== undefined && this.operations[i].Tag.driver !== undefined) {
                            var driverToCheck = this.operations[i].Tag.driver;
                            if (driverToCheck !== driver) {
                                driverToCheck.Collapse();
                            } else {
                                driverToCheck.MoveToTop();
                                if (operation.AnalysisStatus !== "nottested" && operation.AnalysisStatus !== "partialsuccess") {
                                    (<any>dom).set(opDiv[0], operation.BoundingBox.x + operation.BoundingBox.width + that.controlPanelPadding, -operation.Position.y, 0, 0, 0, 0.65);
                                }
                            }
                        }
                    }

                    (<any>dom).updateLayout();
                });

                driver.SetOnStepsChangedCallback(() => {
                    operation.Tag.steps = driver.GetSteps();
                    if (operation.AnalysisStatus !== "nottested") {
                        (<any>dom).set(opDiv[0], operation.BoundingBox.x + operation.BoundingBox.width + that.controlPanelPadding, -operation.Position.y, 0, 0, 0, 0.5);
                        operation.AnalysisStatus = "nottested";
                        driver.SetMessage(undefined);
                    }
                    that.OnOperationsChanged(false, false);
                });

                var bbox = operation.BoundingBox;
                (<any>dom).add(opDiv, "none", bbox.x + bbox.width + this.controlPanelPadding, -operation.Position.y, 0, 0 /*40 * 57.28 / 27, 40*/, 0, 0.5);

                operation.Tag = {
                    data: undefined,
                    negdata: undefined,
                    steps: defaultSteps,
                    driver: driver,
                    dommarker: opDiv
                };
            }

            private RunAllQueries() {
                var that = this;
                for (var i = 0; i < that.operations.length; i++) {
                    var op = that.operations[i];
                    if (op.AnalysisStatus === "nottested") {
                        that.PerformLTL(op);
                    }
                }
                that.OnOperationsChanged(false, false);
            }
        }
    }
} 