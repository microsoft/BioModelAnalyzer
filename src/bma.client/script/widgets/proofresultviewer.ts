/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.proofresultviewer", {
        options: {
            issucceeded: undefined,
            time: undefined
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();
            var text = "";
            if (options.issucceeded === undefined)
                return;
            if (options.issucceeded === true)
                text = "Stabilizes";
            else text = "Not stabilizes";
            $('<div>' + text + '</div>').appendTo(that.element);
            if (options.time !== undefined) {
                $('<div>' + options.time + '</div>').appendTo(that.element);
            }
        },

        _create: function () {
            var that = this;
            this.element.addClass("zoomslider-container");
            //var options = this.options;
            this.refresh();
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
            this._super(key, value);
            this.refresh();
        }

    });
} (jQuery));

interface JQuery {
    proofresultviewer(): JQuery;
    proofresultviewer(settings: Object): JQuery;
}