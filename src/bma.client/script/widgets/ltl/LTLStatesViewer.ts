/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlstatesviewer", {
        options: {
        },

        _create: function () {
            var that = this;
            var elem = this.element;
            var key_div = $('<div></div>').appendTo(elem);
            this.key_content = $('<div></div>').keyframecompact({canedit: true});
            this.key_content.appendTo(key_div);

            this.elempanel = $('<div></div>').appendTo(elem);
            this._create_elempanel();

            var inita = this.key_content.find('a').eq(0);
            inita.attr('href', '#ltlinit');

            var inittab = $('<div></div>').attr('id', 'ltlinit').appendTo(elem);
            this.keyframetable = $('<div></div>').appendTo(inittab);
            this.keyframetable.keyframetable();
            this.element.tabs();
        },


        _refresh: function () {
            this.element.tabs('refresh');
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

        addState: function (items) {
            this.key_content.keyframecompact('add', items);
            var li = this.key_content.find('li');
            var a = li.eq(li.length-1).children('a').eq(0);
            var id = 'ltlstatetab' + (li.length - 1).toString();
            var div = $('<div></div>').attr('id', id).appendTo(this.element);
            var did = $('<div></div>').keyframetable();
            did.appendTo(div);
            a.attr('href', '#' + id);
            this._refresh();
        },


    });
} (jQuery));

interface JQuery {
    ltlstatesviewer(): JQuery;
    ltlstatesviewer(settings: Object): JQuery;
    ltlstatesviewer(optionLiteral: string, optionName: string): any;
    ltlstatesviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}