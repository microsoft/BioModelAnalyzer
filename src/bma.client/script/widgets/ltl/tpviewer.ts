/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.tpformulaoptions", {

        _registry: null,
        _container: null,
        _commands: null,

        _create: function () {
            var that = this;
            that._container = $("<div></div>").addClass("ltl-tpotions-toolbar").appendTo(this.element);
        },

        _createContent: function () {
            var that = this;
            if (that._registry !== null && that._container !== null) {
                that._container.empty();
                var elements = that._registry.Operators();
                var elementPanel = that._container;
                for (var i = 0; i < elements.length; i++) {
                    var elem = elements[i];
                    var input = $("<input></input>")
                        .attr("type", "radio")
                        .attr("id", "btn-" + elem.Type)
                        .attr("name", "drawing-button")
                        .attr("data-type", elem.Type)
                        .appendTo(elementPanel);

                    var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
                    var img = $("<div></div>").addClass(elem.IconClass).attr("title", elem.Description).appendTo(label);
                }


                elementPanel.children("input").next().draggable({

                    helper: function (event, ui) {
                        var classes = $(this).children().children().attr("class").split(" ");
                        return $('<div></div>').addClass(classes[0]).addClass("draggable-helper-element").appendTo('body');
                    },

                    scroll: false,

                    start: function (event, ui) {
                        $(this).draggable("option", "cursorAt", {
                            left: Math.floor(ui.helper.width() / 2),
                            top: Math.floor(ui.helper.height() / 2)
                        });
                        $('#' + $(this).attr("for")).click();
                    }
                });

                $("#modelelemtoolbar input").click(function (event) {
                    window.Commands.Execute("AddLTLOperatorSelect", $(this).attr("data-type"));
                });

                elementPanel.buttonset();

            }
        },

        _subscribeToClick: function (inp, name) {
            inp.click(function () {
                if (this.commands !== null) {
                    this.commands.Execute("AddLTLOperatorSelect", name);
                }
            });
        },


        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "registry":
                    that._registry = value;
                    that._createContent();
                    break;
                case "commands":
                    that.commands = value;
                    break;
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));

interface JQuery {
    tpformulaoptions(): JQuery;
    tpformulaoptions(settings: Object): JQuery;
    tpformulaoptions(optionLiteral: string, optionName: string): any;
    tpformulaoptions(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
 