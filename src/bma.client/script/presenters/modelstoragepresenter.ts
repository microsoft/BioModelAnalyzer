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
                                functions: [
                                    function () {
                                        userDialog.detach();
                                    },
                                    function () {
                                        userDialog.detach();
                                        load();
                                    },
                                    function () {
                                        userDialog.detach();
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
                                functions: [
                                    function () {
                                        userDialog.detach();
                                    },
                                    function () {
                                        userDialog.detach();
                                        load();
                                    },
                                    function () {
                                        userDialog.detach();
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
                    catch (ex) { alert(ex); }
                });
            }
        }
    }
} 