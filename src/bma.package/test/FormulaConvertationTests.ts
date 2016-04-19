describe("BMA.UIDrivers.StatesEditorDriver.ConvertationTest",() => {

    var statesEditorDriver: BMA.UIDrivers.StatesEditorDriver;
    var ltlCommands = new BMA.CommandRegistry();
    var popup = $("<div></div>");
    var keyframes = [];
    var states = [];
    var equation = [];
    var doubleEquation = [];

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

    beforeEach(() => {
        statesEditorDriver = new BMA.UIDrivers.StatesEditorDriver(ltlCommands, popup);
        statesEditorDriver.SetModel(biomodel, layout);
        keyframes = [];
        states = [];
        equation = [];
        doubleEquation = [];
    });

    it("state with one keyframe equation with '<='",() => {
        keyframes = [];
        states.push({
            name: "state1",
            description: "",
            formula: [
                [
                    { type: "variable", value: { container: 15, variable: 34 } },
                    { type: "operator", value: "<=" },
                    { type: "const", value: 56 },
                    undefined,
                    undefined
                ]
            ],
        });
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("name1", 34), "<=", new BMA.LTLOperations.ConstOperand(56))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state1", "", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with one keyframe equation with '>=' and one unfinished keyframe equation",() => {
        keyframes = [];
        states.push({
            name: "state1",
            description: "",
            formula: [
                [
                    undefined,
                    undefined,
                    undefined,
                    undefined,
                    undefined
                ],
                [
                    { type: "const", value: 56 },
                    { type: "operator", value: ">=" },
                    { type: "variable", value: { container: "cell", variable: "var1" } },
                    undefined,
                    undefined
                ]
            ],
        });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("state with unfinished keyframe equation",() => {
        states.push({
            name: "state2",
            description: "",
            formula: [
                [
                    undefined,
                    { type: "operator", value: ">=" },
                    { type: "variable", value: { container: "cell", variable: "var1" } },
                    undefined,
                    undefined
                ]
            ]
        });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("empty state",() => {
        states.push({
            name: "state3",
            description: "",
            formula: [
            ]
        });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("state with double unfinished keyframe equation",() => {
        states.push({
            name: "state4",
            description: "",
            formula: [
                [
                    { type: "const", value: 56 },
                    { type: "operator", value: ">=" },
                    { type: "variable", value: { container: "cell", variable: "var1" } },
                    { type: "operator", value: ">=" },
                    undefined
                ],
            ],
        });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });
    

    it("state with finished keyframe equation with '='",() => {
        states.push(
            {
                name: "state7",
                description: "",
                formula: [
                    [
                        { type: "const", value: 56 },
                        { type: "operator", value: "=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        undefined,
                        undefined,
                    ],
                ],
            });
        
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("state with finished keyframe equation with '='",() => {
        states.push(
            {
                name: "state9",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "!=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), "!=", new BMA.LTLOperations.ConstOperand(55)),];
        keyframes.push(new BMA.LTLOperations.Keyframe("state9", "", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with finished and unfinished keyframe equations",() => {
        states.push(
            {
                name: "state10",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        undefined,
                        undefined,
                        { type: "const", value: 56 },
                    ],
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: "<" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "<=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("states with finished and unfinished keyframe equations",() => {
        states.push(
            {
                name: "state11",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                    ],
                ],
            });
        states.push(
            {
                name: "state112",
                description: "",
                formula: [
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: "<" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "<=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        equation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(4), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state112", "", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("unfinished equation",() => {
        states.push(
            {
                name: "state131",
                description: "",
                formula: [
                    [
                        undefined,
                        { type: "operator", value: ">=" },
                        undefined,
                        undefined,
                        undefined,
                    ],
                ],
            });
        states.push(
            {
                name: "state132",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                    ],
                ],
            });
        states.push(
            {
                name: "state133",
                description: "",
                formula: [
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: "<" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        expect(statesEditorDriver.Convert(states)).toEqual([]);
    });

    it("state with double finished keyframe equation with two '='",() => {
        states.push(
            {
                name: "state14",
                description: "",
                formula: [
                    [
                        { type: "const", value: 56 },
                        { type: "operator", value: "=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(55), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57)),
            new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(55), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state14", "", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with double finished keyframe equation with '=' and '>='",() => {
        states.push(
            {
                name: "state15",
                description: "",
                formula: [
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: ">" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(3), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(5)),
            new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(55))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state6", "", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });
}); 