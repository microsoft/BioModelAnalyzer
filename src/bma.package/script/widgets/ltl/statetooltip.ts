// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.statetooltip", {
        options: {
            state: undefined,
        },

        _create: function () {
            var that = this;

           // if (this.options.state && this.options.state.formula && this.options.state.formula.lenght !== 0) {
                this.element.tooltip({
                    tooltipClass: "state-tooltip",
                    position: {
                        at: "left-48px bottom",
                        collision: 'none',
                    },
                    content: function() {
                        return that.createContent();
                    },
                    show: null,
                    hide: false,
                    items: "div.state-button",
                    close: function (event, ui) {
                        that.element.data("ui-tooltip").liveRegion.children().remove();
                    },
                });
            //}

            this.refresh();
        },

        createContent: function () {
            var that = this;
            var stateTooltip = $("<div></div>");
            var description = $("<div>" + that.options.state.description + "</div>").appendTo(stateTooltip);
            if (that.options.state.description)
                description.show();
            else
                description.hide();
            if (that.options.state && that.options.state.formula && that.options.state.formula.lenght !== 0) {
                var table = $("<table></table>").appendTo(stateTooltip);
                var tbody = $("<tbody></tbody>").appendTo(table);
                var k = that.options.state.formula.length;
                for (var j = 0; j < k && j < 3; j++) {
                    var tr = that.getFormula(that.options.state.formula[j]);
                    tr.appendTo(tbody);
                }
                var message = "and " + (k - 3) + " more condition" + ((k - 3) > 1 ? "s" : "");
                var tooMuchStates = $("<div>" + message + "</div>").appendTo(stateTooltip);
                if (k > 3)
                    tooMuchStates.show();
                else
                    tooMuchStates.hide();
            }
            return stateTooltip;
        },

        getFormula: function (formula) {
            var tr = $("<tr></tr>");

            var variableTd = $("<td></td>").addClass("variable-name").appendTo(tr);
            var variableImg = $("<img>").attr("src", "../images/state-variable.svg").appendTo(variableTd);
            var br = $("<br>").appendTo(variableTd);
            var variableName = $("<div>" + formula.variable + "</div>").appendTo(variableTd);

            var operatorTd = $("<td></td>").appendTo(tr);
            var op = $("<img>").attr("width", "30px").attr("height", "30px").appendTo(operatorTd);
            switch (formula.operator) {
                case ">":
                    op.attr("src", "../images/ltlimgs/mo.png");
                    break;          
                case ">=":          
                    op.attr("src", "../images/ltlimgs/moeq.png");
                    break;          
                case "<":           
                    op.attr("src", "../images/ltlimgs/le.png");
                    break;          
                case "<=":          
                    op.attr("src", "../images/ltlimgs/leeq.png");
                    break;          
                case "=":          
                    op.attr("src", "../images/ltlimgs/eq.png");
                    break;          
                case "!=":          
                    op.attr("src", "../images/ltlimgs/noeq.png");
                    break;
                default: break;
            }

            var constTd = $("<td></td>").appendTo(tr);
            var cons = $("<div>" + formula.const + "</div>").appendTo(constTd);
            
            return tr;
        },

        refresh: function () {
            //var that = this;

            //if (this.options.state && this.options.state.formula && this.options.state.formula.lenght !== 0) {
            //    this.element.tooltip({
            //        content: function () {
            //            var stateTooltip = $("<div></div>");//.addClass("state-tooltip");
            //            var description = $("<div>" + that.options.state.description + "</div>").appendTo(stateTooltip);
            //            if (that.options.state.description)
            //                description.show();
            //            else
            //                description.hide();
            //            var table = $("<table></table>").appendTo(stateTooltip);
            //            var tbody = $("<tbody></tbody>").appendTo(table);
            //            for (var j = 0; j < that.options.state.formula.length; j++) {
            //                var tr = that.getFormula(that.options.state.formula[j]);
            //                tr.appendTo(tbody);
            //            }
            //            return stateTooltip;
            //        },
            //    });
            //}
        },

        //_setOption: function (key, value) {
        //    var that = this;
        //    switch (key) {
        //        case "state":
        //            if (value && value.formula && value.formula.length != 0)
        //                that.options.state = value;
        //            break;
        //        default: break;
        //    }

        //    this.refresh();
        //}
    });
} (jQuery));

interface JQuery {
    statetooltip(): JQuery;
    statetooltip(settings: Object): JQuery;
    statetooltip(optionLiteral: string, optionName: string): any;
    statetooltip(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    statetooltip(methodName: string, methodValue: any): JQuery;
} 
