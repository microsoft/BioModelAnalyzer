/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model\biomodel.ts"/>
/// <reference path="script\model\model.ts"/>
/// <reference path="script\commands.ts"/>
/// <reference path="script\elementsregistry.ts"/>
/// <reference path="script\functionsregistry.ts"/>
/// <reference path="script\uidrivers.interfaces.ts"/>
/// <reference path="script\uidrivers.ts"/>
/// <reference path="script\presenters\presenters.ts"/>
/// <reference path="script\SVGHelper.ts"/>
/// <reference path="script\widgets\drawingsurface.ts"/>
/// <reference path="script\widgets\simulationplotviewer.ts"/>
/// <reference path="script\widgets\simulationviewer.ts"/>
/// <reference path="script\widgets\accordeon.ts"/>
/// <reference path="script\widgets\visibilitysettings.ts"/>
/// <reference path="script\widgets\elementbutton.ts"/>
/// <reference path="script\widgets\bmaslider.ts"/>
/// <reference path="script\widgets\variablesOptionsEditor.ts"/>
/// <reference path="script\widgets\proofresultviewer.ts"/>
/// <reference path="script\widgets\resultswindowviewer.ts"/>
/// <reference path="script\widgets\coloredtableviewer.ts"/>

$(document).ready(function () {
    //Creating CommandRegistry
    window.Commands = new BMA.CommandRegistry();

    //Creating ElementsRegistry
    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    //Creating FunctionsRegistry
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();

    //Creating model and layout
    var appModel = new BMA.Model.AppModel();

    //Loading widgets
    var drawingSurface = $("#drawingSurface");
    drawingSurface.drawingsurface();
    $("#zoomslider").bmazoomslider();

    //$("#modelToolbarHeader").toolbarpanel();
    //$("#modelToolbarContent").toolbarpanel();
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
        menu: [
            { title: "Copy", cmd: "copy", uiIcon: "ui-icon-copy" },
            { title: "----" },
            {
                title: "More", children: [
                    { title: "Sub 1", cmd: "sub1" },
                    { title: "Sub 2", cmd: "sub1" }
                ]
            }
        ],
        select: function (event, ui) {
            alert("select " + ui.cmd + " on " + ui.target.text());
        }
    });

    var data = [];
    data[0] = [1, 2, 3, 3, 3, 3, 2, 2, 2, 1];
    data[1] = [2, 2, 2, 2, 2, 2, 1, 1, 1, 0];

    $("#Div2").simulationplotviewer({ data: data });

    $("#analytics").bmaaccordion({ position: "right" });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon2", val: true } });

    //$("#icon1").click();
    //setTimeout(function () { $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } }) }, 2000);
    //Preparing elements panel
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

    //undo/redo panel
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
    $("#tabs-1").proofresultviewer();
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

    //Loading Drivers
    var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
    var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
    var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
    var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("#editor"));
    var proofViewer = new BMA.UIDrivers.ProofViewer($("#analytics"), $("#tabs-1"));
    var popupDriver = new BMA.UIDrivers.PopupDriver(popup);
    var fileLoaderDriver = new BMA.UIDrivers.ModelFileLoader($("#fileLoader"));

    window.Commands.On("ZoomSliderChanged", function (args) {
        svgPlotDriver.SetZoom(args.value);
    });

    //Loading presenters
    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, svgPlotDriver, svgPlotDriver, undoDriver, redoDriver, variableEditorDriver);
    var proofPresenter = new BMA.Presenters.ProofPresenter(appModel, proofViewer, popupDriver);
    var storagePresenter = new BMA.Presenters.ModelStoragePresenter(appModel, fileLoaderDriver);
});
//# sourceMappingURL=app.js.map
