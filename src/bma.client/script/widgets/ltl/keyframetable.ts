/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.keyframetable", {
        options: {
            id: 0,
        },

        _create: function () {
            var that = this;
            var elem = this.element.addClass('keyframe-table');

            var table = $('<table></table>').appendTo(elem);
            for (var i = 0; i < 5; i++) {
                var td = $('<td></td>').appendTo(table);
                td.droppable({
                    drop: function (event, ui) {
                        window.Commands.Execute('KeyframeDropped', { location: $(this) });
                    }
                });
            }
            var remove = $('<div></div>').addClass('remove-keyframe').appendTo(elem);
            remove.bind('click', function () {
                window.Commands.Execute('RemoveKeyframe', that.options.id);
            })
        },

        _destroy: function () {
            this.element.empty();
        }
    });
} (jQuery));

interface JQuery {
    keyframetable(): JQuery;
    keyframetable(settings: Object): JQuery;
    keyframetable(optionLiteral: string, optionName: string): any;
    keyframetable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 