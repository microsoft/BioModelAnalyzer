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
        var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($());
        //var presenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, undefined, undefined);
        //expect(presenter).toBeDefined();

        window.Commands = new BMA.CommandRegistry();
        window.ElementRegistry = new BMA.Elements.ElementsRegistry();
        var testbutton = new BMA.Test.TestUndoRedoButton();
        var presenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, {
            GetDragSubject: function () {
                this.subscribe = function () { };
            }
        }, testbutton, testbutton, variableEditorDriver);
        expect(presenter).toBeDefined();
    });

    it("should create proper SVG for specified model and layout", () => {

    });

    it("creates drawingsurface widget", () => {
        var ds = $("<div id='DRAWINGSURFACE'></div>");
        ds.drawingsurface();
    });
});