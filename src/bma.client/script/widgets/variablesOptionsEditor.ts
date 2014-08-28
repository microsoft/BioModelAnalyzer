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
            functions: ["var", "avg", "min", "max", "const", "plus", "minus", "times", "div", "ceil", "floor"],
            inputs: ["qqq", "www", "eee", "rrr"],
            formula: "",
            approved: true
        },

        initialize: function (data) {
            this.options.name = data.name;
            this.options.rangeFrom = data.rangeFrom;
            this.options.rangeTo = data.rangeTo;
            this.options.inputs = data.inputs;
            this.options.formula = data.formula;
            this.options.approved = data.approved;
        },

        resetElement: function () {
            var that = this;
            this.name.val(that.options.name);
            this.rangeFrom.val(that.options.rangeFrom);
            this.rangeTo.val(that.options.rangeTo); 
            this.listOfInputs.empty();
            var inputs = this.options.inputs;
            inputs.forEach(function (val, ind) {
                var item = $('<div>' + val + '</div>').appendTo(that.listOfInputs);
                item.bind("click", function () {
                    that.textarea.insertAtCaret($(this).text());
                    that.listOfInputs.hide();
                });
            });

            if (this.options.approved) {
                that.prooficon.removeClass("formula-not-validated");
                that.prooficon.addClass("formula-validated");
            }
            else {
                that.prooficon.removeClass("formula-validated");
                that.prooficon.addClass("formula-not-validated");
            };
            this.textarea.val(that.options.formula);
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
            this.element.addClass("newWindow");
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
            var div1 = $('<div style="height:20px"></div>').appendTo(that.element);
            var nameLabel = $('<div class="labels-in-variables-editor"></div>').text("Name").appendTo(div1);
            var rangeLabel = $('<div class="labels-in-variables-editor"></div>').text("Range").appendTo(div1);
            var inputscontainer = $('<div class="inputs-container"></div>').appendTo(that.element);
            this.expandLabel = $('<button class="editorExpander"></button>').appendTo(inputscontainer);
            this.name = $('<input type="text" size="15">').appendTo(inputscontainer);

            this.rangeFrom = $('<input type="text" min="0" max="100" size="1">').appendTo(inputscontainer);
            var divtriangles1 = $('<div class="div-triangles"></div>').appendTo(inputscontainer);

            var upfrom = $('<div></div>').addClass("triangle-up").appendTo(divtriangles1);
            upfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                that._setOption("rangeFrom", valu + 1);
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                that._setOption("rangeFrom", valu - 1);
            });

            this.rangeTo = $('<input type="text" min="0" max="100" size="1">').appendTo(inputscontainer);
            var divtriangles2 = $('<div class="div-triangles"></div>').appendTo(inputscontainer);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu + 1);
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu - 1);
            });
        },

        _processExpandingContent: function () {
            var that = this;
            this.content = $('<div class="expanding"></div>').appendTo(this.element).hide();
            var span = $('<div>Target Function</div>').appendTo(that.content);

            var div = $('<div></div>').appendTo(that.content);
            var div1 = $('<div class="bma-functions-list"></div>').appendTo(div);
            var div2 = $('<div class="functions-info"></div>').appendTo(div);

            var functions = this.options.functions;
            functions.forEach(
                function (val, ind) {
                    var item = $('<div class="label-for-functions">' + val + '</div>').appendTo(div1);
                    item.bind("click", function () {
                        that.selected = $(this).addClass("ui-selected");
                        div1.children().not(that.selected).removeClass("ui-selected");
                        that._refreshText(div2);
                    });
                });
            var insertButton = $('<button class="bma-insert-function-button">insert</button>').appendTo(div);

            insertButton.bind("click", function () {
                var about = window.FunctionsRegistry.GetFunctionByName(that.selected.text());
                var caret = that.getCaretPos(that.textarea) + about.Offset;
                that.textarea.insertAtCaret(about.InsertText);
                that.textarea[0].setSelectionRange(caret, caret);
            });
            $(div1.children()[0]).click();

            this.inputsList = $('<div class="inputs-list-header">Inputs</div>').appendTo(that.content);

            this.listOfInputs = $('<div class="inputs-list-content"></div>').appendTo(that.content).hide();
            this.inputsList.bind("click", function () {
                if (that.listOfInputs.is(":hidden"))
                    that.listOfInputs.show();
                else that.listOfInputs.hide();
            });

            var inputs = this.options.inputs;
            this.textarea = $('<textarea></textarea>').appendTo(that.content);
            this.prooficon = $('<div><div>').appendTo(that.content);
        },

        _refreshText: function (div: JQuery) {
            var that = this;
            div.empty();
            var fun = window.FunctionsRegistry.GetFunctionByName(that.selected.text());
            $('<p style="font-weight: bold">' + fun.Head + '</p>').appendTo(div);
            $('<p>' + fun.About + '</p>').appendTo(div);
        },

        _bindExpanding: function () {
            var that = this;

            this.name.bind("input change", function () {
                that._setOption("name", that.name.val());
            });

            this.rangeFrom.bind("input change", function () {
                that._setOption("rangeFrom", that.rangeFrom.val());
            });

            this.rangeTo.bind("input change", function () {
                that._setOption("rangeTo", that.rangeTo.val());
            });

            this.expandLabel.bind("click", function () {
                if (that.content.is(':hidden')) 
                    that.content.show();
                else
                    that.content.hide();
                $(this).toggleClass("editorExpanderChecked", "editorExpander");
            });

            this.textarea.bind("input change", function () {
                that._setOption("formula", that.textarea.val());
            });
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "rangeFrom" || key === "rangeTo")
            {
                if (value > 100) {
                    that._setOption(key, 100);
                    return;
                }

                if (value < 0) {
                    that._setOption(key, 0);
                    return;
                } 
            }

            if (this.options[key] !== value) {
                
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);

            
            this.resetElement();
            window.Commands.Execute("variableeditorchanged", {});
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));


interface JQuery {
    bmaeditor(): JQuery;
    bmaeditor(settings: Object): JQuery;
    bmaeditor(fun: string, param: any): any;
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