/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlviewer", {
        options: {
        },

        _create: function () {
            var that = this;
            var elem = this.element.addClass('ltl-results-tab');
            this.key_div = $('<div></div>').appendTo(elem);
            var key_content = $('<div></div>').keyframecompact();//.keyframeviewer();
            this.key_div.resultswindowviewer({
                header: "Keyframes",
                icon: "max",
                content: key_content,
                tabid: "LTLStates"
            });

            this.temp_prop = $('<div></div>').appendTo(elem);

            /*
            var temp_content = $('<div></div>'); //.temppropviewer();
            this.formula = $('<input type="text">').appendTo(temp_content);
            var submit = $('<button>LTLNOW</button>').addClass('action-button green').appendTo(temp_content);
            submit.click(() => {
                window.Commands.Execute("LTLRequested", { formula: this.formula.val()});
            });
            */
            this.temp_content = $('<div></div>').height(150).temporalpropertiesviewer();

            this.temp_prop.resultswindowviewer({
                header: "Temporal properties",
                icon: "max",
                content: this.temp_content,
                tabid: "LTLTempProp"
            });

            /*
            this.results = $('<div></div>').appendTo(elem);
            var res_table = $('<div id="LTLResults"></div>').addClass('scrollable-results');
            this.results.resultswindowviewer({
                header: "Results",
                icon: "max",
                content: res_table,
                tabid: "LTLResults"
            });
            */
        },

        _destroy: function () {
            this.element.empty();
        },

        Get: function (param) {
            switch (param) {
                case "LTLStates":
                    //alert('widget ' + this.key_content.text());
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