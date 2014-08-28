/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.popupwindow", {
        options: {
            content: $(),
            header: ""
        },

        refresh: function () {
            this.content.empty();
            this.options.content.appendTo(this.content);
        },

        _create: function () {
            var that = this;
            var options = this.options;
            //this.window = $('<div></div>').appendTo(this.element);
            this.button = $('<img class="togglePopUpWindow" src="../../images/maximize.png">').appendTo(this.element);
            $('<div>' + options.header + '</div>').appendTo(this.element);
            this.content = $('<div></div>').appendTo(this.element);
            

            //this.maxiwindow = $('<div></div>').addClass("popup-window").hide();
            //var minbutton = $('<img class="togglePopUpWindow" src="../../images/minimize.png">').appendTo(this.maxiwindow);
            //$('<div>' + options.header + '</div>').appendTo(this.maxiwindow);
            //options.maxcontent.appendTo(this.maxiwindow);
            //this.maxiwindow.appendTo('body');
            
            //this.button.bind("click", function () {
            //    that._toggle();
            //});
            //minbutton.bind("click", function () {
            //    that._toggle();
            //});

            //this.maxiwindow.draggable({ scroll: false, constraint: parent });
            this.refresh();
        },

        //_toggle: function () { 
        //    var that = this;
        //    //this.maxiwindow.toggle('size', { easing: 'easeInExpo' }, 200, function () {  });
        //    //that.window.toggle();
        //},

        button: function () {
            return this.button;
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            this._super(key, value);
            this.refresh();
        }

    });
} (jQuery));

interface JQuery {
    popupwindow (): JQuery;
    popupwindow(settings: Object): JQuery;
    popupwindow(optionLiteral: string, optionName: string): any;
    popupwindow(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 