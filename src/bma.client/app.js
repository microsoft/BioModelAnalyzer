
$(document).ready(function () {
    window.Commands = new BMA.CommandRegistry();

    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();

    var appModel = new BMA.Model.AppModel();

    window.PlotSettings = {
        MaxWidth: 3200,
        MinWidth: 800
    };

    var drawingSurface = $("#drawingSurface");
    drawingSurface.drawingsurface();
    $("#zoomslider").bmazoomslider({ value: 50 });

    $("#modelToolbarHeader").buttonset();
    $("#modelToolbarContent").buttonset();
    $("#modelToolbarSlider").bmaaccordion({ position: "left" });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion();

    $("#modelNameEditor").click(function (e) {
        e.stopPropagation();
    });

    $("#drawingSurceContainer").contextmenu({
        delegate: ".bma-drawingsurface",
        preventContextMenuForPopup: true,
        preventSelect: true,
        taphold: true,
        menu: [
            { title: "Delete", cmd: "Delete", uiIcon: "ui-icon-trash" },
            {
                title: "Size", cmd: "Size", children: [
                    { title: "1x1", cmd: "ResizeCellTo1x1" },
                    { title: "2x2", cmd: "ResizeCellTo2x2" },
                    { title: "3x3", cmd: "ResizeCellTo3x3" }
                ],
                uiIcon: "ui-icon-arrow-4-diag"
            },
            { title: "Edit", cmd: "Edit", uiIcon: "ui-icon-pencil" }
        ],
        beforeOpen: function (event, ui) {
            var left = event.pageX - $(".bma-drawingsurface").offset().left;
            var top = event.pageY - $(".bma-drawingsurface").offset().top;

            console.log("top " + top);
            console.log("left " + left);

            window.Commands.Execute("DrawingSurfaceContextMenuOpening", {
                left: left,
                top: top
            });
        },
        select: function (event, ui) {
            var args = {};
            var commandName = "DrawingSurface";
            if (ui.cmd === "ResizeCellTo1x1") {
                args.size = 1;
                commandName += "ResizeCell";
            } else if (ui.cmd === "ResizeCellTo2x2") {
                args.size = 2;
                commandName += "ResizeCell";
            } else if (ui.cmd === "ResizeCellTo3x3") {
                args.size = 3;
                commandName += "ResizeCell";
            } else {
                commandName += ui.cmd;
            }

            args.left = event.pageX - $(".bma-drawingsurface").offset().left;
            args.top = event.pageY - $(".bma-drawingsurface").offset().top;

            window.Commands.Execute(commandName, args);
        }
    });

    $("#analytics").bmaaccordion({ position: "right" });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon2", val: true } });

    var elementPanel = $("#modelelemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
        var elem = elements[i];
        $("<input></input>").attr("type", "radio").attr("id", "btn-" + elem.Type).attr("name", "drawing-button").attr("data-type", elem.Type).appendTo(elementPanel);

        var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
        var img = $("<img></img>").attr("src", elem.IconURL).attr("title", elem.Description).appendTo(label);
    }

    elementPanel.children("input").not('[data-type="Activator"]').not('[data-type="Inhibitor"]').next().draggable({
        helper: function (event, ui) {
            return $(this).children().clone().appendTo('body');
        },
        scroll: false,
        start: function (event, ui) {
            $(this).draggable("option", "cursorAt", {
                left: Math.floor(ui.helper.width() / 2),
                top: Math.floor(ui.helper.height() / 2)
            });
            $('#' + $(this).attr("for")).click();
        }
    });

    $("#modelelemtoolbar input").click(function (event) {
        window.Commands.Execute("AddElementSelect", $(this).attr("data-type"));
    });

    elementPanel.buttonset();

    $("#button-pointer").click(function () {
        window.Commands.Execute("AddElementSelect", undefined);
    });

    $("#undoredotoolbar").buttonset();
    $("#button-undo").click(function () {
        window.Commands.Execute("Undo", undefined);
    });
    $("#button-redo").click(function () {
        window.Commands.Execute("Redo", undefined);
    });

    $("#editor").bmaeditor();
    $("#Proof-Analysis").proofresultviewer();
    $("#Further-Testing").furthertesting();
    $("#tabs-2").simulationviewer();
    var popup = $('<div class="popup-window"></div>').appendTo('body').hide().resultswindowviewer({ icon: "min" });

    $("#newModelBtn").click(function (args) {
        window.Commands.Execute("NewModel", undefined);
    });

    $("#importModelBtn").click(function (args) {
        window.Commands.Execute("ImportModel", undefined);
    });

    $("#exportModelBtn").click(function (args) {
        window.Commands.Execute("ExportModel", undefined);
    });
    var expandedSimulation = $('<div></div>').simulationexpanded();

    var visualSettings = new BMA.Model.AppVisualSettings();

    window.Commands.On("Commands.ToggleLabels", function (param) {
        visualSettings.TextLabelVisibility = param;
        window.ElementRegistry.LabelVisibility = param;
        window.Commands.Execute("DrawingSurfaceRefreshOutput", {});
    });

    window.Commands.On("Commands.LabelsSize", function (param) {
        visualSettings.TextLabelSize = param;
        window.ElementRegistry.LabelSize = param;
        window.Commands.Execute("DrawingSurfaceRefreshOutput", {});
    });

    window.Commands.On("Commands.LineWidth", function (param) {
        visualSettings.LineWidth = param;
        window.ElementRegistry.LineWidth = param;
        window.Commands.Execute("DrawingSurfaceRefreshOutput", {});
    });

    var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
    var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
    var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
    var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("#editor"));
    var proofViewer = new BMA.UIDrivers.ProofViewer($("#analytics"), $("#Proof-Analysis"));
    var furtherTestingDriver = new BMA.UIDrivers.FurtherTestingDriver($("#Further-Testing"), undefined);
    var simulationViewer = new BMA.UIDrivers.SimulationViewerDriver($("#tabs-2"));
    var fullSimulationViewer = new BMA.UIDrivers.SimulationExpandedDriver(expandedSimulation);
    var popupDriver = new BMA.UIDrivers.PopupDriver(popup);
    var fileLoaderDriver = new BMA.UIDrivers.ModelFileLoader($("#fileLoader"));
    var contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver($("#drawingSurceContainer"));
    var accordionHider = new BMA.UIDrivers.AccordionHider($("#analytics"));

    window.Commands.On("Commands.ToggleGrid", function (param) {
        visualSettings.GridVisibility = param;
        svgPlotDriver.SetGridVisibility(param);
    });

    window.Commands.On("ZoomSliderBind", function (value) {
        $("#zoomslider").bmazoomslider({ value: value });
    });

    window.Commands.On("AppModelChanged", function () {
        popupDriver.Hide();
        accordionHider.Hide();
    });

    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, svgPlotDriver, svgPlotDriver, undoDriver, redoDriver, variableEditorDriver, contextMenuDriver);
    var proofPresenter = new BMA.Presenters.ProofPresenter(appModel, proofViewer, popupDriver);
    var furtherTestingPresenter = new BMA.Presenters.FurtherTestingPresenter(furtherTestingDriver, popupDriver);
    var simulationPresenter = new BMA.Presenters.SimulationPresenter(appModel, fullSimulationViewer, simulationViewer, popupDriver);
    var storagePresenter = new BMA.Presenters.ModelStoragePresenter(appModel, fileLoaderDriver);
    var formulaValidationPresenter = new BMA.Presenters.FormulaValidationPresenter(variableEditorDriver);
});
