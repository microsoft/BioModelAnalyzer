/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _stateButtons: null,
        _addStateButton: null,
        _toolbar: null,
        _keyframes: null,
        _description: null,
        _ltlStates: null,
        _ltlAddFormulaButton: null,
        _activeState: null,

        options: {
            variables: [],
            states: [],
            minConst: -99,
            maxConst: 100,
            commands: undefined,
            onStatesUpdated: undefined,
            onComboBoxOpen: undefined,
        },

        _create: function () {
            var that = this;

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            that._initStates();

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons).click(function () {
                that.addState();
                that.executeStatesUpdate({ states: that.options.states, changeType: "stateAdded" });
            });

            this._description = $("<input></input>").attr("type", "text").addClass("state-description").attr("size", "15").attr("data-row-type", "description")
                .attr("placeholder", "Description").appendTo(this.element);
            this._description.bind("input change", function () {
                var idx = that.options.states.indexOf(that._activeState);
                that.options.states[idx].description = this.value;
                that._activeState.description = this.value;

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });

            this._ltlStates = $("<div></div>").addClass("LTL-states").appendTo(this.element);

            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "add").appendTo(this._ltlStates);
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            this._ltlAddFormulaButton = $("<td>+</td>").addClass("LTL-line-new").appendTo(tr).click(function () {
                var idx = that.options.states.indexOf(that._activeState);
                var emptyFormula = [undefined, undefined, undefined, undefined, undefined];
                that.options.states[idx].formula.push(emptyFormula);
                that.addFormula();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });
        },

        _initStates: function () {
            var that = this;
            for (var i = this.options.states.length - 1; i >= 0; i--) {
                var stateButton = $("<div>" + this.options.states[i].name + "</div>").attr("data-state-name", this.options.states[i].name)
                    .addClass("state-button").addClass("state").prependTo(this._stateButtons).click(function () {
                        that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                        for (var j = 0; j < that.options.states.length; j++) {
                            if (that.options.states[j].name == $(this).attr("data-state-name")) {
                                that._activeState = that.options.states[j];
                                break;
                            }
                        }
                        that.refresh();
                    });
            }
        },

        addState: function (state = null) {
            var that = this;
            var stateName;
            var idx;
            if (state == null) {
                var k = this.options.states.length;
                idx = k;
                var lastStateName = "";
                for (var i = 0; i < k; i++) {
                    var lastStateIdx = (lastStateName && lastStateName.length > 1) ? parseFloat(lastStateName.slice(1)) : 0;
                    var stateIdx = this.options.states[i].name.length > 1 ? parseFloat(this.options.states[i].name.slice(1)) : 0;

                    if (stateIdx >= lastStateIdx) {
                        lastStateName = (lastStateName && stateIdx == lastStateIdx
                            && lastStateName.charAt(0) > this.options.states[i].name.charAt(0)) ?
                            lastStateName : this.options.states[i].name;
                    }
                }

                var charCode = lastStateName ? lastStateName.charCodeAt(0) : 65;
                var n = (lastStateName && lastStateName.length > 1) ? parseFloat(lastStateName.slice(1)) : 0;

                if (charCode >= 90) {
                    n++;
                    charCode = 65;
                } else if (lastStateName) charCode++;

                stateName = n ? String.fromCharCode(charCode) + n : String.fromCharCode(charCode);

                var newState = {
                    name: stateName,
                    description: "",
                    formula: [
                        [
                            undefined,
                            undefined,
                            undefined,
                            undefined,
                            undefined
                        ]
                    ],
                };

                this.options.states.push(newState);
            } else {
                stateName = state.name;
                idx = this.options.states.indexOf(state);
            }

            var state = $("<div>" + stateName + "</div>").attr("data-state-name", stateName).addClass("state-button").addClass("state").click(function () {
                that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                for (var j = 0; j < that.options.states.length; j++) {
                    if (that.options.states[j].name == $(this).attr("data-state-name")) {
                        that._activeState = that.options.states[j];
                        break;
                    }
                }
                that.refresh();
            });

            if (this._activeState != null)
                that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
            this._activeState = this.options.states[idx];
            state.insertBefore(this._stateButtons.children().last());

            this.refresh();
        },

        addFormula: function (formula = null) {
            var that = this;
            var table = $("<table></table>").addClass("state-condition").appendTo(this._ltlStates);
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            var variableTd = $("<td></td>").addClass("variable").appendTo(tr);
            var variableImg = $("<td></td>").attr("src", "../LTL-state-tool-var.svg").appendTo(variableTd);
            var selectedVariable = $("<p></p>").appendTo(variableTd);
            var expandButton = $("<div></div>").addClass('inputs-expandbttn').appendTo(variableTd);
            that.createVariablePicker();
           
            //selector for variables

            var operatorTd = $("<td></td>").addClass("operator").appendTo(tr);
            //selector for operator

            var constTd = $("<td></td>").addClass("const").appendTo(tr);
            var constInput = $("<input></input>").attr("type", "text").appendTo(constTd);
            //input for const

            var removeTd = $("<td></td>").addClass("LTL-line-del").appendTo(tr);
            var removeIcon = $("<img>").attr("src", "../images/state-line-del.svg").appendTo(removeTd);

            if (formula) {
                //filling fields and 
                selectedVariable.text(formula[0].value.variable);
                
            }

            table.insertBefore(this._ltlStates.children().last());
        },

        createVariablePicker: function () {
            


        },

        refresh: function () {
            var that = this;
            this._stateButtons.find("[data-state-name='" + this._activeState.name + "']").addClass("active");
            $(this._ltlStates).find("[data-row-type='condition']").remove();

            this._description.val(this._activeState.description);
            for (var i = 0; i < this._activeState.formula.length; i++) {
                this.addFormula(this._activeState.formula[i]);
            }
        },

        _setOptions: function (key, value) {
            var that = this;
            this._super(key, value);

            switch (key) {
                case "variables": {
                    this.options.variables = [];
                    for (var i = 0; i < value.length; i++)
                        this.options.variables.push(value[i]);
                    break;
                }
                case "states": {
                    this.options.states = [];
                    this._stateButtons.children(".state").remove();

                    for (var i = 0; i < value.length; i++) {
                        this.options.states.push(value[i]);
                        if (value[i].formula.length == 0)
                            value[i].formula.push([undefined, undefined, undefined, undefined, undefined]);
                        //this.addState(value[i]);
                    }
                    

                    //this.options.states = value;
                    if (this.options.states.length == 0) {
                        that.addState();
                        that.executeStatesUpdate({ states: that.options.states, changeType: "stateAdded" });
                    } else {
                        this._initStates();
                        if (this._activeState != null)
                            that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                        this._activeState = this.options.states[0];
                        this.refresh();
                    }
                    break;
                }
                default: break;
            }
        },
    });
} (jQuery));

interface JQuery {
    stateseditor(): JQuery;
    stateseditor(settings: Object): JQuery;
    stateseditor(optionLiteral: string, optionName: string): any;
    stateseditor(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    stateseditor(methodName: string, methodValue: any): JQuery;
} 