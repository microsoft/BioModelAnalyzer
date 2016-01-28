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

            var zoomplus = $('<img>')
                .attr("id", "zoom-plus")
                .attr("src", "images/zoomplus.svg")
                .addClass("hoverable")
                .appendTo(that.element);

            this.zoomslider = $('<div></div>')
                .appendTo(that.element);

            var zoomminus = $('<img>')
                .attr("id", "zoom-minus")
                .attr("src", "images/zoomminus.svg")
                .addClass("hoverable")
                .appendTo(that.element);

            this.zoomslider.slider({
                min: that.options.min,
                max: that.options.max,
                //step: that.options.step,
                value: that.options.value,
                change: function (event, ui) {
                    var val = that.zoomslider.slider("option", "value");
                    var isExternal =
                        val > that.options.max ||
                        val < that.options.min;
                    if (!isExternal) {
                        that.options.value = val;
                    }
                    else {
                        var newval = val > that.options.max ? that.options.max : that.options.min;
                        that.options.value = newval;
                        that.zoomslider.slider("option", "value", newval);
                    }
                    if (command !== undefined && command !== "") {
                        window.Commands.Execute(command, { value: val, isExternal: isExternal });
                    }
            }
            });
            
            this.zoomslider.removeClass().addClass("zoomslider-bar");
            this.zoomslider.find('span').removeClass().addClass('zoomslider-pointer');

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
            this.element.removeClass("zoomslider-container");
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