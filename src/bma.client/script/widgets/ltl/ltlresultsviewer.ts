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
            colors: [],
            onExportCSV: undefined
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

            var stepsul = $('<ul></ul>').addClass('button-list').css("float", "left").appendTo(root);
            var li = $('<li></li>').addClass('action-button-small grey').appendTo(stepsul);

            var exportCSV = $('<button></button>')
                .text('EXPORT CSV')
                .appendTo(li);
            exportCSV.bind('click', function () {
                if (that.options.onExportCSV !== undefined) {
                    that.options.onExportCSV();
                }
            })

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
                    needUpdate = true;
                    break;
                }
                case "init": {
                    needUpdate = true;
                    break;
                }
                case "interval": {
                    needUpdate = true;
                    break;
                }
                case "variables": {
                    needUpdate = true;
                    break;
                }
                case "id": {
                    needUpdate = true;
                    break;
                }
                case "ranges": {
                    var variables = [];
                    if (this.options.ranges !== undefined && this.options.variables !== undefined) {
                        for (var i = 0; i < this.options.variables.length; i++) {
                            that.options.variables[i][3] = that.options.ranges[i].min;
                            that.options.variables[i][4] = that.options.ranges[i].max;
                        }
                        needUpdate = true;
                    }
                    break;
                }
                case "visibleItems": {
                    var variables = [];
                    if (this.options.visibleItems !== undefined && this.options.variables !== undefined) {
                        for (var i = 0; i < this.options.variables.length; i++)
                            this.options.variables[i][1] = this.options.visibleItems[i];  
                        needUpdate = true;    
                    }
                    break;
                }
                case "colors": {
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
                    var tags = [];
                    tags.push([]);
                    tags[0].push("A");
                    tags[0].push("B");
                    for (var i = 1; i < that.options.data.length / 3; i++) {
                        tags.push("A");
                    }
                    for (var i = that.options.data.length / 3; i < that.options.data.length *2/3; i++)
                        tags.push("B");
                    for (var i = that.options.data.length * 2 / 3; i < that.options.data.length; i++)
                        tags.push("A");
                    this._table.progressiontable({
                        interval: that.options.interval,
                        data: that.options.data,
                        tags: tags,
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

                pData.push(this.options.init[i]);
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