describe("VariablesEditor", () => {
    var editor: JQuery;

    beforeEach(() => {
        window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
        window.Commands = new BMA.CommandRegistry();
        editor = $("<div></div>");
        editor.bmaeditor();
    });

    afterEach(() => {
        editor.bmaeditor("destroy");
    });

    it("should change a name", () => {
        var value = "testname";
        var nameinput = editor.find("input").eq(0);
        editor.bmaeditor("option", "name", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a name-2", () => {
        var value = "testme";
        var nameinput = editor.find("input").eq(0);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "name")).toEqual(value);
    });
    
    it("should change a rangeFrom", () => {
        var value = "178";
        var nameinput = editor.find("input").eq(1);
        editor.bmaeditor("option", "rangeFrom", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeFrom-2", () => {
        var value = "16";
        var nameinput = editor.find("input").eq(1);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(value);
    });

    it("should change a rangeTo", () => {
        var value = "178";
        var nameinput = editor.find("input").eq(2);
        editor.bmaeditor("option", "rangeTo", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeTo-2", () => {
        var value = "16";
        var nameinput = editor.find("input").eq(2);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(value);
    });

    it("should change a formula", () => {
        var value = "var(f)*15";
        var nameinput = editor.find("textarea")
        editor.bmaeditor("option", "formula", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a formula-2", () => {
        var value = "min(4,77)-199";
        var nameinput = editor.find("textarea")
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "formula")).toEqual(value);
    });

})   