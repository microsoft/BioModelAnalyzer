describe("Export/Import tests", () => {

    //Export state block
    it("Export state: name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("var1");
        var result = { _type: "NameOperand", name: "var1" };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
    });
    
    it("Export state: empty name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand("");
        var result = { _type: "NameOperand", name: "" };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
    });

    it("Export state: undefined name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(undefined);
        var result = { _type: "NameOperand", name: undefined };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
    });

    it("Export state: null name operand", () => {
        var nameOperand = new BMA.LTLOperations.NameOperand(null);
        var result = { _type: "NameOperand", name: null };
        expect(BMA.Model.ExportState(nameOperand)).toEqual(result);
    });

    it("Export state: const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(56);
        var result = { _type: "ConstOperand", "const": 56 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
    });

    it("Export state: empty const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(0);
        var result = { _type: "ConstOperand", "const": 0 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
    });

    it("Export state: undefined const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(undefined);
        var result = { _type: "ConstOperand", "const": 0 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
    });

    it("Export state: null const operand", () => {
        var constOperand = new BMA.LTLOperations.ConstOperand(null);
        var result = { _type: "ConstOperand", "const": 0 };
        expect(BMA.Model.ExportState(constOperand)).toEqual(result);
    });

    //Export operation

    //ExportLTLContents

    //ImportLTLContents

    //ImportOperand

});