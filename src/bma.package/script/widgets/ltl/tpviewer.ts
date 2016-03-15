(function ($) {
    $.widget("BMA.temporalpropertiesviewer", {
        _svg: undefined,
        _pixelOffset: 10,
        _anims: [],

        options: {
            operations: [],
            padding: { x: 3, y: 5 }
        },

        _create: function () {
            var that = this;
            var root = this.element;

            root.css("overflow-y", "auto").css("overflow-x", "auto").css("position", "relative");

            this.attentionDiv = $("<div></div>").addClass("state-compact").appendTo(root);
            $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.attentionDiv);
            $("<div>start by defining some temporal properties</div>").addClass("state-placeholder").appendTo(this.attentionDiv);
            

            var svgdiv = $("<div></div>").appendTo(root);

            this.svgdiv = svgdiv;

            var pixofs = this._pixelOffset;
            svgdiv.svg({
                onLoad: function (svg) {
                    that._svg = svg;

                    svg.configure({
                        width: root.width() - pixofs,
                        height: root.height() - pixofs,
                        viewBox: "0 0 " + (root.width() - pixofs) + " " + (root.height() - pixofs),
                        preserveAspectRatio: "none meet"
                    }, true);

                    that.refresh();
                }
            });

            svgdiv.hide();

        },

        refresh: function () {
            if (this._svg !== undefined) {

                this._svg.clear();

                var svg = this._svg;
                var defs = svg.defs("ltlCompactBmaDefs");

                var pattern = svg.pattern(defs, "pattern-stripe1", 0, 0, 8, 4, {
                    patternUnits: "userSpaceOnUse",
                    patternTransform: "rotate(45)"
                });
                svg.rect(pattern, 0, 0, 4, 4, {
                    transform: "translate(0,0)",
                    fill: "white"
                });

                var mask = svg.mask(defs, "mask-stripe1");
                svg.rect(mask, "-50%", "-50%", "100%", "100%", {
                    fill: "url(#pattern-stripe1)"
                });

                var maxHeight = 25 * 4;

                var operations = this.options.operations;
                var currentPos = { x: 0, y: 0 };
                var height = this.options.padding.y;
                var width = 0;

                for (var i = 0; i < this._anims.length; i++) {
                    this._anims[i].remove();
                }
                this._anims = [];


                for (var i = 0; i < operations.length; i++) {
                    var opLayout = new BMA.LTLOperations.OperationLayout(this._svg, operations[i].operation, { x: 0, y: 0 });
                    opLayout.MaskUrl = "url(#mask-stripe1)";
                    opLayout.AnalysisStatus = operations[i].status;
                    var opbbox = opLayout.BoundingBox;

                    if (opbbox.height > maxHeight) {
                        opLayout.Scale = {
                            x: maxHeight / opbbox.height,
                            y: maxHeight / opbbox.height
                        }
                        opbbox.width *= maxHeight / opbbox.height;
                        opbbox.height *= maxHeight / opbbox.height;
                    }

                    opLayout.Position = { x: opbbox.width / 2 + this.options.padding.x, y: height + opbbox.height / 2 };

                    if (operations[i].status !== "nottested" && operations[i].status !== "processing" && operations[i].steps !== undefined) {
                        var t = svg.text(opbbox.width + 10, opLayout.Position.y + 5, operations[i].steps + " steps", {
                            "font-size": 14,
                            "fill": opLayout.AnalysisStatus === "fail" ? "rgb(254, 172, 158)" : "green"
                        });
                        opbbox.width += t.getBBox().width + 10;
                    } else if (operations[i].status === "processing") {
                        var anim = this._createWaitAnimation(opbbox.width + 10, opLayout.Position.y - 7);
                        this._anims.push(anim);
                        opbbox.width += 30;
                    }

                    height += opbbox.height + this.options.padding.y;
                    width = Math.max(width, opbbox.width);
                }

                width += 2 * this.options.padding.x;
                height += this.options.padding.y;

                

                this.svgdiv.width(width);
                this.svgdiv.height(height);
                this._svg.configure({
                    width: width, // - this._pixelOffset,
                    height: height, // - this._pixelOffset,
                    viewBox: "0 0 " + width + " " + height
                }, true);

            }
        },

        _createWaitAnimation: function (x, y) {
            var snipperCnt = $('<div></div>').width(30).css("position", "absolute").css("top", y).css("left", x).appendTo(this.element);
            var snipper = $('<div></div>').css("display", "inline-block").addClass('spinner').appendTo(snipperCnt);
            for (var i = 1; i < 4; i++) {
                $('<div></div>').addClass('bounce' + i).appendTo(snipper);
            }
            return snipper;
        },

        /*
        _createWaitAnimation: function (x, y) {

            var x0 = x;
            var myrect = this._svg.circle(x0, y, 2, { stroke: "gray", fill: "gray" });
            var animate = function () {
                $(myrect).animate({ svgR: "+=5" }, 500, function () {
                    $(myrect).animate({ svgR: "-=5" }, 500, function () {
                        animate();
                    });
                });
            }
            animate();

            x0 += 13;
            var myrect2 = this._svg.circle(x0, y, 2, { stroke: "gray", fill: "gray" });
            var animate2 = function () {
                $(myrect2).animate({ svgR: "+=5" }, 500, function () {
                    $(myrect2).animate({ svgR: "-=5" }, 500, function () {
                        animate2();
                    });
                });
            }
            animate2();

            x0 += 13;
            var myrect3 = this._svg.circle(x0, y, 2, { stroke: "gray", fill: "gray" });
            var animate3 = function () {
                $(myrect3).animate({ svgR: "+=5" }, 500, function () {
                    $(myrect3).animate({ svgR: "-=5" }, 500, function () {
                        animate3();
                    });
                });
            }
            animate3();
        },
        */

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "operations":
                    if (value !== undefined && value.length > 0) {
                        that.svgdiv.show();
                        that.attentionDiv.hide();
                    } else {
                        that.svgdiv.hide();
                        that.attentionDiv.show();
                    }
                    break;
                case "padding":
                    //this.refresh();
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