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
            private stagingOperation: BMA.LTLOperations.OperationLayout;
            private selectedOperatorType: string;

            private driver: BMA.UIDrivers.ISVGPlot;
            private navigationDriver: BMA.UIDrivers.INavigationPanel;
            private dragService: BMA.UIDrivers.IElementsPanel;

            private operatorRegistry: BMA.LTLOperations.OperatorsRegistry;

            constructor(
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                navigationDriver: BMA.UIDrivers.INavigationPanel,
                dragService: BMA.UIDrivers.IElementsPanel) {

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

                window.Commands.On("DrawingSurfaceClick",(args: { x: number; y: number; screenX: number; screenY: number }) => {
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
                                emptyCell.opLayout.Position = emptyCell.opLayout.Position;
                            }
                        } else {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), op, position);
                            if (that.HasIntersections(operationLayout)) {
                                operationLayout.Clear();
                            } else {
                                that.operations.push(operationLayout);
                            }
                        }
                    }
                });

                
                var dragSubject = dragService.GetDragSubject();
                dragSubject.dragStart.subscribe(
                    (gesture) => {
                        if (that.selectedOperatorType === undefined) {
                            var staginOp = this.GetOperationAtPoint(gesture.x, gesture.y);
                            if (staginOp !== undefined) {
                                this.stagingOperation = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), staginOp.Operation, gesture);
                            }
                        }
                    });

                dragSubject.drag.subscribe(
                    (gesture) => {
                        if (this.stagingOperation !== undefined) {
                            this.stagingOperation.Position = { x: <number>gesture.x1, y: <number>gesture.y1 };
                        }
                    });
                
                dragSubject.dragEnd.subscribe(
                    (gesture) => {
                        this.stagingOperation = undefined;
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
                    var bbox = operations[i].BoundingBox;

                    if (opBbox.x <= bbox.x + bbox.width &&
                        opBbox.x + opBbox.width >= bbox.x &&
                        opBbox.y <= bbox.y + bbox.height &&
                        opBbox.y + opBbox.height >= bbox.y)
                        return true;
                         
                }

                return false;
            }
        }
    }
} 