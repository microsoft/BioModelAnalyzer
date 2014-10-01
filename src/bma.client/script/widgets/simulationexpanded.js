﻿/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.simulationexpanded", {
        options: {
            data: undefined,
            init: undefined,
            interval: undefined,
            variables: undefined,
            num: 10
        },
        _create: function () {
            var that = this;
            var options = that.options;

            var RunButton = $('<div></div>').text("Run").addClass("bma-run-button").appendTo(that.element);

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
            });

            this.table1 = $('<div></div>').width("40%").appendTo(that.element);

            //this.table1.css("position", "relative");
            this.progression = $('<div></div>').appendTo(that.element).progressiontable(); //.addClass("bma-simulation-table")
            this.progression.css("position", "absolute");
            this.progression.css("left", "45%");
            this.progression.css("top", 0);
            if (options.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.variables });
                if (options.interval !== undefined && options.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.interval, init: options.init, data: options.data });
                }
            }

            RunButton.bind("click", function () {
                that.progression.progressiontable("ClearData");
                window.Commands.Execute("RunSimulation", { data: that.progression.progressiontable("GetInit"), num: that.options.num });
            });

            that.element.css("margin-top", "30px");
            that.element.css("margin-bottom", "40px");
            that.element.css("position", "relative");

            //that.element.children().css("margin", "10px");
            this.refresh();
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
            switch (key) {
                case "data":
                    this.options.data = value;
                    if (value !== null && value !== undefined)
                        if (options.interval !== undefined && options.interval.length !== 0) {
                            this.progression.progressiontable({ interval: options.interval, data: options.data });
                        }
                    break;
                case "init":
                    this.options.init = value;
                    this.progression.progressiontable({ init: value });
                    break;
                case "num":
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
            }
            this._super(key, value);
        }
    });
}(jQuery));
//# sourceMappingURL=simulationexpanded.js.map
