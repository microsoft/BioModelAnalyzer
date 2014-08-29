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
            arr[0][2] = 2;
            arr[1] = [];
            arr[1][0] = 10;
            arr[1][1] = 11;
            arr[1][2] = 12;
            arr[2] = [];
            arr[2][0] = 20;
            arr[2][1] = 21;
            arr[2][2] = 22;

            var t = BMA.ArrayToTable(arr);
            t.addClass("bma-prooftable");
            variables.compactproofviewer({ header: "Variables", content: t.clone(), icon: "max" });
            var popup = $('<div class="popup-window"></div>').appendTo('body').hide();

            var proofPropagation = $("<div></div>").appendTo(that.element);
            proofPropagation.compactproofviewer({ header: "Proof Propagation", icon: "max" });

            var log = [];
            log[0] = [];
            log[1] = [];
            log[1][1] = true;
            log[2] = [];
            log[2][1] = false;
            BMA.LogicToTable(t, log);

            popup.compactproofviewer({ header: "Variables", content: t, icon: "min" });
            var button = variables.compactproofviewer("getbutton");
            var closepopup = popup.compactproofviewer("getbutton");

            button.bind("click", function () {
                popup.compactproofviewer("toggle");
                variables.hide();
            });

            closepopup.bind("click", function () {
                variables.show();
                popup.compactproofviewer("toggle");
            });
        },
        _create: function () {
            var that = this;
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
