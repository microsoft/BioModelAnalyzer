describe("XML parser", () => {

    //beforeEach(() => {
    //    slider = $("<div></div>");
    //    slider.bmazoomslider();
    //});

    //afterEach(() => {
    //    slider.children().detach();
    //    slider.bmazoomslider("destroy");
    //})

    it("reads simple model", done => {
        $.get("data/2var_unstable.xml", null, "xml").then(data => {
            var $xml = $(data);
            var $vars = $xml.children("Model").children("Variables").children("Variable");
            expect($vars.length).toBe(2);
            done();
        });
    });

})   