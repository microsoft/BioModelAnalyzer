describe("VisibilitySettings", function () {
    var vsTable, l1, l2, l3, ul, op1;
    beforeEach(function () {
        vsTable = $("<div></div>");
        ul = $('<ul></ul>').appendTo(vsTable);
        l1 = $("<li></li>").appendTo(ul);
        op1 = $("<div>option1</div>").appendTo(l1);
        l2 = $('<div data-behavior="toggle" data-command="command"></div>').appendTo(l1);
        l3 = $('<div data-behavior="increment" value=14></div>').appendTo(l1);
        vsTable.visibilitysettings();
    });

    afterEach(function () {
        vsTable.visibilitysettings("destroy");
        vsTable.children().detach();
    });

    it("should be created correctly", function () {
        expect(l2.text()).toEqual("ON");
        expect($(l3.children()[0]).text()).toEqual('+');
        expect($(l3.children()[1]).text()).toEqual('-');
    });

    it("another option added", function () {
        vsTable.visibilitysettings("destroy");
        var li2 = $('<li></li>').appendTo(ul);
        var op2 = $('<div>option2</div>').appendTo(li2);
        var t2 = $('<div data-behavior="toggle" data-default="true" data-command="command"></div>').appendTo(li2);
        vsTable.visibilitysettings();
        expect(t2.text()).toEqual("ON");
    });
});
