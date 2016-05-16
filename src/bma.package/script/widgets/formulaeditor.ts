/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.formulaeditor", {

        _create: function () {
            var that = this;
            var root = this.element;

            //var title = $("<div></div>").addClass("window-title").text("Temporal Properties").appendTo(root);
            var toolbar = $("<div></div>").addClass("temporal-toolbar").width("calc(100% - 20px)").appendTo(root);
            
            //Adding states
            var states = $("<div></div>").addClass("state-buttons").width("calc(100% - 570px)").html("Variables<br>").appendTo(toolbar);
            this.statesbtns = $("<div></div>").addClass("btns").appendTo(states);
            //this._refreshStates();
            
            //Adding operators
            var operators = $("<div></div>").addClass("temporal-operators").html("Operators<br>").appendTo(toolbar);
            operators.width(350);
            var operatorsDiv = $("<div></div>").addClass("operators").appendTo(operators);

            var operatorsArr = [
                { Name: "&nbsp;+&nbsp;", OperandsCount: 2, isFunction: false },
                { Name: "&nbsp;-&nbsp;", OperandsCount: 2, isFunction: false },
                { Name: "&nbsp;*&nbsp;", OperandsCount: 2, isFunction: false },
                { Name: "&nbsp;/&nbsp;", OperandsCount: 2, isFunction: false },
                { Name: "AVG", OperandsCount: 2, isFunction: true },
                { Name: "MIN", OperandsCount: 2, isFunction: true },
                { Name: "MAX", OperandsCount: 2, isFunction: true },
                { Name: "CEIL", OperandsCount: 1, isFunction: false },
                { Name: "FLOOR", OperandsCount: 1, isFunction: false },
            ];

            for (var i = 0; i < operatorsArr.length; i++) {
                var operator = operatorsArr[i];

                var opDiv = $("<div></div>")
                    .addClass("operator")
                    .addClass("ltl-tp-droppable")
                    .attr("data-operator", operator.Name)
                    .css("z-index", 6)
                    .css("cursor", "pointer")
                    .appendTo(operatorsDiv);

                var spaceStr = "&nbsp;&nbsp;";
                if (operator.OperandsCount > 1 && !operator.isFunction) {
                    $("<div></div>").addClass("hole").appendTo(opDiv);
                    spaceStr = "";
                }
                
                var label = $("<div></div>").addClass("label").html(spaceStr + operator.Name).appendTo(opDiv);
                $("<div></div>").addClass("hole").appendTo(opDiv);
                if (operator.OperandsCount > 1 && operator.isFunction) {
                    //$("<div>&nbsp;&nbsp;</div>").appendTo(opDiv);
                    $("<div></div>").addClass("hole").appendTo(opDiv);
                }

                opDiv.draggable({
                    helper: "clone",
                    cursorAt: { left: 0, top: 0 },
                    opacity: 0.4,
                    cursor: "pointer",
                    start: function (event, ui) {
                        //that._executeCommand("AddOperatorSelect", $(this).attr("data-operator"));
                    }
                });

            }

            //Adding operators toggle basic/advanced
            /*
            var toggle = $("<div></div>").addClass("toggle").width(60).attr("align", "right").text("Advanced").appendTo(toolbar);
            toggle.click((args) => {
                if (toggle.text() === "Advanced") {
                    toggle.text("Basic");
                    operatorsDiv.height(98);
                    this.statesbtns.height(98);
                    if (this.drawingSurfaceContainerRef !== undefined) {
                        this.drawingSurfaceContainerRef.height("calc(100% - 113px - 30px - 34px)");
                    }
                } else {
                    toggle.text("Advanced");
                    operatorsDiv.height(64);
                    this.statesbtns.height(64);
                    if (this.drawingSurfaceContainerRef !== undefined) {
                        this.drawingSurfaceContainerRef.height("calc(100% - 113px - 30px)");
                    }
                }
                //$('body,html').css("zoom", 1.0000001);
                //root.height(root.height() + 1);
                this.updateLayout();
            });
            */

            //Adding drawing surface
            var svgDiv = $("<div></div>").css("background-color", "white").height(200).width("100%").appendTo(root);
        },

        _setOption: function (key, value) {
            var that = this;
            var needRefreshStates = false;
            switch (key) {
                default:
                    break;
            }
        },

        destroy: function () {
            this.element.empty();
        }

    })
} (jQuery));

interface JQuery {
    formulaeditor(): any;
    formulaeditor(settings: Object): any;
    formulaeditor(methodName: string, arg: any): any;
}