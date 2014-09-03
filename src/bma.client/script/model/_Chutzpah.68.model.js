/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
var BMA;
(function (BMA) {
    (function (Model) {
        var AppModel = (function () {
            function AppModel() {
                this.model = new BMA.Model.BioModel("model 1", [], []);
                this.layout = new BMA.Model.Layout([], []);
            }
            Object.defineProperty(AppModel.prototype, "BioModel", {
                get: function () {
                    return this.model;
                },
                set: function (value) {
                    this.model = value;
                    //TODO: update inner components (analytics)
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
                    //TODO: update inner components (analytics)
                },
                enumerable: true,
                configurable: true
            });


            Object.defineProperty(AppModel.prototype, "ProofResult", {
                get: function () {
                    return this.proofResult;
                },
                set: function (value) {
                    this.proofResult = value;
                },
                enumerable: true,
                configurable: true
            });


            AppModel.prototype.Reset = function (serializedModel) {
                if (serializedModel !== undefined) {
                    var ml = JSON.parse(serializedModel);

                    //TODO: verify model
                    this.model = ml.model;
                    this.layout = ml.layout;
                } else {
                    this.model = new BMA.Model.BioModel("model 1", [], []);
                    this.layout = new BMA.Model.Layout([], []);
                }

                this.proofResult = undefined;
                window.Commands.Execute("ModelReset", undefined);
            };

            AppModel.prototype.Serialize = function () {
                return JSON.stringify({ model: this.model, layout: this.layout });
            };
            return AppModel;
        })();
        Model.AppModel = AppModel;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
