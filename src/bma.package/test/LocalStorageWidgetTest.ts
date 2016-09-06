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

    //it("should execute 'LocalStorageRemoveModel' command on removing item after click on appropriate button", () => {
    //    spyOn(window.Commands, "Execute");
    //    var list = widget.find("ol").children("li");
    //    list.eq(1).children("button").click();
    //    expect(window.Commands.Execute).toHaveBeenCalledWith("LocalStorageRemoveModel", "user."+items[1]);
    //});

    //it("should execute 'LocalStorageLoadModel' command when item from list was selected", () => {

    //    spyOn(window.Commands, "Execute");
    //    var ol = widget.find("ol").eq(0);
    //    ol.children().eq(0).addClass("ui-selected");
    //    var st = ol.selectable("option", "stop");
    //    st();
    //    expect(window.Commands.Execute).toHaveBeenCalledWith("LocalStorageLoadModel", 'user.' + items[0]);
    //});

}) 