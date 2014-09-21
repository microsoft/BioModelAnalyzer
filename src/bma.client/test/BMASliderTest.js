describe("BMASlider", function () {
    var slider, bma;

    beforeEach(function () {
        slider = $("<div></div>");
        slider.bmazoomslider();
    });

    afterEach(function () {
        slider.children().detach();
        slider.bmazoomslider("destroy");
    });

    it("should set a value", function () {
        slider.bmazoomslider({ value: 5 });
        expect(slider.bmazoomslider("option", "value")).toEqual(5);
    });

    it("should be 0 initially", function () {
        expect(slider.bmazoomslider("option", "value")).toEqual(0);
    });

    it("should increace and decrease", function () {
        expect(slider.bmazoomslider("option", "value")).toEqual(0);
        slider.children("#zoom-minus").click();
        slider.children("#zoom-minus").click();
        expect(slider.bmazoomslider("option", "value")).toEqual(20);
        slider.children("#zoom-plus").click();
        expect(slider.bmazoomslider("option", "value")).toEqual(10);
    });
});
//# sourceMappingURL=BMASliderTest.js.map
