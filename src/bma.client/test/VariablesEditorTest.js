describe("VariablesEditor", function () {
    var editor;

    beforeEach(function () {
        window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
        window.Commands = new BMA.CommandRegistry();
        editor = $("<div></div>");
        editor.bmaeditor();
    });

    afterEach(function () {
        editor.bmaeditor("destroy");
    });

    it("should change a name", function () {
        var value = "testname";
        var nameinput = editor.find("input").eq(0);
        editor.bmaeditor("option", "name", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a name-2", function () {
        var value = "testme";
        var nameinput = editor.find("input").eq(0);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "name")).toEqual(value);
    });

    it("should change a rangeFrom", function () {
        var value = "178";
        var nameinput = editor.find("input").eq(1);
        editor.bmaeditor("option", "rangeFrom", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeFrom-2", function () {
        var value = "16";
        var nameinput = editor.find("input").eq(1);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(value);
    });

    it("should change a rangeTo", function () {
        var value = "178";
        var nameinput = editor.find("input").eq(2);
        editor.bmaeditor("option", "rangeTo", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeTo-2", function () {
        var value = "16";
        var nameinput = editor.find("input").eq(2);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(value);
    });

    it("should change a formula", function () {
        var value = "var(f)*15";
        var nameinput = editor.find("textarea");
        editor.bmaeditor("option", "formula", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a formula-2", function () {
        var value = "min(4,77)-199";
        var nameinput = editor.find("textarea");
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "formula")).toEqual(value);
    });
});
//# sourceMappingURL=VariablesEditorTest.js.map
