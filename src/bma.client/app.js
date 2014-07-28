/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model.ts"/>
/// <reference path="script\layout.ts"/>
/// <reference path="script\master.ts"/>
/// <reference path="script\drawingsurface.ts"/>
var BMA;
(function (BMA) {
    var BMAApplicationHub = (function () {
        function BMAApplicationHub() {
        }
        return BMAApplicationHub;
    })();
})(BMA || (BMA = {}));

window.onload = function () {
    $("#content").drawingsurface();
};
//# sourceMappingURL=app.js.map
