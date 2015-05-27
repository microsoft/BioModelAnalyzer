/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\script\uidrivers.interfaces.ts"/>
var BMA;
(function (BMA) {
    (function (Test) {
        var ModelRepositoryTest = (function () {
            function ModelRepositoryTest() {
                this.modelsList = {};
            }
            //constructor() {
            //    this.modelsList = [];
            //}
            ModelRepositoryTest.prototype.GetModelList = function () {
                var list = [];
                for (var attr in this.modelsList) {
                    list.push(this.modelsList[attr]);
                }
                return list;
            };

            ModelRepositoryTest.prototype.LoadModel = function (id) {
                //var i = parseInt(id);
                //if (i < this.modelsList.length) {
                //    return JSON.parse('{"test": ' + this.modelsList[i] + '}');
                //}
                return JSON.parse('{"test": ' + this.modelsList[id] + '}');
            };

            ModelRepositoryTest.prototype.RemoveModel = function (id) {
                var newlist = [];
                for (var i in this.modelsList) {
                    if (i !== id)
                        newlist.push(this.modelsList[i]);
                }
                this.modelsList = newlist;
            };

            ModelRepositoryTest.prototype.SaveModel = function (id, model) {
                this.modelsList[id] = JSON.stringify(model);
            };

            ModelRepositoryTest.prototype.IsInRepo = function (id) {
                return this.modelsList[id] !== undefined;
            };
            return ModelRepositoryTest;
        })();
        Test.ModelRepositoryTest = ModelRepositoryTest;

        var LocalStorageTestDriver = (function () {
            function LocalStorageTestDriver() {
            }
            LocalStorageTestDriver.prototype.Message = function (msg) {
            };

            LocalStorageTestDriver.prototype.AddItem = function (key, item) {
            };

            LocalStorageTestDriver.prototype.Show = function () {
            };

            LocalStorageTestDriver.prototype.Hide = function () {
            };

            LocalStorageTestDriver.prototype.SetItems = function (keys) {
            };
            return LocalStorageTestDriver;
        })();
        Test.LocalStorageTestDriver = LocalStorageTestDriver;

        var VariableEditorTestDriver = (function () {
            function VariableEditorTestDriver() {
            }
            Object.defineProperty(VariableEditorTestDriver.prototype, "Variable", {
                get: function () {
                    return this.variable;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(VariableEditorTestDriver.prototype, "Model", {
                get: function () {
                    return this.model;
                },
                enumerable: true,
                configurable: true
            });

            VariableEditorTestDriver.prototype.GetVariableProperties = function () {
                return {
                    name: this.variable.Name,
                    formula: this.variable.Formula,
                    rangeFrom: this.variable.RangeFrom,
                    rangeTo: this.variable.RangeTo
                };
            };

            VariableEditorTestDriver.prototype.Initialize = function (variable, model) {
                this.variable = variable;
                this.model = model;
            };

            VariableEditorTestDriver.prototype.Show = function (x, y) {
            };

            VariableEditorTestDriver.prototype.Hide = function () {
            };

            VariableEditorTestDriver.prototype.SetValidation = function (val, message) {
            };
            return VariableEditorTestDriver;
        })();
        Test.VariableEditorTestDriver = VariableEditorTestDriver;

        var AjaxTestDriver = (function () {
            function AjaxTestDriver() {
            }
            AjaxTestDriver.prototype.Invoke = function (data) {
                var deferred = $.Deferred();
                var result;

                //switch (url) {
                //    case "api/Validate":
                //        if (data.Formula === "true")
                //            result = { IsValid: true, Message: "Ok" };
                //    default:
                //            result = { IsValid: false, Message: "Not Ok" };
                //}
                result = { IsValid: true, Message: "Ok" };
                console.log("result: " + result.IsValid);
                deferred.resolve(result);
                return deferred.promise();
            };
            return AjaxTestDriver;
        })();
        Test.AjaxTestDriver = AjaxTestDriver;

        var NavigationTestDriver = (function () {
            function NavigationTestDriver() {
                this.ison = false;
                this.zoom = 1;
                this.center = { x: 0, y: 0 };
            }
            Object.defineProperty(NavigationTestDriver.prototype, "IsOn", {
                get: function () {
                    return this.ison;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(NavigationTestDriver.prototype, "Zoom", {
                get: function () {
                    return this.zoom;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(NavigationTestDriver.prototype, "Center", {
                get: function () {
                    return this.center;
                },
                enumerable: true,
                configurable: true
            });

            NavigationTestDriver.prototype.TurnNavigation = function (isOn) {
                this.ison = isOn;
            };
            NavigationTestDriver.prototype.SetZoom = function (zoom) {
                this.zoom = zoom;
            };
            NavigationTestDriver.prototype.SetCenter = function (x, y) {
                this.center = { x: x, y: y };
            };
            return NavigationTestDriver;
        })();
        Test.NavigationTestDriver = NavigationTestDriver;
    })(BMA.Test || (BMA.Test = {}));
    var Test = BMA.Test;
})(BMA || (BMA = {}));