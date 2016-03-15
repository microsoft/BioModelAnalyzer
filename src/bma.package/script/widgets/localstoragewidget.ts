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
            this.element.addClass('model-repository');

            var header = $('<span></span>')
                .text("Repository")
                .addClass('window-title')
                .appendTo(that.element);
            var closediv = $('<div></div>').addClass('close-icon').appendTo(that.element);
            var closing = $('<img src="../../images/close.png">').appendTo(closediv);
            closing.bind("click", function () {
                that.element.hide();
            });
            that.element.draggable({ containment: "parent", scroll: false });
            this.message = $('<div></div>')
                //.addClass('localstorage-widget-message')
                .appendTo(this.element);

            this.repo = $('<div></div>')
                .addClass("localstorage-repo")
                //.addClass('localstorage-widget')
                .appendTo(this.element);   

            if (Silverlight.isInstalled()) {
                var slWidget = $('<div></div>').appendTo(this.element);

                var getSilverlightMethodCall =
                    "javascript:Silverlight.getSilverlight(\"5.0.61118.0\");"
                        var installImageUrl =
                    "http://go.microsoft.com/fwlink/?LinkId=161376";
                var imageAltText = "Get Microsoft Silverlight";
                var altHtml =
                    "<a href='{1}' style='text-decoration: none;'>" +
                    "<img src='{2}' alt='{3}' " +
                    "style='border-style: none'/></a>";
                altHtml = altHtml.replace('{1}', getSilverlightMethodCall);
                altHtml = altHtml.replace('{2}', installImageUrl);
                altHtml = altHtml.replace('{3}', imageAltText);

                Silverlight.createObject(
                    "ClientBin/BioCheck.xap",
                    slWidget[0], "slPlugin",
                    {
                        width: "250", height: "50",
                        background: "white", alt: altHtml,
                        version: "5.0.61118.0"
                    },
                    // See the event handlers in the full example.
                    { onError: onSilverlightError },
                    "param1=value1,param2=value2", "row3");
            }
                     
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
                //var a = $('<a></a>').addClass('delete').appendTo(li);
                var removeBtn = $('<button></button>').addClass("delete icon-delete").appendTo(li);// $('<img alt="" src="../images/icon-delete.svg">').appendTo(a);//
                removeBtn.bind("click", function (event) {
                    event.stopPropagation();
                    window.Commands.Execute("LocalStorageRemoveModel", "user."+items[$(this).parent().index()]);
                })
            }

            this.ol.selectable({
                stop: function () {
                    var ind = that.repo.find(".ui-selected").index();
                    window.Commands.Execute("LocalStorageLoadModel", "user."+items[ind]);
                }
            });
        },

        Message: function (msg) {
            this.message.text(msg);
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
 