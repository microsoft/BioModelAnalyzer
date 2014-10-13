module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private driver: BMA.UIDrivers.ILocalStorageDriver;

            constructor(appModel: BMA.Model.AppModel, editor: BMA.UIDrivers.ILocalStorageDriver) {
                var that = this;
                this.driver = editor;
                window.Commands.On("LocalStorageRequested", function () {

                    for (var i = 0; i < window.localStorage.length; i++) {
                        var key = window.localStorage.key(i);
                        var item = window.localStorage.getItem(key);
                        var model = that.ParseItem(item);
                        if (model !== undefined) {
                            that.driver.AddItem(key, model);
                        }
                    }
                    that.driver.Show();
                });

                window.Commands.On("LocalStorageSave", function () {
                    if (window.localStorage.getItem(appModel.BioModel.Name) !== undefined)
                        alert(window.localStorage.getItem(appModel.BioModel.Name));

                    window.localStorage.setItem(appModel.BioModel.Name, appModel.Serialize());
                    //alert(appModel.BioModel.Name);
                });
            }

            public ParseItem(item) {

            }
        }
    }
}
