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
                var stateButton = $("<div>" + this.options.states[i].name + "</div>").addClass("state-button").appendTo(this._stateButtons).hover(function () {
                    //that._stateOptionsWindow = $("<div></div>").addClass("state-options-window").appendTo(that.element);
                    //var windowPointer = $("<div></div>").addClass("pointer").appendTo(that._stateOptionsWindow);
                    //var stateOptions = $("<div></div>").addClass("state-options").appendTo(that._stateOptionsWindow);
                });
            }

            if (this.options.states.length == 0) {
                this._stateButtons.hide();
            } else {
                this._emptyStateAddButton.hide();
                this._emptyStatePlaceholder.hide();
            }
        },

        _setOption: function (key, value) {
            switch (key) {
                case "states": {
                    this.options.states = [];
                    this._stateButtons.children().remove();
                    for (var i = 0; i < value.length; i++) {
                        if (value[i].formula.length != 0) {
                            this.options.states.push(value[i]);
                            var stateButton = $("<div>" + value[i].name + "</div>").attr("data-state-name", value[i].name)
                                .addClass("state-button").appendTo(this._stateButtons);
                            stateButton.tooltip({
                                content: value[i].tooltip,
                                show: null,
                                items: "div.state-button"
                            });
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