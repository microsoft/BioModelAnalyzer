/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model.ts"/>
/// <reference path="script\commands.ts"/>
/// <reference path="script\elementsregistry.ts"/>
/// <reference path="script\uidrivers.interfaces.ts"/>
/// <reference path="script\uidrivers.ts"/>
/// <reference path="script\presenters.ts"/>
/// <reference path="script\widgets\drawingsurface.ts"/>
/// <reference path="script\widgets\toolbarpanel.ts"/>
/// <reference path="script\widgets\accordeon.ts"/>
/// <reference path="script\widgets\skinmodel.ts"/>
/// <reference path="script\widgets\visibilitysettings.ts"/>
/// <reference path="script\widgets\elementbutton.ts"/>

$(document).ready(function () {
    //Creating CommandRegistry
    window.Commands = new BMA.CommandRegistry();

    //Creating ElementsRegistry
    window.ElementRegistry = new BMA.Elements.ElementsRegistry();

    //Loading widgets
    $("#drawingSurface").drawingsurface();
    $("#modelToolbarHeader").toolbarpanel();
    $("#modelToolbarContent").toolbarpanel();
    $("#modelToolbarSlider").bmaaccordion({ position: "left" });
    $("#visibilityOptionsContent").visibilitysettings();
    $("#visibilityOptionsSlider").bmaaccordion({ header: $("#visibilityOptionsHeader"), context: $("#visibilityOptionsContent") });
    $("#analytics").bmaaccordion({ position: "right" });
    /*
    var elementPanel = $("#elemtoolbar");
    var elements = window.ElementRegistry.Elements;
    for (var i = 0; i < elements.length; i++) {
    var elem = elements[i];
    $("<input></input>").attr("type", elem.Type).attr("id", "btn-" + elem.Type).appendTo(elementPanel);
    var label = $("<label></label>").attr("for", "btn-" + elem.Type).appendTo(elementPanel);
    $("<img></img>").attr("src", elem.IconURL).attr("title", elem.Description).appendTo(label);
    }
    elementPanel.buttonset();
    */
    //Loading Drivers
    //Loading presenters
});
//# sourceMappingURL=app.js.map
