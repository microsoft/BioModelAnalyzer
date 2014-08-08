declare var BMAExt: any;
declare var InteractiveDataDisplay: any;

describe("SVGPlot", () => {
    it("should be succesfully created", () => {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", () => {
    it("should be created from BioModel, Layout and ISVGPlot driver instance", () => {
        var model = new BMA.Model.BioModel([],[],[]);
        var layout = new BMA.Model.Layout([],[]);
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();

        var presenter = new BMA.Presenters.DesignSurfacePresenter(model, layout, svgPlotDriver, undefined, undefined);
        expect(presenter).toBeDefined();
    });

    it("should create proper SVG for specified model and layout", () => {

    });
     
});