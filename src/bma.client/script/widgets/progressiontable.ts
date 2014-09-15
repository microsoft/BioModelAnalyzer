/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.progressiontable", {
        options: {
            interval: undefined, // table with interval of values
            data: undefined, // table with data
            header: "Initial Value",
            init: undefined
        },

        _create: function () {
            var that = this;
            var options = that.options;

            var randomise = $('<div></div>').width(120).appendTo(that.element);
            var randomIcon = $('<div></div>').addClass("bma-random-icon2").appendTo(randomise);
            var randomLabel = $('<div></div>').text("Randomise").addClass("randomize-button").appendTo(randomise);


            this.table = $('<table></table>')
                .addClass("bma-prooftable")
                .appendTo(that.element);

            //that.element.appendTo($('body'))
            this.refresh();
            this.addData(this.options.data);
            randomise.bind("click", function () {
                var rands = that.element.find("tr").not(":first-child").children("td:nth-child(2)");
                rands.click();
            })

            window.Commands.On("AddResult", function (param) {
                that.addData(param);
            })
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            this.table.empty();
            var tr0 = $('<tr></tr>').appendTo(that.table);

            if (that.options.header !== undefined)
                $('<td></td>').width(120).attr("colspan","2").text(that.options.header).appendTo(tr0);
            //var init = that.options.init || that.options.interval;
            if (that.options.init !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(that.table);

                    var td = $('<td></td>').text(that.options.init[i]).appendTo(tr);

                    var random = $('<td></td>').addClass("bma-random-icon1").appendTo(tr);

                    random.bind("click", function () {
                        var index = $(this).parent().index() - 1;
                        var randomValue = that.getRandomInt(that.options.interval[index][0], that.options.interval[index][1]);
                        var text = $(this).prev().text(randomValue);
                    })
                }
            }
            else
            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(that.table);
                    
                    var td = $('<td></td>').text(that.options.interval[i][0]).appendTo(tr);

                    var random = $('<td></td>').addClass("bma-random-icon1").appendTo(tr);
                    //var text = $('<div></div>').appendTo(td);
                    
                    random.bind("click", function () {
                        var index = $(this).parent().index()-1;
                        var randomValue = that.getRandomInt(that.options.interval[index][0], that.options.interval[index][1]);
                        var text = $(this).prev();
                        text.text(randomValue);
                    })
                }
            }
        },

        getInit: function () {
            var init = []
            
            var tds = this.element.find("tr:not(:first-child)").children("td:first-child");
            tds.each(function (ind, val) {
                init[ind] = parseInt($(this).text());
            })
            
            return init;
        },

        getLast: function () {
            var init = [];
            var tds;
            if (this.element.find("tr:not(:first-child)").eq(0).children("td").length <= 2)
                tds = this.element.find("tr:not(:first-child)").children("td:first-child")
            else
                tds = this.element.find("tr:not(:first-child)").children("td:last-child");
            tds.each(function (ind, val) {
                init[ind] = parseInt($(this).text());
            })
            return init;
        },


        addData: function (data) {
            var that = this;
            //var data = this.options.data;
            if (data !== undefined) {
                var trs = that.element.find("tr").not(":first-child");
                trs.each(function (ind) {
                    var td = $('<td></td>').text(data[ind]).appendTo($(this));
                    var prev = td.prev();
                    if (td.index() === 1) {
                        prev = prev.prev();
                    }
                    if (td.text() !== prev.text())
                        td.css("background-color", "#fffcb5");

                    //if (td.index() > 1) {
                    //    //if (td.text() !== td.prev().text())
                    //    //    td.css("background-color", "#fffcb5");
                    //}
                    //else {
                    //    if (td.text() !== td.prev().prev().text())
                    //        td.css("background-color", "#fffcb5");
                    //}


                })
            }
        },

        getRandomInt: function(min, max) {
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
                    this.addData(value);
                    break;
                case "interval":
                    this.options.interval = value;
                    this.refresh();
                    break;
                case "header":
                    this.options.header = value;
                    this.refresh();
                    break;
                case "init":
                    this.options.init = value;
                    this.refresh();
                    break;
            }

            this._super(key, value);
            //if (value !== null && value !== undefined)
            //    this.addData();
        }
    });
} (jQuery));

interface JQuery {
    progressiontable(): JQuery;
    progressiontable(settings: Object): JQuery;
    progressiontable(settings: string): any;
    progressiontable(optionLiteral: string, optionName: string): any;
    progressiontable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   