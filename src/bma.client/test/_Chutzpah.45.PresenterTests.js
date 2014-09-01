describe("SVGPlot", function () {
    it("should be succesfully created", function () {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", function () {
    it("should be created from BioModel, Layout and ISVGPlot driver instance", function () {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($());
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("should create proper presenter for specified model and layout", function () {
        var appModel = new BMA.Model.AppModel();

        //var drawingSurface = $("<div></div>");
        //drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($());
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, elementPanel, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("creates drawingsurface widget", function () {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
        expect(ds).toBeDefined();
    });
});
