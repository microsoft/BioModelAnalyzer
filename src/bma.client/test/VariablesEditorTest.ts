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

    it("should change a name in <input> when widget option changed", () => {
        var value = "testname";
        var nameinput = editor.find("input").eq(0);
        editor.bmaeditor("option", "name", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a name option in widget when it was inputed by user", () => {
        var value = "testme";
        var nameinput = editor.find("input").eq(0);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "name")).toEqual(value);
    });
    
    it("should change a rangeFrom in <input> when widget option changed", () => {
        var value = "17";
        var nameinput = editor.find("input").eq(1);
        editor.bmaeditor("option", "rangeFrom", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeFrom option in widget when it was inputed by user", () => {
        var value = "16";
        var nameinput = editor.find("input").eq(1);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(value);
    });

    it("should change a rangeTo in <input> when widget option changed", () => {
        var value = "17";
        var nameinput = editor.find("input").eq(2);
        editor.bmaeditor("option", "rangeTo", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a rangeTo option in widget when it was inputed by user", () => {
        var value = "16";
        var nameinput = editor.find("input").eq(2);
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(value);
    });

    it("should change a formula in <input> when widget option changed", () => {
        var value = "var(f)*15";
        var nameinput = editor.find("textarea")
        editor.bmaeditor("option", "formula", value);
        expect(nameinput.val()).toEqual(value);
    });

    it("should change a formula option in widget when it was inputed by user", () => {
        var value = "min(4,77)-199";
        var nameinput = editor.find("textarea")
        nameinput.val(value).change();
        expect(editor.bmaeditor("option", "formula")).toEqual(value);
    });

    it("should set options", () => {
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

    it("should change functions option", () => {
        var funs = editor.bmaeditor("option", "functions");//find(".labels-for-functions");
        expect(funs.length).toEqual(11);

        var labels = editor.find(".label-for-functions");
        expect(labels.length).toEqual(11);

        var edd = $('<div></div>');
        var arr = ["ceil", "plus", "minus"];
        edd.bmaeditor({ functions: arr});
        labels = edd.find(".label-for-functions");
        expect(labels.length).toEqual(3);

        for (var i = 0; i < arr.length; i++) {
            expect(labels.eq(i).text()).toEqual(arr[i]);
        }

    });

    it("should throw error when functions are not registered", () => {
        var edd = $('<div></div>');
        try {
            expect(edd.bmaeditor({ functions: ["myownfunction"] })).toThrow();
        }
        catch (e) { };
    });

    it("should increase rangeFrom value when clicked triangle-up and triangle-down", () => {
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(0);
        var init = 48;
        var rangeFrominput = editor.find("input").eq(1);
        var rangeToinput = editor.find("input").eq(2);

        editor.bmaeditor("option", "rangeFrom", init);
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init);
        
        editor.find(".triangle-up").eq(0).click();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init+1);
        expect(rangeFrominput.val()).toEqual((init + 1).toString());

        editor.find(".triangle-down").eq(0).click();
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init);
        expect(rangeFrominput.val()).toEqual(init.toString());

        init = 69;

        editor.bmaeditor("option", "rangeTo", init);
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(init);

        editor.find(".triangle-up").eq(1).click();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(init + 1);
        expect(rangeToinput.val()).toEqual((init + 1).toString());

        editor.find(".triangle-down").eq(1).click();
        expect(editor.bmaeditor("option", "rangeTo")).toEqual(init);
        expect(rangeToinput.val()).toEqual(init.toString());
    });

    it("should not allow to set rangeFrom not from interval [0,100]", () => {
        editor.bmaeditor("option", "rangeFrom", 234);
        expect(editor.bmaeditor("option", "rangeFrom")).toEqual(100);
    });

    it("should set inputs", () => {
        var inputslist = editor.find(".inputs-list-content");
        expect(inputslist.length).toEqual(1);
        expect(inputslist.children().length).toEqual(4);
        var arr = ["fight", "rebel", "riot"]
        editor.bmaeditor("option", "inputs", arr);
        expect(inputslist.children().length).toEqual(3);

        for (var i = 0; i < arr.length; i++) {
            expect(inputslist.children().eq(i).text()).toEqual(arr[i]);
        }
    });

    it("should input formula correctly after choosing function and variable from list", () => {
        var functions = editor.find(".label-for-functions");
        var inputs = editor.find(".inputs-list-content").children();

        functions.eq(0).click();
        inputs.eq(1).click();

        //expect
    });

    it("should create a variableeditorchanged command", () => {
        spyOn(window.Commands, "Execute");
        editor.bmaeditor("option", "name", "test");
        expect(window.Commands.Execute).toHaveBeenCalledWith("variableeditorchanged", {});
    });

    it("should not create a variableeditorchanged command", () => {
        spyOn(window.Commands, "Execute");
        editor.bmaeditor("initialize", {name: "myname"});
        expect(window.Commands.Execute).not.toHaveBeenCalled();
    });

})   