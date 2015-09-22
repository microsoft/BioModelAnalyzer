describe("BMA.UIDrivers.StatesEditorDriver.ConvertationTest",() => {

    var statesEditorDriver: BMA.UIDrivers.StatesEditorDriver;
    var ltlCommands = new BMA.CommandRegistry();
    var popup = $("<div></div>");
    statesEditorDriver = new BMA.UIDrivers.StatesEditorDriver(ltlCommands, popup);

    it("should be converted",() => {
        var keyframes = [];
        var states = [
            {
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
            }
        ];
        var equation = [new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=", new BMA.LTLOperations.NameOperand("var1"))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state1",
            equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);

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

        states.push(
            {
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
        keyframes.push(new BMA.LTLOperations.Keyframe("state3", equation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);

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
        var doubleEquation = [new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(56), ">=",
            new BMA.LTLOperations.NameOperand("var1"), ">=", new BMA.LTLOperations.ConstOperand(56))];
        keyframes.push(new BMA.LTLOperations.Keyframe("state4", doubleEquation));
        expect(statesEditorDriver.Convert(states)).toEqual(keyframes);
    });

}); 