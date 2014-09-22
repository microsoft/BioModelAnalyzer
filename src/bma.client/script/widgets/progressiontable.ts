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
            randomise.css("position", "absolute");
            randomise.css("top", "-30px");
            var randomIcon = $('<div></div>').addClass("bma-random-icon2").appendTo(randomise);
            var randomLabel = $('<div></div>').text("Randomise").addClass("randomize-button").appendTo(randomise);

            //var div = $('<div></div>')
            //    .appendTo(that.element);

            this.init = $('<div></div>')
                .appendTo(that.element);
            this.init.css("display", "inline-block");
            //that.element.appendTo($('body'))
            this.refreshInit();
            this.data = $('<div></div>')
                .addClass("bma-simulation-data-table")
                .appendTo(that.element);
            //this.data.css("display", "inline-block");
            this.initData();
            randomise.bind("click", function () {
                var rands = that.init.find("tr").not(":first-child").children("td:nth-child(2)");
                rands.click();
            })

            window.Commands.On("AddResult", function (param) {
                that.addData(param);
            })
        },

        initData: function () {
            this.data.empty();
            if (this.options.data !== undefined && this.options.data.length !== 0) {
                var data = this.options.data;
                if (data[0].length === this.options.interval.length) 
                    for (var i = 0; i < data.length && 0 < data[i].length; i++) {
                        this.addData(data[i]);
                    }
                //var table = $('<table></table>')
                //    .addClass("bma-progressiontable")
                //    .appendTo(this.data);
                //for (var i = 0; i < data.length && 0 < data[i].length; i++) {
                //    var tr = $('<tr></tr>').appendTo(table);
                //    $('<td></td>').text(data[i][0]).appendTo(tr);
                //    for (var j = 1; j < data[i].length; j++) {
                //        var td = $('<td></td>').text(data[i][j]).appendTo(tr);
                //        if (data[i][j] !== data[i][j - 1])
                //            td.css("background-color", "#fffcb5");
                //    }
                //}
            }
        },

        refreshInit: function () {
            var that = this;
            var options = this.options;
            this.init.empty();
            var table = $('<table></table>')
                .addClass("bma-prooftable")
                .addClass("bma-simulation-table")
                .appendTo(that.init);
            var tr0 = $('<tr></tr>').appendTo(table);

            if (that.options.header !== undefined)
                $('<td></td>').width(120).attr("colspan","2").text(that.options.header).appendTo(tr0);
            // || undefined;//that.options.init || that.options.interval;
            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(table);
                    var td = $('<td></td>').appendTo(tr);
                    var input = $('<input type="text">').attr("size","1").appendTo(td);
                    var init = that.options.init !== undefined ? that.options.init[i] || that.options.interval[i] : that.options.interval[i];
                    if (Array.isArray(init)) 
                        input.val(init[0]);
                    else
                        input.val(init);

                    var random = $('<td></td>').addClass("bma-random-icon1").appendTo(tr);

                    //input.bind ("input change")

                    random.bind("click", function () {
                        var index = $(this).parent().index() - 1;
                        //alert(that.options.interval[index][0] + ' ' + that.options.interval[index][1]);
                        var randomValue = that.getRandomInt(that.options.interval[index][0], that.options.interval[index][1]);
                        //$(this).prev().text('');
                        //
                        $(this).prev().children("input").eq(0).val(randomValue);//randomValue);
                    })
                }
            }
        },

        findClone: function (column: JQuery): number {
            var trs = this.data.find("tr");
            var tr0 = trs.eq(0);
            for (var i = 0; i < tr0.children("td").length-1; i++) {
                var tds = trs.children("td:nth-child(" + (i + 1) + ")");
                if (this.isClone(column, tds)) {
                    alert()
                    if (this.repeat === undefined)
                        this.repeat = tds;
                    return i;
                }
            }
            return undefined;
        },

        isClone: function (td1,td2): boolean {
            if (td1.length !== td2.length)
                return false;
            else {
                var arr = [];
                for (var i = 0; i < td1.length; i++) {
                    //console.log(td1.eq(i).text() + ' ' + td2.eq(i).text())
                    
                    //for (var i = 0; i < td1.length; i++) {
                        arr[i] = td1.eq(i).text() + " " + td2.eq(i).text();
                    //}
                    
                    if (td1.eq(i).text() !== td2.eq(i).text())
                        return false;
                }
                //alert(arr);
                return true;
            }
        },

        getInit: function () {
            var init = [];
            var inputs = this.init.find("tr:not(:first-child)").children("td:first-child").children("input");
            inputs.each(function (ind) {
                init[ind] = parseInt($(this).val());
            });
            return init;
        },

        //getLast: function () {
        //    var init = [];
        //    var tds;
        //    if (this.element.find("tr:not(:first-child)").eq(0).children("td").length <= 2)
        //        tds = this.element.find("tr:not(:first-child)").children("td:first-child")
        //    else
        //        tds = this.element.find("tr:not(:first-child)").children("td:last-child");
        //    tds.each(function (ind, val) {
        //        init[ind] = parseInt($(this).text());
        //    })
        //    return init;
        //},

        clearData: function () {
            this.data.empty();
        },

        addData: function (data) {
            var that = this;
            //var data = this.data;
            if (data !== undefined) {
                var trs = that.data.find("tr");

                if (trs.length === 0) {
                    var table = $('<table></table>')
                        .addClass("bma-progressiontable")
                        .appendTo(that.data);
                    for (var i = 0; i < data.length; i++) {
                        var tr = $('<tr></tr>').appendTo(table);
                        $('<td></td>').text(data[i]).appendTo(tr);
                    }
                }
                else {
                    trs.each(function (ind) {
                        var td = $('<td></td>').text(data[ind]).appendTo($(this));
                        if (td.text() !== td.prev().text())
                            td.css("background-color", "#fffcb5");
                    })
                    var last = that.data.find("tr").children("td:last-child");
                    //var arr = [];
                    //for (var i = 0; i < last.length; i++) {
                    //    arr[i] = last.eq(i).text();
                    //}
                    //alert(arr);
                    if (that.repeat !== undefined) {
                        //var arr = [];
                        //for (var i = 0; i < that.repeat.length; i++) {
                        //    arr[i] = that.repeat.eq(i).text();
                        //}
                        //alert(arr);
                        if (that.isClone(that.repeat, last))
                            that.highlight(that.data.find("tr:first-child").children("td").length);
                        else;
                    }
                    else {
                        
                        var cloneInd = that.findClone(last);
                        //alert(cloneInd);
                        if (cloneInd !== undefined) {
                            that.highlight(that.data.find("tr:first-child").children("td").length);
                            that.highlight(cloneInd);
                        }
                    }
                }
            }
        },

        highlight: function (ind) {
            //alert(ind);
            var tds = this.data.find("tr").children("td:nth-child(" + ind+1 + ")");
            tds.addClass("bma-highlighting-columns");
        },

        getRandomInt: function (min, max) {
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
                    this.refreshInit();
                    break;
                case "header":
                    this.options.header = value;
                    //this.refresh();
                    break;
                case "init":
                    this.options.init = value;
                    this.refreshInit();
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
    progressiontable(func: string, param1: any, param2: any ): any;
    progressiontable(optionLiteral: string, optionName: string): any;
    progressiontable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   