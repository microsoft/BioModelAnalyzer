/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _windowTitle: null,
        _stateButtons: null,
        _addStateButton: null,
        _toolbar: null,
        _kfrms: null,
        _description: null,
        _ltlStates: null,
        _ltlAddConditionButton: null,
        _activeState: null,

        _options: {
            variables: [],
            states: []
        },

        _create: function () {
            var that = this;
            this._windowTitle = $("<div>LTL States</div>").addClass("window-title").appendTo(this.element);

            this._stateButtons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            if (this._options.states.length < 1) {
                var newState = {
                    name: "Init",
                    description: "",
                    formula: [
                        [ undefined,
                          { type: "operator", value: "<" },
                          { type: "variable", value: "y" },
                          { type: "operator", value: "<" },
                          undefined ]
                    ]
                };

                this._setOption("states", newState);
            } 

            this._setOption("variables", "xvariabledfsfsdfsdfsdf");
            this._setOption("variables", "y");

            this._activeState = this._options.states[0];

            for (var i = 0; i < this._options.states.length; i++) {
                var stateButton = $("<div>" + this._options.states[i].name + "</div>").addClass("state-button").appendTo(this._stateButtons).click(function () {
                    var stateIndex = Array.prototype.indexOf.call(that._stateButtons[0].children, this);

                    var idx = that._options.states.indexOf(that._activeState);
                    $(that._stateButtons[0].children[idx]).removeClass("active");

                    that._activeState = that._options.states[stateIndex];

                    that.refresh();
                });
            }

            this._addStateButton = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._stateButtons).click(function () {
                that.addState();
            });

            this._toolbar = $("<div></div>").addClass("toolbar").appendTo(this.element);
            this.createToolbar();

            this._ltlStates = $("<div></div>").addClass("LTL-states").appendTo(this.element);
            this._description = $("<input></input>").attr("type", "text").addClass("description").attr("data-row-type", "description")
                .attr("value", "Description").appendTo(this._ltlStates).change(function () {
                var idx = that._options.states.indexOf(that._activeState);
                that._options.states[idx].description = this.value;
                that._activeState.description = this.value;
            });

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
            switch (key) {
                case "variables": {
                    this._options.variables.push(value);
                    break;
                }
                case "states": {
                    this._options.states.push(value);
                    break;
                }
            }
            this._super(key, value);
        },

        _setOptions: function (options) {
            this._super(options);
        },


        createToolbar: function () {
            this._kfrms = window.KeyframesRegistry.Keyframes;

            for (var i = 0; i < this._kfrms.length; i++) {
                var keyframe_elem = $("<img>").attr("src", this._kfrms[i].Icon).attr("name", this._kfrms[i].Name).addClass("tool-buttons").attr("data-tool-type", this._kfrms[i].ToolType).appendTo(this._toolbar);

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
                    } else if (formula[i].type == "variable") {
                        varCount++;
                        if (varCount > 1 || i % 2 == 1 || i == 4)
                            return false;
                    } else if (formula[i].type == "const") {
                        constCount++;
                        if (i % 2 == 1 || constCount > 2)
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
            var idx = this._options.states.indexOf(this._activeState);
            $(this._stateButtons[0].children[idx]).addClass("active");

            var children = $(this._ltlStates).find("[data-row-type='condition']").remove();

            this._description[0].value = this._activeState.description;

            for (var i = 0; i < this._activeState.formula.length; i++) {
                this.addCondition();
                var table = this._ltlStates[0].children[i + 1];
                var tbody = table.children[0];
                var condition = tbody.children[0];
                for (var j = 0; j < 5; j++) {
                    if (this._activeState.formula[i][j] !== undefined) {
                        if (this._activeState.formula[i][j].type == "variable") {
                            var currSymbol = this._activeState.formula[i][j];
                            var img = $("<img>").attr("src", this._kfrms[0].Icon).attr("name", this._kfrms[0].Name).attr("data-tool-type", this._kfrms[0].ToolType).appendTo($(condition.cells[j]));

                            var selectVariable = this.createNewSelect($(condition.cells[j]), currSymbol);

                            if (this._options.variables.indexOf(this._activeState.formula[i][j].value) > -1)
                                $(selectVariable[0].children[0]).text(this._activeState.formula[i][j].value);
                        } else if (this._activeState.formula[i][j].type == "const") {
                            var currNumber = this._activeState.formula[i][j];
                            var num = $("<input autofocus></input>").attr("value", parseFloat(this._activeState.formula[i][j].value))
                                .addClass("number-input").appendTo($(condition.cells[j])).change(function () {
                                currNumber.value = this.value;
                                });
                            //$(num).spinner({
                            //    spin: function (event, ui) {
                            //        if (ui.value > 999) {
                            //            $(this).spinner("value", -999);
                            //            return false;
                            //        } else if (ui.value < -999) {
                            //            $(this).spinner("value", 999);
                            //            return false;
                            //        }
                            //    }
                            //});
                        } else if (this._activeState.formula[i][j].type == "operator") {
                            var img;
                            switch (this._activeState.formula[i][j].value) {
                                case "=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("equal");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).attr("data-tool-type", keyframe.ToolType).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case "<": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("less");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).attr("data-tool-type", keyframe.ToolType).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case "<=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("leeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).attr("data-tool-type", keyframe.ToolType).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case ">": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("more");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).attr("data-tool-type", keyframe.ToolType).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case ">=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("moeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).attr("data-tool-type", keyframe.ToolType).appendTo($(condition.cells[j]));
                                    break;
                                }
                                default: break;
                            } 
                        }
                    }
                    if (!this.validation(this._activeState.formula[i]))
                        $(condition).addClass("error");
                    else $(condition).removeClass("error");
                }
            }
        },

        addState: function () {
            var that = this;
            var k = this._options.states.length;
            var stateName = String.fromCharCode(64 + k);
            var state = $("<div>" + stateName + "</div>").addClass("state-button").click(function () {
                var stateIndex = Array.prototype.indexOf.call(that._stateButtons[0].children, this);

                var idx = that._options.states.indexOf(that._activeState);
                $(that._stateButtons[0].children[idx]).removeClass("active");

                that._activeState = that._options.states[stateIndex];

                that.refresh();
            });
            var newState = {
                name: stateName,
                description: "",
                formula: [[]],
            };
            this._setOption("states", newState);

            var idx = this._options.states.indexOf(this._activeState);
            $(this._stateButtons[0].children[idx]).removeClass("active");

            this._activeState = this._options.states[k];

            this._stateButtons[0].insertBefore(state[0], this._stateButtons[0].lastChild);

            this.refresh();
        },

        addCondition: function () {
            var table = this.createStatesTable();
            this._ltlStates[0].insertBefore(table[0], this._ltlStates[0].lastChild);
        },

        createStatesTable: function () {
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
                        var tableIndex = Array.prototype.indexOf.call(that._ltlStates[0].children, table[0]) - 1;
                        $(this.children).remove();

                        switch (ui.draggable[0].name) {
                            case "var": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);

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
                                var num = $("<input autofocus></input>").addClass("number-input").appendTo(this).change(function () {
                                    currNumber.value = this.value;
                                });

                                //$(num).spinner({
                                //    spin: function (event, ui) {
                                //        if (ui.value > 999) {
                                //            $(this).spinner("value", -999);
                                //            return false;
                                //        } else if (ui.value < -999) {
                                //            $(this).spinner("value", 999);
                                //            return false;
                                //        }
                                //    }
                                //});
                                break;
                            }
                            case "equal": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "="
                                }
                                break;
                            }
                            case "more": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: ">"
                                }
                                break;
                            }
                            case "less": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "<"
                                }
                                break;
                            }
                            case "moeq": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: ">="
                                }
                                break;
                            }
                            case "leeq": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("data-tool-type", ui.draggable[0].getAttribute("data-tool-type")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "<="
                                }
                                break;
                            }
                            default: break;
                        }
                        if (!that.validation(that._options.states[stateIndex].formula[tableIndex]))
                            $(this.parentElement).addClass("error");
                        else $(this.parentElement).removeClass("error");
                    }
                });
            }
            var td = $("<td></td>").addClass("LTL-line-del").appendTo(tr).click(function () {
                $(table).remove();
            });
            td[0].appendChild(($("<img>").attr("src", "../images/ltlimgs/remove.png"))[0]);
            return table;
        }
    });
})(jQuery);