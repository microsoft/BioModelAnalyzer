/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.bmaeditor", {
        options: {
            //variable: BMA.Model.Variable
            name: "name",
            rangeFrom: 0,
            rangeTo: 0,
            functions: ["var", "avg", "min", "max", "const", "plus", "minus", "times", "div", "ceil", "floor"],
            inputs: ["qqq", "www","eee","rrr"],
            formula: "formula"
        },

        _resetElement: function () {
            var that = this;
            this.name.val(that.options.name);
            this.rangeFrom.val(that.options.rangeFrom);
            this.rangeTo.val(that.options.rangeTo); 
            this.inputsList.empty();
            var inputs = this.options.inputs;
            inputs.forEach(function (val, ind) {
                $('<option>' + val + '</option>').appendTo(that.inputsList);
            });
            this.inputsList.change(function () {
                var inserting = $(this).children("option:selected").text();
                that._insert(inserting);
            });
            this.textarea.val(this.options.formula);
            //elemDiv.attr("title", this.options.description);
        },

        _insert: function (ins: string) {
            var text = this.textarea.val() + ins;
            this.textarea.val(text);
        },

        _create: function () {
            var that = this;
            this.element.addClass("newWindow");
            this._appendInputs();
            this._processExpandingContent();
            this._bindExpanding();
            this._resetElement();
        },

        _appendInputs: function () {
            var that = this;
            this.labletable = $('<table></table>').appendTo(that.element);
            var tr0 = $('<tr></tr>').appendTo(that.labletable);
            var td01 = $('<td></td>').appendTo(tr0);
            var nameLabel = $('<label></label>').text("Name").appendTo(td01);
            //var td2 = $('<td></tr>').appendTo(labelsdiv);
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
                //if (valu < 100)
                that.rangeFrom.val(valu + 1);
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {

                var valu = Number(that.rangeFrom.val());
                //if (valu > 0) 
                that.rangeFrom.val(valu - 1);
            });

            var td4 = $('<td></td>').appendTo(tr); 
            this.rangeTo = $('<input type="text" min="0" max="100" size="1">').appendTo(td4);

            var td5 = $('<td></td>').appendTo(tr); 

            var divtriangles2 = $('<div></div>').appendTo(td5);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                //if (valu < 100)
                that.rangeTo.val(valu + 1);
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                //if (valu > 0) 
                that.rangeTo.val(valu - 1);
            });

            var td6 = $('<td></td>').appendTo(tr); 
            this.expandLabel = $('<button class="editorExpander"></button>').appendTo(td6);
            
        },

        _processExpandingContent: function () {
            var that = this;
            this.content = $('<div></div>').appendTo(this.element).hide();
            var table = $('<table></table>').appendTo(that.content);
            var tr1 = $('<tr></tr>').appendTo(table);
            var td1 = $('<td></td>').appendTo(tr1);
            var span = $('<div>Target Function</div>').appendTo(td1);
            var tr2 = $('<tr></tr>').appendTo(table);
            var td21 = $('<td></td>').appendTo(tr2);
            var div = $('<div class="bma-functions-list"></div>').appendTo(td21);
            var td22 = $('<td></td>').appendTo(tr2);
            var div22 = $('<div></div>').appendTo(td22);

            var functions = this.options.functions;
            functions.forEach(
                function (val, ind) {
                    var item = $('<div class="label-for-functions">' + val + '</div>').appendTo(div);
                    item.bind("click", function () {
                        that.selected = $(this).addClass("ui-selected");
                        div.children().not(that.selected).removeClass("ui-selected");
                        that._refreshText(div22);
                    });
                });
            var insertButton = $('<button>Insert</button>').appendTo(td22);
            insertButton.bind("click", function () {
                that._insert(that.selected.text());
                //alert(that.selected.text());
            });
            $(div.children()[0]).click();
            
            var tr3 = $('<tr></tr>').appendTo(table);
            var td31 = $('<td></td>').appendTo(tr3);
            this.inputsList = $('<select></select>').appendTo(td31);
            var inputs = this.options.inputs;
            this.textarea = $('<textarea></textarea>').appendTo(that.content);
            this.textarea.css("width", "80%");
        },

        _refreshText: function (div: JQuery) {
            div.empty();
            var text = this.selected.text();
            $('<h2>' + text + '</h2>').appendTo(div);

            switch (text) {
                //add text about function
            };
        },

        _bindExpanding: function () {
            var that = this;
            this.expandLabel.bind("click", function () {
                if (that.content.is(':hidden')) 
                    that.content.show();
                else
                    that.content.hide();
                $(this).toggleClass("editorExpanderChecked", "editorExpander");
            });
        },

        _setOption: function (key, value) {
            if (this.options[key] !== value) {
                this._resetElement();
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
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