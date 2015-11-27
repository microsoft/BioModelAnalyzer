﻿/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _stateToolbar: null,
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

            this._stateToolbar = $("<div></div>").addClass("state-toolbar").appendTo(this.element);
            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this._stateToolbar);

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

            this._ltlStates = $("<div></div>").addClass("LTL-state").appendTo(this.element);

            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "add").appendTo(this._ltlStates);
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            this._ltlAddFormulaButton = $("<td>+</td>").addClass("LTL-line-new").appendTo(tr).click(function () {
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
                            undefined
                        ],
                    ]
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

            var stateIdx = that.options.states.indexOf(that._activeState);
            var formulaIdx = that.options.states[stateIdx].formula.indexOf(formula);

            if (formula == null) {
                that.options.states[stateIdx].formula.push(formula);
                formulaIdx = that.options.states[stateIdx].formula.length - 1;
            } 

            formula = formula && formula[0] && formula[1] && formula[2] ? formula : [
                {
                    type: "variable", value: formula && formula[0] && formula[0].value ? formula[0].value : {
                        container: undefined,
                        variable: undefined
                    },
                },
                { type: "operator", value: formula && formula[1] && formula[1].value ? formula[1].value : "=" },
                { type: "const", value: formula && formula[2] && formula[2].value ? formula[2].value : 0 },
            ];

            if (formula.length == 5) {
                var secondEq = [
                    {
                        type: "variable", value: formula[2] && formula[2].value ? formula[2].value : {
                            container: undefined,
                            variable: undefined
                        },
                    },
                    { type: "operator", value: formula[3] && formula[3].value ? formula[3].value : "<" },
                    { type: "const", value: formula[4] && formula[4].value ? formula[4].value : 0 },
                ];
                that.options.states[stateIdx].formula.splice(formulaIdx + 1, 0, secondEq);

                switch (formula[1].value) {
                    case ">":
                        formula[1].value = "<";
                    case "<":
                        formula[1].value = ">";
                    default: break;
                }

                formula = [
                    {
                        type: "variable", value: formula[2] && formula[2].value ? formula[2].value : {
                            container: undefined,
                            variable: undefined
                        },
                    },
                    { type: "operator", value: formula[1] && formula[1].value ? formula[1].value : ">" },
                    { type: "const", value: formula[0] && formula[0].value ? formula[0].value : 0 },
                ];
            } 

            that.options.states[stateIdx].formula[formulaIdx] = formula;

            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "formula");
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);
            
            var variableTd = $("<td></td>").addClass("variable").appendTo(tr);
            that.createVariablePicker(variableTd, formula[0]);
           

            var operatorTd = $("<td></td>").addClass("operator").appendTo(tr);
            var operatorImg = $("<img>").appendTo(operatorTd);

            var setOperatorValue = function (value) {
                switch (value) {
                    case ">":
                        operatorImg.attr("src", "images/ltlimgs/mo.png");
                        break;
                    case ">=":
                        operatorImg.attr("src", "images/ltlimgs/moeq.png");
                        break;
                    case "<":
                        operatorImg.attr("src", "images/ltlimgs/le.png");
                        break;
                    case "<=":
                        operatorImg.attr("src", "images/ltlimgs/leeq.png");
                        break;
                    case "=":
                        operatorImg.attr("src", "images/ltlimgs/eq.png");
                        break;  
                    case "<>":
                        value = ">";
                        formula[1].value = value;
                        operatorImg.attr("src", "images/ltlimgs/mo.png");
                        var secondEq = [
                            {
                                type: "variable", value: formula[0] && formula[0].value ? formula[0].value : {
                                    container: undefined,
                                    variable: undefined
                                },
                            },
                            { type: "operator", value: "<" },
                            { type: "const", value: 0 },
                        ];
                        that.options.states[stateIdx].formula.splice(formulaIdx + 1, 0, secondEq);
                        that.refresh();
                        break;                      
                    default: break;
                }
                formula[1].value = value;
            }

            that.createOperatorPicker(operatorTd, setOperatorValue);
            setOperatorValue(formula[1].value);


            var constTd = $("<td></td>").addClass("const").appendTo(tr);
            var constInput = $("<input></input>").addClass("number-input").attr("type", "text").attr("value", formula[2] ? formula[2].value : 0).appendTo(constTd);
            constInput.bind("input change", function () {
                if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                formula[2].value = this.value;

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });

            var removeTd = $("<td></td>").addClass("LTL-line-del").appendTo(tr).click(function () {
                var stateIndex = that.options.states.indexOf(that._activeState);
                var tableIndex = table.index();
                that.options.states[stateIndex].formula.splice(tableIndex, 1);
                $(table).remove();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });

            var removeIcon = $("<img>").attr("src", "../images/state-line-del.svg").appendTo(removeTd);
            
            table.insertBefore(this._ltlStates.children().last());
        },

        createOperatorPicker: function (operatorTd, setOperatorValue) {
            var that = this;

            var firstLeft = $(operatorTd).offset().left;
            var firstTop = $(operatorTd).offset().top;

            var operatorExpandButton = $("<div></div>").addClass('arrow-down').appendTo(operatorTd);
            var operatorSelector = $("<div></div>").addClass("operator-picker").appendTo('body').hide();
            operatorSelector.offset({ top: firstTop + 47, left: firstLeft });

            var greDiv = $("<div></div>").attr("data-operator-type", ">").appendTo(operatorSelector);
            var gre = $("<img>").attr("src", "images/ltlimgs/mo.png").appendTo(greDiv);

            var greqDiv = $("<div></div>").attr("data-operator-type", ">=").appendTo(operatorSelector);
            var greq = $("<img>").attr("src", "images/ltlimgs/moeq.png").appendTo(greqDiv);

            var lesDiv = $("<div></div>").attr("data-operator-type", "<").appendTo(operatorSelector);
            var les = $("<img>").attr("src", "images/ltlimgs/le.png").appendTo(lesDiv);

            var lesqDiv = $("<div></div>").attr("data-operator-type", "<=").appendTo(operatorSelector);
            var lesq = $("<img>").attr("src", "images/ltlimgs/leeq.png").appendTo(lesqDiv);

            var equDiv = $("<div></div>").attr("data-operator-type", "=").appendTo(operatorSelector);
            var equ = $("<img>").attr("src", "images/ltlimgs/eq.png").appendTo(equDiv);

            var rangeDiv = $("<div></div>").attr("data-operator-type", "<>").appendTo(operatorSelector)
            var range = $("<img>").attr("src", "images/range.png").appendTo(rangeDiv);

            operatorExpandButton.bind("click", function () {
                if (operatorSelector.is(":hidden")) {
                    var offLeft = $(operatorTd).offset().left - firstLeft;
                    var offTop = $(operatorTd).offset().top - firstTop;
                    operatorSelector.offset({ top: offTop, left: offLeft });
                    firstLeft = $(operatorTd).offset().left;
                    firstTop = $(operatorTd).offset().top;

                    operatorSelector.show();
                    //operatorExpandButton.addClass('inputs-list-header-expanded');
                } else {
                    operatorSelector.hide();
                    //operatorExpandButton.removeClass('inputs-list-header-expanded');
                }
            });

            operatorSelector.children().bind("click", function () {
                var newOperator = $(this).attr("data-operator-type");
                setOperatorValue(newOperator);
                operatorSelector.hide();
                //operatorExpandButton.removeClass('inputs-list-header-expanded');

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });

            $(document).mousedown(function (e) {
                if (!operatorSelector.is(":hidden")) {
                    if (!operatorTd.is(e.target) && operatorTd.has(e.target).length === 0
                        && !operatorSelector.is(e.target) && operatorSelector.has(e.target).length === 0) {
                        operatorSelector.hide();
                        //operatorExpandButton.removeClass('inputs-list-header-expanded');
                    }
                }
            });
        },

        createVariablePicker: function (variableTd, variable) {
            var that = this;

            var containerImg = $("<img>")./*addClass("container-image")*/attr("src", "../images/state-container.svg").addClass("hidden").appendTo(variableTd);
            var selectedContainer = $("<p></p>").addClass("hidden").appendTo(variableTd);

            var variableImg = $("<img>")./*addClass("variable-image")*/attr("src", "../images/state-variable.svg").appendTo(variableTd);
            var selectedVariable = $("<p></p>").appendTo(variableTd);
            var expandButton = $("<div></div>").addClass('arrow-down').appendTo(variableTd);

            var firstLeft = $(variableTd).offset().left;
            var firstTop = $(variableTd).offset().top;

            var variablePicker = $("<div></div>").addClass("variable-picker").appendTo('body').hide();
            variablePicker.offset({ top: firstTop + 47, left: firstLeft });
            var table = $("<table></table>").appendTo(variablePicker);
            var tbody = $("<tbody></tbody>").appendTo(table);

            var tr = $("<tr></tr>").appendTo(tbody);
            var tdContainer = $("<td></td>").appendTo(tr);
            var imgContainer = $("<div></div>").addClass("container-image")/*attr("src", "../images/container.svg")*/.appendTo(tdContainer);
            var tdVariable = $("<td></td>").appendTo(tr);
            var imgVariable = $("<div></div>").addClass("variable-image")/*attr("src", "../images/variable.svg")*/.appendTo(tdVariable);

            var trList = $("<tr></tr>").appendTo(tbody);

            var setSelectedValue = function (value) {
                var containerName;
                for (var i = 0; i < that.options.variables.length; i++) 
                    if (that.options.variables[i].id == value.container) {
                        containerName = that.options.variables[i].name;
                        break;
                    }

                $(selectedContainer).text(containerName? containerName: "ALL");
                $(selectedVariable).text(value.variable);

                variablePicker.hide();
                //expandButton.removeClass('inputs-list-header-expanded');
                if (containerName !== "ALL") {
                    containerImg.removeClass("hidden");
                    selectedContainer.removeClass("hidden");
                }
            }

            var trDivs = this.updateVariablePicker(trList, setSelectedValue, variable);

            $(document).mousedown(function (e) {
                if (!variablePicker.is(":hidden")) {
                    if (!variableTd.is(e.target) && variableTd.has(e.target).length === 0
                        && !variablePicker.is(e.target) && variablePicker.has(e.target).length === 0) {
                        variablePicker.hide();
                        //expandButton.removeClass('inputs-list-header-expanded');
                    }
                }
            });

            expandButton.bind("click", function () {
                if (variablePicker.is(":hidden")) {
                    var offLeft = $(variableTd).offset().left - firstLeft;
                    var offTop = $(variableTd).offset().top - firstTop;
                    variablePicker.offset({ top: offTop, left: offLeft });
                    firstLeft = $(variableTd).offset().left;
                    firstTop = $(variableTd).offset().top;

                    that.executeonComboBoxOpen();
                    trDivs = that.updateVariablePicker(trList, setSelectedValue, variable);
                    variablePicker.show();
                    //expandButton.addClass('inputs-list-header-expanded');
                } else {
                    variablePicker.hide();
                    //expandButton.removeClass('inputs-list-header-expanded');
                }
            });
        },
        
        updateVariablePicker: function (trList, setSelectedValue, currSymbol) {
            var that = this;
            trList.children().remove();
            var tdContainersList = $("<td></td>").addClass("container list").appendTo(trList);
            var divContainers = $("<div></div>").addClass("scrollable").appendTo(tdContainersList);
            var tdVariablesList = $("<td></td>").addClass("variable list").appendTo(trList);
            var divVariables = $("<div></div>").addClass("scrollable").appendTo(tdVariablesList);

            if (currSymbol.value.container === undefined) currSymbol.value.container = 0;

            for (var i = 0; i < this.options.variables.length; i++) {
                var container = $("<a>" + this.options.variables[i].name + "</a>").attr("data-container-id", this.options.variables[i].id)
                    .appendTo(divContainers).click(function () {
                        that.setActiveContainer(divContainers, divVariables, this, setSelectedValue, currSymbol);
                    });
                if (currSymbol.value != 0 && currSymbol.value.container == this.options.variables[i].id)
                    that.setActiveContainer(divContainers, divVariables, container, setSelectedValue, currSymbol);
            }
            if (currSymbol.value == 0)
                that.setActiveContainer(divContainers, divVariables, divContainers.children().eq(0), setSelectedValue, currSymbol);

            return { containers: divContainers, variables: divVariables, setSelectedValue: setSelectedValue };
        },

        setActiveContainer: function (divContainers, divVariables, container, setSelectedValue, currSymbol) {
            var that = this;

            divContainers.find(".active").removeClass("active");
            divVariables.children().remove();

            var idx = $(container).index();
            $(container).addClass("active");

            for (var j = 0; j < that.options.variables[idx].vars.length; j++) {

                var variableName = that.options.variables[idx].vars[j];
                if (that.options.variables[idx].vars[j] == "")
                    variableName = "Unnamed";

                var variable = $("<a>" + variableName + "</a>").attr("data-variable-name", that.options.variables[idx].vars[j])
                    .appendTo(divVariables).click(function () {
                        divVariables.find(".active").removeClass("active");
                        $(this).addClass("active");
                        
                        currSymbol.value = { container: $(container).attr("data-container-id"), variable: $(this).attr("data-variable-name") };
                        setSelectedValue({ container: currSymbol.value.container, variable: currSymbol.value.variable ? currSymbol.value.variable : "Unnamed" });

                        that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                    });

                if (currSymbol.value != 0 && currSymbol.value.container == $(container).attr("data-container-id")
                    && currSymbol.value.variable == that.options.variables[idx].vars[j]) {
                    variable.addClass("active");
                    setSelectedValue({ container: $(container).attr("data-container-id"), variable: variableName });
                }
            }
        },

        refresh: function () {
            var that = this;
            this._stateButtons.find("[data-state-name='" + this._activeState.name + "']").addClass("active");
            $(this._ltlStates).find("[data-row-type='formula']").remove();

            this._description.val(this._activeState.description);
            for (var i = 0; i < this._activeState.formula.length; i++) {
                this.addFormula(this._activeState.formula[i]);
            }
        },

        executeStatesUpdate: function (args) {
            if (this.options.onStatesUpdated !== undefined) {
                this.options.onStatesUpdated(args);
            }
        },

        executeonComboBoxOpen: function (args) {
            if (this.options.onComboBoxOpen !== undefined) {
                this.options.onComboBoxOpen();
            }
        },

        _setOption: function (key, value) {
            var that = this;
            this._super(key, value);

            switch (key) {
                case "variables": {
                    this.options.variables = [];
                    for (var i = 0; i < value.length; i++) {
                        this.options.variables.push(value[i]);
                    }
                    break;
                }
                case "states": {
                    this.options.states = [];
                    this._stateButtons.children(".state").remove();

                    for (var i = 0; i < value.length; i++) {
                        this.options.states.push(value[i]);
                        if (value[i].formula.length == 0)
                            value[i].formula.push([undefined, undefined, undefined, undefined, undefined]);
                    }
                    
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