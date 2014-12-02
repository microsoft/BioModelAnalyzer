/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model\biomodel.ts"/>
/// <reference path="script\model\model.ts"/>
/// <reference path="script\model\visualsettings.ts"/>
/// <reference path="script\commands.ts"/>
/// <reference path="script\elementsregistry.ts"/>
/// <reference path="script\functionsregistry.ts"/>
/// <reference path="script\localRepository.ts"/>
/// <reference path="script\uidrivers.interfaces.ts"/>
/// <reference path="script\uidrivers.ts"/>
/// <reference path="script\presenters\undoredopresenter.ts"/>
/// <reference path="script\presenters\presenters.ts"/>
/// <reference path="script\presenters\furthertestingpresenter.ts"/>
/// <reference path="script\presenters\simulationpresenter.ts"/>
/// <reference path="script\presenters\formulavalidationpresenter.ts"/>
/// <reference path="script\SVGHelper.ts"/>
/// <reference path="script\changeschecker.ts"/>
/// <reference path="script\widgets\drawingsurface.ts"/>
/// <reference path="script\widgets\simulationplot.ts"/>
/// <reference path="script\widgets\simulationviewer.ts"/>
/// <reference path="script\widgets\simulationexpanded.ts"/>
/// <reference path="script\widgets\accordeon.ts"/>
/// <reference path="script\widgets\visibilitysettings.ts"/>
/// <reference path="script\widgets\elementbutton.ts"/>
/// <reference path="script\widgets\bmaslider.ts"/>
/// <reference path="script\widgets\userdialog.ts"/>
/// <reference path="script\widgets\variablesOptionsEditor.ts"/>
/// <reference path="script\widgets\progressiontable.ts"/>
/// <reference path="script\widgets\proofresultviewer.ts"/>
/// <reference path="script\widgets\furthertestingviewer.ts"/>
/// <reference path="script\widgets\localstoragewidget.ts"/>
/// <reference path="script\widgets\resultswindowviewer.ts"/>
/// <reference path="script\widgets\coloredtableviewer.ts"/>
/// <reference path="script\widgets\containernameeditor.ts"/>

function onSilverlightError(sender, args) {
    var appSource = "";
    if (sender != null && sender != 0) {
        appSource = sender.getHost().Source;
    }

    var errorType = args.ErrorType;
    var iErrorCode = args.ErrorCode;

    if (errorType == "ImageError" || errorType == "MediaError") {
        return;
    }

    var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

    errMsg += "Code: " + iErrorCode + "    \n";
    errMsg += "Category: " + errorType + "       \n";
    errMsg += "Message: " + args.ErrorMessage + "     \n";

    if (errorType == "ParserError") {
        errMsg += "File: " + args.xamlFile + "     \n";
        errMsg += "Line: " + args.lineNumber + "     \n";
        errMsg += "Position: " + args.charPosition + "     \n";
    } else if (errorType == "RuntimeError") {
        if (args.lineNumber != 0) {
            errMsg += "Line: " + args.lineNumber + "     \n";
            errMsg += "Position: " + args.charPosition + "     \n";
        }
        errMsg += "MethodName: " + args.methodName + "     \n";
    }

    alert(errMsg);
}

function getSearchParameters() {
    var prmstr = window.location.search.substr(1);
    return prmstr != null && prmstr != "" ? transformToAssocArray(prmstr) : {};
}

function transformToAssocArray(prmstr) {
    var params = {};
    var prmarr = prmstr.split("&");
    for (var i = 0; i < prmarr.length; i++) {
        var tmparr = prmarr[i].split("=");
        params[tmparr[0]] = tmparr[1];
    }
    return params;
}

$(document).ready(function () {
    //Creating CommandRegistry
    window.Commands = new BMA.CommandRegistry();

    //Creating ElementsRegistry
    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    //Creating FunctionsRegistry
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();

    //Creating model and layout
    var appModel = new BMA.Model.AppModel();

    window.PlotSettings = {
        MaxWidth: 3200,
        MinWidth: 800
    };

    window.GridSettings = {
        xOrigin: 0,
        yOrigin: 0,
        xStep: 250,
        yStep: 280
    };

    //Loading widgets
    var drawingSurface = $("#drawingSurface");
    drawingSurface.drawingsurface();
    $("#zoomslider").bmazoomslider({ value: 50 });

    //$("#modelToolbarHeader").toolbarpanel();
    //$("#modelToolbarContent").toolbarpanel();
    $("#modelToolbarHeader").buttonset();
    $("#modelToolbarContent").buttonset();
    $("#modelToolbarSlider").bmaaccordion({ position: "left", z_index: 1 });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion();

    $("#modelNameEditor").val(appModel.BioModel.Name);
    $("#modelNameEditor").click(function (e) {
        e.stopPropagation();
    });
    $("#modelNameEditor").bind("input change", function () {
        appModel.BioModel.Name = $(this).val();
    });
    window.Commands.On("ModelReset", function () {
        $("#modelNameEditor").val(appModel.BioModel.Name);
    });

    $("#drawingSurceContainer").contextmenu({
        delegate: ".bma-drawingsurface",
        preventContextMenuForPopup: true,
        preventSelect: true,
        taphold: true,
        menu: [
            { title: "Cut", cmd: "Cut", uiIcon: "ui-icon-scissors" },
            { title: "Copy", cmd: "Copy", uiIcon: "ui-icon-copy" },
            { title: "Paste", cmd: "Paste", uiIcon: "ui-icon-clipboard" },
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
            ui.menu.zIndex(50);

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

    $("#analytics").bmaaccordion({ position: "right", z_index: 4 });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon2", val: true } });

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
    $("#Proof-Analysis").proofresultviewer();
    $("#Further-Testing").furthertesting();
    $("#tabs-2").simulationviewer();
    var popup = $('<div class="popup-window"></div>').appendTo('body').hide().resultswindowviewer({ icon: "min" });

    //popup.draggable({ containment: 'parent', scroll: false });
    $("#localSaveBtn").click(function (args) {
        window.Commands.Execute("LocalStorageSaveModel", undefined);
    });
    $("#localStorageBtn").click(function (args) {
        window.Commands.Execute("LocalStorageRequested", undefined);
    });

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

    //Visual Settings Presenter
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

    //window.Commands.On("Commands.ToggleIcons", function (param) {
    //    visualSettings.IconsVisibility = param;
    //});
    //window.Commands.On("Commands.IconsSize", function (param) {
    //    visualSettings.IconsSize = param;
    //});
    window.Commands.On("Commands.LineWidth", function (param) {
        visualSettings.LineWidth = param;
        window.ElementRegistry.LineWidth = param;
        window.Commands.Execute("DrawingSurfaceRefreshOutput", {});
    });

    var localStorageWidget = $('<div></div>').addClass('newWindow').appendTo('#drawingSurceContainer').localstoragewidget();

    //Loading Drivers
    var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);
    var undoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-undo"));
    var redoDriver = new BMA.UIDrivers.TurnableButtonDriver($("#button-redo"));
    var variableEditorDriver = new BMA.UIDrivers.VariableEditorDriver($("#editor"));
    var containerEditorDriver = new BMA.UIDrivers.ContainerEditorDriver($("#containerEditor"));
    var proofViewer = new BMA.UIDrivers.ProofViewer($("#analytics"), $("#Proof-Analysis"));
    var furtherTestingDriver = new BMA.UIDrivers.FurtherTestingDriver($("#Further-Testing"), undefined);
    var simulationViewer = new BMA.UIDrivers.SimulationViewerDriver($("#tabs-2"));
    var fullSimulationViewer = new BMA.UIDrivers.SimulationExpandedDriver(expandedSimulation);
    var popupDriver = new BMA.UIDrivers.PopupDriver(popup);
    var fileLoaderDriver = new BMA.UIDrivers.ModelFileLoader($("#fileLoader"));
    var contextMenuDriver = new BMA.UIDrivers.ContextMenuDriver($("#drawingSurceContainer"));
    var accordionHider = new BMA.UIDrivers.AccordionHider($("#analytics"));
    var localStorageDriver = new BMA.UIDrivers.LocalStorageDriver(localStorageWidget);

    //var ajaxServiceDriver = new BMA.UIDrivers.AjaxServiceDriver();
    var messagebox = new BMA.UIDrivers.MessageBoxDriver();

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

    window.Commands.On("DrawingSurfaceVariableEditorOpened", function () {
        popupDriver.Hide();
        accordionHider.Hide();
    });

    var localRepositoryTool = new BMA.LocalRepositoryTool(messagebox);
    var changesCheckerTool = new BMA.ChangesChecker();
    changesCheckerTool.Snapshot(appModel);

    //Loaing ServiсeDrivers
    var formulaValidationService = new BMA.UIDrivers.FormulaValidationService();
    var furtherTestingServiсe = new BMA.UIDrivers.FurtherTestingService();
    var proofAnalyzeService = new BMA.UIDrivers.ProofAnalyzeService();
    var simulationService = new BMA.UIDrivers.SimulationService();

    //Loading presenters
    var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, containerEditorDriver, contextMenuDriver);
    var proofPresenter = new BMA.Presenters.ProofPresenter(appModel, proofViewer, popupDriver, proofAnalyzeService, messagebox);
    var furtherTestingPresenter = new BMA.Presenters.FurtherTestingPresenter(appModel, furtherTestingDriver, popupDriver, furtherTestingServiсe, messagebox);
    var simulationPresenter = new BMA.Presenters.SimulationPresenter(appModel, fullSimulationViewer, simulationViewer, popupDriver, simulationService);
    var storagePresenter = new BMA.Presenters.ModelStoragePresenter(appModel, fileLoaderDriver, changesCheckerTool);
    var formulaValidationPresenter = new BMA.Presenters.FormulaValidationPresenter(variableEditorDriver, formulaValidationService);
    var localStoragePresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageDriver, localRepositoryTool, messagebox, changesCheckerTool);

    //Loading model from URL
    var params = getSearchParameters();
    if (params.Model !== undefined) {
        var s = params.Model.split('.');
        if (s[s.length - 1] == "json") {
            $.getJSON(params.Model, function (fileContent) {
                appModel.Reset(JSON.stringify(fileContent));
            });
        } else {
            $.get(params.Model, function (fileContent) {
                try  {
                    var model = BMA.ParseXmlModel(fileContent, window.GridSettings);
                    appModel.Reset2(model.Model, model.Layout);
                } catch (exc) {
                    console.log(exc);
                    appModel.Reset(fileContent);
                }
            });
        }
    }

    function popup_position() {
        var my_popup = $('.popup-window, .bma-userdialog');
        my_popup.each(function () {
            var my_popup_w = $(this).outerWidth(), my_popup_h = $(this).outerHeight(), win_w = $(window).outerWidth(), win_h = $(window).outerHeight(), popup_half_w = (win_w - my_popup_w) / 2, popup_half_h = (win_h - my_popup_h) / 2;
            if (win_w > my_popup_w) {
                my_popup.css({ 'left': popup_half_w });
            }
            if (win_w < my_popup_w) {
                my_popup.css({ 'left': 5 });
            }
            if (win_h > my_popup_h) {
                my_popup.css({ 'top': popup_half_h });
            }
            if (win_h < my_popup_h) {
                my_popup.css({ 'top': 5 });
            }
        });
    }
    $(document).ready(function () {
        popup_position();
    });
    $(window).resize(function () {
        popup_position();
    });

    window.onunload = function () {
        window.localStorage.setItem("bma", appModel.Serialize());
    };

    window.onload = function () {
        window.Commands.Execute("LocalStorageLoadModel", "bma");
    };
});
