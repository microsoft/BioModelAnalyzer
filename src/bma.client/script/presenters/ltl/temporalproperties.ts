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

                        var emptyCell = undefined;
                        for (var i = 0; i < that.operations.length; i++) {
                            emptyCell = that.operations[i].GetEmptySlotAtPosition(position.x, position.y);
                            if (emptyCell !== undefined) {
                                emptyCell.opLayout = that.operations[i];
                                break;
                            }
                        }

                        if (emptyCell === undefined) {
                            var operationLayout = new BMA.LTLOperations.OperationLayout(that.driver.GetSVGRef(), op, position);
                            that.operations.push(operationLayout);
                        } else {
                            emptyCell.operation.Operands[emptyCell.operandIndex] = op;
                            emptyCell.opLayout.Position = emptyCell.opLayout.Position;
                        }
                    }
                });
            }
        }
    }
} 