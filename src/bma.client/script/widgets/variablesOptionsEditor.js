/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.bmaeditor", {
        options: {
            //variable: BMA.Model.Variable
            name: "name",
            rangeFrom: 0,
            rangeTo: 0
        },
        _resetElement: function () {
            var that = this;

            //var variable = that.options.variable;
            this.name.val(that.options.name);
            this.rangeFrom.val(that.options.rangeFrom);
            this.rangeTo.val(that.options.rangeTo);
            //elemDiv.attr("title", this.options.description);
        },
        _create: function () {
            var that = this;

            //if (that.options.variable.Id === undefined) {
            //    that.options.variable = new BMA.Model.Variable(147, 0, "type", 2, 8, "formula");
            //}
            //var variable = this.options.variable;
            this.element.addClass("newWindow");
            this.labletable = $('<table></table>').appendTo(that.element);
            var tr = $('<tr></tr>').appendTo(that.labletable);
            var td1 = $('<td></td>').appendTo(tr);
            var nameLabel = $('<label></label>').text("Name").appendTo(td1);

            //var td2 = $('<td></tr>').appendTo(labelsdiv);
            var td2 = $('<td></td>').appendTo(tr);
            var rangeLabel = $('<label></label>').text("Range").appendTo(td2);

            this._appendRangeInputs();

            //
            this._resetElement();
        },
        _appendRangeInputs: function () {
            var that = this;
            var tr = $('<tr></tr>').appendTo(that.labletable);
            var td1 = $('<td></td>').appendTo(tr);
            this.name = $('<input type="text" size="15">').appendTo(td1);

            var td2 = $('<td></td>').appendTo(tr);
            this.rangeFrom = $('<input type="text" min="0" max="100" size="1">').appendTo(td2);

            var td3 = $('<td></td>').appendTo(tr);
            var divtriangles1 = $('<div></div>').appendTo(td3);

            var upfrom = $('<div></div>').addClass("triangle-up").appendTo(divtriangles1);
            upfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());

                //if (valu < 100)
                that.rangeFrom.val(valu + 1);
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());

                //if (valu > 0)
                that.rangeFrom.val(valu - 1);
            });

            var td4 = $('<td></td>').appendTo(tr);
            this.rangeTo = $('<input type="text" min="0" max="100" size="1">').appendTo(td4);

            var td5 = $('<td></td>').appendTo(tr);

            var divtriangles2 = $('<div></div>').appendTo(td5);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());

                //if (valu < 100)
                that.rangeTo.val(valu + 1);
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());

                //if (valu > 0)
                that.rangeTo.val(valu - 1);
            });

            var td6 = $('<td></td>').appendTo(tr);
            $('<input class="ui-helper-hidden-accessible"></input>').attr("type", "checkbox").attr("id", "btn-editor-expander").appendTo(td6);

            var label = $('<label></label>').attr("for", "btn-editor-expander").appendTo(td6);
            $('<button class="editorExpander"></button>').appendTo(label);
            label.bind("click", function () {
                var checkbox = $('#' + label.attr("for"));
                var ch = checkbox.attr("checked");
                if (ch === undefined) {
                    checkbox.attr("checked", "");
                } else {
                    checkbox.removeAttr("checked");
                }
                label.children().toggleClass("editorExpanderChecked", "editorExpander");
                //if ($('#' + label.attr("for")).attr("checked"))
                //    label.children().addClass("editorExpanderChecked");
                //else
                //    label.children().addClass("editorExpander");
            });
        },
        _setOption: function (key, value) {
            if (this.options[key] !== value) {
                this._resetElement();
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        },
        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }
    });
}(jQuery));
//# sourceMappingURL=variablesOptionsEditor.js.map
