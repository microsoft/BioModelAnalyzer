/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.localstoragewidget", {


        options: {
            items: [],
        },

        _create: function () {

            var that = this;
            var closing = $('<img src="../../images/close.png" class="closing-button">').appendTo(that.element);
            closing.bind("click", function () {
                that.element.hide();
            });
            that.element.draggable({ constraint: parent });
            this.table = $('<div></div>')
                .addClass("bma-localstoragewidget")
                .appendTo(that.element)
                .coloredtableviewer();
            this.refresh();
        },

        refresh: function () {
            var that = this;
            var items = this.options.items;
            this.table.coloredtableviewer({ header: ["Models"], numericData: that._createTableView(items) });
        },

        AddItem: function (item) {
            this.options.items.push(item);
            this.refresh();
        },

        _createTableView: function (items) {
            var table = [];
            if (items !== undefined && items !== null && items.length !==0)
                for (var i = 0; i < items.length; i++) {
                    table[i] = [];
                    table[i][0] = items[i];
                }
            return table;
        },

        _setOption: function (key, value) {
            switch (key) {
                case "items":
                    this.options.items = value;
                    this.refresh();
                    break;
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));

interface JQuery {
    localstoragewidget(): JQuery;
    localstoragewidget(settings: Object): JQuery;
    localstoragewidget(optionLiteral: string, optionName: string): any;
    localstoragewidget(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
 