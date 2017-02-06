// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.userdialog", {
        options: {
            message: '',
            actions: [
                { button: 'Yes', callback: function () { }},
                { button: 'No', callback: function () { }},
                { button: 'Cancel', callback: function () { }}
            ]
        },

        _create: function () {
            var that = this;
            this.element.addClass("window dialog").css("z-index", InteractiveDataDisplay.ZIndexDOMMarkers + 100);
            this.element.draggable({ containment: "parent", scroll: false });
            this._add_close_button();
            this.message = $('<div><div>')
                .text(this.options.message)
                .addClass('window-title')
                .appendTo(that.element);
            this.buttons = $('<div><div>')
                .addClass("button-list")
                .appendTo(that.element);
            var actions = this.options.actions;
            if (actions !== undefined) {
                for (var i = 0; i < actions.length; i++) {
                    var bttn = $('<button></button>').text(actions[i].button).appendTo(that.buttons);
                    bttn.bind('click', actions[i].callback);
                }
            }
            //var yesBtn = $('<button></button>').text('Yes').appendTo(this.buttons);
            //var noBtn = $('<button></button>').text('No').appendTo(this.buttons);
            //var cancelBtn = $('<button></button>').text('Cancel').appendTo(this.buttons);
            //this._bind_functions();
            this._popup_position();
        },

        _add_close_button: function () {
            var that = this;
            var closediv = $('<div></div>').addClass("close-icon").appendTo(that.element);
            var closing = $('<img>').attr('src', '../../images/close.png').appendTo(closediv);
            closing.bind("click", function () {
                that.element.hide();
            });
        },

        _popup_position: function () {
            var my_popup = $('.dialog'); 
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

        //_bind_functions: function () {
        //    var functions = this.options.functions;
        //    var btns = this.buttons.children("button");
        //    if (functions !== undefined) {
        //        for (var i = 0; i < functions.length; i++)
        //            btns.eq(i).bind("click", functions[i]);
        //    }
        //},


        Show: function () {
            this._popup_position();
            this.element.show();
        },

        Hide: function () {
            this.element.hide();
        },

        _destroy: function () {
            this.element.empty();
            this.element.detach();
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
