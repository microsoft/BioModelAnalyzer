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
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        //expect(appModel).toBeDefined();
        //expect(svgPlotDriver).toBeDefined();
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var presenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, testbutton, testbutton);
        expect(presenter).toBeDefined();
    });

    it("should create proper SVG for specified model and layout", () => {

    });
     
});