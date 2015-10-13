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
        },

        _create: function () {
            var that = this;

            this.element.addClass("state-compact");
            this._emptyStateAddButton = $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.element).click(function () {
                that.executeCommand("AddFirstStateRequested", {});
            });

            this._emptyStatePlaceholder = $("<div>start by defining some model states</div>").addClass("state-placeholder").appendTo(this.element);

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element).click(function () {
                that.executeCommand("AddFirstStateRequested", {});
            });

            for (var i = 0; i < this.options.states.length; i++) {
                var stateButton = $("<div>" + this.options.states[i].name + "</div>").addClass("state-button").appendTo(this._stateButtons);
                that.createToolTip(this.options.states[i], stateButton);
            }

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
                            var stateButton = $("<div>" + value[i].name + "</div>").attr("data-state-name", value[i].name)
                                .addClass("state-button").appendTo(this._stateButtons);
                            that.createToolTip(value[i], stateButton);
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
                default: break;
            }
        },

        _setOptions: function (options) {
            this._super(options);
        }, 

        executeCommand: function (commandName, args) {
            if (this.options.commands !== undefined) {
                this.options.commands.Execute(commandName, args);
            }
        },

        refresh: function () {
        }, 

        addState: function (state) {

        },

        createToolTip: function (value, button) {
            var that = this;
            button.tooltip({
                content: function () {
                    var description = (value.description === undefined) ? "" : value.description;
                    var stateTooltip = $("<div>" + description + "<br>" + "</div>").addClass("state-tooltip");
                    var table = $("<table></table>").appendTo(stateTooltip);
                    var tbody = $("<tbody></tbody>").appendTo(table);
                    for (var j = 0; j < value.formula.length; j++) {
                        var tr = that.getFormula(value.formula[j]);
                        tr.appendTo(tbody);
                    }
                    return stateTooltip;
                },
                show: null,
                items: "div.state-button"
            });
        },

        getFormula: function (formula) {
            var tr = $("<tr></tr>");
            for (var i = 0; i < 5; i++) {
                if (formula[i] !== undefined) {
                    switch (formula[i].type) {
                        case "variable": {
                            var td = $("<td>" + formula[i].value + "</td>").addClass("variable-name").appendTo(tr);
                            var img = $("<img>").attr("src", "../../images/LTL-state-tool-var.svg").appendTo(td);
                            var br = $("<br>").appendTo(td);
                            break;
                        }
                        case "const": {
                            var td = $("<td>" + formula[i].value + "</td>").appendTo(tr);
                            break;
                        }
                        case "operator": {
                            var td = $("<td>" + formula[i].value + "</td>").appendTo(tr);
                            break;
                        }
                        default: break;
                    }
                }
            }
            return tr;
        }
    });
} (jQuery));

interface JQuery {
    statescompact(): JQuery;
    statescompact(settings: Object): JQuery;
    statescompact(optionLiteral: string, optionName: string): any;
    statescompact(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    statescompact(methodName: string, methodValue: any): JQuery;
} 