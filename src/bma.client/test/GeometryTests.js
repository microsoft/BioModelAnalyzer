describe("GeometryDictionary", function () {
    var geometryDictionary = new BMA.GeometryDictionary();
    geometryDictionary.AddGeometry("trianlge", "triangle geometry");

    it("should return geometry for specific type", function () {
        var g = geometryDictionary.GetGeometry("triangle");
        expect(g).toBe("triangle geometry");
    });

    it("should throw exception for unknown geometry type", function () {
        expect(geometryDictionary.GetGeometry("triangle")).toThrow();
    });
});
//# sourceMappingURL=GeometryTests.js.map
