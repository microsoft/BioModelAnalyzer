﻿/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
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
            inputs: ["qqq", "www","eee","rrr"],
            formula: ""
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

            //this.textarea.val(this.options.formula);
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
            this.labletable = $('<table class="inputs-table"></table>').appendTo(that.element);
            var tr0 = $('<tr></tr>').appendTo(that.labletable);
            var td01 = $('<td></td>').appendTo(tr0);
            var nameLabel = $('<label></label>').text("Name").appendTo(td01);

            var td02 = $('<td></td>').appendTo(tr0);
            var rangeLabel = $('<label></label>').text("Range").appendTo(td02);
            var tr = $('<tr></tr>').appendTo(that.labletable);
            var td1 = $('<td></td>').appendTo(tr); 
            this.name = $('<input type="text" size="15">').appendTo(td1);

            var td2 = $('<td></td>').appendTo(tr); 
            this.rangeFrom = $('<input type="text" min="0" max="100" size="1">').appendTo(td2);

            var td3 = $('<td></td>').appendTo(tr); 
            var divtriangles1 = $('<div></div>').appendTo(td3);

            var upfrom = $('<div></div>').addClass("triangle-up").appendTo(divtriangles1);
            upfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                if (valu < 100)
                    that._setOption("rangeFrom", valu + 1);
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {

                var valu = Number(that.rangeFrom.val());
                if (valu > 0) 
                    that._setOption("rangeFrom", valu - 1);
            });

            var td4 = $('<td></td>').appendTo(tr); 
            this.rangeTo = $('<input type="text" min="0" max="100" size="1">').appendTo(td4);

            var td5 = $('<td></td>').appendTo(tr); 

            var divtriangles2 = $('<div></div>').appendTo(td5);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                if (valu < 100)
                //that.rangeTo.val(valu + 1);
                    that._setOption("rangeTo", valu + 1);
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                if (valu > 0) 
                    that._setOption("rangeTo", valu - 1);
            });

            var td6 = $('<td></td>').appendTo(tr); 
            this.expandLabel = $('<button class="editorExpander"></button>').appendTo(td6);
            
        },

        _processExpandingContent: function () {
            var that = this;
            this.content = $('<div></div>').appendTo(this.element).hide();
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
            var insertButton = $('<button class="bma-insert-function-button">Insert</button>').appendTo(div);

            insertButton.bind("click", function () {
                //var about = that.getAbout(that.selected.text());
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
            this.textarea.css("width", "80%");
        },

        _refreshText: function (div: JQuery) {
            var that = this;
            div.empty();
            //var text = this.getAbout(that.selected.text());
            var fun = window.FunctionsRegistry.GetFunctionByName(that.selected.text());

            $('<h2>' + fun.Head + '</h2>').appendTo(div);
            $('<p>' + fun.About + '</p>').appendTo(div);
        },

        _bindExpanding: function () {
            var that = this;

            this.name.bind("input", function () {
                that._setOption("name", that.name.val());
            });

            this.rangeFrom.bind("input", function () {
                that._setOption("rangeFrom", that.rangeFrom.val());
            });

            this.rangeTo.bind("input", function () {
                that._setOption("rangeTo", that.rangeTo.val());
            });

            this.expandLabel.bind("click", function () {
                if (that.content.is(':hidden')) 
                    that.content.show();
                else
                    that.content.hide();
                $(this).toggleClass("editorExpanderChecked", "editorExpander");
            });

            this.textarea.bind("input", function () {
                that._setOption("formula", that.textarea.val());
            });
        },

        _setOption: function (key, value) {
            var that = this;
        //if (key === "inputs")
        //    {
        //        this.listOfInputs.empty();
        //        var inputs = this.options.inputs;
        //        inputs.forEach(function (val, ind) {
        //            var item = $('<div>' + val + '</div>').appendTo(that.listOfInputs);
        //            item.bind("click", function () {
        //                that.textarea.insertAtCaret($(this).text());
        //                that.listOfInputs.hide();
        //            });
        //        });
        //    }

            if (this.options[key] !== value) {
                
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);

            
            this.resetElement();
            that.element.trigger("variableeditorchanged", {});
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
} (jQuery));

interface ElementButton extends JQuery {

}

interface JQuery {
    bmaeditor(): JQuery;
    bmaeditor(settings: Object): JQuery;
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