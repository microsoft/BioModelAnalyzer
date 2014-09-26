describe("SimultaionPlotViewer", function () {
    var widget = $('<div></div>');

    afterEach(function () {
        widget.simulationplot("destroy");
    });

    it("creates a widget", function () {
        widget.simulationplot();
    });

    it("creates widget with data", function () {
        var data = [];
        data[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        widget.simulationplot({ colors: data });
        expect(widget.simulationplot("option", "colors")).toEqual(data);
    });

    it("creates div with plot", function () {
        widget.simulationplot();
        expect(widget.children().eq(0).attr("data-idd-plot")).toEqual("plot");
    });

    it("creates gridline plot", function () {
        widget.simulationplot();
        expect(widget.children().eq(0).children().eq(0).attr("data-idd-plot")).toEqual("scalableGridLines");
    });

    it("don't creates polylines without data", function () {
        widget.simulationplot();
        expect(widget.children().eq(0).children().length).toEqual(1);
    });

    it("should add polylines", function () {
        var data = [];
        data[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        data[1] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        widget.simulationplot({ colors: data });
        expect(widget.children().eq(0).children().length).toEqual(1 + data.length);
    });

    it("should updata polylines after setting another data", function () {
        var data = [];
        data[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        data[1] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        widget.simulationplot({ colors: data });

        var data2 = [];
        data2[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        widget.simulationplot({ colors: data2 });
        expect(widget.children().eq(0).children().length).toEqual(1 + data2.length);
    });

    it("should set proper options for polylines", function () {
        var data = [];
        data[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        data[1] = {
            Id: 3,
            Color: "#CCCCCC",
            Seen: false,
            Plot: [4, 8, 2]
        };
        widget.simulationplot({ colors: data });
        var plot = widget.simulationplot("getPlot");
        //for (var i = 0; i < data.length; i++) {
        //    //var y = data[i].Plot;
        //    var polyline = plot.get(widget.children().eq(0).children().eq(i + 1));
        //    expect(polyline.stroke).toEqual(data[i].Color);
        //    expect(polyline.isVisible).toEqual(data[i].Seen);
        //}
    });
});
//# sourceMappingURL=SimulationPlotViewerTest.js.map
