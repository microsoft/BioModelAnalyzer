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

                    if (ml === undefined || ml.model === undefined || ml.layout === undefined || ml.model.variables === undefined || ml.layout.variables === undefined || ml.model.variables.length !== ml.layout.variables.length || ml.layout.containers === undefined || ml.model.relationships === undefined) {
                        alert("Invalid model");
                        return;
                    }

                    var variables = [];
                    for (var i = 0; i < ml.model.variables.length; i++) {
                        variables.push(new BMA.Model.Variable(ml.model.variables[i].id, ml.model.variables[i].containerId, ml.model.variables[i].type, ml.model.variables[i].name, ml.model.variables[i].rangeFrom, ml.model.variables[i].rangeTo, ml.model.variables[i].formula));
                    }

                    var variableLayouts = [];
                    for (var i = 0; i < ml.layout.variables.length; i++) {
                        variableLayouts.push(new BMA.Model.VarialbeLayout(ml.layout.variables[i].id, ml.layout.variables[i].positionX, ml.layout.variables[i].positionY, ml.layout.variables[i].cellX, ml.layout.variables[i].cellY, ml.layout.variables[i].angle));
                    }

                    var relationships = [];
                    for (var i = 0; i < ml.model.relationships.length; i++) {
                        relationships.push(new BMA.Model.Relationship(ml.model.relationships[i].fromVariableId, ml.model.relationships[i].toVariableId, ml.model.relationships[i].type));
                    }

                    var containers = [];
                    for (var i = 0; i < ml.layout.containers.length; i++) {
                        containers.push(new BMA.Model.ContainerLayout(ml.layout.containers[i].id, ml.layout.containers[i].size, ml.layout.containers[i].positionX, ml.layout.containers[i].positionY));
                    }

                    this.model = new BMA.Model.BioModel(ml.model.name, variables, relationships);
                    this.layout = new BMA.Model.Layout(containers, variableLayouts);
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
