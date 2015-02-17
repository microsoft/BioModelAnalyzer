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
            step: 10
        },

        _create: function () {
            var that = this;
            var options = that.options;

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
            this.big_table.progressiontable();
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
                        data: options.data
                    });
                }
            }

            var step = this.options.step;

            var stepsul = $('<ul></ul>').addClass('button-list').appendTo(stepsdiv);
            var li0 = $('<li></li>').appendTo(stepsul);
            var li1 = $('<li></li>').addClass('steps').appendTo(stepsul);
            var li2 = $('<li></li>').appendTo(stepsul);
            var li3 = $('<li></li>').addClass('run').appendTo(stepsul);

            var add10 = $('<button></button>').text('+ ' + step).appendTo(li0);
            add10.bind("click", function () {
                that._setOption("num", that.options.num + step);
            });

            this.num = $('<button></button>').text('STEPS: ' + that.options.num).appendTo(li1);
            var min10 = $('<button></button>').text('- ' + step).appendTo(li2);
            min10.bind("click", function () {
                that._setOption("num", that.options.num - step);
            })
            this.RunButton = $('<button></button>').text('Run').appendTo(li3);
            this.refresh();
        },

        ChangeMode: function () {
            var that = this;
            switch (this.options.buttonMode) {
                case "ActiveMode":
                    this.RunButton.parent().removeClass('waiting');
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
                    this.RunButton.parent().addClass('waiting');
                    this.RunButton.text('');
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