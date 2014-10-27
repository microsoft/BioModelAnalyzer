module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private driver: BMA.UIDrivers.ILocalStorageDriver;
            private tool: BMA.UIDrivers.IModelRepository;
            private messagebox: BMA.UIDrivers.IMessageServise;

            constructor(
                appModel: BMA.Model.AppModel,
                editor: BMA.UIDrivers.ILocalStorageDriver,
                tool: BMA.UIDrivers.IModelRepository,
                messagebox: BMA.UIDrivers.IMessageServise
                ) {
                var that = this;
                this.driver = editor;
                this.tool = tool;
                var keys = that.tool.GetModelList();
                this.driver.SetItems(keys);
                this.driver.Hide();

                window.Commands.On("LocalStorageChanged", function () {
                    var keys = that.tool.GetModelList();
                    that.driver.SetItems(keys);
                });

                window.Commands.On("LocalStorageRemove", function (key) {
                    that.tool.RemoveModel(key);
                });

                window.Commands.On("LocalStorageRequested", function () {
                    that.driver.Show();
                });

                window.Commands.On("LocalStorageSaveModel", function () {
                    var key = appModel.BioModel.Name;
                    that.tool.SaveModel(key, JSON.parse(appModel.Serialize()));
                });

                window.Commands.On("LocalStorageLoadModel", function (key) {
                    if (that.tool.IsInRepo(key))
                        appModel.Reset(JSON.stringify(that.tool.LoadModel(key)));
                    else {
                        that.messagebox.Show("The model was removed from outside");
                        window.Commands.Execute("LocalStorageChanged", {});
                    }
                })
            }
        }
    }
}
