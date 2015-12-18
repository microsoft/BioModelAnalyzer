describe("Export/Import tests", () => {

    //Export state block

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
    it("Export and import state: keyframe equation", () => {
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
        result = {
            _type: "KeyframeEquation",
            leftOperand: { _type: "ConstOperand", "const": 56 },
            operator: "=",
            rightOperand: { _type: "NameOperand", name: "var1" }
        };
        expect(BMA.Model.ExportState(keyframe)).toEqual(result);
        expect(BMA.Model.ImportOperand(result, undefined)).toEqual(keyframe);
    });

    it("Export and import state: keyframe equation, null operand", () => {
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
    });

    it("Export and import state: keyframe equation, null operand", () => {
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
    });

    //Export operation

    //ExportLTLContents

    //ImportLTLContents

    //ImportOperand

});