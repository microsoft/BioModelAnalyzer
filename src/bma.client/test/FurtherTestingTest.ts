
describe("Further Testing", () => {

    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    window.Commands = new BMA.CommandRegistry();
    var editor = $("<div></div>");

    afterEach(() => {
        editor.furthertesting();
        editor.furthertesting("destroy");
    }) 

    it("creates a button and executes 'FurtherTestingRequested' command on click", () => {
        editor.furthertesting();
        var button = editor.children().eq(0);
        spyOn(window.Commands, "Execute");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("FurtherTestingRequested", {});
    })

    it("can get button from widget with calling 'GetToggler' function", () => {
        editor.furthertesting();
        var button = editor.furthertesting("GetToggler");
        spyOn(window.Commands, "Execute");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("FurtherTestingRequested", {});
    })

    it("creates table in widget with data option", () => {
        editor.furthertesting();
        var data = ["q", "w", "e", "q, w, e, r, t, y"];
        editor.furthertesting({ data: data });
        var table = editor.children().eq(1).resultswindowviewer("option", "content");
        expect(table.coloredtableviewer("option", "numericData")).toEqual(data);
        expect(table.coloredtableviewer("option", "type")).toEqual("standart");
        expect(table.coloredtableviewer("option", "header")).toEqual(["Cell", "Name", "Calculated Bound", "Oscillation"]);
    })

    it("hide toggler", () => {
        editor.furthertesting();
        editor.furthertesting("HideStartToggler");
        var button = editor.furthertesting("GetToggler");
        expect(button.css("display")).toEqual("none");
    })

    it("show toggler", () => {
        editor.furthertesting();
        editor.furthertesting("HideStartToggler");
        editor.furthertesting("ShowStartToggler");
        var button = editor.furthertesting("GetToggler");
        expect(button.css("display")).not.toEqual("none");
    })

    it("destroy table with results when 'data' option is undefined", () => {
        editor.furthertesting();
        var data = ["q", "w", "e", "q, w, e, r, t, y"];
        editor.furthertesting({ data: data });
        editor.furthertesting({ data: undefined });
        expect(editor.children().eq(1).children().length).toEqual(0);
    })
}) 