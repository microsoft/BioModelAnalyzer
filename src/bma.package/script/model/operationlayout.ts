module BMA {
    export module LTLOperations {
        export interface IOperationLayout {
        }

        export class OperationLayout {
            private operation: IOperand;
            private layout: any;
            private padding: { x: number; y: number };
            private keyFrameSize = 25;
            private svg: any;
            private bbox = undefined;
            private position: { x: number; y: number } = { x: 0, y: 0 };
            private isVisible: boolean = true;
            private scale: { x: number; y: number } = { x: 1, y: 1 };
            private borderThickness: number = 1;
            private fill: string = undefined;
            private status: string = "nottested";
            private tag: any = undefined;
            private useMask = false;
            private mask = "url(#mask-stripe)";

            private version = 0;

            constructor(svg: any, operation: IOperand, position: { x: number; y: number }) {
                this.svg = svg;
                this.operation = operation;
                this.padding = { x: 5, y: 10 };
                this.position = position;

                this.Render();
            }

            public get Version(): number {
                return this.version;
            }

            public get IsCompleted(): boolean {
                return this.checkIsCompleted(this.operation);
            }

            private checkIsCompleted(operation: IOperand) {
                if ((<BMA.LTLOperations.Operation>operation).Operator !== undefined) {
                    var operands = (<BMA.LTLOperations.Operation>operation).Operands;

                    for (var i = 0; i < operands.length; i++) {
                        if (operands[i] === undefined)
                            return false;
                        else if (!this.checkIsCompleted(operands[i]))
                            return false;
                    }
                }

                return true;
            }

            public get AnalysisStatus(): string {
                return this.status;
            }

            public set AnalysisStatus(value: string) {
                switch (value) {
                    case "nottested":
                        this.status = value;
                        this.Fill = "white";
                        break;
                    case "processing":
                        this.status = value;
                        this.Fill = "white";
                        break;
                    case "processinglra":
                        this.status = value;
                        this.Fill = "white";
                        break;
                    case "success":
                        this.status = value;
                        this.Fill = "rgb(217,255,182)";
                        break;
                    case "partialsuccess":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-green)";
                        break;
                    case "partialfail":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-red)";
                        break;
                    case "partialsuccesspartialfail":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-half-half)";
                        break;
                    case "processing, partialsuccess":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-half-green)";
                        break;
                    case "processinglra, partialsuccess":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-half-green)";
                        break;
                    case "processing, partialfail":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-half-red)";
                        break;
                    case "processinglra, partialfail":
                        this.status = value;
                        this.Fill = "url(#stripe-pattern-half-red)";
                        break;
                    case "fail":
                        this.status = value;
                        this.Fill = "rgb(254, 172, 158)";
                        break;
                    default:
                        throw "Invalid status!";
                }
            }

            public get Tag(): any {
                return this.tag;
            }

            public set Tag(value: any) {
                this.tag = value;
            }

            public get MaskUrl(): string {
                return this.mask;
            }

            public set MaskUrl(value: string) {
                if (this.mask !== value) {
                    this.mask = value;

                    if (this.majorRect !== undefined && this.status === "partialsuccess") {
                        this.svg.change(this.majorRect, {
                            mask: this.mask
                        });
                    }
                }
            }

            public get IsOperation(): boolean {
                return (<BMA.LTLOperations.Operation>this.operation).Operator !== undefined;
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

            public get Fill(): string {
                return this.fill;
            }

            public set Fill(value: string) {
                if (value !== this.fill) {
                    this.fill = value;

                    if (this.majorRect !== undefined) {
                        this.svg.change(this.majorRect, {
                            fill: this.fill
                        });
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

            public get Operation(): IOperand {
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
                        this.bbox.y = this.bbox.y - oldPosition.y + value.y;
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



            private SetPositionOffsets(layout, position) {
                var padding = this.padding;
                layout.position = position

                if (layout.operands !== undefined) {
                    var w = layout.operatorWidth;

                    var offset = position.x - layout.width / 2 + padding.x;
                    if (layout.isFunction || layout.operands.length === 1) {
                        offset += w + padding.x;
                    }

                    for (var i = 0; i < layout.operands.length; i++) {
                        offset += layout.operands[i].width / 2;
                        this.SetPositionOffsets(layout.operands[i], { x: offset, y: position.y });
                        offset += layout.operands[i].width / 2 + padding.x;

                        if (!layout.isFunction) {
                            offset += w + padding.x;
                        }
                    }
                }
            }

            /*
            private UpdateFill() {
                if (this.layout !== undefined) {

                    var updateFillOfPart = function (layoutPart) {
                        if (layoutPart !== undefined) {
                            this.svg.change(layoutPart.svgref, {
                                fill: this.fill
                            });

                            if (layoutPart.operands !== undefined) {
                                for (var i = 0; i < layoutPart.operands.length; i++) {
                                    updateFillOfPart(layoutPart.operands[i]);
                                }
                            }
                        }
                    }

                    updateFillOfPart(this.layout);
                }
            }
            */

            private GetOperatorWidth(svg: any, operator: string, fontSize: number): { width: number; height: number } {
                var t = svg.text(0, 0, operator, {
                    "font-size": fontSize,
                    "fill": "rgb(96,96,96)"
                });
                var bbox = undefined;
                try {
                    bbox = t.getBBox();
                }
                catch (exc) {
                    bbox = { x: 0, y: 0, width: 1, height: 1 };
                }
                var result = { width: bbox.width, height: bbox.height };
                //console.log(operator + ": " + bbox.width);
                svg.remove(t);
                return result;
            }

            private RenderLayoutPart(svg: any, position: { x: number; y: number }, layoutPart: any, options: any) {
                var paddingX = this.padding.x;
                var paddingY = this.padding.y;

                if (layoutPart.isEmpty) {
                    layoutPart.svgref = svg.circle(this.renderGroup, position.x, position.y, this.keyFrameSize / 2, {
                        stroke: "rgb(96,96,96)", fill: "rgb(96,96,96)"
                    });
                } else {
                    var operator = layoutPart.operator;
                    if (operator !== undefined) {
                        var operation = layoutPart;

                        var halfWidth = layoutPart.width / 2;
                        var height = this.keyFrameSize + paddingY * layoutPart.layer;

                        var fill = options && options.fill ? options.fill : "transparent";
                        var stroke = options && options.stroke ? options.stroke : "rgb(96,96,96)";

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
                            strokeWidth: strokeWidth,
                            fill: "transparent"
                        });


                        layoutPart.svgref = opSVG;

                        var operands = operation.operands;

                        var offset = position.x - halfWidth + paddingX;
                        if (layoutPart.isFunction || operands.length === 1) {
                            svg.text(this.renderGroup, offset, position.y + 3, operation.operator, {
                                "font-size": 10,
                                "fill": "rgb(96,96,96)"
                            });
                            offset += layoutPart.operatorWidth + paddingX;
                        }
                        for (var i = 0; i < operands.length; i++) {
                            offset += operands[i].width / 2;

                            this.RenderLayoutPart(svg, {
                                x: offset,
                                y: position.y
                            },
                                operands[i], undefined);

                            offset += operands[i].width / 2 + paddingX;
                            if (!layoutPart.isFunction) {
                                if (i < operands.length - 1) {
                                    svg.text(this.renderGroup, offset, position.y + 3, operation.operator, {
                                        "font-size": 10,
                                        "fill": "rgb(96,96,96)"
                                    });
                                }
                                offset += layoutPart.operatorWidth + paddingX;
                            }
                        }

                    } else {
                        var stateGroup = svg.group(this.renderGroup, {
                            transform: "translate(" + position.x + ", " + position.y + ")"
                        });

                        var uniquename = this.GenerateUUID();
                        var path = svg.circle(stateGroup, 0, 0, this.keyFrameSize / 2, { stroke: "rgb(96,96,96)", fill: "rgb(238,238,238)", id: uniquename });

                        if (layoutPart.type === "keyframe") {
                            var textGroup = svg.group(stateGroup, {
                            });


                            var label = svg.text(textGroup, 0, 0, layoutPart.name, {
                                "font-size": 16,
                                "fill": "rgb(96,96,96)",
                                //"text-anchor": "middle", 
                                //"alignment-baseline": "middle",
                                //"dominant-baseline": "central"
                            });

                            var bbox = undefined;
                            try {
                                bbox = label.getBBox();
                            }
                            catch (exc) {
                                bbox = { x: 0, y: 0, width: 1, height: 1 };
                            }


                            var scale = 1;
                            if (bbox.width > this.keyFrameSize / 2) {
                                scale = this.keyFrameSize / (2 * bbox.width);
                            }

                            this.svg.change(textGroup, {
                                transform: "scale(" + scale + ", " + scale + ") translate(" + -bbox.width / 2 + ", " + bbox.height / 4 + ")"
                            });
                        } else {
                            var img = svg.image(stateGroup, - this.keyFrameSize / 2, - this.keyFrameSize / 2, this.keyFrameSize, this.keyFrameSize, GetKeyframeImagePath(layoutPart.type, '..'));
                        }

                        layoutPart.svgref = stateGroup;
                    }
                }
            }

            private renderGroup = undefined;
            private majorRect = undefined;
            private Render() {
                var that = this;
                var position = this.position;
                var svg = this.svg;

                if (this.majorRect !== undefined) {
                    this.majorRect = undefined;
                }

                if (this.renderGroup !== undefined) {
                    svg.remove(this.renderGroup);
                }

                this.layout = CreateLayout(this.operation, (name, fontSize) => { return that.GetOperatorWidth(that.svg, name, fontSize).width; }, this.padding, this.keyFrameSize); //this.CreateLayout(svg, this.operation);
                this.position = position;
                this.SetPositionOffsets(this.layout, position);

                this.renderGroup = svg.group({
                    transform: "translate(" + this.position.x + ", " + this.position.y + ") scale(" + this.scale.x + ", " + this.scale.y + ")",
                });

                var halfWidth = this.layout.width / 2;
                var height = this.keyFrameSize + this.padding.y * this.layout.layer;
                this.bbox = {
                    x: position.x - halfWidth,
                    y: position.y - height / 2,
                    width: halfWidth * 2,
                    height: height
                }

                this.majorRect = svg.rect(this.renderGroup, - halfWidth, - height / 2, halfWidth * 2, height, height / 2, height / 2, {
                    fill: this.fill === undefined ? "white" : this.fill,
                    mask: this.useMask ? this.mask : undefined,
                });

                this.RenderLayoutPart(svg, { x: 0, y: 0 }, this.layout, {
                    stroke: "rgb(96,96,96)",
                    strokeWidth: 1,
                    isRoot: true,
                });
            }

            private Clear() {
                if (this.renderGroup !== undefined) {
                    this.svg.remove(this.renderGroup);
                    this.renderGroup = undefined;
                    this.majorRect = undefined;
                }
            }

            public Refresh() {
                if (this.isVisible)
                    this.Render();
            }

            private GetIntersectedChild(x: number, y: number, position: { x: number; y: number }, layoutPart: any, accountEmpty: boolean): any {
                var width = layoutPart.width;
                var halfWidth = width / 2;
                var paddingY = this.padding.y;
                var paddingX = this.padding.x;
                var height = this.keyFrameSize + paddingY * layoutPart.layer;

                if (x < position.x - halfWidth || x > position.x + halfWidth || y < position.y - height / 2 || y > position.y + height / 2) {
                    return undefined;
                }

                var operands = layoutPart.operands;

                if (operands === undefined)
                    return layoutPart;

                var offset = position.x - halfWidth + paddingX;

                if (layoutPart.isFunction || operands.length === 1) {
                    offset += layoutPart.operatorWidth + paddingX;
                }

                for (var i = 0; i < layoutPart.operands.length; i++) {
                    offset += layoutPart.operands[i].width / 2;

                    if (!operands[i].isEmpty) {
                        var highlighted = this.GetIntersectedChild(x, y, {
                            x: offset,
                            y: position.y
                        }, operands[i], accountEmpty);
                        if (highlighted !== undefined) {
                            return highlighted;
                        }
                    } else {
                        if (accountEmpty) {
                            var pos1 = {
                                x: offset,
                                y: position.y
                            };

                            if (Math.sqrt(Math.pow(pos1.x - x, 2) + Math.pow(pos1.y - y, 2)) <= this.keyFrameSize / 2)
                                return operands[i];
                        }
                    }

                    offset += layoutPart.operands[i].width / 2 + paddingX;
                    if (!layoutPart.isFunction) {
                        offset += layoutPart.operatorWidth + paddingX;
                    }
                }

                return layoutPart;
            }

            public HighlightAtPosition(x: number, y: number) {
                if (this.layout !== undefined) {
                    this.Refresh();

                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout, true);

                    if (layoutPart !== undefined) {
                        if (layoutPart.isEmpty) {
                            this.svg.change(layoutPart.svgref, {
                                fill: "lightgray"
                            });
                        } else {
                            this.svg.change(layoutPart.svgref, {
                                strokeWidth: 4
                            });
                        }
                    }
                }
            }

            public PickOperation(x: number, y: number) {
                if (this.layout !== undefined) {
                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout, false);
                    if (layoutPart !== undefined) {
                        return {
                            operation: layoutPart.operation,
                            isRoot: layoutPart.parentoperation === undefined
                        };
                    }
                }

                return undefined;
            }

            public UnpinOperation(x: number, y: number) {
                if (this.layout !== undefined) {
                    var layoutPart = this.GetIntersectedChild(x, y, this.position, this.layout, false);

                    if (layoutPart !== undefined) {
                        if (layoutPart.parentoperation !== undefined) {
                            layoutPart.parentoperation.operands[layoutPart.parentoperationindex] = undefined;
                            this.Refresh();
                        } else {
                            this.IsVisible = false;
                        }
                    }

                    return {
                        operation: layoutPart.operation,
                        isRoot: layoutPart.parentoperation === undefined,
                        parentoperation: layoutPart.parentoperation,
                        parentoperationindex: layoutPart.parentoperationindex
                    };
                }

                return undefined;
            }

            private HiglightEmptySlotsInternal(color: string, layoutPart: any) {
                if (layoutPart !== undefined) {
                    if (layoutPart.isEmpty) {
                        this.svg.change(layoutPart.svgref, {
                            fill: color
                        });
                    } else {
                        if (layoutPart.operands !== undefined) {
                            for (var i = 0; i < layoutPart.operands.length; i++) {
                                this.HiglightEmptySlotsInternal(color, layoutPart.operands[i]);
                            }
                        }
                    }
                }
            }

            public HighlightEmptySlots(color: string) {
                if (this.layout !== undefined) {
                    this.HiglightEmptySlotsInternal(color, this.layout);
                }
            }

            public RefreshStates(states: BMA.LTLOperations.Keyframe[]) {
                var wasUpdated = BMA.LTLOperations.RefreshStatesInOperation(this.operation, states);
                if (wasUpdated) {
                    this.AnalysisStatus = "nottested";
                    this.UpdateVersion();
                }
                
                //this.Refresh();
            }

            private GenerateUUID() {
                var d = new Date().getTime();
                if (window.performance && typeof window.performance.now === "function") {
                    d += performance.now();; //use high-precision timer if available
                }
                var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = (d + Math.random() * 16) % 16 | 0;
                    d = Math.floor(d / 16);
                    return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
                });
                return uuid;
            }

            public UpdateVersion() {
                this.version++;
            }
        }
    }
}