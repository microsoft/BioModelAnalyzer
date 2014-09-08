var BMA;
(function (BMA) {
    (function (Presenters) {
        var ModelStoragePresenter = (function () {
            function ModelStoragePresenter(appModel, fileLoaderDriver) {
                var that = this;

                window.Commands.On("NewModel", function (args) {
                    appModel.Reset();
                });

                window.Commands.On("ImportModel", function (args) {
                    fileLoaderDriver.OpenFileDialog().done(function (fileName) {
                        var fr = that.OpenFile(fileName);
                    });
                });

                window.Commands.On("ExportModel", function (args) {
                    var data = appModel.BioModel.GetJSON();
                    var json = JSON.stringify(data);
                    saveTextAs(json, appModel.BioModel.Name + ".json");
                });
            }
            ModelStoragePresenter.prototype.OpenFile = function (fileName) {
                var fileReader = new FileReader();

                //fileReader.onloadstart = callbacks["onloadstart"];
                //fileReader.onerror = callbacks["onerror"];
                //fileReader.onabort = callbacks["onabort"];
                //fileReader.onload = callbacks["onload"];
                //fileReader.onloadend = callbacks["onloadend"];
                fileReader.onload = function () {
                };

                fileReader.readAsText(fileName);

                return fileReader;
            };
            return ModelStoragePresenter;
        })();
        Presenters.ModelStoragePresenter = ModelStoragePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=modelstoragepresenter.js.map
