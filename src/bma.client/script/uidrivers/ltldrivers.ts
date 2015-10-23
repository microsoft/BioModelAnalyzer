/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {

        export class KeyframesExpandedViewer implements IKeyframesFull {
            private keyframe: JQuery;

            constructor(keyframe: JQuery) {
                this.keyframe = keyframe;
            }

            public AddState(items) {
                //this.keyframe.ltlstatesviewer('addState', items);
            }

            public GetContent() {
                return this.keyframe;
            }

            public RemovePart(p1, p2) {
                //this.keyframe.ltlstatesviewer('removePart', items);
            }
        }


        export class LTLViewer implements ILTLViewer, IKeyframesList {

            private ltlviewer: JQuery;

            constructor(ltlviewer: JQuery) {
                this.ltlviewer = ltlviewer;
            }

            public AddState(items) {
                var resdiv = this.ltlviewer.ltlviewer('Get', 'LTLStates');
                var content = resdiv.resultswindowviewer('option', 'content');
                content.keyframecompact('add', items);
            }

            public Show(tab) {
                if (tab !== undefined) {
                    var content: JQuery = this.ltlviewer.ltlviewer('Get', tab);
                    content.show();
                }
                else {
                    this.ltlviewer.ltlviewer('Show', undefined);
                }
            }

            public Hide(tab) {
                if (tab !== undefined) {
                    var content: JQuery = this.ltlviewer.ltlviewer('Get', tab);
                    content.hide();
                }
            }

            public SetResult(res) {
                var resdiv = this.ltlviewer.ltlviewer('Get', 'LTLResults');
                var content: JQuery = resdiv.resultswindowviewer('option', 'content');
                content.coloredtableviewer({ "colorData": res, type: "color" });
                content.find(".proof-propagation-overview").addClass("ltl-result-table");
                content.find('td.propagation-cell-green').removeClass("propagation-cell-green");
                content.find('td.propagation-cell-red').removeClass("propagation-cell-red").addClass("change");
            }

            GetContent() {
                return this.ltlviewer;
            }

            GetTemporalPropertiesViewer() {
                return new BMA.UIDrivers.TemporalPropertiesViewer(this.ltlviewer.ltlviewer("GetTPViewer"));
            }

            GetStatesViewer() {
                return new BMA.UIDrivers.StatesViewerDriver(this.ltlviewer.ltlviewer("GetStatesViewer"));
            }
        }

        export class TemporalPropertiesEditorDriver implements ITemporalPropertiesEditor {
            private popupWindow: JQuery;
            private tpeditor: JQuery;
            private commands: ICommandRegistry;
            private svgDriver: SVGPlotDriver;
            private contextMenuDriver: IContextMenu;
            private statesToSet = [];

            constructor(commands: ICommandRegistry, popupWindow: JQuery) {
                this.popupWindow = popupWindow;
                this.commands = commands;
            }

            public Show() {
                var shouldInit = this.tpeditor === undefined;
                if (shouldInit) {
                    this.tpeditor = $("<div></div>").width(800);
                }

                this.popupWindow.resultswindowviewer({ header: "", tabid: "", content: this.tpeditor, icon: "min" });
                popup_position();
                this.popupWindow.show();

                if (shouldInit) {
                    this.tpeditor.temporalpropertieseditor({ commands: this.commands, states: this.statesToSet });
                    this.svgDriver = new BMA.UIDrivers.SVGPlotDriver(this.tpeditor.temporalpropertieseditor("getDrawingSurface"));
                    this.svgDriver.SetGridVisibility(false);

                    this.contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver(this.tpeditor.temporalpropertieseditor("getContextMenuPanel"));
                }
            }

            Hide() {
                this.popupWindow.hide();
            }

            GetSVGDriver(): ISVGPlot {
                return this.svgDriver;
            }

            GetNavigationDriver(): INavigationPanel {
                return this.svgDriver;
            }

            GetDragService(): IElementsPanel {
                return this.svgDriver;
            }

            GetContextMenuDriver(): IContextMenu {
                return this.contextMenuDriver;
            }

            HighlightCopyZone(ishighlighted: boolean) {
                this.tpeditor.temporalpropertieseditor("highlightcopyzone", ishighlighted);
            }

            HighlightDeleteZone(ishighlighted: boolean) {
                this.tpeditor.temporalpropertieseditor("highlightdeletezone", ishighlighted);
            }

            GetCopyZoneBBox() {
                return this.tpeditor.temporalpropertieseditor("getcopyzonebbox");
            }

            GetDeleteZoneBBox() {
                return this.tpeditor.temporalpropertieseditor("getdeletezonebbox");
            }

            SetCopyZoneVisibility(isVisible: boolean) {
                this.tpeditor.temporalpropertieseditor("setcopyzonevisibility", isVisible);
            }

            SetDeleteZoneVisibility(isVisible: boolean) {
                this.tpeditor.temporalpropertieseditor("setdeletezonevisibility", isVisible);
            }

            SetStates(states: BMA.LTLOperations.Keyframe[]) {
                if (this.tpeditor !== undefined) {
                    this.tpeditor.temporalpropertieseditor({ states: states });
                } else {
                    this.statesToSet = states;
                }
            }
        }

        export class StatesViewerDriver implements IStatesViewer {
            private statesViewer: JQuery;

            constructor(statesViewer: JQuery) {
                this.statesViewer = statesViewer;
            }

            public SetCommands(commands: BMA.CommandRegistry) {
                this.statesViewer.statescompact({ commands: commands });
            }

            public SetStates(states: BMA.LTLOperations.Keyframe[]) {
                var wstates = [];
                for (var i = 0; i < states.length; i++) {
                    var s = states[i];
                    var ws = {
                        name: s.Name,
                        description: s.Description,
                        formula: [],
                        tooltip: s.GetFormula()
                    };

                    for (var j = 0; j < s.Operands.length; j++) {
                        var opnd = s.Operands[j];
                        var formula = [];

                        formula.push({
                            type: (<any>opnd).LeftOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).LeftOperand.Name === undefined ? (<any>opnd).LeftOperand.Value : (<any>opnd).LeftOperand.Name
                        });

                        if ((<any>opnd).MiddleOperand !== undefined) {
                            var leftop = (<any>opnd).LeftOperator;
                            formula.push({
                                type: "operator",
                                value: leftop
                            });

                            var middle = (<any>opnd).MiddleOperand;
                            formula.push({
                                type: middle.Name === undefined ? "const" : "variable",
                                value: middle.Name === undefined ? middle.Value : middle.Name
                            });

                            var rightop = (<any>opnd).RightOperator;
                            formula.push({
                                type: "operator",
                                value: rightop
                            });

                        } else {
                            formula.push({
                                type: "operator",
                                value: (<any>opnd).Operator
                            });
                        }

                        formula.push({
                            type: (<any>opnd).RightOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).RightOperand.Name === undefined ? (<any>opnd).RightOperand.Value : (<any>opnd).RightOperand.Name
                        });
                        ws.formula.push(formula);
                    }

                    wstates.push(ws);
                }

                if (this.statesViewer !== undefined) {
                    this.statesViewer.statescompact({ states: wstates });
                }
            }
        }

        export class StatesEditorDriver implements IStatesEditor {
            private popupWindow: JQuery;
            private commands: ICommandRegistry;
            private statesEditor: JQuery;
            private statesToSet: BMA.LTLOperations.Keyframe[];
            private variablesToSet;

            constructor(commands: ICommandRegistry, popupWindow: JQuery) {
                this.popupWindow = popupWindow;
                this.commands = commands;
            }

            public Convert(states: any) {
                var wstates: BMA.LTLOperations.Keyframe[] = [];
                for (var i = 0; i < states.length; i++) {
                    var ops = [];
                    var formulas = states[i].formula;
                    var op = undefined;
                    var isEmpty = false;
                    for (var j = 0; j < formulas.length; j++) {
                        var op = undefined;
                        var f = formulas[j];
                        if (f[0] !== undefined && f[0].type == "variable" && f[0].value != 0) {
                            if (f[1] !== undefined && f[2] !== undefined) {
                                if (f[1].value == ">=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[0].value.variable),
                                        ">", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) - 1));
                                else if (f[1].value == "<=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[0].value.variable),
                                        "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) + 1));
                                else if (f[1].value == "=")
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[0].value.variable), "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) + 1));
                                else op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[0].value.variable),
                                    f[1].value, new BMA.LTLOperations.ConstOperand(f[2].value));
                                ops.push(op);
                            }
                        } else if (f[2] !== undefined && f[2].type == "variable" && f[2].value != 0) {
                            if (f[0] !== undefined && f[1] !== undefined && f[3] !== undefined && f[4] !== undefined) {
                                var leftConst = parseFloat(f[0].value);
                                var leftOperand = f[1].value;
                                var rightConst = parseFloat(f[4].value);
                                var rightOperand = f[3].value;
                                var leftEqual = false;
                                var rightEqual = false;
                                if (leftOperand == "<=") {
                                    leftConst--;
                                    leftOperand = "<";
                                } else if (leftOperand == ">=") {
                                    leftConst++;
                                    leftOperand = ">";
                                } else if (leftOperand == "=") {
                                    leftEqual = true;
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(leftConst - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[2].value.variable), "<", new BMA.LTLOperations.ConstOperand(leftConst + 1));
                                    ops.push(op);
                                }
                                if (rightOperand == "<=") {
                                    rightConst++;
                                    rightOperand = "<";
                                } else if (rightOperand == ">=") {
                                    rightConst--;
                                    rightOperand = ">";
                                } else if (rightOperand == "=") {
                                    rightEqual = true;
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(rightConst - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[2].value.variable), "<", new BMA.LTLOperations.ConstOperand(rightConst + 1));
                                    ops.push(op);
                                }
                                if (!leftEqual && !rightEqual) {
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(leftConst), leftOperand,
                                        new BMA.LTLOperations.NameOperand(f[2].value.variable), rightOperand, new BMA.LTLOperations.ConstOperand(rightConst));
                                    ops.push(op);
                                } else if (leftEqual && !rightEqual) {
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), rightOperand,
                                        new BMA.LTLOperations.ConstOperand(rightConst));
                                    ops.push(op);
                                } else if (rightEqual && !leftEqual) {
                                    if (leftOperand == ">")
                                        leftOperand = "<";
                                    else leftOperand = ">";
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), leftOperand,
                                        new BMA.LTLOperations.ConstOperand(leftConst));
                                    ops.push(op);
                                }
                            } else if (f[0] !== undefined && f[1] !== undefined && f[3] === undefined && f[4] === undefined) {
                                if (f[1].value == ">=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), "<",
                                        new BMA.LTLOperations.ConstOperand(parseFloat(f[0].value) + 1));
                                else if (f[1].value == "<=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), ">",
                                        new BMA.LTLOperations.ConstOperand(parseFloat(f[0].value) - 1));
                                else if (f[1].value == "=")
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(parseFloat(f[0].value) - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[2].value.variable), "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[0].value) + 1));
                                else if (f[1].value == "<")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), ">",
                                        new BMA.LTLOperations.ConstOperand(f[0].value));
                                else if (f[1].value == ">")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), "<",
                                        new BMA.LTLOperations.ConstOperand(f[0].value));
                                ops.push(op);
                            } else if (f[0] === undefined && f[1] === undefined && f[3] !== undefined && f[4] !== undefined) {
                                if (f[3].value == ">=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), ">",
                                        new BMA.LTLOperations.ConstOperand(parseFloat(f[4].value) - 1));
                                else if (f[3].value == "<=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), "<",
                                        new BMA.LTLOperations.ConstOperand(parseFloat(f[4].value) + 1));
                                else if (f[3].value == "=")
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(parseFloat(f[4].value) - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[2].value.variable), "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[4].value) + 1));
                                else op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), f[3].value,
                                    new BMA.LTLOperations.ConstOperand(f[4].value));
                                ops.push(op);
                            }
                        } else if (f[4] !== undefined && f[4].type == "variable" && f[4].value != 0) {
                            if (f[2] !== undefined && f[3] !== undefined) {
                                if (f[3].value == ">=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[4].value.variable),
                                        "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) + 1));
                                else if (f[3].value == "<=")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[4].value.variable),
                                        ">", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) - 1));
                                else if (f[3].value == "=")
                                    op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) - 1), "<",
                                        new BMA.LTLOperations.NameOperand(f[4].value.variable), "<", new BMA.LTLOperations.ConstOperand(parseFloat(f[2].value) + 1));
                                else if (f[3].value == ">")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[4].value.variable),
                                        "<", new BMA.LTLOperations.ConstOperand(f[2].value));
                                else if (f[3].value == "<")
                                    op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[4].value.variable),
                                        ">", new BMA.LTLOperations.ConstOperand(f[2].value));
                                ops.push(op);
                            }
                        }
                        if (op === undefined)
                            isEmpty = true;
                    }
                    if (formulas.length != 0 && ops.length != 0 && !isEmpty) {
                        var ws = new BMA.LTLOperations.Keyframe(states[i].name, states[i].description, ops);
                        wstates.push(ws);
                    }
                }
                return wstates;
            }

            public Show() {
                var shouldInit = this.statesEditor === undefined;
                if (shouldInit) {
                    this.statesEditor = $("<div></div>");
                }

                this.popupWindow.resultswindowviewer({ header: "States", tabid: "", content: this.statesEditor, icon: "min" });
                popup_position();
                this.popupWindow.show();

                if (shouldInit) {
                    var that = this;
                    var onStatesUpdated = function (args) {
                        var wstates = that.Convert(args.states);
                        that.commands.Execute("KeyframesChanged", { states: wstates });
                    };

                    var onComboBoxOpen = function () {
                        that.commands.Execute("UpdateStatesEditorOptions", {});
                    };

                    this.statesEditor.stateseditor({ onStatesUpdated: onStatesUpdated, onComboBoxOpen: onComboBoxOpen });

                    if (this.variablesToSet !== undefined) {
                        this.statesEditor.stateseditor({ variables: this.variablesToSet });
                        this.variablesToSet = undefined;
                    }

                    if (this.statesToSet !== undefined) {
                        this.statesEditor.stateseditor({ states: this.statesToSet });
                        this.statesToSet = undefined;
                    }


                }
            }

            public Hide() {
                this.popupWindow.hide();
            }

            public SetModel(model: BMA.Model.BioModel, layout: BMA.Model.Layout) {
                var allGroup = {
                    name: "ALL",
                    id: 0,
                    vars: []
                };

                for (var i = 0; i < model.Variables.length; i++) {
                    allGroup.vars.push(model.Variables[i].Name);
                }

                var variables = [allGroup];

                for (var i = 0; i < layout.Containers.length; i++) {
                    var vars = [];

                    for (var j = 0; j < model.Variables.length; j++) {
                        if (layout.Containers[i].Id == model.Variables[j].ContainerId)
                            vars.push(model.Variables[j].Name);
                    }

                    variables.push({
                        name: layout.Containers[i].Name,
                        id: layout.Containers[i].Id,
                        vars: vars
                    });
                }

                if (this.statesEditor !== undefined) {
                    this.statesEditor.stateseditor({ variables: variables });
                } else {
                    this.variablesToSet = variables;
                }
            }

            public SetStates(states: BMA.LTLOperations.Keyframe[]) {
                var wstates = [];
                for (var i = 0; i < states.length; i++) {
                    var s = states[i];
                    var ws = {
                        name: s.Name,
                        description: s.Description,
                        formula: []
                    };
                    for (var j = 0; j < s.Operands.length; j++) {
                        var opnd = s.Operands[j];
                        var formulaPart = [];

                        var op = {
                            type: (<any>opnd).LeftOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).LeftOperand.Name === undefined ? (<any>opnd).LeftOperand.Value : { variable: (<any>opnd).LeftOperand.Name }
                        }

                        formulaPart.push(op);

                        if ((<any>opnd).MiddleOperand !== undefined) {
                            var leftop = (<any>opnd).LeftOperator;
                            formulaPart.push({
                                type: "operator",
                                value: leftop
                            });

                            var middle = (<any>opnd).MiddleOperand;
                            formulaPart.push({
                                type: middle.Name === undefined ? "const" : "variable",
                                value: middle.Name === undefined ? middle.Value : { variable: middle.Name }
                            });

                            var rightop = (<any>opnd).RightOperator;
                            formulaPart.push({
                                type: "operator",
                                value: rightop
                            });

                        } else {
                            formulaPart.push({
                                type: "operator",
                                value: (<any>opnd).Operator
                            });
                        }

                        formulaPart.push({
                            type: (<any>opnd).RightOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).RightOperand.Name === undefined ? (<any>opnd).RightOperand.Value : { variable: (<any>opnd).RightOperand.Name }
                        });

                        ws.formula.push(formulaPart);
                    }

                    wstates.push(ws);
                }

                if (this.statesEditor !== undefined) {
                    this.statesEditor.stateseditor({ states: wstates });
                } else {
                    this.statesToSet = wstates;
                }
            }
        }

        export class TemporalPropertiesViewer implements ITemporalPropertiesViewer {
            private tpviewer: JQuery;
            constructor(tpviewer: JQuery) {
                this.tpviewer = tpviewer;
            }

            public SetOperations(operations: { operation: BMA.LTLOperations.IOperand[]; status: string }) {
                this.tpviewer.temporalpropertiesviewer({ operations: operations });
            }
        }

        export class LTLResultsViewerFactory implements ILTLResultsViewerFactory {
            constructor() {
            }

            CreateCompactLTLViewer(div: JQuery) {
                return new LTLResultsCompactViewer(div);
            }
        }

        export class LTLResultsCompactViewer implements ICompactLTLResultsViewer {
            private compactltlresult: JQuery = undefined;
            private steps: number = 10;
            private ltlrequested;
            private expandedcallback;
            private showresultcallback;

            constructor(compactltlresult: JQuery) {
                var that = this;

                this.compactltlresult = compactltlresult;
                this.compactltlresult.compactltlresult({
                    status: "nottested",
                    isexpanded: false,
                    ontestrequested: function () {
                        if (that.ltlrequested !== undefined)
                            that.ltlrequested();
                    },
                    onstepschanged: function (steps) {
                        that.steps = steps;
                    },
                    onexpanded: function () {
                        if (that.expandedcallback !== undefined) {
                            that.expandedcallback();
                        }
                    },
                    onshowresultsrequested: function () {
                        if (that.showresultcallback !== undefined) {
                            that.showresultcallback();
                        }
                    }
                });
            }

            public SetStatus(status: string) {
                this.compactltlresult.compactltlresult({ status: status, isexpanded: false });
            }

            public GetSteps(): number {
                return this.steps;
            }

            public SetLTLRequestedCallback(callback) {
                this.ltlrequested = callback;
            }

            public SetOnExpandedCallback(callback) {
                this.expandedcallback = callback;
            }

            public SetShowResultsCallback(callback) {
                this.showresultcallback = callback;
            }
        }

        export class LTLResultsViewer implements ILTLResultsViewer {
            private popupWindow: JQuery;
            private commands: ICommandRegistry;
            private ltlResultsViewer: JQuery;

            private exportCSVcallback = undefined;

            private dataToSet = undefined;

            constructor(commands: ICommandRegistry, popupWindow: JQuery) {
                this.popupWindow = popupWindow;
                this.commands = commands;
            }

            public Show() {
                var that = this;

                var shouldInit = this.ltlResultsViewer === undefined;
                if (shouldInit) {
                    this.ltlResultsViewer = $("<div></div>");
                }

                this.popupWindow.resultswindowviewer({ header: "LTL Simulation", tabid: "", content: this.ltlResultsViewer, icon: "min" });
                popup_position();
                this.popupWindow.show();

                if (shouldInit) {
                    if (this.dataToSet !== undefined) {
                        this.ltlResultsViewer.ltlresultsviewer(this.dataToSet);
                        this.dataToSet = undefined;
                    } else {
                        this.ltlResultsViewer.ltlresultsviewer();
                    }

                    if (this.exportCSVcallback !== undefined) {
                        this.ltlResultsViewer.ltlresultsviewer({ onExportCSV: that.exportCSVcallback });
                        this.exportCSVcallback = undefined;
                    }
                }
            }

            public Hide() {
                this.popupWindow.hide();
            }

            private Compare(value1: number, value2: number, operator: string): boolean {
                switch (operator) {
                    case "<":
                        return value1 < value2;
                    case "<=":
                        return value1 <= value2;
                    case ">":
                        return value1 > value2;
                    case ">=":
                        return value1 >= value2;
                    case "=":
                        return value1 == value2;
                    default:
                        throw "Unknown operator";
                }
            }

            public SetData(model: BMA.Model.BioModel, layout: BMA.Model.Layout, ticks: any, states: BMA.LTLOperations.Keyframe[]) {
                var that = this;

                var vars = model.Variables.sort((x, y) => {
                    return x.Id < y.Id ? -1 : 1;
                });

                var id = [];
                var init = [];
                var data = [];
                var pData = [];
                var ranges = [];
                var variables = [];
                var tags = [];

                for (var i = 0; i < vars.length; i++) {
                    id.push(vars[i].Id);
                    ranges.push({
                        min: vars[i].RangeFrom,
                        max: vars[i].RangeTo
                    });

                    var color = this.getRandomColor();
                    variables.push([color, true, vars[i].Name, vars[i].RangeFrom, vars[i].RangeTo]);
                }

                ticks = ticks.sort((x, y) => {
                    return x.Time < y.Time ? -1 : 1;
                });

                for (var i = 0; i < ticks.length; i++) {
                    var tick = ticks[i].Variables;
                    if (i != 0) {
                        data.push([]);
                        tags.push([]);
                    }
                    for (var k = 0; k < vars.length; k++) {
                        for (var j = 0; j < tick.length; j++) {
                            if (tick[j].Id == vars[k].Id) {
                                var ij = tick[j];
                                if (ij.Lo === ij.Hi) {
                                    (i == 0) ? init.push(ij.Lo) : data[i - 1].push(ij.Lo);
                                }
                                else {
                                    (i == 0) ? init.push(ij.Lo + ' - ' + ij.Hi) : data[i - 1].push(ij.Lo + ' - ' + ij.Hi);
                                }
                            }
                        }
                    }
                }

                for (var i = 0; i < states.length; i++) {
                    var state = states[i];
                    for (var k = 0; k < data.length; k++) {
                        //var curValue = data[k][i];
                        var result = true;
                        for (var j = 0; j < state.Operands.length; j++) {
                            var op = state.Operands[i];
                            if (op instanceof BMA.LTLOperations.KeyframeEquation) {
                                
                                if (op.LeftOperand instanceof BMA.LTLOperations.NameOperand) {
                                    var varName = (<BMA.LTLOperations.NameOperand>op.LeftOperand).Name;
                                    var ind;
                                    for (var n = 0; n < vars.length; n++)
                                        if (vars[i].Name == varName) {
                                            ind = n;
                                            break;
                                        }
                                    var curValue = data[k][ind];
                                    var rightOp = (op.RightOperand instanceof BMA.LTLOperations.ConstOperand) ? (<BMA.LTLOperations.ConstOperand>op.RightOperand).Value :
                                        undefined;
                                    result = result && this.Compare(curValue, rightOp, op.Operator);
                                } else {
                                    var varName = (<BMA.LTLOperations.NameOperand>op.RightOperand).Name;
                                    var ind;
                                    for (var n = 0; n < vars.length; n++)
                                        if (vars[i].Name == varName) {
                                            ind = n;
                                            break;
                                        }
                                    var curValue = data[k][ind];
                                    var leftOp = (op.LeftOperand instanceof BMA.LTLOperations.ConstOperand) ? (<BMA.LTLOperations.ConstOperand>op.LeftOperand).Value :
                                        undefined;
                                    result = result && this.Compare(leftOp, curValue, op.Operator);
                                }
                            } else if (op instanceof BMA.LTLOperations.DoubleKeyframeEquation) {
                                var varName = (<BMA.LTLOperations.NameOperand>op.MiddleOperand).Name;
                                var ind;
                                for (var n = 0; n < vars.length; n++)
                                    if (vars[i].Name == varName) {
                                        ind = n;
                                        break;
                                    }
                                var curValue = data[k][ind];
                                var rightOp = (op.RightOperand instanceof BMA.LTLOperations.ConstOperand) ? (<BMA.LTLOperations.ConstOperand>op.RightOperand).Value :
                                    undefined;
                                var leftOp = (op.LeftOperand instanceof BMA.LTLOperations.ConstOperand) ? (<BMA.LTLOperations.ConstOperand>op.LeftOperand).Value :
                                    undefined;
                                result = result && this.Compare(leftOp, curValue, op.LeftOperator) && this.Compare(curValue, rightOp, op.RightOperator);
                            }
                        }
                        if (state.Operands.length !== 0 && result)
                            tags[k].push(state.Name);
                    }
                }

                var interval = this.CreateInterval(vars);

                var options = {
                    id: id,
                    interval: interval,
                    tags: tags,
                    data: data,
                    init: init,
                    variables: variables,
                };

                if (this.ltlResultsViewer !== undefined) {
                    this.ltlResultsViewer.ltlresultsviewer(options);
                } else {
                    that.dataToSet = options;
                }
            }

            public CreateInterval(variables) {
                var table = [];
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = variables[i].RangeFrom;
                    table[i][1] = variables[i].RangeTo;
                }
                return table;
            }

            public getRandomColor() {
                var r = this.GetRandomInt(0, 255);
                var g = this.GetRandomInt(0, 255);
                var b = this.GetRandomInt(0, 255);
                return "rgb(" + r + ", " + g + ", " + b + ")";
            }

            public GetRandomInt(min, max) {
                return Math.floor(Math.random() * (max - min + 1) + min);
            }

            public SetOnExportCSV(callback) {
                if (this.ltlResultsViewer !== undefined) {
                    this.ltlResultsViewer.ltlresultsviewer({ onExportCSV: callback });
                } else {
                    this.exportCSVcallback = callback;
                }
            }

        }
    }
}