describe("SimultaionPlotViewer", () => {

    var widget = $('<div></div>');

    it("creates a widget", () => {
        widget.simulationplotviewer();
    })

    it("creates widget with data", () => {
        var data = [];
        data[0] = [1, 1, 1];
        widget.simulationplotviewer({ data: data });
        expect(widget.simulationplotviewer("option", "data")).toEqual(data);
    })
}) 