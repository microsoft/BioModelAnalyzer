/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlresultsviewer", {
        _plot: undefined,
        _variables: undefined,
        _table: undefined,

        options: {
            data: [],
            init: [],
            variables: [],
            id: [],
            ranges: [],
            visibleItems: [],
            colors: []
        },

        _create: function () {
            var that = this;
            this.element.empty();
            this.element.addClass("ltlresultsviewer");

            var root = this.element;
            this._variables = $("<div></div>").addClass("small-simulation-popout-table").appendTo(root);
            this._table = $("<div></div>").addClass("big-simulation-popout-table").addClass("simulation-progression-table-container").appendTo(root);

            //var plotContainer = $("<div></div>").addClass("ltl-simplot-container").appendTo(root);
            this._plot = $("<div></div>").addClass("ltl-results").appendTo(root);

            if (this._variables !== undefined && this._variables.length !== 0) {
                this._variables.coloredtableviewer({
                    header: ["Graph", "Name", "Range"],
                    type: "graph-max",
                    numericData: that.options.variables
                });
                if (this.options.interval !== undefined && this.options.interval.length !== 0
                    && this.options.data !== undefined && this.options.data.length !== 0) {
                    this._table.progressiontable({
                        interval: that.options.interval,
                        data: that.options.data,
                        canEditInitialValue: false
                    });
                    this.createPlotData();
                }
            }
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {

                case "data": {
                    this.options.data = value;
                    if (this.options.interval !== undefined && this.options.interval.length !== 0
                        && this.options.data !== undefined && this.options.data.length !== 0) {
                        this._table.progressiontable({
                            interval: that.options.interval,
                            data: value,
                            canEditInitialValue: false
                        });
                        this.createPlotData();
                    }
                    break;
                }
                case "init": {
                    this.options.init = value;
                    if (this.options.interval !== undefined && this.options.interval.length !== 0
                        && this.options.data !== undefined && this.options.data.length !== 0)
                        this._table.progressiontable({
                            interval: that.options.interval,
                            data: that.options.data,
                            canEditInitialValue: false,
                            init: value
                        });
                    break;
                }
                case "variables": {
                    this.options.variables = value;
                    if (this._variables !== undefined && this._variables.length !== 0) {
                        this._variables.coloredtableviewer({
                            header: ["Graph", "Name", "Range"],
                            type: "graph-max",
                            numericData: value
                        });
                        
                        this.createPlotData();
                    }
                    break;
                }
                case "id": {
                    this.options.id = value;
                    break;
                }
                case "colors": {
                    this.options.colors = value;
                    if (this.options.colors !== undefined && this.options.colors.length !== 0)
                        this._plot.simulationplot({
                            colors: that.options.colors,
                        });
                    break;
                }
                default: break;
            }
            this._super(key, value);
            this.refresh();
        },

        _setOptions: function (options) {
            this._super(options);
        },

        refresh: function () {
            var that = this;

        },

        createPlotData: function () {
            var that = this;
            var plotData = [];
            for (var i = 0; i < this.options.variables.length; i++) {
                var pData = [];
                this.options.ranges = [];
                this.options.visibleItems = [];

                for (var j = 0; j < this.options.data.length; j++)
                    pData.push(this.options.data[j][i]);

                plotData.push({
                    Id: i,
                    Color: that.options.variables[i][0],
                    Seen: that.options.variables[i][1],
                    Plot: pData,
                    Init: that.options.init[i],
                    Name: that.options.variables[i][2],
                });

                this.options.ranges.push({
                    min: that.options.variables[i][3],
                    max: that.options.variables[i][4]
                });

                this.options.visibleItems.push(that.options.variables[i][1]);
            }
            this._setOption("colors", plotData);
        },

        //GetRandomInt: function (min, max) {
        //    return Math.floor(Math.random() * (max - min + 1) + min);
        //},

        //getRandomColor: function () {
        //    var r = this.GetRandomInt(0, 255);
        //    var g = this.GetRandomInt(0, 255);
        //    var b = this.GetRandomInt(0, 255);
        //    return "rgb(" + r + ", " + g + ", " + b + ")";
        //},

    });
} (jQuery));

interface JQuery {
    ltlresultsviewer(): JQuery;
    ltlresultsviewer(settings: Object): JQuery;
    ltlresultsviewer(optionLiteral: string, optionName: string): any;
    ltlresultsviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 