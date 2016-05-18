/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.statescompact", {
        _stateButtons: null,
        _emptyStateAddButton: null,
        _emptyStatePlaceholder: null,
        _stateOptionsWindow: null,

        options: {
            states: [],
            commands: undefined,
            statesEditorExpand: undefined,
        },

        _create: function () {
            var that = this;

            this.element.addClass("state-compact");
            this._emptyStateAddButton = $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.element).click(function () {
                if (that.options.states != null && that.options.states !== undefined && that.options.states.length !== 0) {
                    this._stateButtons.show();
                    this._emptyStateAddButton.hide();
                    this._emptyStatePlaceholder.hide();
                } 
            });

            this._emptyStatePlaceholder = $("<div>start by defining some model states</div>").addClass("state-placeholder").appendTo(this.element);

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);
            //that.addContextMenu();

            if (this.options.states.length == 0) {
                this._stateButtons.hide();
            } else {
                this._emptyStateAddButton.hide();
                this._emptyStatePlaceholder.hide();
            }
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "states": {
                    this.options.states = [];
                    this._stateButtons.children().remove();
                    for (var i = 0; i < value.length; i++) {
                        if (value[i].formula.length != 0) {
                            this.options.states.push(value[i]);
                            this.createStateButton(value[i]);
                            //that.createToolTip(value[i], stateButton);
                        }
                    }
                    if (this.options.states.length == 0) {
                        this._stateButtons.hide();
                        this._emptyStateAddButton.show();
                        this._emptyStatePlaceholder.show();
                    } else {
                        this._stateButtons.show();
                        this._emptyStateAddButton.hide();
                        this._emptyStatePlaceholder.hide();
                    }

                    this.refresh();
                    break;
                }
                case "commands": {
                    this.options.commands = value;
                    break;
                }
                case "statesEditorExpand": {
                    this.options.statesEditorExpand = value;
                    break;
                }
                default: break;
            }
        },
        
        createStateButton: function (value) {
            var that = this;
            var stateButton = $("<div>" + value.name + "</div>").attr("data-state-name", value.name)
                .addClass("state-button").appendTo(this._stateButtons);
            var convertedState = that.convertForTooltip(value);
            stateButton.statetooltip({ state: convertedState });
            stateButton.hover(function (e) {
                var variablesIds = [];
                for (var i = 0; i < convertedState.formula.length; i++) {
                    var variableId = parseFloat(convertedState.formula[i].id);
                    if (variableId)
                        variablesIds.push(variableId);
                }
                
                window.Commands.Execute("HighlightContent", {
                    variableHighlightIds: variablesIds,
                    containerHighlightIds: [],
                });
            }, (e) => {
                window.Commands.Execute("UnhighlightContent", undefined);
            });
        },

        executeCommand: function (commandName, args) {
            if (this.options.commands) {
                this.options.commands.Execute(commandName, args);
            }
        },

        executeStatesEditorExpand: function () {
            if (this.options.statesEditorExpand) {
                this.options.statesEditorExpand();
            }
        },

        refresh: function () {
        }, 

        //addContextMenu: function () {
        //    var that = this;

        //    this._stateButtons.contextmenu({
        //        delegate: ".state-button",
        //        autoFocus: true,
        //        preventContextMenuForPopup: true,
        //        preventSelect: true,
        //        menu: [{ title: "Delete State", cmd: "DeleteState" }],
        //        beforeOpen: function (event, ui) {
        //            ui.menu.zIndex(50);

        //        },
        //        select: function (event, ui) {
        //            var args: any = {};
        //            args.command = ui.cmd;
        //            var state = ui.target.context;
        //            args.stateName = $(state).attr("data-state-name");
        //            for (var j = 0; j < that.options.states.length; j++) {
        //                if (that.options.states[j].name == $(state).attr("data-state-name")) {
        //                    args.stateIdx = j;
        //                    break;
        //                }
        //            }
        //            that.onContextMenuItemSelected(args);
        //        }
        //    });
        //},

        //onContextMenuItemSelected: function (args) {
        //    var that = this;

        //    that.options.states.splice(args.stateIdx, 1);
        //    that._stateButtons.find("[data-state-name='" + args.stateName + "']").remove();

        //    if (this.options.states.length == 0) {
        //        this._stateButtons.hide();
        //    } else {
        //        this._emptyStateAddButton.hide();
        //        this._emptyStatePlaceholder.hide();
        //    }

        //    window.Commands.Execute("KeyframesChanged", { states: that.options.states });
        //},

        convertForTooltip: function (state) {
            var formulas = [];
            for (var j = 0; j < state.formula.length; j++) {
                var formula = state.formula[j];
                var newFormula = {
                    variable: undefined,
                    id: undefined,
                    operator: undefined,
                    const: undefined
                };
                for (var i = 0; i < 5; i++) {
                    if (formula[i] !== undefined) {
                        switch (formula[i].type) {
                            case "variable": {
                                newFormula.variable = formula[i].value.name ? formula[i].value.name: "Unnamed";
                                newFormula.id = formula[i].value.id;                            
                                break;
                            }
                            case "const": {
                                newFormula.const = formula[i].value;
                                break;
                            }
                            case "operator": {
                                newFormula.operator = formula[i].value;
                                break;
                            }
                            default: break;
                        }
                    }
                }
                if (newFormula.variable && newFormula.const !== undefined && newFormula.operator)
                    formulas.push(newFormula);
            }
            return { description: state.description, formula: formulas };
        },

        //createToolTip: function (value, button) {
        //    var that = this;
        //    button.tooltip({
        //        tooltipClass: "state-tooltip",
        //        content: function () {
        //            var stateTooltip = $("<div></div>");//.addClass("state-tooltip");
        //            var description = $("<div>" + value.description + "</div>").appendTo(stateTooltip);
        //            if (value.description)
        //                description.show();
        //            else
        //                description.hide();
        //            var table = $("<table></table>").appendTo(stateTooltip);
        //            var tbody = $("<tbody></tbody>").appendTo(table);
        //            for (var j = 0; j < value.formula.length; j++) {
        //                var tr = that.getFormula(value.formula[j]);
        //                tr.appendTo(tbody);
        //            }
        //            return stateTooltip;
        //        },
        //        position: {
        //            at: "left-48px bottom",
        //        },
        //        show: null,
        //        hide: false,
        //        items: "div.state-button"
        //    });
        //},

        //getFormula: function (formula) {
        //    var tr = $("<tr></tr>");
        //    for (var i = 0; i < 5; i++) {
        //        if (formula[i] !== undefined) {
        //            switch (formula[i].type) {
        //                case "variable": {
        //                    var td = $("<td></td>").addClass("variable-name").appendTo(tr);
        //                    var img = $("<img>").attr("src", "../../images/state-variable.svg").appendTo(td);
        //                    var br = $("<br>").appendTo(td);
        //                    var variableName = $("<div>" + formula[i].value + "</div>").appendTo(td);
        //                    break;
        //                }
        //                case "const": {
        //                    var td = $("<td></td>").appendTo(tr);
        //                    var cons = $("<div>" + formula[i].value + "</div>").appendTo(td);
        //                    break;
        //                }
        //                case "operator": {
        //                    var td = $("<td></td>").appendTo(tr);
        //                    var op = $("<img>").attr("width", "30px").attr("height", "30px").appendTo(td);
        //                    switch (formula[i].value) {
        //                        case ">":
        //                            op.attr("src", "images/ltlimgs/mo.png");
        //                            break;
        //                        case ">=":
        //                            op.attr("src", "images/ltlimgs/moeq.png");
        //                            break;
        //                        case "<":
        //                            op.attr("src", "images/ltlimgs/le.png");
        //                            break;
        //                        case "<=":
        //                            op.attr("src", "images/ltlimgs/leeq.png");
        //                            break;
        //                        case "=":
        //                            op.attr("src", "images/ltlimgs/eq.png");
        //                            break;
        //                        case "!=":
        //                            op.attr("src", "images/ltlimgs/noeq.png");
        //                            break;
        //                        default: break;
        //                    }
        //                    break;
        //                }
        //                default: break;
        //            }
        //        }
        //    }
        //    return tr;
        //}
    });
} (jQuery));

interface JQuery {
    statescompact(): JQuery;
    statescompact(settings: Object): JQuery;
    statescompact(optionLiteral: string, optionName: string): any;
    statescompact(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    statescompact(methodName: string, methodValue: any): JQuery;
} 