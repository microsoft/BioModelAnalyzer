/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.temporalpropertieseditor", {
        _drawingSurface: undefined,

        options: {
            states: [],
            drawingSurfaceHeight: 500
        },

        _refreshStates: function () {
            var that = this;
            this.statesbtns.empty();
            for (var i = 0; i < this.options.states.length; i++) {
                var stateName = this.options.states[i].Name;

                var stateDiv = $("<div></div>")
                    .addClass("state-button")
                    .addClass("ltl-tp-droppable")
                    .attr("data-state", stateName)
                    .css("z-index", 6)
                    .css("cursor", "pointer")
                    .text(stateName)
                    .appendTo(that.statesbtns);

                stateDiv.draggable({
                    helper: "clone",
                    start: function (event, ui) {
                        $(this).draggable("option", "cursorAt", {
                            left: 0,   //Math.floor(ui.helper.width() / 2),
                            top: 0     //Math.floor(ui.helper.height() / 2)
                        });
                        that._executeCommand("AddStateSelect", $(this).attr("data-state"));
                    }
                });
            }
        },

        _create: function () {
            var that = this;

            var root = this.element;

            var title = $("<div></div>").addClass("window-title").text("Temporal Properties").appendTo(root);
            var toolbar = $("<div></div>").addClass("temporal-toolbar").appendTo(root);
            
            //Adding states
            var states = $("<div></div>").addClass("state-buttons").html("States<br>").appendTo(toolbar);
            this.statesbtns = $("<div></div>").addClass("btns").appendTo(states);
            this._refreshStates();
            //$("<div></div>").addClass("state-button new").text("+").appendTo(statesbtns);

            //Adding operators
            var operators = $("<div></div>").addClass("temporal-operators").html("Operators<br>").appendTo(toolbar);
            var operatorsDiv = $("<div></div>").addClass("operators").appendTo(operators);
            
            var registry = new BMA.LTLOperations.OperatorsRegistry();
            for (var i = 0; i < registry.Operators.length; i++) {
                var operator = registry.Operators[i];

                var opDiv = $("<div></div>")
                    .addClass("operator")
                    .addClass("ltl-tp-droppable")
                    .attr("data-operator", operator.Name)
                    .css("z-index", 6)
                    .css("cursor", "pointer")
                    .appendTo(operatorsDiv);

                var spaceStr = "&nbsp;&nbsp;";
                if (operator.OperandsCount > 1) {
                    $("<div></div>").addClass("hole").appendTo(opDiv);
                    spaceStr = "";
                }
                var label = $("<div></div>").addClass("label").html(spaceStr + operator.Name).appendTo(opDiv);
                $("<div></div>").addClass("hole").appendTo(opDiv);

                opDiv.draggable({
                    helper: "clone",
                    start: function (event, ui) {
                        $(this).draggable("option", "cursorAt", {
                            left: 0,   //Math.floor(ui.helper.width() / 2),
                            top: 0     //Math.floor(ui.helper.height() / 2)
                        });
                        that._executeCommand("AddOperatorSelect", $(this).attr("data-operator"));
                    }
                });
            }

            //Adding drawing surface
            var drawingSurfaceCnt = $("<div></div>").addClass("bma-drawingsurfacecontainer").height(this.options.drawingSurfaceHeight).appendTo(root);
            this._drawingSurface = $("<div></div>").addClass("bma-drawingsurface").appendTo(drawingSurfaceCnt);
            this._drawingSurface.drawingsurface();
            var drawingSurface = this._drawingSurface;
            drawingSurface.drawingsurface({
                gridVisibility: false,
                dropFilter: ["ltl-tp-droppable"]
            });

            if (that.options.commands !== undefined) {
                drawingSurface.drawingsurface({ commands: that.options.commands });
            }  
            
            drawingSurface.drawingsurface({ visibleRect: { x: 0, y: 0, width: drawingSurfaceCnt.width(), height: drawingSurfaceCnt.height() } });          

            //Context menu
            var holdCords = {
                holdX: 0,
                holdY: 0
            };

            $(document).on('vmousedown', function (event) {
                holdCords.holdX = event.pageX;
                holdCords.holdY = event.pageY;
            });

            drawingSurfaceCnt.contextmenu({
                delegate: drawingSurfaceCnt,//".bma-drawingsurface",
                autoFocus: true,
                preventContextMenuForPopup: true,
                preventSelect: true,
                taphold: true,
                menu: [
                    { title: "Cut", cmd: "Cut", uiIcon: "ui-icon-scissors" },
                    { title: "Copy", cmd: "Copy", uiIcon: "ui-icon-copy" },
                    { title: "Paste", cmd: "Paste", uiIcon: "ui-icon-clipboard" },
                    { title: "Delete", cmd: "Delete", uiIcon: "ui-icon-trash" }
                ],
                beforeOpen: function (event, ui) {
                    ui.menu.zIndex(50);
                    var x = holdCords.holdX || event.pageX;
                    var y = holdCords.holdX || event.pageY;
                    var left = x - drawingSurface.offset().left;
                    var top = y - drawingSurface.offset().top;

                    that._executeCommand("TemporalPropertiesEditorContextMenuOpening", {
                        left: left,
                        top: top
                    });
                },
                select: function (event, ui) {
                    var args: any = {};
                    var commandName = "TemporalPropertiesEditor" + ui.cmd;
                    var x = holdCords.holdX || event.pageX;
                    var y = holdCords.holdX || event.pageY;
                    args.left = x - drawingSurface.offset().left;
                    args.top = y - drawingSurface.offset().top;
                    that._executeCommand(commandName, args);
                }
            });
        },

        getContextMenuPanel: function () {
            return this.element.find(".bma-drawingsurfacecontainer");
        },

        getDrawingSurface: function () {
            return this.element.find(".bma-drawingsurface");
        },

        _executeCommand: function (commandName, args) {
            if (this.options.commands !== undefined) {
                this.options.commands.Execute(commandName, args);
            } else {
                window.Commands.Execute(commandName, args);
            }
        },

        _setOption: function (key, value) {
            var that = this;
            var needRefreshStates = false;
            switch (key) {
                case "commands":
                    this._drawingSurface.drawingsurface({ commands: value });
                case "states":
                    needRefreshStates = true;
                default:
                    break;
            }
            this._super(key, value);
            if (needRefreshStates) {
                this._refreshStates();
            }
        },

        destroy: function () {
            this.element.empty();
        },
    });
} (jQuery));

interface JQuery {
    temporalpropertieseditor(): any;
    temporalpropertieseditor(settings: Object): any;
    temporalpropertieseditor(methodName: string, arg: any): any;
} 