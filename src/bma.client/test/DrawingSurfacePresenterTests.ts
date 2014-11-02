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
        var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
        var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
        var contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver($("#drawingSurceContainer"));
        var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, contextMenuDriver);
        var testbutton = new BMA.Test.TestUndoRedoButton();
        //var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undefined, svgPlotDriver, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, undefined);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("should create proper presenter for specified model and layout", () => {

        var appModel = new BMA.Model.AppModel();
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver(drawingSurface);
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();//UIDrivers.VariableEditorDriver($());

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undefined, svgPlotDriver, undefined, undefined, elementPanel, variableEditorDriver, undefined);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    xit("turns navigation on executing 'AddElementSelect' command", () => {
        var appModel = new BMA.Model.AppModel();
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver(drawingSurface);
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.Test.TestVariableEditor();

        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undefined, svgPlotDriver, undefined, undefined, elementPanel, variableEditorDriver, undefined);

        spyOn(svgPlotDriver, "TurnNavigation");
        window.Commands.Execute("AddElementSelect", undefined);
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(true);

        window.Commands.Execute("AddElementSelect", "Container");
        expect(svgPlotDriver.TurnNavigation).toHaveBeenCalledWith(false);

    });


    xit("should initialize the variableEditorDriver", () => {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undefined, svgPlotDriver, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, undefined);

        
        window.Commands.Execute("AddElementSelect", "Constant");
        console.log("first click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.11, y: 0.11 });
        window.Commands.Execute("AddElementSelect", undefined);
        spyOn(variableEditorDriver, "Initialize");
        console.log("second click");
        window.Commands.Execute("DrawingSurfaceClick", { x: 0.11, y: 0.11 });
        expect(variableEditorDriver.Initialize).toHaveBeenCalled();
    });


    it("creates drawingsurface widget", () => {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
        expect(ds).toBeDefined();
    });
});