﻿/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
var BMA;
(function (BMA) {
    (function (UIDrivers) {
        var SVGPlotDriver = (function () {
            function SVGPlotDriver() {
            }
            SVGPlotDriver.prototype.Draw = function (svg) {
            };
            return SVGPlotDriver;
        })();
        UIDrivers.SVGPlotDriver = SVGPlotDriver;
    })(BMA.UIDrivers || (BMA.UIDrivers = {}));
    var UIDrivers = BMA.UIDrivers;
})(BMA || (BMA = {}));
//# sourceMappingURL=uidrivers.js.map
