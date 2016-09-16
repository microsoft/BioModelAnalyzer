declare var BMAExt: any;
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
        window.PlotSettings = {
            MaxWidth: 3200,
            MinWidth: 800
        };
    });

    it("codeditor widget initializes correctly", () => {
        var drawingSurface = $("<div></div>");
        var editor = drawingSurface.codeeditor({
            text: 'max(var(LET-23), var(lin-3), const(3))',
            language: 'bma.targetfunc',
            suggestVariables: ['LET-23', 'lin-3']
        });
        expect(editor).toBeDefined();
    });

    it("should be created from BioModel, Layout and ISVGPlot driver instance", () => {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var containerEditorDriver = new BMA.UIDrivers.ContainerEditorDriver($("<div></div>"));
        var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
        var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
        var contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver($("#drawingSurceContainer"));
        var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
        var exportservice = new BMA.UIDrivers.ExportService();

        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, containerEditorDriver, contextMenuDriver, exportservice, undefined);
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
        var navigationDriver = new BMA.Test.NavigationTestDriver();
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var containerEditorDriver = new BMA.UIDrivers.ContainerEditorDriver($("<div></div>"));
        var exportservice = new BMA.UIDrivers.ExportService();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
        var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
        var contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver($("#drawingSurceContainer"));
        var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, navigationDriver, elementPanel, variableEditorDriver, containerEditorDriver, contextMenuDriver, exportservice, undefined);
        expect(drawingSurfacePresenter).toBeDefined();
    });

    it("turns navigation on executing 'AddElementSelect' command", () => {
        var appModel = new BMA.Model.AppModel();
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var svgPlotDriver = new BMA.Test.TestSVGPlotDriver(drawingSurface);
        var elementPanel = new BMA.Test.TestElementsPanel();
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var containerEditorDriver = new BMA.UIDrivers.ContainerEditorDriver($("<div></div>"));
        var navigationDriver = new BMA.Test.NavigationTestDriver();
        var exportservice = new BMA.UIDrivers.ExportService();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, testbutton, testbutton);
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, navigationDriver, elementPanel, variableEditorDriver, containerEditorDriver, undefined, exportservice, undefined);

        spyOn(navigationDriver, "TurnNavigation");
        window.Commands.Execute("AddElementSelect", undefined);
        expect(navigationDriver.TurnNavigation).toHaveBeenCalledWith(true);

        window.Commands.Execute("AddElementSelect", "Container");
        expect(navigationDriver.TurnNavigation).toHaveBeenCalledWith(false);

    });


    it("should initialize the variableEditorDriver", () => {
        var drawingSurface = $("<div></div>");
        drawingSurface.drawingsurface();
        var appModel = new BMA.Model.AppModel();
        var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("<div></div>"));
        var containerEditorDriver = new BMA.UIDrivers.ContainerEditorDriver($("<div></div>"));
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, testbutton, testbutton);
        var exportservice = new BMA.UIDrivers.ExportService();
        var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, containerEditorDriver, undefined, exportservice, undefined);

        expect(appModel.BioModel.Variables.length).toEqual(0);
        window.Commands.Execute("AddElementSelect", "Constant");
        
        window.Commands.Execute("DrawingSurfaceClick", { x: 150, y: 250, screenX: 1, screenY: 2 });
        expect(appModel.BioModel.Variables.length).toEqual(1);

        window.Commands.Execute("AddElementSelect", undefined);
        spyOn(variableEditorDriver, "Initialize");
        window.Commands.Execute("DrawingSurfaceClick", { x: 150, y: 250, screenX: 1, screenY: 2 });
        expect(variableEditorDriver.Initialize).toHaveBeenCalled();
    });


    it("creates drawingsurface widget", () => {
        var ds = $("<div id='drawingsurface'></div>");
        ds.drawingsurface();
        expect(ds).toBeDefined();
    });

});