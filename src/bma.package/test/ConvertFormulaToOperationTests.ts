describe("BMA.ModelHelper.ConvertFormulaToOperation", () => {
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
        it("should return operation for '(AND A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("AND");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(and A B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(IMPLIES A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("IMPLIES");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(implies A B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(OR A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("OR");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(or A B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(UPTO A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("UPTO");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(upto A B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(UNTIL A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("UNTIL");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(UNTIL A B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(RELEASE A B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [A, B];
            expect(ConvertFormulaToOperation("(RELEASE A B)", [A, B, C])).toEqual(operation);
        });
    });

    describe("Should parse single ltl operations", () => {

        it("should return operation for '(NOT B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NOT");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("(NOT B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(NEXT B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("NEXT");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("(NEXT B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(ALWAYS B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("ALWAYS");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("(ALWAYS B)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(EVENTUALLY B)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [B];
            expect(ConvertFormulaToOperation("(EVENTUALLY B)", [A, B, C])).toEqual(operation);
        });
    });

    describe("Should throw exceptions for wrong formulas", () => {
        
        it("should throw exception for 'AND A B)'", () => {
            expect(ConvertFormulaToOperation.bind(this, "and A B)", [A, B, C])).toThrow();
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

        it("should throw exception for '(NOT A B)'", () => {
            expect(ConvertFormulaToOperation.bind(this, "(NOT A B)", [A, B, C])).toThrow();
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

        it("should return operation for '(RELEASE A D) without state D'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [A, undefined];
            expect(ConvertFormulaToOperation("(RELEASE A D)", [A, B, C])).toEqual(operation);
        });

        it("should return operation for '(RELEASE F D) without states'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("RELEASE");
            operation.Operands = [undefined, undefined];
            expect(ConvertFormulaToOperation("(RELEASE F D)", [A, B, C])).toEqual(operation);
        });

        it("should create operation for '(EVENTUALLY OSCILLATION)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [oscillation];
            expect(ConvertFormulaToOperation("(EVENTUALLY OSCILLATION)", [A, B, C])).toEqual(operation);
        });

        it("should create operation for '(EVENTUALLY SelfLoop)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [selfloop];
            expect(ConvertFormulaToOperation("(EVENTUALLY SelfLoop)", [A, B, C])).toEqual(operation);
        });

        it("should create operation for '(EVENTUALLY True)'", () => {
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = window.OperatorsRegistry.GetOperatorByName("EVENTUALLY");
            operation.Operands = [truekeyframe];
            expect(ConvertFormulaToOperation("(EVENTUALLY true)", [A, B, C])).toEqual(operation);
        });
    });
});