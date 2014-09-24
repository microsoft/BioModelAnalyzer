﻿declare var InteractiveDataDisplay: any;

describe("SimultaionPlotViewer", () => {

    var widget = $('<div></div>');

    afterEach(() => {
        widget.simulationplot("destroy");
    })

    it("creates a widget", () => {
        widget.simulationplot();
    })

    it("creates widget with data", () => {
        var data = [];
        data[0] = {
            Id: 3,
            Color: "#FFFFFF",
            Seen: true,
            Plot: [3, 5, 9]
        };
        widget.simulationplot({ colors: data });
        expect(widget.simulationplot("option", "colors")).toEqual(data);
    })

    it("creates div with plot", () => {
        widget.simulationplot();
        expect(widget.children().eq(0).attr("data-idd-plot")).toEqual("plot");
    })

    it("creates gridline plot", () => {
        widget.simulationplot();
        expect(widget.children().eq(0).children().eq(0).attr("data-idd-plot")).toEqual("scalableGridLines");
    })

    it("don't creates polylines without data", () => {
        widget.simulationplot();
        expect(widget.children().eq(0).children().length).toEqual(1);
    })

    it("should add polylines", () => {
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
    })

    it("should updata polylines after setting another data", () => {
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
    })

    it("should set proper options for polylines", () => {
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
    })

}) 