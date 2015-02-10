/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="widgets\drawingsurface.ts"/>

module BMA {
    export module UIDrivers {
        export class SVGPlotDriver implements ISVGPlot, IElementsPanel, INavigationPanel, IAreaHightlighter {
            private svgPlotDiv: JQuery;

            constructor(svgPlotDiv: JQuery) {
                this.svgPlotDiv = svgPlotDiv;
            }

            public Draw(svg: SVGElement) {
                this.svgPlotDiv.drawingsurface({ svg: svg });
            }

            public DrawLayer2(svg: SVGElement) {
                this.svgPlotDiv.drawingsurface({ lightSvg: svg });
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

            //public GetZoomSubject() {
            //    return this.svgPlotDiv.drawingsurface("getZoomSubject");
            //}

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

            public HighlightAreas(areas: { x: number; y: number; width: number; height: number; fill: string }[]) {
                this.svgPlotDiv.drawingsurface({ rects: areas });
            }

            public SetCenter(x: number, y: number) {
                this.svgPlotDiv.drawingsurface("setCenter", { x: x, y: y });
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
                this.variableEditor.css("left", x).css("top", y);
            }

            public Hide() {
                this.variableEditor.hide();
            }
        }

        export class ContainerEditorDriver implements IContainerEditor {
            private containerEditor: JQuery;

            constructor(containerEditor: JQuery) {
                this.containerEditor = containerEditor;
                this.containerEditor.containernameeditor();
                this.containerEditor.hide();

                this.containerEditor.click(function (e) { e.stopPropagation(); });
            }

            public GetContainerName(): string {
                return this.containerEditor.containernameeditor('option', 'name');
            }

            Initialize(containerLayout: BMA.Model.ContainerLayout) {
                this.containerEditor.containernameeditor('option', 'name', containerLayout.Name);
            }

            public Show(x: number, y: number) {
                this.containerEditor.show();
                this.containerEditor.css("left", x).css("top", y);
            }

            public Hide() {
                this.containerEditor.hide();
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
                //if (params.issucceeded !== undefined)
                //    this.proofContentViewer.proofresultviewer({ issucceeded: params.issucceeded });
                //if (params.time !== undefined)
                //    this.proofContentViewer.proofresultviewer({ time: params.time });
                if (params !== undefined)
                    this.proofContentViewer.proofresultviewer(params);
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

            public ShowStartFurtherTestingToggler() {
                this.viewer.furthertesting("ShowStartToggler");
            }

            public HideStartFurtherTestingToggler() {
                this.viewer.furthertesting("HideStartToggler");
            }

            public ShowResults(data) {
                if (data !== undefined)
                    this.viewer.furthertesting("SetData", { tabLabels: data.tabLabels, tableHeaders: data.tableHeaders, data: data.data });
                else { this.viewer.furthertesting("SetData", undefined) }
                //var content = $('<div></div>')
                //    .addClass("scrollable-results")
                //    .coloredtableviewer({ numericData: data, header: ["Cell", "Name", "Calculated Bound", "Oscillation"] });
                //this.results.resultswindowviewer({header: "Further Testing", content: content, icon: "max"})
            }
            
            public HideResults() {
                this.viewer.furthertesting({data: null});
                //this.results.resultswindowviewer("destroy");
            }

            public StandbyMode() {
                this.viewer.furthertesting({ buttonMode: "StandbyMode" });
            }

            public ActiveMode() {
                this.viewer.furthertesting({ buttonMode: "ActiveMode" });
            }
        }


        export class PopupDriver implements IPopup {
            private popupWindow: JQuery;
            constructor(popupWindow: JQuery) {
                this.popupWindow = popupWindow;
            }

            public Seen() {
                return !this.popupWindow.is(":hidden");
            }

            public Show(params: any) {
                var that = this;
                //this.createResultView(params);
                var header = "";
                this.popupWindow
                    .removeClass('proof-propagation-popout')
                    .removeClass('proof-variables-popout')
                    .removeClass('simulation-popout');

                switch (params.tab) {
                    case "ProofVariables": 
                        header = "Variables";
                        this.popupWindow.addClass('proof-variables-popout');
                        break;
                    case "ProofPropagation":
                        header = "Proof Progression";
                        this.popupWindow.addClass('proof-propagation-popout');
                        break;
                    case "SimulationVariables":
                        header = "Simulation Progression";
                        this.popupWindow.addClass('simulation-popout');
                        break;
                    case "FurtherTesting": 
                        header = "Further Testing";
                        break;
                }
                this.popupWindow.resultswindowviewer({ header: header, tabid: params.tab, content: params.content, icon: "min" });
                this.popup_position();
                this.popupWindow.show();
            }

            private popup_position() {
                var my_popup = $('.popup-window, .bma-userdialog'); // наш попап
                my_popup.each(function () {
                    var my_popup_w = $(this).outerWidth(), // ширина попапа
                        my_popup_h = $(this).outerHeight(), // высота попапа

                        win_w = $(window).outerWidth(), // ширина окна
                        win_h = $(window).outerHeight(), // высота окна
                        popup_half_w = (win_w - my_popup_w) / 2,
                        popup_half_h = (win_h - my_popup_h) / 2;
                    if (win_w > my_popup_w) { // если ширина окна больше ширины попапа
                        my_popup.css({ 'left': popup_half_w });
                    }
                    if (win_w < my_popup_w) { // если ширина окна меньше ширины попапа                  
                        my_popup.css({ 'left': 5, });
                    }
                    if (win_h > my_popup_h) { // если высота окна больше ширины попапа
                        my_popup.css({ 'top': popup_half_h });
                    }
                    if (win_h < my_popup_h) { // если высота окна меньше ширины попапа
                        my_popup.css({ 'top': 5 });
                    }
                })
            }

            public Hide() {
                this.popupWindow.hide();
                //window.Commands.Execute("Collapse", this.popupWindow.resultswindowviewer("option", "tabid"));
            }

            public Collapse() {
                window.Commands.Execute("Collapse", this.popupWindow.resultswindowviewer("option", "tabid"));
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
                //var toAdd = this.CreatePlotView(data.colors);
                this.viewer.simulationexpanded({ variables: table, init: data.init, interval: interval, data: undefined });//, data: toAdd });
                //this.viewer.simulationexpanded("option", "data", toAdd);
            }

            public SetData(data) {
                var toAdd = this.CreatePlotView(data);
                this.viewer.simulationexpanded("option", "data", toAdd);
            }

            public GetViewer(): JQuery {
                return this.viewer;
            }

            public StandbyMode() {
                this.viewer.simulationexpanded({buttonMode: "StandbyMode"});
            }

            public ActiveMode() {
                this.viewer.simulationexpanded({buttonMode: "ActiveMode"});
            }

            public AddResult(res) {
                var result = this.ConvertResult(res);
                this.viewer.simulationexpanded("AddResult", result);
            }

            public CreatePlotView(colors) {
                var data = [];
                for (var i = 1; i < colors[0].Plot.length; i++) {
                    data[i-1] = []; //= colors[i].Plot;
                    for (var j = 0; j < colors.length; j++) {
                        data[i-1][j] = colors[j].Plot[i];
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

        export class LocalStorageDriver implements ILocalStorageDriver {
            private widget: JQuery;

            constructor(widget: JQuery) {
                this.widget = widget;
            }

            public AddItem(key, item) {
                this.widget.localstoragewidget("AddItem", key);
            }

            public Show() {
                this.widget.show();
            }

            public Hide() {
                this.widget.hide();
            }

            public SetItems(keys) {
                this.widget.localstoragewidget({ items: keys });
            }

            public Message(msg: string) {
                this.widget.localstoragewidget("Message", msg);
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

            public EnableMenuItems(optionVisibilities: { name: string; isEnabled: boolean }[]) {
                for (var i = 0; i < optionVisibilities.length; i++) {
                    this.contextMenu.contextmenu("enableEntry", optionVisibilities[i].name, optionVisibilities[i].isEnabled);
                }
            }

            public ShowMenuItems(optionVisibilities: { name: string; isVisible: boolean }[]) {
                for (var i = 0; i < optionVisibilities.length; i++) {
                    this.contextMenu.contextmenu("showEntry", optionVisibilities[i].name, optionVisibilities[i].isVisible);
                }
            }

            public GetMenuItems() {
                return [];
            }
        }

        export class AccordionHider implements IHider {
            private acc: JQuery;

            constructor(acc: JQuery) {
                this.acc = acc;
            }

            public Hide() {
                var coll = this.acc.children().filter('[aria-selected="true"]').trigger("click");
            }
        }

        export class FormulaValidationService implements IServiceDriver {
            public Invoke(data): JQueryPromise<any> {
                return $.ajax({
                    type: "POST",
                    url: "api/Validate",
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    dataType: "json"
                });
            }
        }

        export class FurtherTestingService implements IServiceDriver {
            public Invoke(data): JQueryPromise<any> {
                return $.ajax({
                    type: "POST",
                    url: "api/FurtherTesting",
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    dataType: "json"
                });
            }
        }

        export class ProofAnalyzeService implements IServiceDriver {
            public Invoke(data): JQueryPromise<any> {
                return $.ajax({
                    type: "POST",
                    url: "api/Analyze",
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    dataType: "json"
                });
            }
        }

        export class SimulationService implements IServiceDriver {
            public Invoke(data): JQueryPromise<any> {
                return $.ajax({
                    type: "POST",
                    url: "api/Simulate",
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    dataType: "json"
                });
            }
        }

        //export class AjaxServiceDriver implements IServiceDriver {
           
        //    public Invoke(url, data): JQueryPromise<any> {
        //        return $.ajax({
        //            type: "POST",
        //            url: url,
        //            data: JSON.stringify(data),
        //            contentType: "application/json",
        //            dataType: "json"
        //        });
        //    }
        //}

        export class MessageBoxDriver implements IMessageServiсe {

            public Show(message: string){
                alert(message);
            }

            public Log(message: string) {
                console.log(message);
            }
        }

        
    }
} 