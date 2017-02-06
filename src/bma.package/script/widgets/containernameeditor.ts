// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\functionsregistry.ts"/>

(function ($) {
    $.widget("BMA.containernameeditor", {
        options: {
            name: "name",
            oneditorclosing: undefined,
            placeholder: "Container Name"
        },
        
        _create: function () {
            var that = this;
            var closediv = $('<div></div>').addClass('close-icon').appendTo(that.element);
            closediv.bind("click", function () {
                that.element.hide();
                if (that.options.oneditorclosing !== undefined) {
                    that.options.oneditorclosing();
                }
            });
            this.element
                .addClass("container-name")
                .draggable({ containment: "parent", scroll: false });

            this.name = $('<input>')
                .attr("type", "text")
                .attr("size", 15)
                .attr("placeholder", this.options.placeholder)
                .appendTo(that.element);
            this.name.bind("input change", function () {
                that.options.name = that.name.val();
                window.Commands.Execute("ContainerNameEdited", {});
            });
            this.name.val(that.options.name);
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "name") {
                this.options.name = value;
                this.name.val(value);
            } else if (key === "placeholder") {
                this.name.attr("placeholder", value);
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
    containernameeditor(): JQuery;
    containernameeditor(settings: Object): JQuery;
    containernameeditor(fun: string, param: any): any;
    containernameeditor(optionLiteral: string, optionName: string): any;
    containernameeditor(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}

