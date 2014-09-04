﻿declare var BMAExt: any;
declare var InteractiveDataDisplay: any;

describe("SVGPlot", () => {
    it("should be succesfully created", () => {
        var svgPlot = new BMAExt.SVGPlot($("<div></div>"), undefined);
        expect(svgPlot).toBeDefined();
    });
});

describe("DesignSurfacePresenter", () => {

    beforeEach(() => {
        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    });

    it("should be created from BioModel, Layout and ISVGPlot driver instance", () => {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($());
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("should create proper presenter for specified model and layout", () => {

        var appModel = new BMA.Model.AppModel();
        //var drawingSurface = $("<div></div>");
        //drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();//UIDrivers.VariableEditorDriver($());

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, elementPanel, testbutton, testbutton, variableEditorDriver);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("turns navigation on executing 'AddElementSelect' command", () => {
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver();
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, elementPanel, testbutton, testbutton, variableEditorDriver);

        spyOn(svgPlotDriver, "TurnNavigation");
        window.Commands.Execute("AddElementSelect", undefined);
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(true);

        window.Commands.Execute("AddElementSelect", "Container");
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(false);

    });


    it("should initialize the variableEditorDriver", () => {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, testbutton, testbutton, variableEditorDriver);

        
        window.Commands.Execute("AddElementSelect", "Constant");
        console.log("first click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.5, y: 0.5 });
        window.Commands.Execute("AddElementSelect", undefined);
        spyOn(variableEditorDriver, "Initialize");
        console.log("second click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.5, y: 0.5 });
        expect(variableEditorDriver.Initialize).toHaveBeenCalled();
    });


    it("creates drawingsurface widget", () => {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
        expect(ds).toBeDefined();
    });
});