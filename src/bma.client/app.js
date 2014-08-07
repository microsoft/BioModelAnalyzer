
$(document).ready(function () {
    window.Commands = new BMA.CommandRegistry();

    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    $("#drawingSurface").drawingsurface();
    $("#modelToolbarHeader").toolbarpanel();
    $("#modelToolbarContent").toolbarpanel();
    $("#modelToolbarSlider").bmaaccordion({ position: "left" });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion({ header: $("#visibilityOptionsHeader"), context: $("#visibilityOptionsContent") });
    $("#analytics").bmaaccordion({ position: "right" });

    var elementPanel = $("#modelelemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
        var elem = elements[i];
        $("<input></input>").attr("type", "radio").attr("id", "btn-" + elem.Type).attr("name", "drawing-button").attr("data-type", elem.Type).appendTo(elementPanel);

        var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
        $("<img></img>").attr("src", elem.IconURL).attr("title", elem.Description).appendTo(label);
    }
    $("#modelelemtoolbar input").click(function () {
        window.Commands.Execute("AddElementSelect", $(this).attr("data-type"));
    });
    elementPanel.buttonset();

    $("#undoredotoolbar").buttonset();
});
