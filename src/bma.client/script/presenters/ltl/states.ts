module BMA {
    export module LTL {
        export class StatesPresenter {
            private appModel: BMA.Model.AppModel;
            private statesEditor: BMA.UIDrivers.IStatesEditor;

            constructor(appModel: BMA.Model.AppModel, stateseditordriver: BMA.UIDrivers.IStatesEditor) {
                this.appModel = appModel;
                this.statesEditor = stateseditordriver;

                this.statesEditor.SetStates(appModel.States);
            }

            public GetStateByName(name: string): BMA.LTLOperations.Keyframe {
                var keyframes = this.appModel.States;
                for (var i = 0; i < keyframes.length; i++) {
                    if (keyframes[i].Name === name)
                        return keyframes[i];//TODO: Check whether clone is needed here
                }

                return undefined;
            }
        }
    }
} 