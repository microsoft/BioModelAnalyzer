/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.compactproofviewer", {
        options: {
            content: $(),
            header: "",
            icon: "",
            effects: { effect: 'size', easing: 'easeInExpo', duration: 200, complete: function () {
                } }
        },
        refresh: function () {
            this.content.empty();
            this.options.content.appendTo(this.content);
        },
        _create: function () {
            var that = this;
            var options = this.options;
            var url = "";

            //this.window = $('<div></div>').appendTo(this.element);
            if (this.options.icon === "max")
                url = "../../images/maximize.png";
            else if (this.options.icon === "min")
                url = "../../images/minimize.png";
            else
                url = this.options.icon;

            this.button = $('<img class="togglePopUpWindow" src="' + url + '">').appendTo(this.element);
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
        toggle: function () {
            //var that = this;
            //this.maxiwindow.toggle('size', { easing: 'easeInExpo' }, 200, function () {  });
            //that.window.toggle();
            this.element.toggle(this.options.effects);
        },
        getbutton: function () {
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
//# sourceMappingURL=compactproofviewer.js.map
