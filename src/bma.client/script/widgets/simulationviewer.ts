/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationviewer", {
        options: {
            data: undefined, // data{variables: [][], colorData: [][], plot: [][]}
            plot: undefined
        },

        refresh: function () {
            var that = this;
            
            var options = this.options;
            var container = $('<div></div>');
            this.variables.resultswindowviewer();
            //that.plotDiv.resultswindowviewer();
            if (options.data !== undefined && options.data.variables !== undefined && options.data.variables.length !== 0) {
                var variablestable = $('<div></div>')
                    .appendTo(container)
                    .addClass("scrollable-results")
                    .coloredtableviewer({ header: ["Graph", "Cell", "Name", "Range"], type: "graph-min", numericData: options.data.variables });
                if (options.data.colorData !== undefined && options.data.colorData.length !== 0) {
                    var colortable = $('<div id="Simulation-min-table"></div>')
                        .appendTo(container)
                        .coloredtableviewer({ type: "simulation-min", colorData: options.data.colorData });
                }
                that.variables.resultswindowviewer({ header: "Variables", content: container, icon: "max", tabid: "SimulationVariables" });
            }
            else that.variables.resultswindowviewer("destroy");

            if (that.options.plot !== undefined) {
                that.plot.simulationplot({ colors: that.options.plot });
                //that.plotDiv.resultswindowviewer({ content: plot, icon: "max", tabid: "SimulationPlot" });
            }
            //else that.plotDiv.resultswindowviewer("destroy");
        },


        _create: function () {
            var that = this;
            var options = this.options;
            this.variables = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer();
            this.plot = $('<div></div>').simulationplot({ colors: that.options.plot });
            this.plotDiv = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer({ content: this.plot, icon: "max", tabid: "SimulationPlot" });

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
        }

    });
} (jQuery));

interface JQuery {
    simulationviewer(): JQuery;
    simulationviewer(settings: Object): JQuery;
    simulationviewer(optionLiteral: string, optionName: string): any;
    simulationviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}