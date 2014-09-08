describe("SimultaionPlotViewer", function () {
    var widget = $('<div></div>');

    it("creates a widget", function () {
        widget.simulationplotviewer();
    });

    it("creates widget with data", function () {
        var data = [];
        data[0] = [1, 1, 1];
        widget.simulationplotviewer({ data: data });
        expect(widget.simulationplotviewer("option", "data")).toEqual(data);
    });
});
//# sourceMappingURL=SimulationPlotViewerTest.js.map
