describe("BMA.Model.AppModel", () => {

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

    var VL1 = new BMA.Model.VariableLayout(15, 97, 0, 54, 32, 16);
    var VL2 = new BMA.Model.VariableLayout(62, 22, 41, 0, 3, 7);
    var VL3 = new BMA.Model.VariableLayout(9, 14, 75, 6, 4, 0);
    var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
    var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
    var containers = [CL1, CL2];
    var varialbes = [VL1, VL2, VL3];
    var layout = new BMA.Model.Layout(containers, varialbes);

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
        var ml = {
            "model":
            {
                "name": "model 1",
                "variables":
                    [{
                        "id": 3,
                        "containerId": 1,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 4,
                        "containerId": 1,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 5,
                        "containerId": 1,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 6,
                        "containerId": 2,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 7,
                        "containerId": 2,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 8,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 9,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 10,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 11,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 12,
                        "containerId": 1,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 13,
                        "containerId": 1,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 14,
                        "containerId": 2,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    }],
                "relationships": [
                    { "id": 15, "fromVariableId": 4, "toVariableId": 3, "type": "Activator" },
                    { "id": 16, "fromVariableId": 3, "toVariableId": 10, "type": "Activator" },
                    { "id": 17, "fromVariableId": 12, "toVariableId": 8, "type": "Activator" },
                    { "id": 18, "fromVariableId": 3, "toVariableId": 12, "type": "Activator" },
                    { "id": 19, "fromVariableId": 3, "toVariableId": 13, "type": "Activator" },
                    { "id": 20, "fromVariableId": 11, "toVariableId": 10, "type": "Activator" },
                    { "id": 21, "fromVariableId": 13, "toVariableId": 11, "type": "Activator" },
                    { "id": 22, "fromVariableId": 8, "toVariableId": 4, "type": "Inhibitor" },
                    { "id": 23, "fromVariableId": 4, "toVariableId": 12, "type": "Inhibitor" },
                    { "id": 24, "fromVariableId": 12, "toVariableId": 5, "type": "Inhibitor" },
                    { "id": 25, "fromVariableId": 5, "toVariableId": 3, "type": "Inhibitor" },
                    { "id": 26, "fromVariableId": 3, "toVariableId": 14, "type": "Inhibitor" },
                    { "id": 27, "fromVariableId": 14, "toVariableId": 7, "type": "Activator" },
                    { "id": 28, "fromVariableId": 7, "toVariableId": 6, "type": "Activator" },
                    { "id": 29, "fromVariableId": 6, "toVariableId": 9, "type": "Activator" },
                    { "id": 30, "fromVariableId": 9, "toVariableId": 8, "type": "Activator" },
                    { "id": 31, "fromVariableId": 8, "toVariableId": 7, "type": "Inhibitor" },
                    { "id": 32, "fromVariableId": 5, "toVariableId": 14, "type": "Inhibitor" }
                ]
            },
            "layout": {
                "containers": [
                    { "id": 1, "name": "", "size": 1, "positionX": 2, "positionY": -2 },
                    { "id": 2, "name": "", "size": 1, "positionX": 3, "positionY": -1 }
                ],
                "variables": [
                    { "id": 3, "positionX": 565.8333333333334, "positionY": -383.33333333333337, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 4, "positionX": 615.8333333333334, "positionY": -438.33333333333337, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 5, "positionX": 655, "positionY": -368.33333333333337, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 6, "positionX": 890, "positionY": -143.33333333333334, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 7, "positionX": 843.3333333333334, "positionY": -201.66666666666668, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 8, "positionX": 890, "positionY": -440, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 9, "positionX": 1208.3333333333334, "positionY": -475.83333333333337, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 10, "positionX": 372.5, "positionY": -408.33333333333337, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 11, "positionX": 534.1666666666667, "positionY": -129.16666666666668, "cellX": 0, "cellY": 0, "angle": 0 },
                    { "id": 12, "positionX": 589.1207411107429, "positionY": -538.5492663468165, "cellX": 0, "cellY": 0, "angle": 343.1873737645758 },
                    { "id": 13, "positionX": 546.0174166465277, "positionY": -337.68596171434933, "cellX": 0, "cellY": 0, "angle": 223.81881108667335 },
                    { "id": 14, "positionX": 770.550548111634, "positionY": -143.90326466745861, "cellX": 0, "cellY": 0, "angle": 272.1369108537573 }]
            }
        }

        var variables = [];
        for (var i = 0; i < ml.model.variables.length; i++) {
            variables.push(new BMA.Model.Variable(
                ml.model.variables[i].id,
                ml.model.variables[i].containerId,
                ml.model.variables[i].type,
                ml.model.variables[i].name,
                ml.model.variables[i].rangeFrom,
                ml.model.variables[i].rangeTo,
                ml.model.variables[i].formula));
        }

        var variableLayouts = [];
        for (var i = 0; i < ml.layout.variables.length; i++) {
            variableLayouts.push(new BMA.Model.VariableLayout(
                ml.layout.variables[i].id,
                ml.layout.variables[i].positionX,
                ml.layout.variables[i].positionY,
                ml.layout.variables[i].cellX,
                ml.layout.variables[i].cellY,
                ml.layout.variables[i].angle));
        }

        var relationships = [];
        for (var i = 0; i < ml.model.relationships.length; i++) {
            relationships.push(new BMA.Model.Relationship(
                ml.model.relationships[i].id,
                ml.model.relationships[i].fromVariableId,
                ml.model.relationships[i].toVariableId,
                ml.model.relationships[i].type));
        }

        var containers = [];
        for (var i = 0; i < ml.layout.containers.length; i++) {
            containers.push(new BMA.Model.ContainerLayout(
                ml.layout.containers[i].id,
                ml.layout.containers[i].name,
                ml.layout.containers[i].size,
                ml.layout.containers[i].positionX,
                ml.layout.containers[i].positionY));
        }

        this.model = new BMA.Model.BioModel(ml.model.name, variables, relationships);
        this.layout = new BMA.Model.Layout(containers, variableLayouts);
        
        var serializedModel = JSON.stringify(ml);
        appModel.Reset(serializedModel);
        expect(appModel.BioModel).toEqual(this.model);
        expect(appModel.Layout).toEqual(this.layout);
        expect(appModel.ProofResult).toEqual(undefined);
    });

    it("shouldn't reset model when serializedModel isn't correct", () => {
        var ml = {
            "model":
            {
                "name": "model 1",
                "variables":
                [{
                    "id": 3,
                    "containerId": 1,
                    "type": "Default",
                    "rangeFrom": 0,
                    "rangeTo": 1,
                    "formula": "",
                    "name": ""
                },
                    {
                        "id": 4,
                        "containerId": 1,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 5,
                        "containerId": 1,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 6,
                        "containerId": 2,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 7,
                        "containerId": 2,
                        "type": "Default",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 8,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 9,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 10,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 11,
                        "containerId": 0,
                        "type": "Constant",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 12,
                        "containerId": 1,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 13,
                        "containerId": 1,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    },
                    {
                        "id": 14,
                        "containerId": 2,
                        "type": "MembraneReceptor",
                        "rangeFrom": 0,
                        "rangeTo": 1,
                        "formula": "",
                        "name": ""
                    }]
            }
        }

        
        var serialized = JSON.stringify(ml);

        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        newAppModel.BioModel = biomodel;
        newAppModel.Layout = layout;
        newAppModel.ProofResult = proof;

        appModel.Reset(serialized);
        
        expect(appModel).toEqual(newAppModel);
    });

    it("should turn model as new when serializedModel is undefined", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        appModel.Reset(undefined);

        expect(appModel).toEqual(newAppModel);
    });

    it("should turn model as new when serializedModel is null", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var newAppModel = new BMA.Model.AppModel();
        appModel.Reset(null);

        expect(appModel).toEqual(newAppModel);
    });

    it("should serialize model", () => {
        appModel.BioModel = biomodel;
        appModel.Layout = layout;
        appModel.ProofResult = proof;

        var str = '{"model":{"name":"TestBioModel","variables":[{"id":34,"containerId":15,"type":"Default","rangeFrom":3,"rangeTo":7,"formula":"formula1","name":"name1"},{"id":38,"containerId":10,"type":"Constant","rangeFrom":1,"rangeTo":14,"formula":"formula2","name":"name2"}],"relationships":[{"id":3,"fromVariableId":34,"toVariableId":38,"type":"Activator"},{"id":3,"fromVariableId":38,"toVariableId":34,"type":"Activator"},{"id":3,"fromVariableId":34,"toVariableId":34,"type":"Inhibitor"}]},"layout":{"containers":[{"id":7,"name":"","size":5,"positionX":1,"positionY":6},{"id":3,"name":"","size":24,"positionX":81,"positionY":56}],"variables":[{"id":15,"positionX":97,"positionY":0,"cellX":54,"cellY":32,"angle":16},{"id":62,"positionX":22,"positionY":41,"cellX":0,"cellY":3,"angle":7},{"id":9,"positionX":14,"positionY":75,"cellX":6,"cellY":4,"angle":0}]}}'

        expect(appModel.Serialize()).toEqual(str);
    });
}); 