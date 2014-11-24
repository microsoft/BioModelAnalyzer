module BMA {

    export class ChangesChecker implements BMA.UIDrivers.ICheckChanges {

        private currentModel: BMA.Model.AppModel = new BMA.Model.AppModel();

        Snapshot(model: BMA.Model.AppModel) {
            this.currentModel.Reset(model.Serialize());
        }

        IsChanged(model: BMA.Model.AppModel): boolean {
            return this.currentModel.Serialize() !== model.Serialize();
        }
    }
} 