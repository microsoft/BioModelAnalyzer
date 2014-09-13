/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationprogressiontable", {
        options: {
            data: undefined
        },

        _create: function () {
            var that = this;
            var options = that.options;
            //options.data.
            var header = ["Graph", "Name", "Range"];
            var numericData = [];
            numericData[0] = ["rgb(255, 0, 0)", "name1", 0, 1];
            numericData[1] = [undefined, "name2", 1, 5];
            numericData[2] = ["rgb(0, 0, 0)", "name3", 3, 6];
            options.data = {
                variables: numericData
            }

            if (options.data !== undefined && options.data.variables !== undefined) {
                var table1 = $('<div></div>').coloredtableviewer({ header: header, type: "graph-max", numericData: that.options.data.variables });
                table1.addClass("popup-window").show().appendTo(that.element);
            }
            
            that.element.appendTo($('body'))
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
            if (key === "data") this.options.data = value;

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
        }
    });
} (jQuery));

interface JQuery {
    simulationprogressiontable(): JQuery;
    simulationprogressiontable(settings: Object): JQuery;
    simulationprogressiontable(settings: string): any;
    simulationprogressiontable(optionLiteral: string, optionName: string): any;
    simulationprogressiontable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   