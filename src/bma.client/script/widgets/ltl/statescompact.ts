/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.statescompact", {
        _stateButtons: null,
        _addStateButton: null,
        _emptyStateAddButton: null,
        _emptyStatePlaceholder: null,
        _stateOptionsWindow: null,

        _options: {
            states: []
        },

        _create: function () {
            var that = this;

            this.element.addClass("state-compact");
            this._emptyStateAddButton = $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.element);
            this._emptyStatePlaceholder = $("<div>start by defining some model states</div>").addClass("state-placeholder").appendTo(this.element);

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            var newState = {
                name: "A",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        undefined,
                        undefined,
                        undefined
                    ]
                ]
            };

            this._options.states.push(newState);

            for (var i = 0; i < this._options.states.length; i++) {
                var stateButton = $("<div>" + this._options.states[i].name + "</div>").addClass("state-button").appendTo(this._stateButtons).hover(function () {
                    //that._stateOptionsWindow = $("<div></div>").addClass("state-options-window").appendTo(that.element);
                    //var windowPointer = $("<div></div>").addClass("pointer").appendTo(that._stateOptionsWindow);
                    //var stateOptions = $("<div></div>").addClass("state-options").appendTo(that._stateOptionsWindow);
                });
            }

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons);

            if (this._options.states.length == 0) {
                this._stateButtons.hide();
            } else {
                this._emptyStateAddButton.hide();
                this._emptyStatePlaceholder.hide();
            }
        },

        _setOption: function (key, value) {
            switch (key) {
                case "states": {
                    this._options.states = value;
                    this.refresh();
                    break;
                }
                default: break;
            }
        },

        _setOptions: function (options) {
            this._super(options);
        }, 

        refresh: function () {
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