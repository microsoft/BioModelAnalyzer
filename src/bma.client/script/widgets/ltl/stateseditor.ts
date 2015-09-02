/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.stateseditor", {
        _window_title: null,
        _state_buttons: null,
        _add_state_button: null,
        _toolbar: null,
        _kfrms: null,
        _description: null,
        _ltl_states: null,
        _ltl_add_condition_button: null,
        _activeState: null,

        _options: {
            variables: [],
            states: []
        },

        _create: function () {
            var that = this;
            this._window_title = $("<div>LTL States</div>").addClass("window-title").appendTo(this.element);

            this._state_buttons = $("<div></div>").addClass("state-buttons").appendTo(this.element);

            if (this._options.states.length < 1) {
                var newState = {
                    name: "Init",
                    description: "",
                    formula: [
                        [ undefined,
                          { type: "operator", value: "<" },
                          { type: "variable", value: "var1" },
                          { type: "operator", value: "<" },
                          undefined ]
                    ]
                };

                this._setOption("states", newState);
            } 

            this._activeState = this._options.states[0];

            for (var i = 0; i < this._options.states.length; i++) {
                var state_button = $("<div>" + this._options.states[i].name + "</div>").addClass("state-button").appendTo(this._state_buttons).click(function () {
                    var stateIndex = Array.prototype.indexOf.call(that._state_buttons[0].children, this);

                    var idx = that._options.states.indexOf(that._activeState);
                    $(that._state_buttons[0].children[idx]).removeClass("active");

                    that._activeState = that._options.states[stateIndex];

                    that.refresh();
                });
            }

            this._add_state_button = $("<div>+</div>").addClass("state-button").addClass("new").appendTo(this._state_buttons).click(function () {
                that.addState();
            });

            this._toolbar = $("<div></div>").addClass("toolbar").appendTo(this.element);
            this._create_toolbar();

            this._ltl_states = $("<div></div>").addClass("LTL-states").appendTo(this.element);
            this._description = $("<input></input>").attr("type", "text").addClass("description").attr("data-row-type", "description").attr("value", "Description").appendTo(this._ltl_states);

            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "add").appendTo(this._ltl_states);
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            this._ltl_add_condition_button = $("<td>+</td>").addClass("LTL-line-new").appendTo(tr).click(function () {
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

        refresh: function () {
            var idx = this._options.states.indexOf(this._activeState);
            $(this._state_buttons[0].children[idx]).addClass("active");

            var children = $(this._ltl_states).find("[data-row-type='condition']").remove();

            this._description.value = this._activeState.description;

            for (var i = 0; i < this._activeState.formula.length; i++) {
                this.addCondition();
                var condition = this._ltl_states[0].children[i + 1];
                for (var j = 0; j < 5; j++) {
                    if (this._activeState.formula[i][j] !== undefined) {
                        if (this._activeState.formula[i][j].type == "variable") {
                            var img = $("<img>").attr("src", this._kfrms[0].Icon).attr("name", this._kfrms[0].Name).addClass("variable").appendTo($(condition.cells[j]));
                            if (j == 0) {
                                $(condition.cells[2]).droppable("option", "accept", ".const");

                                for (var k = 3; k < 5; k++)
                                    $(condition.cells[k]).droppable("option", "accept", false);

                            } else if (j == 2) {
                                $(condition.cells[0]).droppable("option", "accept", ".const");
                                $(condition.cells[4]).droppable("option", "accept", ".const");
                            }
                        }
                        if (this._activeState.formula[i][j].type == "operator") {
                            var img;
                            switch (this._activeState.formula[i][j].value) {
                                case "=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("equal");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case "<": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("less");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case "<=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("leeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case ">": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("more");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).appendTo($(condition.cells[j]));
                                    break;
                                }
                                case ">=": {
                                    var keyframe = window.KeyframesRegistry.GetFunctionByName("moeq");
                                    img = $("<img>").attr("src", keyframe.Icon).attr("name", keyframe.Name).appendTo($(condition.cells[j]));
                                    break;
                                }
                                default: break;
                            } 
                            img.addClass("operators");
                        }
                        if (this._activeState.formula[i][j].type == "const") {
                            var img = $("<img>").attr("src", this._kfrms[1].Icon).attr("name", this._kfrms[1].Name).addClass("const").appendTo($(condition.cells[j]));
                            if (j == 2) {
                                $(condition.cells[0]).droppable("option", "accept", ".variable");

                                for (var k = 3; k < 5; k++)
                                    $(condition.cells[k]).droppable("option", "accept", false);

                            } else if (this.cellIndex == 0 || this.cellIndex == 4) {
                                $(condition.cells[0]).droppable("option", "accept", ".const");
                                $(condition.cells[4]).droppable("option", "accept", ".const");
                                $(condition.cells[2]).droppable("option", "accept", ".variable");
                            }
                        }

                        if (j % 2 == 1) 
                            $(condition.cells[j]).droppable("option", "accept", ".operations");
                    }
                }
            }
        },

        _create_toolbar: function () {
            this._kfrms = window.KeyframesRegistry.Keyframes;

            for (var i = 0; i < this._kfrms.length; i++) {
                var keyframe_elem = $("<img>").attr("src", this._kfrms[i].Icon).attr("name", this._kfrms[i].Name).addClass("tool-buttons").appendTo(this._toolbar);

                if (i > 1)
                    keyframe_elem.addClass("operations");
                else
                    keyframe_elem.addClass((i == 0) ? "variable" : "const");

                keyframe_elem.draggable({
                    helper: "clone", 
                });
            }
        },

        addState: function () {
            var that = this;
            var k = this._options.states.length;
            var stateName = String.fromCharCode(64 + k);
            var state = $("<div>" + stateName + "</div>").addClass("state-button").click(function () {
                var stateIndex = Array.prototype.indexOf.call(that._state_buttons[0].children, this);

                var idx = that._options.states.indexOf(that._activeState);
                $(that._state_buttons[0].children[idx]).removeClass("active");

                that._activeState = that._options.states[stateIndex];

                that.refresh();
            });
            var newState = {
                name: stateName,
                description: "",
                formula: [],
            };
            this._setOption("states", newState);

            var idx = this._options.states.indexOf(this._activeState);
            $(this._state_buttons[0].children[idx]).removeClass("active");

            this._activeState = this._options.states[k];

            this._state_buttons[0].insertBefore(state[0], this._state_buttons[0].lastChild);

            this.refresh();
        },

        addCondition: function () {
            var table = this._create_states_table();
            this._ltl_states[0].insertBefore(table[0], this._ltl_states[0].lastChild);
        },

        _create_states_table: function () {
            var that = this;
            var table = $("<table></table>").addClass("state-condition").attr("data-row-type", "condition");
            var tbody = $("<tbody></tbody>").appendTo(table);
            var tr = $("<tr></tr>").appendTo(tbody);

            for (var i = 0; i < 5; i++) {
                var td = $("<td></td>").appendTo(tr);
                td.droppable({
                    drop: function (event, ui) {
                        var stateIndex = that._options.states.indexOf(that._activeState);

                        var tableIndex = Array.prototype.indexOf.call(that._ltl_states[0].children, table[0]) - 1;

                        switch (ui.draggable[0].name) {
                            case "var": {
                                if (this.cellIndex == 0) {
                                    $(tr[0].cells[2]).droppable("option", "accept", ".const");

                                    for (var j = 3; j < 5; j++)
                                        $(tr[0].cells[j]).droppable("option", "accept", false);

                                } else if (this.cellIndex == 2) {
                                    $(tr[0].cells[0]).droppable("option", "accept", ".const");
                                    $(tr[0].cells[4]).droppable("option", "accept", ".const");
                                } else {
                                    break;
                                }
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "variable",
                                    value: "variable"
                                };
                                break;
                            }
                            case "num": {
                                if (this.cellIndex == 2) {
                                    $(tr[0].cells[0]).droppable("option", "accept", ".variable");

                                    for (var j = 3; j < 5; j++)
                                        $(tr[0].cells[j]).droppable("option", "accept", false);

                                } else if (this.cellIndex == 0 || this.cellIndex == 4) {
                                    $(tr[0].cells[0]).droppable("option", "accept", ".const");
                                    $(tr[0].cells[4]).droppable("option", "accept", ".const");
                                    $(tr[0].cells[2]).droppable("option", "accept", ".variable");
                                }
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "const",
                                    value: "num"
                                }
                                break;
                            }
                            case "equal": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "="
                                }
                                break;
                            }
                            case "more": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: ">"
                                }
                                break;
                            }
                            case "less": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "<"
                                }
                                break;
                            }
                            case "moeq": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: ">="
                                }
                                break;
                            }
                            case "leeq": {
                                var img = $("<img>").attr("src", ui.draggable[0].getAttribute("src")).attr("class", ui.draggable[0].getAttribute("class")).appendTo(this);
                                that._options.states[stateIndex].formula[tableIndex][this.cellIndex] = {
                                    type: "operator",
                                    value: "<="
                                }
                                break;
                            }
                        }
                    }
                });

                if (i % 2 == 1)
                    td.droppable("option", "accept", ".operations");
            }
            var td = $("<td></td>").addClass("LTL-line-del").appendTo(tr).click(function () {
                $(table).remove();
            });
            td[0].appendChild(($("<img>").attr("src", "../images/ltlimgs/remove.png"))[0]);
            return table;
        }
    });
})(jQuery);