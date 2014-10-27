/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="..\..\script\uidrivers.interfaces.ts"/>

module BMA {
    export module Test {

        export class VariableEditorTestDriver implements BMA.UIDrivers.IVariableEditor {

            private variable: BMA.Model.Variable;
            private model: BMA.Model.BioModel;

            public get Variable() {
                return this.variable;
            }

            public get Model() {
                return this.model;
            }

            public GetVariableProperties(): { name: string; formula: string; rangeFrom: number; rangeTo: number; } {
        return {
                    name: this.variable.Name,
                    formula: this.variable.Formula,
                    rangeFrom: this.variable.RangeFrom,
                    rangeTo: this.variable.RangeTo
                }
    }

            public Initialize(variable: BMA.Model.Variable, model: BMA.Model.BioModel) {
                this.variable = variable;
                this.model = model;
            }

            public Show(x: number, y: number) { }

            public Hide() { }

            public SetValidation(val: boolean, message: string) { }
        }


        export class AjaxTestDriver implements BMA.UIDrivers.IServiceDriver {
            public Invoke(url, data): JQueryPromise<any> {
                var deferred = $.Deferred();
                var result: { IsValid: boolean; Message: string };
                switch (url) {
                    case "api/Validate":
                        if (data.Formula === "true")
                            result = { IsValid: true, Message: "Ok" };
                    default:
                            result = { IsValid: false, Message: "Not Ok" };
                        
                }
                console.log("result: " + result.IsValid);
                deferred.resolve(result);
                return deferred.promise();
            }
        }
    }
}

