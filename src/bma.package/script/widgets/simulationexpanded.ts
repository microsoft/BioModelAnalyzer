// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationexpanded", {
        options: {
            data: undefined,
            init: undefined,
            interval: undefined,
            variables: undefined,
            num: 10,
            buttonMode: "ActiveMode",
            step: 10,
            onChangePlotVariables: undefined,
            createStateRequested: undefined,
        },

        _create: function () {
            var that = this;
            var options = that.options;

            var randomise = $('<div></div>')
                .addClass("randomise-button")
                .appendTo(that.element);
            var randomIcon = $('<div></div>').addClass("bma-random-icon2").appendTo(randomise);
            var randomLabel = $('<div></div>').text("Randomise").appendTo(randomise);

            var tables = $('<div></div>')
                .addClass("scrollable-results")
                .appendTo(this.element);
            this.small_table = $('<div></div>')
                .addClass('small-simulation-popout-table')
                .appendTo(tables);
            this.big_table = $('<div></div>')
                .addClass('big-simulation-popout-table')
                .appendTo(tables);

            var stepsdiv = $('<div></div>').addClass('steps-container').appendTo(that.element);

            var onContextMenuItemSelected = function (args) {
                if (args.command == "CreateState" && that.options.createStateRequested !== undefined)
                    that.options.createStateRequested(args);
            };

            this.big_table.progressiontable({
                columnContextMenuItems: [{ title: "Create LTL State", cmd: "CreateState" }],
                onContextMenuItemSelected: onContextMenuItemSelected
            });

            randomise.click(function () {
                that.big_table.progressiontable("Randomise");
            });

            if (options.variables !== undefined) {
                this.small_table.coloredtableviewer({
                    header: ["Graph", "Name", "Range"],
                    type: "graph-max",
                    numericData: that.options.variables
                });
                if (options.interval !== undefined && options.interval.length !== 0) {
                    this.big_table.progressiontable({
                        init: options.init,
                        interval: options.interval,
                        data: options.data, 
                    });
                }
            }

            if (options.onChangePlotVariables !== undefined) {
                this.small_table.coloredtableviewer({
                    onChangePlotVariables: options.onChangePlotVariables
                });
            }

            var step = this.options.step;

            var stepsul = $('<ul></ul>').addClass('button-list').appendTo(stepsdiv);
            var li = $('<li></li>').addClass('action-button-small grey').appendTo(stepsul);
            var li0 = $('<li></li>').appendTo(stepsul);
            var li1 = $('<li></li>').addClass('steps').appendTo(stepsul);
            var li2 = $('<li></li>').appendTo(stepsul);
            var li3 = $('<li></li>').addClass('action-button green').appendTo(stepsul);

            var exportCSV = $('<button></button>')
                .text('EXPORT CSV')
                .appendTo(li);
            exportCSV.bind('click', function () {
                window.Commands.Execute('ExportCSV', {});
            })
            var add10 = $('<button></button>').text('+ ' + step).appendTo(li2);
            add10.bind("click", function () {
                if (!li2.hasClass("disabled"))
                    that._setOption("num", that.options.num + step);
            });
            this.add = add10;

            this.num = $('<button></button>').text('STEPS: ' + that.options.num).appendTo(li1);
            var min10 = $('<button></button>').text('- ' + step).appendTo(li0);
            min10.bind("click", function () {
                if (!li0.hasClass("disabled"))
                    that._setOption("num", that.options.num - step);
            })
            this.min = min10;
            this.RunButton = $('<button></button>').addClass('run-button').text('Run').appendTo(li3);

            this.refresh();
        },

        
        ChangeMode: function () {
            var that = this;
            switch (this.options.buttonMode) {
                case "ActiveMode":
                    var li = this.RunButton.parent();
                    li.removeClass('waiting');
                    this.min.parent().removeClass("disabled");
                    this.add.parent().removeClass("disabled");
                    li.find('.spinner').detach();
                    this.RunButton.text('Run');
                    this.RunButton.bind("click", function () {
                        that.big_table.progressiontable("ClearData");
                        window.Commands.Execute("RunSimulation", {
                            data: that.big_table.progressiontable("GetInit"),
                            num: that.options.num
                        });
                    })
                    break;
                case "StandbyMode":
                    var li = this.RunButton.parent();
                    li.addClass('waiting');
                    this.min.parent().addClass("disabled");
                    this.add.parent().addClass("disabled");
                    this.RunButton.text('');
                    var snipper = $('<div class="spinner"></div>').appendTo(this.RunButton);
                    for (var i = 1; i < 4; i++) {
                        $('<div></div>').addClass('bounce' + i).appendTo(snipper);
                    }
        //                < div class="bounce1" > </div>
        //< div class="bounce2" > </div>
        //< div class="bounce3" > </div>
        
                    
                    this.RunButton.unbind("click");
                    break;
            }
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.variables !== undefined) {
                this.small_table.coloredtableviewer({
                    header: ["Graph", "Name", "Range"],
                    type: "graph-max",
                    numericData: that.options.variables
                });
                if (options.interval !== undefined && options.interval.length !== 0) {
                    this.big_table.progressiontable({ interval: options.interval, data: options.data });
                }
            }
            this.ChangeMode();
        },

        AddResult: function (res) {
            this.big_table.progressiontable("AddData", res);
        },

        getColors: function () {
            this.small_table.coloredtableviewer("GetColors");
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            var options = this.options;
            switch(key) {
                case "data":
                    this.options.data = value;
                    //if (value !== null && value !== undefined)
                    if (options.interval !== undefined && options.interval.length !== 0) {
                        this.big_table.progressiontable({ interval: options.interval, data: options.data });
                    }
                    break;
                case "init": 
                    this.options.init = value;
                    this.big_table.progressiontable({ init: value });
                    break;
                case "num":
                    if (value < 0) value = 0;
                    this.options.num = value;
                    this.num.text('STEPS: ' + value);
                    break;
                case "variables": 
                    this.options.variables = value;
                    this.small_table.coloredtableviewer({
                        header: ["Graph", "Name", "Range"],
                        type: "graph-max",
                        numericData: that.options.variables
                    });
                case "interval":
                    this.options.interval = value;
                    this.big_table.progressiontable({ interval: value });
                    break;
                case "buttonMode":
                    this.options.buttonMode = value;
                    this.ChangeMode();
                    break;
                case "onChangePlotVariables":
                    this.small_table.coloredtableviewer({
                        onChangePlotVariables: value
                    });
                    break;
                default:
                    break;
        }
            this._super(key, value);
            
        }
    });
} (jQuery));

interface JQuery {
    simulationexpanded(): JQuery;
    simulationexpanded(settings: Object): JQuery;
    simulationexpanded(settings: string): any;
    simulationexpanded(optionLiteral: string, optionName: string): any;
    simulationexpanded(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   
