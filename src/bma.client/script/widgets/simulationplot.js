(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            colors: undefined
        },
        _create: function () {
            var that = this;

            this.refresh();
        },
        changeVisibility: function (param) {
            var polyline = this._chart.get(this.chartdiv.children().eq(param.ind).attr("id"));
            polyline.isVisible = param.check;
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();
            this.chartdiv = $('<div id="chart"></div>').attr("data-idd-plot", "figure").width("100%").height("100%").appendTo(that.element);

            $("<div></div>").attr("data-idd-axis", "numeric").attr("data-idd-placement", "left").appendTo(this.chartdiv);
            $("<div></div>").attr("data-idd-axis", "numeric").attr("data-idd-placement", "bottom").appendTo(this.chartdiv);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(this.chartdiv);

            if (that.options.colors !== undefined && that.options.colors !== null) {
                for (var i = 0; i < that.options.colors.length; i++)
                    $('<div></div>').attr("id", "polyline" + i).attr("data-idd-plot", "polyline").appendTo(this.chartdiv);

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
                        var polyline = that._chart.get("polyline" + i);
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
        ChangeVisibility: function (ind, check) {
            var polyline = this._chart.get("polyline" + ind);
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
}(jQuery));
