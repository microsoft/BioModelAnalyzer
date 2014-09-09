﻿(function ($) {
    $.widget("BMA.simulationviewer", {
        options: {
            data: undefined,
            ticks: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
        },
        refresh: function () {
            var that = this;
            var options = this.options;
        },
        _create: function () {
            var that = this;
            var options = this.options;
            var variables = $('<div></div>').appendTo(that.element).resultswindowviewer({ header: "Variables", icon: "max" });

            var plotDiv = $('<div></div>').appendTo(that.element).simulationplotviewer({ data: that.options.data, ticks: that.options.ticks });

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
