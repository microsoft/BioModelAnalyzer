var BMA;
(function (BMA) {
    (function (Presenters) {
        var ModelStoragePresenter = (function () {
            function ModelStoragePresenter(appModel, fileLoaderDriver) {
                var that = this;

                window.Commands.On("NewModel", function (args) {
                    appModel.Reset(undefined);
                });

                window.Commands.On("ImportModel", function (args) {
                    fileLoaderDriver.OpenFileDialog().done(function (fileName) {
                        var fileReader = new FileReader();
                        fileReader.onload = function () {
                            appModel.Reset(fileReader.result);
                        };
                        fileReader.readAsText(fileName);
                    });
                });

                window.Commands.On("ExportModel", function (args) {
                    var data = appModel.Serialize();
                    saveTextAs(data, appModel.BioModel.Name + ".json");
                });
            }
            return ModelStoragePresenter;
        })();
        Presenters.ModelStoragePresenter = ModelStoragePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=modelstoragepresenter.js.map
