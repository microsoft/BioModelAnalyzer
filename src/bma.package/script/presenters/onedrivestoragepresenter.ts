// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module BMA {
    export module Presenters {
        export class OneDriveStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.IOneDriveDriver;
            private tool: BMA.OneDrive.OneDriveRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private waitScreen: BMA.UIDrivers.IWaitScreen;
            private setOnCopy: Function;
            private setOnActive: Function;
            private setOnIsActive: Function;
            private setRequestLoad: Function;

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
                this.waitScreen = waitScreen;
                //this.commandsIds = [];

                that.UpdateModelsList();

                that.driver.SetOnRemoveModel(function (key) {
                    that.driver.SetOnLoading(true);
                    that.tool.RemoveModel(key).done(function (result) {
                        that.driver.Message("");
                        if (result)
                            window.Commands.Execute("OneDriveStorageChanged", {});
                        //that.driver.SetOnLoading(false);
                    }).fail(function () {
                        //that.messagebox.Show("Failed to remove model");
                        that.driver.Message("Failed to remove model");
                        that.driver.SetOnLoading(false);
                    });
                });

                that.driver.SetOnShareCallback(function (key) { });

                that.driver.SetOnActiveShareCallback(function (key) { });

                that.driver.SetOnOpenBMALink(function (key) { });

                that.driver.SetOnCopyToLocalCallback(function (modelInfo) {
                    var deffered = $.Deferred();
                    if (that.tool.IsInRepo(modelInfo.id)) {
                        that.tool.LoadModel(modelInfo.id).done(function (result) {
                            that.driver.Message("");
                            if (that.setOnCopy !== undefined) {
                                that.setOnCopy(modelInfo.name, result);
                                deffered.resolve();
                            } else deffered.reject();
                        }).fail(function (errorThrown) {
                            var res = JSON.parse(JSON.stringify(errorThrown));
                            //that.messagebox.Show(res.statusText);
                            that.driver.Message(res.statusText);
                            deffered.reject();
                        });
                    }
                    else {
                        //that.messagebox.Show("The model was removed from outside");
                        that.driver.Message("The model was removed from outside");
                        window.Commands.Execute("OneDriveStorageChanged", {});
                        deffered.reject();
                    }
                    return deffered.promise();
                });

                that.commandsIds.push({ commandName: "OneDriveStorageChanged", id: window.Commands.On("OneDriveStorageChanged", () => that.UpdateModelsList()) });
                
                that.driver.SetOnRequestLoadModel(function (modelInfo) {
                    if (that.setRequestLoad !== undefined)
                        that.setRequestLoad(modelInfo);
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
                            that.driver.Message("Couldn't save model: " + ex);
                            //that.messagebox.Show("Couldn't save model: " + ex);
                        }
                    })
                });
            }

            public SetOnCopyCallback(callback: Function) {
                this.setOnCopy = callback;
            }

            public UpdateModelsList() {
                var that = this;
                that.driver.SetOnLoading(true);
                that.tool.GetModelList().done(function (modelsInfo) {
                    if (modelsInfo === undefined || modelsInfo.length == 0)
                        that.driver.Message("The model repository is empty");
                    else that.driver.Message('');
                    that.driver.SetItems(modelsInfo);
                    if (that.setOnIsActive !== undefined && that.setOnIsActive())
                        that.driver.SetActiveModel(that.appModel.BioModel.Name);
                    that.driver.SetOnLoading(false);
                }).fail(function (errorThrown) {
                    var res = JSON.parse(JSON.stringify(errorThrown));
                    //that.messagebox.Show(res.statusText);
                    that.driver.Message(res.statusText);
                    that.driver.SetItems([]);
                    that.driver.SetOnLoading(false);
                });
            }

            public SetOnActiveCallback(callback: Function) {
                this.setOnActive = callback;
            }

            public SetOnRequestLoad(callback: Function) {
                this.setRequestLoad = callback;
            }

            public SetOnIsActive(callback: Function) {
                this.setOnIsActive = callback;
            }

            public LoadModel(modelInfo) {
                var that = this;
                that.waitScreen.Show();
                if (that.tool.IsInRepo(modelInfo.id)) {
                    that.tool.LoadModel(modelInfo.id).done(function (result) {
                        that.appModel.Deserialize(JSON.stringify(result));
                        that.checker.Snapshot(that.appModel);
                        that.driver.SetActiveModel(modelInfo.name);
                        that.driver.Message("");
                        if (that.setOnActive !== undefined)
                            that.setOnActive();
                        that.waitScreen.Hide();
                    }).fail(function (result) {
                        var res = JSON.parse(JSON.stringify(result));
                        //that.messagebox.Show(res.statusText);
                        that.driver.Message(res.statusText);
                        that.waitScreen.Hide();
                    });
                }
                else {
                    //that.messagebox.Show("The model was removed from outside");
                    that.driver.Message("The model was removed from outside");
                    window.Commands.Execute("OneDriveStorageChanged", {});
                    that.waitScreen.Hide();
                }
            }
            
            public Destroy() {
                var that = this;
                for (var i = 0; i < that.commandsIds.length; i++) 
                    window.Commands.OffById(that.commandsIds[i].commandName, that.commandsIds[i].id);
            }
        }
    }
}
