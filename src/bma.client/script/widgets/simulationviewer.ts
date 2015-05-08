/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationviewer", {
        options: {
            data: undefined, // data{variables: [][], colorData: [][]}
            plot: undefined
        },

        refresh: function () {
            var that = this;
            var data = this.options.data;
            
            if (that.options.error !== undefined) {
                that.errorDiv.empty();
                that.errorDiv.show();
                var errTitle = $('<div></div>').addClass('proof-state').appendTo(that.errorDiv);
                $('<img src="../../images/failed.svg">').appendTo(errTitle);
                $('<div></div>').addClass('stabilize-failed').text(that.options.error.title).appendTo(errTitle);
                $('<div></div>').text(that.options.error.message).appendTo(that.errorDiv);
            } else {
                that.errorDiv.hide();
            }

            var container = $('<div></div>').addClass("marginable");
            
            if (data !== undefined &&
                data.variables !== undefined &&
                data.variables.length !== 0) {

                var variablestable = $('<div></div>')
                    .appendTo(container)
                    .addClass("scrollable-results");

                variablestable.coloredtableviewer({
                    header: ["Graph", "Cell", "Name", "Range"],
                    type: "graph-min",
                    numericData: data.variables
                });

                if (data.colorData !== undefined && data.colorData.length !== 0) {
                    var colortable = $('<div></div>')
                        //.attr("id", "Simulation-min-table")
                        .addClass("scrollable-results")
                        .appendTo(container)
                        .coloredtableviewer({
                            type: "simulation-min",
                            colorData: data.colorData
                        });
                }
                that.variables.resultswindowviewer({
                    header: "Variables",
                    content: container,
                    icon: "max",
                    tabid: "SimulationVariables"
                });
            }
            else {
                this.variables.resultswindowviewer();
                that.variables.resultswindowviewer("destroy");
            }

            if (that.options.plot !== undefined && that.options.plot.length !== 0) {
                that.plot = $('<div></div>').addClass('plot-min').simulationplot({ colors: that.options.plot });//.height(160)
                that.plotDiv.resultswindowviewer({
                    header: "Simulation Graph",
                    content: that.plot,
                    icon: "max",
                    tabid: "SimulationPlot"
                });
            }
            else {
                that.plotDiv.resultswindowviewer();
                that.plotDiv.resultswindowviewer("destroy");
            }
        },


        _create: function () {
            var that = this;

            this.errorDiv = $('<div></div>')
                .appendTo(that.element);

            this.variables = $('<div></div>')
                .addClass('simulation-variables')
                .appendTo(that.element)
                .resultswindowviewer();

            this.plotDiv = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer();

            this.refresh();
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            var options = this.options;

            if (key === "data")
                this.options.data = value;
            if (key === "plot")
                this.options.plot = value;
            this._super(key, value);
            this.refresh();
        },

        show: function (tab) {
            switch (tab) {
                case undefined:
                    this.variables.show();
                    this.plotDiv.show();
                    break;
                case "SimulationVariables":
                    this.variables.show();
                    break;
                case "SimulationPlot":
                    this.plotDiv.show();
                    break;
            }
        },

        hide: function (tab) {
            if (tab === "SimulationVariables") {
                this.variables.hide();
                this.element.children().not(this.variables).show();
            }
            if (tab === "SimulationPlot") {
                this.plotDiv.hide();
                this.element.children().not(this.plotDiv).show();
            }
        },

        ChangeVisibility: function (ind, check) {
            try {
                this.plot.simulationplot("ChangeVisibility", ind, check);
            }
            catch (ex){ }
        }

    });
} (jQuery));

interface JQuery {
    simulationviewer(): JQuery;
    simulationviewer(settings: Object): JQuery;
    simulationviewer(optionLiteral: string, optionName: string): any;
    simulationviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}