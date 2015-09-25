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

    it("state with one keyframe equation with '>='",() => {
        keyframes = [];
        states.push({
            name: "state1",
            description: "",
            formula: [
                [
                    { type: "const", value: 56 },
                    { type: "operator", value: ">=" },
                    { type: "variable", value: { container: "cell", variable: "var1" } },
                    undefined,
                    undefined
                ]
            ],
        });
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state1", equation));
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
    
    it("state with double finished keyframe equation with '>=' and '<='",() => {
        states.push(
            {
                name: "state5",
                description: "",
                formula: [
                    [
                        { type: "const", value: 56 },
                        { type: "operator", value: ">=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: "<=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(55), ">",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state5", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with double finished keyframe equation with '=' and '>='", ()=> {
        states.push(
            {
                name: "state6",
                description: "",
                formula: [
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: "=" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: ">=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(3), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(5)),
            new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), ">", new BMA.LTLOperations.ConstOperand(55))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state6", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
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
        doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(55), "<",
            new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state7", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with double finished keyframe equation with '<' and '>='",() => {
        states.push(
            {
                name: "state8",
                description: "",
                formula: [
                    [
                        { type: "const", value: 4 },
                        { type: "operator", value: "<" },
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: ">=" },
                        { type: "const", value: 56 },
                    ],
                ],
            });
        equation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(4), "<",
            new BMA.LTLOperations.NameOperand("var1"), ">", new BMA.LTLOperations.ConstOperand(55))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state8", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

    it("state with finished keyframe equation and double keyframe equation",() => {
        states.push(
            {
                name: "state9",
                description: "",
                formula: [
                    [
                        undefined,
                        undefined,
                        { type: "variable", value: { container: "cell", variable: "var1" } },
                        { type: "operator", value: ">=" },
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
        equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand("var1"), ">", new BMA.LTLOperations.ConstOperand(55)),
            new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(4), "<",
                new BMA.LTLOperations.NameOperand("var1"), "<", new BMA.LTLOperations.ConstOperand(57))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state9", equation));
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state112", equation));
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state14", doubleEquation));
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state6", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });
}); 