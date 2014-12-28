/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.bmazoomslider", {
        options: {
            step: 10,
            value: 0,
            min: 0,
            max: 100
        },

        _create: function () {
            var that = this;
            this.element.addClass("zoomslider-container");

            var command = this.element.attr("data-command");

            var zoomplus = $('<img id="zoom-plus">')
                .addClass("hoverable")
                .attr("src", "images/zoomplus.svg")
                .appendTo(that.element);

            this.zoomslider = $('<div></div>')
                .addClass("bma-toolbarpanel-visibilityoptions-zoomslider")
                .appendTo(that.element);

            var zoomminus = $('<img id="zoom-minus">')
                .addClass("hoverable")
                .attr("src", "images/zoomminus.svg")
                .appendTo(that.element);

            this.zoomslider.slider({
                min: that.options.min,
                max: that.options.max,
                //step: that.options.step,
                value: that.options.value,
                change: function (event, ui) {
                    var isExternal = //Math.abs(that.options.value - ui.value) < 1 ||
                        ui.value > that.options.max ||
                        ui.value < that.options.min;
                    if (!isExternal) {
                        that.options.value = ui.value;
                        if (that.zoomslider.slider("option", "value") !== ui.value)
                            that.zoomslider.slider("option", "value", ui.value);
                    }
                    else {
                        var newval = ui.value > that.options.max ? that.options.max : that.options.min;
                        that.options.value = newval;
                        that.zoomslider.slider("option", "value", newval);
                    }
                    if (command !== undefined && command !== "") {
                        window.Commands.Execute(command, { value: ui.value, isExternal: isExternal });
                    }
            }
            });
            

            zoomplus.bind("click", function () {
                var val = that.zoomslider.slider("option", "value") - that.options.step;
                that.zoomslider.slider("option", "value", val);
            });

            zoomminus.bind("click", function () {
                var val = that.zoomslider.slider("option", "value") + that.options.step;
                that.zoomslider.slider("option", "value", val);
            });
        },

        _destroy: function () {
            var contents;

            // clean up main element
            this.element
                .removeClass("zoomslider-container");

            this.element.children().filter(".bma-elementspanel-visibilityoptions-zoomslider")
                .removeClass("bma-elementspanel-visibilityoptions-zoomslider")
                .removeUniqueId();

            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "value": 
                    if (this.options.value !== value) {
                        this.options.value = value;
                        this.zoomslider.slider("option", "value", value);
                    }
                    break;
                case "min":
                    this.options.min = value;
                    this.zoomslider.slider("option", "min", value);
                    break;
                case "max":
                    this.options.max = value;
                    this.zoomslider.slider("option", "max", value);
                    break;
            }
            this._super(key, value);
        }

    });
} (jQuery));

interface JQuery {
    bmazoomslider(): JQuery;
    bmazoomslider(settings: any): JQuery;
    bmazoomslider(optionLiteral: string, optionName: string): any;
    bmazoomslider(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}