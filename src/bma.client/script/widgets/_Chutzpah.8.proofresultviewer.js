/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.proofresultviewer", {
        options: {
            issucceeded: true,
            time: 0,
            data: undefined
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.resultDiv.empty();
            this.successTable = $('<table></table>').appendTo(this.resultDiv);

            //var text = "";
            if (options.issucceeded === undefined || options.time === undefined)
                return;

            var tr1 = $('<tr></tr>').appendTo(this.successTable);
            var td1 = $('<td></td>').appendTo(tr1);
            var td2 = $('<td></td>').appendTo(tr1);
            if (options.issucceeded) {
                $('<img src="../../images/succeeded.png">').appendTo(td1);
                $('<h3 style="color: green; font-weight:bold">Stabilizes</h3>').appendTo(td2);
                $('<p style="font-size:small">BMA succeeded in checking every possible state of the model in ' + options.time + ' seconds. After stepping through separate interactions, the model eventually reached a single stable state.</p>').appendTo(that.resultDiv);
            } else {
                $('<img src="../../images/failed.png">').appendTo(td1);
                $('<h3 style="color: red; font-weight:bold">Failed to Stabilize</h3>').appendTo(td2);
                $('<p style="font-size:small">After stepping through separate interactions in the model, the analisys failed to determine a final stable state</p>').appendTo(that.resultDiv);
            }
            //var arr = [];
            //arr[0] = [];
            //arr[0][0] = 0;
            //arr[0][1] = 1;
            //arr[0][2] = 2;
            //arr[1] = [];
            //arr[1][0] = 10;
            //arr[1][1] = 11;
            //arr[1][2] = 12;
            //arr[2] = [];
            //arr[2][0] = 20;
            //arr[2][1] = 21;
            //arr[2][2] = 22;
            //var log = [];
            //log[0] = [];
            //log[1] = [];
            //log[1][1] = true;
            //log[2] = [];
            //log[2][1] = false;
            //window.Commands.On("Expand", (param) => {
            //    alert(param);
            //});
        },
        show: function (tab) {
            if (tab === "Variables") {
                this.compactvariables.show();
            }
            if (tab === "Proof Propagation") {
                this.proofPropagation.show();
            }
        },
        hide: function (tab) {
            if (tab === "Variables") {
                this.compactvariables.hide();
                this.proofPropagation.show();
            }
            if (tab === "Proof Propagation") {
                this.proofPropagation.hide();
                this.compactvariables.show();
            }
        },
        _create: function () {
            var that = this;
            var options = this.options;
            this.resultDiv = $('<div></div>').appendTo(that.element);
            this.successTable = $('<table></table>').appendTo(this.resultDiv);
            this.variables = $("<div></div>").coloredtableviewer({ numericData: options.numericData, colorData: options.colorData, header: ["Variables", "Formula", "Range"] });
            this.compactvariables = $('<div></div>').appendTo(that.element).resultswindowviewer({ header: "Variables", content: that.variables, icon: "max" });

            this.proof = $("<div></div>");
            this.proofPropagation = $("<div></div>").appendTo(that.element).resultswindowviewer({ header: "Proof Propagation", content: that.proof, icon: "max" });
            this.refresh();
        },
        _destroy: function () {
            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            if (key === "issucceeded") {
                this.options.issucceeded = value;
                this.refresh();
            }

            if (key === "time") {
                this.options.time = value;
                this.refresh();
            }

            if (key === "data") {
                this.options.data = value;
                this.variables.coloredtableviewer(value);
            }

            this._super(key, value);
        }
    });
}(jQuery));
