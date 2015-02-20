/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            //data: undefined,
            colors: undefined,
        },

        _create: function () {
            var that = this;
            this.refresh();
            this.element.addClass('simulation-plot-box');
        },

        changeVisibility: function (param) {
            var polyline = this._chart.get(this.chartdiv.children().eq(param.ind).attr("id"));
            polyline.isVisible = param.check;
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();

            this.chartdiv = $('<div id="chart"></div>')
                .attr("data-idd-plot", "figure")
                .width("70%")
                .height('100%')
                .css("float", "left")
                
                .appendTo(that.element);
            var legendDiv = $('<div></div>').addClass("simulationplot-legend-legendcontainer").appendTo(that.element);

            var gridLinesPlotDiv = $("<div></div>")
                .attr("id", "glPlot")
                .attr("data-idd-plot", "scalableGridLines")
                .appendTo(this.chartdiv);

            that._chart = InteractiveDataDisplay.asPlot(that.chartdiv);



            if (that.options.colors !== undefined && that.options.colors !== null) {
                for (var i = 0; i < that.options.colors.length; i++) {
                    var plotName = options.colors[i].Name === "" || options.colors[i].Name === undefined ? "plot" + i : options.colors[i].Name;
                    that._chart.polyline(plotName, undefined);
                }

                that._chart.isAutoFitEnabled = true;
                this._gridLinesPlot = that._chart.get(gridLinesPlotDiv[0]);
                this._gridLinesPlot.x0 = 0;
                this._gridLinesPlot.y0 = 0;
                this._gridLinesPlot.xStep = 1;
                this._gridLinesPlot.yStep = 1;
                var bottomLabels = [];
                var leftLabels = [];
                var max = 0;
                if (options.colors !== undefined) {
                    for (var i = 0; i < options.colors.length; i++) {
                        var y = options.colors[i].Plot;
                        var m = that.Max(y);
                        if (m > max) max = m;
                        var plotName = options.colors[i].Name === "" || options.colors[i].Name === undefined ? "plot" + i : options.colors[i].Name;
                        var polyline = that._chart.get(plotName);
                        if (polyline !== undefined) {
                            polyline.stroke = options.colors[i].Color;
                            polyline.isVisible = options.colors[i].Seen;
                            polyline.draw({ y: y, thickness: 4, lineJoin: 'round' });
                        }

                        var legendItem = $("<div></div>").addClass("simulationplot-legend-legenditem").attr("data-index", i).appendTo(legendDiv);
                        var colorBoxContainer = $("<div></div>").addClass("simulationplot-legend-colorboxcontainer").appendTo(legendItem);
                        var colorBox = $("<div></div>").addClass("simulationplot-legend-colorbox").css("background-color", options.colors[i].Color).appendTo(colorBoxContainer);
                        var nameBox = $("<div></div>").text(options.colors[i].Name).addClass("simulationplot-legend-namebox").appendTo(legendItem);

                        legendItem.hover(
                            function () {
                                var index = parseInt($(this).attr("data-index"));
                                var p = that.highlightPlot;
                                if (p !== undefined) {
                                    p.stroke = options.colors[index].Color;
                                    p.isVisible = true;
                                    p.draw({ y: options.colors[index].Plot, thickness: 8, lineJoin: 'round' });

                                    for (var i = 0; i < options.colors.length; i++) {
                                        var plotName = options.colors[i].Name === "" || options.colors[i].Name === undefined ? "plot" + i : options.colors[i].Name;
                                        var polyline = that._chart.get(plotName);
                                        if (polyline !== undefined) {
                                            polyline.stroke = "lightgray";
                                        }
                                    }
                                }
                            },
                            function () {
                                var p = that.highlightPlot;
                                if (p !== undefined) {
                                    p.isVisible = false;

                                    for (var i = 0; i < options.colors.length; i++) {
                                        var plotName = options.colors[i].Name === "" || options.colors[i].Name === undefined ? "plot" + i : options.colors[i].Name;
                                        var polyline = that._chart.get(plotName);
                                        if (polyline !== undefined) {
                                            polyline.stroke = options.colors[i].Color;
                                        }
                                    }
                                }
                            });
                    }
                    for (var i = 0; i < options.colors[0].Plot.length; i++) {
                        bottomLabels[i] = i.toString();
                    }
                    for (var i = 0; i < max + 1; i++) {
                        leftLabels[i] = i.toString();
                    }
                }

                this.highlightPlot = that._chart.polyline("_hightlightPlot", undefined);

                var bottomAxis = that._chart.addAxis("bottom", "labels", { labels: bottomLabels });
                var leftAxis = that._chart.addAxis("left", "labels", { labels: leftLabels });
                var bounds = that._chart.aggregateBounds();
                bounds.bounds.height += 0.04; // padding
                bounds.bounds.y -= 0.02;      // padding
                that._chart.navigation.setVisibleRect(bounds.bounds, false);

                var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._chart.centralPart);
                var bottomAxisGestures = InteractiveDataDisplay.Gestures.applyHorizontalBehavior(InteractiveDataDisplay.Gestures.getGesturesStream(bottomAxis));
                var leftAxisGestures = InteractiveDataDisplay.Gestures.applyVerticalBehavior(InteractiveDataDisplay.Gestures.getGesturesStream(leftAxis));
                that._chart.navigation.gestureSource = gestureSource.merge(bottomAxisGestures.merge(leftAxisGestures));
            }

        },

        Max: function (y) {

            if (y !== null && y !== undefined) {
                var max = y[0];
                for (var i = 0; i < y.length; i++) {
                    if (y[i] > max) max = y[i];
                }
                return max;
            }
            else return undefined;
        },

        getPlot: function () {
            return this._chart;
        },

        ChangeVisibility: function (ind, check) {
            var plotName = this.options.colors[ind].Name === "" || this.options.colors[ind].Name === undefined ? "plot" + ind : this.options.colors[ind].Name;
            var polyline = this._chart.get(plotName);
            this.options.colors[ind].Seen = check;
            polyline.isVisible = check;
        },

        _destroy: function () {
            var that = this;
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "colors":
                    this.options.colors = value;
                    break;
            }
            if (value !== null && value !== undefined)
                this.refresh();
            this._super(key, value);

        }
    });
} (jQuery));

interface JQuery {
    simulationplot(): JQuery;
    simulationplot(settings: Object): JQuery;
    simulationplot(settings: string): any;
    simulationplot(optionLiteral: string, optionName: string): any;
    simulationplot(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   