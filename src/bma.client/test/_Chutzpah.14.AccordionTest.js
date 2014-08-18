describe("Accordion", function () {
    var acc, h1, c1, h2, c2;

    beforeEach(function () {
        acc = $("<div></div>");
        h1 = $("<div></div>").appendTo(acc);
        c1 = $("<div></div>").appendTo(acc);
        h2 = $("<div></div>").appendTo(acc);
        c2 = $("<div></div>").appendTo(acc);
        acc.bmaaccordion();
    });

    afterEach(function () {
        acc.bmaaccordion("destroy");
        acc.children().detach();
    });

    it("should inspect button widget", function () {
        var data = $('<button></button>').button();
        data.button("disable");
        data.button("enable");
        expect(data.button("option", "disabled")).toEqual(false);
    });

    xit("should get widget object", function () {
        var data = $('<button></button>').button().data("button");
        data.disable();
        data.enable();
        expect(data.options.disabled).toEqual(false);
    });

    it("should correctry set event option", function () {
        var event = "click";
        expect(acc.bmaaccordion("option", "event")).toEqual(event);
        event = "toggle";
        acc.bmaaccordion("option", "event", event);
        expect(acc.bmaaccordion("option", "event")).toEqual(event);
        //spyOn(acc, "eventHandler");
        //c1.trigger(event);
        //expect(data.active).toBe(context);
        //expect(acc.bmaaccordion("eventHandler", )).toHaveBeenCalled();
    });

    it("should be collapsible", function () {
        expect(acc.bmaaccordion("option", "collapsible")).toEqual(true);
    });

    it("should set right position", function () {
        var pos = "left";
        expect(acc.bmaaccordion("option", "position")).toEqual("center");
        pos = "wqdfsghj";
        acc.bmaaccordion("option", "position", pos);
        expect(acc.bmaaccordion("option", "position")).toEqual("center");
        pos = "top";
        acc.bmaaccordion("option", "position", pos);
        expect(acc.bmaaccordion("option", "position")).toEqual(pos);
    });

    it("should activate correct context", function () {
        h2.click();
        expect(c2.attr('aria-hidden')).toEqual('false');
        expect(c1.attr('aria-hidden')).toEqual("true");

        h1.click();
        expect(c1.attr('aria-hidden')).toEqual('false');
        expect(c2.attr('aria-hidden')).toEqual("true");
    });

    it("sets a contentLoaded option ", function () {
        acc.bmaaccordion({ contentLoaded: { ind: 1, val: false } });
        h2.click();
        //var loading = h2.children();//.filter(".loading");
        //expect(loading.length).toEqual(1);
    });
});
