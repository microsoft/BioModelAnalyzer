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
                parentoperationindex: number
            };
            private selectedOperatorType: string;

            private driver: BMA.UIDrivers.ISVGPlot;
            private navigationDriver: BMA.UIDrivers.INavigationPanel;
            private dragService: BMA.UIDrivers.IElementsPanel;

            private operatorRegistry: BMA.LTLOperations.OperatorsRegistry;
            private previousHighlightedOperation: BMA.LTLOperations.OperationLayout;

            private clipboard: any;
            private contextElement: any;

            constructor(
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                navigationDriver: BMA.UIDrivers.INavigationPanel,
                dragService: BMA.UIDrivers.IElementsPanel,
                contextMenu: BMA.UIDrivers.IContextMenu) {

                var that = this;

                this.driver = svgPlotDriver;
                this.navigationDriver = navigationDriver;
                this.dragService = dragService;

                this.operatorRegistry = new BMA.LTLOperations.OperatorsRegistry();
                this.operations = [];

                window.Commands.On("AddOperatorSelect",(type: string) => {
                    that.selectedOperatorType = type;
                    that.navigationDriver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceDrop",(args: { x: number; y: number; screenX: number; screenY: number }) => {
                    if (that.selectedOperatorType !== undefined) {
                        var registry = this.operatorRegistry;
                        var position = { x: args.x, y: args.y };

                        var op = new BMA.LTLOperations.Operation();
                        op.Operator = registry.GetOperatorByName(that.selectedOperatorType);
                        op.Operands = op.Operator.OperandsCount > 1 ? [undefined, undefined] : [undefined];

                        var operation = that.GetOperationAtPoint(args.x, args.y);

                        if (operation !== undefined) {
                            var emptyCell = undefined;
                            emptyCell = operation.GetEmptySlotAtPosition(position.x, position.y);
                            if (emptyCell !== undefined) {
                                emptyCell.opLayout = operation;
                                emptyCell.operation.Operands[emptyCell.operandIndex] = op;
                                emptyCell.opLayout.Refresh();
                            }
                        } else {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), op, position);
                            if (that.HasIntersections(operationLayout)) {
                                operationLayout.IsVisible = false;
                            } else {
                                that.operations.push(operationLayout);
                            }
                        }
                    }
                });

                window.Commands.On("TemporalPropertiesEditorContextMenuOpening",(args) => {
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
                            canPaste = !this.HasIntersections(operationLayout);
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

                window.Commands.On("TemporalPropertiesEditorCut",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        var unpinned = this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = unpinned.operation !== undefined ? unpinned.operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned,
                        };

                        if (unpinned.isRoot) {
                            this.operations.splice(this.operations.indexOf(this.contextElement.operationlayoutref), 1);
                            this.contextElement.operationlayoutref.IsVisible = false;
                        }
                    }
                });

                window.Commands.On("TemporalPropertiesEditorCopy",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        var operation = this.contextElement.operationlayoutref.PickOperation(this.contextElement.x, this.contextElement.y);
                        var clonned = operation !== undefined ? operation.Clone() : undefined;
                        this.clipboard = {
                            operation: clonned
                        };
                    }
                });

                window.Commands.On("TemporalPropertiesEditorPaste",(args: { top: number; left: number }) => {
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
                    }
                });

                window.Commands.On("TemporalPropertiesEditorDelete",(args: { top: number; left: number }) => {
                    if (this.contextElement !== undefined) {
                        if (!this.contextElement.isRoot) {
                            this.contextElement.operationlayoutref.UnpinOperation(this.contextElement.x, this.contextElement.y);
                        } else {
                            this.operations.splice(this.operations.indexOf(this.contextElement.operationlayoutref), 1);
                        }
                    }
                });

                dragService.GetMouseMoves().subscribe(
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


                var dragSubject = dragService.GetDragSubject();
                dragSubject.dragStart.subscribe(
                    (gesture) => {
                        var staginOp = this.GetOperationAtPoint(gesture.x, gesture.y);
                        if (staginOp !== undefined) {
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

                            this.stagingOperation.operation.Scale = { x: 0.4, y: 0.4 };
                            staginOp.IsVisible = !unpinned.isRoot;
                        }
                    });

                dragSubject.drag.subscribe(
                    (gesture) => {
                        if (this.stagingOperation !== undefined) {
                            this.stagingOperation.operation.Position = { x: <number>gesture.x1, y: <number>gesture.y1 };
                        }
                    });

                dragSubject.dragEnd.subscribe(
                    (gesture) => {
                        if (this.stagingOperation !== undefined) {
                            that.navigationDriver.TurnNavigation(true);
                            this.stagingOperation.operation.IsVisible = false;

                            var position = this.stagingOperation.operation.Position;

                            if (!this.HasIntersections(this.stagingOperation.operation)) {
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
                                var operation = this.GetOperationAtPoint(position.x, position.y);
                                if (operation !== undefined && (!this.stagingOperation.isRoot || this.operations.indexOf(operation) !== this.stagingOperation.originIndex)) {
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
                                }
                            }

                            this.stagingOperation.operation.IsVisible = false;
                            this.stagingOperation = undefined;
                        }
                    });
            }

            private GetOperationAtPoint(x: number, y: number) {
                var that = this;
                var operations = this.operations;
                for (var i = 0; i < operations.length; i++) {
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

                    var isXIntersects = opBbox.x <= bbox.x + bbox.width && opBbox.x + opBbox.width >= bbox.x;
                    var isYIntersects = opBbox.y <= bbox.y + bbox.height && opBbox.y + opBbox.height >= bbox.y;

                    if (isXIntersects && isYIntersects)
                        return true;
                }

                return false;
            }
        }
    }
} 