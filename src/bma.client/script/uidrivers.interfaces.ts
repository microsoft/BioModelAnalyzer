/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {
        export interface ISVGPlot {
            Draw(svg: SVGElement);
            SetGrid(x0: number, y0: number, xStep: number, yStep: number);
            GetPlotX(left: number);
            GetPlotY(top: number);
            GetPixelWidth();
            SetGridVisibility(isOn: boolean);
        }

        export interface IHider {
            Hide();
        }

        export interface INavigationPanel {
            TurnNavigation(isOn: boolean);
            GetZoomSubject(): any;
            SetZoom(zoom: number);
        }

        export interface IVariableEditor {
            GetVariableProperties(): {
                name: string; formula: string; rangeFrom: number; rangeTo: number;
            };
            Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel);
            Show(x: number, y: number);
            Hide();
            SetValidation(val: boolean, message: string);
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

        export interface IFurtherTesting {
            ShowStartToggler();
            HideStartToggler();
            ShowResults(data);
            HideResults();
            GetViewer();
        }

        export interface ISimulationViewer {
            SetData(params);
            Show(params);
            Hide(params);
            ChangeVisibility(params);
        }

        export interface ISimulationExpanded {
            AddResult(res);
            GetViewer();
            Set(data);
        }

        export interface IFileLoader {
            OpenFileDialog(): JQueryPromise<File>;
        }

        export interface IContextMenu {
            GetMenuItems(): string[];
            EnableMenuItems(optionsVisibility: { name: string; isVisible: boolean}[]) : void ;
        }

    }
} 