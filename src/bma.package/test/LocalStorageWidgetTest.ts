// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("localstoragewidget",() => {

    window.Commands = new BMA.CommandRegistry();
    var widget = $("<div></div>");
    var items = ["first", "second", "third"];

    beforeEach(() => {
        widget.localstoragewidget({ items: items });
    });

    afterEach(() => {
        widget.localstoragewidget();
        widget.localstoragewidget("destroy");
    });

    it("sets 'items' option", () => {
        expect(widget.localstoragewidget("option", "items")).toEqual(items);
    });

    it("creates correct list of inputs", () => {
        var list = widget.find("ol").children("li");
        expect(list.length).toEqual(items.length);

        for (var i = 0; i < items.length; i++) {
            expect(list.eq(i).text()).toEqual(items[i]);
            expect(list.eq(i).children("button").length).toEqual(1);
        }
    });

}) 
