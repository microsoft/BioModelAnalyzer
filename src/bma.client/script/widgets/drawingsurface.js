(function ($) {
    $.widget("BMA.drawingsurface", {
        _plot: null,
        _svgPlot: null,
        options: {},
        _create: function () {
            var that = this;

            var plotDiv = $("<div></div>").width("100%").height("100%").attr("data-idd-plot", "plot").appendTo(that.element);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(plotDiv);
            var svgPlotDiv = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);

            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this._plot.aspectRatio = 1 / 1.3;
            var svgPlot = that._plot.get(svgPlotDiv[0]);

            plotDiv.click(function (arg) {
                var cs = svgPlot.getScreenToDataTransform();
                window.Commands.Execute("DrawingSurfaceClick", {
                    x: cs.screenToDataX(arg.clientX),
                    y: cs.screenToDataY(arg.clientY)
                });
            });

            var grid = that._plot.get(gridLinesPlotDiv[0]);
            grid.xStep = 300;
            grid.x0 = 0;
            grid.yStep = 350;

            that._plot.navigation.setVisibleRect({ x: 0, y: 0, width: 2500, height: 1000 }, false);

            $(window).resize(function () {
                that.resize();
            });
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
}(jQuery));
