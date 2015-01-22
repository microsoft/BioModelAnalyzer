describe("model transformation", () => {

    it("maps variable names to IDs in formulas", () => {
        expect(BMA.Model.MapVariableNames("var(a)-var(b)", s => s.toUpperCase())).toBe("var(A)-var(B)");
    });

    it("handles duplicate variables names", () => {
        var bm = new BMA.Model.BioModel(
            "Test model",
            [new BMA.Model.Variable(1, 0, BMA.Model.VariableTypes.Default, "a", 0, 1, ""),
             new BMA.Model.Variable(2, 0, BMA.Model.VariableTypes.Default, "a", 0, 1, ""),
             new BMA.Model.Variable(3, 0, BMA.Model.VariableTypes.Constant, "b", 0, 1, "var(a)")],
            [new BMA.Model.Relationship(1, 1, 2, BMA.Model.RelationshipTypes.Activator),
             new BMA.Model.Relationship(2, 2, 3, BMA.Model.RelationshipTypes.Inhibitor)]);
        var ebm = BMA.Model.ExportBioModel(bm);
        expect(ebm.Variables.filter(v => v.Id == 3)[0].Formula).toBe("var(2)");
    });

    it("exports and imports model", () => {
        var name = "TestBioModel";
        var v1 = new BMA.Model.Variable(34, 15, BMA.Model.VariableTypes.Default, "name1", 3, 7, "formula1");
        var v2 = new BMA.Model.Variable(38, 10, BMA.Model.VariableTypes.Constant, "name2", 1, 14, "formula2");
        var r1 = new BMA.Model.Relationship(3, 34, 38, BMA.Model.RelationshipTypes.Activator);
        var r2 = new BMA.Model.Relationship(3, 38, 34, BMA.Model.RelationshipTypes.Activator);
        var r3 = new BMA.Model.Relationship(3, 34, 34, BMA.Model.RelationshipTypes.Inhibitor);
        var variables = [v1, v2];
        var relationships = [r1, r2, r3];
        var biomodel = new BMA.Model.BioModel(name, variables, relationships);

        var VL1 = new BMA.Model.VariableLayout(34, 97, 0, 54, 32, 16);
        var VL2 = new BMA.Model.VariableLayout(38, 22, 41, 0, 3, 7);
        var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
        var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
        var containers = [CL1, CL2];
        var layoutVariables = [VL1, VL2];
        var layout = new BMA.Model.Layout(containers, layoutVariables);

        var str = JSON.stringify(BMA.Model.ExportModelAndLayout(biomodel, layout));
        var imported = BMA.Model.ImportModelAndLayout(JSON.parse(str));

        expect(imported.Model).toEqual(biomodel);
        expect(imported.Layout).toEqual(layout);
    });
});