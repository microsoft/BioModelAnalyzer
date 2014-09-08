describe("SimultaionPlotViewer", function () {
    var widget = $('<div></div>');

    it("creates a widget", function () {
        widget.simulationplot();
    });

    it("creates widget with data", function () {
        var data = [];
        data[0] = [1, 1, 1];
        widget.simulationplot({ data: data });
        expect(widget.simulationplot("option", "data")).toEqual(data);
    });

    it("creates div with plot", function () {
        expect(widget.children().eq(0).attr("data-idd-plot")).toEqual("polyline");
    });
});
