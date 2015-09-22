/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {

        export interface ILTLViewer {
            Show(p: string);
            Hide(p: string);
            SetResult(result);
            GetTemporalPropertiesViewer(): BMA.UIDrivers.ITemporalPropertiesViewer;
            GetStatesViewer(): BMA.UIDrivers.IStatesViewer;
        }

        export interface IKeyframesList {
            AddState(items);
            GetContent();
        }

        export interface IKeyframesFull {
            AddState(items);
            GetContent();
            RemovePart(keyframe, ind);
        }

        export interface IStatesEditor {
            SetStates(states: BMA.LTLOperations.Keyframe[]);
            SetModel(model: BMA.Model.BioModel, layout: BMA.Model.Layout);
            Show();
            Hide();
        }

        export interface IStatesViewer {
            SetCommands(commands: BMA.CommandRegistry);
            SetStates(states: BMA.LTLOperations.Keyframe[]);
        }

        export interface ITemporalPropertiesEditor {
            Show();
            Hide();
            SetStates(states: BMA.LTLOperations.Keyframe[]);
            GetSVGDriver(): ISVGPlot;
            GetNavigationDriver(): INavigationPanel;
            GetDragService(): IElementsPanel;
            GetContextMenuDriver(): IContextMenu;
        }

        export interface ITemporalPropertiesViewer {
            SetOperations(operations: BMA.LTLOperations.IOperand[]);
        }
    }
} 