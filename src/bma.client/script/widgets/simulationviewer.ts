/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationviewer", {
        options: {
            data: undefined
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            
        },


        _create: function () {
            var that = this;
            var options = this.options;
            var variables = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer({ header: "Variables", icon: "max" });
            
            var plotDiv = $('<div></div>')
                .appendTo(that.element)
                .simulationplot({ data: that.options.data });
            //$('<div>Plot should be here</div>').appendTo(that.element);
            //that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this.refresh();
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "data") this.options.data = value;
            this._super(key, value);
            this.refresh();
        }

    });
} (jQuery));

interface JQuery {
    simulationviewer(): JQuery;
    simulationviewer(settings: Object): JQuery;
    simulationviewer(optionLiteral: string, optionName: string): any;
    simulationviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}