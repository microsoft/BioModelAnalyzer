// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("BMA.ModelHelper.ConvertFormulaToOperationAndBack", () => {
    window.OperatorsRegistry = new BMA.LTLOperations.OperatorsRegistry();

    var a = new BMA.Model.Variable(1, 0, "Default", "a", 0, 1, "");
    var b = new BMA.Model.Variable(2, 0, "Default", "b", 0, 1, "");
    var c = new BMA.Model.Variable(3, 0, "Default", "c", 0, 1, "");
    var d = new BMA.Model.Variable(4, 0, "Default", "d", 0, 1, "");

    var variables = [a, b, c, d];

    describe("should convert target function to operation", () => {
        it("should return operation for 'avg(1,2,3) - var(c)'", () => {
            var avg = window.OperatorsRegistry.GetOperatorByName("AVG");
            var minus = window.OperatorsRegistry.GetOperatorByName("-");
            var variable = new BMA.LTLOperations.NameOperand(c.Name, c.Id);
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = avg;
            operation.Operands = [new BMA.LTLOperations.ConstOperand(1), new BMA.LTLOperations.ConstOperand(2), new BMA.LTLOperations.ConstOperand(3)];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = minus;
            operation1.Operands = [operation, variable];
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('avg(1,2,3) - var(c)', variables)).toEqual(operation1);
        });

        it("should return operation for 'avg(1,2,3) - var(c)*const(4)'", () => {
            var avg = window.OperatorsRegistry.GetOperatorByName("AVG");
            var multiply = window.OperatorsRegistry.GetOperatorByName("*");
            var minus = window.OperatorsRegistry.GetOperatorByName("-");
            var variable = new BMA.LTLOperations.NameOperand(c.Name, c.Id);
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = avg;
            operation.Operands = [new BMA.LTLOperations.ConstOperand(1), new BMA.LTLOperations.ConstOperand(2), new BMA.LTLOperations.ConstOperand(3)];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = multiply;
            operation1.Operands = [variable, new BMA.LTLOperations.ConstOperand(4)];
            var operation2 = new BMA.LTLOperations.Operation();
            operation2.Operator = minus;
            operation2.Operands = [operation, operation1];
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('avg(1,2,3) - var(c)*const(4)', variables)).toEqual(operation2);
        });

        it("should return operation for 'var(c)'", () => {
            var variable = new BMA.LTLOperations.NameOperand(c.Name, c.Id);
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('var(c)', variables)).toEqual(variable);
        });

        it("should return operation for '(var(c))'", () => {
            var variable = new BMA.LTLOperations.NameOperand(c.Name, c.Id);
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('(var(c))', variables)).toEqual(variable);
        });

        it("should return operation for 'const(2)'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('const(2)', variables)).toEqual(new BMA.LTLOperations.ConstOperand(2));
        });

        it("should return operation for '(const(2))'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('(const(2))', variables)).toEqual(new BMA.LTLOperations.ConstOperand(2));
        });

        it("should return opertion for '1 - (2*(avg(1,2,3) + var(c)/min(2, const(1), var(b))))'", () => {
            var avg = window.OperatorsRegistry.GetOperatorByName("AVG");
            var min = window.OperatorsRegistry.GetOperatorByName("MIN");
            var multiply = window.OperatorsRegistry.GetOperatorByName("*");
            var minus = window.OperatorsRegistry.GetOperatorByName("-");
            var plus = window.OperatorsRegistry.GetOperatorByName("+");
            var divide = window.OperatorsRegistry.GetOperatorByName("/");
            var variableC = new BMA.LTLOperations.NameOperand(c.Name, c.Id);
            var variableB = new BMA.LTLOperations.NameOperand(b.Name, b.Id);
            var const1 = new BMA.LTLOperations.ConstOperand(1);
            var const2 = new BMA.LTLOperations.ConstOperand(2);
            var const3 = new BMA.LTLOperations.ConstOperand(3);
            var operation = new BMA.LTLOperations.Operation();
            operation.Operator = min;
            operation.Operands = [const2, const1, variableB];
            var operation1 = new BMA.LTLOperations.Operation();
            operation1.Operator = divide;
            operation1.Operands = [variableC, operation];
            var operation2 = new BMA.LTLOperations.Operation();
            operation2.Operator = avg;
            operation2.Operands = [const1, const2, const3];
            var operation3 = new BMA.LTLOperations.Operation();
            operation3.Operator = plus;
            operation3.Operands = [operation2, operation1];
            var operation4 = new BMA.LTLOperations.Operation();
            operation4.Operator = multiply;
            operation4.Operands = [const2, operation3];
            var operation5 = new BMA.LTLOperations.Operation();
            operation5.Operator = minus;
            operation5.Operands = [const1, operation4];
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation('1 - (2*(avg(1,2,3) + var(c)/min(2, const(1), var(b))))', variables)).toEqual(operation5);
        });

        it("should return undefined for empty formula", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation("", variables)).toEqual(undefined);
        });
    });

    describe("should throw exception", () => {
        it("should throw exception for '()'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "()", variables)).toThrow();
        });

        it("should throw exception for '1 - (2*(avg(1,2,3) + var(c)/min(2, const(1), var(b)))'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "1 - (2*(avg(1,2,3) + var(c)/min(2, const(1), var(b)))", variables)).toThrow();
        });

        it("should throw exception for 'avg(1)'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "avg(1)", variables)).toThrow();
        });

        it("should throw exception for 'avg()'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "avg()", variables)).toThrow();
        });

        it("should throw exception for 'avg(1,2'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "avg(1,2", variables)).toThrow();
        });

        it("should throw exception for 'ceil(1,2)'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "ceil(1,2)", variables)).toThrow();
        });

        it("should throw exception for 'ceil()'", () => {
            expect(BMA.ModelHelper.ConvertTargetFunctionToOperation.bind(this, "ceil()", variables)).toThrow();
        });
    });
});
