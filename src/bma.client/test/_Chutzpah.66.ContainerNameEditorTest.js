describe("containernameeditor", function () {
    var widget = $('<div></div>');
    window.Commands = new BMA.CommandRegistry();

    //beforeEach(() => {
    //    widget.containernameeditor();
    //})
    //afterEach(() => {
    //    widget.containernameeditor("destroy");
    //})
    it("creates a 'Container Name' label div", function () {
        widget.containernameeditor();
        expect(widget.children("div").eq(0).text()).toEqual("Container Name");
    });

    it("creates an input with default name", function () {
        widget.containernameeditor();
        expect(widget.children("input").eq(0).val()).toEqual("name");
    });

    it("changes name with setting option", function () {
        widget.containernameeditor();
        var testname = "test";
        widget.containernameeditor({ name: "test" });
        expect(widget.children("input").eq(0).val()).toEqual(testname);
        testname = "another";
        widget.containernameeditor("option", "name", testname);
        expect(widget.children("input").eq(0).val()).toEqual(testname);
    });

    it("changes name option when input edited", function () {
        var text = "changename";
        widget.containernameeditor();
        widget.children("input").eq(0).val(text).change();
        expect(widget.containernameeditor("option", "name")).toEqual(text);
    });

    it("creates a command when input edited", function () {
        spyOn(window.Commands, "Execute");
        var text = "changename";
        widget.containernameeditor();
        widget.children("input").eq(0).val(text).change();
        expect(window.Commands.Execute).toHaveBeenCalledWith("ContainerNameEdited", {});
    });
});
