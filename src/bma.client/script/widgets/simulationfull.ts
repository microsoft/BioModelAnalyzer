/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationfull", {
        options: {
            data: undefined,
            init: undefined
        },

        _create: function () {
            var that = this;
            var options = that.options;
           
            var RunButton = $('<div></div>').addClass("bma-run-button").appendTo(that.element);
            RunButton.bind("click", function () {
                window.Commands.Execute("RunSimulation", {data: that.progression.progressiontable("getLast"), num: 10 });
            })

            this.table1 = $('<div></div>').width("40%").appendTo(that.element);
            this.progression = $('<div></div>').addClass("bma-simulation-table").appendTo(that.element);
            if (options.data !== undefined && options.data.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.data.variables });
                if (options.data.interval !== undefined && options.data.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.data.interval });
                }
            }



            that.element.css("display", "flex");
            that.element.children().css("margin", "10px");
            this.refresh();
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.data !== undefined && options.data.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.data.variables });
                if (options.data.interval !== undefined && options.data.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.data.interval });
                }
            }
        },


        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch(key) {
                case "data":
                    this.options.data = value;
                    if (value !== null && value !== undefined)
                        this.refresh();
                    break;
                case "init": 
                    this.options.init = value;
                    this.progression.progressiontable({ init: value });
                    break;

        }
            this._super(key, value);
            
        }
    });
} (jQuery));

interface JQuery {
    simulationfull(): JQuery;
    simulationfull(settings: Object): JQuery;
    simulationfull(settings: string): any;
    simulationfull(optionLiteral: string, optionName: string): any;
    simulationfull(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   