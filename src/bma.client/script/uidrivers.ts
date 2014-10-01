/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="widgets\drawingsurface.ts"/>

module BMA {
    export module UIDrivers {
        export class SVGPlotDriver implements ISVGPlot, IElementsPanel, INavigationPanel {
            private svgPlotDiv: JQuery;

            constructor(svgPlotDiv: JQuery) {
                this.svgPlotDiv = svgPlotDiv;
            }

            public Draw(svg: SVGElement) {
                this.svgPlotDiv.drawingsurface({ svg: svg });
            }

            public TurnNavigation(isOn: boolean) {
                this.svgPlotDiv.drawingsurface({ isNavigationEnabled: isOn });
            }

            public SetGrid(x0: number, y0: number, xStep: number, yStep: number) {
                this.svgPlotDiv.drawingsurface({ grid: { x0: x0, y0: y0, xStep: xStep, yStep: yStep } });
            }

            public GetDragSubject() {
                return this.svgPlotDiv.drawingsurface("getDragSubject");
            }

            public GetZoomSubject() {
                return this.svgPlotDiv.drawingsurface("getZoomSubject");
            }

            public SetZoom(zoom: number) {
                this.svgPlotDiv.drawingsurface({ zoom: zoom });
            }

            public GetPlotX(left: number) {
                return this.svgPlotDiv.drawingsurface("getPlotX", left);
            }

            public GetPlotY(top: number) {
                return this.svgPlotDiv.drawingsurface("getPlotY", top);
            }

            public GetPixelWidth() {
                return this.svgPlotDiv.drawingsurface("getPixelWidth");
            }

            public SetGridVisibility(isOn: boolean) {
                this.svgPlotDiv.drawingsurface({ gridVisibility: isOn });
            }
        }

        export class TurnableButtonDriver implements ITurnableButton {
            private button: JQuery;

            constructor(button: JQuery) {
                this.button = button;
            }

            public Turn(isOn: boolean) {
                this.button.button("option", "disabled", !isOn);
            }

        }

        export class VariableEditorDriver implements IVariableEditor {
            private variableEditor: JQuery;

            constructor(variableEditor: JQuery) {
                this.variableEditor = variableEditor;
                this.variableEditor.bmaeditor();
                this.variableEditor.hide();

                this.variableEditor.click(function (e) { e.stopPropagation(); });
            }

            public GetVariableProperties(): { name: string; formula: string; rangeFrom: number; rangeTo: number } {
                return {
                    name: this.variableEditor.bmaeditor('option', 'name'),
                    formula: this.variableEditor.bmaeditor('option', 'formula'),
                    rangeFrom: this.variableEditor.bmaeditor('option', 'rangeFrom'),
                    rangeTo: this.variableEditor.bmaeditor('option', 'rangeTo')
                };
            }

            public SetValidation(val: boolean, message: string) {
                this.variableEditor.bmaeditor("SetValidation", val, message);
            }

            public Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) {
                this.variableEditor.bmaeditor('option', 'name', variable.Name);
                this.variableEditor.bmaeditor('option', 'formula', variable.Formula);
                this.variableEditor.bmaeditor('option', 'rangeFrom', variable.RangeFrom);
                this.variableEditor.bmaeditor('option', 'rangeTo', variable.RangeTo);

                var options = [];
                var id = variable.Id;
                for (var i = 0; i < model.Relationships.length; i++) {
                    var rel = model.Relationships[i];
                    if (rel.ToVariableId === id) {
                        options.push(model.GetVariableById(rel.FromVariableId).Name);
                    }
                }
                this.variableEditor.bmaeditor('option', 'inputs', options);
            }

            public Show(x: number, y: number) {
                this.variableEditor.show();
            }

            public Hide() {
                this.variableEditor.hide();
            }
        }

        export class ProofViewer implements IProofResultViewer {
            private proofAccordion: JQuery;
            private proofContentViewer: JQuery;

            constructor(proofAccordion, proofContentViewer) {
                this.proofAccordion = proofAccordion;
                this.proofContentViewer = proofContentViewer;
            }

            public SetData(params) {
                this.proofContentViewer.proofresultviewer({ issucceeded: params.issucceeded, time: params.time, data: params.data });
            }

            public ShowResult(result: BMA.Model.ProofResult) {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } });
            }

            public OnProofStarted() {
                this.proofAccordion.bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
            }

            public OnProofFailed() {
                $("#icon1").click();
            }

            public Show(params: any) {
                this.proofContentViewer.proofresultviewer("show", params.tab);
            }


            public Hide(params) {
                this.proofContentViewer.proofresultviewer("hide", params.tab);
            }

        }

        export class FurtherTestingDriver implements IFurtherTesting {

            private viewer: JQuery;

            constructor(viewer: JQuery, toggler: JQuery) {
                this.viewer = viewer;
            }

            public GetViewer() {
                return this.viewer;
            }

            public ShowStartToggler() {
                this.viewer.furthertesting("ShowStartToggler");
            }

            public HideStartToggler() {
                this.viewer.furthertesting("HideStartToggler");
            }

            public ShowResults(data) {
                this.viewer.furthertesting({ data: data });
                //var content = $('<div></div>')
                //    .addClass("scrollable-results")
                //    .coloredtableviewer({ numericData: data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                //this.results.resultswindowviewer({header: "Further Testing", content: content, icon: "max"})
            }
            
            public HideResults() {
                this.viewer.furthertesting({data: undefined});
                //this.results.resultswindowviewer("destroy");
            }
        }


        export class PopupDriver implements IPopup {
            private popupWindow;
            constructor(popupWindow) {
                this.popupWindow = popupWindow;
            }

            public Show(params: any) {
                var that = this;
                //this.createResultView(params);
                var header = "";
                switch (params.tab) {
                    case "ProofVariables": 
                        header = "Variables";
                        break;
                    case "ProofPropagation":
                        header = "Proof Progression";
                        break;
                    case "SimulationVariables":
                        header = "Simulation Progression";
                        break;
                    case "FurtherTesting": 
                        header = "Further Testing";
                        break;
                }
                this.popupWindow.resultswindowviewer({ header: header, tabid: params.tab, content: params.content, icon: "min" });
                this.popupWindow.show();
            }

            public Hide() {
                this.popupWindow.hide();
            }

            //private createResultView(params) {
            //    if (params.type === "coloredTable") {
            //    }
            //}
        }

        export class SimulationExpandedDriver implements ISimulationExpanded {
            private viewer;

            constructor(view: JQuery) {
                this.viewer = view;
            }

            public Set(data: { variables; colors; init }) {
                var table = this.CreateExpandedTable(data.variables, data.colors);
                var interval = this.CreateInterval(data.variables);
                var toAdd = this.CreatePlotView(data.colors);
                this.viewer.simulationexpanded({ variables: table, init: data.init, interval: interval, data: toAdd });
            }

            public GetViewer(): JQuery {
                return this.viewer;
            }

            public AddResult(res) {
                var result = this.ConvertResult(res);
                this.viewer.simulationexpanded("AddResult", result);
            }

            public CreatePlotView(colors) {
                var data = [];
                for (var i = 0; i < colors[0].Plot.length; i++) {
                    data[i] = []; //= colors[i].Plot;
                    for (var j = 0; j < colors.length; j++) {
                        data[i][j] = colors[j].Plot[i];
                    }
                }
                return data;
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

            public ConvertResult(res) {

                var data = [];
                if (res.Variables !== undefined && res.Variables !== null)
                    data = [];
                for (var i = 0; i < res.Variables.length; i++)
                    data[i] = res.Variables[i].Value;
                return data;
            }

            public findColorById(colors, id) {
                for (var i = 0; i < colors.length; i++)
                    if (id === colors[i].Id)
                        return colors[i];
                return undefined;
            }

            public CreateExpandedTable(variables,colors) {
                var table = [];
                //var variables = this.appModel.BioModel.Variables;
                for (var i = 0; i < variables.length; i++) {
                    table[i] = [];
                    table[i][0] = this.findColorById(colors, variables[i].Id).Color;
                    table[i][1] = this.findColorById(colors, variables[i].Id).Seen;
                    table[i][2] = variables[i].Name;
                    table[i][3] = variables[i].RangeFrom
                    table[i][4] = variables[i].RangeTo;
                }
                return table;
            }
        }

        export class SimulationViewerDriver implements ISimulationViewer {
            private viewer;

            constructor(viewer) {
                this.viewer = viewer;
            }

            public ChangeVisibility(param) {
                this.viewer.simulationviewer("ChangeVisibility", param.ind, param.check);
            }

            public SetData(params) {
                this.viewer.simulationviewer(params);//{ data: params.data, plot: params.plot });
            }

            public Show(params: any) {
                this.viewer.simulationviewer("show", params.tab);
            }

            public Hide(params) {
                this.viewer.simulationviewer("hide", params.tab);
            }
        }

        export class ModelFileLoader implements IFileLoader {
            private fileInput: JQuery;
            private currentPromise = undefined;

            constructor(fileInput: JQuery) {
                var that = this;
                this.fileInput = fileInput;

                fileInput.change(function (arg) {
                    var e: any = arg;
                    if (e.target.files !== undefined && e.target.files.length == 1 && that.currentPromise !== undefined) {
                        that.currentPromise.resolve(e.target.files[0]);
                        that.currentPromise = undefined;
                        fileInput.val("");
                    }
                });
            }

            public OpenFileDialog(): JQueryPromise<File> {
                var deferred = $.Deferred();
                this.currentPromise = deferred;
                this.fileInput.click();
                return deferred.promise();
            }

            private OnCheckFileSelected(): boolean {
                return false;
            }
        }

        export class ContextMenuDriver implements IContextMenu {
            private contextMenu: JQuery;

            constructor(contextMenu: JQuery) {
                this.contextMenu = contextMenu;
            }

            public EnableMenuItems(optionVisibilities: { name: string; isVisible: boolean }[]) {
                for (var i = 0; i < optionVisibilities.length; i++) {
                    this.contextMenu.contextmenu("enableEntry", optionVisibilities[i].name, optionVisibilities[i].isVisible);
                }
            }

            public GetMenuItems() {
                return [];
            }
        }
    }
} 