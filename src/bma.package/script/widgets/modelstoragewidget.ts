/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.modelstoragewidget", {


        options: {
            items: [],
            oneDriveItems: [],
            isAuthorized: false,
            onsigninonedrive: undefined,
            onsignoutonedrive: undefined,
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

            this.switcher = $("<div></div>").addClass("repository-switcher").appendTo(this.element).hide();
            this.localStorageBtn = $("<div>Local</div>").addClass("repository-bttn").addClass("active").appendTo(this.switcher).click(function() {
                that.localStorageBtn.addClass("active");
                that.oneDriveStorageBtn.removeClass("active");
                that.localStorage.show();
                that.oneDriveStorage.hide();
            });

            this.oneDriveStorageBtn = $("<div>OneDrive</div>").addClass("repository-bttn").appendTo(this.switcher).click(function () {
                that.localStorageBtn.removeClass("active");
                that.oneDriveStorageBtn.addClass("active");
                that.localStorage.hide();
                that.oneDriveStorage.show();
            });

            this.localStorage = $("<div></div>").addClass("localstorage-repo").appendTo(this.element);
            this.oneDriveStorage = $("<div></div>").addClass("localstorage-repo").appendTo(this.element); 

            this.localStorage.localstoragewidget();
            this.oneDriveStorage.onedrivestoragewidget();
            if (this.options.items) {
                this.localStorage.localstoragewidget({ items: that.options.items });
            }

            if (this.options.oneDriveItems) {
                this.oneDriveStorage.onedrivestoragewidget({ items: that.options.oneDriveItems });
            }
            
            this.oneDriveStorage.hide();

            //this.repo = $('<div></div>')
            //    .addClass("localstorage-repo")
            ////.addClass('localstorage-widget')
            //    .appendTo(this.element);

            //if (Silverlight && Silverlight.isInstalled()) {
            //    var slWidget = $('<div></div>').appendTo(this.element);

            //    var getSilverlightMethodCall =
            //        "javascript:Silverlight.getSilverlight(\"5.0.61118.0\");"
            //    var installImageUrl =
            //        "http://go.microsoft.com/fwlink/?LinkId=161376";
            //    var imageAltText = "Get Microsoft Silverlight";
            //    var altHtml =
            //        "<a href='{1}' style='text-decoration: none;'>" +
            //        "<img src='{2}' alt='{3}' " +
            //        "style='border-style: none'/></a>";
            //    altHtml = altHtml.replace('{1}', getSilverlightMethodCall);
            //    altHtml = altHtml.replace('{2}', installImageUrl);
            //    altHtml = altHtml.replace('{3}', imageAltText);

            //    Silverlight.createObject(
            //        "ClientBin/BioCheck.xap",
            //        slWidget[0], "slPlugin",
            //        {
            //            width: "250", height: "50",
            //            background: "white", alt: altHtml,
            //            version: "5.0.61118.0"
            //        },
            //        // See the event handlers in the full example.
            //        { onError: onSilverlightError },
            //        "param1=value1,param2=value2", "row3");
            //}

            this.singinOneDriveBtn = $("<div>Sign in with OneDrive</div>").addClass("signin").appendTo(this.element).click(function () {
                if ($(this).text() == "Sign in with OneDrive") {
                    if (that.options.onsigninonedrive !== undefined) {
                        that.options.onsigninonedrive();
                    }
                } else {
                    if (that.options.onsignoutonedrive !== undefined) {
                        that.options.onsignoutonedrive();
                    }
                }
            }); 

            this.refresh();
        },

        refresh: function () {
           // this._createHTML();
        },

        Message: function (msg) {
            this.message.text(msg);
        },

        AddItem: function (item) {
            var that = this;
            this.options.items.push(item);
            this.localStorage.localstoragewidget( "AddItem", item );
            this.oneDriveStorage.onedrivestoragewidget("AddItem", item);
            this.refresh();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "items":
                    this.options.items = value;
                    this.localStorage.localstoragewidget({ items: that.options.items });
                    this.refresh();
                    break;
                case "oneDriveItems":
                    this.options.oneDriveItems = value;
                    this.oneDriveStorage.onedrivestoragewidget({ items: that.options.oneDriveItems});
                    this.refresh();
                    break;
                case "onsigninonedrive":
                    that.options.onsigninonedrive = value;
                    break;
                case "onsignoutonedrive":
                    that.options.onsignoutonedrive = value;
                    break;
                case "isAuthorized":
                    that.options.isAuthorized = value;
                    if (that.options.isAuthorized) {
                        that.singinOneDriveBtn.text("Sign out OneDrive");
                        that.switcher.show();
                    } else {
                        that.localStorageBtn.addClass("active");
                        that.oneDriveStorageBtn.removeClass("active");
                        that.localStorage.show();
                        that.oneDriveStorage.hide();

                        that.singinOneDriveBtn.text("Sign in with OneDrive");
                        that.switcher.hide();
                    }
                    break;
            }
            this._super(key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
            this.element.empty();
        }

    });
} (jQuery));

interface JQuery {
    modelstoragewidget(): JQuery;
    modelstoragewidget(settings: Object): JQuery;
    modelstoragewidget(optionLiteral: string, optionName: string): any;
    modelstoragewidget(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
 