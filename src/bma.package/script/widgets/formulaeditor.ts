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
            that.svgDiv = svgDiv;

            var pixofs = 0;
            svgDiv.svg({
                onLoad: function (svg) {
                    that._svg = svg;

                    svg.configure({
                        width: svgDiv.width() - pixofs,
                        height: svgDiv.height() - pixofs,
                        viewBox: "0 0 " + (svgDiv.width() - pixofs) + " " + (svgDiv.height() - pixofs),
                        preserveAspectRatio: "none meet"
                    }, true);


                    that._refresh();
                }
            });

            svgDiv.mousemove(function (arg) {
                if (that.operationLayout !== undefined) {
                    var opL = <BMA.LTLOperations.OperationLayout>that.operationLayout;
                    var parentOffset = $(this).offset(); 
                    var relX = arg.pageX - parentOffset.left;
                    var relY = arg.pageY - parentOffset.top;
                    var svgCoords = that._getSVGCoords(relX, relY);
                    opL.HighlightAtPosition(svgCoords.x, svgCoords.y);
                }
            });

            svgDiv.droppable();
        },

        _getSVGCoords: function (x, y) {
            var bbox = this.operationLayout.BoundingBox;
            var aspect = this.svgDiv.width() / this.svgDiv.height();
            var bboxx = -bbox.width / 2 - 10;
            var width = bbox.width + 20;
            var height = width / aspect;
            var bboxy = -height / 2;
            var svgX = width * x / this.svgDiv.width() + bboxx;
            var svgY = height * y / this.svgDiv.height() + bboxy;
            return {
                x: svgX,
                y: svgY
            };
        },

        _refresh: function () {
            var that = this;

            if (that._svg === undefined)
                return;

            if (that.options.operation !== undefined) {
                this.operationLayout = new BMA.LTLOperations.OperationLayout(that._svg, that.options.operation, { x: 0, y: 0 });
                var bbox = this.operationLayout.BoundingBox;
                var aspect = that.svgDiv.width() / that.svgDiv.height();
                var x = -bbox.width / 2 - 10;
                var width = bbox.width + 20;
                var height = width / aspect;
                var y = -height / 2;
                that._svg.configure({
                    viewBox: x + " " + y + " " + width + " " + height,
                }, true);
            } else {
                if (that.operationLayout !== undefined) {
                    that.operationLayout.IsVisible = false;
                }
            }
        },

        _setOption: function (key, value) {
            var that = this;
            var needRefreshStates = false;
            switch (key) {
                case "operation":
                    break;
                default:
                    break;
            }

            that._refresh();
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