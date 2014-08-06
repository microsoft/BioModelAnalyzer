/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.toolbarpanel", {
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
                    var command = eval($(this).attr("data-command"));
                    if (command !== undefined && command.Execute !== undefined) {
                        $(this).bind("click", function (e) {
                            command.Execute();
                        });
                    }
                } catch (ex) {
                    console.log("Error binding to command: " + ex);
                }
            });
        },
        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }
    });
}(jQuery));
//# sourceMappingURL=toolbarpanel.js.map
