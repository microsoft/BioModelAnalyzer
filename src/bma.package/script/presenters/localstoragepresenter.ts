// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private appModel: BMA.Model.AppModel;
            private driver: BMA.UIDrivers.ILocalStorageDriver;
            private tool: BMA.UIDrivers.IModelRepository;
            private messagebox: BMA.UIDrivers.IMessageServiсe;
            private checker: BMA.UIDrivers.ICheckChanges;
            private waitScreen: BMA.UIDrivers.IWaitScreen;
            private setOnCopy: Function;
            private setOnActive: Function;
            private setOnIsActive: Function;
            private setRequestLoad: Function;

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
                this.waitScreen = waitScreen;

                that.tool.GetModelList().done(function (keys) {
                    that.driver.SetItems(keys);
                    that.driver.Message("");
                    //that.driver.Hide();
                }).fail(function (errorThrown) {
                    var res = JSON.parse(JSON.stringify(errorThrown));
                    that.driver.Message(res.statusText);
                });

                window.Commands.On("LocalStorageChanged", function () {
                    that.tool.GetModelList().done(function (keys) {
                        if (keys === undefined || keys.length == 0)
                            that.driver.Message("The model repository is empty");
                        else that.driver.Message('');
                        that.driver.SetItems(keys);

                        if (that.setOnIsActive !== undefined && that.setOnIsActive())
                            that.driver.SetActiveModel(that.appModel.BioModel.Name);
                    }).fail(function (errorThrown) {
                        var res = JSON.parse(JSON.stringify(errorThrown));
                        that.driver.Message(res.statusText);
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
                        that.driver.Message("");
                        //that.driver.Show();
                    }).fail(function (errorThrown) {
                        var res = JSON.parse(JSON.stringify(errorThrown));
                        that.driver.Message(res.statusText);
                    });
                });

                window.Commands.On("LocalStorageSaveModel", function () {
                    try {
                        logService.LogSaveModel();
                        var key = appModel.BioModel.Name;
                        that.tool.SaveModel(key, JSON.parse(appModel.Serialize()));
                        window.Commands.Execute("LocalStorageChanged", {});
                        that.checker.Snapshot(that.appModel);
                    }
                    catch (ex) {
                        that.driver.Message("Couldn't save model: " + ex);
                    }
                });

                that.driver.SetOnCopyToOneDriveCallback(function (key) {
                    var deffered = $.Deferred();
                    if (that.tool.IsInRepo(key)) {
                        that.tool.LoadModel(key).done(function (result) {
                            that.driver.Message("");
                            if (that.setOnCopy !== undefined) {
                                var sp = key.split('.');
                                if (sp[0] === "user") {
                                    var q = sp[1];
                                    for (var i = 2; i < sp.length; i++) {
                                        q = q.concat('.');
                                        q = q.concat(sp[i]);
                                    }
                                    that.setOnCopy(q, result);
                                    deffered.resolve();
                                }
                            }
                            deffered.reject();
                        }).fail(function (error) {
                            var res = JSON.parse(JSON.stringify(error));
                            //that.messagebox.Show(res.statusText);
                            that.driver.Message(res.statusText);
                            deffered.reject();
                        });
                    }
                    else {
                        //that.messagebox.Show("The model was removed from outside");
                        that.driver.Message("The model was removed from outside");
                        window.Commands.Execute("LocalStorageChanged", {});
                        deffered.reject();
                    }
                    return deffered.promise();
                });

                that.driver.SetOnRequestLoadModel(function (key) {
                    //window.Commands.On("LocalStorageLoadModel", function (key) {
                    if (that.setRequestLoad !== undefined)
                        that.setRequestLoad(key);
                });

                window.Commands.On("LocalStorageInitModel", function (key) {
                    if (that.tool.IsInRepo(key)) {
                        that.tool.LoadModel(key).done(function (result) {
                            that.driver.Message("");
                            appModel.Deserialize(JSON.stringify(result));
                            that.checker.Snapshot(that.appModel);
                        }).fail(function (result) {
                            var res = JSON.parse(JSON.stringify(result));
                            that.driver.Message(res);
                            //that.messagebox.Show(JSON.stringify(result));
                        });
                    }
                });

                window.Commands.Execute("LocalStorageChanged", {});
            }

            public SetOnCopyCallback(callback: Function) {
                this.setOnCopy = callback;
            }

            public SetOnRequestLoad(callback: Function) {
                this.setRequestLoad = callback;
            }

            public SetOnActiveCallback(callback: Function) {
                this.setOnActive = callback;
            }

            public SetOnIsActive(callback: Function) {
                this.setOnIsActive = callback;
            }

            public LoadModel(key) {
                var that = this;
                that.waitScreen.Show();
                if (that.tool.IsInRepo(key)) {
                    that.tool.LoadModel(key).done(function (result) {
                        that.driver.Message("");
                        that.appModel.Deserialize(JSON.stringify(result));
                        that.checker.Snapshot(that.appModel);
                        that.driver.SetActiveModel(key);
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
                    window.Commands.Execute("LocalStorageChanged", {});
                    that.waitScreen.Hide();
                }
            }
        }
    }
}
