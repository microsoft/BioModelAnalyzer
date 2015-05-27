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
        var my_popup_w = $(this).outerWidth(), my_popup_h = $(this).outerHeight(), win_w = $(window).outerWidth(), win_h = $(window).outerHeight(), popup_half_w = (win_w - my_popup_w) / 2, popup_half_h = (win_h - my_popup_h) / 2;
        if (win_w > my_popup_w) {
            my_popup.css({ 'left': popup_half_w });
        }
        if (win_w < my_popup_w) {
            my_popup.css({ 'left': 5, });
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
    var snipper = $('<div></div>').addClass('spinner').appendTo($('.loading-text'));
    for (var i = 1; i < 4; i++) {
        $('<div></div>').addClass('bounce' + i).appendTo(snipper);
    }
    var deferredLoad = function () {
        var dfd;
        dfd = $.Deferred();
        try {
            loadScript();
            dfd.resolve();
        }
        catch (ex) {
            dfd.reject(ex.message);
        }
        return dfd.promise();
    };
    deferredLoad().done(function () {
        $('.page-loading').detach();
    }).fail(function (err) {
        alert("Page loadind failed: " + err);
    });
    $(document).ready(function () {
        popup_position();
    });
    $(window).resize(function () {
        popup_position();
    });
});
function loadScript() {
    var version_key = 'bma-version';
    var version = {
        'major': '1',
        'minor': '2',
        'build': '0042'
    };
    $('.version-number').text('v. ' + version.major + '.' + version.minor + '.' + version.build);
    window.Commands = new BMA.CommandRegistry();
    window.ElementRegistry = new BMA.Elements.ElementsRegistry();
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
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
    var drawingSurface = $("#drawingSurface");
    drawingSurface.drawingsurface();
    $("#zoomslider").bmazoomslider({ value: 50 });
    $("#modelToolbarHeader").buttonset();
    $("#modelToolbarContent").buttonset();
    $("#modelToolbarSlider").bmaaccordion({ position: "left", z_index: 1 });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion();
    $("#visibilityOptionsContent");
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
    };
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
                title: "Size",
                cmd: "Size",
                children: [
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
            }
            else if (ui.cmd === "ResizeCellTo2x2") {
                args.size = 2;
                commandName += "ResizeCell";
            }
            else if (ui.cmd === "ResizeCellTo3x3") {
                args.size = 3;
                commandName += "ResizeCell";
            }
            else {
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
    var ulsizes;
    aas.each(function () {
        switch ($(this).text()) {
            case "Cut":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-cut.svg"><span>Cut</span>';
                break;
            case "Copy":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-copy.svg"><span>Copy</span>';
                break;
            case "Paste":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-paste.svg"><span>Paste</span>';
                break;
            case "Edit":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-edit.svg"><span>Edit</span>';
                break;
            case "Size":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-size.svg"><span>Size  ></span>';
                ulsizes = $(this).next('ul');
                break;
            case "Delete":
                $(this)[0].innerHTML = '<img alt="" src="../images/icon-delete.svg"><span>Delete</span>';
                break;
        }
    });
    if (ulsizes !== undefined)
        ulsizes.addClass('context-menu-small');
    if (asizes !== undefined) {
        var asizes = ulsizes.children('li').children('a');
        asizes.each(function (ind) {
            $(this)[0].innerHTML = '<img alt="" src="../images/' + (ind + 1) + 'x' + (ind + 1) + '.svg">';
        });
    }
    $("#analytics").bmaaccordion({ position: "right", z_index: 4 });
    var elementPanel = $("#modelelemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
        var elem = elements[i];
        $("<input></input>").attr("type", "radio").attr("id", "btn-" + elem.Type).attr("name", "drawing-button").attr("data-type", elem.Type).appendTo(elementPanel);
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
    var localStorageWidget = $('<div></div>').addClass('window').appendTo('#drawingSurceContainer').localstoragewidget();
    $("#editor").bmaeditor();
    $("#Proof-Analysis").proofresultviewer();
    $("#Further-Testing").furthertesting();
    $("#tabs-2").simulationviewer();
    var popup = $('<div></div>').addClass('popup-window window').appendTo('body').hide().resultswindowviewer({ icon: "min" });
    popup.draggable({ scroll: false });
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
    window.Commands.On("Commands.ToggleGrid", function (param) {
        visualSettings.GridVisibility = param;
        svgPlotDriver.SetGridVisibility(param);
    });
    window.Commands.On("ZoomSliderBind", function (value) {
        $("#zoomslider").bmazoomslider({ value: value });
    });
    window.Commands.On('SetPlotSettings', function (value) {
        if (value.MaxWidth !== undefined) {
            window.PlotSettings.MaxWidth = value.MaxWidth;
            $("#zoomslider").bmazoomslider({ max: (value.MaxWidth - window.PlotSettings.MinWidth) / 24 });
        }
        if (value.MinWidth !== undefined) {
            window.PlotSettings.MinWidth = value.MinWidth;
        }
    });
    window.Commands.On("AppModelChanged", function () {
        if (changesCheckerTool.IsChanged) {
            popupDriver.Hide();
            accordionHider.Hide();
            window.Commands.Execute("Expand", '');
        }
    });
    window.Commands.On("DrawingSurfaceVariableEditorOpened", function () {
        popupDriver.Hide();
        accordionHider.Hide();
    });
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
    var messagebox = new BMA.UIDrivers.MessageBoxDriver();
    var localRepositoryTool = new BMA.LocalRepositoryTool(messagebox);
    var changesCheckerTool = new BMA.ChangesChecker();
    changesCheckerTool.Snapshot(appModel);
    var exportService = new BMA.UIDrivers.ExportService();
    var formulaValidationService = new BMA.UIDrivers.FormulaValidationService();
    var furtherTestingServiсe = new BMA.UIDrivers.FurtherTestingService();
    var proofAnalyzeService = new BMA.UIDrivers.ProofAnalyzeService();
    var simulationService = new BMA.UIDrivers.SimulationService();
    var logService = new BMA.SessionLog();
    var undoRedoPresenter = new BMA.Presenters.UndoRedoPresenter(appModel, undoDriver, redoDriver);
    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, undoRedoPresenter, svgPlotDriver, svgPlotDriver, svgPlotDriver, variableEditorDriver, containerEditorDriver, contextMenuDriver, exportService);
    var proofPresenter = new BMA.Presenters.ProofPresenter(appModel, proofViewer, popupDriver, proofAnalyzeService, messagebox, logService);
    var furtherTestingPresenter = new BMA.Presenters.FurtherTestingPresenter(appModel, furtherTestingDriver, popupDriver, furtherTestingServiсe, messagebox, logService);
    var simulationPresenter = new BMA.Presenters.SimulationPresenter(appModel, $("#analytics"), fullSimulationViewer, simulationViewer, popupDriver, simulationService, logService, exportService, messagebox);
    var storagePresenter = new BMA.Presenters.ModelStoragePresenter(appModel, fileLoaderDriver, changesCheckerTool, logService, exportService);
    var formulaValidationPresenter = new BMA.Presenters.FormulaValidationPresenter(variableEditorDriver, formulaValidationService);
    var localStoragePresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageDriver, localRepositoryTool, messagebox, changesCheckerTool, logService);
    var reserved_key = "InitialModel";
    var params = getSearchParameters();
    if (params.Model !== undefined) {
        var s = params.Model.split('.');
        if (s.length > 1 && s[s.length - 1] == "json") {
            $.ajax(params.Model, {
                dataType: "text",
                success: function (fileContent) {
                    appModel.Deserialize(fileContent);
                }
            });
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
    var toolsdivs = $('#tools').children('div');
    function resize_header_tools() {
        toolsdivs.each(function () {
            $(this).toggleClass('box-sizing');
        });
    }
    var lastversion = window.localStorage.getItem(version_key);
    if (lastversion !== JSON.stringify(version)) {
        var userDialog = $('<div></div>').appendTo('body').userdialog({
            message: "BMA client was updated to version " + $('.version-number').text(),
            actions: [
                {
                    button: 'Ok',
                    callback: function () {
                        userDialog.detach();
                    }
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
            ClientVersion: "BMA HTML5 2.0"
        });
        var sendBeacon = navigator['sendBeacon'];
        if (sendBeacon) {
            sendBeacon('/api/ActivityLog', data);
        }
        else {
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
