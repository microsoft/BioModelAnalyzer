describe("BMA.UIDrivers.StatesEditorDriver.ConvertationTest",() => {

    var statesEditorDriver: BMA.UIDrivers.StatesEditorDriver;
    var ltlCommands = new BMA.CommandRegistry();
    var popup = $("<div></div>");
    var keyframes = [];
    var states = [];
    var equation = [];
    var doubleEquation = [];

    beforeEach(() => {
        statesEditorDriver = new BMA.UIDrivers.StatesEditorDriver(ltlCommands, popup);
        keyframes = [];
        states = [];
        equation = [];
        doubleEquation = [];
    });

    it("keyframe equation",() => {
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
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=", new BMA.LTLOperations.NameOperand("var1"))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state1", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("single unfinished equation",() => {
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state2", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("double unfinished equation and single finished equation",() => {
        states.push({
            name: "state3",
            description: "",
            formula: [
                [
                    { type: "const", value: 56 },
                    { type: "operator", value: ">=" },
                    { type: "variable", value: { container: "cell", variable: "var1" } },
                    { type: "operator", value: ">=" },
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
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=", new BMA.LTLOperations.NameOperand("var1"))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state3", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });
    
    it("double finished equation",() => {
        states.push(
            {
                name: "state4",
                description: "",
                formula: [
                    [
                        { type: "const", value: 56 },
                        { type: "operator", value: ">=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: ">=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=",
            new BMA.LTLOperations.NameOperand("var1"), ">=", new BMA.LTLOperations.ConstOperand(56))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state4", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("double unfinished equation", ()=> {
        states.push(
            {
                name: "state5",
                description: "",
                formula: [
                    [
                        undefined,
                        { type: "operator", value: ">=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        undefined,
                        { type: "const", value: 56 },
                    ],
                ],
            });
        keyframes.push(new BMA.LTLOperations.Keyframe("state5", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("single unfinished equation",() => {
        states.push(
            {
                name: "state6",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        undefined,
                        { type: "const", value: 56 },
                    ],
                ],
            });
        keyframes.push(new BMA.LTLOperations.Keyframe("state6", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("single finished equation",() => {
        states.push(
            {
                name: "state7",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        { type: "const", value: 56 },
                        { type: "operator", value: ">=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                    ],
                ],
            });
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=", new BMA.LTLOperations.NameOperand("var1"))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state7", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("single finished equation",() => {
        states.push(
            {
                name: "state8",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: ">=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), ">=", new BMA.LTLOperations.ConstOperand(56))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state8", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("unfinished equation",() => {
        states.push(
            {
                name: "state9",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        undefined,
                        undefined,
                        { type: "const", value: 56 },
                    ],
                ],
            });
        keyframes.push(new BMA.LTLOperations.Keyframe("state9", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("unfinished equation",() => {
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
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                    ],
                ],
            });
        keyframes.push(new BMA.LTLOperations.Keyframe("state10", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("unfinished equation",() => {
        states.push(
            {
                name: "state11",
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state11", []));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });
}); 