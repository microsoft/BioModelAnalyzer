
describe("Simulation Expanded", () => {

    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    window.Commands = new BMA.CommandRegistry();
    var widget = $('<div></div>');

    afterEach(() => {
        widget.simulationexpanded();
        widget.simulationexpanded("destroy");
    })

    it("should create Run button and execute 'RunSimulation' command on click", () => {
        widget.simulationexpanded();
        spyOn(window.Commands, "Execute");
        widget.children().eq(0).click();
        expect(window.Commands.Execute).toHaveBeenCalled();
    })

    it("should create widget with variables and append coloredtablewiever", () => {
        var variables = [];
        variables[0] = ["#FFFFFF", false, "var1", "3-15"];
        widget.simulationexpanded({ variables: variables });
        var variablesTable = widget.children().eq(1).children().eq(0);
        expect(variablesTable.coloredtableviewer("option", "header")).toEqual(["Graph", "Name", "Range"]);
        expect(variablesTable.coloredtableviewer("option", "type")).toEqual("graph-max");
        expect(variablesTable.coloredtableviewer("option", "numericData")).toEqual(variables);
        expect(variablesTable.find("tr").length).toEqual(2 + variables.length);
    })

    it("should apppend progression table with 'interval','init','data' options", () => {
        var variables = [];
        variables[0] = ["#FFFFFF", false, "var1", "3-15"];
        variables[1] = ["#FFFFFF", true, "var2", "0-3"];
        var interval = [];
        interval[0] = [3, 15];
        interval[1] = [0, 3];
        var init = [4, 2];
        var data = [];
        data[0] = [4, 3];
        data[1] = [5, 2];
        data[3] = [4, 1];

        widget.simulationexpanded({ variables: variables, interval: interval, init: init, data: data });

        var progressionTable = widget.children().eq(1).children().eq(1);
        expect(progressionTable.progressiontable("option", "interval")).toEqual(interval);
        expect(progressionTable.progressiontable("option", "init")).toEqual(init);
        expect(progressionTable.progressiontable("option", "data")).toEqual(data);
    })

    it("should execute 'RunSimulation' command with right parameters", () => {
        var variables = [];
        variables[0] = ["#FFFFFF", false, "var1", "3-15"];
        variables[1] = ["#FFFFFF", true, "var2", "0-3"];
        var interval = [];
        interval[0] = [3, 15];
        interval[1] = [0, 3];
        var init = [4, 2];
        var data = [];
        data[0] = [4, 3];
        data[1] = [5, 2];
        data[3] = [4, 1];

        widget.simulationexpanded({ variables: variables, interval: interval, init: init, data: data });
        var num = widget.simulationexpanded("option", "num");
        var run = widget.children().eq(0);
        spyOn(window.Commands, "Execute");
        run.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("RunSimulation", {data: init, num: num});
    })

    it("should initially set num option", () => {
        widget.simulationexpanded();
        var span = widget.children(":last-child").children().eq(0);
        expect(span.text()).toEqual('10');
        expect(span.text()).toEqual(widget.simulationexpanded("option", "num").toString());

        widget.simulationexpanded({num: 15});
        expect(span.text()).toEqual('15');
    })

    it("should change num option", () => {
        widget.simulationexpanded();
        var span = widget.children(":last-child").children().eq(0);
        var inc = widget.children(":last-child").children("button").eq(0);
        var dec = widget.children(":last-child").children("button").eq(1);
        var initValue = parseInt(span.text());

        inc.click();
        expect(widget.simulationexpanded("option", "num")).toEqual(initValue + 10);
        expect(span.text()).toEqual((initValue + 10).toString());

        dec.click();
        dec.click();
        expect(widget.simulationexpanded("option", "num")).toEqual(initValue - 10);
        expect(span.text()).toEqual((initValue - 10).toString());
    })
})