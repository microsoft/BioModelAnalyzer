/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {
        export interface ISVGPlot {
            Draw(svg: SVGElement);
            DrawLayer2(svg: SVGElement);
            SetGrid(x0: number, y0: number, xStep: number, yStep: number);
            GetPlotX(left: number);
            GetPlotY(top: number);
            GetLeft(x: number);
            GetTop(y: number);
            GetPixelWidth();
            SetGridVisibility(isOn: boolean);
            SetVisibleRect(rect: { x: number; y: number; width: number; height: number });
            GetSVG(): string;
            GetSVGRef(): any;
            GetLightSVGRef(): any;
            SetConstraintFunc(constraint: Function);
        }

        export interface IHider {
            Hide();
            HideTab(index);
            ContentLoaded(index, value);
        }

        export interface INavigationPanel {
            GetNavigationSurface(): any; //TODO: add proper type here
            TurnNavigation(isOn: boolean);
            SetZoom(zoom: number);
            SetCenter(x: number, y: number);
            MoveDraggableOnTop();
            MoveDraggableOnBottom();
        }

        export interface IServiceDriver {
            Invoke(data): JQueryPromise<any>;
        }

        /*
        export interface JQueryPromise<any> {
            abort: Function;
        }
        */

        export interface IExportService {
            Export(content: string, name: string, extension: string)
        }

        export interface IVariableEditor {
            GetVariableProperties(): {
                name: string; formula: string; rangeFrom: number; rangeTo: number; TFdescription: string;
            };
            Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel, layout: BMA.Model.Layout);
            Show(x: number, y: number);
            Hide();
            SetValidation(val: boolean, message: string);
            SetOnClosingCallback(callback: Function);
            SetOnVariableEditedCallback(callback: Function);
            SetOnFormulaEditedCallback(callback: Function);
        }

        export interface IContainerEditor {
            GetContainerName(): string;
            Initialize(containerLayout: BMA.Model.ContainerLayout);
            Show(x: number, y: number);
            Hide();
            SetOnClosingCallback(callback: Function);
        }

        export interface IElementsPanel {
            GetDragSubject(): any;
            GetMouseMoves(): any;
        }

        export interface ITurnableButton {
            Turn(isOn: boolean);
        }

        export interface IProcessLauncher {

        }

        export interface IPopup {
            Seen();
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
            SetData(data);
            StandbyMode();
            ActiveMode();
            SetOnPlotVariablesSelectionChanged(callback);
            SetOnCreateStateRequested(callback);
        }

        export interface ILocalStorageDriver {
            SetItems(keys);
            AddItem(key, item);
            //Show();
            //Hide();
            Message(msg: string);
            SetOnEnableContextMenu(enable: boolean);
            SetOnLoadModel(callback: Function);
            SetOnRemoveModel(callback: Function);
            SetOnCopyToOneDriveCallback(callback: Function);
        }

        export interface IOneDriveDriver {
            SetItems(keys);
            AddItem(key, item);
            //Show();
            //Hide();
            Message(msg: string);
            SetOnLoadModel(callback: Function);
            SetOnRemoveModel(callback: Function);
            SetOnCopyToLocalCallback(callback: Function);
            SetOnShareCallback(callback: Function);
            SetOnActiveShareCallback(callback: Function);
            SetOnOpenBMALink(callback: Function);
        }

        export interface IModelStorageDriver {
            Show();
            Hide();
            SetAuthorizationStatus(status: boolean);
            //SetOnSignInCallback(callback: Function);
            //SetOnSignOutCallback(callback: Function);
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



        export interface ModelInfo {
            id: string;
            name: string;
        }

        export interface IModelRepository {
            GetModelList(): JQueryPromise<string[]>;
            LoadModel(id: string): JQueryPromise<JSON>;
            RemoveModel(id: string);
            SaveModel(id: string, model: JSON);
            IsInRepo(id: string);
            
            //OnRepositoryUpdated();
        }

        export interface IMessageServiсe {
            Show(message: string);
            Log(message: string);
        }

        export interface ICheckChanges {
            Snapshot(model);
            IsChanged(model);
        }

        export interface IWaitScreen {
            Show();
            Hide();
        }

        export interface IDragnDropExtender {
            HandleDrop(screenLocation: { x: number; y: number }, dropObject: any): boolean;
        }
       
    }
} 