/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.drawingsurface", {
        _plot: null,
        _svgPlot: null,
        options: {
            isNavigationEnabled: true,
            svg: undefined
        },
        _svgLoaded: function () {
            if (this.options.svg !== undefined && this._svgPlot !== undefined) {
                //this._svgPlot.svg.load("../images/svgtest.txt");
            }
        },
        _create: function () {
            var that = this;

            var plotDiv = $("<div></div>").width("100%").height("100%").attr("data-idd-plot", "plot").appendTo(that.element);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(plotDiv);
            var svgPlotDiv = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);

            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this._plot.aspectRatio = 1;
            var svgPlot = that._plot.get(svgPlotDiv[0]);
            this._svgPlot = svgPlot;

            if (this.options.svg !== undefined) {
                if (svgPlot.svg === undefined) {
                    svgPlot.host.on("svgLoaded", this._svgLoaded);
                } else {
                    svgPlot.svg.clear();
                    svgPlot.svg.add(this.options.svg);
                }
            }

            plotDiv.bind("click touchstart", function (arg) {
                if (that.options.isNavigationEnabled !== true) {
                    var cs = svgPlot.getScreenToDataTransform();
                    window.Commands.Execute("DrawingSurfaceClick", {
                        x: cs.screenToDataX(arg.clientX - plotDiv.offset().left),
                        y: -cs.screenToDataY(arg.clientY - plotDiv.offset().top)
                    });
                }
            });

            var grid = that._plot.get(gridLinesPlotDiv[0]);
            grid.xStep = 300;
            grid.x0 = 0;
            grid.yStep = 350;

            if (this.options.isNavigationEnabled) {
                var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._plot.host);
                that._plot.navigation.gestureSource = gestureSource;
            } else {
                that._plot.navigation.gestureSource = undefined;
            }

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
            switch (key) {
                case "svg":
                    this._svgPlot.svg.clear();
                    this._svgPlot.svg.add(value);
                    break;
                case "isNavigationEnabled":
                    if (value === true) {
                        var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host);
                        this._plot.navigation.gestureSource = gestureSource;
                    } else {
                        this._plot.navigation.gestureSource = undefined;
                    }
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
//# sourceMappingURL=drawingsurface.js.map
