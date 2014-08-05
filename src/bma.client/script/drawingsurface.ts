/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

$.widget("BMA.drawingsurface", {
    _plot: null,
    _svgPlot: null,
    options: {
    },

    _create: function () {
        var that = this;

        //$("<div></div>").css("background-color", "red").width(800).height(600).appendTo(that.element);

        var plotDiv = $("<div></div>").width("100%").height("100%").attr("data-idd-plot", "plot").appendTo(that.element);
        var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(plotDiv);
        var svgPlotDiv = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);

       
        that._plot = InteractiveDataDisplay.asPlot(plotDiv);

        var grid = that._plot.get(gridLinesPlotDiv[0]);
        grid.xStep = 15;
        grid.x0 = 5;
        grid.yStep = 35;

        that._plot.navigation.setVisibleRect({ x: 0, y: 0, width: 100, height: 100 }, false);

        $(window).resize(function () { that.resize(); });
        that.resize();
        this.refresh();
    },

    resize: function () {
        if (this._plot !== null && this._plot !== undefined) {
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

interface JQuery {
    drawingsurface(): JQuery;
    drawingsurface(settings: Object): JQuery;
}
