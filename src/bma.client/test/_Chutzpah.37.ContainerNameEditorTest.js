describe("containernameeditor", function () {
    var widget = $('<div></div>');

    //afterEach(() => {
    //    widget.containernameeditor("destroy");
    //})
    xit("creates a 'Container Name' label div", function () {
        widget.containernameeditor();
        expect(widget.children("div").eq(0).text()).toEqual("Container Name");
    });

    xit("creates an input with default name", function () {
        widget.containernameeditor();
        expect(widget.children("input").eq(0).val()).toEqual("name");
    });

    it("changes name with setting option", function () {
        var testname = "test";
        widget.containernameeditor({ name: testname });
        expect(widget.children("input").eq(0).val()).toEqual(testname);
    });
});
