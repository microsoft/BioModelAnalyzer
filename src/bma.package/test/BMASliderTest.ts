// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("BMASlider", () => {
    var slider: JQuery, bma: JQuery;
    var command = "TestCommand";
    window.Commands = new BMA.CommandRegistry();

    beforeEach(() => {
        slider = $("<div></div>").attr("data-command", command);;
        slider.bmazoomslider();
    });

    afterEach(() => {
        slider.children().detach();
        slider.bmazoomslider("destroy");
    });

    it("should set options", () => {
        var step = 82, value = 34, min = 10, max = 58;

        slider.bmazoomslider({
            step: step,
            value: value,
            min: min,
            max: max
        });

        expect(slider.bmazoomslider("option", "step")).toEqual(step);
        expect(slider.bmazoomslider("option", "value")).toEqual(value);
        expect(slider.bmazoomslider("option", "min")).toEqual(min);
        expect(slider.bmazoomslider("option", "max")).toEqual(max);
    });


    it("should create jqueryui slider with same options into bmazoomslider", () => {
        var step = 82, value = 34, min = 10, max = 58;
        slider.bmazoomslider({
            step: step,
            value: value,
            min: min,
            max: max
        });
        var jqslider = slider.children("div").eq(0);
        expect(jqslider.slider("option", "value")).toEqual(value);
        expect(jqslider.slider("option", "min")).toEqual(min);
        expect(jqslider.slider("option", "max")).toEqual(max);
    });

    describe("bounds checking", () => {
        var step = 10, value = 34, min = 10, max = 58;
        var plusButton: JQuery, minusButton: JQuery, jqslider: JQuery;
        beforeEach(() => {
            slider.bmazoomslider({
                step: step,
                value: value,
                min: min,
                max: max
            });
            plusButton = slider.children("#zoom-plus");
            minusButton = slider.children("#zoom-minus");
            jqslider = slider.children("div").eq(0);
        });

        it("should execute command which is attribule of slider element on click on plus/minus buttons", () => {
            spyOn(window.Commands, "Execute");

            plusButton.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith(command, { value: value - step, isExternal: false });

            minusButton.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith(command, { value: value, isExternal: false });
        });

        it("should set max value when it is greater than max", () => {
            var newvalue = 50;
            spyOn(window.Commands, "Execute");
            slider.bmazoomslider({ value: newvalue });
            minusButton.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith(command, { value: newvalue + slider.bmazoomslider("option", "step"), isExternal: true });
            expect(slider.bmazoomslider("option", "value")).toEqual(max);
        });

        it("should set min value when it is less than min", () => {
            var newvalue = 12;
            spyOn(window.Commands, "Execute");
            slider.bmazoomslider({ value: newvalue });
            plusButton.click();
            expect(window.Commands.Execute).toHaveBeenCalledWith(command, { value: newvalue - slider.bmazoomslider("option", "step"), isExternal: true });
            expect(slider.bmazoomslider("option", "value")).toEqual(min);
        });

        it("should change internal slider with changing bmazoomslider value", () => {
            plusButton.click();
            expect(slider.bmazoomslider("option", "value")).toEqual(jqslider.slider("option", "value"));

            minusButton.click();
            expect(slider.bmazoomslider("option", "value")).toEqual(jqslider.slider("option", "value"));
        })
    });



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
        expect(slider.bmazoomslider("option", "value")).toEqual(20);
        slider.children("#zoom-plus").click();
        expect(slider.bmazoomslider("option", "value")).toEqual(10);
    });
})
