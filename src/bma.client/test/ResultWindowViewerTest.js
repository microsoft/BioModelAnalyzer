describe("ResultsWindowViewer", function () {
    var widget = $('<div></div>');

    afterEach(function () {
        widget.resultswindowviewer("destroy");
    });

    beforeEach(function () {
        window.Commands = new BMA.CommandRegistry();
        widget.resultswindowviewer();
    });

    it("sets widget header, icon and content", function () {
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "max" });
        expect(widget.children().eq(0).text()).toEqual(header);
        expect(widget.children().eq(1).children().eq(0).attr("id")).toEqual("Test");
    });

    it("creates Expand command when icon is max and click a button", function () {
        spyOn(window.Commands, "Execute");
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "max" });
        var button = widget.resultswindowviewer("getbutton");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("Expand", header);
    });

    it("creates Collapse command when icon is min and click a button", function () {
        spyOn(window.Commands, "Execute");
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "min" });
        var button = widget.resultswindowviewer("getbutton");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("Collapse", header);
    });
});
//# sourceMappingURL=ResultWindowViewerTest.js.map
