/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.keyframecompact", {

        options: {
            items: ['init'],
            canedit: false
        },

        _create: function () {
            this.element.addClass('keyframe-compact');
            this.content = $('<ul></ul>').appendTo(this.element);
            
            var addbttn = $('<div></div>')
                .addClass('keyframe-btn add')
                .appendTo(this.content);
            addbttn.bind('click', function () {
                window.Commands.Execute('AddKeyframe', {});
            });

            this.refresh();
        },

        refresh: function () {
            var that = this;
            this.content.children(':not(.add)');
            var items = this.options.items;
            items.forEach(function (i) {
                that._appendbutton(i);
            });
        },

        add: function (items) {
            var that = this;
            if (Array.isArray(items)) {
                items.forEach(function (i) {
                    that._appendbutton(i);
                    that.options.items.push('i');
                });
            }
            else {
                that._appendbutton(items);
                that.options.items.push(items);
            }
        },
        
        del: function (ind) {
            var that = this;
            this.options.items.splice(ind, 1);
            this.content.child().eq(ind + 1).detach();
        },
         
        _appendbutton: function (item) {
            var that = this;
            if (that.options.canedit) {
                var li = $('<li></li>').insertBefore(that.content.find('.add'));
                var btn = $('<a></a>')
                    .addClass('keyframe-btn mutable')
                    .appendTo(li);
                btn.bind('click', function () {
                    that.content.find('.keyframe-btn').removeClass('selected');
                    $(this).addClass('selected');
                    window.Commands.Execute("KeyframeSelected", { ind: $(this).index() });
                });
                var input = $('<input type="text" size="2">').appendTo(btn);
                input.val(item);
                input.bind('change input', function () {
                    var ind = $(this).parent().index();
                    that.options.items[ind] = $(this).val();
                    window.Commands.Execute("ChangedKeyframeName", { ind: ind, name: that.options.items[ind] });
                });
            }
            else {
                var li = $('<li></li>').insertBefore(that.content.find('.add'));
                var btn = $('<a></a>')
                    .addClass('keyframe-btn')
                    .appendTo(li);
                var name = $('<div></div>').text(item).appendTo(btn);
            }
        }

    });
} (jQuery));

interface JQuery {
    keyframecompact(): JQuery;
    keyframecompact(settings: Object): JQuery;
    keyframecompact(optionLiteral: string, optionName: string): any;
    keyframecompact(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}