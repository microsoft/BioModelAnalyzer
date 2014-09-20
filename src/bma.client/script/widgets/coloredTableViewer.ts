/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.coloredtableviewer", {
        options: {
            header: [],
            numericData: undefined,
            colorData: undefined,
            type: "standart" // "color","graph-min","graph-max", "simulation-min", "simulation-max"
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
                        this.table.addClass("bma-prooftable");
                        this.createHeader(options.header);
                        this.arrayToTable(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;
                case "color":

                    if (options.colorData !== undefined && options.colorData.length !== 0) {
                        var that = this;
                        var color = options.colorData;
                        for (var i = 0; i < color.length; i++) {
                            var tr = $('<tr></tr>').appendTo(that.table);
                            for (var j = 0; j < color[i].length; j++) {
                                var td = $('<td></td>').appendTo(tr);
                                if (color[i][j] !== undefined) {
                                    if (color[i][j]) td.css("background-color", "#CCFF99");
                                    else td.css("background-color", "#FFADAD");
                                }
                            }
                        }

                        this.table.addClass("bma-color-prooftable");

                    }
                    break;

                case "graph-min":
                    if (options.numericData !== undefined && options.numericData !== null && options.numericData.length !== 0) {
                        this.table.addClass("bma-prooftable");
                        this.createHeader(options.header);
                        this.arrayToTableGraphMin(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;

                case "graph-max":
                    if (options.numericData !== undefined && options.numericData !== null && options.numericData.length !== 0) {
                        this.table.addClass("bma-graph-max-table");
                        this.createHeader(options.header);
                        var tr0 = that.table.find("tr").eq(0);
                        tr0.children("td").eq(0).attr("colspan", "2");
                        tr0.children("td").eq(2).attr("colspan", "2");
                        //var tr1 = that.table.find("tr").eq(1);
                        //tr1.children("td").eq(2).bind("click", () => {
                        //    alert("fuck");
                        //})
                        //tr1.children("td").eq(3).attr("display", "none");//.width(35).css("background-color", "green");
                        //tr1.children("td").eq(4).attr("display", "none");//.width(35).css("background-color", "green");
                        //var td01 = $('<td colspan="2"></td>').text("Graph").appendTo(tr0);
                        //var td02 = $('<td></td>').text("Name").appendTo(tr0);
                        //var td03 = $('<td colspan="2"></td>').text("Range").appendTo(tr0);
                        this.arrayToTableGraphMax(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
                    }
                    break;
                case "simulation-min":
                    this.table.addClass("bma-color-prooftable bma-color-simulationtable");
                    if (options.colorData !== undefined && options.colorData.length !== 0) {
                        var that = this;
                        var color = options.colorData;
                        for (var i = 0; i < color.length; i++) {
                            var tr = $('<tr></tr>').appendTo(that.table);
                            for (var j = 0; j < color[i].length; j++) {
                                var td = $('<td></td>').appendTo(tr);
                                if (color[i][j]) {
                                    td.css("background-color", "#FFF729");
                                }
                            }
                        }
                        
                        //this.paintSimulationTable(options.colorData);

                    }
                    break;

                default:
                    alert("undefined type of table");
            }
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "header") this.options.header = value;
            if (key === "numericData") this.options.numericData = value;
            if (key === "colorData") this.options.colorData = value;

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
                    buttontd.addClass("addVariableToPlot");
                }

                buttontd.bind("click", function () {
                    $(this).toggleClass("addVariableToPlot");
                    var check = $(this).hasClass("addVariableToPlot");
                    if (check) {
                        $(this).prev().css("background-color", array[$(this).parent().index() - 1][0]);
                        that.alldiv.attr("checked", that.checkAllButtons());

                    }
                    else {
                        that.alldiv.attr("checked", false);
                        $(this).prev().css("background-color", "transparent");
                    }
                    window.Commands.Execute("ChangePlotVariables", { ind: $(this).parent().index()-1, check: check });
                })

                for (var j = 2; j < array[i].length; j++) {
                    $('<td></td>').text(array[i][j]).appendTo(tr);
                }
            }
            this.buttons = that.table.find("tr").not(":first-child").find("td:eq(1)");
            var alltr = $('<tr></tr>').appendTo(that.table);
            var tdall0 = $('<td></td>').appendTo(alltr).css("border", "none");
            tdall0.css("background-color", "white");
            this.allcheck = $('<td id="allcheck"></td>').appendTo(alltr).addClass("addVariableToPlot")
            this.allcheck.css("border-right", "none");
            var tdall1 = $('<td></td>').appendTo(alltr);
            this.alldiv = $('<div></div>').attr("checked", that.checkAllButtons()).text("ALL").appendTo(tdall1);

            tdall1.css("border-left", "none");
            

            this.allcheck.bind("click", () => {
                that.alldiv.attr("checked", !that.alldiv.attr("checked"));
                
                if (that.alldiv.attr("checked")) {
                    that.buttons.each( function () {
                        if (!$(this).hasClass("addVariableToPlot"))
                            $(this).click();
                    })
                }
                else {
                    that.buttons.each(function () {
                        if ($(this).hasClass("addVariableToPlot"))
                            $(this).click();
                    })
                }
            })


        },

        getColors: function () {
            var that = this;
            if (this.options.type === "graph-max") {
                var tds = this.table.find("tr:not(:first-child)").children("td: nth-child(2)");
                var data = [];
                tds.each(function (ind, val) {
                    if ($(this).hasClass("addVariableToPlot"))
                        data[ind] = that.options.data.variables[ind].color;
                })
            }
        },

        getRandomColor: function () {
            var letters = '0123456789ABCDEF'.split('');
            var color = '#';
            for (var i = 0; i < 6; i++) {
                color += letters[Math.floor(Math.random() * 16)];
            }
            return color;
        },

        checkAllButtons: function (): boolean {
            var that = this;
            var l = that.buttons.length;
            for (var i = 0; i < l; i++) {
                if (!that.buttons.eq(i).hasClass("addVariableToPlot"))
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

        //paintSimulationTable: function (color) {
        //    var that = this;
        //    var table = that.table;//.clone();
        //    var over = 0;
        //    if (that.options.header !== undefined && that.options.header.length !== 0) over = 1;
        //    if (color.length > table.find("tr").length) { console.log("Incompatible sizes of numeric and color data"); return };

        //    for (var i = 0; i < color.length; i++) {
        //        if (color[i].length > table.find("tr").eq(i + over).children().length) { console.log("Incompatible sizes of numeric and color data-2"); return };

        //        for (var j = 0; j < color[i].length; j++) {

        //            var td = table.find("tr").eq(i + over).children("td").eq(j);
        //            if (color[i][j] !== undefined) {
        //                if (color[i][j]) td.css("background-color", "#CCFF99");
        //                else td.css("background-color", "#FFADAD");
        //            }
        //        }
        //    }
        //    return table;
        //},

        paintTable: function (color) {
            var that = this;
            var table = that.table;//.clone();
            var over = 0;
            if (that.options.header !== undefined && that.options.header.length !== 0) over = 1;
            if (color.length > table.find("tr").length) { console.log("Incompatible sizes of numeric and color data"); return };

            for (var i = 0; i < color.length; i++) {
                if (color[i].length > table.find("tr").eq(i+over).children().length) { console.log("Incompatible sizes of numeric and color data-2"); return };

                for (var j = 0; j < color[i].length; j++) {
                    
                    var td = table.find("tr").eq(i+over).children("td").eq(j);
                     if (color[i][j] !== undefined) {
                         if (color[i][j]) td.css("background-color", "#CCFF99");
                        else td.css("background-color", "#FFADAD");
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