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
            functions: ["VAR", "CONST", "POS", "NEG"],//],
            operators1: ["+", "-", "*", "/"], 
            operators2: ["AVG", "MIN", "MAX", "CEIL", "FLOOR"],
            inputs: [],
            formula: "",
            approved: undefined
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
                    that.formulaTextArea.insertAtCaret($(this).text()).change();
                    that.listOfInputs.hide();
                });
            });

            this.formulaTextArea.val(that.options.formula);
            window.Commands.Execute("FormulaEdited", {});
        },

        SetValidation: function (result: boolean, message: string) {
            this.options.approved = result;
            var that = this;
            that.prooficon.removeClass("formula-failed");
            that.prooficon.removeClass("formula-validated");
            if (this.options.approved === true)
                that.prooficon.addClass("formula-validated");
            else if (this.options.approved === false)
                that.prooficon.addClass("formula-failed");
            that.errorMessage.text(message);
        },


        getCaretPos: function (jq)
        {
            var obj = jq[0];
            obj.focus();

            if (obj.selectionStart) return obj.selectionStart; //Gecko
            else if (document.selection)  //IE
            {
                var sel = document.selection.createRange();
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
            this.element.addClass("bma-variables-options-editor");
            this.element.draggable({ containment: "parent", scroll: false  });
            this._appendInputs();
            this._processExpandingContent();
            this._bindExpanding();
            this.resetElement();
        },

        _appendInputs: function () {
            var that = this;
            var closing = $('<img src="../../images/close.png" class="closing-button">').appendTo(that.element);
            closing.bind("click", function () {
                that.element.hide();
            });
            //var div1 = $('<div style="height:20px"></div>').appendTo(that.element);
            //var nameLabel = $('<div class="labels-in-variables-editor"></div>').text("Name").appendTo(div1);
            
            //var inputscontainer = $('<div class="inputs-container"></div>').appendTo(that.element);
            //this.expandLabel = $('<button class="editorExpander"></button>').appendTo(inputscontainer);
            this.name = $('<input type="text" size="15">')
                .attr("placeholder", "Variable Name")
                .appendTo(that.element);

            var rangeDiv = $('<div></div>').appendTo(that.element);
            var rangeLabel = $('<span class="labels-in-variables-editor"></span>').text("Range").appendTo(rangeDiv);
            this.rangeFrom = $('<input type="text" min="0" max="100" size="1">')
                .attr("placeholder", "min")
                .appendTo(rangeDiv);
            var divtriangles1 = $('<div class="div-triangles"></div>').appendTo(rangeDiv);

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
            var divtriangles2 = $('<div class="div-triangles"></div>').appendTo(rangeDiv);

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

            var formulaDiv = $('<div></div>').appendTo(that.element);
            $('<div></div>').text("Target Function").appendTo(formulaDiv);

            this.prooficon = $('<div><div>')
                .addClass("bma-formula-validation-icon")
                .appendTo(formulaDiv);

            this.formulaTextArea = $('<textarea></textarea>').attr("spellcheck", "false").appendTo(formulaDiv);
            
            this.errorMessage = $('<div></div>')
                .addClass("bma-formula-validation-message")
                .appendTo(formulaDiv);
        },

        _processExpandingContent: function () {
            var that = this;
            //this.content = $('<div class="expanding"></div>').appendTo(this.element);
            //var span = $('<div>Target Function</div>').appendTo(that.content);

            var inputsDiv = $('<div></div>').addClass('list-of-functions').appendTo(that.element);
            $('<div></div>').text("Inputs").appendTo(inputsDiv);
            var inpUl = $('<ul></ul>').appendTo(inputsDiv);
            var div = $('<div></div>').appendTo(that.element);
            var operatorsDiv = $('<div></div>').addClass('list-of-operators').appendTo(div);
            $('<div></div>').text("Operators").appendTo(operatorsDiv);
            var opUl1 = $('<ul></ul>').appendTo(operatorsDiv);
            var opUl2 = $('<ul></ul>').appendTo(operatorsDiv);

            //var div1 = $('<div class="bma-functions-list"></div>').appendTo(div);
            this.infoTextArea = $('<div class="functions-info"></div>').appendTo(div);

            var functions = this.options.functions;
            functions.forEach(
                function (val, ind) {
                    var item = $('<li></li>').appendTo(inpUl);
                    var span = $('<span></span>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("span"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("span"), that.infoTextArea) }
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
                    var span = $('<span></span>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("span"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("span"), that.infoTextArea) }
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
                    var span = $('<span></span>').text(val).appendTo(item);
                    item.hover(
                        function () { that._OnHoverFunction($(this).children("span"), that.infoTextArea) },
                        function () { that._OffHoverFunction($(this).children("span"), that.infoTextArea) }
                        );
                    item.click(function () {
                        var about = window.FunctionsRegistry.GetFunctionByName($(this).text());
                        that._InsertToFormula(about);
                    })
                });

            //var insertButton = $('<button class="bma-insert-function-button">insert</button>').appendTo(div);

            //insertButton.bind("click", function () {
            //    var about = window.FunctionsRegistry.GetFunctionByName(that.selected.text());
            //    var caret = that.getCaretPos(that.formulaTextArea) + about.Offset;
            //    that.formulaTextArea.insertAtCaret(about.InsertText).change();
            //    that.formulaTextArea[0].setSelectionRange(caret, caret);
            //});
            //$(div1.children()[0]).click();

            this.inputsList = inpUl.children().eq(0).addClass("inputs-list-header-collapsed");
            this.listOfInputs = $('<div class="inputs-list-content"></div>').width(this.inputsList.outerWidth()).appendTo(that.inputsList).hide();


            this.inputsList.bind("click", function () {
                if (that.listOfInputs.is(":hidden") && that.listOfInputs.children().length !== 0) {
                    that.inputsList.css("border-radius", "10px 10px 0 0");
                    that.listOfInputs.show();
                }
                else {
                    that.inputsList.css("border-radius", "10px");
                    that.listOfInputs.hide();
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
            $('<p style="font-weight: bold">' + fun.Head + '</p>').appendTo(div);
            $('<p>' + fun.About + '</p>').appendTo(div);
        },

        _bindExpanding: function () {
            var that = this;

            this.name.bind("input change", function () {
                that._setOption("name", that.name.val());
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

            //this.expandLabel.bind("click", function () {
            //    if (that.content.is(':hidden')) 
            //        that.content.show();
            //    else
            //        that.content.hide();
            //    $(this).toggleClass("editorExpanderChecked", "editorExpander");
            //});

            this.formulaTextArea.bind("input change propertychange", function () {
                that._setOption("formula", that.formulaTextArea.val());
                //that.options.formula = that.formulaTextArea.val();
                window.Commands.Execute("VariableEdited", {});
            });

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

                    if (this.formulaTextArea.val() !== that.options.formula)
                        this.formulaTextArea.val(that.options.formula);
                    window.Commands.Execute("FormulaEdited", that.options.formula);
                    
                    break;
                case "inputs": 
                    this.options.inputs = value;
                    this.listOfInputs.empty();
                    var inputs = this.options.inputs;
                    inputs.forEach(function (val, ind) {
                        var item = $('<div></div>').text(val).appendTo(that.listOfInputs);
                        item.bind("click", function () {
                            that.formulaTextArea.insertAtCaret($(this).text()).change();
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
            if (document.selection) {
                // Для браузеров типа Internet Explorer
                this.focus();
                var sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                // Для браузеров типа Firefox и других Webkit-ов
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