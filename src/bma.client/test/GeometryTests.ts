describe("GeometryDictionary", () => {
    var geometryDictionary = new BMA.GeometryDictionary();
    geometryDictionary.AddGeometry("trianlge", "triangle geometry");

    it("should return geometry for specific type", () => {
        var g = geometryDictionary.GetGeometry("triangle");
        expect(g).toBe("triangle geometry");
    });

    it("should throw exception for unknown geometry type", () => {
        expect(geometryDictionary.GetGeometry("triangle")).toThrow();
    });

}); 