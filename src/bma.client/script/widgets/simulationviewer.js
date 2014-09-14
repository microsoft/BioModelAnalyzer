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
            var container = $('<div></div>');

            //this.variables.resultswindowviewer("destroy");
            //this.plotDiv.empty();
            if (options.data === undefined)
                return;
            if (options.data.variables !== undefined && options.data.variables.length !== 0) {
                var variablestable = $('<div></div>').appendTo(container).addClass("scrollable-results").coloredtableviewer({ header: ["Graph", "Cell", "Name", "Range"], type: "graph-min", numericData: options.data.variables });
                if (options.data.colorData !== undefined) {
                    var colortable = $('<div></div>').appendTo(container).coloredtableviewer({ type: "simulation-min", colorData: options.data.colorData });
                }

                that.variables.resultswindowviewer({ header: "Variables", content: container, icon: "max", tabid: "SimulationVariables" });
            }

            //else this.variables.empty();
            if (that.options.data.plot !== undefined) {
                var plot = $('<div></div>').simulationplot({ data: that.options.data.plot });
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
            this.variables = $('<div></div>').appendTo(that.element).resultswindowviewer();

            //this.simulation = $('<div></div>')
            //    .appendTo(that.element);
            this.plotDiv = $('<div></div>').appendTo(that.element);

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
            this._super(key, value);
            this.refresh();
        },
        show: function (tab) {
            if (tab === "SimulationVariables") {
                this.variables.show();
            }
            if (tab === "SimulationPlot") {
                this.plotDiv.show();
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
}(jQuery));
//# sourceMappingURL=simulationviewer.js.map
