module BMA {
    export module Presenters {
        export class StoragePresenter {
            private localTool: BMA.UIDrivers.IModelRepository;
            private oneDriveTool: BMA.UIDrivers.IModelRepository;
            private oneDrivePresenter: BMA.Presenters.OneDriveStoragePresenter;
            private localStoragePresenter: BMA.Presenters.LocalStoragePresenter;
        }
    }
}