﻿module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.ILocalStorageDriver;
            private tool: BMA.UIDrivers.IModelRepository;
            private messagebox: BMA.UIDrivers.IMessageServise;
            private checker: BMA.UIDrivers.ICheckChanges;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.ILocalStorageDriver,
                tool: BMA.UIDrivers.IModelRepository,
                messagebox: BMA.UIDrivers.IMessageServise,
                checker: BMA.UIDrivers.ICheckChanges
                ) {
                var that = this;
                this.appModel = appModel;
                this.driver = editor;
                this.tool = tool;
                this.messagebox = messagebox;
                this.checker = checker;

                var keys = that.tool.GetModelList();
                this.driver.SetItems(keys);
                this.driver.Hide();

                window.Commands.On("LocalStorageChanged", function () {
                    var keys = that.tool.GetModelList();
                    if (keys === undefined || keys.length == 0) 
                        that.driver.Message("The model repository is empty");
                    else that.driver.Message('');
                    that.driver.SetItems(keys);
                });

                window.Commands.On("LocalStorageRemoveModel", function (key) {
                    that.tool.RemoveModel(key);
                });

                window.Commands.On("LocalStorageRequested", function () {
                    that.driver.Show();
                });

                window.Commands.On("LocalStorageSaveModel", function () {
                    var key = appModel.BioModel.Name;
                    that.tool.SaveModel(key, JSON.parse(appModel.Serialize()));
                    that.checker.Snapshot(that.appModel);
                });

                window.Commands.On("LocalStorageLoadModel", function (key) {
                    if (that.checker.IsChanged(that.appModel)) {
                        var userDialog = $('<div id="usrdialog"></div>').appendTo('body').userdialog({
                            message: "Do you want to save changes?",
                            functions: [
                                function () {
                                    $('#usrdialog').detach();
                                    window.Commands.Execute("LocalStorageSaveModel", {});
                                },
                                function () {
                                    $('#usrdialog').detach();
                                    load();
                                },
                                function () {
                                    $('#usrdialog').detach();
                                }
                            ]
                        });
                    }
                    else load();

                    function load() {
                        if (that.tool.IsInRepo(key)) {
                            appModel.Reset(JSON.stringify(that.tool.LoadModel(key)));
                            that.checker.Snapshot(that.appModel);
                        }
                        else {
                            that.messagebox.Show("The model was removed from outside");
                            window.Commands.Execute("LocalStorageChanged", {});
                        }
                    }
                });

                window.Commands.On("LocalStorageInitModel", function (key) {
                    if (that.tool.IsInRepo(key)) {
                        appModel.Reset(JSON.stringify(that.tool.LoadModel(key)));
                        that.checker.Snapshot(that.appModel);
                    }
                });

                window.Commands.Execute("LocalStorageChanged", {});
            }
        }
    }
}
