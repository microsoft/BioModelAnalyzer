// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.furthertesting", {
        options: {
            header: "Further Testing",
            toggler: undefined,
            tabid: "FurtherTesting",
            buttonMode: "ActiveMode",
            tableHeaders: [],
            data: [],
            tabLabels: [],
        },

        ChangeMode: function () {
            var that = this;
            switch (this.options.buttonMode) {
                case "ActiveMode":
                    this.toggler
                        .removeClass("waiting")
                        .text("Further Testing");
                    break;
                case "StandbyMode":
                    this.toggler
                        .addClass("waiting")
                        .text("");
                    var snipper = $('<div class="spinner"></div>').appendTo(this.toggler);
                    for (var i = 1; i < 4; i++) {
                        $('<div></div>').addClass('bounce' + i).appendTo(snipper);
                    }
                    break;
            }
            
        },

        _create: function () {
            var that = this;
            var options = this.options;
            var defaultToggler = $('<button></button>').text("Further Testing").addClass('action-button-small red further-testing-button');

            this.element.addClass("further-testing-box");
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
            var tabs = $('<div></div>');
            var ul = $('<ul></ul>').appendTo(tabs);
            if (that.options.data !== null && that.options.data !== undefined && that.options.tabLabels !== null && that.options.tabLabels !== undefined && that.options.tableHeaders !== null && that.options.tableHeaders !== undefined) {
                for (var i = 0; i < that.options.data.length; i++) {
                    var li = $('<li></li>').appendTo(ul);
                    var a = $('<a href="#FurtherTestingTab' + i + '"></a>').appendTo(li);
                    that.options.tabLabels[i].appendTo(a);
                }

                for (var i = 0; i < that.options.data.length; i++) {
                    var content = $('<div></div>')
                        .attr('id', 'FurtherTestingTab' + i)
                        .addClass("scrollable-results")
                        .coloredtableviewer({ numericData: that.options.data[i], header: that.options.tableHeaders[i] })
                        .appendTo(tabs);
                    content.find('table').removeClass('variables-table').addClass('furhter-testing');
                }

                
                tabs.tabs();
                tabs.removeClass("ui-widget ui-widget-content ui-corner-all");
                tabs.children("ul").removeClass("ui-helper-reset ui-widget-header ui-corner-all");
                tabs.children("div").removeClass("ui-tabs-panel ui-widget-content ui-corner-bottom");

                this.results.resultswindowviewer({ header: that.options.header, content: tabs, icon: "max", tabid: that.options.tabid });
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

        SetData: function (arg) {
            if (arg !== undefined) {
                this.options.data = arg.data;
                this.options.tableHeaders = arg.tableHeaders;
                this.options.tabLabels = arg.tabLabels;
            }
            else {
                this.options.data = this.options.tableHeaders = this.options.tabLabels = undefined;
            }
            this.refresh();
            
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
    furthertesting(fun: string, arg: any): any;
    furthertesting(optionLiteral: string, optionName: string): any;
    furthertesting(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}  
