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
                        formula: []
                    };
                    for (var j = 0; j < s.Operands.length; j++) {
                        var opnd = s.Operands[j];

                        ws.formula.push({
                            type: (<any>opnd).LeftOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).LeftOperand.Name === undefined ? (<any>opnd).LeftOperand.Value : (<any>opnd).LeftOperand.Name
                        });

                        if ((<any>opnd).MiddleOperand !== undefined) {
                            var leftop = (<any>opnd).LeftOperator;
                            ws.formula.push({
                                type: "operator",
                                value: leftop
                            });

                            var middle = (<any>opnd).MiddleOperand;
                            ws.formula.push({
                                type: middle.Name === undefined ? "const" : "variable",
                                value: middle.Name === undefined ? middle.Value : middle.Name
                            });

                            var rightop = (<any>opnd).RightOperator;
                            ws.formula.push({
                                type: "operator",
                                value: rightop
                            });

                        } else {
                            ws.formula.push({
                                type: "operator",
                                value: (<any>opnd).Operator
                            });
                        }

                        ws.formula.push({
                            type: (<any>opnd).RightOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).RightOperand.Name === undefined ? (<any>opnd).RightOperand.Value : (<any>opnd).RightOperand.Name
                        });
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

            public Convert(args: any) {
                var states = args.states;
                var wstates = [];
                for (var i = 0; i < states.length; i++) {
                    var ops = [];
                    var formulas = states[i].formula;
                    var ws = new BMA.LTLOperations.Keyframe(states[i].name, ops);
                    var op = undefined;
                    for (var j = 0; j < formulas.length; j++) {
                        var f = formulas[j];
                        if (f[0] !== undefined && f[0] == "variable") {
                            if (f[1] !== undefined && f[2] != undefined) {
                                op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[0].value.variable),
                                    f[1].value, new BMA.LTLOperations.ConstOperand(f[2].value));
                                ops.push(op);
                            }
                        } else if (f[2] !== undefined && f[2] == "variable") {
                            if (f[0] !== undefined && f[1] !== undefined && f[3] !== undefined && f[4] !== undefined) {
                                op = new BMA.LTLOperations.DoubleKeyframeEquation(new BMA.LTLOperations.ConstOperand(f[0].value), f[1].value,
                                    new BMA.LTLOperations.NameOperand(f[2].value.variable), f[3].value, new BMA.LTLOperations.ConstOperand(f[4].value));
                                ops.push(op);
                            } else if (f[0] !== undefined && f[1] !== undefined && f[3] == undefined && f[4] == undefined) {
                                op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(f[0].value), f[1].value,
                                    new BMA.LTLOperations.NameOperand(f[2].value.variable));
                                ops.push(op);
                            } else if (f[0] == undefined && f[1] == undefined && f[3] !== undefined && f[4] !== undefined) {
                                op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.NameOperand(f[2].value.variable), f[3].value,
                                    new BMA.LTLOperations.ConstOperand(f[4].value));
                                ops.push(op);
                            }
                        } else if (f[4] !== undefined && f[4] == "variable") {
                            if (f[2] !== undefined && f[3] !== undefined) {
                                op = new BMA.LTLOperations.KeyframeEquation(new BMA.LTLOperations.ConstOperand(f[2].value),
                                    f[3].value, new BMA.LTLOperations.NameOperand(f[4].value.variable));
                                ops.push(op);
                            }
                        }
                    }
                    wstates.push(ws);
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
                        var wstates = that.Convert(args);
                        that.commands.Execute("KeyframesChanged", { states: wstates });
                    };

                    this.statesEditor.stateseditor({ onStatesUpdated: onStatesUpdated });
                    if (this.statesToSet !== undefined) {
                        this.statesEditor.stateseditor({ states: this.statesToSet });
                        this.statesToSet = undefined;
                    }
                    if (this.variablesToSet !== undefined) {
                        this.statesEditor.stateseditor({ variables: this.variablesToSet });
                        this.variablesToSet = undefined;
                    }

                }
            }

            public Hide() {
                this.popupWindow.hide();
            }

            public SetModel(model: BMA.Model.BioModel, layout: BMA.Model.Layout) {
                var allGroup = {
                    name: "ALL",
                    vars: []
                };

                for (var i = 0; i < model.Variables.length; i++) {
                    allGroup.vars.push(model.Variables[i].Name);
                }

                var variables = [ allGroup ];

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
                        formula: []
                    };
                    for (var j = 0; j < s.Operands.length; j++) {
                        var opnd = s.Operands[j];

                        ws.formula.push({
                            type: (<any>opnd).LeftOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).LeftOperand.Name === undefined ? (<any>opnd).LeftOperand.Value : (<any>opnd).LeftOperand.Name
                        });

                        if ((<any>opnd).MiddleOperand !== undefined) {
                            var leftop = (<any>opnd).LeftOperator;
                            ws.formula.push({
                                type: "operator",
                                value: leftop
                            });

                            var middle = (<any>opnd).MiddleOperand;
                            ws.formula.push({
                                type: middle.Name === undefined ? "const" : "variable",
                                value: middle.Name === undefined ? middle.Value : middle.Name
                            });

                            var rightop = (<any>opnd).RightOperator;
                            ws.formula.push({
                                type: "operator",
                                value: rightop
                            });

                        } else {
                            ws.formula.push({
                                type: "operator",
                                value: (<any>opnd).Operator
                            });
                        }

                        ws.formula.push({
                            type: (<any>opnd).RightOperand.Name === undefined ? "const" : "variable",
                            value: (<any>opnd).RightOperand.Name === undefined ? (<any>opnd).RightOperand.Value : (<any>opnd).RightOperand.Name
                        });
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

            public SetOperations(operations: BMA.LTLOperations.IOperand[]) {
                this.tpviewer.temporalpropertiesviewer({ operations: operations });
            }
        }
    }
}