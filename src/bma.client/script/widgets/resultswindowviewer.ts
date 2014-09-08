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



        _create: function () {
            var that = this;
            var options = this.options;
            var head = $('<div style="height: 28px"></div>').appendTo(this.element);
            
            var url = "";
            if (this.options.icon === "max") 
                url = "../../images/maximize.png";
            else
                if (this.options.icon === "min")
                    url = "../../images/minimize.png";
            else url = this.options.icon;
            
            this.button = $('<img>').attr("src", url).addClass('togglePopUpWindow').appendTo(head);
            this.button.bind("click", function () {
                if (options.icon === "max")
                    window.Commands.Execute("Expand", that.options.header);
                if (options.icon === "min")
                    window.Commands.Execute("Collapse", that.options.header);
            });

            this.header = $('<div></div>').text(options.header).appendTo(head);
            this.content = $('<div></div>').appendTo(this.element);
            if (options.content !== undefined)
                options.content.appendTo(this.content);
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
            if (key === "content") {
                this.options.content = value;
                this.content.empty();
                value.appendTo(this.content);
            }

            if (key === "header") {
                this.header.text(value);
            }
                
            this._super(key, value);
        }
    });
} (jQuery));

interface JQuery {
    resultswindowviewer (): JQuery;
    resultswindowviewer(settings: Object): JQuery;
    resultswindowviewer(fun: string): any;
    resultswindowviewer(optionLiteral: string, optionName: string): any;
    resultswindowviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 