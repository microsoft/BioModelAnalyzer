/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {

        export interface ILTLViewer {
            Show(p: string);
            Hide(p: string);
            SetResult(result);
            GetTemporalPropertiesViewer(): BMA.UIDrivers.ITemporalPropertiesViewer;
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

        export interface ITemporalPropertiesEditor {
            Show();
            Hide();
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