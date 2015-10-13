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
            var tablesContainer = $("<div></div>").addClass('ltl-simplot-container').appendTo(root);
            this._variables = $("<div></div>").addClass("small-simulation-popout-table").appendTo(tablesContainer);//root);
            this._table = $("<div></div>").addClass("big-simulation-popout-table").addClass("simulation-progression-table-container").appendTo(tablesContainer);//root);

            //var plotContainer = $("<div></div>").addClass("ltl-simplot-container").appendTo(root);
            this._plot = $("<div></div>").addClass("ltl-results").appendTo(root);

            var changeVisibility = function (params) {
                var visibility = that.options.visibleItems.slice(0);
                visibility[params.ind] = params.check;
                that._setOption("visibleItems", visibility);
            };

            this._variables.coloredtableviewer({
                onChangePlotVariables: changeVisibility
            });

            this.refresh();
        },

        _setOption: function (key, value) {
            var that = this;
            var needUpdate = false;
            this._super(key, value);
            switch (key) {

                case "data": {
                    //this.options.data = value;
                    //this.createPlotData();
                    needUpdate = true;
                    break;
                }
                case "init": {
                    //this.options.init = value;
                    //this.createPlotData();
                    needUpdate = true;
                    break;
                }
                case "interval": {
                    //this.optiopns.interval = value;
                    needUpdate = true;
                    break;
                }
                case "variables": {
                    //this.options.variables = value;
                    //this.createPlotData();
                    needUpdate = true;
                    break;
                }
                case "id": {
                    //this.options.id = value;
                    //this.createPlotData();
                    needUpdate = true;
                    break;
                }
                case "ranges": {
                    //this.options.ranges = value;
                    var variables = [];
                    if (this.options.ranges !== undefined && this.options.variables !== undefined) {
                        for (var i = 0; i < this.options.variables.length; i++) {
                            that.options.variables[i][3] = that.options.ranges[i].min;
                            that.options.variables[i][4] = that.options.ranges[i].max;
                        }
                        //this.createPlotData();
                        needUpdate = true;
                    }
                    break;
                }
                case "visibleItems": {
                    //this.options.visibleItems = value;
                    var variables = [];
                    if (this.options.visibleItems !== undefined && this.options.variables !== undefined) {
                        for (var i = 0; i < this.options.variables.length; i++)
                            this.options.variables[i][1] = this.options.visibleItems[i];  
                        //this.createPlotData();
                        needUpdate = true;    
                    }
                    break;
                }
                case "colors": {
                    //this.options.colors = value;
                    if (value !== undefined && value.length !== 0)
                        this._plot.simulationplot({
                            colors: value,
                        });
                    break;
                }
                default: break;
            }
            if (needUpdate) {
                //this.refresh();
                this.createPlotData();
            }
        },

        _setOptions: function (options) {
            this._super(options);
        },

        refresh: function () {
            var that = this;

            if (this.options.variables !== undefined && this.options.variables.length !== 0) {

                this._variables.coloredtableviewer({
                    header: ["Graph", "Name", "Range"],
                    type: "graph-max",
                    numericData: that.options.variables,
                });

                if (this.options.interval !== undefined && this.options.interval.length !== 0
                    && this.options.data !== undefined && this.options.data.length !== 0) {
                    this._table.progressiontable({
                        interval: that.options.interval,
                        data: that.options.data,
                        canEditInitialValue: false,
                        init: that.options.init
                    });
                    if (this.options.colors === undefined || this.options.colors.length == 0)
                        this.createPlotData();
                }
            }
        },

        createPlotData: function () {
            var that = this;
            var plotData = [];
            if (this.options.id === undefined && this.options.id.length == 0)
                this.options.id = [];
            if (this.options.ranges == undefined && this.options.ranges.length == 0)
                this.options.ranges = [];
            if (this.options.visibleItems == undefined && this.options.visibleItems.length == 0)
                this.options.visibleItems = [];

            for (var i = 0; i < this.options.variables.length; i++) {
                var pData = [];
                if (this.options.id.length < i + 1)
                    this.options.id.push(i);

                //for (var j = 0; j < this.options.init.length; j++)
                //    pData.push(this.options.init[j]);

                for (var j = 0; j < this.options.data.length; j++)
                    pData.push(this.options.data[j][i]);

                plotData.push({
                    Id: that.options.id[i],
                    Color: that.options.variables[i][0],
                    Seen: that.options.variables[i][1],
                    Plot: pData,
                    Init: that.options.init[i],
                    Name: that.options.variables[i][2],
                });

                if (this.options.ranges.length < i + 1)
                    this.options.ranges.push({
                        min: that.options.variables[i][3],
                        max: that.options.variables[i][4]
                    });
                if (this.options.visibleItems.length < i + 1)
                    this.options.visibleItems.push(that.options.variables[i][1]);
            }
            if (plotData !== undefined && plotData.length !== 0)
                this._plot.simulationplot({
                    colors: plotData,
                });
        },

    });
} (jQuery));

interface JQuery {
    ltlresultsviewer(): JQuery;
    ltlresultsviewer(settings: Object): JQuery;
    ltlresultsviewer(optionLiteral: string, optionName: string): any;
    ltlresultsviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 