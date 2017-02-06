// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlresultsviewer", {
        _plot: undefined,
        _variables: undefined,
        _table: undefined,
        tablesContainer: undefined,
        loading: undefined,
        scrollBarSize: undefined,

        options: {
            data: [],
            init: [],
            variables: [],
            id: [],
            ranges: [],
            visibleItems: [],
            colors: [],
            onExportCSV: undefined,
            createStateRequested: undefined,
            columnContextMenuItems: undefined
        },

        _create: function () {
            var that = this;
            this.element.empty();
            this.element.addClass("ltlresultsviewer");

            var root = this.element;
            this.tablesContainer = $("<div></div>").addClass('ltl-simplot-container').appendTo(root);
            this._variables = $("<div></div>").addClass("small-simulation-popout-table").appendTo(this.tablesContainer);
            this._table = $("<div></div>").addClass("big-simulation-popout-table").addClass("simulation-progression-table-container").appendTo(this.tablesContainer);
            
            this.scrollBarSize = BMA.ModelHelper.GetScrollBarSize();            

            this._table.on('scroll', function () {
                that._variables.scrollTop($(this).scrollTop());
            });

            this._variables.css("max-height", 322 - that.scrollBarSize.height);
            
            this._plot = $("<div></div>").addClass("ltl-results").appendTo(root);
            this.loading = $("<div></div>").addClass("page-loading").css("position", "inherit").css("height", 322).appendTo(this._plot);
            var loadingText = $("<div> Loading </div>").addClass("loading-text").appendTo(this.loading);
            var snipper = $('<div></div>').addClass('spinner').appendTo(loadingText);
            for (var i = 1; i < 4; i++) {
                $('<div></div>').addClass('bounce' + i).appendTo(snipper);
            }

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
                if (that._plot !== undefined) {
                    that._plot.simulationplot("ChangeVisibility", params.ind, params.check);
                }
                if (that.options.visibleItems !== undefined && that.options.visibleItems.length != 0)
                    that.options.visibleItems[params.ind] = params.check;
                if (that.options.variables !== undefined && that.options.variables.length != 0)
                    that.options.variables[params.ind][1] = params.check;
            };

            this._variables.coloredtableviewer({
                onChangePlotVariables: changeVisibility
            });

            var onContextMenuItemSelected = function (args) {
                if (that.options.data !== undefined && that.options.data.length !== 0) {
                    var columnData = [];
                    for (var i = 0; i < that.options.data[args.column].length; i++) {
                        columnData.push({
                            variable: that.options.variables[i][3],
                            variableId: that.options.id[i],
                            value: that.options.data[args.column][i]
                        });
                    }

                    if (args.command == "CreateState" && that.options.createStateRequested !== undefined)
                        that.options.createStateRequested(columnData);
                }
            };

            this._table.progressiontable({
                canEditInitialValue: false,
                showInitialValue: false,
                onContextMenuItemSelected: onContextMenuItemSelected
            });
            
            var after = $("<div></div>").css("height", 23).css("width", "100%").appendTo(this._table);

            this.refresh();
        },

        _setOption: function (key, value) {
            var that = this;
            var needUpdate = false;
            this._super(key, value);
            switch (key) {

                case "tags": {
                    //needUpdate = true;
                    if (that._table !== undefined)
                        that._table.progressiontable({ tags: value });
                    break;
                }
                case "labels": {
                    if (that._plot !== undefined)
                        this._plot.simulationplot({ labels: value });
                    break;
                }
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
                            that.options.variables[i][4] = that.options.ranges[i].min;
                            that.options.variables[i][5] = that.options.ranges[i].max;
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

                case "columnContextMenuItems": {
                    needUpdate = false;
                    this._table.progressiontable({
                        columnContextMenuItems: that.options.columnContextMenuItems,
                    });
                    break;
                }
                default: break;
            }
            if (needUpdate) {
                this.refresh();
            }
        },

        _setOptions: function (options) {
            var that = this;
            var needRefresh = false;
            $.each(options, function (key, value) {
                switch (key) {
                    case "tags": {
                        needRefresh = true;
                        that.options.tags = value;
                        break;
                    }
                    case "labels": {
                        needRefresh = true;
                        that.options.labels = value;
                        break;
                    }
                    case "data": {
                        needRefresh = true;
                        that.options.data = value;
                        break;
                    }
                    case "init": {
                        needRefresh = true;
                        that.options.init = value;
                        break;
                    }
                    case "interval": {
                        needRefresh = true;
                        that.options.interval = value;
                        break;
                    }
                    case "variables": {
                        needRefresh = true;
                        that.options.variables = value;
                        break;
                    }
                    case "id": {
                        needRefresh = true;
                        that.options.id = value;
                        break;
                    }
                    default: that._setOption(key, value);
                        break;
                }
            });
            if (needRefresh)
                this.refresh();
        },
        
        

        refresh: function () {
            var that = this;

            if (this.options.variables !== undefined && this.options.variables.length !== 0) {

                this._variables.coloredtableviewer({
                    header: ["Graph", "Cell", "Name", "Range"],
                    type: "graph-all",
                    numericData: that.options.variables,
                });

                if (this.options.interval !== undefined && this.options.interval.length !== 0
                    && this.options.data !== undefined && this.options.data.length !== 0
                    && this.options.tags !== undefined && this.options.tags.length !== 0) {

                    this._table.progressiontable({
                        interval: that.options.interval,
                        data: that.options.data,
                        tags: that.options.tags,
                        init: that.options.init,
                    });

                    var width = this._table.children().eq(1).width();

                    if (width + that.scrollBarSize.width < 160) {
                        this._variables.css("max-height", 322);
                    } else {
                        this._variables.css("max-height", 322 - that.scrollBarSize.height);
                    }
                    
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
            //if (this.options.ranges == undefined && this.options.ranges.length == 0)
                //this.options.ranges = [];
            //if (this.options.visibleItems == undefined && this.options.visibleItems.length == 0)
                //this.options.visibleItems = [];

            for (var i = 0; i < this.options.variables.length; i++) {
                var pData = [];
                if (this.options.id.length < i + 1)
                    this.options.id.push(i);

                pData.push(this.options.init[i]);
                for (var j = 1; j < this.options.data.length; j++)
                    pData.push(this.options.data[j][i]);

                var name = that.options.variables[i][2] + (that.options.variables[i][2] ? "." : "") + that.options.variables[i][3];

                plotData.push({
                    Id: that.options.id[i],
                    Color: that.options.variables[i][0],
                    Seen: that.options.variables[i][1],
                    Plot: pData,
                    Init: that.options.init[i],
                    Name: name,
                });

                //if (this.options.ranges.length < i + 1)
                    //this.options.ranges.push({
                    //    min: that.options.variables[i][3],
                    //    max: that.options.variables[i][4]
                    //});
                //if (this.options.visibleItems.length < i + 1)
                    //this.options.visibleItems.push(that.options.variables[i][1]);
            }
            if (plotData !== undefined && plotData.length !== 0) {
                this.loading.show();
                setTimeout(function () {
                    that._plot.simulationplot({
                        colors: plotData,
                        labels: that.options.labels
                    });
                    that.loading.hide();
                },1000);
            }
        },

    });
} (jQuery));

interface JQuery {
    ltlresultsviewer(): JQuery;
    ltlresultsviewer(settings: Object): JQuery;
    ltlresultsviewer(optionLiteral: string, optionName: string): any;
    ltlresultsviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 
