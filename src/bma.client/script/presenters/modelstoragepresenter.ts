module BMA {
    export module Presenters {
        export class ModelStoragePresenter {
            constructor(appModel: BMA.Model.AppModel, fileLoaderDriver: BMA.UIDrivers.IFileLoader, checker: BMA.UIDrivers.ICheckChanges, logService: BMA.ISessionLog) {
                var that = this;

                window.Commands.On("NewModel", (args) => {
                    if (checker.IsChanged(appModel)) {
                        var userDialog = $('<div id="usrdialog"></div>').appendTo('body').userdialog({
                            message: "Do you want to save changes?",
                            functions: [
                                function () {
                                    $('#usrdialog').detach();
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
                    else load()

                    function load() {
                        appModel.Deserialize(undefined);
                        checker.Snapshot(appModel);
                    }
                });

                window.Commands.On("ImportModel", (args) => {

                    if (checker.IsChanged(appModel)) {
                        var userDialog = $('<div id="usrdialog"></div>').appendTo('body').userdialog({
                            message: "Do you want to save changes?",
                            functions: [
                                function () {
                                    $('#usrdialog').detach();
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
                    else {
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
                                    console.log(exc);
                                    appModel.Deserialize(fileReader.result);
                                }
                                checker.Snapshot(appModel);
                            };
                            fileReader.readAsText(fileName);
                        });
                        
                    }
                });

                window.Commands.On("ExportModel", (args) => {
                    var data = appModel.Serialize();
                    var ret = saveTextAs(data, appModel.BioModel.Name + ".json");
                    checker.Snapshot(appModel);
                });
            }
        }
    }
} 