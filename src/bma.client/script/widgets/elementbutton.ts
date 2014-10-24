/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\elementsregistry.ts"/>


(function ($) {
    $.widget("BMA.elementbutton", {
        options: {
            image: undefined,
            command: undefined,
            commandparameter: undefined,
            description: undefined,
            toggle: false
        },

        _resetElement: function () {
            this.element.empty();
            this.element.addClass("bma-elementtoolbar-element")
            this.element.css("background-image", "url(" + this.options.image + ")");

            if (this.options.toggle) {

            }

            var command = this.options.command;
            if (command !== undefined) {
                this.element.click(function () {
                    window.Commands.Execute(command, this.options.commandparameter);
                });
            }
            //elemDiv.attr("title", this.options.description);
        },

        _create: function () {
            var that = this;
            this._resetElement();
        },

        _setOption: function (key, value) {
            if (this.options[key] !== value) {
                this._resetElement();
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));

interface ElementButton extends JQuery {
    
}

interface JQuery {
    elementbutton(): JQuery;
    elementbutton(settings: Object): JQuery;
} 