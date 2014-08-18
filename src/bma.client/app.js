$(document).ready(function () {
    window.Commands = new BMA.CommandRegistry();

    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    var appModel = new BMA.Model.AppModel();

    var drawingSurface = $("#drawingSurface");
    drawingSurface.drawingsurface();

    $("#modelToolbarHeader").buttonset();
    $("#modelToolbarContent").buttonset();
    $("#modelToolbarSlider").bmaaccordion({ position: "left" });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion({ header: $("#visibilityOptionsHeader") });
    $("#analytics").bmaaccordion({ position: "right" });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: false } });
    $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon2", val: false } });
    $("#icon1").click();
    setTimeout(function () {
        $("#analytics").bmaaccordion({ contentLoaded: { ind: "#icon1", val: true } });
    }, 2000);

    $(".bma-elementspanel-visibilityoptions-zoomslider").slider();

    var elementPanel = $("#modelelemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
        var elem = elements[i];
        $("<input></input>").attr("type", "radio").attr("id", "btn-" + elem.Type).attr("name", "drawing-button").attr("data-type", elem.Type).appendTo(elementPanel);

        var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
        $("<img></img>").attr("src", elem.IconURL).attr("title", elem.Description).appendTo(label);
    }
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

    var svgPlotDriver = new BMA.UIDrivers.SVGPlotDriver(drawingSurface);

    var drawingSurfacePresenter = new BMA.Presenters.DesignSurfacePresenter(appModel, svgPlotDriver, new BMA.UIDrivers.TurnableButtonDriver($("#button-undo")), new BMA.UIDrivers.TurnableButtonDriver($("#button-redo")));
});
