/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.coloredtableviewer", {
        options: {
            numericData: undefined,
            colorData: undefined
        },

        _init: function () {
            this.element.empty();
            
            var that = this;
            var options = this.options;
            if (options.numericData === undefined) {
                console.log("numericData undefined");
                return;
            }
            this.table = this.arrayToTable(options.numericData);
            if (options.colorData !== undefined)
                this.table = this.paintTable(options.colorData);
            this.table
                .addClass("bma-prooftable")
                .appendTo(this.element);
        },
        
        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            this._super(key, value);
            this.refresh();
        },

        arrayToTable: function (array) {
            var table = $('<table></table>');
            for (var i = 0; i < array.length; i++) {
                var tr = $('<tr></tr>').appendTo(table);
                for (var j = 0; j < array[i].length; j++) {
                    $('<td>' + array[i][j] + '</td>').appendTo(tr);
                }
            }
            return table;
        },

        paintTable: function (color) {
            var that = this;
            var table = that.table.clone();
            if (color.length > table.find("tr").length) { console.log("Incompatible sizes of numeric and color data"); return };

            for (var i = 0; i < color.length; i++) {
                if (color[i].length > table.find("tr").eq(i).children().length) { console.log("Incompatible sizes of numeric and color data"); return };

                 for (var j = 0; j < color[i].length; j++) {
                    var td = table.find("tr").eq(i).children("td").eq(j);
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
    coloredtableviewer(optionLiteral: string, optionName: string): any;
    coloredtableviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}  