// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("VisibilitySettings", () => {
    var vsTable, li, l1, l2, l3, ul, op1;
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    window.Commands = new BMA.CommandRegistry();

    beforeEach(() => {
        vsTable = $("<div></div>");
        ul = $('<ul></ul>').appendTo(vsTable);
        li = $("<li></li>").appendTo(ul);
        op1 = $("<div>option1</div>").appendTo(li);
        var div = $('<div></div>').appendTo(li);
        l1 = $('<div data-behavior="toggle" data-command="command.toggle" data-default="false"></div>').appendTo(div);
        l2 = $('<div data-behavior="increment" data-command="command.increment" data-default="14"></div>').appendTo(div);
        var act = $('<button data-behavior="action" data-command="ModelFitToView" id="fitToViewBtn">FIT TO VIEW</button>').appendTo(div);
        vsTable.visibilitysettings();
    });

    afterEach(() => {
        vsTable.visibilitysettings("destroy");
        vsTable.children().detach();
    });

    it("should be created correctly", () => {
        expect(l1.text()).toEqual("OFF");
        expect(l2.children().eq(0).text()).toEqual('+');
        expect(l2.children().eq(1).text()).toEqual('-');
    });

    it("another option added", () => {
        vsTable.visibilitysettings("destroy");
        var li2 = $('<li></li>').appendTo(ul);
        var op2 = $('<div>option2</div>').appendTo(li2);
        var div2 = $('<div></div>').appendTo(li2);
        var t2 = $('<div data-behavior="toggle" data-default="true" data-command="command"></div>').appendTo(div2);
        vsTable.visibilitysettings();
        expect(t2.text()).toEqual("ON");
    })

    it("should change text on toggle button after click", () => {
        expect(l1.text()).toEqual("OFF");
        l1.children("button").eq(0).click();
        expect(l1.text()).toEqual("ON");
    })

    it("should execute command after click on toggle button", () => {
        spyOn(window.Commands, "Execute");
        l1.children("button").eq(0).click();
        expect(window.Commands.Execute).toHaveBeenCalledWith(l1.attr("data-command"), true);
    })

    it("should execute command after click on increment '+' button", () => {
        spyOn(window.Commands, "Execute");
        l2.children("button").eq(0).click();
        var initialvalue = parseInt(l2.attr("data-default"));
        expect(window.Commands.Execute).toHaveBeenCalledWith(l2.attr("data-command"), initialvalue+1);
    })

    it("should execute command after click on increment '-' button", () => {
        spyOn(window.Commands, "Execute");
        l2.children("button").eq(1).click();
        var initialvalue = parseInt(l2.attr("data-default"));
        expect(window.Commands.Execute).toHaveBeenCalledWith(l2.attr("data-command"), initialvalue - 1);
    })


    it("should execute ModelFitToView",() => {
        var ftv = vsTable.find("[data-behavior='action']");
        spyOn(window.Commands, "Execute");
        ftv.click();
        expect(ftv.length).toEqual(1);
        expect(window.Commands.Execute).toHaveBeenCalledWith(ftv.attr('data-command'), {});
    });
})
