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
            TFdescription: "",
            formula: "",
            approved: undefined,
            oneditorclosing: undefined,
            onvariablechangedcallback: undefined,
            onformulachangedcallback: undefined,
        },

        resetElement: function () {
            var that = this;
            this.name.val(that.options.name);
            this.rangeFrom.val(that.options.rangeFrom);
            this.rangeTo.val(that.options.rangeTo); 
        },

        SetValidation: function (result: boolean, message: string) {
            var newmessage = this.ParseErrorMessage(message);
            this.texteditor.tftexteditor("SetValidation", result, newmessage);
        },

        ParseErrorMessage: function (message) {
            var newmessage = "";
            if (!message) return newmessage;
            if (message.stack) {
                var splitedMessage = message.stack.split("Expecting");
                var errorPlace = splitedMessage[0];
                var missingPart = splitedMessage[1];
                if (missingPart === undefined) {
                    splitedMessage = message.stack.split("Unexpected");
                } else {
                    missingPart = missingPart.split(" got")[0].toLowerCase().replace(/'eof',/g, "");
                    if (missingPart.endsWith(",")) missingPart = missingPart.slice(0, missingPart.length - 1);
                    newmessage += "Expecting: " + missingPart;
                }
            } else newmessage = message;
            return newmessage;
        },
        
        _create: function () {
            var that = this;
            this.element.addClass("variable-editor");
            this.element.draggable({ containment: "parent", scroll: false });
            this._appendInputs();
            this._bindExpanding();
            this.resetElement();
        },

        _appendInputs: function () {
            var that = this;
            var div = $('<div></div>').addClass("close-icon").appendTo(that.element);
            //var closing = $('<img src="../../images/close.png">').appendTo(div);
            div.bind("click", function () {
                that.options.formula = that.getFormula();
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
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
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });
            var downfrom = $('<div></div>').addClass("triangle-down").appendTo(divtriangles1);
            downfrom.bind("click", function () {
                var valu = Number(that.rangeFrom.val());
                that._setOption("rangeFrom", valu - 1);
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });

            this.rangeTo = $('<input type="text" min="0" max="100" size="1">')
                .attr("placeholder", "max")
                .appendTo(rangeDiv);
            var divtriangles2 = $('<div></div>').addClass("div-triangles").appendTo(rangeDiv);

            var upto = $('<div></div>').addClass("triangle-up").appendTo(divtriangles2);
            upto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu + 1);
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });
            var downto = $('<div></div>').addClass("triangle-down").appendTo(divtriangles2);
            downto.bind("click", function () {
                var valu = Number(that.rangeTo.val());
                that._setOption("rangeTo", valu - 1);
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });

            var descriptionDiv = $("<div></div>")
                .addClass("description")
                .appendTo(that.element);
            $('<div></div>')
                .addClass("window-title")
                .text("Description")
                .appendTo(descriptionDiv);
            this.description = $("<input type='text'>")
                .addClass("description-input")
                .appendTo(descriptionDiv);

            this.switcher = $("<div></div>").addClass("tfswitcher").appendTo(that.element);
            this.textEdButton = $("<div>T</div>").addClass("tfswitch").appendTo(that.switcher).click(function () {
                that.element.removeClass("bmaeditor-expanded").removeClass("bmaeditor-expanded-horizontaly");
                that.switcher.children().removeClass("selected");
                that.textEdButton.addClass("selected");
                if (that.texteditor.css("display") === "none") {
                    try {
                        that.options.formula = /*BMA.ModelHelper.ConvertTFOperationToString(*/that.formulaeditor.formulaeditor("option", "operation");
                        that.texteditor.tftexteditor({ formula: that.options.formula });
                        that.texteditor.show();
                        that.formulaeditor.hide();
                    } catch (ex) {
                        console.log(ex);
                        that.element.addClass("bmaeditor-expanded").addClass("bmaeditor-expanded-horizontaly");
                        that.switcher.children().removeClass("selected");
                        that.formulaEdButton.addClass("selected");
                    }
                }
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                that.updateLayout();
            });

            this.formulaEdButton = $("<div>G</div>").addClass("tfswitch").appendTo(that.switcher).click(function () {
                if (!$(this).hasClass("disabled")) {
                    that.element.addClass("bmaeditor-expanded").addClass("bmaeditor-expanded-horizontaly");
                    that.switcher.children().removeClass("selected");
                    that.formulaEdButton.addClass("selected");
                    if (that.formulaeditor.css("display") === "none") {
                        try {
                            that.options.formula = that.texteditor.tftexteditor("option", "formula");
                            console.log("everything is ok");
                            that.formulaeditor.formulaeditor({
                                formula: that.options.formula
                            });
                            //that.formulaeditor.formulaeditor({
                            //    operation: BMA.ModelHelper.ConvertTargetFunctionToOperation(that.options.formula, that.options.inputs)
                            //});

                            that.texteditor.hide();
                            that.formulaeditor.show();
                        } catch (ex) {
                            console.log(ex);
                            that.element.removeClass("bmaeditor-expanded").removeClass("bmaeditor-expanded-horizontaly");
                            that.switcher.children().removeClass("selected");
                            that.textEdButton.addClass("selected");
                        }
                    }
                    that.updateLayout();
                    //if (that.options.onvariablechangedcallback !== undefined) {
                    //    that.options.onvariablechangedcallback();
                    //}
                }
            });

            this.texteditor = $("<div></div>").appendTo(that.element);
            this.formulaeditor = $("<div></div>").css("margin-top", "20px").css("width", "600px").appendTo(that.element);

            this.formulaeditor.formulaeditor(
                //{
                //onvariablechangedcallback: () => {
                //    that.options.formula = BMA.ModelHelper.ConvertTFOperationToString(that.formulaeditor.formulaeditor("option", "operation"));
                //    that.texteditor.tftexteditor({ formula: that.options.formula });
                //    if (that.options.onvariablechangedcallback !== undefined) {
                //        that.options.onvariablechangedcallback();
                //    }
                //}
                //}
            );
            this.formulaeditor.hide();
            that.textEdButton.addClass("selected");
            
            this.texteditor.tftexteditor({
                onformulachangedcallback: //that.options.onformulachangedcallback,
                (params) => {
                    try {
                        BMA.ModelHelper.ConvertTargetFunctionToOperation(params.formula, that.options.inputs);
                        that.formulaEdButton.removeClass("disabled");
                        that.SetValidation(true, "");
                    } catch (ex) {
                        that.formulaEdButton.addClass("disabled");
                        that.SetValidation(false, ex);
                    }
                },
                //onvariablechangedcallback: () => {
                //    that.options.formula = that.texteditor.tftexteditor("option", "formula");
                //    that.formulaeditor.formulaeditor({
                //        operation: BMA.ModelHelper.ConvertTargetFunctionToOperation(that.options.formula, that.options.inputs)
                //    });
                //    if (that.options.onvariablechangedcallback !== undefined) {
                //        that.options.onvariablechangedcallback();
                //    }
                //}
            });

            if (that.options.formula) {
                this.texteditor.tftexteditor({
                    formula: that.options.formula
                });
                this.formulaeditor.formulaeditor({
                    formula: that.options.formula
                });
                //this.formulaeditor.formulaeditor({
                //    operation: BMA.ModelHelper.ConvertTargetFunctionToOperation(that.options.formula, that.options.inputs)
                //});
            }

            if (that.options.inputs.length) {
                this.texteditor.tftexteditor({
                    inputs: that.options.inputs
                });
                var variables = [];
                for (var i = 0; i < that.options.inputs.length; i++)
                    variables.push({ Name: that.options.inputs[i] });

                this.formulaeditor.formulaeditor({
                    variables: variables
                });
            }
            
        },
        
        updateLayout: function () {
            if (this.formulaeditor !== undefined) {
                this.formulaeditor.formulaeditor("updateLayout");
            }
            this.name.width("calc(100% - 210px)");
        },

        getFormula: function () {
            var that = this;
            if (that.texteditor.css("display") === "none") {
                that.options.formula = BMA.ModelHelper.ConvertTFOperationToString(that.formulaeditor.formulaeditor("option", "operation"));
            } else {
                that.options.formula = that.texteditor.tftexteditor("option", "formula");
            }
            return that.options.formula;
        },

        getOperation: function () {
            var that = this;
            var formula = that.getFormula();
            return BMA.ModelHelper.ConvertTargetFunctionToOperation(formula, that.options.inputs);
        },

        _bindExpanding: function () {
            var that = this;

            this.name.bind("input change", function () {
                that.options.name = that.name.val();
                if (that.options.onvariablechangedcallback !== undefined) {
                    that.options.onvariablechangedcallback();
                }
                //window.Commands.Execute("VariableEdited", {});
            });

            this.rangeFrom.bind("input change", function () {
                that._setOption("rangeFrom", that.rangeFrom.val());
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });

            this.rangeTo.bind("input change", function () {
                that._setOption("rangeTo", that.rangeTo.val());
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
            });

            this.description.bind("input change", function () {
                that.options.TFdescription = that.description.val();
                //if (that.options.onvariablechangedcallback !== undefined) {
                //    that.options.onvariablechangedcallback();
                //}
                //window.Commands.Execute("VariableEdited", {});
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
                    this.texteditor.tftexteditor({ formula: value });
                    if (this.formulaEdButton.hasClass("selected")) {
                        this.formulaeditor.formulaeditor({ formula: value });
                        //try {
                        //    this.formulaeditor.formulaeditor({
                        //        operation: BMA.ModelHelper.ConvertTargetFunctionToOperation(that.options.formula, that.options.inputs)
                        //    });
                        //} catch (ex) {
                        //    console.log(ex);
                        //}
                    }
                    break;
                case "TFdescription":
                    that.options.TFdescription = value;
                    if (this.description.val() !== that.options.TFdescription)
                        this.description.val(that.options.TFdescription);
                    break;
                case "inputs": 
                    this.options.inputs = value;
                    //this.listOfInputs.empty();
                    var inputs = this.options.inputs;
                    var variables = [];
                    for (var i = 0; i < that.options.inputs.length; i++)
                        variables.push({ Name: that.options.inputs[i].Name });

                    this.texteditor.tftexteditor({ inputs: variables});

                    this.formulaeditor.formulaeditor({
                        variables: variables
                    });
                    break;
                case "onformulachangedcallback":
                    that.options.onformulachangedcallback = value;
                    this.texteditor.tftexteditor({
                        onformulachangedcallback: (params) => {
                            try {
                                BMA.ModelHelper.ConvertTargetFunctionToOperation(params.formula, that.options.inputs);
                                that.formulaEdButton.removeClass("disabled");
                                that.element.removeClass('bmaeditor-expanded');
                                this.SetValidation(true, "");
                            } catch (ex) {
                                this.formulaEdButton.addClass("disabled");
                                that.element.addClass('bmaeditor-expanded');
                                this.SetValidation(false, ex);
                            }
                        }
                    });
                    break;
                case "onvariablechangedcallback":
                    that.options.onvariablechangedcallback = value;
                    //this.formulaeditor.formulaeditor(
                    //{
                    //    onvariablechangedcallback: () => {
                    //        that.options.formula = BMA.ModelHelper.ConvertTFOperationToString(that.formulaeditor.formulaeditor("option", "operation"));
                    //        that.texteditor.tftexteditor({ formula: that.options.formula });
                    //        if (that.options.onvariablechangedcallback !== undefined) {
                    //            that.options.onvariablechangedcallback();
                    //        }
                    //    }
                    //}
                    //);
                    //this.texteditor.tftexteditor({
                    //    onvariablechangedcallback: () => {
                    //        that.options.formula = that.texteditor.tftexteditor("option", "formula");
                    //        that.formulaeditor.formulaeditor({
                    //            operation: BMA.ModelHelper.ConvertTargetFunctionToOperation(that.options.formula, that.options.inputs)
                    //        });
                    //        if (that.options.onvariablechangedcallback !== undefined) {
                    //            that.options.onvariablechangedcallback();
                    //        }
                    //    }
                    //});
                    break;
            }
            this._super(key, value);
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
    bmaeditor(fun: string): string;
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