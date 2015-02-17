describe("Simulation Viewer", () => {

    var div = $('<div></div>');
    
    var variables = [];
    variables[0] = ["q", "w", "e", "r"];
    variables[1] = ["a", "s", "d", "f"];
    variables[2] = ["z", "x", "c", "v"];

    var colorData = [];
    colorData[0] = [true, true, false];
    colorData[1] = [true, false, false];
    colorData[2] = [false, true, false];
    colorData[3] = [true, true, true];

    var data = {
        variables: variables,
        colorData: colorData
    }

    var plot = [];
    plot.push({
        Id: 3,
        Color: "#ADFECC",
        Seen: true,
        Plot: []
    });
    plot.push({
        Id: 1,
        Color: "#FFAECA",
        Seen: false,
        Plot: []
    });

    afterEach(() => {
        div.simulationviewer();
        div.simulationviewer("destroy");
    });

    it("creates widget with options", () => {
        div.simulationviewer({ data: data, plot: plot });
        expect(div.simulationviewer("option", "data")).toEqual(data);
        expect(div.simulationviewer("option", "plot")).toEqual(plot);
    })

    it("creates 2 resultswindowviewer widgets inside", () => {
        div.simulationviewer({ data: data, plot: plot });
        var r0: JQuery = div.children().eq(0);
        var r1: JQuery = div.children().eq(1);

        expect(r0.resultswindowviewer("option", "header")).toEqual("Variables");
        expect(r0.resultswindowviewer("option", "icon")).toEqual("max");
        expect(r0.resultswindowviewer("option", "tabid")).toEqual("SimulationVariables");

        expect(r1.resultswindowviewer("option", "icon")).toEqual("max");
        expect(r1.resultswindowviewer("option", "tabid")).toEqual("SimulationPlot");
    });

    it("creates 2 coloredtableviewer widgets inside the 1st resultswindowviewer", () => {
        div.simulationviewer({ data: data, plot: plot });
        var r0: JQuery = div.children().eq(0).resultswindowviewer("option", "content");

        var c0 = r0.children().eq(0);
        expect(c0.coloredtableviewer("option", "numericData")).toEqual(data.variables);
        expect(c0.coloredtableviewer("option", "header")).toEqual(["Graph", "Cell", "Name", "Range"]);
        expect(c0.coloredtableviewer("option", "type")).toEqual("graph-min");

        var c1 = r0.children().eq(1);
        expect(c1.coloredtableviewer("option", "colorData")).toEqual(data.colorData);
        expect(c1.coloredtableviewer("option", "type")).toEqual("simulation-min");
    });

    it("creates simulationplot widget inside the 2nd resultswindowviewer", () => {
        div.simulationviewer({ data: data, plot: plot });
        var r1: JQuery = div.children().eq(1).resultswindowviewer("option", "content");
    });

    it("creates only plot when data.variables is undefined", () => {
        data.variables = undefined;
        div.simulationviewer({ data: data, plot: plot });
        var c0: JQuery = div.children().eq(0);
        var r1: JQuery = div.children().eq(1);
        expect(c0.children().length).toEqual(0);
        expect(c0[0].outerHTML).toEqual('<div class="simulation-variables"></div>');
        expect(r1.resultswindowviewer("option", "tabid")).toEqual("SimulationPlot");
        data.variables = variables;
    });

    it("creates variables table and plot when colorData is undefined", () => {
        data.colorData = undefined;
        div.simulationviewer({ data: data, plot: plot });
        var r0: JQuery = div.children().eq(0).resultswindowviewer("option", "content");
        var r1: JQuery = div.children().eq(1);

        var c0 = r0.children().eq(0);
        expect(c0.coloredtableviewer("option", "numericData")).toEqual(data.variables);
        expect(c0.coloredtableviewer("option", "header")).toEqual(["Graph", "Cell", "Name", "Range"]);
        expect(c0.coloredtableviewer("option", "type")).toEqual("graph-min");
        expect(r1.resultswindowviewer("option", "tabid")).toEqual("SimulationPlot");
        data.colorData = colorData;
    });

    it("doesn't create plot when this option is undefined", () => {
        div.simulationviewer({ data: data, plot: undefined });
        var r0: JQuery = div.children().eq(0);
        var r1: JQuery = div.children().eq(1);

        
        expect(r0.resultswindowviewer("option", "tabid")).toEqual("SimulationVariables");

        expect(r1.children().length).toEqual(0);
        expect(r1[0].outerHTML).toEqual("<div></div>");
    })
}) 