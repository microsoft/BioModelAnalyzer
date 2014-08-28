/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.popupwindow", {
        options: {
            mincontent: $(),
            maxcontent: $(),
            header: ""
        },
        refresh: function () {
            var that = this;
            this.element.empty();
            this.options.content.appendTo(this.element);
        },
        _create: function () {
            var that = this;
            var options = this.options;
            this.miniwindow = $('<div></div>').appendTo(this.element);
            var maxbutton = $('<img class="togglePopUpWindow" style="" src="../../images/maximize.png">').appendTo(this.miniwindow);
            $('<div>' + options.header + '</div>').appendTo(this.miniwindow);
            options.mincontent.appendTo(this.miniwindow);

            this.maxiwindow = $('<div></div>').addClass("popup-window").hide();
            var minbutton = $('<img class="togglePopUpWindow" src="../../images/minimize.png">').appendTo(this.maxiwindow);
            $('<div>' + options.header + '</div>').appendTo(this.maxiwindow);
            options.maxcontent.appendTo(this.maxiwindow);
            this.maxiwindow.appendTo('body');

            maxbutton.bind("click", function () {
                that._toggle();
            });
            minbutton.bind("click", function () {
                that._toggle();
            });
            //this.maxiwindow.draggable({ scroll: false, constraint: parent });
        },
        _toggle: function () {
            var that = this;
            this.maxiwindow.toggle('size', { easing: 'easeInExpo' }, 200, function () {
                that.miniwindow.toggle();
            });
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
}(jQuery));
