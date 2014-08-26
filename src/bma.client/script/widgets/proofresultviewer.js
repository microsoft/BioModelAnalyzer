/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
(function ($) {
    $.widget("BMA.proofresultviewer", {
        options: {
            issucceeded: true,
            time: 0
        },
        refresh: function () {
            var that = this;
            var options = this.options;
            this.element.empty();

            var text = "";
            if (options.issucceeded === undefined || options.time === undefined)
                return;
            var table = $('<table></table>').appendTo(that.element);
            var tr1 = $('<tr></tr>').appendTo(table);
            var td1 = $('<td></td>').appendTo(tr1);
            var td2 = $('<td></td>').appendTo(tr1);
            if (options.issucceeded) {
                $('<img src="../../images/succeeded.png">').appendTo(td1);
                $('<h3 style="color: green; font-weight:bold">Stabilizes</h3>').appendTo(td2);
                $('<p style="font-size:small">BMA succeeded in checking every possible state of the model in ' + options.time + ' seconds. After stepping through separate interactions, the model eventually reached a single stable state.</p>').appendTo(that.element);
            } else {
                $('<img src="../../images/failed.png">').appendTo(td1);
                $('<h3 style="color: red; font-weight:bold">Failed to Stabilize</h3>').appendTo(td2);
                $('<p style="font-size:small">After stepping through separate interactions in the model, the analisys failed to determine a final stable state</p>').appendTo(that.element);
            }

            var variables = $("<div></div>").appendTo(that.element);

            var arr = [];
            arr[0] = [];
            arr[0][0] = 0;
            arr[0][1] = 1;
            arr[1] = [];
            arr[1][0] = 10;
            arr[1][1] = 11;

            var t = BMA.ArrayToTable(arr);
            variables.popupwindow({ header: "Variables", mincontent: t });
            var proofPropagation = $("<div></div>").appendTo(that.element);
            proofPropagation.popupwindow({ header: "Proof Propagation" });
        },
        _create: function () {
            var that = this;

            //this.element.addClass("zoomslider-container");
            this.element.height(250);

            //var options = this.options;
            this.refresh();
        },
        _destroy: function () {
            var contents;

            this.element.empty();
        },
        _setOption: function (key, value) {
            var that = this;
            this._super(key, value);
            this.refresh();
        }
    });
}(jQuery));
//# sourceMappingURL=proofresultviewer.js.map
