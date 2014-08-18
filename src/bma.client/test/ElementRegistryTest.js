describe("SVGPlot", function () {
    it("should be succesfully created", function () {
        var type = "testType";
        var description = "cell";
        var url = "images/container.png";
        var elem = new BMA.Elements.Element(type, function (jqSvg, renderParams) {
            var g = jqSvg.group({
                transform: "translate(" + renderParams.PositionX + ", " + renderParams.PositionY + ") scale(2.5)"
            });

            var innerCellData = "M3.6-49.9c-26.7,0-48.3,22.4-48.3,50c0,27.6,21.6,50,48.3,50c22.8,0,41.3-22.4,41.3-50C44.9-27.5,26.4-49.9,3.6-49.9z";
            var innerPath = jqSvg.createPath();
            jqSvg.path(g, innerPath, {
                stroke: 'transparent',
                fill: "#FAAF42",
                d: innerCellData
            });

            var outeCellData = "M3.6,45.5C-16.6,45.5-33,25.1-33,0.1c0-25,16.4-45.3,36.6-45.3c20.2,0,36.6,20.3,36.6,45.3C40.2,25.1,23.8,45.5,3.6,45.5z";
            var outerPath = jqSvg.createPath();
            jqSvg.path(g, outerPath, {
                stroke: 'transparent',
                fill: "#FFF",
                d: outeCellData
            });

            var svgElem = $(jqSvg.toSVG()).children();
            return svgElem;
        }, description, url);

        var drawingSvg = $("<div></div>");

        expect(drawingSvg).toBeDefined();

        expect(elem.Type).toEqual(type);
        expect(elem.Description).toEqual(description);
        expect(elem.IconURL).toEqual(url);
    });
});
