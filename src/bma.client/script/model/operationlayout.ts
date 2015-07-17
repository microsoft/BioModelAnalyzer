﻿module BMA {
    export module LTLOperations {
        export class OperationLayout {
            private operation: Operation;
            private layout: any;
            private padding: { x: number; y: number };
            private keyFrameSize = 25;

            private emptyLocations = [];

            constructor(operation: Operation) {
                this.operation = operation;
                this.padding = { x: 5, y: 10 };

                this.emptyLocations = [];
                this.layout = this.CreateLayout(operation);
            }

            public get Operation(): Operation {
                return this.operation;
            }

            public get Padding(): { x: number; y: number } {
                return this.padding;
            }

            public set Padding(value: { x: number; y: number }) {
                this.padding = value;

                this.emptyLocations = [];
                this.layout = this.CreateLayout(this.operation);
            }

            private CreateLayout(operation): any {
                var that = this;
                var layout: any = {};

                var paddingX = this.padding.x;

                var op = operation;
                var operator = (<any>op).Operator;
                if (operator !== undefined) {
                    layout.operands = [];
                    layout.operator = operator.Name;
                    var operands = (<BMA.LTLOperations.Operation>op).Operands;
                    var layer = 0;
                    var width = BMA.SVGHelper.GetOperatorWidth(operator, paddingX);

                    for (var i = 0; i < operands.length; i++) {
                        var operand = operands[i];

                        if (operand !== undefined) {
                            var calcLW = that.CreateLayout(operand);
                            layer = Math.max(layer, calcLW.layer);
                            layout.operands.push(calcLW);
                            width += (calcLW.width + paddingX * 2);
                        } else {
                            layout.operands.push({ isEmpty: true, width: this.keyFrameSize + paddingX });
                            width += (this.keyFrameSize + paddingX + 2 * paddingX);
                        }
                    }

                    layout.layer = layer + 1;
                    layout.width = width;
                    return layout;
                } else {
                    var w = this.keyFrameSize + paddingX;
                    layout.layer = 1;
                    layout.width = w;
                    return layout;
                }
            }

            private RenderLayoutPart(svg: any, position: { x: number; y: number }, layoutPart: any, operandPosition: string) {
                var paddingX = this.padding.x;
                var paddingY = this.padding.y;

                if (layoutPart.isEmpty) {
                    var keyframePadding = operandPosition === "left" ? paddingX : -paddingX;
                    svg.circle(position.x - keyframePadding / 2, position.y, this.keyFrameSize / 2, { stroke: "black", fill: "red" });
                } else {
                    var operator = layoutPart.operator;
                    if (operator !== undefined) {
                        var operation = layoutPart;

                        var halfWidth = layoutPart.width / 2;
                        var height = 25 + paddingY * layoutPart.layer;

                        var opSVG = svg.rect(position.x - halfWidth, position.y - height / 2, halfWidth * 2, height, height / 2, height / 2, { stroke: "black", fill: "transparent" });

                        var operands = operation.operands;

                        var operatorPadding = 1;
                        var operatorW = layoutPart.operator.length * 4 + paddingX; //GetOperatorWidth(operation.Operator, paddingX);
                        switch (operands.length) {
                            case 1:

                                svg.text(position.x - halfWidth + paddingX, position.y + 3, operation.operator, {
                                    "font-size": 10,
                                    "fill": "black"
                                });

                                this.RenderLayoutPart(svg, {
                                    x: position.x + halfWidth - (<any>operands[0]).width / 2 - paddingX,
                                    y: position.y
                                },
                                    operands[0], "right");

                                break;
                            case 2:

                                this.RenderLayoutPart(svg, {
                                    x: position.x - halfWidth + (<any>operands[0]).width / 2 + paddingX,
                                    y: position.y
                                },
                                    operands[0], "left");


                                this.RenderLayoutPart(svg, {
                                    x: position.x + halfWidth - (<any>operands[1]).width / 2 - paddingX,
                                    y: position.y
                                },
                                    operands[1], "right");

                                var extraPadding = (<any>operands[0]).operator !== undefined ? paddingX : 0;
                                svg.text(position.x - halfWidth + (<any>operands[0]).width + paddingX + extraPadding, position.y + 3, operation.operator, {
                                    "font-size": 10,
                                    "fill": "black"
                                });

                                break;
                            default:
                                throw "Rendering of operators with " + operands.length + " operands is not supported";


                        }
                    } else {
                        var keyframePadding = operandPosition === "left" ? paddingX : -paddingX;
                        svg.circle(position.x - keyframePadding / 2, position.y, this.keyFrameSize / 2, { stroke: "black", fill: "rgb(238,238,238)" });
                    }
                }
            }

            public Render(svg: any, position: { x: number; y: number }) {
                this.RenderLayoutPart(svg, position, this.layout, undefined);
            }
        }
    }
}