/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.simulationplotviewer", {
        options: {
            data: undefined,
            ticks: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            if (that.options.data !== undefined) {
                for (var i = 0; i < that.options.data.length; i++) {
                    that._plot.draw({ x: that.options.ticks, y: that.options.data[i] });
                    //InteractiveDataDisplay.PolylinePlot.draw(that.options.data[i]);
                }
            }
        },
        _create: function () {
            var that = this;
            var options = this.options;
            var plotDiv = $('<div></div>').attr("data-idd-plot", "polyline").appendTo(that.element);
            $('<div>Plot should be here</div>').appendTo(that.element);
            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this.refresh();
        },
        _destroy: function () {
            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            if (key === "data")
                this.options.data = value;
            this._super(key, value);
            this.refresh();
        }
    });
}(jQuery));
//# sourceMappingURL=simulationplotviewer.js.map
