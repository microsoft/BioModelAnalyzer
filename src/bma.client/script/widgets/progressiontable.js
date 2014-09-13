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

            var randomise = $('<div></div>').width(120).appendTo(that.element);
            var randomIcon = $('<div></div>').addClass("bma-random-icon2").appendTo(randomise);
            var randomLabel = $('<div></div>').text("Randomise").addClass("randomize-button").appendTo(randomise);

            this.table = $('<table></table>').addClass("bma-prooftable").appendTo(that.element);

            //that.element.appendTo($('body'))
            this.refresh();
            this.addData();
            randomise.bind("click", function () {
                var rands = that.element.find("tr").not(":first-child").children("td:first-child").children("div");
                rands.click();
            });
        },
        refresh: function () {
            var that = this;
            var options = this.options;

            var tr0 = $('<tr></tr>').appendTo(that.table);

            if (that.options.header !== undefined)
                $('<td></td>').width(120).text(that.options.header).appendTo(tr0);

            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(that.table);

                    var td = $('<td></td>').appendTo(tr);
                    var random = $('<div></div>').addClass("bma-random-icon1").appendTo(td);
                    var text = $('<div></div>').text(that.options.interval[i][0]).appendTo(td);

                    random.bind("click", function () {
                        var index = $(this).parent().parent().index() - 1;
                        var randomValue = that.getRandomInt(that.options.interval[index][0], that.options.interval[index][1]);
                        $(this).next().text(randomValue);
                    });
                }
            }
        },
        addData: function () {
            var that = this;
            var data = this.options.data;
            if (data !== undefined) {
                var trs = that.element.find("tr").not(":first-child");
                trs.each(function (ind) {
                    $('<td></td>').text(data[ind]).appendTo($(this));
                });
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
            switch (key) {
                case "data":
                    this.options.data = value;

                    break;
                case "interval":
                    this.options.interval = value;
                    this.refresh();
                    break;
                case "header":
                    this.options.header = value;
                    this.refresh();
                    break;
            }

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.addData();
        }
    });
}(jQuery));
//# sourceMappingURL=progressiontable.js.map
