
describe("ResultsWindowViewer", () => {
     
    var widget = $('<div></div>');
    window.Commands = new BMA.CommandRegistry();
    afterEach(() => {
        widget.resultswindowviewer("destroy");
    })

    beforeEach(() => {
        widget.resultswindowviewer();
    })

    it("sets widget header, icon and content",()=> {
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "max" });
        var headerspan = widget.children('div').eq(0).children('span').eq(0);
        expect(headerspan.text()).toEqual(header);
        var contentdiv = widget.children().eq(1);
        expect(contentdiv.attr("id")).toEqual("Test");
    })

    it("changes a header", () => {
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "max" });
        var headerspan = widget.children('div').eq(0).children('span').eq(0);
        expect(headerspan.text()).toEqual(header);

        header = "newHeader";
        widget.resultswindowviewer({
            header: header
        });
        headerspan = widget.children('div').eq(0).children('span').eq(0);
        expect(headerspan.text()).toEqual(header);
    })

    it("changes a content", () => {
        var header = "header";
        var content = $('<div id="Test"></div>');
        widget.resultswindowviewer({ header: header, content: content, icon: "max" });
        var contentDiv = widget.children().eq(1);
        expect(content.attr("id")).toEqual(contentDiv.attr("id"));

        var content2 = $('<div id="Test2"></div>');
        widget.resultswindowviewer({ content: content2 });
        var contentDiv2 = widget.children().eq(1);
        expect(content2.attr("id")).toEqual(contentDiv2.attr("id"));
    })


    it("creates Expand command when icon is max and click a button", () => {
        spyOn(window.Commands, "Execute");
        var header = "header";
        var content = $('<div id="Test"></div>');
        var tabid = "Testtabid";
        widget.resultswindowviewer({ header: header, content: content, icon: "max", tabid: tabid });
        var button = widget.resultswindowviewer("getbutton");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("Expand", tabid);
    })

    it("creates Collapse command when icon is min and click a button", () => {
        spyOn(window.Commands, "Execute");
        var header = "header";
        var content = $('<div id="Test"></div>');
        var tabid = "Testtabid";
        widget.resultswindowviewer({ header: header, content: content, icon: "min", tabid: tabid });
        var button = widget.resultswindowviewer("getbutton");
        button.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("Collapse", tabid);
    })

})