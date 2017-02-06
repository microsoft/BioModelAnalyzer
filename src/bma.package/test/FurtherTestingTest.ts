// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("Further Testing", () => {

    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    window.Commands = new BMA.CommandRegistry();
    var editor = $("<div></div>");

    

    describe("editor.furthertesting()", () => {


        beforeEach(() => {
            editor.furthertesting();
        })
        afterEach(() => {
            editor.furthertesting();
            editor.furthertesting("destroy");
        }) 

        it("creates a button and executes 'FurtherTestingRequested' command on click", () => {
            var button = editor.children().eq(0);
            spyOn(window.Commands, "Execute");
            button.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith("FurtherTestingRequested", {});
        });

        it("can get button from widget with calling 'GetToggler' function", () => {
            var button = editor.furthertesting("GetToggler");
            spyOn(window.Commands, "Execute");
            button.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith("FurtherTestingRequested", {});
        });

        it("hides toggler", () => {
            editor.furthertesting("HideStartToggler");
            var button = editor.furthertesting("GetToggler");
            expect(button.css("display")).toEqual("none");
        })

        it("shows toggler", () => {
            editor.furthertesting("HideStartToggler");
            editor.furthertesting("ShowStartToggler");
            var button = editor.furthertesting("GetToggler");
            expect(button.css("display")).not.toEqual("none");
        })
    })

    describe("editor.furthertesting() with options", () => {
        var data = [];
        data[0] = [];
        data[0][0] = ["q", "w", "e", "q, w, e, r, t, y"];
        data[0][1] = ["a", "s", "d", "a, s, d, f, g, h"];
        data[0][2] = ["z", "x", "c", "z, x, c, v, b, n"];

        data[1] = [];
        data[1][0] = ["one", "84, 45, 96"];
        data[1][1] = ["frG", "71, 39, 64"];
        var header = [];
        header[0] = ["Cell", "Name", "Calculated Bound", "Oscillation"];
        header[1] = ["Name", "Values"];
        var labels = [];
        labels[0] = $('<div></div>').addClass('bma-futhertesting-oscillations-icon');
        labels[1] = $('<div></div>').addClass('bma-futhertesting-bifurcations-icon');

        beforeEach(() => {
            editor.furthertesting({
                data: data,
                tableHeaders: header,
                tabLabels: labels
            });
        });

        afterEach(() => {
            editor.furthertesting();
            editor.furthertesting("destroy");
        });

        it("creates resultswindowviewer inside", () => {
            var results = editor.children().eq(1);
            expect(results.resultswindowviewer("option", "content")).toBeDefined();
        });

        it("creates JQueryUI tabs() inside resultswindowviewer", () => {
            var results = editor.children().eq(1);
            var tabs = results.children().eq(1);
            expect(tabs.tabs("option", "active")).toBeDefined();
        });

        it("creates coloredtableviewers inside tabs widget with right data and headers", () => {
            var results = editor.children().eq(1);
            var tabs = results.children().eq(1);
            var tab0 = tabs.children("div").eq(0);
            var tab1 = tabs.children("div").eq(1);

            expect(tab0.coloredtableviewer("option", "numericData")).toEqual(data[0]);
            expect(tab1.coloredtableviewer("option", "numericData")).toEqual(data[1]);

            expect(tab0.coloredtableviewer("option", "header")).toEqual(header[0]);
            expect(tab1.coloredtableviewer("option", "header")).toEqual(header[1]);
        });

        it("should put labels to tabs", () => {
            var results = editor.children().eq(1);
            var tabs = results.children().eq(1);
            var ul = tabs.children("ul");
            var lab0 = ul.children("li").eq(0).children().eq(0);
            var lab1 = ul.children("li").eq(1).children().eq(0);
            expect(lab0.length).toEqual(1);
            expect(lab1.length).toEqual(1);
            expect(lab0[0].innerHTML).toEqual(labels[0][0].outerHTML);
            expect(lab1[0].innerHTML).toEqual(labels[1][0].outerHTML);
        });

        it("destroys table with results when 'data' option is undefined", () => {
            editor.furthertesting({ data: undefined });
            expect(editor.children().eq(1).children().length).toEqual(0);
        });
    })
}) 
