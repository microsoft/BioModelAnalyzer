/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.simulationfull", {
        options: {
            data: undefined,
            init: undefined,
            num: 10
        },
        _create: function () {
            var that = this;
            var options = that.options;

            var RunButton = $('<div></div>').text("Run").addClass("bma-run-button").appendTo(that.element);
            RunButton.bind("click", function () {
                that.progression.progressiontable("clearData");
                window.Commands.Execute("RunSimulation", { data: that.progression.progressiontable("getInit"), num: that.options.num });
            });

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

            //this.table1.css("display", "inline-block");
            this.progression = $('<div></div>').appendTo(that.element); //.addClass("bma-simulation-table")
            this.progression.css("position", "absolute");
            this.progression.css("left", "45%");
            this.progression.css("top", "75px");
            if (options.data !== undefined && options.data.variables !== undefined) {
                this.table1.coloredtableviewer({ header: ["Graph", "Name", "Range"], type: "graph-max", numericData: that.options.data.variables });
                if (options.data.interval !== undefined && options.data.interval.length !== 0) {
                    this.progression.progressiontable({ interval: options.data.interval });
                }
            }

            that.element.css("margin-top", "20px");

            //that.element.children().css("margin", "10px");
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
            switch (key) {
                case "data":
                    this.options.data = value;
                    if (value !== null && value !== undefined)
                        this.refresh();
                    break;
                case "init":
                    this.options.init = value;
                    this.progression.progressiontable({ init: value });
                    break;
                case "num":
                    this.options.num = value;
                    this.num.text(value);
                    break;
            }
            this._super(key, value);
        }
    });
}(jQuery));
//# sourceMappingURL=simulationfull.js.map
