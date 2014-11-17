describe("containernameeditor", () => {


    var widget = $('<div></div>');
    window.Commands = new BMA.CommandRegistry();

    //beforeEach(() => {
    //    widget.containernameeditor();
    //})

    //afterEach(() => {
    //    widget.containernameeditor("destroy");
    //})



    it("creates an input with default name", () => {
        widget.containernameeditor();
        expect(widget.children("input").eq(0).val()).toEqual("name");
    })

    it("changes name with setting option", () => {
        widget.containernameeditor();
        var testname = "test";
        widget.containernameeditor({ name: "test" });
        expect(widget.children("input").eq(0).val()).toEqual(testname);
        testname = "another";
        widget.containernameeditor("option", "name", testname);
        expect(widget.children("input").eq(0).val()).toEqual(testname);
    })

    it("changes name option when input edited", () => {
        var text = "changename";
        widget.containernameeditor();
        widget.children("input").eq(0).val(text).change();
        expect(widget.containernameeditor("option", "name")).toEqual(text);
    })

    it("creates a command when input edited", () => {
        spyOn(window.Commands, "Execute");
        var text = "changename";
        widget.containernameeditor();
        widget.children("input").eq(0).val(text).change();
        expect(window.Commands.Execute).toHaveBeenCalledWith("ContainerNameEdited", {})
    })

    it("don't create a command when setting name as option", () => {
        spyOn(window.Commands, "Execute");
        widget.containernameeditor();
        widget.containernameeditor("option", "name", "testme");
        expect(window.Commands.Execute).not.toHaveBeenCalled();
    })
}) 