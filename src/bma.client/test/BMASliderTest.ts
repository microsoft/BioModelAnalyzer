describe("BMASlider", () => {
    var slider, bma: JQuery;

    beforeEach(() => {
        slider = $("<div></div>");
        slider.bmazoomslider();
    });

    afterEach(() => {
        slider.children().detach();
        slider.bmazoomslider("destroy");
    })

    it("should set a value", () => {
        slider.bmazoomslider({ value: 5 });
        expect(slider.bmazoomslider("option", "value")).toEqual(5);
    });

    it("should be 0 initially", () => {
        expect(slider.bmazoomslider("option", "value")).toEqual(0);
    });

    it("should increace and decrease", () => {
        expect(slider.bmazoomslider("option", "value")).toEqual(0);
        slider.children("#zoom-minus").click();
        slider.children("#zoom-minus").click();
        expect(slider.bmazoomslider("option", "value")).toEqual(2);
        slider.children("#zoom-plus").click();
        expect(slider.bmazoomslider("option", "value")).toEqual(1);
    });

})  