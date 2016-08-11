/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\script\uidrivers\commoninterfaces.ts"/>

module BMA {
    export module Test {

        export class TestWaitScreen implements BMA.UIDrivers.IWaitScreen {
            constructor() { }
            Show() { }
            Hide() { }
        }

        export class ModelRepositoryTest implements BMA.UIDrivers.IModelRepository {

            private modelsList = {};

            //constructor() {
            //    this.modelsList = [];
            //}
            GetModelList(): JQueryPromise<string[]>{
                var result = $.Deferred();
                var list: string[] = [];
                for (var attr in this.modelsList) {
                    list.push(this.modelsList[attr]);
                }
                result.resolve(list);

                return result.promise();
            }

            LoadModel(id: string): JQueryPromise<JSON> {
                //var i = parseInt(id);
                //if (i < this.modelsList.length) {
                //    return JSON.parse('{"test": ' + this.modelsList[i] + '}');
                //}
                var result = $.Deferred();
                result.resolve(JSON.parse('{"test": ' + this.modelsList[id] + '}'));
                return result.promise();
            }

            RemoveModel(id: string) {
                var newlist = [];
                for (var i in this.modelsList) {
                    if (i !== id)
                        newlist.push(this.modelsList[i]);
                }
                this.modelsList = newlist;
            }

            SaveModel(id: string, model: JSON) {
                this.modelsList[id] = JSON.stringify(model);
            }

            IsInRepo(id: string) {
                return this.modelsList[id] !== undefined;
            }
            //OnRepositoryUpdated();
        }

        export class LocalStorageTestDriver implements BMA.UIDrivers.IStorageDriver {

            public Message(msg: string) { }

            public AddItem(key, item) {
            }

            public Show() {
            }

            public Hide() {
            }

            public SetItems(keys) {
            }
        }

        export class VariableEditorTestDriver implements BMA.UIDrivers.IVariableEditor {

            private variable: BMA.Model.Variable;
            private model: BMA.Model.BioModel;
            private layout: BMA.Model.Layout;

            public get Variable() {
                return this.variable;
            }

            public get Model() {
                return this.model;
            }

            public GetVariableProperties(): { name: string; formula: string; rangeFrom: number; rangeTo: number; TFdescription: string;} {
        return {
                    name: this.variable.Name,
                    formula: this.variable.Formula,
                    rangeFrom: this.variable.RangeFrom,
                    rangeTo: this.variable.RangeTo,
                    TFdescription: this.layout.GetVariableById(this.variable.Id).TFDescription
                }
    }

            public Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) {
                this.variable = variable;
                this.model = model;
            }

            public Show(x: number, y: number) { }

            public Hide() { }

            public SetValidation(val: boolean, message: string) { }

            public SetOnClosingCallback(callback: Function) { };

            public SetOnVariableEditedCallback(callback: Function) { };

            public SetOnFormulaEditedCallback(callback: Function) { };
        }


        export class AjaxTestDriver implements BMA.UIDrivers.IServiceDriver {
            public Invoke(data): JQueryPromise<any> {
                var deferred = $.Deferred();
                var result: { IsValid: boolean; Message: string };
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
            }
        }

        export class NavigationTestDriver implements BMA.UIDrivers.INavigationPanel {
            private ison = false;
            private zoom = 1;
            private center = { x: 0, y: 0 };

            public get IsOn() { return this.ison; }
            public get Zoom() { return this.zoom; }
            public get Center() { return this.center; }

            public TurnNavigation(isOn: boolean) { this.ison = isOn; }
            public SetZoom(zoom: number) { this.zoom = zoom; }
            public SetCenter(x: number, y: number) { this.center = { x: x, y: y }; }
            public MoveDraggableOnTop() { }
            public MoveDraggableOnBottom() { }

            public GetNavigationSurface() {
                return {
                    master: undefined
                };
            }
        }
    }
}

