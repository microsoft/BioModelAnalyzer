(function ($) {
    $.widget("BMA.progressiontable", {
        options: {
            interval: undefined,
            data: undefined,
            header: "Initial Value",
            init: undefined
        },
        _create: function () {
            var that = this;
            var options = that.options;

            var randomise = $('<div></div>').addClass("bma-progressiontable-randimise").appendTo(that.element);

            var randomIcon = $('<div></div>').addClass("bma-random-icon2").appendTo(randomise);
            var randomLabel = $('<div></div>').text("Randomise").addClass("randomize-button").appendTo(randomise);

            this.init = $('<div></div>').appendTo(that.element);
            this.init.css("display", "inline-block");
            this.RefreshInit();
            this.data = $('<div></div>').addClass("bma-simulation-data-table").appendTo(that.element);
            this.InitData();
            randomise.bind("click", function () {
                var rands = that.init.find("tr").not(":first-child").children("td:nth-child(2)");
                rands.click();
            });
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
            var table = $('<table></table>').addClass("bma-prooftable").addClass("bma-simulation-table").appendTo(that.init);
            var tr0 = $('<tr></tr>').appendTo(table);

            if (that.options.header !== undefined)
                $('<td></td>').width(120).attr("colspan", "2").text(that.options.header).appendTo(tr0);

            if (that.options.interval !== undefined) {
                for (var i = 0; i < that.options.interval.length; i++) {
                    var tr = $('<tr></tr>').appendTo(table);
                    var td = $('<td></td>').appendTo(tr);
                    var input = $('<input type="text">').height("100%").width("100%").appendTo(td);
                    var init = that.options.init !== undefined ? that.options.init[i] || that.options.interval[i] : that.options.interval[i];
                    if (Array.isArray(init))
                        input.val(init[0]);
                    else
                        input.val(init);

                    var random = $('<td></td>').addClass("bma-random-icon1 hoverable").appendTo(tr);

                    random.bind("click", function () {
                        var index = $(this).parent().index() - 1;
                        var randomValue = that.GetRandomInt(parseInt(that.options.interval[index][0]), parseInt(that.options.interval[index][1]));
                        $(this).prev().children("input").eq(0).val(randomValue);
                    });
                }
            }
        },
        FindClone: function (column) {
            var trs = this.data.find("tr");
            var tr0 = trs.eq(0);
            for (var i = 0; i < tr0.children("td").length - 1; i++) {
                var tds = trs.children("td:nth-child(" + (i + 1) + ")").children("span:first-child");
                if (this.IsClone(column, tds)) {
                    if (this.repeat === undefined)
                        this.repeat = tds;
                    return i;
                }
            }
            return undefined;
        },
        IsClone: function (td1, td2) {
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

            if (data !== undefined) {
                var trs = that.data.find("tr");

                if (trs.length === 0) {
                    that.repeat = undefined;
                    var table = $('<table></table>').addClass("bma-progressiontable").appendTo(that.data);
                    for (var i = 0; i < data.length; i++) {
                        var tr = $('<tr></tr>').appendTo(table);
                        var td = $('<td></td>').appendTo(tr);
                        td.css("min-width", "20");
                        $('<span></span>').text(data[i]).appendTo(td);
                    }
                } else {
                    trs.each(function (ind) {
                        var td = $('<td></td>').appendTo($(this));
                        td.css("min-width", "20");
                        $('<span></span>').text(data[ind]).appendTo(td);
                        if (td.children("span").eq(0).text() !== td.prev().children("span:first-child").text())
                            td.css("background-color", "#fffcb5");
                    });
                    var last = that.data.find("tr").children("td:last-child").children("span:first-child");
                    if (that.repeat !== undefined) {
                        if (that.IsClone(that.repeat, last))
                            that.Highlight(that.data.find("tr:first-child").children("td").length - 1);
                        else
                            ;
                    } else {
                        var cloneInd = that.FindClone(last);
                        if (cloneInd !== undefined) {
                            that.Highlight(cloneInd);
                            that.Highlight(that.data.find("tr:first-child").children("td").length - 1);
                        }
                    }
                }
            }
        },
        Highlight: function (ind) {
            var that = this;
            var tds = this.data.find("tr").children("td:nth-child(" + (ind + 1) + ")");
            tds.each(function (ind) {
                var div = $('<div></div>').appendTo($(this));
                div.css("background-color", "rgb(223, 223, 245, 0.5)");
            });
        },
        GetRandomInt: function (min, max) {
            return Math.floor(Math.random() * (max - min + 1) + min);
        },
        _hexToRGB: function (hex) {
            var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
            return result ? {
                r: parseInt(result[1], 16),
                g: parseInt(result[2], 16),
                b: parseInt(result[3], 16)
            } : null;
        },
        _colorToRGB: function (color) {
            alert(color);
            if (color.substr(0, 1) === '#') {
                return color;
            }
            var digits = /(.*?)rgb\((\d+), (\d+), (\d+)\)/.exec(color);

            var red = parseInt(digits[2]);
            var green = parseInt(digits[3]);
            var blue = parseInt(digits[4]);

            var rgb = blue | (green << 8) | (red << 16);
            return { r: red, g: green, b: blue };
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
            }
            this._super(key, value);
        }
    });
}(jQuery));
