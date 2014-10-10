/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.furthertesting", {
        options: {
            header: "Further Testing",
            toggler: undefined,
            tabid: "FurtherTesting",
            data: undefined,
            buttonMode: "ActiveMode"
        },

        ChangeMode: function () {
            var that = this;
            var toAddClass = "", toRemoveClass = "", text = "";
            switch (this.options.buttonMode) {
                case "ActiveMode":
                    toAddClass = "bma-furthertesting-button";
                    toRemoveClass = "bma-furthertesting-button-waiting";
                    text = "Further Testing";
                    //this.toggler.bind("click", function () {
                    //    window.Commands.Execute("FurtherTestingRequested", {});
                    //})
                    break;
                case "StandbyMode":
                    toAddClass = "bma-furthertesting-button-waiting";
                    toRemoveClass = "bma-furthertesting-button";
                    //this.toggler.unbind("click");
                    break;
            }
            this.toggler.removeClass(toRemoveClass).addClass(toAddClass).text(text);
        },

        _create: function () {
            var that = this;
            var options = this.options;
            var defaultToggler = $('<button></button>').text("Further Testing").addClass('bma-furthertesting-button');

            this.toggler = that.options.toggler || defaultToggler;
            this.toggler
                .appendTo(this.element)
                .hide();
            this.toggler.bind("click", function () {
                window.Commands.Execute("FurtherTestingRequested", {});
            })

            this.results = $('<div></div>')
                .appendTo(this.element)
                .resultswindowviewer();
            this.refresh();
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (that.options.data !== undefined) {
                var content = $('<div></div>')
                    .addClass("scrollable-results")
                    .coloredtableviewer({ numericData: that.options.data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                this.results.resultswindowviewer({ header: that.options.header, content: content, icon: "max", tabid: that.options.tabid });
            }
            else {
                this.results.resultswindowviewer();
                this.results.resultswindowviewer("destroy");
            }
            this.ChangeMode();
        },

        GetToggler: function (): JQuery {
            return this.toggler;
        },

        ShowStartToggler: function () {
            this.toggler.show();
        },

        HideStartToggler: function () {
            this.toggler.hide();
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "header":
                    this.options.header = value;
                    this.header.text(value);
                    break;
                case "data":
                    this.options.data = value;
                    this.refresh();
                    break;
                case "buttonMode":
                    this.options.buttonMode = value;
                    this.ChangeMode();
                    break;
            }

            this._super(key, value);
            //this.refresh();
        }
    });
} (jQuery));

interface JQuery {
    furthertesting(): JQuery;
    furthertesting(settings: Object): JQuery;
    furthertesting(fun: string): any;
    furthertesting(optionLiteral: string, optionName: string): any;
    furthertesting(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}  