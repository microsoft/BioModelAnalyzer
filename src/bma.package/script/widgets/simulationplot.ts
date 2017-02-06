// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            //data: undefined,
            colors: undefined,
            labels: undefined,
        },

        _create: function () {
            var that = this;
            
            //this.refresh();

            this.element.addClass('simulation-plot-box');

            this.chartdiv = $('<div id="chart"></div>')
                .attr("data-idd-plot", "figure")
                .width("70%")
                .height('100%')
                .css("float", "left")
                .appendTo(that.element);
            this.legendDiv = $('<div></div>').addClass("simulationplot-legend-legendcontainer").appendTo(that.element);

            var gridLinesPlotDiv = $("<div></div>")
                .attr("id", "glPlot")
                .attr("data-idd-plot", "scalableGridLines")
                .appendTo(this.chartdiv);

            var rectsPlotDiv = $("<div></div>")
                .attr("id", "rectsPlot")
                .attr("data-idd-plot", "rectsPlot")
                .appendTo(this.chartdiv);

            that._chart = InteractiveDataDisplay.asPlot(that.chartdiv);
            that._chart.isToolTipEnabled = false;
            that._chart.isAutoFitEnabled = true;
            this._gridLinesPlot = that._chart.get(gridLinesPlotDiv[0]);
            this._gridLinesPlot.x0 = 0;
            this._gridLinesPlot.y0 = 0;
            this._gridLinesPlot.xStep = 1;
            this._gridLinesPlot.yStep = 1;

            this.highlightPlot = that._chart.polyline("_hightlightPlot", undefined);
            this.highlightPlot.host.css("z-index", 10);

            this.bottomAxis = that._chart.addAxis("bottom", "labels", { labels: [] });
            this.leftAxis = that._chart.addAxis("left", "labels", { labels: [] });

            var bottomAxis = this.bottomAxis;
            var leftAxis = this.leftAxis;

            that._chart.centralPart.mousedown(function (e) {
                e.stopPropagation();
            });

            bottomAxis.mousedown(function (e) {
                e.stopPropagation();
            });

            leftAxis.mousedown(function (e) {
                e.stopPropagation();
            });

            var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._chart.centralPart);
            var bottomAxisGestures = InteractiveDataDisplay.Gestures.applyHorizontalBehavior(InteractiveDataDisplay.Gestures.getGesturesStream(bottomAxis));
            var leftAxisGestures = InteractiveDataDisplay.Gestures.applyVerticalBehavior(InteractiveDataDisplay.Gestures.getGesturesStream(leftAxis));
            that._chart.navigation.gestureSource = gestureSource.merge(bottomAxisGestures.merge(leftAxisGestures));

            this.refresh();
        },

        refresh: function () {
            var that = this;
            var options = this.options;

            //Clear legend
            this.legendDiv.empty();

            //states markers on plot
            if (that.options.labels !== undefined && that.options.labels !== null) {
                that.rectsPlot = that._chart.get("rectsPlot");
                var rects = [];
                for (var i = 0; i < that.options.labels.length; i++)
                    rects.push({
                        x: that.options.labels[i].x, y: that.options.labels[i].y,
                        width: that.options.labels[i].width, height: that.options.labels[i].height,
                        fill: i % 2 == 0 ? "gray" : "lightgray",
                        opacity: 0.5,
                        labels: that.options.labels[i].text
                    });
                that.rectsPlot.draw({ rects: rects });
            }

            this.highlightPlot.draw({ y: [] });
            if (that.options.colors !== undefined && that.options.colors !== null) {
                var index = 0;
                while (true) {
                    var plotName = "plot" + index;
                    var polyline = that._chart.get(plotName);
                    if (polyline !== undefined) {
                        polyline.remove();
                        index++;
                    } else {
                        break;
                    }
                }

                var bottomLabels = [];
                var leftLabels = [];
                var max = 0;

                if (options.colors !== undefined) {
                    for (var i = 0; i < options.colors.length; i++) {
                        var y = options.colors[i].Plot;
                        var m = that.Max(y);
                        if (m > max) max = m;
                        var plotName = "plot" + i;
                        var polyline = that._chart.get(plotName);
                        if (polyline === undefined) {
                            polyline = that._chart.polyline(plotName, undefined);
                        }

                        polyline.stroke = options.colors[i].Color;
                        polyline.isVisible = options.colors[i].Seen;
                        polyline.draw({ y: y, thickness: 4, lineJoin: 'round' });


                        var legendItem = $("<div></div>").addClass("simulationplot-legend-legenditem").attr("data-index", i).appendTo(that.legendDiv);
                        if (!options.colors[i].Seen) legendItem.hide();
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
                                        var plotName = "plot" + i;
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
                                        var plotName = "plot" + i;
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

                that._chart.removeDiv(this.bottomAxis[0]);
                this.bottomAxis.remove();
                this.bottomAxis = that._chart.addAxis("bottom", "labels", { labels: bottomLabels });
                that._chart.removeDiv(this.leftAxis[0]);
                this.leftAxis.remove();
                this.leftAxis = that._chart.addAxis("left", "labels", { labels: leftLabels });
                
                //var bounds = that._chart.aggregateBounds();
                //console.log(bounds);
                that._chart.fitToView();
                /*
                bounds.bounds.height += 0.04; // padding
                bounds.bounds.y -= 0.02;      // padding
                that._chart.navigation.setVisibleRect(bounds.bounds, false);
                */
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
            var plotName = "plot" + ind;
            var polyline = this._chart.get(plotName);
            this.options.colors[ind].Seen = check;
            polyline.isVisible = check;

            var legenditem = this.element.find(".simulationplot-legend-legenditem[data-index=" + ind + "]");//.attr("data-index", i)
            if (check) legenditem.show();
            else legenditem.hide();
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
                case "labels":
                    this.options.labels = value;
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
