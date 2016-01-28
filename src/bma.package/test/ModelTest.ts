describe("BMA.Model.AppModel",() => {

    window.Commands = new BMA.CommandRegistry();
    var appModel: BMA.Model.AppModel;

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
    //var VL3 = new BMA.Model.VariableLayout(9, 14, 75, 6, 4, 0);
    var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
    var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
    var containers = [CL1, CL2];
    var layoutVariables = [VL1, VL2];//, VL3];
    var layout = new BMA.Model.Layout(containers, layoutVariables);

    var isStable = true;
    var time = 15;
    var ticks = ["one", 4, { ten: 10 }];
    var proof = new BMA.Model.ProofResult(isStable, time, ticks);

    beforeEach(function () {
        appModel = new BMA.Model.AppModel();
    });

    afterEach(function () {
        appModel = undefined;
    });

    it("sets BioModel property", () => {
        appModel.BioModel = biomodel;
        expect(appModel.BioModel).toEqual(biomodel);
    });

    it("executes 'AppModelChanged' command when BioModel property changed", () => {
        spyOn(window.Commands, 'Execute');
        appModel.BioModel = biomodel;
        expect(window.Commands.Execute).toHaveBeenCalledWith("AppModelChanged", {});
    });

    it("sets Layout property", () => {
        appModel.Layout = layout;
        expect(appModel.Layout).toEqual(layout);
    });

    it("sets ProofResult property", () => {
        appModel.ProofResult = proof;
        expect(appModel.ProofResult).toEqual(proof);
    });

    it("should reset model to correct serializedModel", () => {
        var ml = { "Model": { "Name": "model 1", "Variables": [{ "Id": 1, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "1" }, { "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }], "Relationships": [{ "Id": 4, "FromVariable": 1, "ToVariable": 2, "Type": "Activator" }, { "Id": 5, "FromVariable": 2, "ToVariable": 3, "Type": "Activator" }, { "Id": 6, "FromVariable": 3, "ToVariable": 1, "Type": "Activator" }] }, "Layout": { "Variables": [{ "Id": 1, "Name": "", "Type": "Constant", "ContainerId": 0, "PositionX": -356.212986463621, "PositionY": -93.81451737860536, "CellX": 0, "CellY": 0, "Angle": 0 }, { "Id": 2, "Name": "sdccc", "Type": "Constant", "ContainerId": 0, "PositionX": -169.28934010152284, "PositionY": -106.15853176100808, "CellX": 0, "CellY": 0, "Angle": 0 }, { "Id": 3, "Name": "", "Type": "Constant", "ContainerId": 0, "PositionX": -137.54758883248732, "PositionY": 71.94796147080243, "CellX": 0, "CellY": 0, "Angle": 0 }], "Containers": [] } };

        var variables = [];
        for (var i = 0; i < ml.Model.Variables.length; i++) {
            variables.push(new BMA.Model.Variable(
                ml.Model.Variables[i].Id,
                ml.Layout.Variables[i].ContainerId,
                ml.Layout.Variables[i].Type,
                ml.Layout.Variables[i].Name,
                ml.Model.Variables[i].RangeFrom,
                ml.Model.Variables[i].RangeTo,
                ml.Model.Variables[i].Formula));
        }

        var variableLayouts = [];
        for (var i = 0; i < ml.Layout.Variables.length; i++) {
            variableLayouts.push(new BMA.Model.VariableLayout(
                ml.Layout.Variables[i].Id,
                ml.Layout.Variables[i].PositionX,
                ml.Layout.Variables[i].PositionY,
                ml.Layout.Variables[i].CellX,
                ml.Layout.Variables[i].CellY,
                ml.Layout.Variables[i].Angle));
        }

        var relationships = [];
        for (var i = 0; i < ml.Model.Relationships.length; i++) {
            relationships.push(new BMA.Model.Relationship(
                ml.Model.Relationships[i].Id,
                ml.Model.Relationships[i].FromVariable,
                ml.Model.Relationships[i].ToVariable,
                ml.Model.Relationships[i].Type));
        }

        var containers = [];
        for (var i = 0; i < ml.Layout.Containers.length; i++) {
            containers.push(new BMA.Model.ContainerLayout(
                ml.Layout.Containers[i].id,
                ml.Layout.Containers[i].name,
                ml.Layout.Containers[i].size,
                ml.Layout.Containers[i].positionX,
                ml.Layout.Containers[i].positionY));
        }

        this.model = new BMA.Model.BioModel(ml.Model.Name, variables, relationships);
        this.layout = new BMA.Model.Layout(containers, variableLayouts);
        
        var serializedModel = JSON.stringify(ml);
        appModel.Deserialize(serializedModel);
        expect(appModel.BioModel).toEqual(this.model);
        expect(appModel.Layout).toEqual(this.layout);
        expect(appModel.ProofResult).toEqual(undefined);
    });

    it("shouldn't reset model when serializedModel isn't correct", () => {
        var ml = {
            "Model": {
                "Variables": [
                    { "Id": 1, "RangeFrom": 0, "RangeTo": 1, "Formula": "" },
                    { "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "1" },
                    { "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }],
                //"Relationships": [
                //    { "Id": 4, "FromVariable": 1, "ToVariable": 2, "Type": "Activator" },
                //    { "Id": 5, "FromVariable": 2, "ToVariable": 3, "Type": "Activator" },
                //    { "Id": 6, "FromVariable": 3, "ToVariable": 1, "Type": "Activator" }
                //]
            }, "Layout": {
                "Variables": [
                    { "Id": 1, "Name": "", "Type": "Constant", "ContainerId": 0, "PositionX": -356.212986463621, "PositionY": -93.81451737860536, "CellX": 0, "CellY": 0, "Angle": 0 },
                    { "Id": 2, "Name": "sdccc", "Type": "Constant", "ContainerId": 0, "PositionX": -169.28934010152284, "PositionY": -106.15853176100808, "CellX": 0, "CellY": 0, "Angle": 0 },
                    { "Id": 3, "Name": "", "Type": "Constant", "ContainerId": 0, "PositionX": -137.54758883248732, "PositionY": 71.94796147080243, "CellX": 0, "CellY": 0, "Angle": 0 }],
                "Containers": []
            }
        };

        
        var serialized = JSON.stringify(ml);

        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        newAppModel.BioModel = biomodel;
        newAppModel.Layout = layout;
        newAppModel.ProofResult = proof;
        try {
            appModel.Deserialize(serialized);
        }
        catch (ex) { }

        expect(appModel).toEqual(newAppModel);
    });

    it("should turn model as new when serializedModel is undefined", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        appModel.Deserialize(undefined);

        expect(appModel).toEqual(newAppModel);
    });

    it("should turn model as new when serializedModel is null", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        appModel.Deserialize(null);

        expect(appModel).toEqual(newAppModel);
    });

    /*
    //TODO: Check this test
    it("should serialize model", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var str = '{"Model":{"Name":"TestBioModel","Variables":[{"Id":34,"RangeFrom":3,"RangeTo":7,"Formula":"formula1"},{"Id":38,"RangeFrom":1,"RangeTo":14,"Formula":"formula2"}],"Relationships":[{"Id":3,"FromVariable":34,"ToVariable":38,"Type":"Activator"},{"Id":3,"FromVariable":38,"ToVariable":34,"Type":"Activator"},{"Id":3,"FromVariable":34,"ToVariable":34,"Type":"Inhibitor"}]},"Layout":{"Variables":[{"Id":34,"Name":"name1","Type":"Default","ContainerId":15,"PositionX":97,"PositionY":0,"CellX":54,"CellY":32,"Angle":16},{"Id":38,"Name":"name2","Type":"Constant","ContainerId":10,"PositionX":22,"PositionY":41,"CellX":0,"CellY":3,"Angle":7}],"Containers":[{"Id":7,"Name":"","Size":5,"PositionX":1,"PositionY":6},{"Id":3,"Name":"","Size":24,"PositionX":81,"PositionY":56}]}}';
        expect(appModel.Serialize()).toEqual(str);
    });
    */

    //States validation

    var kfm1 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("name1", 34), "<", new BMA.LTLOperations.ConstOperand(2));
    var kfm2 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("name2", 38), "=", new BMA.LTLOperations.ConstOperand(0));
    var kfm3 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("name34", 34), "<", new BMA.LTLOperations.ConstOperand(2));

    var s1 = new BMA.LTLOperations.Keyframe("A", "", [kfm1]);
    var s2 = new BMA.LTLOperations.Keyframe("B", undefined, [kfm2]);
    var s3 = new BMA.LTLOperations.Keyframe("A", "", [kfm3]);

    var states = [s1, s2];

    var v3 = new BMA.Model.Variable(34, 15, BMA.Model.VariableTypes.Default, "name34", 3, 7, "formula1"); 

    it("States validation: model is not changed", () => {
        expect(BMA.ModelHelper.UpdateStatesWithModel(biomodel, layout, states)).toEqual({
            states: states, isChanged: false
        });
    });

    it("States validation: variable 'name1' removed", () => {
        biomodel = new BMA.Model.BioModel(name, [v2], []);
        expect(BMA.ModelHelper.UpdateStatesWithModel(biomodel, layout, states)).toEqual({
            states: [s2], isChanged: true
        });
    });

    it("States validation: variable 'name2' removed", () => {
        biomodel = new BMA.Model.BioModel(name, [v1], [r3]);
        expect(BMA.ModelHelper.UpdateStatesWithModel(biomodel, layout, states)).toEqual({
            states: [s1], isChanged: true
        });
    });

    it("States validation: variable 'name1' renamed", () => {
        biomodel = new BMA.Model.BioModel(name, [v3, v2], [r1, r2, r3]);
        expect(BMA.ModelHelper.UpdateStatesWithModel(biomodel, layout, states)).toEqual({
            states: [s3, s2], isChanged: true
        });
    });

    it("States validation: variable 'name34' added", () => {
        biomodel = new BMA.Model.BioModel(name, [v1, v2, v3], [r1, r2, r3]);
        expect(BMA.ModelHelper.UpdateStatesWithModel(biomodel, layout, states)).toEqual({
            states: [s1, s2], isChanged: false
        });
    });
}); 