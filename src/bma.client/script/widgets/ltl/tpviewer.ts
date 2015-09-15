(function ($) {
    $.widget("BMA.temporalpropertiesviewer", {
        _svg: undefined,
        _pixelOffset: 10,

        options: {
            operations: [],
            padding: { x: 5, y: 5 }
        },

        _create: function () {
            var that = this;
            var root = this.element;

            root.css("overflow-y", "auto").css("overflow-x", "auto");

            var svgdiv = $("<div></div>").appendTo(root);

            var pixofs = this._pixelOffset;
            svgdiv.svg({
                onLoad: function (svg) {
                    that._svg = svg;

                    svg.configure({
                        width: root.width() - pixofs,
                        height: svgdiv.height() - pixofs,
                        viewBox: "0 0 " + (root.width() - pixofs) + " " + (svgdiv.height() - pixofs),
                        preserveAspectRatio: "none meet"
                    }, true);

                    that.refresh();
                }
            });
           
        },

        refresh: function () {
            if (this._svg !== undefined) {
                this._svg.clear();
                var operations = this.options.operations;
                var currentPos = { x: 0, y: 0 };
                var height = this.options.padding.y;
                var width = 0;
                for (var i = 0; i < operations.length; i++) {
                    var opLayout = new BMA.LTLOperations.OperationLayout(this._svg, operations[i], { x: 0, y: 0 });
                    var opbbox = opLayout.BoundingBox;
                    opLayout.Position = { x: opbbox.width / 2, y: height + opbbox.height / 2 };
                    height += opbbox.height + this.options.padding.y;
                    width = Math.max(width, opbbox.width);
                }

                width += 2 * this.options.padding.x;
                height += this.options.padding.y;

                /*
                this._svg.configure({
                    viewBox: "0 0 " + width + " " + height,
                }, true);
                */
            }
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "operations":
                    //this.refresh();
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