describe("Export/Import LTL tests", () => {

    window.OperatorsRegistry = new BMA.LTLOperations.OperatorsRegistry();

    //Export and import state block
    //Export and import name operand
    it("Export and import state: name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var result = { _type: "NameOperand", name: "var1" };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(nameOperand);
    });
    
    it("Export and import state: empty name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("");
        var result = { _type: "NameOperand", name: "" };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(nameOperand);
    });

    it("Export and import state: undefined name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(undefined);
        var result = { _type: "NameOperand", name: undefined };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(nameOperand);
    });

    it("Export and import state: null name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(null);
        var result = { _type: "NameOperand", name: null };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(nameOperand);
    });

    //Export and import const operand
    it("Export and import state: const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var result = { _type: "ConstOperand", "const": 56 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(constOperand);
    });

    it("Export and import state: empty const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(0);
        var result = { _type: "ConstOperand", "const": 0 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(constOperand);
    });

    it("Export and import state: undefined const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(undefined);
        var result = { _type: "ConstOperand", "const": undefined };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(constOperand);
    });

    it("Export and import state: null const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(null);
        var result = { _type: "ConstOperand", "const": null };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(constOperand);
    });

    //Export and import keyframe equation
    it("Export and import state: keyframe equation, operator =", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "=", constOperand); 
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "=", nameOperand); 
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, operator >", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, ">", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: ">",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, ">", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: ">",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, operator <", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "<", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "<",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "<", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "<",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, operator <=", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "<=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "<=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "<=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, operator >=", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, ">=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: ">=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, ">=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: ">=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, operator !=", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "!=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "!=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, null operator", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, null, constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: null,
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, null, nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: null,
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, undefined operator", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, undefined, constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: undefined,
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, undefined, nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: undefined,
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, null name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(null);
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: null },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);
        
        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "!=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "!=",
            rightOperand: { _type: "NameOperand", name: null }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, undefined name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(undefined);
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: undefined },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "!=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "!=",
            rightOperand: { _type: "NameOperand", name: undefined }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, null const operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(null);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": null }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "!=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": null },
            operator: "!=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, undefined const operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(undefined);
        var keyframe = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": undefined }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);

        var keyframe = new BMA.LTLOperations.KeyframeEquation(constOperand, "!=", nameOperand);
        var result1 = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": undefined },
            operator: "!=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result1);
        expect(BMA.Model.ImportOperand(result1, undefined)).toEqual(keyframe);
    });

    //Export and import double keyframe equation

    it("Export and import state: double keyframe equation", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframe = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var result = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);
    });

    //Expost and import keyframe
    it("Export and import state: keyframe", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56}
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var result = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: [kfrmEq, dblKfrmEq, kfrmEq]
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);
        expect(BMA.Model.ImportOperand(result, [keyframe])).toEqual(keyframe);
    });

    it("Export and import state: keyframe without operands", () => {
        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", []);

        var result = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: []
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);
    });


    it("Export and import state: wrong type", () => {
        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", []);
        var result = {
            _type: "Keyframe1",
            description: "state A",
            name: "state",
            operands: []
        };

        var result1 = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: []
        };
        expect(BMA.Model.ExportState).toThrow("Unsupported State Type");
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(undefined);
        expect(BMA.Model.ImportOperand.bind(this, undefined, undefined)).toThrow("Invalid LTL Operand");
        expect(BMA.Model.ImportOperand.bind(this, undefined, null)).toThrow("Invalid LTL Operand");
        expect(BMA.Model.ImportOperand.bind(this, result1, [])).toThrow("No suitable states found");
        expect(BMA.Model.ImportOperand.bind(this, result1, [null])).toThrow("No suitable states found");
        expect(BMA.Model.ImportOperand.bind(this, result1, [undefined])).toThrow("No suitable states found");
        expect(BMA.Model.ImportOperand(result1, [undefined, keyframe])).toEqual(keyframe);
    });

    //Export and import operation block
    it("Export and import operation: operation without states", () => {
        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("ALWAYS");
        operation.Operands = [];

        var result = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: []
        };

        expect(BMA.Model.ExportOperation(operation, false)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(operation);
        expect(BMA.Model.ImportOperand(result, [])).toEqual(operation);
        expect(BMA.Model.ImportOperand(result, [undefined, undefined])).toEqual(operation);
    });

    it("Export and import operation: operation with operations", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: [kfrmEq, dblKfrmEq, kfrmEq]
        };

        var operation1 = new BMA.LTLOperations.Operation();
        operation1.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
        operation1.Operands = [keyframe];

        var resultOP1 = {
            _type: "Operation",
            operator: {
                name: operation1.Operator.Name,
                operandsCount: operation1.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        var operation2 = new BMA.LTLOperations.Operation();
        operation2.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
        operation2.Operands = [keyframe];

        var resultOP2 = {
            _type: "Operation",
            operator: {
                name: operation2.Operator.Name,
                operandsCount: operation2.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
        operation.Operands = [operation1, operation2];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultOP1, resultOP2]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
        expect(BMA.Model.ImportOperand(resultOP, [keyframe])).toEqual(operation);
        expect(BMA.Model.ImportOperand.bind(this, resultOP, [])).toThrow("No suitable states found");
    });

    it("Export and import operation: operation with operations without states", () => {
        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", []);

        var resultKeyframe = {
            _type: "Keyframe",
            name: "state"
        };

        var operation1 = new BMA.LTLOperations.Operation();
        operation1.Operands = [keyframe];
        operation1.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");

        var resultOP1 = {
            _type: "Operation",
            operator: {
                name: operation1.Operator.Name,
                operandsCount: operation1.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        var operation2 = new BMA.LTLOperations.Operation();
        operation2.Operands = [keyframe];
        operation2.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");

        var resultOP2 = {
            _type: "Operation",
            operator: {
                name: operation2.Operator.Name,
                operandsCount: operation2.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operands = [operation1, operation2];
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultOP1, resultOP2]
        };

        expect(BMA.Model.ExportOperation(operation, false)).toEqual(resultOP);
        //expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
        expect(BMA.Model.ImportOperand(resultOP, [keyframe])).toEqual(operation);
    });

    it("Export and import operation: empty operation", () => {
        var operation = new BMA.LTLOperations.Operation();

        var result = {
            _type: "Operation",
            operator: undefined,
            operands: []
        };

        expect(BMA.Model.ExportOperation.bind(this, operation, false)).toThrow("Operation must have operator");
        expect(BMA.Model.ImportOperand.bind(this, result, undefined)).toThrow("Operation must have name of operator");

        var result = {
            _type: "Operation",
            operator: null,
            operands: []
        };
        expect(BMA.Model.ExportOperation.bind(this, operation, false)).toThrow("Operation must have operator");
        expect(BMA.Model.ImportOperand.bind(this, result, undefined)).toThrow("Operation must have name of operator");
    });

    it("Export and import operation: operation with keyframe", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: [kfrmEq, dblKfrmEq, kfrmEq]
        };
        
        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
        operation.Operands = [keyframe];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
        expect(BMA.Model.ImportOperand(resultOP, [keyframe])).toEqual(operation);
    });

    it("Export and import operation: operation with keyframe without states", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            name: "state",
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operands = [keyframe];
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, false)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, [keyframe])).toEqual(operation);

        operation.Operands = [new BMA.LTLOperations.Keyframe("state", undefined, [])];
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
    });

    it("Export and import operation: operation with empty keyframe", () => {
       
        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", []);

        var resultKeyframe = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: []
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
        operation.Operands = [keyframe];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
        expect(BMA.Model.ImportOperand(resultOP, [keyframe])).toEqual(operation);
    });

    it("Export and import operation: operation with TrueKeyframe", () => {

        var keyframe = new BMA.LTLOperations.TrueKeyframe();

        var resultKeyframe = {
            _type: "TrueKeyframe",
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
        operation.Operands = [keyframe];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
    });

    it("Export and import operation: operation with OscillationKeyframe", () => {

        var keyframe = new BMA.LTLOperations.OscillationKeyframe();

        var resultKeyframe = {
            _type: "OscillationKeyframe",
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
        operation.Operands = [keyframe];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
    });

    it("Export and import operation: operation with SelfLoopKeyframe", () => {

        var keyframe = new BMA.LTLOperations.SelfLoopKeyframe();

        var resultKeyframe = {
            _type: "SelfLoopKeyframe",
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
        operation.Operands = [keyframe];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultKeyframe]
        };

        expect(BMA.Model.ExportOperation(operation, true)).toEqual(resultOP);
        expect(BMA.Model.ImportOperand(resultOP, undefined)).toEqual(operation);
    });

    //ExportLTLContents and ImportLTLContents block
    it("Export and import LTL contents: operation with keyframes", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: [kfrmEq, dblKfrmEq, kfrmEq]
        };

        var operation1 = new BMA.LTLOperations.Operation();
        operation1.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
        operation1.Operands = [keyframe];

        var resultOP1 = {
            _type: "Operation",
            operator: {
                name: operation1.Operator.Name,
                operandsCount: operation1.Operator.OperandsCount
            },
            operands: [{
                _type: "Keyframe",
                name: "state"
            }]
        };

        var operation2 = new BMA.LTLOperations.Operation();
        operation2.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
        operation2.Operands = [keyframe];

        var resultOP2 = {
            _type: "Operation",
            operator: {
                name: operation2.Operator.Name,
                operandsCount: operation2.Operator.OperandsCount
            },
            operands: [{
                _type: "Keyframe",
                name: "state"
            }]
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
        operation.Operands = [operation1, operation2];

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultOP1, resultOP2]
        };

        var result = {
            states: [resultKeyframe],
            operations: [resultOP]
        };

        expect(BMA.Model.ExportLTLContents([keyframe], [operation])).toEqual(result);
        expect(BMA.Model.ImportLTLContents(result)).toEqual({ states: [keyframe], operations: [operation] });
    });

    it("Export and import LTL contents: undefined operation with keyframes", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            description: "state A",
            name: "state",
            operands: [kfrmEq, dblKfrmEq, kfrmEq]
        };

        
        var result = {
            states: [resultKeyframe],
            operations: []
        };

        expect(BMA.Model.ExportLTLContents([keyframe], undefined)).toEqual(result);
        expect(BMA.Model.ImportLTLContents(result)).toEqual({ states: [keyframe], operations: undefined });
    });

    it("Export and import LTL contents: operation with undefined keyframe", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var keyframeEquation = new BMA.LTLOperations.KeyframeEquation(nameOperand, "!=", constOperand);
        var kfrmEq = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "NameOperand", name: "var1" },
            operator: "!=",
            rightOperand: { _type: "ConstOperand", "const": 56 }
        };

        var doubleKeyframeEquation = new BMA.LTLOperations.DoubleKeyframeEquation(constOperand, "<", nameOperand, "<=", constOperand);
        var dblKfrmEq = {
            _type: "DoubleKeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            leftOperator: "<",
            middleOperand: { _type: "NameOperand", name: "var1" },
            rightOperator: "<=",
            rightOperand: { _type: "ConstOperand", "const": 56 },
        }

        var operands = [keyframeEquation, doubleKeyframeEquation, keyframeEquation];

        var keyframe = new BMA.LTLOperations.Keyframe("state", "state A", operands);

        var resultKeyframe = {
            _type: "Keyframe",
            name: "state",
        };

        var operation1 = new BMA.LTLOperations.Operation();
        operation1.Operands = [keyframe];
        operation1.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");

        var resultOP1 = {
            _type: "Operation",
            operator: {
                name: operation1.Operator.Name,
                operandsCount: operation1.Operator.OperandsCount
            },
            operands: [{
                _type: "Keyframe",
                name: "state"
            }]
        };

        var operation2 = new BMA.LTLOperations.Operation();
        operation2.Operands = [keyframe];
        operation2.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");

        var resultOP2 = {
            _type: "Operation",
            operator: {
                name: operation2.Operator.Name,
                operandsCount: operation2.Operator.OperandsCount
            },
            operands: [{
                _type: "Keyframe",
                name: "state"
            }]
        };

        var operation = new BMA.LTLOperations.Operation();
        operation.Operands = [operation1, operation2];
        operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");

        var resultOP = {
            _type: "Operation",
            operator: {
                name: operation.Operator.Name,
                operandsCount: operation.Operator.OperandsCount
            },
            operands: [resultOP1, resultOP2]
        };

        var result = {
            states: [],
            operations: [resultOP]
        };

        expect(BMA.Model.ExportLTLContents(undefined, [operation])).toEqual(result);

        operation1.Operands = [new BMA.LTLOperations.Keyframe("state", undefined, [])];
        operation2.Operands = [new BMA.LTLOperations.Keyframe("state", undefined, [])];

        expect(BMA.Model.ImportLTLContents(result)).toEqual({ states: undefined, operations: [operation] });
    });
});