/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.resultswindowviewer", {
        options: {
            content: $(),
            header: "",
            icon: "",
            effects: { effect: 'size', easing: 'easeInExpo', duration: 200, complete: function () {
                } }
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();
            var table = $('<table></table>').width("100%").appendTo(this.element);
            var tr = $('<tr></tr>').appendTo(table);
            var td1 = $('<td></td>').appendTo(tr);
            var td2 = $('<td></td>').appendTo(tr);
            var url = "";
            if (this.options.icon === "max")
                url = "../../images/maximize.png";
            else if (this.options.icon === "min")
                url = "../../images/minimize.png";
            else
                url = this.options.icon;

            this.button = $('<img>').attr("src", url).addClass('togglePopUpWindow').appendTo(td2);
            this.button.bind("click", function () {
                if (options.icon === "max")
                    window.Commands.Execute("Expand", that.options.header);
                if (options.icon === "min")
                    window.Commands.Execute("Collapse", that.options.header);
            });

            this.header = $('<div></div>').text(options.header).appendTo(td1);

            //this.content = $('<div></div>').appendTo(this.element);
            if (options.content !== undefined) {
                options.content.clone().appendTo(this.element);
            }
        },
        _create: function () {
            this.refresh();
        },
        toggle: function () {
            this.element.toggle(this.options.effects);
        },
        getbutton: function () {
            return this.button;
        },
        _destroy: function () {
            this.element.empty();
            alert("destroy window");
        },
        _setOption: function (key, value) {
            var that = this;
            if (key === "content")
                this.options.content = value;
            if (key === "header")
                this.options.header = value;

            this._super(key, value);
            this.refresh();
        }
    });
}(jQuery));
//# sourceMappingURL=resultswindowviewer.js.map
