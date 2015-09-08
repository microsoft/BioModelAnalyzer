/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {

        export interface ILTLViewer {
            Show(p: string);
            Hide(p: string);
            SetResult(result);
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

        export interface ITemporalPropertiesViewer {
            SetOperation(operation: BMA.LTLOperations.Operation);
        }

        export interface ITemporalPropertiesEditor {
            GetContent();
        }
    }
} 