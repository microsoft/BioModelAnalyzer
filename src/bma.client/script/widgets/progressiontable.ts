/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.progressiontable", {
        options: {
            interval: undefined, // table with interval of values
            data: undefined, // table with data
            header: "Initial Value",
            tags: undefined,
            init: undefined,
            canEditInitialValue: true,
        },

        _create: function () {
            var that = this;
            var options = that.options;

            that.element.addClass('simulation-progression-table-container');
            this.init = $('<div></div>')
                .appendTo(that.element);
            this.RefreshInit();
            this.data = $('<div></div>')
                .addClass("bma-simulation-data-table")
                .appendTo(that.element);
            this.InitData();
        },

        Randomise: function () {
            var rands = this.init.find("tr").not(":first-child").children("td:nth-child(2)");
            rands.click();
        },

        InitData: function () {
            this.ClearData();
            if (this.options.data !== undefined && this.options.data.length !== 0) {
                var data = this.options.data;
                if (data[0].length === this.options.interval.length) 
                    for (var i = 0; i < data.length; i++) {
                        this.AddData(data[i]);
                    }
            }
        },

        RefreshInit: function () {
            var that = this;
            var options = this.options;
            this.init.empty();
            var table = $('<table></table>')
                .addClass("variables-table")
                .appendTo(that.init);
            var tr0 = $('<tr></tr>').appendTo(table);

            if (that.options.header !== undefined)
                $('<td></td>').width(120).attr("colspan","2").text(that.options.header).appendTo(tr0);
            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(table);
                    var td = $('<td></td>').appendTo(tr);
                    var input = $('<input type="text">').width("100%").appendTo(td);
                    
                    var init = that.options.init !== undefined ? that.options.init[i] || that.options.interval[i] : that.options.interval[i];
                    if (Array.isArray(init)) 
                        input.val(init[0]);
                    else
                        input.val(init);

                    if (that.options.canEditInitialValue) {
                        var random = $('<td></td>')
                            .addClass("random-small bma-random-icon2 hoverable")
                            .appendTo(tr);
                        //random.filter(':nth-child(even)').addClass('bma-random-icon1');
                        //random.filter(':nth-child(odd)').addClass('bma-random-icon2');
                        random.bind("click", function () {
                            var prev = parseInt($(this).prev().children("input").eq(0).val());
                            var index = $(this).parent().index() - 1;
                            var randomValue = that.GetRandomInt(parseInt(that.options.interval[index][0]), parseInt(that.options.interval[index][1]));
                            $(this).prev().children("input").eq(0).val(randomValue);//randomValue);
                            if (randomValue !== prev)
                                $(this).parent().addClass('red');
                            else
                                $(this).parent().removeClass('red');
                        });
                    } else {
                        input.attr("disabled", "disabled");
                    }
                }
            }
        },

        FindClone: function (column: JQuery): number {
            var trs = this.data.find("tr");
            var tr0 = trs.eq(0);
            for (var i = 0; i < tr0.children("td").length-1; i++) {
                var tds = trs.children("td:nth-child(" + (i + 1) + ")");
                if (this.IsClone(column, tds)) {
                    if (this.repeat === undefined)
                        this.repeat = tds;
                    return i;
                }
            }
            return undefined;
        },

        IsClone: function (td1,td2): boolean {
            if (td1.length !== td2.length)
                return false;
            else {
                var arr = [];
                for (var i = 0; i < td1.length; i++) {
                        arr[i] = td1.eq(i).text() + " " + td2.eq(i).text();
                    if (td1.eq(i).text() !== td2.eq(i).text())
                        return false;
                }
                return true;
            }
        },

        GetInit: function () {
            var init = [];
            var inputs = this.init.find("tr:not(:first-child)").children("td:first-child").children("input");
            inputs.each(function (ind) {
                init[ind] = parseInt($(this).val());
            });
            return init;
        },

        ClearData: function () {
            this.data.empty();
        },

        AddData: function (data) {
            var that = this;
            //var data = this.data;
            if (data !== undefined) {
                var trs = that.data.find("tr");

                if (trs.length === 0) {
                    that.repeat = undefined;
                    var table = $('<table></table>')
                        .addClass("progression-table")
                        .appendTo(that.data);

                    if (that.options.tags !== undefined) {
                        var tr0 = $('<tr></tr>').addClass("table-tags").appendTo(table);
                        var count = (that.options.tags.length > 0) ? 1 : 0;
                        var prevState = undefined;
                        var prevTd;

                        var compareTags = function (prev, curr) {
                            if (prev === undefined)
                                return false;
                            if (prev.length === curr.length) {
                                for (var j = 0; j < prev.length; j++) {
                                    if (prev[j] !== curr[j])
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }

                        for (var i = 0; i < that.options.tags.length; i++) {
                            if (!compareTags(prevState, that.options.tags[i])) {
                                if (count > 1)
                                    $(prevTd).attr("colspan", count);
                                prevState = that.options.tags[i];
                                prevTd = $('<td></td>').text(prevState).appendTo(tr0);
                                count = 1;
                            } else {
                                count++;
                            }
                        }

                        if (count > 1)
                            $(prevTd).attr("colspan", count);
                    }

                    for (var i = 0; i < data.length; i++) {
                        var tr = $('<tr></tr>').appendTo(table);
                        var td = $('<td></td>').text(data[i]).appendTo(tr);
                        //$('<span></span>').text(data[i]).appendTo(td);
                    }
                }
                else {
                    if (that.options.tags !== undefined)
                        trs = trs.slice(1);
                    trs.each(function (ind) {
                        var td = $('<td></td>').text(data[ind]).appendTo($(this));
                        //$('<span></span>').text(data[ind]).appendTo(td);
                        if (td.text() !== td.prev().text())
                            td.addClass('change')
                    })
                    var last = that.data.find("tr").children("td:last-child");
                    if (that.repeat !== undefined) {
                        if (that.IsClone(that.repeat, last))
                            that.Highlight(that.data.find("tr:first-child").children("td").length-1);
                        else;
                    }
                    else {
                        var cloneInd = that.FindClone(last);
                        if (cloneInd !== undefined) {
                            that.Highlight(cloneInd);
                            that.Highlight(that.data.find("tr:first-child").children("td").length-1);
                        }
                    }
                }
            }
        },

        Highlight: function (ind) {
            var that = this;
            var tds = this.data.find("tr").children("td:nth-child(" + (ind + 1) + ")");
            tds.each(function (ind) {
                $(this).addClass('repeat');
                //var div = $('<div></div>').appendTo($(this));
                //div.addClass('repeat');
            });
        },

        GetRandomInt: function (min, max) {
            return Math.floor(Math.random() * (max - min + 1) + min);
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "interval":
                    this.options.interval = value;
                    this.RefreshInit();
                    break;
                case "header":
                    this.options.header = value;
                    break;
                case "init":
                    this.options.init = value;
                    this.RefreshInit();
                    break;
                case "data":
                    this.options.data = value;
                    this.InitData();
                    break;
                case "tags":
                    this.options.tags = value;
                    this.InitData();
                    break;
                case "canEditInitialValue":
                    this.options.canEditInitialValue = value;
                    this.RefreshInit();
                    break;
            }
            this._super(key, value);
        }
    });
} (jQuery));

interface JQuery {
    progressiontable(): JQuery;
    progressiontable(settings: Object): JQuery;
    progressiontable(settings: string): any;
    progressiontable(func: string, param1: any, param2: any ): any;
    progressiontable(optionLiteral: string, optionName: string): any;
    progressiontable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   