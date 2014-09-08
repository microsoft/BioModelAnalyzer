/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.graphnamerangetable", {
        options: {
            variables: [{ color: "", name: "", rangeFrom: 0, rangeTo: 1 }],
            header: ["Graph", "Name", "Range" ]
        },

        refresh: function () {
            var that = this;
            var options = this.options;

        },


        _create: function () {
            var that = this;
            var vars = this.options.variables;

            var table = $('<table></table>').appendTo(that.element);

            var tr0 = $('<tr></tr>').appendTo(table);
            var td01 = $('<td colspan="2"></td>').text("Graph").appendTo(tr0);
            var td02 = $('<td></td>').text("Name").appendTo(tr0);
            var td03 = $('<td colspan="2"></td>').text("Range").appendTo(tr0);

            for (var i = 0; i < that.options.variables.length; i++) {
                var tr = $('<tr></tr>').appendTo(table);
                var td1 = $('<td></td>').appendTo(tr);
                if (vars[i].color !== undefined) {
                    td1.css("background", vars[i].color);
                }
                var buttontd = $('<td></td>').appendTo(tr);
                //var button = $('<img>').attr("src", )
                $('<td></td>').text(vars[i].name).appendTo(tr);
                $('<td></td>').text(vars[i].rangeFrom).appendTo(tr);
                $('<td></td>').text(vars[i].rangeTo).appendTo(tr);
            }

        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "data") this.options.data = value;
            this._super(key, value);
            this.refresh();
        }

    });
} (jQuery));

interface JQuery {
    graphnamerangetable(): JQuery;
    graphnamerangetable(settings: Object): JQuery;
    graphnamerangetable(optionLiteral: string, optionName: string): any;
    graphnamerangetable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 