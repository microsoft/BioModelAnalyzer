// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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
        widget.find('.green').children().eq(0).click();
        expect(window.Commands.Execute).toHaveBeenCalled();
    })

    it("should create widget with variables and append coloredtablewiever", () => {
        var variables = [];
        variables[0] = ["#FFFFFF", false, "var1", "3-15"];
        widget.simulationexpanded({ variables: variables });
        var variablesTable = widget.find('.small-simulation-popout-table');
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

        var progressionTable = widget.find('.big-simulation-popout-table');
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
        var run = widget.find('.green').children().eq(0);
        spyOn(window.Commands, "Execute");
        run.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("RunSimulation", {data: init, num: num});
    })

    it("should initially set num option", () => {
        widget.simulationexpanded();
        var span = widget.find(".steps");
        expect(span.text()).toEqual('STEPS: 10');
        expect(span.text()).toEqual('STEPS: ' + widget.simulationexpanded("option", "num").toString());

        widget.simulationexpanded({num: 15});
        expect(span.text()).toEqual('STEPS: 15');
    })

    it("should change num option", () => {
        widget.simulationexpanded();
        var span = widget.find(".steps");
        var dec = span.prev().children(':first-child');
        var inc = span.next().children(':first-child');
        var initValue = parseInt(span.text().split(' ')[1]);

        inc.click();
        expect(widget.simulationexpanded("option", "num")).toEqual(initValue + 10);
        expect(span.text()).toEqual('STEPS: '+ (initValue + 10).toString());

        dec.click();
        dec.click();
        expect(widget.simulationexpanded("option", "num")).toEqual(initValue - 10);
        expect(span.text()).toEqual('STEPS: ' + (initValue - 10).toString());
    })

    it("should add Randomise button",() => {
        widget.simulationexpanded();
        var randomise = widget.find('.randomise-button');
        expect(randomise.children().length).toEqual(2);
        expect(randomise.children(".bma-random-icon2").index()).toEqual(0);
        expect(randomise.children().eq(1).text()).toEqual("Randomise");
    })
})
