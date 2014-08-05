/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model.ts"/>
/// <reference path="script\commands.ts"/>
/// <reference path="script\elementsregistry.ts"/>
/// <reference path="script\uidrivers.interfaces.ts"/>
/// <reference path="script\uidrivers.ts"/>
/// <reference path="script\presenters.ts"/>
/// <reference path="script\drawingsurface.ts"/>
/// <reference path="script\modeltoolbar.ts"/>
/// <reference path="script\accordeon.ts"/>
/// <reference path="script\skinmodel.ts"/>
/// <reference path="script\visibilitysettings.ts"/>

window.onload = function () {
    //Creating CommandRegistry
    window.Commands = new BMA.CommandRegistry();

    //Creating ElementsRegistry
    window.Elements = new BMA.Elements.ElementsRegistry();

    //Loading widgets
    $("#drawingSurface").drawingsurface();
    $("#modelToolbarContent").modeltoolbar();
    $("#modelToolbarContent").modeltoolbar();
    $("modelToolbarSlider").bmaaccordion({ header: $("#visButton"), context: $("#visibility") });
    //Loading Drivers
    //Loading presenters
};
//# sourceMappingURL=app.js.map
