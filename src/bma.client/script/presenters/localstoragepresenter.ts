module BMA {
    export module Presenters {
        export class LocalStoragePresenter {
            private driver: BMA.UIDrivers.ILocalStorageDriver;

            constructor(appModel: BMA.Model.AppModel, editor: BMA.UIDrivers.ILocalStorageDriver) {
                var that = this;
                this.driver = editor;
                var keys = that.ScanLocalStorage();
                this.driver.SetItems(keys);
                this.driver.Hide();

                window.Commands.On("LocalStorageChanged", function (arg) {
                    if (arg !== undefined && window.localStorage.getItem(arg) === undefined) {
                        that.driver.AddItem(arg, {});
                    }
                    else {
                        var keys = that.ScanLocalStorage();
                        that.driver.SetItems(keys);
                    }
                });

                window.Commands.On("LocalStorageRequested", function () {
                    that.driver.Show();
                });

                window.Commands.On("LocalStorageSave", function () {
                    var key = appModel.BioModel.Name;
                    //if (window.localStorage.getItem(key) !== undefined)
                    //    alert(window.localStorage.getItem(key));

                    window.localStorage.setItem(key, appModel.Serialize());
                    window.Commands.Execute("LocalStorageChanged", {});
                });

                window.Commands.On("LocalStorageOpen", function (key) {
                    appModel.Reset(window.localStorage.getItem(key));
                })
            }

            public ParseItem(item) {
                return 1;
            }

            public ScanLocalStorage(): any[] {
                var keys = [];
                for (var i = 0; i < window.localStorage.length; i++) {
                    var key = window.localStorage.key(i);
                    var item = window.localStorage.getItem(key);
                    var model = this.ParseItem(item);
                    if (model !== undefined) {
                        keys.push(key);
                    }
                }
                return keys;
            }
        }
    }
}
