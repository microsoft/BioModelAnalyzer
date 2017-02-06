// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("VariablesEditor", () => {
    var editor: JQuery;
    //var require = { paths: { 'vs': '../node_modules/monaco-editor/min/vs' } };

    beforeEach(() => {
        window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
        window.Commands = new BMA.CommandRegistry();
        editor = $("<div></div>");
        editor.bmaeditor();
    });

    afterEach(() => {
        editor.bmaeditor();
        editor.bmaeditor("destroy");
    });
    
    //it("should change a name in <input> when widget option changed", () => {
    //    var value = "testname";
    //    var nameinput = editor.find("input").eq(0);
    //    editor.bmaeditor("option", "name", value);
    //    expect(nameinput.val()).toEqual(value);
    //});

    //it("should change a name option in widget when it was inputed by user", () => {
    //    var value = "testme";
    //    var nameinput = editor.find("input").eq(0);
    //    nameinput.val(value).change();
    //    expect(editor.bmaeditor("option", "name")).toEqual(value);
    //});
    
    //it("should change a rangeFrom in <input> when widget option changed", () => {
    //    var value = "17";
    //    var rangeFrom = editor.find("input").eq(1);
    //    editor.bmaeditor("option", "rangeFrom", value);
    //    expect(rangeFrom.val()).toEqual(value);
    //});

    //it("should change a rangeFrom option in widget when it was inputed by user", () => {
    //    var value = "16";
    //    var rangeFrom = editor.find("input").eq(1);
    //    rangeFrom.val(value).change();
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(value);
    //});

    //it("should change a rangeTo in <input> when widget option changed", () => {
    //    var value = "17";
    //    var rangeTo = editor.find("input").eq(2);
    //    editor.bmaeditor("option", "rangeTo", value);
    //    expect(rangeTo.val()).toEqual(value);
    //});

    //it("should change a rangeTo option in widget when it was inputed by user", () => {
    //    var value = "16";
    //    var rangeTo = editor.find("input").eq(2);
    //    rangeTo.val(value).change();
    //    expect(editor.bmaeditor("option", "rangeTo")).toEqual(value);
    //});

    //it("should change a formula in <input> when widget option changed", () => {
    //    var value = "var(f)*15";
    //    var formula = editor.find("textarea")
    //    editor.bmaeditor("option", "formula", value);
    //    expect(formula.val()).toEqual(value);
    //});

    //it("should change a formula option in widget when it was inputed by user", () => {
    //    var value = "min(4,77)-199";
    //    var formula = editor.find("textarea")
    //    formula.val(value).change();
    //    expect(editor.bmaeditor("getFormula")).toEqual(value);
    //});

    
    //it("should set options", () => {
    //    var neweditor = $('<div></div>');
    //    //neweditor.bmaeditor({ functions: ["fight", "rebel", "riot"] });
    //    neweditor.bmaeditor({ name: "noname", rangeFrom: 6, rangeTo: 10, formula: "123-ceil(x)", approved: false });
    //    expect(neweditor.bmaeditor("option", "name")).toEqual("noname");
    //    expect(neweditor.bmaeditor("option", "rangeFrom")).toEqual(6);
    //    expect(neweditor.bmaeditor("option", "rangeTo")).toEqual(10);
    //    expect(neweditor.bmaeditor("option", "formula")).toEqual("123-ceil(x)");
    //    expect(neweditor.bmaeditor("option", "approved")).toBeFalsy();
    //    var funs = neweditor.find(".formula-failed");
    //    //expect(funs.length).toEqual(1);

    //    neweditor.bmaeditor("SetValidation", true, "");
    //    funs = neweditor.find(".formula-validated-icon");
    //    expect(funs.length).toEqual(1);


    //    var error = "ErrorMessage";
    //    neweditor.bmaeditor("SetValidation", false, error);
    //    funs = neweditor.find(".formula-failed-icon");
    //    expect(funs.length).toEqual(1);
    //    //expect(neweditor.find("div.bma-formula-validation-message").text()).toEqual(error);
    //});
    

    //it("should throw error when functions are not registered", () => {
    //    var edd = $('<div></div>');
    //    expect(edd.bmaeditor.bind(this, { functions: ["myownfunction"] })).toThrow();
    //});

    //it("should increase rangeFrom value when clicked triangle-up and triangle-down", () => {
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(0);
    //    var init = 48;
    //    var rangeFrominput = editor.find("input").eq(1);
    //    var rangeToinput = editor.find("input").eq(2);

    //    editor.bmaeditor("option", "rangeFrom", init);
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init);
        
    //    editor.find(".triangle-up").eq(0).click();
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init+1);
    //    expect(rangeFrominput.val()).toEqual((init + 1).toString());

    //    editor.find(".triangle-down").eq(0).click();
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(init);
    //    expect(rangeFrominput.val()).toEqual(init.toString());

    //    init = 69;

    //    editor.bmaeditor("option", "rangeTo", init);
    //    expect(editor.bmaeditor("option", "rangeTo")).toEqual(init);

    //    editor.find(".triangle-up").eq(1).click();
    //    expect(editor.bmaeditor("option", "rangeTo")).toEqual(init + 1);
    //    expect(rangeToinput.val()).toEqual((init + 1).toString());

    //    editor.find(".triangle-down").eq(1).click();
    //    expect(editor.bmaeditor("option", "rangeTo")).toEqual(init);
    //    expect(rangeToinput.val()).toEqual(init.toString());
    //});

    //it("should not allow to set rangeFrom not from interval [0,100]", () => {
    //    editor.bmaeditor("option", "rangeFrom", 234);
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(100);
    //    editor.bmaeditor("option", "rangeFrom", -1);
    //    expect(editor.bmaeditor("option", "rangeFrom")).toEqual(0);
    //});

    //it("should set inputs", () => {
    //    var inputslist = editor.find(".inputs-list-content");
    //    expect(inputslist.length).toEqual(1);
    //    expect(inputslist.children().length).toEqual(0);
    //    var v1 = new BMA.Model.Variable(34, 15, BMA.Model.VariableTypes.Default, "one", 3, 7, "formula1");
    //    var v2 = new BMA.Model.Variable(38, 10, BMA.Model.VariableTypes.Constant, "two", 1, 14, "formula2");
    //    var v3 = new BMA.Model.Variable(39, 10, BMA.Model.VariableTypes.Constant, "three", 1, 14, "formula2");
    //    var arr = [v1, v2, v3];
    //    editor.bmaeditor("option", "inputs", arr);
    //    expect(inputslist.children().length).toEqual(3);

    //    for (var i = 0; i < arr.length; i++) {
    //        expect(inputslist.children().eq(i).text()).toEqual(arr[i].Name);
    //    }
    //});
    
    //describe("input functions", () => {
    //    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    //    window.Commands = new BMA.CommandRegistry();
    //    var editor = $("<div></div>").bmaeditor();
        
    //    var textarea = editor.find("textarea");

    //    beforeEach(() => {
    //        textarea.val('');
    //    });

    //    afterEach(() => {
    //        editor.bmaeditor("destroy");
    //    })

    //    it("shouldn't show inputs list on click on 'var()' function when it is no inputs",() => {
    //        var functions = editor.find(".list-of-functions").children("ul").children("li");
    //        expect(editor.find(".inputs-list-content").css("display")).toEqual("none");
    //        functions.eq(0).click();
    //        expect(editor.find(".inputs-list-content").css("display")).toEqual("none");
    //    });

    //    it("should show inputs list on click on 'var()' function after add of inputs", () => {
    //        var inputs = ["htr", "asdas"];
    //        editor.bmaeditor({ inputs: inputs });

    //        var functions = editor.find(".functions").children("ul").children("li");
    //        expect(editor.find(".inputs-list-content").css("display")).toEqual("none");
      
    //        functions.eq(0).click();

    //        expect(editor.find(".inputs-list-content").css("display")).not.toEqual("none");
    //        expect(editor.find(".inputs-list-content").children().length).toEqual(inputs.length);
    //    })

    //    it("should input CONST",() => {
    //        var functions = editor.find(".functions").children("ul").children("li");
    //        var inputs = ["htr", "asdas"];
    //        editor.bmaeditor({ inputs: inputs });
    //        functions.eq(1).click();
    //        expect(textarea.val()).toEqual("const()");
    //    });

    //});

    //describe("input operators", () => {

    //    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    //    window.Commands = new BMA.CommandRegistry();
    //    var editor = $("<div></div>").bmaeditor();
    //    var functions = editor.find(".operators").children("ul").children("li");
    //    var textarea = editor.find("textarea");

    //    beforeEach(() => {
    //        textarea.val('');
    //    });

    //    it("should input +", () => {
    //        functions.eq(0).click();
    //        expect(textarea.val()).toEqual(" + ");
    //    });

    //    it("should input -", () => {
    //        functions.eq(1).click();
    //        expect(textarea.val()).toEqual(" - ");
    //    });

    //    it("should input *", () => {
    //        functions.eq(2).click();
    //        expect(textarea.val()).toEqual(" * ");
    //    });

    //    it("should input *", () => {
    //        functions.eq(3).click();
    //        expect(textarea.val()).toEqual(" / ");
    //    });

    //    it("should input AVG", () => {
    //        functions.eq(4).click();
    //        expect(textarea.val()).toEqual("avg(,)");
    //    });

    //    it("should input MIN", () => {
    //        functions.eq(5).click();
    //        expect(textarea.val()).toEqual("min(,)");
    //    });

    //    it("should input MAX", () => {
    //        functions.eq(6).click();
    //        expect(textarea.val()).toEqual("max(,)");
    //    });

    //    it("should input CEIL", () => {
    //        functions.eq(7).click();
    //        expect(textarea.val()).toEqual("ceil()");
    //    });

    //    it("should input FLOOR", () => {
    //        functions.eq(8).click();
    //        expect(textarea.val()).toEqual("floor()");
    //    });
    //})
    
    

    //it("should input vars in formula correctly after choosing from the list", () => {
    //    var inputs = editor.find(".inputs-list-content").children();
    //    var textarea = editor.find("textarea");
    //    textarea.bind("input change", function () {
    //        console.log("inputed variable");
    //    });
    //    inputs.eq(1).click();
    //    expect(editor.bmaeditor("option", "formula")).toEqual(inputs.eq(1).text());
    //    expect(textarea.val()).toEqual(inputs.eq(1).text());
    //});

    //it("should input choosed variables and functions in formula", function () {
    //    var v1 = new BMA.Model.Variable(34, 15, BMA.Model.VariableTypes.Default, "one", 3, 7, "formula1");
    //    var v2 = new BMA.Model.Variable(38, 10, BMA.Model.VariableTypes.Constant, "two", 1, 14, "formula2");
    //    var v3 = new BMA.Model.Variable(39, 10, BMA.Model.VariableTypes.Constant, "three", 1, 14, "formula2");
    //    var arr = [v1, v2, v3];
    //    editor.bmaeditor({ inputs: arr});
    //    var inputs = editor.find(".inputs-list-content").children();
    //    var textarea = editor.find("textarea");

    //    inputs.eq(1).click();
    //    expect(editor.bmaeditor("getFormula")).toEqual("var(" + inputs.eq(1).text() +")");
    //    expect(textarea.val()).toEqual("var(" + inputs.eq(1).text()+")");
    //});

    //it("should create a variableeditorchanged command", () => {
    //    var flag = false;
    //    var varchangedcallback = function () { flag = true; };
    //    editor.bmaeditor({
    //        onvariablechangedcallback: varchangedcallback
    //    });
    //    var nameinput = editor.find("input").eq(0);
    //    editor.bmaeditor("option", "name", "test");
    //    expect(flag).toBeFalsy();
    //    expect(nameinput.val()).toEqual("test");

    //    nameinput.val("test2").change();
    //    expect(flag).toBeTruthy();
    //    expect(nameinput.val()).toEqual("test2");
    //    //expect(editor.bmaeditor("option", "name")).toEqual("test2");
    //});

    //it("should not create a variableeditorchanged command", () => {
    //    var flag = false;
    //    var varchangedcallback = function () { flag = true; };
    //    editor.bmaeditor({
    //        onvariablechangedcallback: varchangedcallback
    //    });
    //    var textarea = editor.find("textarea");
    //    textarea.val("testformula").change();

    //    expect(flag).toBeFalsy();
    //    expect(editor.bmaeditor("option", "formula")).toEqual("");
    //    expect(textarea.val()).toEqual("testformula");
    //});
    

})   
