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
            buttonMode: "ActiveMode"
        },

        _create: function () {
            var that = this;
            var options = that.options;
           
            this.RunButton = $('<div></div>').text("Run").addClass("bma-run-button").appendTo(that.element);

            var tablesDiv = $('<div></div>')
                .addClass("bma-simulation-table-expanded-scroll")
                .appendTo(this.element);
            this.table1 = $('<div></div>').appendTo(tablesDiv);
            this.progression = $('<div></div>').appendTo(tablesDiv).progressiontable();//.addClass("bma-simulation-table")
            this.progression.css("width", "calc(100% - 361px - 60px)");
            if (options.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.variables });
                if (options.interval !== undefined && options.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.interval, init: options.init, data: options.data });

                }
            }

            var steps = $('<div class="steps-setting"></div>').appendTo(that.element);
            this.num = $('<span></span>').text(that.options.num).appendTo(steps);
            $('<span></span>').text("Steps").appendTo(steps);
            var add10 = $('<button></button>').text('+ 10').appendTo(steps);
            var min10 = $('<button></button>').text('- 10').appendTo(steps);
            add10.bind("click", function () {
                that._setOption("num", that.options.num + 10);
            });
            min10.bind("click", function () {
                that._setOption("num", that.options.num - 10);
            })

            that.element.addClass("bma-simulation-expanded");
            this.refresh();
        },

        ChangeMode: function () {
            var that = this;
            var toAddClass = "", toRemoveClass = "", text = "";
            switch (this.options.buttonMode) {
                case "ActiveMode":
                    toAddClass = "bma-run-button";
                    toRemoveClass = "bma-run-button-waiting";
                    text = "Run";
                    this.RunButton.bind("click", function () {
                        that.progression.progressiontable("ClearData");
                        window.Commands.Execute("RunSimulation", { data: that.progression.progressiontable("GetInit"), num: that.options.num });
                    })
                    break;
                case "StandbyMode":
                    toAddClass = "bma-run-button-waiting";
                    toRemoveClass = "bma-run-button";
                    this.RunButton.unbind("click");
                    break;
            }
            this.RunButton.removeClass(toRemoveClass).addClass(toAddClass).text(text);
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.variables });
                if (options.interval !== undefined && options.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.interval, data: options.data });
                }
            }
            this.ChangeMode();
        },

        AddResult: function (res) {
            this.progression.progressiontable("AddData", res);
        },

        getColors: function () {
            this.table1.coloredtableviewer("GetColors");
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
                            this.progression.progressiontable({ interval: options.interval, data: options.data });
                        }
                    break;
                case "init": 
                    this.options.init = value;
                    this.progression.progressiontable({ init: value });
                    break;
                case "num":
                    if (value < 0) value = 0;
                    this.options.num = value;
                    this.num.text(value);
                    break;
                case "variables": 
                    this.options.variables = value;
                    this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.variables });
                case "interval":
                    this.options.interval = value;
                    this.progression.progressiontable({ interval: value });
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