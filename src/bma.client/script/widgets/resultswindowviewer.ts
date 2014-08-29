/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.resultswindowviewer", {
        options: {
            content: $(),
            header: "",
            icon: "",
            effects: { effect: 'size', easing: 'easeInExpo', duration: 200, complete: function () {} }
        },

        refresh: function () {
            this.content.empty();
            this.options.content.appendTo(this.content);
        },

        _create: function () {
            var that = this;
            var options = this.options;
            var url = "";
            if (this.options.icon === "max")
                url = "../../images/maximize.png";
            else
                if (this.options.icon === "min")
                    url = "../../images/minimize.png";
            else url = this.options.icon;
            
            this.button = $('<img class="togglePopUpWindow" src="' + url + '">').appendTo(this.element);
            this.button.bind("click", function () {
                window.Commands.Execute("Expand", that.options.header);
            });

            $('<div>' + options.header + '</div>').appendTo(this.element);
            this.content = $('<div></div>').appendTo(this.element);
            this.refresh();
        },

        toggle: function () { 
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
} (jQuery));

interface JQuery {
    resultswindowviewer (): JQuery;
    resultswindowviewer(settings: Object): JQuery;
    resultswindowviewer(optionLiteral: string, optionName: string): any;
    resultswindowviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 