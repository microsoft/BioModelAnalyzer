/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
$.widget("BMA.drawingsurface", {
    _plot: null,
    _svgPlot: null,
    options: {
        visualization: undefined
    },
    _create: function () {
        var that = this;

        $("<div></div>").css("background-color", "red").width(800).height(600).appendTo(that.element);

        $(window).resize(function () {
            that.resize();
        });
        that.resize();
        this.refresh();
    },
    resize: function () {
        if (this._plot !== null) {
            this._plot.requestUpdateLayout();
        }
    },
    _setOption: function (key, value) {
        if (key === "value") {
            value = this._constrain(value);
        }
        this._super(key, value);
    },
    _setOptions: function (options) {
        this._super(options);
        this.refresh();
    },
    refresh: function () {
    },
    _constrain: function (value) {
        return value;
    },
    destroy: function () {
        this.element.empty();
    }
});
//# sourceMappingURL=drawingsurface.js.map
