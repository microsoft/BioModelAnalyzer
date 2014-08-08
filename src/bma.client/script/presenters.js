/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>

var BMA;
(function (BMA) {
    (function (Presenters) {
        var DesignSurfacePresenter = (function () {
            function DesignSurfacePresenter(bioModel, layout, svgPlotDriver, undoButton, redoButton) {
                var _this = this;
                var that = this;
                this.model = bioModel;
                this.layout = layout;
                this.driver = svgPlotDriver;
                this.models = [];

                window.Commands.On("AddElementSelect", function (type) {
                    _this.selectedType = type;
                    _this.driver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", function (args) {
                    if (that.selectedType !== undefined) {
                        var variables = that.model.Variables;
                        var variableLayouts = that.layout.Variables;

                        variables.push(new BMA.Model.Variable(0, 0, that.selectedType, 0, 0, ""));
                        variableLayouts.push(new BMA.Model.VarialbeLayout(0, args.x, args.y, 0, 0, 0));

                        that.model = new BMA.Model.BioModel([], variables, []);
                        that.layout = new BMA.Model.Layout([], variableLayouts);

                        var drawingSvg = that.CreateSvg();
                        that.driver.Draw(drawingSvg);
                    }
                });

                window.Commands.On("Undo", function () {
                });

                window.Commands.On("Redo", function () {
                });

                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: function (svg) {
                        _this.svg = svg;
                        var drawingSvg = _this.CreateSvg();
                        _this.driver.Draw(drawingSvg);
                    }
                });
            }
            DesignSurfacePresenter.prototype.OnModelUpdated = function () {
                //todo: update application model
            };

            DesignSurfacePresenter.prototype.Undo = function () {
                if (this.CanUndo) {
                    --this.currentModelIndex;
                    this.OnModelUpdated();
                }
            };

            DesignSurfacePresenter.prototype.Redo = function () {
                if (this.CanRedo) {
                    ++this.currentModelIndex;
                    this.OnModelUpdated();
                }
            };

            DesignSurfacePresenter.prototype.Truncate = function () {
                this.models.length = this.currentModelIndex + 1;
            };

            DesignSurfacePresenter.prototype.Dup = function () {
            };

            Object.defineProperty(DesignSurfacePresenter.prototype, "CanUndo", {
                get: function () {
                    return this.currentModelIndex > 0;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(DesignSurfacePresenter.prototype, "CanRedo", {
                get: function () {
                    return this.currentModelIndex < this.models.length - 1;
                },
                enumerable: true,
                configurable: true
            });

            DesignSurfacePresenter.prototype.CreateSvg = function () {
                if (this.svg === undefined)
                    return undefined;

                this.svg.clear();

                var variables = this.model.Variables;
                var variableLayouts = this.layout.Variables;
                for (var i = 0; i < variables.length; i++) {
                    var variable = variables[i];
                    var variableLayout = variableLayouts[i];

                    var element = window.ElementRegistry.GetElementByType(variable.Type);
                    this.svg.add(element.RenderToSvg(variableLayout.PositionX, variableLayout.PositionY));
                }

                return $(this.svg.toSVG()).children();
            };
            return DesignSurfacePresenter;
        })();
        Presenters.DesignSurfacePresenter = DesignSurfacePresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;

    var ModelStack = (function () {
        function ModelStack() {
        }
        Object.defineProperty(ModelStack, "Current", {
            get: function () {
                return ModelStack.models[ModelStack.index];
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ModelStack, "HasModel", {
            get: function () {
                return ModelStack.index >= 0;
            },
            enumerable: true,
            configurable: true
        });

        Object.defineProperty(ModelStack, "CanUndo", {
            get: function () {
                return ModelStack.index > 0;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ModelStack, "CanRedo", {
            get: function () {
                return ModelStack.index < ModelStack.models.length - 1;
            },
            enumerable: true,
            configurable: true
        });

        ModelStack.Undo = function () {
            if (ModelStack.CanUndo) {
                --ModelStack.index;
            }
        };

        ModelStack.Redo = function () {
            if (ModelStack.CanRedo) {
                ++ModelStack.index;
            }
        };

        ModelStack.Set = function (m, l) {
            ModelStack.models = [{ model: m, layout: l }];
            ModelStack.index = 0;
        };

        ModelStack.Dup = function () {
            ModelStack.truncate();
            var orig = ModelStack.Current;
            ModelStack.models[ModelStack.index] = { model: orig.model.Clone(), layout: orig.layout.Clone() };
            ModelStack.models.push(orig);
            ++ModelStack.index;
        };

        ModelStack.truncate = function () {
            ModelStack.models.length = ModelStack.index + 1;
        };
        ModelStack.models = [];
        ModelStack.index = -1;
        return ModelStack;
    })();
    BMA.ModelStack = ModelStack;
})(BMA || (BMA = {}));
//# sourceMappingURL=presenters.js.map
