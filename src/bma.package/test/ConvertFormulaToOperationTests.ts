﻿describe("BMA.ModelHelper.ConvertFormulaToOperation", () => {
    window.OperatorsRegistry = new BMA.LTLOperations.OperatorsRegistry();

    var A = new BMA.LTLOperations.Keyframe("A", "", []);
    var B = new BMA.LTLOperations.Keyframe("B", "", []);
    var C = new BMA.LTLOperations.Keyframe("C", "", []);
    var oscillation = new BMA.LTLOperations.OscillationKeyframe();
    var selfloop = new BMA.LTLOperations.SelfLoopKeyframe();
    var truekeyframe = new BMA.LTLOperations.TrueKeyframe();

    var ConvertFormulaToOperation = function (formula, states) {
        var parsedFormula = BMA.parser.parse(formula);
        var operation = BMA.ModelHelper.ConvertToOperation(parsedFormula, states);
        if (operation instanceof BMA.LTLOperations.Operation) return operation;
    };
    
    describe("Should parse double ltl operations", () => {
        it("should return operation for 'A AND B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A AND B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A IMPLIES B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("IMPLIES");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A IMPLIES B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A OR B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A OR B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A UPTO B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("UPTO");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A UPTO B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A UNTIL B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("UNTIL");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A UNTIL B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A RELEASE B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("A RELEASE B", [A, B, C])).toEqual(operation);
        });
    });

    describe("Should parse single ltl operations", () => {

        it("should return operation for 'NOT B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("NOT B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'NEXT B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NEXT");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("NEXT B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'ALWAYS B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("ALWAYS");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("ALWAYS B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'EVENTUALLY B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("EVENTUALLY B", [A, B, C])).toEqual(operation);
        });
    });

    describe("Should parse ltl operations in right order", () => {

        it("should return operation for 'NOT A AND B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
            operation.Operands = [A];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation1.Operands = [operation, B]
            expect(ConvertFormulaToOperation("NOT A AND B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'A AND NOT B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
            operation.Operands = [B];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation1.Operands = [A, operation]
            expect(ConvertFormulaToOperation("A AND NOT B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'NEXT NOT B'", () => {
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
            operation1.Operands = [B]
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NEXT");
            operation.Operands = [operation1];
            expect(ConvertFormulaToOperation("NEXT NOT B", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'A UPTO C AND ALWAYS B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("ALWAYS");
            operation.Operands = [B];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("UPTO");
            operation1.Operands = [A, C];
            var operation2 = new BMA.LTLOperations.Operation();
            operation2.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation2.Operands = [operation1, operation];
            expect(ConvertFormulaToOperation("A UPTO C AND ALWAYS B", [A, B, C])).toEqual(operation2);
        });

        it("should return operation for 'A AND C UPTO ALWAYS B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("ALWAYS");
            operation.Operands = [B];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("UPTO");
            operation1.Operands = [C, operation];
            var operation2 = new BMA.LTLOperations.Operation();
            operation2.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation2.Operands = [A, operation1];
            expect(ConvertFormulaToOperation("A AND C UPTO ALWAYS B", [A, B, C])).toEqual(operation2);
        });

        it("should return operation for 'A AND C OR B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation.Operands = [A, C];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
            operation1.Operands = [operation, B];
            expect(ConvertFormulaToOperation("A AND C OR B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'A OR C AND B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation.Operands = [C, B];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
            operation1.Operands = [A, operation];
            expect(ConvertFormulaToOperation("A OR C AND B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'A UNTIL C RELEASE B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("UNTIL");
            operation.Operands = [A, C];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation1.Operands = [operation, B];
            expect(ConvertFormulaToOperation("A UNTIL C RELEASE B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'A IMPLIES C IMPLIES B'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("IMPLIES");
            operation.Operands = [C, B];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("IMPLIES");
            operation1.Operands = [A, operation];
            expect(ConvertFormulaToOperation("A IMPLIES C IMPLIES B", [A, B, C])).toEqual(operation1);
        });

        it("should return operation for 'A IMPLIES B AND C'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation.Operands = [B, C];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = window.OperatorsRegistry.GetOperatorByName("IMPLIES");
            operation1.Operands = [A, operation];
            expect(ConvertFormulaToOperation("A IMPLIES B AND C", [A, B, C])).toEqual(operation1);
        });
    });

    describe("Should throw exceptions for wrong formulas", () => {
        
        it("should throw exception for 'A AND B)'", () => {
            expect(ConvertFormulaToOperation.bind(this, "A AND B)", [A, B, C])).toThrow();
        });

        it("should throw exception for 'AND A B'", () => {
            expect(ConvertFormulaToOperation.bind(this, "and A B", [A, B, C])).toThrow();
        });

        it("should throw exception for '(AND A B'", () => {
            expect(ConvertFormulaToOperation.bind(this, "(and A B", [A, B, C])).toThrow();
        });

        it("should throw exception for '(AND A)'", () => {
            expect(ConvertFormulaToOperation.bind(this, "(and A)", [A, B, C])).toThrow();
        });

        it("should throw exception for 'NOT A B'", () => {
            expect(ConvertFormulaToOperation.bind(this, "NOT A B", [A, B, C])).toThrow();
        });

        it("should throw exception for empty string", () => {
            expect(ConvertFormulaToOperation.bind(this, "", [A, B, C])).toThrow();
        });

        it("should throw exception for undefined string", () => {
            expect(ConvertFormulaToOperation.bind(this, undefined, [A, B, C])).toThrow();
        });

        it("should throw exception for null", () => {
            expect(ConvertFormulaToOperation.bind(this, null, [A, B, C])).toThrow();
        });

    });

    describe("Should avoid wrong states in formulas", () => {

        it("should not create operation if recieve only state", () => {
            expect(ConvertFormulaToOperation("A", [A, B, C])).toEqual(undefined);
        });

        it("should return operation for 'A RELEASE D without state D'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [A, undefined];
            expect(ConvertFormulaToOperation("A RELEASE D", [A, B, C])).toEqual(operation);
        });

        it("should return operation for 'F RELEASE D without states'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [undefined, undefined];
            expect(ConvertFormulaToOperation("F RELEASE D", [A, B, C])).toEqual(operation);
        });

        it("should create operation for 'EVENTUALLY OSCILLATION'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [oscillation];
            expect(ConvertFormulaToOperation("EVENTUALLY OSCILLATION", [A, B, C])).toEqual(operation);
        });

        it("should create operation for 'EVENTUALLY SelfLoop'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [selfloop];
            expect(ConvertFormulaToOperation("EVENTUALLY SelfLoop", [A, B, C])).toEqual(operation);
        });

        it("should create operation for 'EVENTUALLY True'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [truekeyframe];
            expect(ConvertFormulaToOperation("EVENTUALLY true", [A, B, C])).toEqual(operation);
        });
    });
});