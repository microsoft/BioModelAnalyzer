(function ($) {
    $.widget("BMA.skinmodel", {
        options: {
            text: "SkinModel"
        },
        _create: function () {
            var that = this;
            this.element.addClass("skinModelContainer");

            var span = $('<button class="skinModel">' + that.options.text + '</button>').appendTo(that.element);

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
