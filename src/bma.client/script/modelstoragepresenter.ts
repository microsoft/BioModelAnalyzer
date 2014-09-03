module BMA {
    export module Presenters {
        export class ModelStoragePresenter {
            constructor(appModel: BMA.Model.AppModel, fileLoaderDriver : BMA.UIDrivers.IFileLoader) {
                var that = this;

                window.Commands.On("NewModel", (args) => {
                    appModel.Reset(undefined);
                });

                window.Commands.On("ImportModel", (args) => {
                    fileLoaderDriver.OpenFileDialog().done(function (fileName) {
                        var fileReader: any = new FileReader(); 
                        fileReader.onload = function () {
                            appModel.Reset(fileReader.result);
                        };
                        fileReader.readAsText(fileName); 
                    });
                });

                window.Commands.On("ExportModel", (args) => {
                    var data = appModel.Serialize();
                    saveTextAs(data, appModel.BioModel.Name + ".json");
                });
            }
        }
    }
} 