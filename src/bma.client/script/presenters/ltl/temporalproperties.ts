﻿/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
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
                parentoperationindex: number
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

            private controlPanels = [];
            private controlPanelPadding = 3;

            private appModel: BMA.Model.AppModel;
            private ajax: BMA.UIDrivers.IServiceDriver;

            private ltlcompactviewfactory: BMA.UIDrivers.ILTLResultsViewerFactory;
            private isUpdateControlRequested = false;

            constructor(
                commands: BMA.CommandRegistry,
                appModel: BMA.Model.AppModel,
                ajax: BMA.UIDrivers.IServiceDriver,
                tpEditorDriver: BMA.UIDrivers.ITemporalPropertiesEditor,
                statesPresenter: BMA.LTL.StatesPresenter) {

                var that = this;
                this.appModel = appModel;
                this.ajax = ajax;
                this.tpEditorDriver = tpEditorDriver;
                this.driver = tpEditorDriver.GetSVGDriver();
                this.navigationDriver = tpEditorDriver.GetNavigationDriver();
                this.dragService = tpEditorDriver.GetDragService();
                this.commands = commands;

                this.ltlcompactviewfactory = new BMA.UIDrivers.LTLResultsViewerFactory();

                this.operatorRegistry = new BMA.LTLOperations.OperatorsRegistry();
                this.operations = [];

                var contextMenu = tpEditorDriver.GetContextMenuDriver();

                commands.On("AddOperatorSelect",(operatorName: string) => {
                    that.elementToAdd = { type: "operator", name: operatorName };
                });

                commands.On("AddStateSelect",(stateName: string) => {
                    that.elementToAdd = { type: "state", name: stateName };
                });

                commands.On("DrawingSurfaceDrop",(args: { x: number; y: number; screenX: number; screenY: number }) => {
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

                commands.On("TemporalPropertiesEditorContextMenuOpening",(args) => {
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
                        ]);

                        contextMenu.EnableMenuItems([
                            { name: "Cut", isEnabled: emptyCell === undefined },
                            { name: "Copy", isEnabled: emptyCell === undefined },
                            { name: "Delete", isEnabled: emptyCell === undefined },
                            { name: "Paste", isEnabled: canPaste }
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
                        ]);

                        contextMenu.EnableMenuItems([
                            { name: "Paste", isEnabled: canPaste }
                        ]);
                    }
                });

                commands.On("TemporalPropertiesEditorCut",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        this.contextElement.operationlayoutref.AnalysisStatus = "nottested";

                        var unpinned = this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = unpinned.operation !== undefined ? unpinned.operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned,
                        };

                        if (unpinned.isRoot) {
                            this.operations.splice(this.operations.indexOf(this.contextElement.operationlayoutref), 1);
                            this.contextElement.operationlayoutref.IsVisible = false;
                        }

                        this.OnOperationsChanged();
                    }
                });

                commands.On("TemporalPropertiesEditorCopy",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        var operation = this.contextElement.operationlayoutref.PickOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = operation !== undefined ? operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned
                        };
                    }
                });

                commands.On("TemporalPropertiesEditorPaste",(args: { top: number; left: number }) => {
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
                            this.operations.push(operationLayout);
                        }

                        this.OnOperationsChanged();
                    }
                });

                commands.On("TemporalPropertiesEditorDelete",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        this.contextElement.operationlayoutref.AnalysisStatus = "nottested";

                        var op = this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        if (op.isRoot) {
                            var ind = this.operations.indexOf(this.contextElement.operationlayoutref);
                            this.contextElement.operationlayoutref.IsVisible = false;
                            this.operations.splice(ind, 1);
                        }
                        this.OnOperationsChanged();
                    }
                });

                commands.On("KeyframesChanged",(args: { states: BMA.LTLOperations.Keyframe[] }) => {
                    this.ClearResults();
                    for (var i = 0; i < this.operations.length; i++) {
                        var op = this.operations[i];
                        op.RefreshStates(args.states);
                    }
                    that.OnOperationsChanged(false);
                    that.isUpdateControlRequested = true;
                });

                commands.On("TemporalPropertiesEditorExpanded",(args) => {
                    if (that.isUpdateControlRequested) {
                        that.UpdateControlPanels();
                        that.isUpdateControlRequested = false;
                    }

                    for (var i = 0; i < that.operations.length; i++) {
                        that.operations[i].Refresh();
                    }
                });

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
                    });


                var dragSubject = that.dragService.GetDragSubject();
                dragSubject.dragStart.subscribe(
                    (gesture) => {
                        var staginOp = this.GetOperationAtPoint(gesture.x, gesture.y);
                        if (staginOp !== undefined) {
                            staginOp.AnalysisStatus = "nottested";

                            that.navigationDriver.TurnNavigation(false);
                            var unpinned = staginOp.UnpinOperation(gesture.x, gesture.y);
                            this.stagingOperation = {
                                operation: new BMA.LTLOperations.OperationLayout(that.driver.GetLightSVGRef(), unpinned.operation, gesture),
                                originRef: staginOp,
                                originIndex: this.operations.indexOf(staginOp),
                                isRoot: unpinned.isRoot,
                                parentoperation: unpinned.parentoperation,
                                parentoperationindex: unpinned.parentoperationindex
                            };

                            if (that.controlPanels !== undefined && that.controlPanels[that.stagingOperation.originIndex] !== undefined) {
                                that.controlPanels[that.stagingOperation.originIndex].dommarker.hide();
                            }

                            //this.stagingOperation.operation.Scale = { x: 0.4, y: 0.4 };
                            staginOp.IsVisible = !unpinned.isRoot;
                        }
                    });

                dragSubject.drag.subscribe(
                    (gesture) => {
                        if (this.stagingOperation !== undefined) {
                            var bbox = this.stagingOperation.operation.BoundingBox;
                            this.stagingOperation.operation.Position = { x: <number>gesture.x1 + this.stagingOperation.operation.Scale.x * bbox.width / 2, y: <number>gesture.y1 + this.stagingOperation.operation.Scale.y * bbox.height / 2 };

                            var copyZoneBB = tpEditorDriver.GetCopyZoneBBox();
                            var deleteZoneBB = tpEditorDriver.GetDeleteZoneBBox();
                            if (copyZoneBB.x <= gesture.x1 && copyZoneBB.x + copyZoneBB.width >= gesture.x1 &&
                                copyZoneBB.y <= gesture.y1 && copyZoneBB.y + copyZoneBB.height >= gesture.y1) {

                                tpEditorDriver.HighlightCopyZone(true);
                                tpEditorDriver.HighlightDeleteZone(false);
                            } else if (deleteZoneBB.x <= gesture.x1 && deleteZoneBB.x + deleteZoneBB.width >= gesture.x1 &&
                                deleteZoneBB.y <= gesture.y1 && deleteZoneBB.y + deleteZoneBB.height >= gesture.y1) {

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
                            if (copyZoneBB.x <= position.x && copyZoneBB.x + copyZoneBB.width >= position.x &&
                                copyZoneBB.y <= position.y && copyZoneBB.y + copyZoneBB.height >= position.y) {

                                this.clipboard = {
                                    operation: this.stagingOperation.operation.Operation.Clone()
                                };

                                //Operation should stay in its origin place
                                if (this.stagingOperation.isRoot) {
                                    this.stagingOperation.originRef.IsVisible = true;
                                } else {
                                    this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                    this.stagingOperation.originRef.Refresh();
                                }


                            } else if (deleteZoneBB.x <= position.x && deleteZoneBB.x + deleteZoneBB.width >= position.x &&
                                deleteZoneBB.y <= position.y && deleteZoneBB.y + deleteZoneBB.height >= position.y) {

                                if (this.stagingOperation.isRoot) {
                                    this.operations.splice(this.stagingOperation.originIndex, 1);
                                }

                            } else {


                                if (!this.HasIntersections(this.stagingOperation.operation)) {
                                    if (this.stagingOperation.operation.IsOperation) {
                                        if (this.stagingOperation.isRoot) {
                                            this.stagingOperation.originRef.Position = this.stagingOperation.operation.Position;
                                            this.stagingOperation.originRef.IsVisible = true;
                                        } else {
                                            this.operations.push(
                                                new BMA.LTLOperations.OperationLayout(
                                                    that.driver.GetSVGRef(),
                                                    this.stagingOperation.operation.Operation,
                                                    this.stagingOperation.operation.Position));
                                        }
                                    } else {
                                        //State should state in its origin place
                                        this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                        this.stagingOperation.originRef.Refresh();
                                    }
                                } else {
                                    var operation = this.GetOperationAtPoint(position.x, position.y);
                                    if (operation !== undefined) {
                                        var emptyCell = undefined;
                                        emptyCell = operation.GetEmptySlotAtPosition(position.x, position.y);
                                        if (emptyCell !== undefined) {
                                            //emptyCell.opLayout = operation;
                                            emptyCell.operation.Operands[emptyCell.operandIndex] = this.stagingOperation.operation.Operation;
                                            operation.Refresh();

                                            if (this.stagingOperation.isRoot) {
                                                this.operations[this.stagingOperation.originIndex].IsVisible = false;
                                                this.operations.splice(this.stagingOperation.originIndex, 1);
                                            }
                                        } else {
                                            //Operation should stay in its origin place
                                            if (this.stagingOperation.isRoot) {
                                                this.stagingOperation.originRef.IsVisible = true;
                                            } else {
                                                this.stagingOperation.parentoperation.Operands[this.stagingOperation.parentoperationindex] = this.stagingOperation.operation.Operation;
                                                this.stagingOperation.originRef.Refresh();
                                            }
                                        }
                                    } else {
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

                            this.stagingOperation.operation.IsVisible = false;
                            this.stagingOperation = undefined;
                            this.OnOperationsChanged();

                        }
                    });
            }

            private GetOperationAtPoint(x: number, y: number) {
                var that = this;
                var operations = this.operations;
                for (var i = 0; i < operations.length; i++) {
                    if (!operations[i].IsVisible)
                        continue;

                    var bbox = operations[i].BoundingBox;

                    if (bbox.x <= x && (bbox.x + bbox.width) >= x && bbox.y <= y && (bbox.y + bbox.height) >= y) {
                        return operations[i]
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

                    /*
                    var isXIntersects = opBbox.x <= bbox.x + bbox.width && opBbox.x + opBbox.width >= bbox.x;
                    var isYIntersects = opBbox.y <= bbox.y + bbox.height && opBbox.y + opBbox.height >= bbox.y;

                    if (isXIntersects && isYIntersects)
                        return true;
                    */
                }

                return false;
            }

            private HasIntersection(bbox, bbox2) {
                var isXIntersects = bbox2.x <= bbox.x + bbox.width && bbox2.x + bbox2.width >= bbox.x;
                var isYIntersects = bbox2.y <= bbox.y + bbox.height && bbox2.y + bbox2.height >= bbox.y;
                return isXIntersects && isYIntersects;
            }

            private PerformLTL(operation: BMA.LTLOperations.OperationLayout, domplot, driver: BMA.UIDrivers.ICompactLTLResultsViewer) {
                var that = this;

                if (operation.IsCompleted) {

                    var formula = operation.Operation.GetFormula();

                    var model = BMA.Model.ExportBioModel(that.appModel.BioModel);
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
                            alert(res.Error);
                        }
                        else {
                            if (res.Status === "True") {
                                driver.SetStatus("success");
                                operation.AnalysisStatus = "success";
                            } else {
                                driver.SetStatus("fail");
                                operation.AnalysisStatus = "fail";
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
                        .fail(function () {
                        alert("LTL failed");
                    })


                    //that.commands.Execute("LTLRequested", { formula: formula });
                } else {
                    operation.HighlightEmptySlots("red");
                    driver.SetStatus("nottested");
                }
            }

            private ClearResults() {
                for (var i = 0; i < this.operations.length; i++) {
                    this.operations[i].AnalysisStatus = "nottested";
                }
            }

            private SubscribeToLTLRequest(driver, domplot, op) {
                var that = this;
                driver.SetLTLRequestedCallback(() => {
                    that.PerformLTL(op, domplot, driver);
                });
            }

            private SubscribeToLTLCompactExpand(driver, domplot) {
                driver.SetOnExpandedCallback(() => {
                    domplot.updateLayout();
                });
            }

            private UpdateControlPanels() {
                var that = this;

                var cps = this.controlPanels;
                var dom = this.navigationDriver.GetNavigationSurface();

                for (var i = 0; i < cps.length; i++) {
                    dom.remove(cps[i].dommarker);
                }

                this.controlPanels = [];

                for (var i = 0; i < this.operations.length; i++) {
                    var op = this.operations[i];
                    var bbox = op.BoundingBox;
                    var opDiv = $("<div></div>");
                    var cp = {
                        dommarker: opDiv,
                        status: "nottested"
                    };
                    var driver = new BMA.UIDrivers.LTLResultsCompactViewer(opDiv);
                    driver.SetStatus("nottested");
                    that.SubscribeToLTLRequest(driver, dom, op);
                    that.SubscribeToLTLCompactExpand(driver, dom);

                    (<any>dom).add(opDiv, "none", bbox.x + bbox.width + this.controlPanelPadding, -op.Position.y, 0, 0, 0, 0.5);
                    this.controlPanels.push(cp);
                }
            }

            private OnOperationsChanged(updateControls: boolean = true) {
                var that = this;

                var ops = [];
                for (var i = 0; i < this.operations.length; i++) {
                    ops.push({ operation: this.operations[i].Operation.Clone(), status: this.operations[i].AnalysisStatus });
                }

                if (updateControls) {
                    this.UpdateControlPanels();
                }

                this.commands.Execute("TemporalPropertiesOperationsChanged", ops);
            }
        }
    }
} 