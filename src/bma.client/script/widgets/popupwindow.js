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
            this.button = $('<img class="togglePopUpWindow" src="../../images/minimize.png">').appendTo(this.element);
            $('<div>' + options.header + '</div>').appendTo(this.element);
            this.content = $('<div></div>').appendTo(this.element);
            this.refresh();
        },
        //toggle: function () {
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
}(jQuery));
//# sourceMappingURL=popupwindow.js.map
