module BMA {
    export module Presenters {
        export class StoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.ModelStorageDriver;
            private oneDrivePresenter: BMA.Presenters.OneDriveStoragePresenter;
            private localStoragePresenter: BMA.Presenters.LocalStoragePresenter;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.ModelStorageDriver,
                odPresenter: BMA.Presenters.OneDriveStoragePresenter,
                lPresenter: BMA.Presenters.LocalStoragePresenter,
                messagebox: BMA.UIDrivers.IMessageServiсe,
                checker: BMA.UIDrivers.ICheckChanges,
                logService: BMA.ISessionLog,
                waitScreen: BMA.UIDrivers.IWaitScreen
            ) {
                var that = this;
                this.appModel = appModel;
                this.driver = editor;
                this.messagebox = messagebox;
                this.checker = checker;
                this.oneDrivePresenter = odPresenter;
                this.localStoragePresenter = lPresenter;

                that.driver.SetOnSignInCallback(function () {
                });

                that.driver.SetOnSignOutCallback(function () {
                });

                that.localStoragePresenter.SetOnCopyCallback(function (key, item) {
                    // set copied model to oneDrivePresenter
                });

                that.oneDrivePresenter.SetOnCopyCallback(function (key, item) {
                    // set copied to localpresenter
                });
            }
        }
    }
}