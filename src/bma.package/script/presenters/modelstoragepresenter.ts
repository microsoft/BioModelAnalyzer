module BMA {
    export module Presenters {
        export class ModelStoragePresenter {
            constructor(
                appModel: BMA.Model.AppModel,
                fileLoaderDriver: BMA.UIDrivers.IFileLoader,
                checker: BMA.UIDrivers.ICheckChanges,
                logService: BMA.ISessionLog,
                exportService: BMA.UIDrivers.ExportService) {
                var that = this;

                window.Commands.On("NewModel",(args) => {
                    try {
                        if (checker.IsChanged(appModel)) {
                            var userDialog = $('<div></div>').appendTo('body').userdialog({
                                message: "Do you want to save changes?",
                                actions: [
                                    {
                                        button: 'Yes',
                                        callback: function () { userDialog.detach(); }
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
                        load();
                    }

                    function load() {
                        window.Commands.Execute('SetPlotSettings', { MaxWidth: 3200, MinWidth: 800 });
                        window.Commands.Execute('ModelFitToView', '');
                        appModel.Deserialize(undefined);
                        checker.Snapshot(appModel);
                        logService.LogNewModelCreated();
                    }
                });

                window.Commands.On("ImportModel", (args) => {
                    try {
                        if (checker.IsChanged(appModel)) {
                            var userDialog = $('<div></div>').appendTo('body').userdialog({
                                message: "Do you want to save changes?",
                                actions: [
                                    {
                                        button: 'Yes',
                                        callback: function () { userDialog.detach(); }
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
                        else {
                            logService.LogImportModel();
                            load();
                        }
                    }
                    catch (ex) {
                        alert(ex);
                        logService.LogImportModel();
                        load();
                    }

                    function load() {
                        window.Commands.Execute('SetPlotSettings', { MaxWidth: 3200, MinWidth: 800 });
                        window.Commands.Execute('ModelFitToView', '');
                        fileLoaderDriver.OpenFileDialog().done(function (fileName) {
                            var fileReader: any = new FileReader();
                            fileReader.onload = function () {
                                var fileContent = fileReader.result;

                                try {
                                var data = $.parseXML(fileContent);
                                var model = BMA.ParseXmlModel(data, window.GridSettings);
                                appModel.Reset(model.Model, model.Layout);
                                
                                }
                                catch (exc) {
                                    console.log("XML parsing failed: " + exc + ". Trying JSON");
                                    try {
                                        appModel.Deserialize(fileReader.result);
                                    }
                                    catch (exc2) {
                                        console.log("JSON failed: " + exc + ". Trying legacy JSON version");
                                        appModel.DeserializeLegacyJSON(fileReader.result);
                                    }
                                }
                                checker.Snapshot(appModel);
                            };
                            fileReader.readAsText(fileName);
                        });
                        
                    }
                });

                window.Commands.On("ExportModel",(args) => {
                    try {
                        var data = appModel.Serialize();
                        exportService.Export(data, appModel.BioModel.Name, 'json');
                        //var ret = saveTextAs(data, appModel.BioModel.Name + ".json");
                        checker.Snapshot(appModel);
                    }
                    catch (ex) {
                        alert("Couldn't export model: " + ex);
                    }
                });
            }
        }
    }
} 