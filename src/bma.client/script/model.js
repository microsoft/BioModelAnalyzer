/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
var BMA;
(function (BMA) {
    (function (Model) {
        var BioModel = (function () {
            function BioModel() {
                this.variables = [];
                this.containers = [];
                this.relationships = [];
            }
            return BioModel;
        })();
        Model.BioModel = BioModel;

        var Variable = (function () {
            function Variable(containerId, type, rangeFrom, rangeTo, formula) {
                this.containerId = containerId;
                this.type = type;
                this.rangeFrom = rangeFrom;
                this.rangeTo = rangeTo;
                this.formula = formula;
            }
            Object.defineProperty(Variable.prototype, "ContainerId", {
                get: function () {
                    return this.containerId;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Variable.prototype, "Type", {
                get: function () {
                    return this.type;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Variable.prototype, "RangeFrom", {
                get: function () {
                    return this.rangeFrom;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Variable.prototype, "RangeTo", {
                get: function () {
                    return this.rangeTo;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Variable.prototype, "Formula", {
                get: function () {
                    return this.formula;
                },
                enumerable: true,
                configurable: true
            });
            return Variable;
        })();
        Model.Variable = Variable;

        var Container = (function () {
            function Container(id) {
                this.id = id;
            }
            Object.defineProperty(Container.prototype, "Id", {
                get: function () {
                    return this.id;
                },
                enumerable: true,
                configurable: true
            });
            return Container;
        })();
        Model.Container = Container;

        var Relationship = (function () {
            function Relationship(fromVariableId, toVariableId, type) {
                this.fromVariableId = fromVariableId;
                this.toVariableId = toVariableId;
                this.type = type;
            }
            Object.defineProperty(Relationship.prototype, "FromVariableId", {
                get: function () {
                    return this.fromVariableId;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Relationship.prototype, "ToVariableId", {
                get: function () {
                    return this.toVariableId;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Relationship.prototype, "Type", {
                get: function () {
                    return this.type;
                },
                enumerable: true,
                configurable: true
            });
            return Relationship;
        })();
        Model.Relationship = Relationship;

        var Layout = (function () {
            function Layout() {
            }
            return Layout;
        })();
        Model.Layout = Layout;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
//# sourceMappingURL=model.js.map
