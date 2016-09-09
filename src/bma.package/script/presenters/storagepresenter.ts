module BMA {
    export module Presenters {
        export class StoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.IModelStorageDriver;
            private localStorageDriver: BMA.UIDrivers.ILocalStorageDriver;
            private oneDriveStorageDriver: BMA.UIDrivers.IOneDriveDriver;
            private oneDrivePresenter: BMA.Presenters.OneDriveStoragePresenter;
            private localStoragePresenter: BMA.Presenters.LocalStoragePresenter;
            private connector: BMA.OneDrive.IOneDriveConnector;
            private localRepository: BMA.UIDrivers.IModelRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private activePresenter: string = "local";

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.IModelStorageDriver,
                localDriver: BMA.UIDrivers.ILocalStorageDriver,
                oneDriveDriver: BMA.UIDrivers.IOneDriveDriver,
                //odPresenter: BMA.Presenters.OneDriveStoragePresenter,
                //lPresenter: BMA.Presenters.LocalStoragePresenter,
                connector: BMA.OneDrive.IOneDriveConnector,
                localRepository: BMA.UIDrivers.IModelRepository,
                messagebox: BMA.UIDrivers.IMessageServiсe,
                checker: BMA.UIDrivers.ICheckChanges,
                logService: BMA.ISessionLog,
                waitScreen: BMA.UIDrivers.IWaitScreen
            ) {
                var that = this;
                this.appModel = appModel;
                this.driver = editor;
                this.localStorageDriver = localDriver;
                this.oneDriveStorageDriver = oneDriveDriver;
                this.messagebox = messagebox;
                this.checker = checker;
                //this.oneDrivePresenter = odPresenter;
                this.connector = connector;
                this.localRepository = localRepository;
                this.oneDrivePresenter = undefined;

                var oneDriveRepository = undefined;

                that.driver.Hide();

                this.localStoragePresenter = new BMA.Presenters.LocalStoragePresenter(that.appModel, that.localStorageDriver,
                    localRepository, messagebox, checker, logService, waitScreen);

                this.localStoragePresenter.SetOnIsActive(function () {
                    if (that.activePresenter == "local") return true;
                    return false;
                });

                var onLogin = function (oneDrive) {
                    that.driver.SetAuthorizationStatus(true);
                    that.localStorageDriver.SetOnEnableContextMenu(true);

                    oneDriveRepository = new BMA.OneDrive.OneDriveRepository(oneDrive);

                    that.oneDrivePresenter = new BMA.Presenters.OneDriveStoragePresenter(that.appModel, that.oneDriveStorageDriver, oneDriveRepository,
                        that.messagebox, that.checker, logService, waitScreen);


                    that.localStoragePresenter.SetOnCopyCallback(function (key, item) {
                        oneDriveRepository.SaveModel(key, item).done(function () {
                            window.Commands.Execute("OneDriveStorageChanged", {});
                        }).fail(function () {
                            that.messagebox.Show("Failed to save model on OneDrive");
                        });
                        //that.oneDriveStorageDriver.AddItem(key, item);
                        // set copied model to oneDrivePresenter
                    });

                    that.localStoragePresenter.SetOnActiveCallback(function () {
                        that.activePresenter = "local";
                        that.oneDriveStorageDriver.SetOnUnselect();
                    });

                    that.oneDrivePresenter.SetOnCopyCallback(function (key, item) {
                        that.localRepository.SaveModel(key, item);
                        window.Commands.Execute("LocalStorageChanged", {});
                        // set copied to localpresenter
                    });

                    that.oneDrivePresenter.SetOnActiveCallback(function () {
                        that.activePresenter = "oneDrive";
                        that.localStorageDriver.SetOnUnselect();
                    });

                    that.oneDrivePresenter.SetOnIsActive(function () {
                        if (that.activePresenter == "oneDrive") return true;
                        return false;
                };

                var onLoginFailed = function (failure) {
                    console.error("Login failed: " + failure.error_description);
                };
                
                var onLogout = function (logout) {
                    that.activePresenter = "local";
                    that.driver.SetAuthorizationStatus(false);
                    that.localStorageDriver.SetOnEnableContextMenu(false);
                    if (that.oneDrivePresenter) {
                        that.oneDrivePresenter.Destroy();
                        that.oneDrivePresenter = undefined;
                        oneDriveRepository = undefined;
                    }
                    console.log("Logout");
                };

                connector.Enable(onLogin, onLoginFailed, onLogout);
                
                //that.driver.SetOnSignInCallback(function () {
                //});

                //that.driver.SetOnSignOutCallback(function () {
                //});

                window.Commands.On("ModelStorageRequested", function () {
                    that.driver.Show();
                    window.Commands.Execute("LocalStorageRequested", undefined);
                });

                window.Commands.On("SaveModel", function () {
                    if (that.activePresenter == "local")
                        window.Commands.Execute("LocalStorageSaveModel", undefined);
                    else window.Commands.Execute("OneDriveStorageSaveModel", undefined);
                });
            }
        }
    }
}