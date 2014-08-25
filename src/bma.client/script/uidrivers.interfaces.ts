/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {
        export interface ISVGPlot {
            Draw(svg: SVGElement);
            TurnNavigation(isOn: boolean);
            SetGrid(x0: number, y0: number, xStep: number, yStep: number);
        }

        export interface IVariableEditor {
            GetVariableProperties(): {
                name: string; formula: string; rangeFrom: number; rangeTo: number;
            };
            Initialize(variable: BMA.Model.Variable);
            Show(x: number, y: number);
            Hide();
        }

        export interface IElementsPanel {
            GetDragSubject(): any;
        }

        export interface ITurnableButton {
            Turn(isOn: boolean);
        }

    }
} 