
$(document).ready(function () {
    $("#drawingSurface").drawingsurface();
    $("#modelToolbarHeader").modeltoolbar();
    $("#modelToolbarContent").modeltoolbar();
    $("#modelToolbarSlider").bmaaccordion({ position: "left" });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion({ header: $("#visibilityOptionsHeader"), context: $("#visibilityOptionsContent") });
    $("#analytics").bmaaccordion({ position: "right" });
});
