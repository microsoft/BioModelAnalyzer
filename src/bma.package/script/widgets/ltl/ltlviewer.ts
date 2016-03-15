/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlviewer", {
        options: {
            opentpeditor: undefined,
            openstateseditor: undefined,
        },

        _create: function () {
            var that = this;
            var elem = this.element.addClass('ltl-results-tab');
            this.key_div = $('<div></div>').appendTo(elem);

            this.key_content = $('<div></div>').width(400).statescompact();
            this.key_content.click(function () {
                if (that.options.openstateseditor !== undefined)
                    that.options.openstateseditor();
            });

            this.key_div.resultswindowviewer({
                header: "States",
                icon: "max",
                content: that.key_content,
                tabid: "LTLStates"
            });

            this.temp_prop = $('<div></div>').appendTo(elem);

            this.temp_content = $('<div></div>').width(400).height(150).temporalpropertiesviewer();
            this.temp_content.click(function () {
                if (that.options.opentpeditor !== undefined) {
                    that.options.opentpeditor();
                }
            });

            this.temp_prop.resultswindowviewer({
                header: "Temporal properties",
                icon: "max",
                content: this.temp_content,
                tabid: "LTLTempProp"
            });

        },

        _destroy: function () {
            this.element.empty();
        },

        Get: function (param) {
            switch (param) {
                case "LTLStates":
                    return this.key_div;
                case "LTLTempProp":
                    return this.temp_prop;
                case "LTLResults":
                    return this.results;
                default:
                    return undefined;
            }
        },

        GetTPViewer: function () {
            return this.temp_content;
        },

        GetStatesViewer: function () {
            return this.key_content;
        },

        Show: function (param) {
            if (param == undefined) {
                this.key_div.show();
                this.temp_prop.show();
                //this.results.show();
            }
        }
    });
} (jQuery)); 

interface JQuery {
    ltlviewer(): JQuery;
    ltlviewer(settings: Object): JQuery;
    ltlviewer(optionLiteral: string, optionName: string): any;
    ltlviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}