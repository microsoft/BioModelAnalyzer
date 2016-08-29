module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.ILocalStorageDriver;
            private tool: BMA.UIDrivers.IModelRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private setOnCopy: Function;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.ILocalStorageDriver,
                tool: BMA.UIDrivers.IModelRepository,
                messagebox: BMA.UIDrivers.IMessageServiсe,
                checker: BMA.UIDrivers.ICheckChanges,
                logService: BMA.ISessionLog,
                waitScreen: BMA.UIDrivers.IWaitScreen
                ) {
                var that = this;
                this.appModel = appModel;
                this.driver = editor;
                this.tool = tool; 
                this.messagebox = messagebox;
                this.checker = checker;

                that.tool.GetModelList().done(function (keys) {
                    that.driver.SetItems(keys);
                    //that.driver.Hide();
                }).fail(function (errorThrown) {
                    alert(errorThrown);
                });

                window.Commands.On("LocalStorageChanged", function () {
                    that.tool.GetModelList().done(function (keys) {
                        if (keys === undefined || keys.length == 0)
                            that.driver.Message("The model repository is empty");
                        else that.driver.Message('');
                        that.driver.SetItems(keys);
                    }).fail(function (errorThrown) {
                        alert(errorThrown);
                    });
                });

                //window.Commands.On("LocalStorageRemoveModel", function (key) {
                //    that.tool.RemoveModel(key);
                //});

                that.driver.SetOnRemoveModel(function (key) {
                    that.tool.RemoveModel(key);
                });

                window.Commands.On("LocalStorageRequested", function () {
                    that.tool.GetModelList().done(function (keys) {
                        that.driver.SetItems(keys);
                        //that.driver.Show();
                    }).fail(function (errorThrown) {
                        alert(errorThrown);
                    });
                });

                window.Commands.On("LocalStorageSaveModel", function () {
                    try {
                        logService.LogSaveModel();
                        var key = appModel.BioModel.Name;
                        that.tool.SaveModel(key, JSON.parse(appModel.Serialize()));
                        that.checker.Snapshot(that.appModel);
                    }
                    catch (ex) {
                        alert("Couldn't save model: " + ex);
                    }
                });

                that.driver.SetOnCopyToOneDriveCallback(function (key) {
                    if (that.tool.IsInRepo(key)) {
                        that.tool.LoadModel(key).done(function (result) {
                            if (that.setOnCopy !== undefined)
                                that.setOnCopy(key, result);
                        }).fail(function (result) {
                            that.messagebox.Show(JSON.stringify(result));
                        });
                    }
                    else {
                        that.messagebox.Show("The model was removed from outside");
                        window.Commands.Execute("OneDriveStorageChanged", {});
                    }
                });

                that.driver.SetOnLoadModel(function (key) {
                //window.Commands.On("LocalStorageLoadModel", function (key) {
                    try {
                        if (that.checker.IsChanged(that.appModel)) {
                            var userDialog = $('<div></div>').appendTo('body').userdialog({
                                message: "Do you want to save changes?",
                                actions: [
                                    {
                                        button: 'Yes',
                                        callback: function () {
                                            userDialog.detach();
                                            window.Commands.Execute("LocalStorageSaveModel", {});
                                        }
                                    },
                                    {
                                        button: 'No',
                                        callback: function () {
                                            userDialog.detach();
                                            load();
                                        }
                                    },
                                    {
                                        button: 'Cancel',
                                        callback: function () { userDialog.detach(); }
                                    }
                                ]
                            });
                        }
                        else load();
                    }
                    catch (ex) {
                        alert(ex);
                        load();
                    }

                    function load() {
                        waitScreen.Show();
                        if (that.tool.IsInRepo(key)) {
                            that.tool.LoadModel(key).done(function (result) {
                                appModel.Deserialize(JSON.stringify(result));
                                that.checker.Snapshot(that.appModel);
                            }).fail(function (result) {
                                that.messagebox.Show(JSON.stringify(result));
                            });
                        }
                        else {
                            that.messagebox.Show("The model was removed from outside");
                            window.Commands.Execute("LocalStorageChanged", {});
                        }
                        waitScreen.Hide();
                    }
                });

                window.Commands.On("LocalStorageInitModel", function (key) {
                    if (that.tool.IsInRepo(key)) {
                        that.tool.LoadModel(key).done(function (result) {
                            appModel.Deserialize(JSON.stringify(result));
                            that.checker.Snapshot(that.appModel);
                        }).fail(function (result) {
                            that.messagebox.Show(JSON.stringify(result));
                        });
                    }
                });

                window.Commands.Execute("LocalStorageChanged", {});
            }

            public SetOnCopyCallback(callback: Function) {
                this.setOnCopy = callback;
            }
        }
    }
}
