// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("Accordion", () => {
    var acc, h1, c1,h2,c2: JQuery;

    beforeEach(() => {
        window.Commands = new BMA.CommandRegistry();
        acc = $("<div></div>");
        h1 = $("<div></div>").appendTo(acc);
        c1 = $("<div></div>").appendTo(acc);
        h2 = $("<div></div>").appendTo(acc);
        c2 = $("<div></div>").appendTo(acc);
        acc.bmaaccordion();
    });

    afterEach(() => {
        acc.bmaaccordion("destroy");
        acc.children().detach();
        
    })


    it("should correctly set event option", () => {
        var event = "click";
        expect(acc.bmaaccordion("option", "event")).toEqual(event);
        event = "toggle";
        acc.bmaaccordion("option", "event", event);
        expect(acc.bmaaccordion("option", "event")).toEqual(event);
    });

    it("should be collapsible", () => {
        expect(acc.bmaaccordion("option", "collapsible")).toEqual(true);
    });

    it("should set right position", () => {
        //var pos = "left";
        expect(acc.bmaaccordion("option", "position")).toEqual("center");
        var pos = "wqdfsghj";
        acc.bmaaccordion("option", "position", pos);
        expect(acc.bmaaccordion("option", "position")).toEqual("center");
        pos = "top";
        acc.bmaaccordion("option", "position", pos);
        expect(acc.bmaaccordion("option", "position")).toEqual(pos);
    });

    it("should activate correct content",() => {
        acc.bmaaccordion("option", "position", "left");
        h2.click();
        expect(c2.attr('aria-hidden')).toEqual('true');
        expect(c1.attr('aria-hidden')).toEqual("true");
        h2.click();
        expect(c2.attr('aria-hidden')).toEqual('true');
        expect(c1.attr('aria-hidden')).toEqual("true");

        //h2.click();
        //expect(c1.attr('aria-hidden')).toEqual('true');
        //expect(c2.attr('aria-hidden')).toEqual('false');
        //h1.click();
        //expect(c2.attr('aria-hidden')).toEqual('true');
        //expect(c1.attr('aria-hidden')).toEqual("false");
        //h1.click();
        //expect(c2.attr('aria-hidden')).toEqual('true');
        //expect(c1.attr('aria-hidden')).toEqual("true");
        //h2.click();
        //h2.click();
        //expect(c1.attr('aria-hidden')).toEqual('true');
        //expect(c2.attr('aria-hidden')).toEqual('false');
    });

    it("sets a contentLoaded option ", () => {
        acc.bmaaccordion({contentLoaded: { ind: 1, val: false }});
        h2.click();
        var loading = h2.children().filter(".loading");
        expect(loading.length).toEqual(1);
    });


    it("should run the command", () => {
        spyOn(window.Commands, "Execute");
        h1.attr("data-command", "testCommand");
        h1.click();
        expect(window.Commands.Execute).toHaveBeenCalledWith("testCommand", {});
        
        //h1.click();
        //expect("click").toHaveBeenTriggered();
    });


}) 
