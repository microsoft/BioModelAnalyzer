/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {
        export interface ISVGPlot {
            Draw(svg: SVGElement);
            SetGrid(x0: number, y0: number, xStep: number, yStep: number);
        }

        export interface INavigationPanel {
            TurnNavigation(isOn: boolean);
            SetZoom(zoom: number);
        }

        export interface IVariableEditor {
            GetVariableProperties(): {
                name: string; formula: string; rangeFrom: number; rangeTo: number;
            };
            Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel);
            Show(x: number, y: number);
            Hide();
            SetValidation(val: boolean);
        }

        export interface IElementsPanel {
            GetDragSubject(): any;
        }

        export interface ITurnableButton {
            Turn(isOn: boolean);
        }

        export interface IProcessLauncher {

        }

        export interface IPopup {
            Show(params);
            Hide();
        }


        export interface IProofResultViewer {
            ShowResult(result: BMA.Model.ProofResult);
            SetData(params);
            OnProofStarted();
            OnProofFailed();
            Show(params);
            Hide(params);
        }

        export interface ISimulationViewer {
            SetData(params);
            Show(params);
            Hide(params);
            ChangeVisibility(params);
        }

        export interface ISimulationFull {
            AddResult(res);
            GetViewer();
            Set(data);
        }

        export interface IFileLoader {
            OpenFileDialog() : JQueryPromise<File>;
        }

    }
} 