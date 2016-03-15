module BMA {
    export module Presenters {
        export class UndoRedoPresenter {
            private appModel: BMA.Model.AppModel;
            private models: { model: BMA.Model.BioModel; layout: BMA.Model.Layout }[];
            private currentModelIndex: number = -1;

            private undoButton: BMA.UIDrivers.ITurnableButton;
            private redoButton: BMA.UIDrivers.ITurnableButton;

            private maxStackCount = 10;

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

            private OnModelUpdated(status: string) {
                this.undoButton.Turn(this.CanUndo);
                this.redoButton.Turn(this.CanRedo);

                this.appModel.BioModel = this.Current.model;
                this.appModel.Layout = this.Current.layout;

                window.Commands.Execute("DrawingSurfaceRefreshOutput", { status: status });
            }

            private Undo() {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    
                    this.OnModelUpdated("Undo");
                }
            }

            private Redo() {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.OnModelUpdated("Redo");
                }
            }

            private Truncate() {
                if (this.models.length < this.maxStackCount) {
                    this.models.length = this.currentModelIndex + 1;
                } else {
                    var cuttedModels = [];
                    for (var i = this.models.length - this.maxStackCount; i < this.models.length; i++) {
                        cuttedModels.push(this.models[i]);
                    }
                    this.models = cuttedModels;
                    this.currentModelIndex = this.models.length - 1;
                }
            }

            public Dup(m: BMA.Model.BioModel, l: BMA.Model.Layout) {
                this.Truncate();
                var current = this.Current;
                this.models[this.currentModelIndex] = { model: current.model.Clone(), layout: current.layout.Clone() };
                this.models.push({ model: m, layout: l });
                ++this.currentModelIndex;
                this.OnModelUpdated("Dup");
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
                this.OnModelUpdated("Set");
            }

            public get Current(): { model: BMA.Model.BioModel; layout: BMA.Model.Layout } {
                return this.models[this.currentModelIndex];
            }
        }
    }
}
