/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.visibilitysettings", {

        _getList: function () {
            return this.list || this.element.find("ol,ul").eq(0);;
        },

        _create: function () {

            var that = this;
            this.list = this._getList();
            this.items = this.list.find("li");
            this.listOptions = [];
            

            this.items.each(function (ind) {

                var item = this;
                that.listOptions[ind] = {};
                var children = $(item).children();//.find("[data-behavior$=undefined]");
                //var wanted = $("children[data-behavior$='undefined']");
                //alert("children.length ="+ wanted.length);
                children.each(function () {

                    var child = this;
                    var text = $(child).text();
                    var behavior = $(child).attr("data-behavior");
                    
                    if (behavior === undefined) {
                        for (var i = 0; i < ind; i++) {
                            if (that.listOptions[i].name === text) 
                                throw ("Options must be different");
                        }
                        that.listOptions[ind].name = text;
                    }
                    else {
                        var command, value = undefined;
                        try {
                            command = $(child).attr("data-command");
                        }
                        catch (ex) {
                            console.log("Error binding to command: " + ex);
                        }

                        switch (behavior) {
                            case "toggle":
                                if (that.listOptions[ind].toggle === undefined) {
                                    value = command !== undefined ? Boolean($(child).attr("data-default")) || true : undefined;
                                    var button = $('<button></button>')
                                        .appendTo($(child))
                                        .addClass("hoverable");
                                    if (value) button.addClass("buttonON").text("ON");
                                    else button.addClass("buttonOFF").text("OFF");

                                    that.listOptions[ind].toggle = value;
                                    that.listOptions[ind].toggleButton = button;
                                    if (command !== undefined) {
                                        button.bind("click", function (e) {
                                            window.Commands.Execute(command, {checked: value});
                                            that.listOptions[ind].toggle = !that.listOptions[ind].toggle;
                                            that.changeButtonONOFFStyle(ind);
                                        });
                                    }
                                }
                                else
                                    console.log("Names of options should be different");
                                break;

                            case "increment":
                                if (that.listOptions[ind].increment === undefined) {
                                    value = command !== undefined ? $(item).attr("data-default") || 10 : undefined;
                                    var plus = $('<button>+</button>').addClass("plusminus")
                                        .appendTo($(child))
                                        .addClass("hoverable");
                                    var minus = $('<button>-</button>').addClass("plusminus")
                                        .appendTo($(child))
                                        .addClass("hoverable");
                                    that.listOptions[ind].increment = value;
                                }
                                break;
                        }
                        if (value === undefined) {
                            console.log("Undefind command or invalid value");
                        }
                    }
                })
            });
        },

        changeButtonONOFFStyle: function (ind)
        {
            var button = this.listOptions[ind].toggleButton;
            if (!this.listOptions[ind].toggle) {
                button.text("OFF");
                button.removeClass("buttonON").addClass("buttonOFF");
            }
            else {
                button.text("ON");
                button.removeClass("buttonOFF").addClass("buttonON");
            }
        },


        _setOption: function (key, value) {
            switch (key) {
                case "settingsState":
                    for (var i = 0; i < this.listOptions.length; i++) {
                        if (this.listOptions[i].name === value.name) {
                            this.listOptions[i].toggle = value.toggle;
                            this.changeButtonONOFFStyle(i);
                            this.listOptions[i].increment = value.increment;
                            return;
                        }
                        else console.log("No such option");
                    }
                    break;
            }
            $.Widget.prototype._setOption.apply(this, arguments);
            this._super("_setOption", key, value);
        },

        destroy: function () {
            $.Widget.prototype.destroy.call(this);
        }

    });
}(jQuery));

interface JQuery {
    visibilitysettings(): JQuery;
    visibilitysettings(settings: Object): JQuery;
    visibilitysettings(optionLiteral: string, optionName: string): any;
    visibilitysettings(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
