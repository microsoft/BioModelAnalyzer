/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model\biomodel.ts"/>
/// <reference path="script\model\model.ts"/>
/// <reference path="script\model\exportimport.ts"/>
/// <reference path="script\model\visualsettings.ts"/>
/// <reference path="script\commands.ts"/>
/// <reference path="script\elementsregistry.ts"/>
/// <reference path="script\functionsregistry.ts"/>
/// <reference path="script\keyframesregistry.ts"/>
/// <reference path="script\operatorsregistry.ts"/>
/// <reference path="script\localRepository.ts"/>
/// <reference path="script\uidrivers\commoninterfaces.ts"/>
/// <reference path="script\uidrivers\commondrivers.ts"/>
/// <reference path="script\uidrivers\commoninterfaces.ts"/>
/// <reference path="script\uidrivers\commondrivers.ts"/>
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
/// <reference path="script\widgets\ltl\keyframecompact.ts"/>
/// <reference path="script\widgets\ltl\keyframetable.ts"/>
/// <reference path="script\widgets\ltl\ltlstatesviewer.ts"/>
/// <reference path="script\widgets\ltl\ltlviewer.ts"/>
/// <reference path="script\widgets\resultswindowviewer.ts"/>
/// <reference path="script\widgets\coloredtableviewer.ts"/>
/// <reference path="script\widgets\containernameeditor.ts"/>

declare var saveTextAs: any;
declare var Silverlight: any;
declare var drawingSurceContainer: any;

interface JQuery {
    contextmenu(): JQueryUI.Widget;
    contextmenu(settings: Object): JQueryUI.Widget;
    contextmenu(optionLiteral: string, optionName: string): any;
    contextmenu(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}

interface Window {
    PlotSettings: any;
    GridSettings: any;
}

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
    }
    else if (errorType == "RuntimeError") {
        if (args.lineNumber != 0) {
            errMsg += "Line: " + args.lineNumber + "     \n";
            errMsg += "Position: " + args.charPosition + "     \n";
        }
        errMsg += "MethodName: " + args.methodName + "     \n";
    }

    alert(errMsg);
}

function getSearchParameters(): any {
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

function popup_position() {
    var my_popup = $('.popup-window, .window.dialog');
    var analytic_tabs = $('.tab-right');
    analytic_tabs.each(function () {
        var tab_h = $(this).outerHeight();
        var win_h = $(window).outerHeight() * 0.8;
        if (win_h > tab_h)
            $(this).css({ 'max-height': win_h * 0.8 });

        else
            $(this).css({ 'max-height': '600px' });
    });

    my_popup.each(function () {
        var my_popup_w = $(this).outerWidth(),
            my_popup_h = $(this).outerHeight(),

            win_w = $(window).outerWidth(),
            win_h = $(window).outerHeight(),
            popup_half_w = (win_w - my_popup_w) / 2,
            popup_half_h = (win_h - my_popup_h) / 2;
        if (win_w > my_popup_w) {
            $(this).css({ 'left': popup_half_w });
        }
        if (win_w < my_popup_w) {
            $(this).css({ 'left': 5, });
        }
        if (win_h > my_popup_h) {
            $(this).css({ 'top': popup_half_h });
        }
        if (win_h < my_popup_h) {
            $(this).css({ 'top': 5 });
        }
    })
}

$(document).ready(function () {
    var snipper = $('<div></div>').addClass('spinner').appendTo($('.loading-text'));
    for (var i = 1; i < 4; i++) {
        $('<div></div>').addClass('bounce' + i).appendTo(snipper);
    }

    var deferredLoad = function (): JQueryPromise<{}> {
        var dfd = $.Deferred();

        loadVersion().done(function (version) {
            loadScript(version);
            window.setInterval(function () { versionCheck(version); }, 3600000 /* 1 hour */);
            dfd.resolve();

        });

        return dfd.promise();
    };

    deferredLoad().done(function () {
        $('.page-loading').detach();
    }).fail(function (err) {
        alert("Page loading failed: " + err);
    });

    $(document).ready(function () {
        popup_position();
    });

    $(window).resize(function () {
        popup_position();
        //resize_header_tools();
    });
});

function versionCheck(version) {
    loadVersion().done(function (newVersion) {
        var v = <any>newVersion;
        if (v.major !== version.major || v.minor !== version.minor || v.build !== version.build) {
            var userDialog = $('<div></div>').appendTo('body').userdialog({
                message: "BMA client was updated on server. Refresh your browser to get latest version",
                actions: [
                    {
                        button: 'Ok',
                        callback: function () { userDialog.detach(); }
                    }
                ]
            });
        } else {
            console.log("server version was succesfully checked: client is up to date");
        }
    }).fail(function (err) {
        console.log("there was an error while trying to check server version: " + err);
    });
}

function loadVersion(): JQueryPromise<Object> {
    var d = $.Deferred();
    $.ajax({
        url: "version.txt",
        dataType: "text",
        success: function (data) {
            var version = JSON.parse(data);
            d.resolve(version);
        },
        error: function (err) {
            d.reject(err);
        }
    });
    return d.promise();
}

function loadScript(version) {
    var version_key = 'bma-version';

    $('.version-number').text('v. ' + version.major + '.' + version.minor + '.' + version.build);
    //Creating CommandRegistry
    window.Commands = new BMA.CommandRegistry();
    var ltlCommands = new BMA.CommandRegistry();

    //Creating ElementsRegistry
    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    //Creating FunctionsRegistry
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
      
    //Creating KeyframesRegistry
    window.KeyframesRegistry = new BMA.Keyframes.KeyframesRegistry();
    window.OperatorsRegistry = new BMA.LTLOperations.OperatorsRegistry();
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

    var holdCords = {
        holdX: 0,
        holdY: 0
    }

    $(document).on('vmousedown', function (event) {

        holdCords.holdX = event.pageX;
        holdCords.holdY = event.pageY;
    });



    $("#drawingSurceContainer").contextmenu({
        delegate: ".bma-drawingsurface",
        autoFocus: true,
        preventContextMenuForPopup: true,
        preventSelect: true,
        taphold: true,
        menu: [
            { title: "Cut", cmd: "Cut", uiIcon: "ui-icon-scissors" },
            { title: "Copy", cmd: "Copy", uiIcon: "ui-icon-copy" },
            { title: "Paste", cmd: "Paste", uiIcon: "ui-icon-clipboard" },
            { title: "Edit", cmd: "Edit", uiIcon: "ui-icon-pencil" },

            {
                title: "Size", cmd: "Size", children: [
                    { title: "1x1", cmd: "ResizeCellTo1x1" },
                    { title: "2x2", cmd: "ResizeCellTo2x2" },
                    { title: "3x3", cmd: "ResizeCellTo3x3" },
                ],
                uiIcon: "ui-icon-arrow-4-diag"
            },
            { title: "Delete", cmd: "Delete", uiIcon: "ui-icon-trash" }

        ],
        beforeOpen: function (event, ui) {
            ui.menu.zIndex(50);
            var x = holdCords.holdX || event.pageX;
            var y = holdCords.holdX || event.pageY;
            var left = x - $(".bma-drawingsurface").offset().left;
            var top = y - $(".bma-drawingsurface").offset().top;
            //console.log("top " + top);
            //console.log("left " + left);

            window.Commands.Execute("DrawingSurfaceContextMenuOpening", {
                left: left,
                top: top
            });
        },
        select: function (event, ui) {
            var args: any = {};
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
    var contextmenu = $('body').children('ul').filter('.ui-menu');
    contextmenu.addClass('command-list window canvas-contextual');
    contextmenu.children('li').children('ul').filter('.ui-menu').addClass('command-list');
    var aas = $('body').children('ul').children('li').children('a');
    aas.children('span').detach();
    var ulsizes: JQuery;
    aas.each(function () {
        switch ($(this).text()) {
            case "Cut": $(this)[0].innerHTML = '<img alt="" src="../images/icon-cut.svg"><span>Cut</span>';
                break;
            case "Copy": $(this)[0].innerHTML = '<img alt="" src="../images/icon-copy.svg"><span>Copy</span>';
                break;
            case "Paste": $(this)[0].innerHTML = '<img alt="" src="../images/icon-paste.svg"><span>Paste</span>';
                break;
            case "Edit": $(this)[0].innerHTML = '<img alt="" src="../images/icon-edit.svg"><span>Edit</span>';
                break;
            case "Size": $(this)[0].innerHTML = '<img alt="" src="../images/icon-size.svg"><span>Size  ></span>';
                ulsizes = $(this).next('ul');
                break;
            case "Delete": $(this)[0].innerHTML = '<img alt="" src="../images/icon-delete.svg"><span>Delete</span>';
                break;
        }
    })
    if (ulsizes !== undefined)
        ulsizes.addClass('context-menu-small');
    if (asizes !== undefined) {
        var asizes = ulsizes.children('li').children('a');
        asizes.each(function (ind) {
            $(this)[0].innerHTML = '<img alt="" src="../images/' + (ind + 1) + 'x' + (ind + 1) + '.svg">';
        });
    }
    $("#analytics").bmaaccordion({ position: "right", z_index: 4 });

    //Preparing elements panel
    var elementPanel = $("#modelelemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
        var elem = elements[i];
        $("<input></input>")
            .attr("type", "radio")
            .attr("id", "btn-" + elem.Type)
            .attr("name", "drawing-button")
            .attr("data-type", elem.Type)
            .appendTo(elementPanel);

        var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
        var img = $("<div></div>").addClass(elem.IconClass).attr("title", elem.Description).appendTo(label);
    }

    elementPanel.children("input").not('[data-type="Activator"]').not('[data-type="Inhibitor"]').next().draggable({

        helper: function (event, ui) {
            var classes = $(this).children().children().attr("class").split(" ");
            return $('<div></div>').addClass(classes[0]).addClass("draggable-helper-element").appendTo('body');
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
    $("#button-undo").click(() => { window.Commands.Execute("Undo", undefined); });
    $("#button-redo").click(() => { window.Commands.Execute("Redo", undefined); });

    $("#btn-local-save").click(function (args) {
        window.Commands.Execute("LocalStorageSaveModel", undefined);
    });
    $("#btn-new-model").click(function (args) {
        window.Commands.Execute("NewModel", undefined);
    });
    $("#btn-local-storage").click(function (args) {
        window.Commands.Execute("LocalStorageRequested", undefined);
    });
    $("#btn-import-model").click(function (args) {
        window.Commands.Execute("ImportModel", undefined);
    });

    $("#btn-export-model").click(function (args) {
        window.Commands.Execute("ExportModel", undefined);
    });

    var localStorageWidget = $('<div></div>')
        .addClass('window')
        .appendTo('#drawingSurceContainer')
        .localstoragewidget();

    $("#editor").bmaeditor();

    $("#Proof-Analysis").proofresultviewer();
    $("#Further-Testing").furthertesting();
    $("#tabs-2").simulationviewer();
    $('#tabs-3').ltlviewer();
    var popup = $('<div></div>')
        .addClass('popup-window window')
        .appendTo('body')
        .hide()
        .resultswindowviewer({ icon: "min" });
    popup.draggable({ scroll: false });

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

    window.Commands.On("Commands.ToggleGrid", function (param) {
        visualSettings.GridVisibility = param;
        svgPlotDriver.SetGridVisibility(param);
    });

    window.Commands.On("ZoomSliderBind",(value) => {
        $("#zoomslider").bmazoomslider({ value: value });
    });

    //window.Commands.On('ZoomConfigure',(value: { min; max }) => {
    //    $("#zoomslider").bmazoomslider({ min: value.min, max: value.max });
    //});

    window.Commands.On('SetPlotSettings',(value) => {

        if (value.MaxWidth !== undefined) {
            window.PlotSettings.MaxWidth = value.MaxWidth;
            $("#zoomslider").bmazoomslider({ max: (value.MaxWidth - window.PlotSettings.MinWidth) / 24 });
        }
        if (value.MinWidth !== undefined) {
            window.PlotSettings.MinWidth = value.MinWidth;
        }
    });

    window.Commands.On("AppModelChanged",() => {
        if (changesCheckerTool.IsChanged) {
            popupDriver.Hide();
            accordionHider.Hide();
            window.Commands.Execute("Expand", '');
        }
    });

    window.Commands.On("DrawingSurfaceVariableEditorOpened",() => {
        popupDriver.Hide();
        accordionHider.Hide();
    });


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
    //var keyframecompactDriver = new BMA.UIDrivers.KeyframesList($('#tabs-3').find('.keyframe-compact'));
    var ltlDriver = new BMA.UIDrivers.LTLViewer($('#tabs-3'));
    var localRepositoryTool = new BMA.LocalRepositoryTool(messagebox);
    var changesCheckerTool = new BMA.ChangesChecker();
    changesCheckerTool.Snapshot(appModel);

    //LTL Drivers
    var tpeditordriver = new BMA.UIDrivers.TemporalPropertiesEditorDriver(ltlCommands, popup);
    var stateseditordriver = new BMA.UIDrivers.StatesEditorDriver(ltlCommands, popup);

    //Loaing ServiсeDrivers 
    var exportService = new BMA.UIDrivers.ExportService();
    var formulaValidationService = new BMA.UIDrivers.FormulaValidationService();
    var furtherTestingServiсe = new BMA.UIDrivers.FurtherTestingService();
    var proofAnalyzeService = new BMA.UIDrivers.ProofAnalyzeService();
    var simulationService = new BMA.UIDrivers.SimulationService();
    var logService = new BMA.SessionLog();
    var ltlService = new BMA.UIDrivers.LTLAnalyzeService();

    //Loading presenters
    var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, containerEditorDriver, contextMenuDriver, exportService);
    var proofPresenter = new BMA.Presenters.ProofPresenter(appModel, proofViewer, popupDriver, proofAnalyzeService, messagebox, logService);
    var furtherTestingPresenter = new BMA.Presenters.FurtherTestingPresenter(appModel, furtherTestingDriver, popupDriver, furtherTestingServiсe, messagebox, logService);
    var simulationPresenter = new BMA.Presenters.SimulationPresenter(appModel, $("#analytics"), fullSimulationViewer, simulationViewer, popupDriver, simulationService, logService, exportService, messagebox);
    var storagePresenter = new BMA.Presenters.ModelStoragePresenter(appModel, fileLoaderDriver, changesCheckerTool, logService, exportService);
    var formulaValidationPresenter = new BMA.Presenters.FormulaValidationPresenter(variableEditorDriver, formulaValidationService);
    var localStoragePresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageDriver, localRepositoryTool, messagebox, changesCheckerTool, logService);

    //LTL Presenters
    var ltlPresenter = new BMA.Presenters.LTLPresenter(ltlCommands, appModel, stateseditordriver, ltlDriver, tpeditordriver, ltlDriver, ltlService, popupDriver);
    
    //Loading model from URL
    var reserved_key = "InitialModel";

    var params = getSearchParameters();
    if (params.Model !== undefined) {

        var s = params.Model.split('.');
        if (s.length > 1 && s[s.length - 1] == "json") {
            $.ajax(params.Model, {
                dataType: "text",
                success: function (fileContent) {
                    appModel.Deserialize(fileContent);
                    //appModel._Reset(fileContent);
                }
            })
        }
        else {
            $.get(params.Model, function (fileContent) {
                try {
                    var model = BMA.ParseXmlModel(fileContent, window.GridSettings);
                    appModel.Reset(model.Model, model.Layout);
                }
                catch (exc) {
                    console.log(exc);
                    appModel.Deserialize(fileContent);
                }
            });
        }
    }
    else {
        window.Commands.Execute("LocalStorageInitModel", reserved_key);
    }

    var lastversion = window.localStorage.getItem(version_key);
    if (lastversion !== JSON.stringify(version)) {
        var userDialog = $('<div></div>').appendTo('body').userdialog({
            message: "BMA client was updated to version " + $('.version-number').text(),
            actions: [
                {
                    button: 'Ok',
                    callback: function () { userDialog.detach(); }
                }
            ]
        });
    }


    window.onunload = function () {
        window.localStorage.setItem(version_key, JSON.stringify(version));
        window.localStorage.setItem(reserved_key, appModel.Serialize());
        var log = logService.CloseSession();
        var data = JSON.stringify({
            SessionID: log.SessionID,
            UserID: log.UserID,
            LogInTime: log.LogIn,
            LogOutTime: log.LogOut,
            FurtherTestingCount: log.FurtherTesting,
            ImportModelCount: log.ImportModel,
            RunSimulationCount: log.Simulation,
            NewModelCount: log.NewModel,
            RunProofCount: log.Proof,
            SaveModelCount: log.SaveModel,
            ProofErrorCount: log.ProofErrorCount,
            SimulationErrorCount: log.SimulationErrorCount,
            FurtherTestingErrorCount: log.FurtherTestingErrorCount,
            ClientVersion: "BMA HTML5 " + version.major + '.' + version.minor + '.' + version.build
        });
        var sendBeacon = navigator['sendBeacon'];
        if (sendBeacon) {
            sendBeacon('/api/ActivityLog', data);
        } else {
            var xhr = new XMLHttpRequest();
            xhr.open('post', '/api/ActivityLog', false);
            xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');
            xhr.setRequestHeader("Content-length", data.length.toString());
            xhr.setRequestHeader("Connection", "close");
            xhr.send(data);
        }
    };

    $("label[for='button-pointer']").click();

}