

describe("ProofResultViewer", () => {

    var widget = $('<div></div>');

    beforeEach(() => {
        widget.proofresultviewer();
    })

    afterEach(() => {
        widget.proofresultviewer("destroy");
    })

    it("creates widget", () => {
        widget.proofresultviewer();
    })

    it("should stabilizing", () => {
        var issucceeded = true;
        var msg = 'Test Message';
        widget.proofresultviewer({ issucceeded: issucceeded, message: msg });
        var success = widget.find(".stabilize-prooved");
        expect(success.text()).toEqual("Stabilizes");
        var p = widget.find("p").eq(0);
        expect(p.text()).toEqual(msg);
    })

    it("should fail to stabilize", () => {
        var issucceeded = false;
        var msg = 'Test Message';
        widget.proofresultviewer({ issucceeded: issucceeded, message: msg });
        var success = widget.find(".stabilize-failed");
        expect(success.text()).toEqual("Failed to Stabilize");
        var p = widget.find("p").eq(0);
        expect(p.text()).toEqual(msg);
    })

    it("should set data", () => {
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];
        var issucceeded = true;
        var time = 17;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time, data: { numericData: numericData, colorData: colorData} });
    })

    it("should create resultswindowviewer for variables table", () => {
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];
        var issucceeded = true;
        var time = 17;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time, data: { numericData: numericData, colorData: colorData } });

        var variablesdiv = widget.children().filter("div").eq(1);
        expect(variablesdiv.resultswindowviewer("option", "content").coloredtableviewer("option", "numericData")).toEqual(numericData);

        var proofdiv = widget.children().filter("div").eq(2);
        expect(proofdiv.resultswindowviewer("option", "content").coloredtableviewer("option", "colorData")).toEqual(colorData);
    })

    it("should create only variables table without colorData", () => {
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var issucceeded = true;
        var time = 17;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time, data: { numericData: numericData } });
        expect(widget.children().eq(3).length).toEqual(1);
    })

    it("should hide tabs", () => {
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];
        var issucceeded = true;
        var time = 17;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time, data: { numericData: numericData, colorData: colorData } });

        var variablesdiv = widget.children().filter("div").eq(1);
        var proofdiv = widget.children().filter("div").eq(2);

        widget.proofresultviewer("hide", "ProofVariables");
        expect(variablesdiv.css("display")).toEqual("none");
        expect(proofdiv.css("display")).not.toEqual("none");

        widget.proofresultviewer("hide", "ProofPropagation");
        expect(proofdiv.css("display")).toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");

        widget.proofresultviewer("show", "ProofPropagation");
        expect(proofdiv.css("display")).not.toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");

        variablesdiv.hide();

        widget.proofresultviewer("show", "ProofVariables");
        expect(proofdiv.css("display")).not.toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");
    });
})