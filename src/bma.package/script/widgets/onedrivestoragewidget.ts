/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.onedrivestoragewidget", {


        options: {
            items: [],
            activeShare: [],
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

        createContextMenu: function () {
            var that = this;
            this.repo.contextmenu({
                delegate: "li",
                autoFocus: true,
                preventContextMenuForPopup: true,
                preventSelect: true,
                menu: [
                    { title: "Move to local", cmd: "MoveToLocal" },
                    { title: "Copy to local", cmd: "CopyToLocal" },
                    {
                        title: "Share", cmd: "Share"/*, children: [
                            { title: "BMA link", cmd: "BMALink" },
                            { title: "Web link", cmd: "WebLink" },
                            { title: "Email", cmd: "Email" },
                        ]*/
                    },
                    { title: "Open BMA link", cmd: "OpenBMALink" },
                    { title: "Active Shares", cmd: "ActiveShares", /*children:[],*/isVisible: false/*that.options.activeShare.length !== 0*/ },
                ],
                beforeOpen: function (event, ui) {
                    ui.menu.zIndex(50);
                    if (that.options.activeShare.length === 0) $(this).contextmenu("showEntry", "ActiveShares", false);
                    if ($(ui.target.context.parentElement).hasClass("table-tags"))
                        return false;
                },
                select: function (event, ui) {
                    var args: any = {};
                    args.command = ui.cmd;
                    if ($(ui.item.context).text() == "Share") {
                        that.menuPopup("Share '" + $(ui.target.context).text() + "'", [
                            { name: "BMA link", callback: function () { console.log("bma link"); } },
                            { name: "Web link", callback: function () { console.log("web link"); } },
                            { name: "Email", callback: function () { console.log("email"); } }
                        ]);
                    }

                    if (that.options.onContextMenuItemSelected !== undefined)
                        that.options.onContextMenuItemSelected(args);
                }
            });
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
                    //window.Commands.Execute("LocalStorageRemoveModel", "user." + items[$(this).parent().index()]);
                })
            }

            this.ol.selectable({
                stop: function () {
                    var ind = that.repo.find(".ui-selected").index();
                    //window.Commands.Execute("LocalStorageLoadModel", "user." + items[ind]);
                }
            });

            this.createContextMenu();
        },

        Message: function (msg) {
            this.message.text(msg);
        },

        menuPopup: function (title, listOfItems) { // list : { name, callback}
            var that = this;
            var content = $("<div></div>").addClass("repository-popup-menu");
            var list = $("<div></div>").appendTo(content);
            for (var i = 0; i < listOfItems.length; i++) {
                var elem = $("<div></div>").attr("data-menu-item-name", listOfItems[i].name).appendTo(list);
                var elemIcon = $("<div></div>").addClass("repository-menu-item").addClass("repository-menu-item-icon").appendTo(elem);
                if (!i) elemIcon.addClass("active");
                var elemName = $("<div>" + listOfItems[i].name + "</div>").addClass("repository-menu-item").appendTo(elem);
                elem.click(function () {
                    list.children().removeClass("active");
                    elemIcon.addClass("active");

                    var idx;
                    for (var j = 0; j < listOfItems.length; j++)
                        if ($(this).attr("data-menu-item-name") == listOfItems[j].name) {
                            idx = j;
                            break;
                        }

                    if (idx !== undefined && listOfItems[idx] !== undefined && listOfItems[idx].callback !== undefined) {
                        listOfItems[idx].callback(readOnlyBtn.hasClass("selected"));
                        content.empty();
                        popup.hide();
                    }
                });
            }

            var readOnlyBtn = $("<div>Read only</div>").addClass("repository-readonly-bttn").appendTo(content).click(function () {
                if (readOnlyBtn.hasClass("selected"))
                    readOnlyBtn.removeClass("selected");
                else readOnlyBtn.addClass("selected");
            });

            var popup = $("<div></div>").addClass('popup-window window').appendTo('body');
            popup.resultswindowviewer({ header: title, content: content, iconOn: false });
        },

        _setOption: function (key, value) {
            switch (key) {
                case "items":
                    this.options.items = value;
                    this.refresh();
                    break;
            }
            //$.Widget.prototype._setOption.apply(this, arguments);
            //this._super("_setOption", key, value);
            this._super(key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
            this.element.empty();
        }

    });
} (jQuery));

interface JQuery {
    onedrivestoragewidget(): JQuery;
    onedrivestoragewidget(settings: Object): JQuery;
    onedrivestoragewidget(optionLiteral: string, optionName: string): any;
    onedrivestoragewidget(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
 