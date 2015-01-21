describe("model transformation", function () {
    it("maps variable names correctly", function () {
        expect(BMA.Model.MapVariableNames("var(a)-var(b)", function (s) {
            return s.toUpperCase();
        })).toBe("var(A)-var(B)");
    });

    it("handles duplicate variables names", function () {
        var bm = new BMA.Model.BioModel("Test model", [
            new BMA.Model.Variable(1, 0, BMA.Model.VariableTypes.Default, "a", 0, 1, ""),
            new BMA.Model.Variable(2, 0, BMA.Model.VariableTypes.Default, "a", 0, 1, ""),
            new BMA.Model.Variable(3, 0, BMA.Model.VariableTypes.Constant, "b", 0, 1, "var(a)")], [
            new BMA.Model.Relationship(1, 1, 2, BMA.Model.RelationshipTypes.Activator),
            new BMA.Model.Relationship(2, 2, 3, BMA.Model.RelationshipTypes.Inhibitor)]);
        var ebm = BMA.Model.ExportBioModel(bm);
        expect(ebm.Variables.filter(function (v) {
            return v.Id == 3;
        })[0].Formula).toBe("var(2)");
    });
});
