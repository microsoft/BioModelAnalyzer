/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.furthertesting", {
        options: {
            header: "Further Testing",
            toggler: undefined,
            tabid: "",
            data: undefined
        },

       

        refresh: function () {
            var that = this;
            var options = this.options;
            if (that.options.data !== undefined) {
                var content = $('<div></div>')
                    .addClass("scrollable-results")
                    .coloredtableviewer({ numericData: that.options.data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                this.results.resultswindowviewer({ header: that.options.header, content: content, icon: "max", tabid: "FurtherTesting" });
            }
            else {
                this.results.resultswindowviewer();
                this.results.resultswindowviewer("destroy");
            }
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

            this.results = $('<div id="FurtherResult"></div>')
                .appendTo(this.element)
                .resultswindowviewer();
            
            //////var table = $('<table></table>').width("100%").appendTo(this.element);
            //////var tr = $('<tr></tr>').appendTo(table);
            ////this.head = $('<div></div>').appendTo(this.element);
            ////this.head.css("position", "relative");
            ////this.head.css("margin-bottom", "10px");
            ////this.header = $('<div></div>')
            ////    .text(options.header)
            ////    .addClass('resultswindowviewer-header')
            ////    .appendTo(this.head);
            ////this.icontd = $('<div></div>').appendTo(this.head);
            //////this.header = $('<div></div>').text(options.header).appendTo(td1);
            ////this.content = $('<div></div>').appendTo(this.element);
            ////this.reseticon();
            this.refresh();
        },

        GetToggler: function () {
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
                    this.header.text(value);
                    break;
                case "data":
                    this.options.data = value;
                    this.refresh();
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