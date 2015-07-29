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

            private selectedOperatorType: string;

            private driver: BMA.UIDrivers.ISVGPlot;
            private navigationDriver: BMA.UIDrivers.INavigationPanel;
            private dragService: BMA.UIDrivers.IElementsPanel;

            private operatorRegistry: BMA.LTLOperations.OperatorsRegistry;

            private commands: BMA.CommandRegistry;

            private svgRef: any;

            constructor(
                commandRegistry: BMA.CommandRegistry,
                svgPlotDriver: BMA.UIDrivers.ISVGPlot,
                navigationDriver: BMA.UIDrivers.INavigationPanel,
                dragService: BMA.UIDrivers.IElementsPanel) {

                var that = this;

                this.commands = commandRegistry;
                this.driver = svgPlotDriver;
                this.navigationDriver = navigationDriver;
                this.dragService = dragService;
                this.operations = [];
                this.operatorRegistry = new BMA.LTLOperations.OperatorsRegistry();
                this.svgRef = svgPlotDriver.GetSVGRef();

                window.Commands.On("AddOperatorSelect",(type: string) => {
                    that.selectedOperatorType = type;
                    that.navigationDriver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick",(args: { x: number; y: number; screenX: number; screenY: number }) => {
                    var position = { x: args.x, y: args.y };

                    if (that.selectedOperatorType !== undefined) {
                        var operation = new BMA.LTLOperations.Operation();
                        operation.Operator = that.operatorRegistry.GetOperatorByName(that.selectedOperatorType);
                        operation.Operands = operation.Operator.OperandsCount > 1 ? [undefined, undefined] : [undefined];

                        var emptyCell = undefined;
                        for (var i = 0; i < that.operations.length; i++) {
                            emptyCell = that.operations[i].GetEmptySlotAtPosition(position.x, position.y);
                            if (emptyCell !== undefined) {
                                emptyCell.opLayout = that.operations[i];
                                break;
                            }
                        }

                        if (emptyCell === undefined) {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.svgRef, operation, position);
                            that.operations.push(operationLayout);
                        } else {
                            emptyCell.operation.Operands[emptyCell.operandIndex] = operation;
                            emptyCell.opLayout.Position = emptyCell.opLayout.Position;
                        }
                    }
                });
            }

            private CheckIntersections(point: { x: number; y: number }): boolean {
                return false;
            }
        }
    }
} 