describe("SVGPlot", function () {
    it("should be succesfully created", function () {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", function () {
    it("should be created from BioModel, Layout and ISVGPlot driver instance", function () {
        var model = new BMA.Model.BioModel();
        var layout = new BMA.Model.Layout();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();

        var presenter = new BMA.Presenters.DesignSurfacePresenter(model, layout, svgPlotDriver);
        expect(presenter).toBeDefined();
    });
});
//# sourceMappingURL=PresenterTests.js.map
