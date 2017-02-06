// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module BMA {

    export class ChangesChecker implements BMA.UIDrivers.ICheckChanges {

        private currentModel: BMA.Model.AppModel = new BMA.Model.AppModel();

        Snapshot(model: BMA.Model.AppModel) {
            this.currentModel.BioModel = model.BioModel.Clone();
            this.currentModel.Layout = model.Layout.Clone();
            this.currentModel.ProofResult = model.ProofResult;

            this.currentModel.Operations = [];
            this.currentModel.OperationAppearances = [];
            for (var i = 0; i < model.Operations.length; i++) {
                this.currentModel.Operations.push(model.Operations[i].Clone());
                if (model.OperationAppearances !== undefined && model.OperationAppearances.length !== 0)
                    this.currentModel.OperationAppearances.push({
                        x: model.OperationAppearances[i].x,
                        y: model.OperationAppearances[i].y,
                        steps: model.OperationAppearances[i].steps,
                    });
            }

            this.currentModel.States = [];
            for (var i = 0; i < model.States.length; i++)
                this.currentModel.States.push(model.States[i].Clone());

            //this.currentModel.Deserialize(model.Serialize());
        }

        IsChanged(model: BMA.Model.AppModel): boolean {
            return this.currentModel.Serialize() !== model.Serialize();
        }
    }
} 
