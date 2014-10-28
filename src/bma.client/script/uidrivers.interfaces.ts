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
            SetZoom(zoom: number);
            SetCenter(x: number, y: number);
        }

        export interface IServiceDriver {
            Invoke(url, data): JQueryPromise<any>;
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
            ShowStartFurtherTestingToggler();
            HideStartFurtherTestingToggler();
            ShowResults(data);
            HideResults();
            GetViewer();
            StandbyMode();
            ActiveMode();
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
            StandbyMode();
            ActiveMode();
        }

        export interface ILocalStorageDriver {
            SetItems(keys);
            AddItem(key, item);
            Show();
            Hide();
        }

        export interface IFileLoader {
            OpenFileDialog(): JQueryPromise<File>;
        }

        export interface IContextMenu {
            GetMenuItems(): string[];
            EnableMenuItems(optionsVisibility: { name: string; isEnabled: boolean }[]): void;
            ShowMenuItems(optionsVisibility: { name: string; isVisible: boolean }[]): void;
        }

        export interface IAreaHightlighter {
            HighlightAreas(areas: { x: number; y: number; width: number; height: number; fill: string}[]);
        }

        export interface IModelRepository {
            GetModelList(): string[];
            LoadModel(id: string): JSON;
            RemoveModel(id: string);
            SaveModel(id: string, model: JSON);
            IsInRepo(id: string);
            //OnRepositoryUpdated();
        }

        export interface IMessageServise {
            Show(message: string);
            Log(message: string);
        }
    }
} 