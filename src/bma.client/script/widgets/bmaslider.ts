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
            //var options = this.options;

            var command = this.element.attr("data-command");

            var zoomplus = $('<img id="zoom-plus" class="hoverable" src="images/zoomplus.png">').appendTo(that.element);
            this.zoomslider = $('<div class="bma-elementspanel-visibilityoptions-zoomslider"></div>')
                .addClass("bma-elementspanel-visibilityoptions-zoomslider")
                .appendTo(that.element);

            this.zoomslider.slider({
                min: that.options.min,
                max: that.options.max,
                //step: that.options.step,
                value: that.options.value,
                change: function (event, ui) {
                    that.options.value = ui.value;

                    if (command !== undefined && command !== "") {
                        window.Commands.Execute(command, { value: ui.value });
                    }
            }
            });
            var zoomminus = $('<img id="zoom-minus" class="hoverable" src="images/zoomminus.png">').appendTo(that.element);

            zoomplus.bind("click", function () {
                var val = that.zoomslider.slider("option", "value") - that.options.step;//that.zoomslider.slider("option", "step");

                that.zoomslider.slider("option", "value", val);

                //if (command !== undefined && command !== "") {
                //    window.Commands.Execute(command, { value: val });
                //}
            });

            zoomminus.bind("click", function () {
                var val = that.zoomslider.slider("option", "value") + that.options.step;//that.zoomslider.slider("option", "step");

                that.zoomslider.slider("option", "value", val);

                //if (command !== undefined && command !== "") {
                //    window.Commands.Execute(command, { value: val });
                //}
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
                    this.options.value = value;
                    this.zoomslider.slider("option", "value", that.options.value);
                    //alert(value);
                    break;
            }
            
            this._super(key, value);
        }

    });
} (jQuery));

interface JQuery {
    bmazoomslider(): JQuery;
    bmazoomslider(settings: Object): JQuery;
    bmazoomslider(optionLiteral: string, optionName: string): any;
    bmazoomslider(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}