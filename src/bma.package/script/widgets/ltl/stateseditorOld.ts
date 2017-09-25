// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor1", {
        _stateButtons: null,
        _addStateButton: null,
        _toolbar: null,
        _keyframes: null,
        _description: null,
        _ltlStates: null,
        _ltlAddConditionButton: null,
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

        _create: function () {
            var that = this;

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

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
            //that._initStates();
            //that.options.states;

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons).click(function () {
                that.addState();
                that.executeStatesUpdate({ states: that.options.states, changeType: "stateAdded" });
            });

            this._toolbar = $("<div></div>").addClass("state-toolbar").appendTo(this.element);
            this.createToolbar();

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

            this._ltlAddConditionButton = $("<td>+</td>").addClass("LTL-line-new").appendTo(tr).click(function () {
                var idx = that.options.states.indexOf(that._activeState);
                var emptyFormula = [undefined, undefined, undefined, undefined, undefined];
                that.options.states[idx].formula.push(emptyFormula);
                that.addCondition();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });

        },

        _setOption: function (key, value) {
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
                case "minConst": {
                    this.options.minConst = value;
                    break;
                }
                case "maxConst": {
                    this.options.maxConst = value;
                    break;
                }
                case "onStatesUpdated": {
                    this.options.onStatesUpdated = value;
                    break;
                }
                case "onComboBoxOpen": {
                    this.options.onComboBoxOpen = value;
                    break;
                }
                default: break;
            }
        },

        _setOptions: function (options) {
            this._super(options);
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

        createToolbar: function () {
            this._keyframes = window.KeyframesRegistry.Keyframes;

            for (var i = 0; i < this._keyframes.length; i++) {
                var stateTool = $("<div></div>").addClass("state-tool").appendTo(this._toolbar);
                var keyframe_elem = $("<img>").attr("src", this._keyframes[i].Icon).attr("name", this._keyframes[i].Name).addClass("state-tool")
                    .attr("data-tool-type", this._keyframes[i].ToolType).appendTo(stateTool);

                keyframe_elem.draggable({
                    helper: "clone",
                });
            }
        },

        validation: function (formula) {
            var varCount = 0;
            var constCount = 0;
            for (var i = 0; i < 5; i++) {
                if (formula[i] !== undefined) {
                    if (formula[i].type == "operator") {
                        if (i % 2 == 0)
                            return false;
                        if (i == 3 && formula[0] !== undefined && formula[0].type == "variable")
                            return false;
                        if (i == 3 && formula[2] !== undefined && formula[2].type == "const")
                            return false;
                    } else if (formula[i].type == "variable") {
                        varCount++;
                        if (varCount > 1 || i % 2 == 1 || i == 4)
                            return false;
                    } else if (formula[i].type == "const") {
                        constCount++;
                        if (i % 2 == 1 || constCount > 2)
                            return false;
                        if (i == 0 && formula[2] !== undefined && formula[2].type != "variable")
                            return false;
                        if (i == 2 && ((formula[0] !== undefined && formula[0].type != "variable") ||
                            formula[4] !== undefined))
                            return false;
                        if (i == 4 && formula[0] !== undefined && formula[0].type == "variable")
                            return false;
                    }
                }
            }
            return true;
        },

        createNewSelect: function (td, currSymbol) {
            var that = this;
            var firstLeft = $(td).offset().left;
            var firstTop = $(td).offset().top;

            var selectVariable = $("<div></div>").addClass("variable-select").appendTo(td);
            var variableSelected = $("<p></p>").appendTo(selectVariable);
            var expandButton = $("<div></div>").addClass('inputs-expandbttn').appendTo(selectVariable);

            var variablePicker = $("<div></div>").addClass("variable-picker").appendTo('body').hide();
            variablePicker.offset({ top: firstTop + 57, left: firstLeft });
            var table = $("<table></table>").appendTo(variablePicker);
            var tbody = $("<tbody></tbody>").appendTo(table);

            var tr = $("<tr></tr>").appendTo(tbody);
            var tdContainer = $("<td></td>").appendTo(tr);
            var imgContainer = $("<img></img>").attr("src", "../images/container.svg").appendTo(tdContainer);
            var tdVariable = $("<td></td>").appendTo(tr);
            var imgVariable = $("<img></img>").attr("src", "../images/variable.svg").appendTo(tdVariable);

            var trList = $("<tr></tr>").appendTo(tbody);

            var setSelectedValue = function (value) {
                variableSelected.text(value);
                variablePicker.hide();
                expandButton.removeClass('inputs-list-header-expanded');
                selectVariable.removeClass("expanded");
            };

            var trDivs = this.updateVariablePicker(trList, setSelectedValue, currSymbol);

            $(document).mousedown(function (e) {
                if (!variablePicker.is(":hidden")) {
                    if (!selectVariable.is(e.target) && selectVariable.has(e.target).length === 0
                        && !variablePicker.is(e.target) && variablePicker.has(e.target).length === 0) {
                        variablePicker.hide();
                        expandButton.removeClass('inputs-list-header-expanded');
                        selectVariable.removeClass("expanded");
                    }
                }
            });
            
            selectVariable.bind("click", function () {
                if (variablePicker.is(":hidden")) {
                    var offLeft = $(td).offset().left - firstLeft;
                    var offTop = $(td).offset().top - firstTop;
                    variablePicker.offset({ top: offTop, left: offLeft });
                    firstLeft = $(td).offset().left;
                    firstTop = $(td).offset().top;

                    that.executeonComboBoxOpen();
                    trDivs = that.updateVariablePicker(trList, setSelectedValue, currSymbol);
                    variablePicker.show();
                    expandButton.addClass('inputs-list-header-expanded');
                    selectVariable.addClass("expanded");
                } else {
                    variablePicker.hide();
                    expandButton.removeClass('inputs-list-header-expanded');
                    selectVariable.removeClass("expanded");
                }
            });

            return trDivs;
        },

        updateVariablePicker: function (trList, setSelectedValue, currSymbol) {
            var that = this;
            trList.children().remove();
            var tdContainersList = $("<td></td>").addClass("list").appendTo(trList);
            var divContainers = $("<div></div>").addClass("scrollable").appendTo(tdContainersList);
            var tdVariablesList = $("<td></td>").addClass("list").appendTo(trList);
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

                var variables = $("<a>" + variableName + "</a>").attr("data-variable-name", that.options.variables[idx].vars[j])
                    .appendTo(divVariables).click(function () {
                        divVariables.find(".active").removeClass("active");
                        $(this).addClass("active");

                        setSelectedValue(($(this).attr("data-variable-name") == "") ? "Unnamed" : $(this).attr("data-variable-name"));
                        currSymbol.value = { container: $(container).attr("data-container-id"), variable: $(this).attr("data-variable-name") };

                        that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                    });

                if (currSymbol.value != 0 && currSymbol.value.container == $(container).attr("data-container-id")
                    && currSymbol.value.variable == that.options.variables[idx].vars[j])
                    variables.addClass("active");
            }
        },

        refresh: function () {
            var that = this;
            this._stateButtons.find("[data-state-name='" + this._activeState.name + "']").addClass("active");
            $(this._ltlStates).find("[data-row-type='condition']").remove();

            this._description.val(this._activeState.description);

            for (var i = 0; i < this._activeState.formula.length; i++) {
                this.addCondition();
                var table = this._ltlStates.children().eq(i);
                var tbody = table.children().eq(0);
                var condition = tbody.children().eq(0);

                for (var j = 0; j < 5; j++) {
                    if (this._activeState.formula[i][j] !== undefined) {

                        if (this._activeState.formula[i][j].type == "variable") {

                            var currSymbol = this._activeState.formula[i][j];
                            var img = $("<img>").attr("src", this._keyframes[0].Icon).attr("name", this._keyframes[0].Name).css("width", "30px")
                                .css("height", "30px").attr("data-tool-type", this._keyframes[0].ToolType).appendTo(condition.children().eq(j));

                            var trList = this.createNewSelect(condition.children().eq(j), currSymbol);
                            var divContainers = trList.containers;
                            var divVariables = trList.variables;

                            var cntName = this._activeState.formula[i][j].value.container === undefined ? 0 : this._activeState.formula[i][j].value.container;
                            var container = divContainers.find("[data-container-id='" + cntName + "']");
                            that.setActiveContainer(divContainers, divVariables, container, trList.setSelectedValue, currSymbol);
                            trList.setSelectedValue(currSymbol.value.variable);

                        } else if (this._activeState.formula[i][j].type == "const") {

                            var currNumber = this._activeState.formula[i][j];
                            var num = $("<input></input>").attr("type", "text").attr("min", "0").attr("max", "100")
                                .attr("value", parseFloat(this._activeState.formula[i][j].value)).attr("size", "1")
                                .addClass("number-input").appendTo(condition.children().eq(j));

                            num.bind("input change", function () {
                                if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                currNumber.value = this.value;

                                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                            });

                            num.trigger("focus");

                        } else if (this._activeState.formula[i][j].type == "operator") {

                            var keyframe;
                            switch (this._activeState.formula[i][j].value) {
                                case "=": {
                                    keyframe = window.KeyframesRegistry.GetFunctionByName("equal");;
                                    break;
                                }
                                case "<": {
                                    keyframe = window.KeyframesRegistry.GetFunctionByName("less");
                                    break;
                                }
                                case "<=": {
                                    keyframe = window.KeyframesRegistry.GetFunctionByName("leeq");
                                    break;
                                }
                                case ">": {
                                    keyframe = window.KeyframesRegistry.GetFunctionByName("more");
                                    break;
                                }
                                case ">=": {
                                    keyframe = window.KeyframesRegistry.GetFunctionByName("moeq");
                                    break;
                                }
                                default: break;
                            }
                            var img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                        }
                    }
                }
            }
        },

        addState: function (state = null) {
            var that = this;
            var stateName;
            var idx;
            if (state == null) {
                var k = this.options.states.length;

                var lastStateName = "";
                for (var i = 0; i < k; i++) {
                    var lastStateIdx = (lastStateName && lastStateName.length > 1)  ? parseFloat(lastStateName.slice(1)) : 0;
                    var stateIdx = this.options.states[i].name.length > 1 ? parseFloat(this.options.states[i].name.slice(1)) : 0;

                    if (stateIdx >= lastStateIdx) {
                        lastStateName = (lastStateName && stateIdx == lastStateIdx
                                        && lastStateName.charAt(0) > this.options.states[i].name.charAt(0)) ?
                                        lastStateName : this.options.states[i].name;
                    }
                }

                var charCode =  lastStateName ? lastStateName.charCodeAt(0) : 65;                
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
                idx = k;
                if (k == 0)
                    this._activeState = this.options.states[k];
            } else {
                stateName = state.name;
                idx = this.options.states.indexOf(state);

            }

            var state = <any>($("<div>" + stateName + "</div>").attr("data-state-name", stateName).addClass("state-button").addClass("state").click(function () {
                that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                for (var j = 0; j < that.options.states.length; j++) {
                    if (that.options.states[j].name == $(this).attr("data-state-name")) {
                        that._activeState = that.options.states[j];
                        break;
                    }
                }
                that.refresh();
            }));

            if(this._activeState != null)
                that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
            this._activeState = this.options.states[idx]; 
            state.insertBefore(this._stateButtons.children().last());

            this.refresh();
        },

        addCondition: function () {
            var that = this;
            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "condition");
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            for (var i = 0; i < 5; i++) {
                var td = $("<td></td>").appendTo(tr);
                td.droppable({
                    drop: function (event, ui) {
                        var stateIndex = that.options.states.indexOf(that._activeState);
                        var cellIndex = this.cellIndex;
                        var tableIndex = table.index();

                        var formula = that.options.states[stateIndex].formula[tableIndex].slice(0);

                        formula[this.cellIndex] = {
                            type: ui.draggable.attr("data-tool-type"),
                            value: 0
                        };

                        if (that.validation(formula)) {

                            $(this.children).remove();

                            var n = (<any>ui.draggable[0]).name;
                            switch (n) {
                                case "var": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type"))
                                        .css("width", "30px").css("height", "30px").appendTo(this);

                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "variable",
                                        value: 0
                                    };

                                    var variablePicker = that.createNewSelect(this, that.options.states[stateIndex].formula[tableIndex][cellIndex]);
                                    break;
                                }
                                case "num": {
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "const",
                                        value: 0
                                    }

                                    var currNumber = that.options.states[stateIndex].formula[tableIndex][this.cellIndex];
                                    var num = $("<input></input>").attr("type", "text").attr("value", "0").attr("min", "0")
                                        .attr("max", "100").addClass("number-input").appendTo(this);

                                    num.bind("input change", function () {
                                        if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                        if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                        currNumber.value = this.value;

                                        that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                                    });

                                    num.trigger("focus");
                                    break;
                                }
                                case "equal": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "="
                                    }
                                    break;
                                }
                                case "more": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: ">"
                                    }
                                    break;
                                }
                                case "less": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "<"
                                    }
                                    break;
                                }
                                case "moeq": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: ">="
                                    }
                                    break;
                                }
                                case "leeq": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "<="
                                    }
                                    break;
                                }
                                default: break;
                            }

                            that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                        }
                    }
                }).dblclick(function () {
                    if (this.childElementCount == 0) {
                        var stateIndex = that.options.states.indexOf(that._activeState);
                        var cellIndex = this.cellIndex;
                        var tableIndex = table.index();

                        var formula = that.options.states[stateIndex].formula[tableIndex].slice(0);

                        formula[this.cellIndex] = {
                            type: "const",
                            value: 0
                        };

                        if (that.validation(formula)) {
                            that.options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                type: "const",
                                value: 0
                            }

                            var currNumber = that.options.states[stateIndex].formula[tableIndex][this.cellIndex];
                            var num = $("<input></input>").attr("type", "text").attr("value", "0").attr("min", "0")
                                .attr("max", "100").addClass("number-input").appendTo(this);

                            num.bind("input change", function () {
                                if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                currNumber.value = this.value;

                                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                            });

                            num.trigger("change");
                            num.trigger("focus");
                        }
                    }
                });
            }

            var td = $("<td></td>").addClass("LTL-line-del").appendTo(tr).click(function () {
                var stateIndex = that.options.states.indexOf(that._activeState);
                var tableIndex = table.index();
                that.options.states[stateIndex].formula.splice(tableIndex, 1);
                $(table).remove();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
            });
            var delTable = $("<img>").attr("src", "../images/state-line-del.svg").appendTo(td);

            table.insertBefore(this._ltlStates.children().last());
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
