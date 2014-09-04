/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.coloredtableviewer", {
        options: {
            header: [],
            numericData: undefined,
            colorData: undefined
        },
        _create: function () {
            this.refresh();
        },
        refresh: function () {
            this.element.empty();
            var that = this;
            var options = this.options;
            this.table = $('<table id="tableInColoredTableViewer"></table>');
            this.table.appendTo(that.element);
            this.createHeader(options.header);
            if (options.numericData !== undefined) {
                this.arrayToTable(options.numericData);
            }
            if (options.colorData !== undefined)
                this.table = this.paintTable(options.colorData);
            this.table.addClass("bma-prooftable");
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
            var table = that.table.clone();
            if (color.length > table.find("tr").length) {
                console.log("Incompatible sizes of numeric and color data");
                return;
            }
            ;

            for (var i = 0; i < color.length; i++) {
                if (color[i].length > table.find("tr").eq(i).children().length) {
                    console.log("Incompatible sizes of numeric and color data");
                    return;
                }
                ;

                for (var j = 0; j < color[i].length; j++) {
                    var td = table.find("tr").eq(i).children("td").eq(j);
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
//# sourceMappingURL=coloredtableviewer.js.map
