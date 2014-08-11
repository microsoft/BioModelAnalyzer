var BMA;
(function (BMA) {
    (function (Model) {
        var BioModel = (function () {
            function BioModel(containers, variables, relationships) {
                this.variables = variables;
                this.containers = containers;
                this.relationships = relationships;
            }
            Object.defineProperty(BioModel.prototype, "Variables", {
                get: function () {
                    return this.variables;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(BioModel.prototype, "Containers", {
                get: function () {
                    return this.containers;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(BioModel.prototype, "Relationships", {
                get: function () {
                    return this.relationships;
                },
                enumerable: true,
                configurable: true
            });

            BioModel.prototype.Clone = function () {
                return new BioModel(this.containers.slice(0), this.variables.slice(0), this.relationships.slice(0));
            };
            return BioModel;
        })();
        Model.BioModel = BioModel;

        var Variable = (function () {
            function Variable(id, containerId, type, rangeFrom, rangeTo, formula) {
                this.id = id;
                this.containerId = containerId;
                this.type = type;
                this.rangeFrom = rangeFrom;
                this.rangeTo = rangeTo;
                this.formula = formula;
            }
            Object.defineProperty(Variable.prototype, "Id", {
                get: function () {
                    return this.id;
                },
                enumerable: true,
                configurable: true
            });

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
            function Layout(containers, varialbes) {
                this.containers = containers;
                this.variables = varialbes;
            }
            Object.defineProperty(Layout.prototype, "Containers", {
                get: function () {
                    return this.containers;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Layout.prototype, "Variables", {
                get: function () {
                    return this.variables;
                },
                enumerable: true,
                configurable: true
            });

            Layout.prototype.Clone = function () {
                return new Layout(this.containers.slice(0), this.variables.slice(0));
            };
            return Layout;
        })();
        Model.Layout = Layout;

        var ContainerLayout = (function () {
            function ContainerLayout(id, size, positionX, positionY) {
                this.id = id;
                this.size = size;
                this.positionX = positionX;
                this.positionY = positionY;
            }
            Object.defineProperty(ContainerLayout.prototype, "Id", {
                get: function () {
                    return this.id;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(ContainerLayout.prototype, "Size", {
                get: function () {
                    return this.size;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(ContainerLayout.prototype, "PositionX", {
                get: function () {
                    return this.positionX;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(ContainerLayout.prototype, "PositionY", {
                get: function () {
                    return this.positionY;
                },
                enumerable: true,
                configurable: true
            });
            return ContainerLayout;
        })();
        Model.ContainerLayout = ContainerLayout;

        var VarialbeLayout = (function () {
            function VarialbeLayout(id, positionX, positionY, cellX, cellY, angle) {
                this.id = id;
                this.positionX = positionX;
                this.positionY = positionY;
                this.cellX = cellX;
                this.cellY = cellY;
                this.angle = angle;
            }
            Object.defineProperty(VarialbeLayout.prototype, "Id", {
                get: function () {
                    return this.id;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VarialbeLayout.prototype, "PositionX", {
                get: function () {
                    return this.positionX;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VarialbeLayout.prototype, "PositionY", {
                get: function () {
                    return this.positionY;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VarialbeLayout.prototype, "CellX", {
                get: function () {
                    return this.cellX;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VarialbeLayout.prototype, "CellY", {
                get: function () {
                    return this.cellY;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VarialbeLayout.prototype, "Angle", {
                get: function () {
                    return this.angle;
                },
                enumerable: true,
                configurable: true
            });
            return VarialbeLayout;
        })();
        Model.VarialbeLayout = VarialbeLayout;

        var BMAModel = (function () {
            function BMAModel() {
            }
            return BMAModel;
        })();
        Model.BMAModel = BMAModel;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
