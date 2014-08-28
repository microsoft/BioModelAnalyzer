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

    xit("should change a name", function () {
        var value = "testname";
        var nameinput = editor.find("input").eq(0);
        editor.bmaeditor("option", "name", value);
        expect(nameinput.val()).toEqual(value);
    });

    xit("should change a name-2", function () {
        var value = "testme";
        var nameinput = editor.find("input").eq(0);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "name")).toEqual(value);
    });

    xit("should change a rangeFrom", function () {
        var value = "178";
        var nameinput = editor.find("input").eq(1);
        editor.bmaeditor("option", "rangeFrom", value);
        expect(nameinput.val()).toEqual(value);
    });

    xit("should change a rangeFrom-2", function () {
        var value = "16";
        var nameinput = editor.find("input").eq(1);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(value);
    });

    xit("should change a rangeTo", function () {
        var value = "178";
        var nameinput = editor.find("input").eq(2);
        editor.bmaeditor("option", "rangeTo", value);
        expect(nameinput.val()).toEqual(value);
    });

    xit("should change a rangeTo-2", function () {
        var value = "16";
        var nameinput = editor.find("input").eq(2);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(value);
    });

    xit("should change a formula", function () {
        var value = "var(f)*15";
        var nameinput = editor.find("textarea");
        editor.bmaeditor("option", "formula", value);
        expect(nameinput.val()).toEqual(value);
    });

    xit("should change a formula-2", function () {
        var value = "min(4,77)-199";
        var nameinput = editor.find("textarea");
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "formula")).toEqual(value);
    });

    xit("should set options", function () {
        var neweditor = $('<div></div>');

        //neweditor.bmaeditor({ functions: ["fight", "rebel", "riot"] });
        neweditor.bmaeditor({ name: "noname", rangeFrom: 6, rangeTo: 10, formula: "123-ceil(x)", approved: false });
        expect(neweditor.bmaeditor("option", "name")).toEqual("noname");
        expect(neweditor.bmaeditor("option", "rangeFrom")).toEqual(6);
        expect(neweditor.bmaeditor("option", "rangeTo")).toEqual(10);
        expect(neweditor.bmaeditor("option", "formula")).toEqual("123-ceil(x)");
        expect(neweditor.bmaeditor("option", "approved")).toBeFalsy();
        var funs = neweditor.find(".formula-not-validated");
        expect(funs.length).toEqual(1);

        neweditor.bmaeditor("option", "approved", true);
        funs = neweditor.find(".formula-validated");
        expect(funs.length).toEqual(1);

        neweditor.bmaeditor({ approved: false });
        funs = neweditor.find(".formula-not-validated");
        expect(funs.length).toEqual(1);
    });

    it("should change functions option", function () {
        //var funs = editor.bmaeditor("option", "functions");//find(".labels-for-functions");
        //expect(funs.length).toEqual(11);
        //var labels = editor.find(".label-for-functions");
        //expect(labels.length).toEqual(11);
        var edd = $('<div></div>');

        //edd.bmaeditor({ functions: ["fight"]});
        edd.bmaeditor();
    });
});
