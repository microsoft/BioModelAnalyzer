/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _emptyStateAddButton: null,
        _emptyStatePlaceholder: null,
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
            onStatesUpdated: undefined,
            onComboBoxOpen: undefined,
        },

        _create: function () {
            var that = this;

            this._emptyStateAddButton = $("<div>+</div>").addClass("state-button-empty").addClass("new").appendTo(this.element).hide().click(function () {
                that._emptyStateAddButton.hide();
                that._emptyStatePlaceholder.hide();
                that._stateButtons.show();
                that._toolbar.show();
                that._description.show();
                that._ltlStates.show();

                that.addState();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateAdded" });
            });

            this._emptyStatePlaceholder = $("<div>start by defining some model states</div>").addClass("state-placeholder").appendTo(this.element).hide();

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            for (var i = 0; i < this.options.states.length; i++) {
                var stateButton = $("<div>" + this.options.states[i].name + "</div>").attr("data-state-name", this.options.states[i].name)
                    .addClass("state-button").addClass("state").appendTo(this._stateButtons).click(function () {
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

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons).click(function () {
                that.addState();

                that.executeStatesUpdate({ states: that.options.states, changeType: "stateAdded" });
            });

            this._toolbar = $("<div></div>").addClass("state-toolbar").appendTo(this.element);
            this.createToolbar();

            this._description = $("<input></input>").attr("type", "text").addClass("state-description").attr("size", "15").attr("data-row-type", "description")
                .attr("placeholder", "Description").appendTo(this.element).change(function () {
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

            if (this.options.states.length == 0) {
                this._emptyStateAddButton.show();
                this._emptyStatePlaceholder.show();
                this._stateButtons.hide();
                this._toolbar.hide();
                this._description.hide();
                this._ltlStates.hide();
            } else {
                this._activeState = this.options.states[0];
                this.refresh();
            }
        },

        _setOption: function (key, value) {
            var that = this;
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
                        this.addState(value[i]);
                    }
                    if (this.options.states.length != 0) {
                        that._emptyStateAddButton.hide();
                        that._emptyStatePlaceholder.hide();
                        that._stateButtons.show();
                        that._toolbar.show();
                        that._description.show();
                        that._ltlStates.show();
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
            this._super(key, value);
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

            var selectVariable = $("<div></div>").addClass("variable-select").appendTo(td);
            var variableSelected = $("<p></p>").appendTo(selectVariable);
            var expandButton = $("<div></div>").addClass('inputs-expandbttn').appendTo(selectVariable);

            var variablePicker = $("<div></div>").addClass("variable-picker").appendTo(td).hide();
            var table = $("<table></table>").appendTo(variablePicker);
            var tbody = $("<tbody></tbody>").appendTo(table);

            var tr = $("<tr></tr>").appendTo(tbody);
            var tdContainer = $("<td></td>").appendTo(tr);
            var imgContainer = $("<img></img>").attr("src", "../images/container.svg").appendTo(tdContainer);
            var tdVariable = $("<td></td>").appendTo(tr);
            var imgVariable = $("<img></img>").attr("src", "../images/variable.svg").appendTo(tdVariable);

            var trList = $("<tr></tr>").appendTo(tbody);

            this.updateVariablePicker(trList, variablePicker, variableSelected, selectVariable, currSymbol);
            
            selectVariable.bind("click", function () {
                if (variablePicker.is(":hidden")) {
                    that.executeonComboBoxOpen();
                    that.updateVariablePicker(trList, variablePicker, variableSelected, selectVariable, currSymbol);
                    variablePicker.show();
                    expandButton.addClass('inputs-list-header-expanded');
                    selectVariable.addClass("expanded");
                    variablePicker.addClass("expanded");
                } else {
                    variablePicker.hide();
                    expandButton.removeClass('inputs-list-header-expanded');
                    selectVariable.removeClass("expanded");
                    variablePicker.removeClass("expanded");
                }
            });

            return trList;
        },

        updateVariablePicker: function (trList, variablePicker, variableSelected, selectVariable, currSymbol) {
            var that = this;
            trList.children().remove();
            var tdContainersList = $("<td></td>").addClass("list").appendTo(trList);
            var divContainers = $("<div></div>").addClass("scrollable").appendTo(tdContainersList);
            var tdVariablesList = $("<td></td>").addClass("list").appendTo(trList);
            var divVariables = $("<div></div>").addClass("scrollable").appendTo(tdVariablesList);

            for (var i = 0; i < this.options.variables.length; i++) {
                var containers = $("<a>" + this.options.variables[i].name + "</a>").attr("data-container-id", this.options.variables[i].id)
                    .appendTo(divContainers).click(function () {
                    var currConteiner = this;
                    divContainers.find(".active").removeClass("active");
                    divVariables.children().remove();
                    var idx = $(this).index();
                    $(this).addClass("active");
                    for (var j = 0; j < that.options.variables[idx].vars.length; j++) {
                        var variableName = that.options.variables[idx].vars[j];
                        if (that.options.variables[idx].vars[j] == "")
                            variableName = "Unnamed";
                        var variables = $("<a>" + variableName + "</a>").attr("data-variable-name", that.options.variables[idx].vars[j])
                            .appendTo(divVariables).click(function () {
                            divVariables.find(".active").removeClass("active");
                            $(this).addClass("active");

                            if ($(this).attr("data-variable-name") == "")
                                variableSelected.text("Unnamed");
                            else
                                variableSelected.text($(this).attr("data-variable-name"));

                            currSymbol.value = { container: $(currConteiner).attr("data-container-id"), variable: $(this).attr("data-variable-name") };

                            if (!variablePicker.is(":hidden"))
                                selectVariable.trigger("click");

                            that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                        });

                        if (currSymbol.value != 0 && currSymbol.value.container == $(currConteiner).attr("data-container-id")
                            && currSymbol.value.variable == that.options.variables[idx].vars[j])
                            variables.addClass("active");
                    }
                });
                if (currSymbol.value != 0 && currSymbol.value.container == this.options.variables[i].id)
                    containers.trigger("click");
            }
            if (currSymbol.value == 0)
                divContainers.children().eq(0).trigger("click");
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
                            var img = $("<img>").attr("src", this._keyframes[0].Icon).attr("name", this._keyframes[0].Name)
                                .attr("data-tool-type", this._keyframes[0].ToolType).appendTo(condition.children().eq(j));

                            var trList = this.createNewSelect(condition.children().eq(j), currSymbol);
                            var td = condition.children().eq(j);
                            var tdContainers = trList.children().eq(0);
                            var divContainers = tdContainers.children().eq(0);
                            var tdVariables = trList.children().eq(1);
                            var divVariables = tdVariables.children().eq(0);

                            divContainers.find("[data-container-id='" + this._activeState.formula[i][j].value.container + "']").trigger("click");
                            divVariables.find("[data-variable-name='" + this._activeState.formula[i][j].value.variable + "']").trigger("click");

                        } else if (this._activeState.formula[i][j].type == "const") {

                            var currNumber = this._activeState.formula[i][j];
                            var num = $("<input autofocus></input>").attr("type", "text").attr("min", "0").attr("max", "100")
                                .attr("value", parseFloat(this._activeState.formula[i][j].value)).attr("size", "1")
                                .addClass("number-input").appendTo(condition.children().eq(j));

                            num.bind("input change", function () {
                                if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                currNumber.value = this.value;

                                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                            });

                        } else if (this._activeState.formula[i][j].type == "operator") {

                            var img;
                            switch (this._activeState.formula[i][j].value) {
                                case "=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("equal");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                        .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                                    break;
                                }
                                case "<": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("less");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                        .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                                    break;
                                }
                                case "<=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("leeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                        .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                                    break;
                                }
                                case ">": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("more");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                        .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                                    break;
                                }
                                case ">=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("moeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name)
                                        .attr("data-tool-type", keyframe.ToolType).appendTo(condition.children().eq(j));
                                    break;
                                }
                                default: break;
                            }
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
                stateName = String.fromCharCode(65 + k);

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

                            switch (ui.draggable[0].name) {
                                case "var": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);

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
                                    var num = $("<input autofocus></input>").attr("type", "text").attr("value", "0").attr("min", "0")
                                        .attr("max", "100").addClass("number-input").appendTo(this);

                                    num.bind("input change", function () {
                                        if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                        if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                        currNumber.value = this.value;

                                        that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                                    });
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
                            var num = $("<input autofocus></input>").attr("type", "text").attr("value", "0").attr("min", "0")
                                .attr("max", "100").addClass("number-input").appendTo(this);

                            num.bind("input change", function () {
                                if (parseFloat(this.value) > that.options.maxConst) this.value = that.options.maxConst;
                                if (parseFloat(this.value) < that.options.minConst) this.value = that.options.minConst;
                                currNumber.value = this.value;

                                that.executeStatesUpdate({ states: that.options.states, changeType: "stateModified" });
                            });
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