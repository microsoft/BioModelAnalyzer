// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("ModelHelper",() => {
    var grid = { xOrigin: 0, yOrigin: 0, xStep: 100, yStep: 100 };

    it("should correctly calculate bounding box for single cell",() => {
        var layout = new BMA.Model.Layout(
            [new BMA.Model.ContainerLayout(1, "", 1, 0, 0)],
            []);

        var bb = BMA.ModelHelper.GetModelBoundingBox(layout, grid);
        expect(bb.x).toBe(0);
        expect(bb.y).toBe(0);
        expect(bb.width).toBe(100);
        expect(bb.height).toBe(100);
    });

    it("should correctly calculate bounding box for 2 cell",() => {
        var layout = new BMA.Model.Layout(
            [new BMA.Model.ContainerLayout(1, "", 1, 0, 0), new BMA.Model.ContainerLayout(2, "", 1, 1, 1)],
            []);

        var bb = BMA.ModelHelper.GetModelBoundingBox(layout, grid);
        expect(bb.x).toBe(0);
        expect(bb.y).toBe(0);
        expect(bb.width).toBe(200);
        expect(bb.height).toBe(200);
    });

    it("should correctly calculate bounding box for 2 cell with different sizes",() => {
        var layout = new BMA.Model.Layout(
            [new BMA.Model.ContainerLayout(1, "", 1, 0, 0), new BMA.Model.ContainerLayout(2, "", 3, 1, 1)],
            []);

        var bb = BMA.ModelHelper.GetModelBoundingBox(layout, grid);
        expect(bb.x).toBe(0);
        expect(bb.y).toBe(0);
        expect(bb.width).toBe(400);
        expect(bb.height).toBe(400);
    });

    it("should correctly calculate bounding box for 2 cell and 1 variable",() => {
        var layout = new BMA.Model.Layout(
            [new BMA.Model.ContainerLayout(1, "", 1, 0, 0), new BMA.Model.ContainerLayout(2, "", 3, 1, 1)],
            [new BMA.Model.VariableLayout(3, -50, -50, 0, 0, 0)]);

        var bb = BMA.ModelHelper.GetModelBoundingBox(layout, grid);
        expect(bb.x).toBe(-100);
        expect(bb.y).toBe(-100);
        expect(bb.width).toBe(500);
        expect(bb.height).toBe(500);
    });

});
