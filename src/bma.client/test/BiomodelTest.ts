describe("model", () => {

    describe("BMA.Model.Variable", () => {

        var id = 245, containerId = 312, type = "testtype", name = "testname", rangeFrom = 10, rangeTo = 15, formula = "testformula";

        it("creates BMA.Model.Variable with right properties", () => {
            var variable = new BMA.Model.Variable(id, containerId, type, name, rangeFrom, rangeTo, formula);
            expect(variable.ContainerId).toEqual(containerId);
            expect(variable.Formula).toEqual(formula);
            expect(variable.Id).toEqual(id);
            expect(variable.Name).toEqual(name);
            expect(variable.RangeFrom).toEqual(rangeFrom);
            expect(variable.RangeTo).toEqual(rangeTo);
            expect(variable.Type).toEqual(type);
        });

        it("creates BMA.Model.Variable and apply function GetJSON", () => {
            var variable = new BMA.Model.Variable(id, containerId, type, name, rangeFrom, rangeTo, formula);
            var json = variable.GetJSON();
            expect(json).toEqual({
                Id: id,
                Name: name,
                RangeFrom: rangeFrom,
                RangeTo: rangeTo,
                formula: formula
            })
        });
    });

    describe("BMA.Model.Relationship", () => {
        var id = 245, fromVariableId = 312, toVariableId = 256, type = "testtype";

        it("creates BMA.Model.Relationship with right properties", () => {

            var relationship = new BMA.Model.Relationship(id, fromVariableId, toVariableId, type);

            expect(relationship.Id).toEqual(id);
            expect(relationship.FromVariableId).toEqual(fromVariableId);
            expect(relationship.ToVariableId).toEqual(toVariableId);
            expect(relationship.Type).toEqual(type);
        });

        it("creates BMA.Model.Relationship and apply function GetJSON", () => {
            var relationship = new BMA.Model.Relationship(id, fromVariableId, toVariableId, type);
            var json = relationship.GetJSON();
            expect(json).toEqual({
                Id: id,
                FromVariableId: fromVariableId,
                ToVariableId: toVariableId,
                Type: type
            })
        });
    });

    describe("BMA.Model.BioModel", () => {

        var name = "TestBioModel";
        var v1 = new BMA.Model.Variable(34, 15, "type1", "name1", 3, 7, "formula1");
        var v2 = new BMA.Model.Variable(38, 10, "type2", "name2", 1, 14, "formula2");
        var r1 = new BMA.Model.Relationship(3, 34, 38, "type1");
        var r2 = new BMA.Model.Relationship(3, 38, 34, "type2");
        var r3 = new BMA.Model.Relationship(3, 34, 34, "type3");
        var variables = [v1, v2];
        var relationships = [r1, r2, r3];

        it("creates BMA.Model.BioModel", () => {
            var biomodel = new BMA.Model.BioModel(name, variables, relationships);
            expect(biomodel.Name).toEqual(name);
            expect(biomodel.Variables).toEqual(variables);
            expect(biomodel.Relationships).toEqual(relationships);
        });

        it("creates BMA.Model.BioModel and changes its name", () => {
            var biomodel = new BMA.Model.BioModel(name, variables, relationships);

            name = "NewTestName";
            expect(biomodel.Name).not.toEqual(name);
            biomodel.Name = name;
            expect(biomodel.Name).toEqual(name);
        });

        it("creates BMA.Model.BioModel and clones it to the second BioModel using 'Clone()' function", () => {

            var biomodel = new BMA.Model.BioModel(name, variables, relationships);
            var biomodel2 = biomodel.Clone();
            expect(biomodel2).toEqual(biomodel);

            biomodel.Name = "CloneName";
            expect(biomodel2).not.toEqual(biomodel);

            biomodel2 = biomodel.Clone();
            expect(biomodel2).toEqual(biomodel);

            relationships = [r1, r3];
            biomodel = new BMA.Model.BioModel(name, variables, relationships);
            expect(biomodel2).not.toEqual(biomodel);

            relationships = [r1, r2, r3];
        });

        it("creates BMA.Model.BioModel and get existing variable from it using function 'GetVariableById(id)'", () => {
            var biomodel = new BMA.Model.BioModel(name, variables, relationships);
            var v = biomodel.GetVariableById(34);
            expect(v).toEqual(v1);
        });

        it("creates BMA.Model.BioModel and try to get not existing variable from it using function 'GetVariableById(id)', should return undefined", () => {
            var biomodel = new BMA.Model.BioModel(name, variables, relationships);
            var v = biomodel.GetVariableById(1);
            expect(v).toEqual(undefined);
        });

        it("creates BMA.Model.BioModel and apply function 'GetJSON()'", () => {
            var biomodel = new BMA.Model.BioModel(name, variables, relationships);
            var result = biomodel.GetJSON();

            var varsJSON = [];
            for (var i = 0; i < variables.length; i++) {
                varsJSON.push(variables[i].GetJSON());
            }

            var relsJSON = [];
            for (var i = 0; i < relationships.length; i++) {
                relsJSON.push(relationships[i].GetJSON());
            }

            expect(result).toEqual({
                ModelName: name,
                Engine: "VMCAI",
                Variables: varsJSON,
                Relationships: relsJSON
            });
        });
    });

    describe("BMA.Model.VarialbeLayout", () => {
        var id = 123, positionX = 18, positionY = 85, cellX = 91, cellY = 64, angle = 37;

        it("creates BMA.Model.VarialbeLayout with right properties", () => {
            var VL = new BMA.Model.VarialbeLayout(id, positionX, positionY, cellX, cellY, angle);

            expect(VL.Angle).toEqual(angle);
            expect(VL.CellX).toEqual(cellX);
            expect(VL.CellY).toEqual(cellY);
            expect(VL.Id).toEqual(id);
            expect(VL.PositionX).toEqual(positionX);
            expect(VL.PositionY).toEqual(positionY);
        });
    });

    describe("BMA.Model.ContainerLayout", () => {
        var id = 134, size = 18, positionX = 64, positionY = 85;

        it("creates BMA.Model.ContainerLayout with right properties", () => {
            var CL = new BMA.Model.ContainerLayout(id, "", size, positionX, positionY);

            expect(CL.Id).toEqual(id);
            expect(CL.Size).toEqual(size);
            expect(CL.PositionX).toEqual(positionX);
            expect(CL.PositionY).toEqual(positionY);
        });
    });

    describe("BMA.Model.Layout", () => {
        var VL1 = new BMA.Model.VarialbeLayout(15, 97, 0, 54, 32, 16);
        var VL2 = new BMA.Model.VarialbeLayout(62, 22, 41, 0, 3, 7);
        var VL3 = new BMA.Model.VarialbeLayout(9, 14, 75, 6, 4, 0);
        var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
        var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
        var containers = [CL1, CL2];
        var varialbes = [VL1, VL2, VL3];

        it("creates BMA.Model.Layout with right properties", () => {

            var layout = new BMA.Model.Layout(containers, varialbes);
            expect(layout.Variables).toEqual(varialbes);
            expect(layout.Containers).toEqual(containers);
        });

        it("creates BMA.Model.Layout and clones it to the second Layout using 'Clone()' function", () => {

            var layout = new BMA.Model.Layout(containers, varialbes);
            var layout2 = layout.Clone();
            expect(layout2).toEqual(layout);

            containers.length = 1;
            layout = new BMA.Model.Layout(containers, varialbes);
            expect(layout2).not.toEqual(layout);

            containers = [CL1, CL2];
        });

        it("creates BMA.Model.Layout and apply function 'GetContainerById(id)'", () => {
            var layout = new BMA.Model.Layout(containers, varialbes);
            var c = layout.GetContainerById(CL2.Id);
            expect(c).toEqual(CL2);
        });

        it("creates BMA.Model.Layout and try to apply function 'GetContainerById(id) with not existing id, should return undefined'", () => {
            var layout = new BMA.Model.Layout(containers, varialbes);
            var c = layout.GetContainerById(1);
            expect(c).toEqual(undefined);
        });
    });

});