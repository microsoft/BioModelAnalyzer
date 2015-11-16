/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.selector", {
        options: {
            data: undefined,
            mode: undefined,
        },

        _create: function () {
            switch (this.options.mode) {
                case "variablePicker": {
                    this.createVariablePicker();
                    break;
                }
                case "operatorPicker": {
                    this.createOperatorPicker();
                    break;
                }
                default: break;
            }
        },

        createVariablePicker: function () {
            var variablePicker = $("<div></div>").addClass("variable-picker").appendTo('body').hide();
            var table = $("<table></table>").appendTo(variablePicker);
            var tbody = $("<tbody></tbody>").appendTo(table);

            var tr = $("<tr></tr>").appendTo(tbody);
            var tdContainer = $("<td></td>").appendTo(tr);
            var imgContainer = $("<img></img>").attr("src", "../images/container.svg").appendTo(tdContainer);
            var tdVariable = $("<td></td>").appendTo(tr);
            var imgVariable = $("<img></img>").attr("src", "../images/variable.svg").appendTo(tdVariable);


        },

        createOperatorPicker: function () {
        },
    })
})(jQuery);