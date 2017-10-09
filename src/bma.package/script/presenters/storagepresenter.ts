// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module BMA {
    export module Presenters {
        export class StoragePresenter {
            private appModel: BMA.Model.AppModel;
            private appModelKey: string;
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
            private isConnectedToOneDrive: boolean = false;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.IModelStorageDriver,
                variableEditorDriver: BMA.UIDrivers.IVariableEditor,
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

                this.localStoragePresenter.SetOnRequestLoad(function (key) {
                    that.RequestLoadModel().done(function () {
                        that.localStoragePresenter.LoadModel(key);
                    });
                });

                var onLogin = function (oneDrive) {
                    that.isConnectedToOneDrive = true;
                    window.Commands.Execute("OneDriveLoggedIn", undefined);
                    that.driver.SetAuthorizationStatus(true);
                    //that.localStorageDriver.SetOnEnableContextMenu(true);
                    that.activePresenter = "oneDrive";

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
                    });

                    that.oneDrivePresenter.SetOnRequestLoad(function (key) {
                        that.RequestLoadModel().done(function () {
                            that.oneDrivePresenter.LoadModel(key);
                        });
                    });

                    that.driver.SetOnUpdateModelList(function () {
                        that.oneDrivePresenter.UpdateModelsList();
                    });
                };

                var onLoginFailed = function (failure) {
                    that.isConnectedToOneDrive = false;
                    that.activePresenter = "local";
                    console.error("Login failed: " + failure.error_description);
                };

                var onLogout = function (logout) {
                    that.isConnectedToOneDrive = false;
                    window.Commands.Execute("OneDriveLoggedOut", undefined);
                    that.activePresenter = "local";
                    that.driver.SetAuthorizationStatus(false);
                    //that.localStorageDriver.SetOnEnableContextMenu(false);
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
                    if (that.activePresenter == "local")
                        window.Commands.Execute("LocalStorageRequested", undefined);
                    else window.Commands.Execute("OneDriveStorageRequested", undefined);
                });

                window.Commands.On("SaveModel", function () {
                    if (variableEditorDriver != undefined)
                        variableEditorDriver.Hide();

                    if (that.activePresenter == "local")
                        window.Commands.Execute("LocalStorageSaveModel", undefined);
                    else window.Commands.Execute("OneDriveStorageSaveModel", undefined);
                });

                window.Commands.On("NewModel", function () {
                    if (variableEditorDriver != undefined)
                        variableEditorDriver.Hide();

                    if (that.activePresenter == "local")
                        that.localStorageDriver.SetOnUnselect();
                    else that.oneDriveStorageDriver.SetOnUnselect();
                });

                window.Commands.On("ModelReset", function () {
                    if (variableEditorDriver != undefined)
                        variableEditorDriver.Hide();

                    if (that.activePresenter == "local")
                        that.localStorageDriver.SetOnUnselect();
                    else that.oneDriveStorageDriver.SetOnUnselect();
                });

                window.Commands.On("TurnRepository", function (args) {
                    args.toggleFunc();
                });

                window.Commands.On("SwitchRepository", function (args) {
                    if (that.activePresenter === "oneDrive") {
                        that.activePresenter = "local";
                        that.driver.SetAuthorizationStatus(false);
                        window.Commands.Execute("OneDriveTurnedOff", undefined);
                    }
                    else
                    {
                        if (that.isConnectedToOneDrive) {
                            that.activePresenter = "oneDrive";
                            that.driver.SetAuthorizationStatus(true);
                            window.Commands.Execute("OneDriveLoggedIn", undefined);
                        } else {
                            args.toggleFunc();
                        }
                    }
                });

            }

            public RequestLoadModel() {
                var that = this;
                var deffered = $.Deferred();
                try {
                    if (that.checker.IsChanged(that.appModel)) {
                        var userDialog = $('<div></div>').appendTo('body').userdialog({
                            message: "Do you want to save changes?",
                            actions: [
                                {
                                    button: 'Yes',
                                    callback: function () {
                                        userDialog.detach();
                                        if (that.activePresenter == "local")
                                            window.Commands.Execute("LocalStorageSaveModel", {});
                                        else window.Commands.Execute("OneDriveStorageSaveModel", {});
                                        deffered.resolve();
                                        //load(key);
                                    }
                                },
                                {
                                    button: 'No',
                                    callback: function () {
                                        userDialog.detach();
                                        deffered.resolve();
                                        //load(key);
                                    }
                                },
                                {
                                    button: 'Cancel',
                                    callback: function () {
                                        userDialog.detach();
                                        deffered.reject();
                                    }
                                }
                            ]
                        });
                    }
                    else {
                        deffered.resolve();
                        //load(key);
                    }
                }
                catch (ex) {
                    this.messagebox.Show(ex);
                    deffered.resolve();
                    //load(key);
                }

                //function load(key) {
                //    that.waitScreen.Show();
                //    if (that.activePresenter == "local")
                //        that.localStoragePresenter && that.localStoragePresenter.LoadModel(key);
                //    else that.oneDrivePresenter && that.oneDrivePresenter.LoadModel(key);
                //    that.waitScreen.Hide();
                //}
                return deffered.promise();
            }
        }
    }
}
