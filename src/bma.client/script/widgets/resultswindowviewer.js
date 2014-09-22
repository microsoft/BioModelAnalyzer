/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.resultswindowviewer", {
        options: {
            content: $(),
            header: "",
            icon: "",
            effects: { effect: 'size', easing: 'easeInExpo', duration: 200, complete: function () {
                } },
            tabid: ""
        },
        reseticon: function () {
            var that = this;
            var options = this.options;
            that.icontd.empty();
            var url = "";
            if (this.options.icon === "max")
                url = "../../images/maximize.png";
            else if (this.options.icon === "min")
                url = "../../images/minimize.png";
            else
                url = this.options.icon;

            this.button = $('<img>').attr("src", url).addClass('togglePopUpWindow').appendTo(that.icontd);
            this.button.bind("click", function () {
                if (options.icon === "max")
                    window.Commands.Execute("Expand", that.options.tabid);
                if (options.icon === "min")
                    window.Commands.Execute("Collapse", that.options.tabid);
            });
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.content.empty();
            if (options.content !== undefined) {
                options.content.appendTo(that.content);
            }
        },
        _create: function () {
            var that = this;
            var options = this.options;
            var table = $('<table></table>').width("100%").appendTo(this.element);
            var tr = $('<tr></tr>').appendTo(table);
            this.header = $('<td></td>').text(options.header).appendTo(tr);
            this.icontd = $('<td></td>').appendTo(tr);

            //this.header = $('<div></div>').text(options.header).appendTo(td1);
            this.content = $('<div></div>').appendTo(this.element);
            this.reseticon();
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
        },
        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "header":
                    this.header.text(value);
                    break;
                case "content":
                    if (this.options.content !== value) {
                        this.options.content = value;
                        this.refresh();
                    }
                    break;
                case "icon":
                    this.options.icon = value;
                    this.reseticon();
                    break;
            }

            this._super(key, value);
            //this.refresh();
        }
    });
}(jQuery));
//# sourceMappingURL=resultswindowviewer.js.map
