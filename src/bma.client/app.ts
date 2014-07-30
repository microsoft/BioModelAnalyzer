/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="script\model.ts"/>
/// <reference path="script\master.ts"/>
/// <reference path="script\drawingsurface.ts"/>

module BMA {
    class BMAApplicationHub {
        private master: Master;

        constructor() {
        }

    }
}

window.onload = () => {
    $("#content").drawingsurface();
};