/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            //data: undefined,
            colors: undefined
        },
        _create: function () {
            var that = this;

            this.refresh();
            //window.Commands.On("ChangePlotVariables", (param) => {
            //    this.changeVisibility(param)
            //}
            //)
            //<div id="p" data-idd-plot="polyline" data-idd-style="stroke: rgb(89,150,255); thickness: 3" unselectable="on" class="unselectable idd-plot-dependant" style="width: 760px; height: 571px; top: 0px; left: 40px;"><canvas class="idd-plot-canvas" width="760" height="571"></canvas></div>
            //<div data-idd-plot="plot" data-idd-placement="center" unselectable="on" class="unselectable idd-plot-dependant" style="z-index: 1000; width: 760px; height: 571px; top: 0px; left: 40px; background-color: rgba(0, 0, 0, 0);"><div class="idd-legend unselectable" style="display: block; float: right;"><div class="idd-legend-item"><canvas style="margin-right: 15px" width="20" height="20"></canvas><span>p</span></div></div></div><div class="idd-figure-container" data-idd-placement="bottom" style="width: 760px; left: 40px; top: 571px;"><div data-idd-axis="numeric" data-idd-placement="bottom" class="idd-axis unselectable" style="position: relative; height: 29px; width: 760px;"><canvas id="canvas" style="position:relative; float:left" height="11" width="760"></canvas><div id="labelsDiv" style="position: relative; float: left; width: 760px; height: 18px;"><div class="idd-axis-label" style="display: block; left: 37.4453947368421px;">0</div><div class="idd-axis-label" style="display: block; left: 144.130263157895px;">0.5</div><div class="idd-axis-label" style="display: block; left: 369.5px;">1.5</div><div class="idd-axis-label" style="display: block; left: 594.869736842105px;">2.5</div><div class="idd-axis-label" style="display: none;">0.8</div><div class="idd-axis-label" style="display: block; left: 262.815131578947px;">1</div><div class="idd-axis-label" style="display: none;">1.2</div><div class="idd-axis-label" style="display: none;">1.4</div><div class="idd-axis-label" style="display: none;">1.6</div><div class="idd-axis-label" style="display: none;">1.8</div><div class="idd-axis-label" style="display: block; left: 488.184868421053px;">2</div><div class="idd-axis-label" style="display: none;">2.2</div><div class="idd-axis-label" style="display: none;">2.4</div><div class="idd-axis-label" style="display: none;">2.6</div><div class="idd-axis-label" style="display: none;">2.8</div><div class="idd-axis-label" style="display: block; left: 713.554605263158px;">3</div></div></div></div><div class="idd-figure-container" data-idd-placement="left" style="height: 571px; left: 0px; top: 0px;"><div data-idd-axis="numeric" data-idd-placement="left" class="idd-axis unselectable" style="position: relative; width: 40px; height: 571px;"><div id="labelsDiv" style="position: relative; float: left; height: 571px; width: 26px;"><div class="idd-axis-label" style="display: block; top: 519.572679509632px; left: 12px;">-1</div><div class="idd-axis-label" style="display: block; top: 397.786339754816px; left: 0px;">-0.5</div><div class="idd-axis-label" style="display: block; top: 154.213660245184px; left: 6px;">0.5</div><div class="idd-axis-label" style="display: none;">-0.4</div><div class="idd-axis-label" style="display: none;">-0.2</div><div class="idd-axis-label" style="display: block; top: 276px; left: 18px;">0</div><div class="idd-axis-label" style="display: none;">0.2</div><div class="idd-axis-label" style="display: none;">0.4</div><div class="idd-axis-label" style="display: none;">0.6</div><div class="idd-axis-label" style="display: none;">0.8</div><div class="idd-axis-label" style="display: block; top: 32.4273204903678px; left: 18px;">1</div></div><canvas id="canvas" style="position: relative; float: left; left: 3px;" width="11" height="571"></canvas></div></div></div>
        },
        changeVisibility: function (param) {
            var polyline = this._chart.get(this.chartdiv.children().eq(param.ind).attr("id"));
            polyline.isVisible = param.check;
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();
            this.chartdiv = $('<div id="chart" data-idd-plot="figure" style="width: 100%; height: 160px;"></div>').appendTo(that.element);

            //var plotDiv = $('<div data-idd-plot="polyline" style="width:100%; height:160px"></div>').appendTo(that.element);
            //var grid = $('<div data-idd-plot="grid" data-idd-placement="center" style="width: 100%; height: 160px; top: 0px; left: 40px;"></div>').appendTo(that.chartdiv);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").attr("data-idd-placement", "center").appendTo(this.chartdiv);
            $("<div></div>").attr("data-idd-axis", "numeric").attr("data-idd-placement", "left").appendTo(this.chartdiv);
            $("<div></div>").attr("data-idd-axis", "numeric").attr("data-idd-placement", "bottom").appendTo(this.chartdiv);

            if (that.options.colors !== undefined && that.options.colors !== null) {
                for (var i = 0; i < that.options.colors.length; i++)
                    $('<div></div>').attr("id", "polyline" + i).attr("data-idd-plot", "polyline").attr("data-idd-placement", "center").width("100%").height("100%").appendTo(this.chartdiv);

                that._chart = InteractiveDataDisplay.asPlot(this.chartdiv);
                that._chart.isAutoFitEnabled = true;

                this._gridLinesPlot = that._chart.get(gridLinesPlotDiv[0]);
                this._gridLinesPlot.x0 = 0;
                this._gridLinesPlot.y0 = 0;
                this._gridLinesPlot.xStep = 1;
                this._gridLinesPlot.yStep = 1;

                if (options.colors !== undefined) {
                    for (var i = 0; i < options.colors.length; i++) {
                        var y = options.colors[i].Plot;
                        var polyline = that._chart.get(that.chartdiv.children().eq(i + 1).attr("id"));
                        polyline.stroke = options.colors[i].Color;
                        polyline.isVisible = options.colors[i].Seen;
                        polyline.draw({ y: y, thickness: 4, lineJoin: 'round' });
                    }
                }

                var bounds = that._chart.aggregateBounds();
                that._chart.navigation.setVisibleRect(bounds.bounds, false);
            }
        },
        getPlot: function () {
            return this._chart;
        },
        _destroy: function () {
            var that = this;

            //alert("destroy");
            //window.Commands.Off("ChangePlotVariables", function (param) {
            //    var polyline = that._chart.get(that.chartdiv.children().eq(param.ind).attr("id"));
            //    polyline.isVisible = param.check;
            //})
            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "colors":
                    //for (var i = 0; i < value.length; i++)
                    //    if (this.options.color[i].Seen !== value[i].Seen) {
                    //        var polyline = that._chart.get(that.chartdiv.children().eq(i+1).attr("id"));
                    //        polyline.isVisible = value[i].Seen;
                    //    }
                    this.options.colors = value;
                    break;
            }
            if (value !== null && value !== undefined)
                this.refresh();
            this._super(key, value);
        }
    });
}(jQuery));
//# sourceMappingURL=simulationplot.js.map
