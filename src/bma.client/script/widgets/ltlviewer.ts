/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlviewer", {
        options: {
        },

        _create: function () {
            var elem = this.element;
            var key_div = $('<div></div>').appendTo(elem);
            var key_content = $('<div>TODO: KEYFRAMES</div>').keyframecompact();//.keyframeviewer();
            key_div.resultswindowviewer({
                header: "Keyframes",
                icon: "max",
                content: key_content
            });

            var temp_prop = $('<div></div>').appendTo(elem);
            var temp_content = $('<div>TODO: TEMPORAL PROPERTIES</div>'); //.temppropviewer();
            temp_prop.resultswindowviewer({
                header: "Temporal properties",
                icon: "max",
                content: temp_content
            });

            var results = $('<div></div>').appendTo(elem);
            var res_table = $('<div>TODO: RESULTS</div>');
            results.resultswindowviewer({
                header: "Results",
                icon: "max",
                content: res_table
            });
        }
    });
} (jQuery)); 

interface JQuery {
    ltlviewer(): JQuery;
    ltlviewer(settings: Object): JQuery;
    ltlviewer(optionLiteral: string, optionName: string): any;
    ltlviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}