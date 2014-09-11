/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.coloredtableviewer", {
        options: {
            header: [],
            numericData: undefined,
            colorData: undefined,
            type: "standart"
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
                        for (var i = 0; i < options.colorData.length; i++) {
                            var tr = $('<tr></tr>').appendTo(that.table);
                            for (var j = 0; j < options.colorData[i].length; j++) {
                                $('<td></td>').appendTo(tr);
                            }
                        }
                        this.paintTable(options.colorData);
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
                        this.table.addClass("bma-prooftable");
                        this.createHeader(options.header);
                        var tr0 = (that.table).find("tr").eq(0);
                        tr0.children("td").eq(0).attr("colspan", "2");
                        tr0.children("td").eq(2).attr("colspan", "2");

                        //var td01 = $('<td colspan="2"></td>').text("Graph").appendTo(tr0);
                        //var td02 = $('<td></td>').text("Name").appendTo(tr0);
                        //var td03 = $('<td colspan="2"></td>').text("Range").appendTo(tr0);
                        this.arrayToTableGraphMax(options.numericData);

                        if (options.colorData !== undefined)
                            this.paintTable(options.colorData);
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
            if (key === "header")
                this.options.header = value;
            if (key === "numericData")
                this.options.numericData = value;
            if (key === "colorData")
                this.options.colorData = value;

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

            for (var i = 0; i < that.array.length; i++) {
                var tr = $('<tr></tr>').appendTo(that.table);
                var td0 = $('<td></td>').appendTo(tr);
                var buttontd = $('<td></td>').appendTo(tr);
                if (array[i][0] !== undefined) {
                    td0.css("background-color", array[i][0]);
                    buttontd.addClass("addVariableToPlot");
                }

                //if (array[i][1] === true)
                buttontd.bind("click", function () {
                    buttontd.toggleClass("addVariableToPlot");
                    window.Commands.Execute("ChangePlotVariables", { ind: $(this).index(), check: buttontd.hasClass("addVariableToPlot") });
                });
                for (var j = 2; i < array[i].length; j++) {
                    $('<td></td>').text(array[i][j]).appendTo(tr);
                }
            }
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
            if (that.options.header !== undefined && that.options.header.length !== 0)
                over = 1;
            if (color.length > table.find("tr").length) {
                console.log("Incompatible sizes of numeric and color data");
                return;
            }
            ;

            for (var i = 0; i < color.length; i++) {
                if (color[i].length > table.find("tr").eq(i + over).children().length) {
                    console.log("Incompatible sizes of numeric and color data-2");
                    return;
                }
                ;

                for (var j = 0; j < color[i].length; j++) {
                    var td = table.find("tr").eq(i + over).children("td").eq(j);
                    if (color[i][j] !== undefined) {
                        if (color[i][j])
                            td.css("background-color", "#CCFF99");
                        else
                            td.css("background-color", "#FFADAD");
                    }
                }
            }
            return table;
        }
    });
}(jQuery));
