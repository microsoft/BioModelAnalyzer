/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.temporalpropertieseditor", {
        _drawingSurface: undefined,

        copyzone: undefined,
        deletezone: undefined,
        copyzonesvg: undefined,
        operation: undefined,

        options: {
            states: [],
            drawingSurfaceHeight: "calc(100% - 113px - 30px)",
            onfittoview: undefined,
            onaddstaterequested: undefined
        },

        _refreshStates: function () {
            var that = this;
            this.statesbtns.empty();
            for (var i = 0; i < this.options.states.length; i++) {
                var stateName = this.options.states[i].Name;
                var stateTooltip = that._convertForTooltip(that.options.states[i]);

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
                    cursorAt: { left: 0, top: 0 },
                    opacity: 0.4,
                    cursor: "pointer",
                    start: function (event, ui) {
                        that._executeCommand("AddStateSelect", $(this).attr("data-state"));
                    }

                });

                stateDiv.statetooltip({ state: stateTooltip });
            }

            var addState = $("<div></div>").addClass("state-button new").text("+").appendTo(that.statesbtns);
            addState.click(function () {
                if (that.options.onaddstaterequested !== undefined) {
                    that.options.onaddstaterequested();
                }
            });
        },

        _convertForTooltip: function (state) {
            var formulas = [];
            for (var i = 0; i < state.Operands.length; i++) {
                var op = state.Operands[i];
                var formula = {
                    variable: undefined,
                    operator: undefined,
                    const: undefined
                };

                (<any>op).LeftOperand.Name === undefined ? formula.const = op.LeftOperand.value : formula.variable = op.LeftOperand.Name;

                var invOperator = function (operator) {
                    switch (operator) {
                        case ">":
                            return "<";
                        case "<":
                            return ">";
                        case ">=":
                            return "<=";
                        case "<=":
                            return ">=";
                        default: break;
                    }
                    return operator;
                };

                if ((<any>op).MiddleOperand) {
                    formula.operator = invOperator((<any>op).LeftOperator);
                    formula.variable = (<any>op).MiddleOperand.Name;
                    formulas.push(formula);
                    formula.operator = (<any>op).RightOperator;
                } else formula.operator = formula.variable ? (<any>op).Operator : invOperator((<any>op).Operator);

                (<any>op).RightOperand.Name === undefined ? formula.const = (<any>op).RightOperand.Value : formula.variable = (<any>op).RightOperand.Name;
                formulas.push(formula);
            }
            return { description: state.Description, formula: formulas };
        },

        _addCustomState: function (statesbtns: JQuery, name, description, imagePath: string) {
            var that = this;

            var state = $("<div></div>")
                .addClass("state-button")
                .addClass("ltl-tp-droppable")
                .attr("data-state", name)
                .css("z-index", 6)
                .css("cursor", "pointer")
                .appendTo(statesbtns);
            $("<img>").attr("src", imagePath).appendTo(state);

            state.statetooltip({
                state: {
                    description: description, formula: undefined
                }
            });

            state.draggable({
                helper: "clone",
                cursorAt: { left: 0, top: 0 },
                opacity: 0.4,
                cursor: "pointer",
                start: function (event, ui) {
                    that._executeCommand("AddStateSelect", $(this).attr("data-state"));
                }

            });

            return state;
        },

        _create: function () {
            var that = this;

            var root = this.element;

            //var title = $("<div></div>").addClass("window-title").text("Temporal Properties").appendTo(root);
            var toolbar = $("<div></div>").addClass("temporal-toolbar").width("calc(100% - 20px)").appendTo(root);
            
            //Adding states
            var states = $("<div></div>").addClass("state-buttons").width("calc(100% - 570px)").html("States<br>").appendTo(toolbar);
            this.statesbtns = $("<div></div>").addClass("btns").appendTo(states);
            this._refreshStates();

            //Adding pre-defined states
            var conststates = $("<div></div>").addClass("state-buttons").width(130).html("&nbsp;<br>").appendTo(toolbar);
            var statesbtns = $("<div></div>").addClass("btns").appendTo(conststates);
            
            //Oscilation state
            this._addCustomState(statesbtns, "oscillationstate", "Part of an unstable loop.", "../images/oscillation-state.svg");

            //Selfloop state
            this._addCustomState(statesbtns, "selfloopstate", "Fixpoint of the network.", "../images/selfloop-state.svg");

            //True-state state
            this._addCustomState(statesbtns, "truestate", "True", "../images/true-state.svg");

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
                    cursorAt: { left: 0, top: 0 },
                    opacity: 0.4,
                    cursor: "pointer",
                    start: function (event, ui) {
                        that._executeCommand("AddOperatorSelect", $(this).attr("data-operator"));
                    }
                });

                //Separating advanced operators
                if (i === registry.Operators.length - 3) {
                    $("<br\>").appendTo(operatorsDiv);
                }
            }

            //Adding operators toggle basic/advanced
            var toggle = $("<div></div>").addClass("toggle").width(0).text("Advanced").appendTo(toolbar);
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

            //Adding drawing surface
            var drawingSurfaceCnt = $("<div></div>").addClass("bma-drawingsurfacecontainer").css("min-height", "200px").height(this.options.drawingSurfaceHeight).width("100%").appendTo(root);
            this.drawingSurfaceContainerRef = drawingSurfaceCnt;

            this._drawingSurface = $("<div></div>").addClass("bma-drawingsurface").appendTo(drawingSurfaceCnt);
            this._drawingSurface.drawingsurface({ useContraints: false });
            var drawingSurface = this._drawingSurface;
            drawingSurface.drawingsurface({
                gridVisibility: false,
                dropFilter: ["ltl-tp-droppable"],
                plotConstraint: {
                    MinWidth: 100,
                    MaxWidth: 1000
                }
            });

            if (that.options.commands !== undefined) {
                drawingSurface.drawingsurface({ commands: that.options.commands });
            }

            drawingSurface.drawingsurface({ visibleRect: { x: 0, y: 0, width: drawingSurfaceCnt.width(), height: drawingSurfaceCnt.height() } });
            
            //Adding drop zones
            /*
             <div class="temporal-dropzones">
	            <div class="dropzone copy">
	            	<img src="../images/LTL-copy.svg" alt="">
	            </div>
	            <div class="dropzone delete">
	            		<img src="../images/LTL-delete.svg" alt="">
	            </div>
	
            </div>
            */

            var dom = drawingSurface.drawingsurface("getCentralPart");

            var dropzonescnt = $("<div></div>").css("position", "absolute").css("bottom", 0).prependTo(dom.host);
            dropzonescnt.width("100%");

            var dropzones = $("<div></div>").addClass("temporal-dropzones").prependTo(dropzonescnt);
            dropzones.width("100%");

            /*
            this.copyzone = $("<div></div>").addClass("dropzone copy").css("z-index", InteractiveDataDisplay.ZIndexDOMMarkers + 1).appendTo(dropzones);
            this.copyzone.width("calc(50% - 15px - 3px)");

            var copyzonesvgdiv = $("<div></div>").width("100%").height("calc(100% - 20px)").css("margin-top", 10).css("margin-bottom", 10).appendTo(this.copyzone);

            copyzonesvgdiv.svg({
                loadURL: "../images/LTL-copy.svg",
                onLoad: function (svg) {
                    that.copyzonesvg = svg;

                    svg.configure({
                        height: "40px",
                        width: "40px"
                    });

                    if (that.options.copyzoneoperation !== undefined) {
                        that.updateCopyZoneIcon(that.options.copyzoneoperation);
                    }
                }
            });
            */

            this.deletezone = $("<div></div>").addClass("dropzone delete").css("z-index", InteractiveDataDisplay.ZIndexDOMMarkers + 1).appendTo(dropzones);
            this.deletezone.width("calc(100% - 30px)").css("margin-left", 15).css("margin-bottom", 0);
            $("<img>").attr("src", "../images/LTL-delete.svg").attr("alt", "").appendTo(this.deletezone);

            var fitDiv = $("<div></div>").addClass("fit-screen").css("z-index", InteractiveDataDisplay.ZIndexDOMMarkers + 1).css("cursor", "pointer").css("position", "relative").appendTo(dom.host);
            $("<img>").attr("src", "../images/screen-fit.svg").appendTo(fitDiv);
            fitDiv.click(function () {
                if (that.options.onfittoview !== undefined) {
                    that.options.onfittoview();
                }
            });
                
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
                addClass: "temporal-properties-contextmenu",
                delegate: drawingSurfaceCnt,//".bma-drawingsurface",
                autoFocus: true,
                preventContextMenuForPopup: true,
                preventSelect: true,
                //taphold: true,
                menu: [
                    { title: "Cut", cmd: "Cut", uiIcon: "ui-icon-scissors" },
                    { title: "Copy", cmd: "Copy", uiIcon: "ui-icon-copy" },
                    { title: "Paste", cmd: "Paste", uiIcon: "ui-icon-clipboard" },
                    { title: "Delete", cmd: "Delete", uiIcon: "ui-icon-trash" },
                    { title: "Export as", cmd: "Export", uiIcon: "ui-icon-export", children: [{ title: "json", cmd: "ExportAsJson" }, { title: "text", cmd: "ExportAsText" } ] },
                    { title: "Import", cmd: "Import", uiIcon: "ui-icon-import" }
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

            root.mousedown(function (e) {
                e.stopPropagation();
                drawingSurfaceCnt.contextmenu("close");
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
                    break;
                case "states":
                    needRefreshStates = true;
                    break;
                case "copyzoneoperation":
                    that.updateCopyZoneIcon(value);
                    break;
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

        updateCopyZoneIcon: function (op) {
            var that = this;
            /*
            if (that.operation !== undefined) {
                that.operation.Clear();
            }

            if (that.copyzonesvg !== undefined) {
                that.copyzonesvg.clear();

                if (op !== undefined) {
                    that.operation = new BMA.LTLOperations.OperationLayout(that.copyzonesvg, op, { x: 0, y: 0 });
                    var bbox = that.operation.BoundingBox;

                    that.copyzonesvg.configure({
                        height: "40px",
                        width: bbox.width,
                        viewBox: bbox.x + " " + (bbox.y - 5) + " " + bbox.width + " " + (bbox.height + 10),
                    }, true);

                    that.operation.Refresh();
                } else {
                    that.copyzonesvg.configure({
                        height: "40px",
                        width: "40px",
                        viewBox: 0 + " " + 0 + " " + 40 + " " + 40,
                    }, true);
                    that.copyzonesvg.load("../images/LTL-copy.svg", { width: 40, height: 40 });
                }
            }
            */
        },

        setcopyzonevisibility: function (isVisible) {
            /*
            if (isVisible) {
                this.copyzone.show();
            } else {
                this.copyzone.hide();
            }
            */
        },

        setdeletezonevisibility: function (isVisible) {
            if (isVisible) {
                this.deletezone.show();
            } else {
                this.deletezone.hide();
            }
        },


        highlightcopyzone: function (isHighlighted) {
            /*
            if (isHighlighted) {
                this.copyzone.addClass("hovered");
            } else {
                this.copyzone.removeClass("hovered");
            }
            */
        },

        highlightdeletezone: function (isHighlighted) {
            if (isHighlighted) {
                this.deletezone.addClass("hovered");
            } else {
                this.deletezone.removeClass("hovered");
            }
        },

        getcopyzonebbox: function () {
            /*
            var x = this._drawingSurface.drawingsurface("getPlotX", 15);
            var y = this._drawingSurface.drawingsurface("getPlotY", this._drawingSurface.height() - 10 - this.copyzone.height());
            var bbox = {
                x: x,
                y: y,
                width: this._drawingSurface.drawingsurface("getPlotX", 15 + this.copyzone.width()) - x,
                height: this._drawingSurface.drawingsurface("getPlotY", this._drawingSurface.height() - 10) - y
            };


            return bbox;
            */
            return {
                x: Number.POSITIVE_INFINITY, y: Number.POSITIVE_INFINITY, width: 0, height: 0
            };
        },

        getdeletezonebbox: function () {

            var x = this._drawingSurface.drawingsurface("getPlotX", 15);
            var y = this._drawingSurface.drawingsurface("getPlotY", this._drawingSurface.height() - 10 - this.deletezone.height());
            var bbox = {
                x: x,
                y: y,
                width: this._drawingSurface.drawingsurface("getPlotX", 15 + this.deletezone.width()) - x,
                height: this._drawingSurface.drawingsurface("getPlotY", this._drawingSurface.height() - 10) - y
            };


            return bbox;

        },

        updateLayout: function () {
            this._drawingSurface.drawingsurface("updateLayout");
        }

    });
} (jQuery));

interface JQuery {
    temporalpropertieseditor(): any;
    temporalpropertieseditor(settings: Object): any;
    temporalpropertieseditor(methodName: string, arg: any): any;
} 