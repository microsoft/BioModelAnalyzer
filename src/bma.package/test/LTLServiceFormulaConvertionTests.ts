describe("GetLTLServiceProcessingFormula", () => {
    var operatorsRegistry = new BMA.LTLOperations.OperatorsRegistry();

    var nameOperand1 = new BMA.LTLOperations.NameOperand("a", "1");
    var nameOperand2 = new BMA.LTLOperations.NameOperand("b", "2");
    var nameOperand3 = new BMA.LTLOperations.NameOperand("c", "3");
    var nameOperand4 = new BMA.LTLOperations.NameOperand("d", "4");

    var constOperand1 = new BMA.LTLOperations.ConstOperand(1);
    var constOperand2 = new BMA.LTLOperations.ConstOperand(2);
    var constOperand3 = new BMA.LTLOperations.ConstOperand(3);
    var constOperand4 = new BMA.LTLOperations.ConstOperand(4);

    var keyframeQuation1 = new BMA.LTLOperations.KeyframeEquation(nameOperand1, "<", constOperand1);
    var keyframeQuation2 = new BMA.LTLOperations.KeyframeEquation(nameOperand2, "<=", constOperand2);
    var keyframeQuation3 = new BMA.LTLOperations.KeyframeEquation(nameOperand3, ">", constOperand3);
    var keyframeQuation4 = new BMA.LTLOperations.KeyframeEquation(nameOperand4, ">=", constOperand4);
    var keyframeQuation5 = new BMA.LTLOperations.KeyframeEquation(nameOperand1, "!=", constOperand2);
    var keyframeQuation4 = new BMA.LTLOperations.KeyframeEquation(nameOperand2, "=", constOperand3);

    var keyframe1 = new BMA.LTLOperations.Keyframe("A", "", [keyframeQuation1]);
    var keyframe2 = new BMA.LTLOperations.Keyframe("A", "", [keyframeQuation1, keyframeQuation2]);
    var keyframe3 = new BMA.LTLOperations.Keyframe("A", "", [keyframeQuation1, keyframeQuation2, keyframeQuation3]);

    var operation1 = new BMA.LTLOperations.Operation();
    operation1.Operator = operatorsRegistry.GetOperatorByName("AND");
    operation1.Operands = [keyframe1, keyframe2];

    var operation2 = new BMA.LTLOperations.Operation();
    operation2.Operator = operatorsRegistry.GetOperatorByName("NOT");
    operation2.Operands = [keyframe1];

    var operation3 = new BMA.LTLOperations.Operation();
    operation3.Operator = operatorsRegistry.GetOperatorByName("OR");
    operation3.Operands = [new BMA.LTLOperations.TrueKeyframe(), keyframe1];

    var operation31 = new BMA.LTLOperations.Operation();
    operation31.Operator = operatorsRegistry.GetOperatorByName("OR");
    operation31.Operands = [new BMA.LTLOperations.OscillationKeyframe(), keyframe2];

    var operation32 = new BMA.LTLOperations.Operation();
    operation32.Operator = operatorsRegistry.GetOperatorByName("OR");
    operation32.Operands = [new BMA.LTLOperations.SelfLoopKeyframe(), keyframe3];

    var operation4 = new BMA.LTLOperations.Operation();
    operation4.Operator = operatorsRegistry.GetOperatorByName("NOT");
    operation4.Operands = [operation2.Clone()];

    var operation5 = new BMA.LTLOperations.Operation();
    operation5.Operator = operatorsRegistry.GetOperatorByName("IMPLIES");
    operation5.Operands = [operation2.Clone(), operation1.Clone()];

    var operation6 = new BMA.LTLOperations.Operation();
    operation6.Operator = operatorsRegistry.GetOperatorByName("UNTIL");
    operation6.Operands = [operation2.Clone(), operation4.Clone()];

    it("Should convert LTL operation with 1 operand", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation2)).toEqual("(Not (< 1 1))");
    });

    it("Should convert LTL operation with 2 operands", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation1)).toEqual("(And (< 1 1) (And (< 1 1) (<= 2 2)))");
    });

    it("Should convert LTL operation with 'True' Keyframe", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation3)).toEqual("(Or True (< 1 1))");
    });

    it("Should convert LTL operation with 'SelfLoop' Keyframe", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation31)).toEqual("(Or Oscillation (And (< 1 1) (<= 2 2)))");
    });

    it("Should convert LTL operation with 'Oscillation' Keyframe", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation32)).toEqual("(Or SelfLoop (And (< 1 1) (And (<= 2 2) (> 3 3))))");
    });

    it("Should convert LTL operation with inner operation", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation4)).toEqual("(Not (Not (< 1 1)))");
    });

    it("Should convert LTL operation with 2 inner operations", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation5)).toEqual("(Implies (Not (< 1 1)) (And (< 1 1) (And (< 1 1) (<= 2 2))))");
    });

    it("Should convert LTL operation with complex inner operations", () => {
        expect(BMA.LTLOperations.GetLTLServiceProcessingFormula(operation6)).toEqual("(Until (Not (< 1 1)) (Not (Not (< 1 1))))");
    });


});