// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
(function ($) {
    $.widget("BMA.temporalpropertiesviewer", {
        _svg: undefined,
        _pixelOffset: 10,
        _anims: [],
        _images: [],

        _operationHeights: [],

        options: {
            operations: [],
            padding: { x: 3, y: 5 },
            showDefaultIcon: true,
            rightOffset: 80
        },

        _create: function () {
            var that = this;
            var root = this.element;

            root.css("overflow-y", "auto").css("overflow-x", "hidden").css("position", "relative");

            this.attentionDiv = $("<div></div>").addClass("state-compact").appendTo(root);

            if (that.options.defaultIcon === undefined) {
                $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.attentionDiv);
                $("<div>start by defining some temporal properties</div>").addClass("state-placeholder").appendTo(this.attentionDiv);
            } else {
                that.options.defaultIcon.appendTo(this.attentionDiv);
            }

            if (!that.options.showDefaultIcon) {
                that.attentionDiv.hide();
            }

            that.canvasDiv = $("<div></div>").width(root.width()).appendTo(root);
            that._canvas = $("<canvas></canvas>").attr("width", root.width()).attr("height", root.height()).width(root.width()).appendTo(that.canvasDiv);


            var patterns = [
                "stripe-pattern-green",
                "stripe-pattern-half-green",
                "stripe-pattern-half-half",
                "stripe-pattern-half-red",
                "stripe-pattern-red",
            ];

            for (var i = 0; i < patterns.length; i++) {
                var img = new Image();
                img.src = "images/" + patterns[i] + ".png";
                img.onload = () => {
                    that.refresh();
                }
                that._images.push(img);
            }

            that.refresh();
        },

        getOperationByY: function (y) {
            var that = this;

            if (that._operationHeights !== undefined && that._operationHeights.length > 0) {
                for (var i = 0; i < that._operationHeights.length; i++) {
                    if (y >= that._operationHeights[i].y && y <= that._operationHeights[i].y + that._operationHeights[i].height)
                        return that._operationHeights[i].operation;
                }
            }

            return null;
        },

        refresh: function () {
            var that = this;
            var canvas = <HTMLCanvasElement>(this._canvas[0]);
            var keyFrameSize = 26;
            var padding = { x: 5, y: 10 };
            var maxHeight = keyFrameSize * 4;
            var context = canvas.getContext("2d");

            that._operationHeights = [];

            var PIXEL_RATIO = (function () {
                var dpr = window.devicePixelRatio || 1;
                var bsr = (<any>context).webkitBackingStorePixelRatio ||
                    (<any>context).mozBackingStorePixelRatio ||
                    (<any>context).msBackingStorePixelRatio ||
                    (<any>context).oBackingStorePixelRatio ||
                    (<any>context).backingStorePixelRatio || 1;

                return dpr / bsr;
            })();

            canvas.height = canvas.height;


            var operations = this.options.operations;
            var currentPos = { x: 0, y: 0 };
            var height = this.options.padding.y;
            var width = that.canvasDiv.width() - this.options.padding.x;

            for (var i = 0; i < this._anims.length; i++) {
                this._anims[i].remove();
            }
            this._anims = [];

            var sizes: {
                size: {
                    width: number;
                    height: number
                };
                scale: {
                    x: number;
                    y: number
                };
                offset: number
            }[] = [];

            for (var i = 0; i < operations.length; i++) {
                var op = operations[i].operation;
                var opSize = BMA.LTLOperations.CalcOperationSizeOnCanvas(canvas, op, padding, keyFrameSize);
                var scale = { x: 1, y: 1 };

                var offset = that.options.rightOffset;
                var w = opSize.width + offset;

                if (w > width) {
                    scale = {
                        x: (width - offset) / opSize.width,
                        y: (width - offset) / opSize.width
                    }
                    opSize.width = width - offset;
                    opSize.height = scale.y * opSize.height;
                }

                sizes.push({ size: opSize, offset: offset, scale: scale });
                
                height += opSize.height + this.options.padding.y;
            }
            if (PIXEL_RATIO !== 1) {
                canvas.height = height * PIXEL_RATIO;
                canvas.width = that._canvas.width() * PIXEL_RATIO;
                that._canvas.height(height);
                context.setTransform(PIXEL_RATIO, 0, 0, PIXEL_RATIO, 0, 0);
            } else {
                canvas.height = height;
            }

            //context.msImageSmoothingEnabled = true;
            context.translate(0.5, 0.5);


            height = this.options.padding.y;
            for (var i = 0; i < operations.length; i++) {
                var op = operations[i].operation;
                var opSize = sizes[i].size;
                var scale = sizes[i].scale;
                var opPosition = { x: opSize.width / 2 + this.options.padding.x, y: Math.floor(height + opSize.height / 2) };

                BMA.LTLOperations.RenderOperation(canvas, op, opPosition, scale, {
                    padding: padding,
                    keyFrameSize: keyFrameSize,
                    stroke: "black",
                    fill: that._getOperationColor(operations[i].status, opSize.width / scale.x, opSize.height / scale.y),
                    isRoot: true,
                    strokeWidth: 1,
                    borderThickness: 1
                });


                if (operations[i].status !== "nottested" && operations[i].status.indexOf("processing") < 0 && operations[i].steps !== undefined) {
                    context.font = "14px Segoe-UI";
                    context.textBaseline = "middle";

                    if (operations[i].status === "partialsuccesspartialfail") {
                        context.fillStyle = "darkgray";
                    } else if (operations[i].status === "fail" || operations[i].status === "partialfail") {
                        context.fillStyle = "rgb(254, 172, 158)";
                    } else if (operations[i].status === "success" || operations[i].status === "partialsuccess") {
                        context.fillStyle = "green";
                    }

                    var text = operations[i].steps + " steps";
                    context.fillText(text, opSize.width + 10, opPosition.y);
                } else if (operations[i].status.indexOf("processing") > -1) {
                    var anim = this._createWaitAnimation(opSize.width + 10, opPosition.y - 7, operations[i].status.indexOf("processinglra") > -1);
                    this._anims.push(anim);
                } else if (operations[i].status === "nottested" && operations[i].message !== undefined && operations[i].message !== null) {
                    context.font = "14px Segoe-UI";
                    context.textBaseline = "middle";
                    context.fillStyle = "rgb(254, 172, 158)";
                    var text = <string>operations[i].message;
                    if (text !== "Timed out" && text.length > 0) {
                        if (text.indexOf("Incorrect Model") > -1) {
                            text = "Incorrect model";
                        } else {
                            text = "Server error";
                        }
                    }
                    context.fillText(text, opSize.width + 10, opPosition.y);
                }

                that._operationHeights.push({
                    y: height,
                    height: opSize.height + this.options.padding.y,
                    operation: operations[i].operation
                });

                height += opSize.height + this.options.padding.y;
            }
        },

        _getOperationColor: function (status, width, height): any {
            switch (status) {
                case "nottested":
                    return "white";
                case "processing":
                    return "white";
                case "success":
                    return "rgb(217,255,182)";
                case "partialsuccess":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[0], "repeat");
                case "processing, partialsuccess":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[1], "repeat");
                case "processinglra, partialsuccess":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[1], "repeat");
                case "processing, partialfail":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[3], "repeat");
                case "processinglra, partialfail":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[3], "repeat");
                case "partialfail":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[4], "repeat");
                case "partialsuccesspartialfail":
                    var canvas = <HTMLCanvasElement>(this._canvas[0]);
                    var context = canvas.getContext("2d");
                    return context.createPattern(this._images[2], "repeat");
                case "fail":
                    return "rgb(254, 172, 158)";
                default:
                    return "white";
            }
        },


        _createWaitAnimation: function (x, y, islra) {
            var width = islra ? 70 : 30;
            var snipperCnt = $('<div></div>').width(width).css("position", "absolute").css("top", y).css("left", x).appendTo(this.element);
            var snipper = $('<div></div>').css("display", "inline-block").addClass('spinner').appendTo(snipperCnt);
            for (var i = 1; i < 4; i++) {
                $('<div></div>').addClass('bounce' + i).appendTo(snipper);
            }
            if (islra) {
                $('<div></div>').css("display", "inline-block").css("margin-left", 5).text("(long)").appendTo(snipperCnt);
            }
            return snipperCnt;
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "operations":
                    if (value !== undefined && value.length > 0) {
                        that.canvasDiv.show();
                        that.attentionDiv.hide();
                    } else {
                        that.canvasDiv.hide();
                        if (that.options.showDefaultIcon) {
                            that.attentionDiv.show();
                        }
                    }
                    break;
                case "padding":
                    //this.refresh();
                    break;
                case "showDefaultIcon":
                    if (!value) {
                        that.attentionDiv.hide();
                    } else {
                        if (that.options.operations === undefined || value.length === 0) {
                            that.attentionDiv.show();
                        }
                    }
                    break;
                case "defaultIcon":
                    this.attentionDiv.empty();
                    value.appendTo(this.attentionDiv);
                    break;
                case "rightOffset":
                    break;
                default:
                    break;
            }
            this._super(key, value);
            this.refresh();
        },

        destroy: function () {
            this.element.empty();
        },
    });
} (jQuery));

interface JQuery {
    temporalpropertiesviewer(): any;
    temporalpropertiesviewer(settings: Object): any;
    temporalpropertiesviewer(methodName: string, arg: any): any;
}
