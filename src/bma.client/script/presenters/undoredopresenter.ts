/*
module BMA {
    export module Presenters {
        export class UndoRedoPresenter {
            private appModel: BMA.Model.AppModel;
            private models: { model: BMA.Model.BioModel; layout: BMA.Model.Layout }[];
            private currentModelIndex: number = -1;

            private undoButton: BMA.UIDrivers.ITurnableButton;
            private redoButton: BMA.UIDrivers.ITurnableButton;

            constructor(appModel: BMA.Model.AppModel,
                undoButton: BMA.UIDrivers.ITurnableButton,
                redoButton: BMA.UIDrivers.ITurnableButton) {

                var that = this;
                this.appModel = appModel;
                this.undoButton = undoButton;
                this.redoButton = redoButton;

                window.Commands.On("Undo", () => {
                    this.Undo();
                });

                window.Commands.On("Redo", () => {
                    this.Redo();
                });

                window.Commands.On("ModelReset", () => {
                    this.Set(this.appModel.BioModel, this.appModel.Layout);
                });

                this.Set(this.appModel.BioModel, this.appModel.Layout);
            }

            private OnModelUpdated() {
                this.undoButton.Turn(this.CanUndo);
                this.redoButton.Turn(this.CanRedo);

                this.appModel.BioModel = this.Current.model;
                this.appModel.Layout = this.Current.layout;

                if (this.editingVariableId !== undefined) {
                    this.variableEditor.Initialize(this.GetVariableById(this.Current.layout, this.Current.model, this.editingVariableId).model, this.Current.model);
                }

                var drawingSvg = <SVGElement>this.CreateSvg();
                this.highlightDriver.HighlightAreas(this.PrepareHighlightAreas());
                this.driver.Draw(drawingSvg);
            }

            private Undo() {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    this.variableEditor.Hide();
                    this.editingVariableId = undefined;
                    this.OnModelUpdated();
                }
            }

            private Redo() {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.variableEditor.Hide();
                    this.editingVariableId = undefined;
                    this.OnModelUpdated();
                }
            }

            private Truncate() {
                this.models.length = this.currentModelIndex + 1;
            }

            public Dup(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
                this.Truncate();
                var current = this.Current;
                this.models[this.currentModelIndex] = { model: current.model.Clone(), layout: current.layout.Clone() };
                this.models.push({ model: m, layout: l });
                ++this.currentModelIndex;
                this.OnModelUpdated();
            }

            private get CanUndo(): boolean {
                return this.currentModelIndex > 0;
            }

            private get CanRedo(): boolean {
                return this.currentModelIndex < this.models.length - 1;
            }

            public Set(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
                this.models = [{ model: m, layout: l }];
                this.currentModelIndex = 0;
                this.ResetVariableIdIndex();
                this.variableEditor.Hide();
                this.editingVariableId = undefined;
                this.OnModelUpdated();
            }

            public get Current(): { model: BMA.Model.BioModel; layout: BMA.Model.Layout } {
                return this.models[this.currentModelIndex];
            }
        }
    }
}
*/