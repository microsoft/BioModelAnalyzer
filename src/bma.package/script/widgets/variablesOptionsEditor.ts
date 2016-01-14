/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\functionsregistry.ts"/>

(function ($) {
    $.widget("BMA.bmaeditor", {
        options: {
            //variable: BMA.Model.Variable
            name: "name",
            rangeFrom: 0,
            rangeTo: 0,
            functions: ["VAR", "CONST"],//, "POS", "NEG"],//],
            operators1: ["+", "-", "*", "/"], 
            operators2: ["AVG", "MIN", "MAX", "CEIL", "FLOOR"],
            inputs: [],
            formula: "",
            approved: undefined,
            oneditorclosing: undefined
        },

        resetElement: function () {
            var that = this;
            this.name.val(that.options.name);
            this.rangeFrom.val(that.options.rangeFrom);
            this.rangeTo.val(that.options.rangeTo); 
            this.listOfInputs.empty();
            var inputs = this.options.inputs;
            inputs.forEach(function (val, ind) {
                var item = $('<div></div>').text(val).appendTo(that.listOfInputs);
                item.bind("click", function () {
                    that.formulaTextArea.insertAtCaret("var(" + $(this).text() + ")").change();
                    that.listOfInputs.hide();
                });
            });

            this.formulaTextArea.val(that.options.formula);
            window.Commands.Execute("FormulaEdited", { formula: that.options.formula, inputs: that._inputsArray() });
        },

        SetValidation: function (result: boolean, message: string) {
            this.options.approved = result;
            var that = this;
            
            if (this.options.approved === undefined) {
                that.element.removeClass('bmaeditor-expanded');
                that.prooficon.removeClass("formula-failed-icon");
                that.prooficon.removeClass("formula-validated-icon");
                this.formulaTextArea.removeClass("formula-failed-textarea");
                this.formulaTextArea.removeClass("formula-validated-textarea");
            }
            else {

                if (this.options.approved === true) {
                    that.prooficon.removeClass("formula-failed-icon").addClass("formula-validated-icon");
                    this.formulaTextArea.removeClass("formula-failed-textarea").addClass("formula-validated-textarea");
                    that.element.removeClass('bmaeditor-expanded');
                }
                else if (this.options.approved === false) {
                    that.prooficon.removeClass("formula-validated-icon").addClass("formula-failed-icon");
                    this.formulaTextArea.removeClass("formula-validated-textarea").addClass("formula-failed-textarea");
                    that.element.addClass('bmaeditor-expanded');
                }

            }
            that.errorMessage.text(message);
            
        },


        getCaretPos: function (jq)
        {
            var obj = jq[0];
            obj.focus();

            if (obj.selectionStart) return obj.selectionStart; //Gecko
            else if ((<any>document).selection)  //IE
            {
                var sel = (<any>document).selection.createRange();
                var clone = sel.duplicate();
                sel.collapse(true);
                clone.moveToElementText(obj);
                clone.setEndPoint('EndToEnd', sel);
                return clone.text.length;
            }

            return 0;
        },

        _create: function () {
            var that = this;
            this.element.addClass("variable-editor");
            this.element.draggable({ containment: "parent", scroll: false  });
            this._appendInputs();
            this._processExpandingContent();
            this._bindExpanding();
            this.resetElement();
        },

        _appendInputs: function () {
            var that = this;
            var div = $('<div></div>').addClass("close-icon").appendTo(that.element);
            var closing = $('<img src="../../images/close.png">').appendTo(div);
            closing.bind("click", function () {
                that.element.hide();
                if (that.options.oneditorclosing !== undefined) {
                    that.options.oneditorclosing();
                }
            });

            var namerangeDiv = $('<div></div>')
                .addClass('editor-namerange-container')
                .appendTo(that.element);
            this.name = $('<input type="text" size="15">')
                .addClass("variable-name")
                .attr("placeholder", "Variable Name")
                .appendTo(namerangeDiv);

            var rangeDiv = $('<div></div>').appendTo(namerangeDiv);
            var rangeLabel = $('<span></span>')
                //.addClass("labels-in-variables-editor variables-editor-headers")
                .text("Range")
                .appendTo(rangeDiv);
            this.rangeFrom = $('<input type="text" min="0" max="100" size="1">')
                .attr("placeholder", "min")
                .appendTo(rangeDiv);
            var divtriangles1 = $('<div></div>').addClass("div-triangles").appendTo(rangeDiv);

            var upfrom = $('<div></div>').addClass("triangle-up").appendTo(divtriangles1);
            upfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                that._setOption("rangeFrom", valu + 1);
                window.Commands.Execute("VariableEdited", {});
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                that._setOption("rangeFrom", valu - 1);
                window.Commands.Execute("VariableEdited", {});
            });

            this.rangeTo = $('<input type="text" min="0" max="100" size="1">')
                .attr("placeholder", "max")
                .appendTo(rangeDiv);
            var divtriangles2 = $('<div></div>').addClass("div-triangles").appendTo(rangeDiv);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu + 1);
                window.Commands.Execute("VariableEdited", {});
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu - 1);
                window.Commands.Execute("VariableEdited", {});
            });

            var formulaDiv = $('<div></div>')
                .addClass('target-function')
                .appendTo(that.element);
            $('<div></div>')
                .addClass("window-title")
                .text("Target Function")
                .appendTo(formulaDiv);
            this.formulaTextArea = $('<textarea></textarea>')
                .attr("spellcheck", "false")
                .addClass("formula-text-area")
                .appendTo(formulaDiv);
            this.prooficon = $('<div></div>')
                .addClass("validation-icon")
                .appendTo(formulaDiv);
            this.errorMessage = $('<div></div>')
                .addClass("formula-validation-message")
                .appendTo(formulaDiv);
        },

        _processExpandingContent: function () {
            var that = this;

            var inputsDiv = $('<div></div>').addClass('functions').appendTo(that.element);
            $('<div></div>')
                .addClass("window-title")
                .text("Inputs")
                .appendTo(inputsDiv);
            var inpUl = $('<ul></ul>').appendTo(inputsDiv);
            //var div = $('<div></div>').appendTo(that.element);
            var operatorsDiv = $('<div></div>').addClass('operators').appendTo(that.element);
            $('<div></div>')
                .addClass("window-title")
                .text("Operators")
                .appendTo(operatorsDiv);
            var opUl1 = $('<ul></ul>').appendTo(operatorsDiv);
            var opUl2 = $('<ul></ul>').appendTo(operatorsDiv);

            this.infoTextArea = $('<div></div>').addClass('operators-info').appendTo(operatorsDiv);

            var functions = this.options.functions;
            functions.forEach(
                function (val, ind) {
                    var item = $('<li></li>').appendTo(inpUl);
                    var span = $('<button></button>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("button"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("button"), that.infoTextArea) }
                        );
                    if (ind !== 0) {
                        item.click(function () {
                            var about = window.FunctionsRegistry.GetFunctionByName($(this).text());
                            that._InsertToFormula(about);
                        })
                    }
                });

            var operators1 = this.options.operators1;
            operators1.forEach(
                function (val, ind) {
                    var item = $('<li></li>').appendTo(opUl1);
                    var span = $('<button></button>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("button"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("button"), that.infoTextArea) }
                        );
                    item.click(function () { 
                        var about = window.FunctionsRegistry.GetFunctionByName($(this).text());
                        that._InsertToFormula(about);
                    })
                });

            var operators2 = this.options.operators2;
            operators2.forEach(
                function (val, ind) {
                    var item = $('<li></li>').appendTo(opUl2);
                    var span = $('<button></button>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("button"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("button"), that.infoTextArea) }
                        );
                    item.click(function () {
                        var about = window.FunctionsRegistry.GetFunctionByName($(this).text());
                        that._InsertToFormula(about);
                    })
                });

            operatorsDiv.width(opUl2.width());

            this.inputsList = inpUl.children().eq(0).addClass("var-button");
            var inpbttn = this.inputsList.children("button").addClass("inputs-list-header");
            var expandinputsbttn = $('<div></div>')
                .addClass('inputs-expandbttn')
                .appendTo(inpbttn);
            this.listOfInputs = $('<div></div>')
                .addClass("inputs-list-content")
                //.width(this.inputsList.outerWidth())
                .appendTo(that.inputsList).hide();


            this.inputsList.bind("click", function () {
                if (that.listOfInputs.is(":hidden")) {
                    that.inputsList.css("border-radius", "15px 15px 0 0");
                    that.listOfInputs.show();
                    inpbttn.addClass('inputs-list-header-expanded');
                }
                else {
                    that.inputsList.css("border-radius", "15px");
                    that.listOfInputs.hide();
                    inpbttn.removeClass('inputs-list-header-expanded');
                }
            });
        },

        _OnHoverFunction: function (item: JQuery, textarea: JQuery) {
            var selected = item.addClass("ui-selected");
            item.parent().children().not(selected).removeClass("ui-selected");
            this._refreshText(selected, textarea);
        },

        _OffHoverFunction: function (item: JQuery, textarea: JQuery) {
            item.parent().children().removeClass("ui-selected");
            textarea.text("");
        },

        _InsertToFormula: function (item: BMA.Functions.BMAFunction) {
            var caret = this.getCaretPos(this.formulaTextArea) + item.Offset;
            this.formulaTextArea.insertAtCaret(item.InsertText).change();
            this.formulaTextArea[0].setSelectionRange(caret, caret);
        },

        _refreshText: function (selected: JQuery,div: JQuery) {
            var that = this;
            div.empty();
            var fun = window.FunctionsRegistry.GetFunctionByName(selected.text());
            $('<h3></h3>').text(fun.Head).appendTo(div);
            $('<p></p>').text(fun.About).appendTo(div);
        },

        _bindExpanding: function () {
            var that = this;

            this.name.bind("input change", function () {
                that.options.name = that.name.val();
                window.Commands.Execute("VariableEdited", {});
            });

            this.rangeFrom.bind("input change", function () {
                that._setOption("rangeFrom", that.rangeFrom.val());
                window.Commands.Execute("VariableEdited", {});
            });

            this.rangeTo.bind("input change", function () {
                that._setOption("rangeTo", that.rangeTo.val());
                window.Commands.Execute("VariableEdited", {});
            });

            this.formulaTextArea.bind("input change propertychange", function () {
                that._setOption("formula", that.formulaTextArea.val());
                window.Commands.Execute("VariableEdited", {});
            });

        },

        _inputsArray() {
            var inputs = this.options.inputs;
            var arr = {};
            for (var i = 0; i < inputs.length; i++) {
                if (arr[inputs[i]] === undefined) arr[inputs[i]] = 1;
                else arr[inputs[i]]++;
            }
            return arr;
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "name":
                    that.options.name = value;
                    this.name.val(that.options.name);
                    break;
                case "rangeFrom":
                    if (value > 100) value = 100;
                    if (value < 0) value = 0;
                    that.options.rangeFrom = value;
                    this.rangeFrom.val(that.options.rangeFrom);
                    break;
                case "rangeTo":
                    if (value > 100) value = 100;
                    if (value < 0) value = 0;
                    that.options.rangeTo = value;
                    this.rangeTo.val(that.options.rangeTo); 
                    break;
                case "formula":
                    that.options.formula = value;
                    var inparr = that._inputsArray();
                    if (this.formulaTextArea.val() !== that.options.formula)
                        this.formulaTextArea.val(that.options.formula);
                    window.Commands.Execute("FormulaEdited", { formula: that.options.formula, inputs: inparr });
                    
                    break;
                case "inputs": 
                    this.options.inputs = value;
                    this.listOfInputs.empty();
                    var inputs = this.options.inputs;
                    inputs.forEach(function (val, ind) {
                        var item = $('<div></div>').text(val).appendTo(that.listOfInputs);
                        item.bind("click", function () {
                            that.formulaTextArea.insertAtCaret("var(" + $(this).text() + ")").change();
                            that.listOfInputs.hide();
                        });
                    });
                    break;
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
            //window.Commands.Execute("VariableEdited", {})
            //this.resetElement();
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));


interface JQuery {
    bmaeditor(): JQuery;
    bmaeditor(settings: Object): JQuery;
    bmaeditor(fun: string, param: any, param2: any): any;
    bmaeditor(optionLiteral: string, optionName: string): any;
    bmaeditor(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}  

jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if ((<any>document).selection) {
                // For Internet Explorer
                this.focus();
                var sel = (<any>document).selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                // For Webkit
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        })
    }
});