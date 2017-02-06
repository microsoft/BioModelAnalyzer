// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("LTLResultsViewerDriver", () => {

    var ltlResultsDriver: BMA.UIDrivers.LTLResultsViewer;
    var popup = $("<div></div>");
    var ltlcommands = new BMA.CommandRegistry();

    var v1 = new BMA.Model.Variable(34, 15, BMA.Model.VariableTypes.Default, "name1", 3, 7, "formula1");
    var v2 = new BMA.Model.Variable(38, 10, BMA.Model.VariableTypes.Constant, "name2", 1, 14, "formula2");
    var v3 = new BMA.Model.Variable(3, 10, BMA.Model.VariableTypes.Constant, "name3", 1, 14, "formula3");
    var v4 = new BMA.Model.Variable(8, 11, BMA.Model.VariableTypes.Constant, "name4", 1, 14, "formula4");
    var v5 = new BMA.Model.Variable(40, 10, BMA.Model.VariableTypes.Constant, "name5", 1, 14, "formula5");
    var variables = [v1, v2, v3, v4, v5];

    var ticks = [];
    for (var i = 0; i < 5; i++) {
        var vars = [];
        vars.push({ Hi: 5, Id: 34, Lo: 5 });
        vars.push({ Hi: 1, Id: 38, Lo: 1 });
        vars.push({ Hi: 1, Id: 3, Lo: 1 });
        vars.push({ Hi: 2, Id: 8, Lo: 2 });
        vars.push({ Hi: 13, Id: 40, Lo: 13 });
        ticks.push({ Time: i, Variables: vars });
    }
    var data = [[5, 1, 1, 2, 13], [5, 1, 1, 2, 13], [5, 1, 1, 2, 13], [5, 1, 1, 2, 13], [5, 1, 1, 2, 13]];

    var ticks2 = [];
    for (var i = 0; i < 5; i++) {
        var vars = [];
        vars.push({ Hi: 5, Id: 34, Lo: 5 });
        vars.push({ Hi: 1, Id: 38, Lo: 1 });
        vars.push({ Hi: 6, Id: 3, Lo: 1 });
        vars.push({ Hi: 2, Id: 8, Lo: 2 });
        vars.push({ Hi: 13, Id: 40, Lo: 13 });
        ticks2.push({ Time: i, Variables: vars });
    }
    var data2 = [[5, 1, '1 - 6', 2, 13], [5, 1, '1 - 6', 2, 13], [5, 1, '1 - 6', 2, 13], [5, 1, '1 - 6', 2, 13], [5, 1, '1 - 6', 2, 13]];

    var kfme1 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name1', 34), '=', new BMA.LTLOperations.ConstOperand(0));
    var kfm1 = new BMA.LTLOperations.Keyframe('N', "Low out", [kfme1]);

    var kfme2 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name2', 38), '=', new BMA.LTLOperations.ConstOperand(1));
    var kfm2 = new BMA.LTLOperations.Keyframe('O', 'High out', [kfme2]);

    var kfme3 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name1', 34), '=', new BMA.LTLOperations.ConstOperand(0));
    var kfme4 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name2', 38), '=', new BMA.LTLOperations.ConstOperand(1));
    var kfme5 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name3', 3), '=', new BMA.LTLOperations.ConstOperand(0));
    var kfme6 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name4', 8), '=', new BMA.LTLOperations.ConstOperand(0));
    var kfme7 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name5', 40), '=', new BMA.LTLOperations.ConstOperand(1));
    var kfm3 = new BMA.LTLOperations.Keyframe('P', '', [kfme3, kfme4, kfme5, kfme6, kfme7]);

    var kfme8 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand('name', 4), '=', new BMA.LTLOperations.ConstOperand(1));
    var kfm4 = new BMA.LTLOperations.Keyframe('C', '', [kfme8]);

    var dblkfm = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(0), "<", new BMA.LTLOperations.NameOperand('name5', 40), "<",
        new BMA.LTLOperations.ConstOperand(1));
    var kfme9 = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(1), '=', new BMA.LTLOperations.NameOperand('name5', 40));

    var states = [kfm1, kfm2, kfm3];
    var states2 = [kfm1, kfm3];

    beforeEach(() => {
        ltlResultsDriver = new BMA.UIDrivers.LTLResultsViewer(ltlcommands, popup);
    });
    


    it("CheckEquation: should check if given state satisfies given variables values", () => {
        expect(ltlResultsDriver.CheckEquation(kfme1, data[0], variables)).toEqual(false);
    
        expect(ltlResultsDriver.CheckEquation(kfme2, data[0], variables)).toEqual(true);
    
        expect(ltlResultsDriver.CheckEquation.bind(this, kfme8, data[0], variables)).toThrow("Variable was not found in model");
    
        expect(ltlResultsDriver.CheckEquation.bind(this, dblkfm, data[0], variables)).toThrow("Unknown equation type");
    
        expect(ltlResultsDriver.CheckEquation.bind(this, kfme9, data[0], variables)).toThrow("Variable are to be first in equation");

        expect(ltlResultsDriver.CheckEquation(kfme3, data2[0], variables)).toEqual(false);//if there are no number values in data, there will be thrown exception
    });
    


    it("PrepareTableData: should create init and data arrays from ticks", () => {
        expect(ltlResultsDriver.PrepareTableData(variables, ticks)).toEqual({
            init: [5, 1, 1, 2, 13],
            data: data
        });
    
        expect(ltlResultsDriver.PrepareTableData(variables, ticks2)).toEqual({
            init: [5, 1, '1 - 6', 2, 13],
            data: data2
        });
    });



    it("PrepareTableTags: should create tags for data table from received states, data and variables", () => {
        expect(ltlResultsDriver.PrepareTableTags(data, states, variables)).toEqual([['O'], ['O'], ['O'], ['O'], ['O']]);
        
        expect(ltlResultsDriver.PrepareTableTags(data, states2, variables)).toEqual([[], [], [], [], []]);

        expect(ltlResultsDriver.PrepareTableTags(data2, states, variables)).toEqual([['O'], ['O'], ['O'], ['O'], ['O']]);
    });



    it("PreparePlotLabels: should create labels from received tags", () => {
        expect(ltlResultsDriver.PreparePlotLabels([["B, C, D"], ["A, D, K"], [], [], [], ["A, D"], ["A, D"]], 2))
            .toEqual([{ text: ["B, C, D"], width: 1, height: 2, x: -0.5, y: 0 },
                { text: ["A, D, K"], width: 1, height: 2, x: 0.5, y: 0 },
                { text: ["A, D"], width: 2, height: 2, x: 4.5, y: 0 },
            ]);  

        expect(ltlResultsDriver.PreparePlotLabels([["A, D"], ["A, D, K"], ["A, D"], ["A, D"]], 2))
            .toEqual([{ text: ["A, D"], width: 1, height: 2, x: -0.5, y: 0 },
                { text: ["A, D, K"], width: 1, height: 2, x: 0.5, y: 0 },
                { text: ["A, D"], width: 2, height: 2, x: 1.5, y: 0 },
            ]);

        expect(ltlResultsDriver.PreparePlotLabels([[], [], [], [], [], []], 2)).toEqual([]);

        expect(ltlResultsDriver.PreparePlotLabels([["A, D"], ["A, D"], ["A, D"], ["A, D"]], 2))
            .toEqual([{ text: ["A, D"], width: 4, height: 2, x: -0.5, y: 0 }]);
    });
});
