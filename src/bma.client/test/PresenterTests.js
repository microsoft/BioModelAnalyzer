describe("SVGPlot", function () {
    it("should be succesfully created", function () {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", function () {
    beforeEach(function () {
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    });

    it("should be created from BioModel, Layout and ISVGPlot driver instance", function () {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($());
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, svgPlotDriver, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("should create proper presenter for specified model and layout", function () {
        var appModel = new BMA.Model.AppModel();

        //var drawingSurface = $("<div></div>");
        //drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, undefined, elementPanel, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    xit("turns navigation on executing 'AddElementSelect' command", function () {
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, undefined, elementPanel, testbutton, testbutton, variableEditorDriver);

        spyOn(svgPlotDriver, "TurnNavigation");
        window.Commands.Execute("AddElementSelect", undefined);
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(true);

        window.Commands.Execute("AddElementSelect", "Container");
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(false);
    });

    xit("should initialize the variableEditorDriver", function () {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, svgPlotDriver, testbutton, testbutton, variableEditorDriver);

        window.Commands.Execute("AddElementSelect", "Constant");
        console.log("first click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.11, y: 0.11 });
        window.Commands.Execute("AddElementSelect", undefined);
        spyOn(variableEditorDriver, "Initialize");
        console.log("second click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.11, y: 0.11 });
        expect(variableEditorDriver.Initialize).toHaveBeenCalled();
    });

    it("creates drawingsurface widget", function () {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
        expect(ds).toBeDefined();
    });
});
//# sourceMappingURL=PresenterTests.js.map
