describe("SVGPlot", function () {
    it("should be succesfully created", function () {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", function () {
    it("should be created from BioModel, Layout and ISVGPlot driver instance", function () {
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();

        //var presenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, undefined, undefined);
        //expect(presenter).toBeDefined();
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var presenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, testbutton, testbutton);
        expect(presenter).toBeDefined();
    });

    it("should create proper SVG for specified model and layout", function () {
    });

    it("creates drawingsurface widget", function () {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
    });
});
