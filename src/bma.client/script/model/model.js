var BMA;
(function (BMA) {
    (function (Model) {
        var AppModel = (function () {
            function AppModel() {
                this.model = new BMA.Model.BioModel([], []);
                this.layout = new BMA.Model.Layout([], []);
            }
            Object.defineProperty(AppModel.prototype, "BioModel", {
                get: function () {
                    return this.model;
                },
                set: function (value) {
                    this.model = value;
                },
                enumerable: true,
                configurable: true
            });


            Object.defineProperty(AppModel.prototype, "Layout", {
                get: function () {
                    return this.layout;
                },
                set: function (value) {
                    this.layout = value;
                },
                enumerable: true,
                configurable: true
            });

            return AppModel;
        })();
        Model.AppModel = AppModel;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
