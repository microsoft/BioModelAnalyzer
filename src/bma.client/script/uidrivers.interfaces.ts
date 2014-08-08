/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {
        export interface ISVGPlot {
            Draw(svg: SVGElement);
            TurnNavigation(isOn: boolean);
        }

        export interface IElementsPanel {

        }


        export interface ITurnableButton {
            Turn(isOn: boolean);
        }

    }
} 