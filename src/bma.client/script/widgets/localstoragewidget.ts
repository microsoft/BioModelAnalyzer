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
            that.element.draggable({ constraint: parent, scroll: false });
            this.repo = $('<div></div>').addClass('localStorageWidget').appendTo(this.element);            
            this.refresh();
        },

        refresh: function () {
            this._createHTML();
        },

        AddItem: function (item) {
            this.options.items.push(item);
            this.refresh();
        },

        _createHTML: function (items) {
            var items = this.options.items;
            this.repo.empty();
            var that = this;
            this.ol = $('<ol></ol>').appendTo(this.repo); 
            
            for (var i = 0; i < items.length; i++) {
                var li = $('<li></li>').text(items[i]).appendTo(this.ol);
                var removeBtn = $('<button></button>').addClass("localstorage-remove-button").appendTo(li);
                removeBtn.bind("click", function () {
                    window.Commands.Execute("LocalStorageRemoveModel", items[$(this).parent().index()]);
                })
            }

            this.ol.selectable({
                stop: function () {
                    console.log("STOP");
                    //$(".ui-selected", this).each(function () {
                    window.Commands.Execute("LocalStorageLoadModel", items[$(this).find(".ui-selected").eq(0).index()]);
                    //});
                }
            });

            //this.ol.on("selectablestop", function () {
            //    $(".ui-selected", this).each(function () {
            //        window.Commands.Execute("LocalStorageLoadModel", that.options.items[$(this).index()]);
            //    });
            //})
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
            this.element.empty();
        }

    });
} (jQuery));

interface JQuery {
    localstoragewidget(): JQuery;
    localstoragewidget(settings: Object): JQuery;
    localstoragewidget(optionLiteral: string, optionName: string): any;
    localstoragewidget(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
 