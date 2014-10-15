/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.localstoragewidget", {


        options: {
            items: [],
        },

        _create: function () {

            var that = this;
            var items = this.options.items;
            var header = $('<div></div>')
                .text("Name")
                .addClass('localStorageWidget-header')
                .appendTo(that.element);
            var closing = $('<img src="../../images/close.png" class="closing-button">').appendTo(that.element);
            closing.bind("click", function () {
                that.element.hide();
            });
            that.element.draggable({ constraint: parent });
            this.repo = $('<div></div>').addClass('localStorageWidget').appendTo(this.element);            
            this.refresh();
        },

        refresh: function () {
            var that = this;
            var items = this.options.items;
            this._cteateHTML();
            //this.table.coloredtableviewer({ header: ["Models"], numericData: that._createTableView(items) });
        },

        AddItem: function (item) {
            this.options.items.push(item);
            this.refresh();
        },

        _cteateHTML: function (items) {
            var items = this.options.items;
            this.repo.empty();

            this.ol = $('<ol></ol>').appendTo(this.repo); 
            
            for (var i = 0; i < items.length; i++) {
                var li = $('<li></li>').appendTo(this.ol);
                var input = $('<input>').attr("type", "text").val(items[i]).appendTo(li);
               
                input.dblclick(function (event) {
                    event.stopPropagation();
                    event.preventDefault();
                    window.Commands.Execute("LocalStorageOpen", items[$(this).parent().index()]);
                    $(this).parent().click();
                })
            }

            this.ol.selectable({
                stop: function () {
                    $(".ui-selected", this).each(function () {
                        window.Commands.Execute("LocalStorageOpen", items[$(this).index()]);
                    });
                }
            });
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
 