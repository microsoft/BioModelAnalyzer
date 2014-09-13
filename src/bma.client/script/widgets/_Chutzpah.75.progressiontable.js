/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.progressiontable", {
        options: {
            interval: undefined,
            data: undefined,
            header: "Initial Value"
        },
        _create: function () {
            var that = this;
            var options = that.options;

            this.table = $('<table></table>').addClass("bma-prooftable").appendTo(that.element);

            //var numericData = [];
            //numericData[0] = ["rgb(255, 0, 0)", "name1", 0, 1];
            //numericData[1] = [undefined, "name2", 1, 5];
            //numericData[2] = ["rgb(0, 0, 0)", "name3", 3, 6];
            //options.data = {
            //    variables: numericData
            //}
            //if (options.data !== undefined && options.data.variables !== undefined) {
            //    var table1 = $('<div></div>').coloredtableviewer({ header: header, type: "graph-max", numericData: that.options.data.variables });
            //    table1.addClass("popup-window").show().appendTo(that.element);
            //}
            //that.element.appendTo($('body'))
            this.refresh();
        },
        refresh: function () {
            var that = this;
            var options = this.options;

            //if (options.data !== undefined) {
            //    for (var i = 0; i < options.data.length; i++) {
            //    }
            //}
            var tr0 = $('<tr></tr>').appendTo(that.table);

            if (that.options.header !== undefined)
                $('<td></td>').text(that.options.header).appendTo(tr0);

            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(that.table);
                    var td = $('<td></td>').text(that.options.interval[i][0]).appendTo(tr);
                    var random = $('<div></div>').addClass("bma-random-icon1").appentTo(td);
                    random.bind("click", function () {
                        var randomValue = that.getRandomInt(that.options.interval[i][0], that.options.interval[i][1]);
                        td.text(randomValue);
                    });
                }
            }
        },
        getRandomInt: function (min, max) {
            return Math.floor(Math.random() * (max - min + 1)) + min;
        },
        _destroy: function () {
            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;

            //switch (key) {
            //    case "data":
            //        this.options.data = value;
            //    case "interval":
            //        this.options.interval = value;
            //    case "header":
            //        this.options.header = value;
            //}
            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
        }
    });
}(jQuery));
