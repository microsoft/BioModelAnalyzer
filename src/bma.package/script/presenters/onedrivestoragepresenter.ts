module BMA {
    export module Presenters {
        export class OneDriveStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.IOneDriveDriver;
            private tool: BMA.OneDrive.OneDriveRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private setOnCopy: Function;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.IOneDriveDriver,
                tool: BMA.OneDrive.OneDriveRepository,
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

                that.tool.GetModelList().done(function (modelsInfo) {
                    if (modelsInfo === undefined || modelsInfo.length == 0)
                        that.driver.Message("The model repository is empty");
                    else that.driver.Message('');
                    that.driver.SetItems(modelsInfo);
                    
                    //that.driver.SetItems(items);
                    //that.driver.Hide();
                }).fail(function (errorThrown) {
                    var res = JSON.parse(JSON.stringify(errorThrown));
                    that.messagebox.Show(res.statusText);
                    //alert("Failed to load models");
                    });

                that.driver.SetOnRemoveModel(function (key) {
                    that.tool.RemoveModel(key).done(function (result) {
                        if (result)
                            window.Commands.Execute("OneDriveStorageChanged", {});
                    }).fail(function () {
                        that.messagebox.Show("Failed to remove model");
                    });
                });

                that.driver.SetOnShareCallback(function (key) { });

                that.driver.SetOnActiveShareCallback(function (key) { });

                that.driver.SetOnOpenBMALink(function (key) { });

                that.driver.SetOnCopyToLocalCallback(function (modelInfo) {
                    var deffered = $.Deferred();
                    if (that.tool.IsInRepo(modelInfo.id)) {
                        that.tool.LoadModel(modelInfo.id).done(function (result) {
                            if (that.setOnCopy !== undefined) {
                                that.setOnCopy(modelInfo.name, result);
                                deffered.resolve();
                            } else deffered.reject();
                        }).fail(function (errorThrown) {
                            var res = JSON.parse(JSON.stringify(errorThrown));
                            that.messagebox.Show(res.statusText);
                            deffered.reject();
                        });
                    }
                    else {
                        that.messagebox.Show("The model was removed from outside");
                        window.Commands.Execute("OneDriveStorageChanged", {});
                        deffered.reject();
                    }
                    return deffered.promise();
                });

                window.Commands.On("OneDriveStorageChanged", function () {
                    that.tool.GetModelList().done(function (modelsInfo) {
                        if (modelsInfo === undefined || modelsInfo.length == 0)
                            that.driver.Message("The model repository is empty");
                        else that.driver.Message('');
                        //var items = [];
                        //for (var i = 0; i < modelsInfo.length; i++) {
                        //    that.tool.IsInRepo(modelsInfo[i].id).done(function (exists) {
                        //        if (exists) items.push(modelsInfo[i]);
                        //    })
                        //}
                        that.driver.SetItems(modelsInfo);
                    }).fail(function (errorThrown) {
                        var res = JSON.parse(JSON.stringify(errorThrown));
                        that.messagebox.Show(res.statusText);
                    });
                });

                that.driver.SetOnLoadModel(function (key) {
                    try {
                        if (that.checker.IsChanged(that.appModel)) {
                            var userDialog = $('<div></div>').appendTo('body').userdialog({
                                message: "Do you want to save changes?",
                                actions: [
                                    {
                                        button: 'Yes',
                                        callback: function () {
                                            userDialog.detach();
                                            window.Commands.Execute("OneDriveStorageSaveModel", {});
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
                                var res = JSON.parse(JSON.stringify(result));
                                that.messagebox.Show(res.statusText);
                            });
                        }
                        else {
                            that.messagebox.Show("The model was removed from outside");
                            window.Commands.Execute("OneDriveStorageChanged", {});
                        }
                        waitScreen.Hide();
                    }
                });

                window.Commands.On("OneDriveStorageRequested", function () {
                    that.tool.GetModelList().done(function (keys) {
                        that.driver.SetItems(keys);
                        //that.driver.Show();
                    }).fail(function (errorThrown) {
                        alert(errorThrown);
                    });
                });

                window.Commands.On("OneDriveStorageSaveModel", function () {
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
            }

            public SetOnCopyCallback(callback: Function) {
                this.setOnCopy = callback;
            }
        }
    }
}