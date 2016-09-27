/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.localstoragewidget", {


        options: {
            items: [],
            onremovemodel: undefined,
            onloadmodel: undefined,
            enableContextMenu: false,
        },

        _create: function () {
            var that = this;
            this.repo = this.element;
            var items = this.options.items;
            //this.repo = $('<div></div>')
            //    .addClass("localstorage-repo")
            //    //.addClass('localstorage-widget')
            //    .appendTo(this.element);   

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
                var li = $('<li></li>')/*.text(items[i])*/.appendTo(this.ol).click(function () {
                    var ind = $(this).index();
                    if (that.options.onloadmodel !== undefined) {
                        that.options.onloadmodel("user." + items[ind]);//.done(function () {
                        //    that.repo.find(".ui-selected").removeClass("ui-selected");
                        //    $(that.options.selectedLi).addClass("ui-selected");
                        //    if (that.options.oncancelselection !== undefined)
                        //        that.options.oncancelselection();
                        //});
                    }
                });
                //var a = $('<a></a>').addClass('delete').appendTo(li);
                var modelName = $("<div>" + items[i] + "</div>").appendTo(li);
                var removeBtn = $('<button></button>').addClass("delete icon-delete").appendTo(li);// $('<img alt="" src="../images/icon-delete.svg">').appendTo(a);//
                removeBtn.bind("click", function (event) {
                    if (that.options.onremovemodel !== undefined) 
                        that.options.onremovemodel("user." + items[$(this).parent().index()]);
                    //event.stopPropagation();
                    //window.Commands.Execute("LocalStorageRemoveModel", "user."+items[$(this).parent().index()]);
                })
            }
            //this.ol.selectable({
            //    stop: function () {
            //        var ind = that.repo.find(".ui-selected").index();
            //        if (that.options.onloadmodel !== undefined) {
            //            that.options.onloadmodel("user." + items[ind]);
            //            if (that.options.oncancelselection !== undefined)
            //                that.options.oncancelselection();
            //        }
            //        //window.Commands.Execute("LocalStorageLoadModel", "user."+items[ind]);
            //    }
            //});

            //this.createContextMenu();
        },

        CancelSelection: function () {
            this.repo.find(".ui-selected").removeClass("ui-selected");
        },

        Message: function (msg) {
            if (this.onmessagechanged !== undefined)
                this.onmessagechanged(msg);
        },

        SetActiveModel: function (key) {
            var that = this;
            var idx;
            for (var i = 0; i < that.options.items.length; i++) {
                if (("user." + that.options.items[i]) == key) {
                    idx = i;
                    break;
                }
            }
            if (idx !== undefined) {
                this.repo.find(".ui-selected").removeClass("ui-selected");
                this.ol.children().eq(idx).addClass("ui-selected");
            }
        },

        createContextMenu: function () {
            var that = this;
            this.repo.contextmenu({
                delegate: "li",
                autoFocus: true,
                preventContextMenuForPopup: true,
                preventSelect: true,
                menu: [
                    { title: "Move to OneDrive", cmd: "MoveToOneDrive" },
                    { title: "Copy to OneDrive", cmd: "CopyToOneDrive" },
                ],
                beforeOpen: function (event, ui) {
                    if (that.options.enableContextMenu) {
                        ui.menu.zIndex(50);
                    } else return false;
                },
                select: function (event, ui) {
                    var args: any = {};
                    var idx = $(ui.target.context).index();

                    if (that.options.setoncopytoonedrive !== undefined) {
                        that.options.setoncopytoonedrive("user." + that.options.items[idx]).done(function () {

                            if (ui.cmd == "MoveToOneDrive") {
                                if (that.options.onremovemodel !== undefined)
                                    that.options.onremovemodel("user." + that.options.items[idx]);
                                //window.Commands.Execute("LocalStorageRemoveModel", "user." + that.options.items[idx]);
                            }
                        });
                    }
                }
            });
        },

        _setOption: function (key, value) {
            switch (key) {
                case "items":
                    this.options.items = value;
                    this.refresh();
                    break;
                case "onloadmodel":
                    this.options.onloadmodel = value;
                    break;
                case "onremovemodel":
                    this.options.onremovemodel = value;
                    break;
                case "setoncopytoonedrive":
                    this.options.setoncopytoonedrive = value;
                    break;
                case "onmessagechanged":
                    this.options.onmessagechanged = value;
                    break;
                case "enableContextMenu":
                    this.options.enableContextMenu = value;
                    break;
                case "oncancelselection":
                    this.options.oncancelselection = value;
                    break;
            }
            this._super(key, value);
        },

        destroy: function () {
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
 