/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            data: undefined
        },

        _create: function () {
            var that = this;
            var plotDiv = $('<div></div>').width("100%").height(160).attr("data-idd-plot", "polyline").appendTo(that.element);
            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            that._plot.isAutoFitEnabled = true;
            this.refresh();
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.data !== undefined) {
                for (var i = 0; i < options.data.length; i++) {
                    //alert(options.data[i].toString());
                    that._plot.draw({ y: options.data[i], thickness: 4, lineJoin: 'round' });
                }
            }
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "data") this.options.data = value;

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
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