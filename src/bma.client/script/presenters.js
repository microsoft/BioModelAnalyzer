/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>
var BMA;
(function (BMA) {
    (function (Presenters) {
        var DesignSurfacePresenter = (function () {
            function DesignSurfacePresenter(bioModel, layout, svgPlotDriver) {
                var _this = this;
                var that = this;
                this.model = bioModel;
                this.layout = layout;
                this.driver = svgPlotDriver;

                window.Commands.On("AddElementSelect", function (type) {
                    _this.selectedType = type;
                    _this.driver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", function (args) {
                    if (that.selectedType !== undefined) {
                        var element = window.ElementRegistry.GetElementByType(that.selectedType);
                        that.driver.Draw(element.RenderToSvg(args.x, args.y));
                    }
                });
            }
            DesignSurfacePresenter.prototype.CreateSvg = function () {
                return null;
            };
            return DesignSurfacePresenter;
        })();
        Presenters.DesignSurfacePresenter = DesignSurfacePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=presenters.js.map
