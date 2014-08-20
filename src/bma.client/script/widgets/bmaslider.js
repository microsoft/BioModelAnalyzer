/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.bmazoomslider", {
        options: {},
        _create: function () {
            var that = this;
            this.element.addClass("zoomslider-container");

            //var options = this.options;
            var zoomplus = $('<img id="zoom-plus" class="hoverable" src="images/zoomplus.png">').appendTo(that.element);
            var zoomslider = $('<div class="bma-elementspanel-visibilityoptions-zoomslider"></div>').addClass("bma-elementspanel-visibilityoptions-zoomslider").appendTo(that.element);
            zoomslider.slider();
            var zoomminus = $('<img id="zoom-minus" class="hoverable" src="images/zoomminus.png">').appendTo(that.element);

            zoomplus.bind("click", function () {
                zoomslider.slider("option", "value", zoomslider.slider("option", "value") + zoomslider.slider("option", "step"));
            });

            zoomminus.bind("click", function () {
                zoomslider.slider("option", "value", zoomslider.slider("option", "value") - zoomslider.slider("option", "step"));
            });
        },
        _destroy: function () {
            var contents;

            // clean up main element
            this.element.removeClass("zoomslider-container");

            this.element.children().filter(".bma-elementspanel-visibilityoptions-zoomslider").removeClass("bma-elementspanel-visibilityoptions-zoomslider").removeUniqueId();

            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            this._super(key, value);
        }
    });
}(jQuery));
//# sourceMappingURL=bmaslider.js.map
