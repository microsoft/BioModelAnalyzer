/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.toolbarpanel", {
        _groups: {},
        _getList: function () {
            return this.buttonlist || this.element.find("ol,ul").eq(0);
            ;
        },
        _create: function () {
            var that = this;
            this.element.addClass("toolbarUL ");
            this.buttonList = this._getList();

            this.buttons = this.buttonList.find("li").children();
            this.buttons.each(function (ind) {
                try  {
                    var command = $(this).attr("data-command");
                    var commandParameter = $(this).attr("data-commandparameter");
                    if (command !== undefined) {
                        $(this).bind("click", function (e) {
                            window.Commands.Execute(command, commandParameter);
                        });
                    }
                } catch (ex) {
                    console.log("Error binding to command: " + ex);
                }
            });
        },
        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        },
        addDivButton: function (div) {
            var type = div.attr("data-type");

            if (type == "toggle") {
                var groupName = div.attr("data-group");

                if (groupName !== undefined) {
                    if (this._groups[groupName] === undefined)
                        this._groups[groupName] = [];

                    this._groups[groupName].push(div);
                }
            }

            div.appendTo(this.element);
        }
    });
}(jQuery));
//# sourceMappingURL=toolbarpanel.js.map
