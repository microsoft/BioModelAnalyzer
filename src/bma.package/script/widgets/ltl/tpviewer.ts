(function ($) {
    $.widget("BMA.temporalpropertiesviewer", {
        _svg: undefined,
        _pixelOffset: 10,

        options: {
            operations: [],
            padding: { x: 3, y: 5 }
        },

        _create: function () {
            var that = this;
            var root = this.element;

            root.css("overflow-y", "auto").css("overflow-x", "auto");

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
                var defs = svg.defs("bmaDefs");

                var pattern = svg.pattern(defs, "pattern-stripe", 0, 0, 8, 4, {
                    patternUnits: "userSpaceOnUse",
                    patternTransform: "rotate(45)"
                });
                svg.rect(pattern, 0, 0, 4, 4, {
                    transform: "translate(0,0)",
                    fill: "white"
                });

                var mask = svg.mask(defs, "mask-stripe");
                svg.rect(mask, "-50%", "-50%", "100%", "100%", {
                    fill: "url(#pattern-stripe)"
                });

                var operations = this.options.operations;
                var currentPos = { x: 0, y: 0 };
                var height = this.options.padding.y;
                var width = 0;
                for (var i = 0; i < operations.length; i++) {
                    var opLayout = new BMA.LTLOperations.OperationLayout(this._svg, operations[i].operation, { x: 0, y: 0 });
                    opLayout.AnalysisStatus = operations[i].status;
                    var opbbox = opLayout.BoundingBox;
                    opLayout.Position = { x: opbbox.width / 2 + this.options.padding.x, y: height + opbbox.height / 2 };
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