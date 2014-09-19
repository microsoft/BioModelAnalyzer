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
            //this.plotDiv.empty();
            //if (options.data === undefined) return;
            if (options.data !== undefined && options.data.variables !== undefined && options.data.variables.length !== 0) {
                var variablestable = $('<div></div>')
                    .appendTo(container)
                    .addClass("scrollable-results")
                    .coloredtableviewer({ header: ["Graph", "Cell", "Name", "Range"], type: "graph-min", numericData: options.data.variables });
                if (options.data.colorData !== undefined) {
                    var colortable = $('<div id="Simulation-min-table"></div>')
                        .appendTo(container)
                        .coloredtableviewer({ type: "simulation-min", colorData: options.data.colorData });
                }

                that.variables.resultswindowviewer({ header: "Variables", content: container, icon: "max", tabid: "SimulationVariables" });
            }
            else that.variables.resultswindowviewer("destroy");
            //that.options.plot = [];
            //that.options.plot[0] = [1, 2, 3, 4, 5];
            if (that.options.plot !== undefined) {
                var plot = $('<div></div>').simulationplot(that.options.plot );
                that.plotDiv.resultswindowviewer({ content: plot, icon: "max", tabid: "SimulationPlot" });
            }
        },


        _create: function () {
            var that = this;
            var options = this.options;

            //    var numericData = [];
            //    numericData[0] = ["rgb(255, 0, 0)", "C1", "name1", "0-1"];
            //    numericData[1] = [undefined, "C1", "name2", "1-5"];
            //    numericData[2] = ["rgb(0, 0, 0)", "C2", "name3", "3-6"];

            //    var colorData = [];
            //    colorData[0] = [true, false, true];
            //    colorData[1] = [false, false, false];
            //    colorData[2] = [true, true, true];

            //    var data = [];
            //    data[0] = [1, 0, 3, 5, 4, 2, 0];
            //    data[1] = [0, 2, 0, 1, 3, 0, 2];

            //    //options.variables = numericData;
            //    //options.colorData = colorData;
            //    options.data = {
            //        plot: data,
            //        variables: numericData,
            //        colorData: colorData
            //};
            this.variables = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer();
            //this.simulation = $('<div></div>')
            //    .appendTo(that.element);
            
            this.plotDiv = $('<div></div>')
                .appendTo(that.element)
                .resultswindowviewer();
                //.simulationplot();
            //$('<div>Plot should be here</div>').appendTo(that.element);
            //that._plot = InteractiveDataDisplay.asPlot(plotDiv);
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