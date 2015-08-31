module BMA {
    export module LTLOperations {
        export class OperationLayout {
            private operation: Operation;
            private layout: any;
            private padding: { x: number; y: number };
            private keyFrameSize = 25;
            private svg: any;
            private bbox = undefined;
            private position: { x: number; y: number } = { x: 0, y: 0 };
            private isVisible: boolean = true;
            private scale: { x: number; y: number } = { x: 1, y: 1 };
            private borderThickness: number = 1;

            constructor(svg: any, operation: Operation, position: { x: number; y: number }) {
                this.svg = svg;
                this.operation = operation;
                this.padding = { x: 5, y: 10 };
                this.position = position;

                this.Render();
            }

            public get KeyFrameSize(): number {
                return this.keyFrameSize;
            }

            public set KeyFrameSize(value: number) {
                if (value > 0) {
                    if (value !== this.keyFrameSize) {
                        this.keyFrameSize = value;
                        this.Refresh();
                    }
                } else
                    throw "KeyFrame Size must be positive";
            }


            public get IsVisible(): boolean {
                return this.isVisible;
            }

            public set IsVisible(value: boolean) {
                if (value !== this.isVisible) {
                    this.isVisible = value;

                    if (value) {
                        this.Render();
                    } else {
                        this.Clear();
                    }
                }
            }

            public get BorderThickness(): number {
                return this.borderThickness;
            }

            public set BorderThickness(value: number) {
                if (value !== this.borderThickness) {
                    this.borderThickness = value;
                    this.Refresh();
                }
            }

            public get Scale(): { x: number; y: number } {
                return this.scale;
            }

            public set Scale(value: { x: number; y: number }) {
                if (value !== undefined) {
                    if (value.x !== this.scale.x || value.y !== this.scale.y) {
                        this.scale = value;
                        this.Refresh();
                    }
                } else {
                    throw "scale is undefined";
                }
            }

            public get Operation(): Operation {
                return this.operation;
            }

            public get Position(): { x: number; y: number } {
                return this.position;
            }

            public set Position(value: { x: number; y: number }) {
                if (value !== undefined) {
                    if (value.x !== this.position.x || value.y !== this.position.y) {
                        var oldPosition = this.position;

                        this.position = value;

                        this.svg.change(this.renderGroup, {
                            transform: "translate(" + this.position.x + ", " + this.position.y + ") scale(" + this.scale.x + ", " + this.scale.y + ")"
                        });

                        this.bbox.x = this.bbox.x - oldPosition.x + value.x;
                        this.bbox.y = this.bbox.y - oldPosition.y + oldPosition.y;
                    }
                } else {
                    throw "position is undefined";
                }
            }

            public get Padding(): { x: number; y: number } {
                return this.padding;
            }

            public set Padding(value: { x: number; y: number }) {
                if (value !== undefined) {
                    if (value.x !== this.padding.x || value.y !== this.padding.y) {
                        this.padding = value;
                        this.Refresh();
                    }
                } else {
                    throw "padding is undefined";
                }
            }

            public get BoundingBox(): { x: number; y: number; width: number; height: number } {
                return this.bbox;
            }

            public GetEmptySlotAtPosition(x: number, y: number) {
                return this.FindEmptySlotAtPosition(this.layout, x, y);
            }

            private FindEmptySlotAtPosition(layout: any, x: number, y: number) {
                if (layout.isEmpty && Math.sqrt(Math.pow((x - layout.position.x), 2) + Math.pow((y - layout.position.y), 2)) < this.keyFrameSize / 2) {
                    return {
                        operation: layout.operationRef,
                        operandIndex: layout.indexRef
                    };
                } else {
                    if (layout.operands !== undefined) {
                        var result = undefined;
                        for (var i = 0; i < layout.operands.length; i++) {
                            result = this.FindEmptySlotAtPosition(layout.operands[i], x, y);
                            if (result !== undefined)
                                return result;
                        }
                        return result;
                    } else {
                        return undefined;
                    }
                }
            }

            private CreateLayout(svg, operation): any {
                var that = this;
                var layout: any = {};
                layout.operation = operation;

                var paddingX = this.padding.x;

                var op = operation;
                var operator = (<any>op).Operator;
                if (operator !== undefined) {
                    layout.operands = [];
                    layout.operator = operator.Name;

                    var operands = (<BMA.LTLOperations.Operation>op).Operands;
                    var layer = 0;
                    var width = (this.GetOperatorWidth(svg, operator.Name)).width;


                    layout.operatorWidth = width;
                    if (operands.length === 1) {
                        width += paddingX;
                    }

                    for (var i = 0; i < operands.length; i++) {
                        var operand = operands[i];

                        if (operand !== undefined) {
                            var calcLW = that.CreateLayout(svg, operand);
                            calcLW.parentoperationindex = i;
                            calcLW.parentoperation = operation;
                            layer = Math.max(layer, calcLW.layer);
                            layout.operands.push(calcLW);
                            width += (calcLW.width + paddingX * 2);
                        } else {
                            layout.operands.push({ isEmpty: true, width: this.keyFrameSize, operationRef: op, indexRef: i });
                            width += (this.keyFrameSize + 2 * paddingX);

                        }
                    }

                    layout.layer = layer + 1;
                    layout.width = width;
                    return layout;
                } else {
                    var w = this.keyFrameSize;
                    layout.layer = 1;
                    layout.width = w;
                    return layout;
                }
            }

            private SetPositionOffsets(layout, position) {
                var padding = this.padding;
                layout.position = position

                if (layout.operands !== undefined) {
                    var w = layout.operatorWidth;

                    switch (layout.operands.length) {
                        case 1:
                            var x = position.x + layout.width / 2 - layout.operands[0].width / 2 - padding.x;
                            this.SetPositionOffsets(layout.operands[0], { x: x, y: position.y });
                            break;
                        case 2:
                            var x1 = position.x + layout.width / 2 - layout.operands[1].width / 2 - padding.x;
                            this.SetPositionOffsets(layout.operands[1], { x: x1, y: position.y });

                            var x2 = position.x - layout.width / 2 + layout.operands[0].width / 2 + padding.x;
                            this.SetPositionOffsets(layout.operands[0], { x: x2, y: position.y });
                            break;
                        default:
                            throw "Unsupported number of operands";
                    }
                }
            }

            private GetOperatorWidth(svg: any, operator: string): { width: number; height: number } {
                var t = svg.text(0, 0, operator, {
                    "font-size": 10,
                    "fill": "black"
                });
                var bbox = t.getBBox();
                var result = { width: bbox.width, height: bbox.height };
                //console.log(operator + ": " + bbox.width);
                svg.remove(t);
                return result;
            }

            private RenderLayoutPart(svg: any, position: { x: number; y: number }, layoutPart: any, options: any) {
                var paddingX = this.padding.x;
                var paddingY = this.padding.y;

                if (layoutPart.isEmpty) {
                    svg.circle(this.renderGroup, position.x, position.y, this.keyFrameSize / 2, { stroke: "black", fill: "black" });
                } else {
                    var operator = layoutPart.operator;
                    if (operator !== undefined) {
                        var operation = layoutPart;

                        var halfWidth = layoutPart.width / 2;
                        var height = this.keyFrameSize + paddingY * layoutPart.layer;

                        var fill = options && options.fill ? options.fill : "transparent";
                        var stroke = options && options.stroke ? options.stroke : "black";

                        var strokeWidth = 1;
                        if (options !== undefined) {
                            if (options.isRoot) {
                                strokeWidth = this.borderThickness;
                            } else if (options.strokeWidth) {
                                strokeWidth = options.strokeWidth;
                            }
                        }

                        var opSVG = svg.rect(this.renderGroup, position.x - halfWidth, position.y - height / 2, halfWidth * 2, height, height / 2, height / 2, {
                            stroke: stroke,
                            fill: fill,
                            strokeWidth: strokeWidth
                        });

                        layoutPart.svgref = opSVG;

                        var operands = operation.operands;
                        switch (operands.length) {
                            case 1:

                                svg.text(this.renderGroup, position.x - halfWidth + paddingX, position.y + 3, operation.operator, {
                                    "font-size": 10,
                                    "fill": "black"
                                });

                                this.RenderLayoutPart(svg, {
                                    x: position.x + halfWidth - (<any>operands[0]).width / 2 - paddingX,
                                    y: position.y
                                },
                                    operands[0], undefined);

                                break;
                            case 2:

                                this.RenderLayoutPart(svg, {
                                    x: position.x - halfWidth + (<any>operands[0]).width / 2 + paddingX,
                                    y: position.y
                                },
                                    operands[0], undefined);


                                this.RenderLayoutPart(svg, {
                                    x: position.x + halfWidth - (<any>operands[1]).width / 2 - paddingX,
                                    y: position.y
                                },
                                    operands[1], undefined);

                                svg.text(this.renderGroup, position.x - halfWidth + (<any>operands[0]).width + 2 * paddingX, position.y + 3, operation.operator, {
                                    "font-size": 10,
                                    "fill": "black"
                                });

                                break;
                            default:
                                throw "Rendering of operators with " + operands.length + " operands is not supported";


                        }
                    } else {
                        layoutPart.svgref = svg.circle(this.renderGroup, position.x, position.y, this.keyFrameSize / 2, { stroke: "black", fill: "rgb(238,238,238)" });
                    }
                }
            }

            private renderGroup = undefined;
            private Render() {
                var position = this.position;
                var svg = this.svg;

                if (this.renderGroup !== undefined) {
                    svg.remove(this.renderGroup);
                }

                this.layout = this.CreateLayout(svg, this.operation);
                this.position = position;
                this.SetPositionOffsets(this.layout, position);

                this.renderGroup = svg.group({
                    transform: "translate(" + this.position.x + ", " + this.position.y + ") scale(" + this.scale.x + ", " + this.scale.y + ")"
                });

                var halfWidth = this.layout.width / 2;
                var height = this.keyFrameSize + this.padding.y * this.layout.layer;
                this.bbox = {
                    x: position.x - halfWidth,
                    y: position.y - height / 2,
                    width: halfWidth * 2,
                    height: height
                }

                this.RenderLayoutPart(svg, { x: 0, y: 0 }, this.layout, {
                    fill: "white",
                    stroke: "black",
                    strokeWidth: 1,
                    isRoot: true,
                });
            }

            private Clear() {
                if (this.renderGroup !== undefined) {
                    this.svg.remove(this.renderGroup);
                    this.renderGroup = undefined;
                }
            }

            public Refresh() {
                if (this.isVisible)
                    this.Render();
            }

            public CopyOperandFromCursor(x: number, y: number, withCut: boolean): BMA.LTLOperations.IOperand {
                if (x < this.bbox.x || x > this.bbox.x + this.bbox.width || y < this.bbox.y || y > this.bbox.y + this.bbox.height) {
                    return undefined;
                }


                return undefined;
            }

            private GetIntersectedChild(x: number, y: number, position: { x: number; y: number }, layoutPart: any): any {
                var width = layoutPart.width;
                var halfWidth = width / 2;
                var paddingY = this.padding.y;
                var paddingX = this.padding.x;
                var height = this.keyFrameSize + paddingY * layoutPart.layer;

                if (x < position.x - halfWidth || x > position.x + halfWidth || y < position.y - height / 2 || y > position.y + height / 2) {
                    return undefined;
                }

                var operands = layoutPart.operands;

                switch (operands.length) {
                    case 1:

                        if (operands[0].isEmpty)
                            return layoutPart;

                        var highlighted = this.GetIntersectedChild(x, y, {
                            x: position.x + halfWidth - (<any>operands[0]).width / 2 - paddingX,
                            y: position.y
                        }, operands[0]);

                        return highlighted !== undefined ? highlighted : layoutPart;

                        break;
                    case 2:

                        if (!operands[0].isEmpty) {
                            var highlighted1 = this.GetIntersectedChild(x, y, {
                                x: position.x - halfWidth + (<any>operands[0]).width / 2 + paddingX,
                                y: position.y
                            }, operands[0]);

                            if (highlighted1 !== undefined) {
                                return highlighted1;
                            }
                        }

                        if (!operands[1].isEmpty) {
                            var highlighted2 = this.GetIntersectedChild(x, y, {
                                x: position.x + halfWidth - (<any>operands[1]).width / 2 - paddingX,
                                y: position.y
                            }, operands[1]);

                            if (highlighted2 !== undefined) {
                                return highlighted2;
                            }
                        }

                        return layoutPart;

                        break;
                    default:
                        throw "Highlighting of operators with " + operands.length + " operands is not supported";

                }

                return layoutPart;
            }

            public HighlightAtPosition(x: number, y: number) {
                if (this.layout !== undefined) {
                    this.Refresh();

                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout);

                    if (layoutPart !== undefined) {
                        this.svg.change(layoutPart.svgref, {
                            strokeWidth: 4
                        });
                    }
                }
            }

            public PickOperation(x: number, y: number) {
                if (this.layout !== undefined) {
                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout);
                    if (layoutPart !== undefined)
                        return layoutPart.operation;
                }

                return undefined;
            }

            public UnpinOperation(x: number, y: number) {
                if (this.layout !== undefined) {
                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout);

                    if (layoutPart !== undefined && layoutPart.parentoperation !== undefined) {
                        layoutPart.parentoperation.operands[layoutPart.parentoperationindex] = undefined;
                        this.Refresh();
                    }

                    return {
                        operation: layoutPart.operation,
                        isRoot: layoutPart.parentoperation === undefined
                    };
                }

                return undefined;
            }
        }
    }
}