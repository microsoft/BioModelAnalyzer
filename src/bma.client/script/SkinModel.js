/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.skinmodel", {
        options: {
            text: "SkinModel"
        },
        _create: function () {
            var that = this;
            this.element.addClass("skinModelContainer");

            //var span = $('<span style="text-align:center;">' + that.options.text + '</span>').appendTo(that.element);
            var span = $('<button class="skinModel">' + that.options.text + '</button>').appendTo(that.element);

            //var span = $('<span style="position: absolute; text-align:center;">' + that.options.text + '</span>').appendTo(that.element);
            //background-image: -ms-linear-gradient(left, gainsboro,white);
            $('<button></button>').addClass("saveIcon").appendTo(that.element);
        },
        _setOption: function (key, value) {
            var that = this;
            switch (key) {
            }
            this._super("_setOption", key, value);
        },
        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }
    });
}(jQuery));
//# sourceMappingURL=skinmodel.js.map
