module BMA {
    export module LTL {
        export class StatesPresenter {
            private appModel: BMA.Model.AppModel;
            private statesEditor: BMA.UIDrivers.IStatesEditor;
            private statesViewer: BMA.UIDrivers.IStatesViewer;


            constructor(commands: BMA.CommandRegistry, appModel: BMA.Model.AppModel, stateseditordriver: BMA.UIDrivers.IStatesEditor, statesviewerdriver: BMA.UIDrivers.IStatesViewer) {
                var that = this;

                this.appModel = appModel;
                this.statesEditor = stateseditordriver;
                this.statesViewer = statesviewerdriver;

                this.statesEditor.SetStates(appModel.States);
                this.statesViewer.SetStates(appModel.States);
                this.statesViewer.SetCommands(commands);

                commands.On("AddFirstStateRequested",(args) => {
                    if (appModel.States.length === 0) {
                        var newState = new BMA.LTLOperations.Keyframe("A", "", [])
                        appModel.States.push(newState);
                        this.statesEditor.SetStates(appModel.States);
                        this.statesViewer.SetStates(appModel.States);
                    }

                    stateseditordriver.Show();
                });

                commands.On("KeyframesChanged",(args) => {
                    appModel.States = args.states;
                    that.statesViewer.SetStates(args.states);
                });

                commands.On("UpdateStatesEditorOptions",(args) => {
                    that.statesEditor.SetModel(that.appModel.BioModel, that.appModel.Layout);
                })
            }

            public GetStateByName(name: string): BMA.LTLOperations.Keyframe {
                var keyframes = this.appModel.States;
                for (var i = 0; i < keyframes.length; i++) {
                    if (keyframes[i].Name === name)
                        return keyframes[i]; //TODO: Check whether clone is needed here
                }

                return undefined;
            }

            public UpdateStatesFromModel() {
                this.statesEditor.SetStates(this.appModel.States);
                this.statesViewer.SetStates(this.appModel.States);
            }

        }
    }
} 