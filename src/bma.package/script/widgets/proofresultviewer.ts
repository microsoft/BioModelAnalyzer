// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.proofresultviewer", {
        options: {
            issucceeded: true,
            time: 0,
            data: undefined,
            message: ''
        },

        refreshSuccess: function () {
            var that = this;
            var options = this.options;
            this.resultDiv.empty();

            switch (options.issucceeded) {
                case true:
                    $('<img src="../../images/succeeded.svg">').appendTo(this.resultDiv);
                    $('<div></div>').addClass('stabilize-prooved').text('Stabilizes').appendTo(this.resultDiv);
                    break;

                case false:
                    $('<img src="../../images/failed.svg">').appendTo(this.resultDiv);
                    $('<div></div>').addClass('stabilize-failed').text('Failed to Stabilize').appendTo(this.resultDiv);
                    break;

                case undefined:
                    $('<img src="../../images/failed.svg">').appendTo(this.resultDiv);
                    $('<div></div>').addClass('stabilize-failed').text('Service Error').appendTo(this.resultDiv);
                    break;

                default:
                    $('<img src="../../images/failed.svg">').appendTo(this.resultDiv);
                    $('<div></div>').addClass('stabilize-failed').text(options.issucceeded).appendTo(this.resultDiv);
                    break;
            }
        },

        refreshMessage: function () {
            this.proofmessage.text(this.options.message);
        },

        refreshData: function () {
            var that = this;
            var options = this.options;
            this.compactvariables.resultswindowviewer();
            this.proofPropagation.resultswindowviewer();

            if (options.data !== undefined && options.data.numericData !== undefined && options.data.numericData !== null && options.data.numericData.length !== 0) {
                var variables = $("<div></div>")
                    .addClass("scrollable-results")
                    .coloredtableviewer({
                    header: ["Name", "Formula", "Range"],
                    numericData: options.data.numericData,
                    colorData: options.data.colorVariables
                });
                this.compactvariables.resultswindowviewer({
                    header: "Variables",
                    content: variables,
                    icon: "max",
                    tabid: "ProofVariables"
                });

                if (options.data.colorData !== undefined && options.data.colorData !== null && options.data.colorData.length !== 0) {
                    var proof = $("<div></div>")
                        .addClass("scrollable-results")
                        .coloredtableviewer({
                        type: "color",
                        colorData: options.data.colorData,
                    });
                    this.proofPropagation.resultswindowviewer({
                        header: "Proof Propagation",
                        content: proof,
                        icon: "max",
                        tabid: "ProofPropagation"
                    });
                }
            }
            else {
                this.compactvariables.resultswindowviewer("destroy");
                this.proofPropagation.resultswindowviewer("destroy");
            }
        },

        show: function (tab) {
            if (tab === undefined) {
                this.compactvariables.show();
                this.proofPropagation.show();
            }
            if (tab === "ProofVariables") {
                this.compactvariables.show();
            }
            if (tab === "ProofPropagation") {
                this.proofPropagation.show();
            }
        },

        hide: function (tab) {
            if (tab === "ProofVariables") {
                this.compactvariables.hide();
                this.element.children().not(this.compactvariables).show();
            }
            if (tab === "ProofPropagation") {
                this.proofPropagation.hide();
                this.element.children().not(this.proofPropagation).show();
            }
        },

        _create: function () {
            var that = this;
            var options = this.options;
            //$('<span>Proof Analysis</span>')
            //    .addClass('window-title')
            //    .appendTo(that.element);
            this.resultDiv = $('<div></div>')
                .addClass("proof-state")
                .appendTo(that.element);

            this.proofmessage = $('<p></p>').appendTo(that.element);
            $('<br></br>').appendTo(that.element);

            this.compactvariables = $('<div></div>')
                .addClass('proof-variables')
                .appendTo(that.element)
                .resultswindowviewer();

            this.proofPropagation = $('<div></div>')
                .addClass('proof-propagation')
                .appendTo(that.element)
                .resultswindowviewer();

            this.refreshSuccess();
            this.refreshMessage();
            this.refreshData();
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "issucceeded": this.options.issucceeded = value;
                    this.refreshSuccess();
                    break;
                case "data":
                    this.options.data = value;
                    this.refreshData();
                    break;
                case "message":
                    this.options.message = value;
                    this.refreshMessage();
                    break;
            }

            this._super(key, value);
        }

    });
} (jQuery));

interface JQuery {
    proofresultviewer(): JQuery;
    proofresultviewer(settings: Object): JQuery;
    proofresultviewer(optionLiteral: string, optionName: string): any;
    proofresultviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}
