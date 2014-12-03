/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.userdialog", {
        options: {
            message: '',
            functions: []
        },

        _create: function () {
            var that = this;

            this.element.addClass("bma-userdialog");
            var closing = $('<img src="../../images/close.png" class="closing-button">').appendTo(that.element);
            closing.bind("click", function () {
                that.element.hide();
            });
            that.element.draggable({ containment: "parent", scroll: false });
            this.message = $('<div><div>').text(this.options.message).appendTo(that.element);

            this.buttons = $('<div><div>')
                .addClass("bma-userdialog-btns-container")
                .appendTo(that.element);
            var yesBtn = $('<button></button>').text('Yes').appendTo(this.buttons);
            var noBtn = $('<button></button>').text('No').appendTo(this.buttons);
            var cancelBtn = $('<button></button>').text('Cancel').appendTo(this.buttons);
            this.BindFunctions();
            this.popup_position();
        },

        popup_position: function () {
            var my_popup = $('.popup-window, .bma-userdialog'); // наш попап
            my_popup.each(function () {
                var my_popup_w = $(this).outerWidth(), 
                    my_popup_h = $(this).outerHeight(),

                    win_w = $(window).outerWidth(), 
                    win_h = $(window).outerHeight(),
                    popup_half_w = (win_w - my_popup_w) / 2,
                    popup_half_h = (win_h - my_popup_h) / 2;
                if (win_w > my_popup_w) { 
                    my_popup.css({ 'left': popup_half_w });
                }
                if (win_w < my_popup_w) {                 
                    my_popup.css({ 'left': 5, });
                }
                if (win_h > my_popup_h) { 
                    my_popup.css({ 'top': popup_half_h });
                }
                if (win_h < my_popup_h) {
                    my_popup.css({ 'top': 5 });
                }
            })
        },

        BindFunctions: function () {
            var functions = this.options.functions;
            var btns = this.buttons.children("button");
            if (functions !== null && functions.length ===3) {
                for (var i = 0; i < 3; i++)
                    btns.eq(i).bind("click", functions[i]);
            }
        },


        Show: function () {
            this.element.show();
        },

        Hide: function () {
            this.element.hide();
        },

        _destroy: function () {
            var contents;

            // clean up main element
            this.element
                .removeClass("zoomslider-container");

            this.element.children().filter(".bma-elementspanel-visibilityoptions-zoomslider")
                .removeClass("bma-elementspanel-visibilityoptions-zoomslider")
                .removeUniqueId();

            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "message":
                    this.message.text(that.options.message);
                    break;
            }
            this._super(key, value);
        }

    });
} (jQuery));

interface JQuery {
    userdialog(): JQuery;
    userdialog(settings: any): JQuery;
    userdialog(optionLiteral: string, optionName: string): any;
    userdialog(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}