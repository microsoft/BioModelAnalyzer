/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlstatesviewer", {
        options: {
        },

        _create: function () {
            var that = this;
            var elem = this.element;
            var key_div = $('<div></div>').appendTo(elem);
            var key_content = $('<div>KEYFRAMES Expanded</div>').keyframecompact({canedit: true});
            key_content.appendTo(key_div);

            this.elempanel = $('<div></div>').appendTo(elem);
            this._create_elempanel();
            this.keyframetable = $('<div></div>').appendTo(elem);
            this.keyframetable.keyframetable();
        },

        _create_elempanel() {
            var ul = $('<div></div>')
                .addClass('keyframe-panel')
                .appendTo(this.elempanel);
            var kfrms = window.KeyframesRegistry.Keyframes;
            for (var i = 0, l = kfrms.length; i < l; i++) {
                var img = kfrms[i].Icon;
                var li = $('<img>')
                    .addClass('keyframe-element')
                    .appendTo(ul);
                li.attr('src', img);
                li.draggable({

                    helper: function (event, ui) {
                        return $('<img>').attr('src', $(this).attr('src')).addClass('keyframe-element-draggable').appendTo('body');
                    },

                    scroll: false,

                    start: function (event, ui) {
                        window.Commands.Execute('KeyframeStartDrag', $(this).index());
                    }
                });
            } 
            ul.buttonset();
        },

        _destroy: function () {
            this.element.removeClass().empty();
        },

    });
} (jQuery));

interface JQuery {
    ltlstatesviewer(): JQuery;
    ltlstatesviewer(settings: Object): JQuery;
    ltlstatesviewer(optionLiteral: string, optionName: string): any;
    ltlstatesviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}