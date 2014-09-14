/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.simulationfull", {
        options: {
            data: undefined
        },
        _create: function () {
            var that = this;
            var options = that.options;

            //options.data.
            var header = ["Graph", "Name", "Range"];

            //var numericData = [];
            //numericData[0] = ["rgb(255, 0, 0)", "name1", 0, 1];
            //numericData[1] = [undefined, "name2", 1, 5];
            //numericData[2] = ["rgb(0, 0, 0)", "name3", 3, 6];
            //options.data = {
            //    variables: numericData
            //}
            if (options.data !== undefined && options.data.variables !== undefined) {
                var table1 = $('<div></div>').coloredtableviewer({ header: header, type: "graph-max", numericData: that.options.data.variables });
                table1.width("40%").appendTo(that.element);
            }

            var RunButton = $('<div></div>').addClass("bma-run-button").appendTo(that.element);

            var interval = [];
            interval[0] = [2, 3];
            interval[1] = [0, 5];
            interval[2] = [7, 18];

            var data = [2, 3, 5];

            this.progression = $('<div></div>').addClass("bma-simulation-table").appendTo(that.element);
            this.progression.progressiontable({ interval: interval, data: data });

            that.element.css("display", "flex"); //.appendTo($('body'));
            that.element.children().css("margin", "10px");
            //this.refresh();
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.data !== undefined) {
                for (var i = 0; i < options.data.length; i++) {
                    alert(options.data[i].toString());
                    that._plot.draw({ y: options.data[i], thickness: 4, lineJoin: 'round' });
                }
            }
        },
        _destroy: function () {
            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            if (key === "data")
                this.options.data = value;

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
        }
    });
}(jQuery));
//# sourceMappingURL=simulationfull.js.map
