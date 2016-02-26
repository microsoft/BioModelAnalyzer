/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.coloredtableviewer", {
        options: {
            header: [],
            numericData: undefined,
            colorData: undefined,
            type: "standart", // "color","graph-min","graph-max", "simulation-min", "simulation-max"
            onChangePlotVariables: undefined,
        },

        _create: function () {

            this.refresh();
        },

        refresh: function () {
            this.element.empty();
            var that = this;
            var options = this.options;
            this.table = $('<table></table>');
            this.table.appendTo(that.element);

            
            switch (options.type) {

                case "standart":
                    if (options.numericData !== undefined && options.numericData !== null && options.numericData.length !== 0) {
                        this.table.addClass("variables-table");
                        this.createHeader(options.header);
                        this.arrayToTable(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;

                case "color":
                    this.table.addClass("proof-propagation-overview");
                    if (options.colorData !== undefined && options.colorData.length !== 0) {
                        var that = this;
                        var color = options.colorData;
                        for (var i = 0; i < color.length; i++) {
                            var tr = $('<tr></tr>').appendTo(that.table);
                            for (var j = 0; j < color[i].length; j++) {
                                var td = $('<td></td>').appendTo(tr);
                                if (color[i][j] !== undefined) {
                                    if (color[i][j]) td.addClass('propagation-cell-green');
                                    else td.addClass('propagation-cell-red');
                                }
                            }
                        }
                    }
                    break;

                case "graph-min":
                    if (options.numericData !== undefined && options.numericData !== null && options.numericData.length !== 0) {
                        this.table.addClass("variables-table");
                        this.createHeader(options.header);
                        this.arrayToTableGraphMin(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;

                case "graph-max":
                    if (options.numericData !== undefined && options.numericData !== null && options.numericData.length !== 0) {
                        this.table.addClass("variables-table");
                        this.createHeader(options.header);
                        var tr0 = that.table.find("tr").eq(0);
                        tr0.children("td").eq(0).attr("colspan", "2");
                        tr0.children("td").eq(2).attr("colspan", "2");
                        this.arrayToTableGraphMax(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;
                case "simulation-min":
                    this.table.addClass("proof-propagation-overview");
                    if (options.colorData !== undefined && options.colorData.length !== 0) {
                        var that = this;
                        var color = options.colorData;
                        for (var i = 0; i < color.length; i++) {
                            var tr = $('<tr></tr>').appendTo(that.table);
                            for (var j = 0; j < color[i].length; j++) {
                                var td = $('<td></td>').appendTo(tr);
                                if (color[i][j]) {
                                    td.addClass('change');//.css("background-color", "#FFF729"); //no guide
                                }
                            }
                        }
                    }
                    break;
            }
            
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "header") this.options.header = value;
            if (key === "numericData") this.options.numericData = value;
            if (key === "colorData") {
                this.options.colorData = value;
                if (this.options.colorData !== undefined) {
                    this.paintTable(this.options.colorData);
                    return;
                }
            }
            if (key === "onChangePlotVariables") this.options.onChangePlotVariables = value;

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
        },

        createHeader: function (header) {
            var that = this;
            var tr = $('<tr></tr>').appendTo(that.table);
            for (var i = 0; i < header.length; i++) {
                $('<td></td>').text(header[i]).appendTo(tr);
            }
        },

        arrayToTableGraphMin: function (array) {
            var that = this;
            for (var i = 0; i < array.length; i++) {
                var tr = $('<tr></tr>').appendTo(that.table);
                var td0 = $('<td></td>').appendTo(tr);
                if (array[i][0] !== undefined)
                    td0.css("background-color", array[i][0]);
                for (var j = 1; j < array[i].length; j++) {
                    $('<td></td>').text(array[i][j]).appendTo(tr);
                }
            }
        },

        arrayToTableGraphMax: function (array) {
            var that = this;
            var vars = this.options.variables;

            for (var i = 0; i < array.length; i++) {

                var tr = $('<tr></tr>').appendTo(that.table);
                var td0 = $('<td></td>').appendTo(tr);
                var buttontd = $('<td></td>').appendTo(tr);
                if (array[i][1] && array[i][0] !== undefined) {
                    td0.css("background-color", array[i][0]);
                    buttontd.addClass("plot-check");
                }

                buttontd.bind("click", function () {
                    $(this).toggleClass("plot-check");
                    var check = $(this).hasClass("plot-check");
                    if (check) {
                        $(this).prev().css("background-color", array[$(this).parent().index() - 1][0]);
                        that.alldiv.attr("checked", that.checkAllButtons());

                    }
                    else {
                        that.alldiv.attr("checked", false);
                        $(this).prev().css("background-color", "transparent");
                    }

                    //window.Commands.Execute("ChangePlotVariables", { ind: $(this).parent().index() - 1, check: check });
                    if (that.options.onChangePlotVariables !== undefined)
                        that.options.onChangePlotVariables({ ind: $(this).parent().index() - 1, check: check });
                });

                for (var j = 2; j < array[i].length; j++) {
                    $('<td></td>').text(array[i][j]).appendTo(tr);
                }
            }
            this.buttons = that.table.find("tr").not(":first-child").find("td:nth-child(2)");
            var alltr = $('<tr></tr>').appendTo(that.table);
            var tdall0 = $('<td></td>').appendTo(alltr);
            this.allcheck = $('<td id="allcheck"></td>').appendTo(alltr).addClass("plot-check");
            var tdall1 = $('<td></td>').appendTo(alltr);
            this.alldiv = $('<div></div>').attr("checked", that.checkAllButtons()).text("ALL").appendTo(tdall1);

            tdall1.css("border-left", "none");


            this.allcheck.bind("click",() => {
                that.alldiv.attr("checked", !that.alldiv.attr("checked"));

                if (that.alldiv.attr("checked")) {
                    for (var i = 0; i < that.buttons.length; i++) {
                        if (!$(that.buttons[i]).hasClass("plot-check"))
                            $(that.buttons[i]).click();
                    }
                }
                else {
                    for (var i = 0; i < that.buttons.length; i++) {
                        if ($(that.buttons[i]).hasClass("plot-check"))
                            $(that.buttons[i]).click();
                    }
                }
            })
        },

        getColors: function () {
            var that = this;
            if (this.options.type === "graph-max") {
                var tds = this.table.find("tr:not(:first-child)").children("td: nth-child(2)");
                var data = [];
                tds.each(function (ind, val) {
                    if ($(this).hasClass("plot-check"))
                        data[ind] = that.options.data.variables[ind].color;
                })
            }
        },

        checkAllButtons: function (): boolean {
            var that = this;
            var l = that.buttons.length;
            for (var i = 0; i < l; i++) {
                if (!that.buttons.eq(i).hasClass("plot-check"))
                    return false;
            }
            return true;
        },

        getAllButton: function () {
            if (this.allcheck !== undefined)
                return this.allcheck;
        },

        arrayToTable: function (array) {
            var that = this;
            for (var i = 0; i < array.length; i++) {
                var tr = $('<tr></tr>').appendTo(that.table);
                for (var j = 0; j < array[i].length; j++) {
                    $('<td></td>').text(array[i][j]).appendTo(tr);
                }
            }
        },

        paintTable: function (color) {
            var that = this;
            var table = that.table;
            var over = 0;
            if (that.options.header !== undefined && that.options.header.length !== 0) over = 1;

            for (var i = 0; i < color.length; i++) {
                var tds = table.find("tr").eq(i + over).children("td");
                if (color[i].length > tds.length) { console.log("Incompatible sizes of numeric and color data-2"); return };

                for (var j = 0; j < color[i].length; j++) {
                    var td = tds.eq(j);
                    if (color[i][j] !== undefined) {
                        if (color[i][j]) td.addClass('propagation-cell-green');
                        else td.addClass('propagation-cell-red');
                    }
                }
            }
            return table;
        }
    });
} (jQuery));

interface JQuery {
    coloredtableviewer(): JQuery;
    coloredtableviewer(settings: Object): JQuery;
    coloredtableviewer(settings: string): any;
    coloredtableviewer(optionLiteral: string, optionName: string): any;
    coloredtableviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}  