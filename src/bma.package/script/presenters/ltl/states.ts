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
                
                commands.On("KeyframesChanged",(args) => {
                    if (this.CompareStatesToAppModel(args.states)) {
                        appModel.States = args.states;
                        that.statesViewer.SetStates(args.states);
                    }
                });

                commands.On("UpdateStatesEditorOptions",(args) => {
                    that.statesEditor.SetModel(that.appModel.BioModel, that.appModel.Layout);
                });

                window.Commands.On("ModelReset",(args) => {
                    this.statesEditor.SetStates(appModel.States);
                    this.statesViewer.SetStates(appModel.States);
                });
                
            }

            public GetStateByName(name: string): BMA.LTLOperations.Keyframe | BMA.LTLOperations.TrueKeyframe | BMA.LTLOperations.OscillationKeyframe | BMA.LTLOperations.SelfLoopKeyframe {
                if (name === "truestate")
                    return new BMA.LTLOperations.TrueKeyframe();
                else if (name === "oscillationstate")
                    return new BMA.LTLOperations.OscillationKeyframe();
                else if (name === "selfloopstate")
                    return new BMA.LTLOperations.SelfLoopKeyframe();

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

            private CompareStatesToAppModel(states: BMA.LTLOperations.Keyframe[]) {
                if (states.length !== this.appModel.States.length)
                    return true;
                else {
                    for (var i = 0; i < states.length; i++) {
                        var st = states[i];
                        var appst = this.appModel.States[i];
                        if (st.Name !== appst.Name || st.Description !== appst.Description || BMA.LTLOperations.GetLTLServiceProcessingFormula(st) !== BMA.LTLOperations.GetLTLServiceProcessingFormula(appst))
                            return true;
                    }

                    return false;
                }
            }

        }
    }
} 