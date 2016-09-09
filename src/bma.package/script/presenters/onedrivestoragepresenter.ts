module BMA {
    export module Presenters {
        export class OneDriveStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.IOneDriveDriver;
            private tool: BMA.OneDrive.OneDriveRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private setOnCopy: Function;
            private setOnActive: Function;
            private setOnIsActive: Function;

            private commandsIds: any[] = [];

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
                //this.commandsIds = [];

                that.UpdateModelsList();

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

                that.commandsIds.push({ commandName: "OneDriveStorageChanged", id: window.Commands.On("OneDriveStorageChanged", () => that.UpdateModelsList()) });
                
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
                                            load();
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
                                        callback: function () {
                                            userDialog.detach();
                                        }
                                    }
                                ]
                            });
                        }
                        else {
                            load();
                        }
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
                                that.driver.SetActiveModel(that.appModel.BioModel.Name);
                                if (that.setOnActive !== undefined)
                                    that.setOnActive();
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

                that.commandsIds.push({ commandName: "OneDriveStorageRequested", id: window.Commands.On("OneDriveStorageRequested", () => that.UpdateModelsList()) });

                that.commandsIds.push({
                    commandName: "OneDriveStorageSaveModel", id: window.Commands.On("OneDriveStorageSaveModel", function () {
                        try {
                            logService.LogSaveModel();
                            var key = appModel.BioModel.Name;
                            that.tool.SaveModel(key, JSON.parse(appModel.Serialize())).done(function () {
                                window.Commands.Execute("OneDriveStorageChanged", {});
                                that.checker.Snapshot(that.appModel);
                            });
                        }
                        catch (ex) {
                            alert("Couldn't save model: " + ex);
                        }
                    })
                });
            }

            public SetOnCopyCallback(callback: Function) {
                this.setOnCopy = callback;
            }

            public UpdateModelsList() {
                var that = this;
                that.tool.GetModelList().done(function (modelsInfo) {
                    if (modelsInfo === undefined || modelsInfo.length == 0)
                        that.driver.Message("The model repository is empty");
                    else that.driver.Message('');
                    that.driver.SetItems(modelsInfo);
                    if (that.setOnIsActive !== undefined && that.setOnIsActive())
                        that.driver.SetActiveModel(that.appModel.BioModel.Name);
                }).fail(function (errorThrown) {
                    var res = JSON.parse(JSON.stringify(errorThrown));
                    that.messagebox.Show(res.statusText);
                    that.driver.SetItems([]);
                });
            }

            public SetOnActiveCallback(callback: Function) {
                this.setOnActive = callback;
            }

            public SetOnIsActive(callback: Function) {
                this.setOnIsActive = callback;
            }
            
            public Destroy() {
                var that = this;
                for (var i = 0; i < that.commandsIds.length; i++) 
                    window.Commands.OffById(that.commandsIds[i].commandName, that.commandsIds[i].id);
            }
        }
    }
}