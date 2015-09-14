/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _windowTitle: null,
        _stateButtons: null,
        _addStateButton: null,
        _toolbar: null,
        _keyframes: null,
        _description: null,
        _ltlStates: null,
        _ltlAddConditionButton: null,
        _activeState: null,

        _options: {
            variables: [],
            states: [],
            minConst: -99,
            maxConst: 100,
        },

        _create: function () {
            var that = this;
            this.element.addClass("window").addClass("LTL-states");
            this._windowTitle = $("<div>LTL States</div>").addClass("window-title").appendTo(this.element);

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            if (this._options.states.length < 1) {
                var newState = {
                    name: "A",
                    description: "",
                    formula: [
                        [undefined,
                            undefined,
                            undefined,
                            undefined,
                            undefined]
                    ]
                };
                this._options.states.push(newState);
            }

            this._options.variables.push("xvariabledfsfsdfsdfsdf");
            this._options.variables.push("y");

            this._activeState = this._options.states[0];

            for (var i = 0; i < this._options.states.length; i++) {
                var stateButton = $("<div>" + this._options.states[i].name + "</div>").attr("data-state-name", this._options.states[i].name)
                    .addClass("state-button").appendTo(this._stateButtons).click(function () {
                    that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                    for (var j = 0; j < that._options.states.length; j++) {
                        if (that._options.states[j].name == $(this).attr("data-state-name")) {
                            that._activeState = that._options.states[j];
                            break;
                        }
                    }
                    that.refresh();
                });
            }

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons).click(function () {
                that.addState();
            });

            this._toolbar = $("<div></div>").addClass("state-toolbar").appendTo(this.element);
            this.createToolbar();

            this._description = $("<input></input>").attr("type", "text").addClass("state-description").attr("size", "15").attr("data-row-type", "description")
                .attr("placeholder", "Description").appendTo(this.element).change(function () {
                var idx = that._options.states.indexOf(that._activeState);
                that._options.states[idx].description = this.value;
                that._activeState.description = this.value;
            });
            this._ltlStates = $("<div></div>").addClass("LTL-states").appendTo(this.element);

            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "add").appendTo(this._ltlStates);
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            this._ltlAddConditionButton = $("<td>+</td>").addClass("LTL-line-new").appendTo(tr).click(function () {
                var idx = that._options.states.indexOf(that._activeState);
                var emptyFormula = [undefined, undefined, undefined, undefined, undefined];
                that._options.states[idx].formula.push(emptyFormula);
                that.addCondition();
            });

            this.refresh();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "variables": {
                    this._options.variables.push(value);
                    break;
                }
                case "states": {
                    try {
                        if (this.validation(value.formula))
                            this._options.states.push(value);
                        else throw "The state " + value.name + " has wrong formula";
                    }
                    catch (ex) { };
                    break;
                }
                case "minConst": {
                    this._options.minConst = value;
                    break;
                }
                case "maxConst": {
                    this._options.maxConst = value;
                    break;
                }
                default: break;
            }
            this._super(key, value);
        },

        _setOptions: function (options) {
            this._super(options);
        },


        createToolbar: function () {
            this._keyframes = window.KeyframesRegistry.Keyframes;

            for (var i = 0; i < this._keyframes.length; i++) {
                var keyframe_elem = $("<img>").attr("src", this._keyframes[i].Icon).attr("name", this._keyframes[i].Name).addClass("state-tool")
                    .attr("data-tool-type", this._keyframes[i].ToolType).appendTo(this._toolbar);

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
            var selectVariable = $("<div></div>").addClass("variable-select").appendTo(td);
            var variableSelected = $("<p></p>").appendTo(selectVariable);
            var expandButton = $("<div></div>").addClass('inputs-expandbttn').appendTo(selectVariable);

            var listOfVariables = $("<div></div>").addClass("variables-list").appendTo(td).hide();

            selectVariable.bind("click", function () {
                if (listOfVariables.is(":hidden")) {
                    listOfVariables.show();
                    expandButton.addClass('inputs-list-header-expanded');
                    selectVariable.addClass("expanded");
                    listOfVariables.addClass("expanded");
                } else {
                    listOfVariables.hide();
                    expandButton.removeClass('inputs-list-header-expanded');
                    selectVariable.removeClass("expanded");
                    listOfVariables.removeClass("expanded");
                }
            });

            for (var k = 0; k < this._options.variables.length; k++)
                var variable = $("<div>" + this._options.variables[k] + "</div>").appendTo(listOfVariables).click(function () {
                    variableSelected.text(this.innerText);
                    currSymbol.value = this.innerText;

                    selectVariable.trigger("click");
                });

            return selectVariable;
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

                            var selectVariable = this.createNewSelect(condition.children().eq(j), currSymbol);

                            if (this._options.variables.indexOf(this._activeState.formula[i][j].value) > -1)
                                selectVariable.children().eq(0).text(this._activeState.formula[i][j].value);

                        } else if (this._activeState.formula[i][j].type == "const") {

                            var currNumber = this._activeState.formula[i][j];
                            var num = $("<input autofocus></input>").attr("type", "text").attr("min", "0").attr("max", "100")
                                .attr("value", parseFloat(this._activeState.formula[i][j].value)).attr("size", "1")
                                .addClass("number-input").appendTo(condition.children().eq(j));

                            num.bind("input change", function () {
                                if (parseFloat(this.value) > that._options.maxConst) this.value = that._options.maxConst;
                                if (parseFloat(this.value) < that._options.minConst) this.value = that._options.minConst;
                                currNumber.value = this.value;
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

        addState: function () {
            var that = this;
            var k = this._options.states.length;
            var stateName = String.fromCharCode(63 + k);
            var state = $("<div>" + stateName + "</div>").attr("data-state-name", stateName).addClass("state-button").click(function () {
                that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
                for (var j = 0; j < that._options.states.length; j++) {
                    if (that._options.states[j].name == $(this).attr("data-state-name")) {
                        that._activeState = that._options.states[j];
                        break;
                    }
                }
                that.refresh();
            });

            var newState = {
                name: stateName,
                description: "",
                formula: [[]],
            };
            this._setOption("states", newState);

            that._stateButtons.find("[data-state-name='" + that._activeState.name + "']").removeClass("active");
            this._activeState = this._options.states[k];
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
                        var stateIndex = that._options.states.indexOf(that._activeState);
                        var cellIndex = this.cellIndex;
                        var tableIndex = table.index();

                        var formula = that._options.states[stateIndex].formula[tableIndex].slice(0);

                        formula[this.cellIndex] = {
                            type: ui.draggable.attr("data-tool-type"),
                            value: 0
                        };

                        if (that.validation(formula)) {

                            $(this.children).remove();

                            switch (ui.draggable[0].name) {
                                case "var": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);

                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "variable",
                                        value: 0
                                    };

                                    var selectVariable = that.createNewSelect(this, that._options.states[stateIndex].formula[tableIndex][cellIndex]);

                                    break;
                                }
                                case "num": {
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "const",
                                        value: 0
                                    }

                                    var currNumber = that._options.states[stateIndex].formula[tableIndex][this.cellIndex];
                                    var num = $("<input autofocus></input>").attr("type", "text").attr("value", "0").attr("min", "0")
                                        .attr("max", "100").addClass("number-input").appendTo(this);

                                    num.bind("input change", function () {
                                        if (parseFloat(this.value) > that._options.maxConst) this.value = that._options.maxConst;
                                        if (parseFloat(this.value) < that._options.minConst) this.value = that._options.minConst;
                                        currNumber.value = this.value;
                                    });
                                    break;
                                }
                                case "equal": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "="
                                    }
                                    break;
                                }
                                case "more": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: ">"
                                    }
                                    break;
                                }
                                case "less": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "<"
                                    }
                                    break;
                                }
                                case "moeq": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: ">="
                                    }
                                    break;
                                }
                                case "leeq": {
                                    var img = $("<img>").attr("src", ui.draggable.attr("src")).attr("data-tool-type", ui.draggable.attr("data-tool-type")).appendTo(this);
                                    that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                        type: "operator",
                                        value: "<="
                                    }
                                    break;
                                }
                                default: break;
                            }
                        }
                    }
                });
            }

            var td = $("<td></td>").addClass("LTL-line-del").appendTo(tr).click(function () {
                $(table).remove();
            });
            var delTable = $("<img>").attr("src", "../images/ltlimgs/remove.png").appendTo(td);

            table.insertBefore(this._ltlStates.children().last());
        },
    });
}(jQuery));

interface JQuery {
    stateseditor(): JQuery;
    stateseditor(settings: Object): JQuery;
    stateseditor(optionLiteral: string, optionName: string): any;
    stateseditor(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    stateseditor(methodName: string, methodValue: any): JQuery;
} 