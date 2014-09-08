

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
        var time = 23;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time });
        var success = widget.find("h3").eq(0);
        expect(success.text()).toEqual("Stabilizes");
        var p = widget.find("p").eq(0);
        expect(p.text()).toEqual('BMA succeeded in checking every possible state of the model in ' + time + ' seconds. After stepping through separate interactions, the model eventually reached a single stable state.');
    })

    it("should fail to stabilize", () => {
        var issucceeded = false;
        var time = 55;
        widget.proofresultviewer({ issucceeded: issucceeded, time: time });
        var success = widget.find("h3").eq(0);
        expect(success.text()).toEqual("Failed to Stabilize");
        var p = widget.find("p").eq(0);
        expect(p.text()).toEqual('After stepping through separate interactions in the model, the analisys failed to determine a final stable state');
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

        widget.proofresultviewer("hide", "Variables");
        expect(variablesdiv.css("display")).toEqual("none");
        expect(proofdiv.css("display")).not.toEqual("none");

        widget.proofresultviewer("hide", "Proof Propagation");
        expect(proofdiv.css("display")).toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");

        widget.proofresultviewer("show", "Proof Propagation");
        expect(proofdiv.css("display")).not.toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");

        variablesdiv.hide();

        widget.proofresultviewer("show", "Variables");
        expect(proofdiv.css("display")).not.toEqual("none");
        expect(variablesdiv.css("display")).not.toEqual("none");
    });
})