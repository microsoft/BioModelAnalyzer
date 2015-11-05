/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module Model {
        export class AppModel {
            private model: BMA.Model.BioModel;
            private layout: BMA.Model.Layout;
            private proofResult: BMA.Model.ProofResult = undefined;
            private states: BMA.LTLOperations.Keyframe[] = [];
            private operations: BMA.LTLOperations.Operation[] = [];

            public get BioModel(): BMA.Model.BioModel {
                return this.model;
            }

            public set BioModel(value: BMA.Model.BioModel) {
                this.model = value;

                window.Commands.Execute("AppModelChanged", {});
                //TODO: update inner components (analytics)
            }

            public get Layout(): BMA.Model.Layout {
                return this.layout;
            }

            public set Layout(value: BMA.Model.Layout) {
                this.layout = value;
                //TODO: update inner components (analytics)
            }

            public get States(): BMA.LTLOperations.Keyframe[] {
                return this.states;
            }

            public set States(value: BMA.LTLOperations.Keyframe[]) {
                this.states = value;
                //TODO: update inner components (ltl)
            }

            public get Operations(): BMA.LTLOperations.Operation[] {
                return this.operations;
            }

            public set Operations(value: BMA.LTLOperations.Operation[]) {
                this.operations = value;
                //TODO: update inner components (ltl)
            }


            public get ProofResult() {
                return this.proofResult;
            }

            public set ProofResult(value: BMA.Model.ProofResult) {
                this.proofResult = value;
            }

            public DeserializeLegacyJSON(serializedModel: string) {

                if (serializedModel !== undefined && serializedModel !== null) {
                    var ml = JSON.parse(serializedModel);
                    //TODO: verify model
                    if (ml === undefined || ml.model === undefined || ml.layout === undefined ||
                        ml.model.variables === undefined ||
                        ml.layout.variables === undefined ||
                        ml.model.variables.length !== ml.layout.variables.length ||
                        ml.layout.containers === undefined ||
                        ml.model.relationships === undefined)
                    {
                        console.log("Invalid model");
                        return;
                    }

                    var variables = [];
                    for (var i = 0; i < ml.model.variables.length; i++) {
                        variables.push(new BMA.Model.Variable(
                            ml.model.variables[i].id,
                            ml.model.variables[i].containerId,
                            ml.model.variables[i].type,
                            ml.model.variables[i].name,
                            ml.model.variables[i].rangeFrom,
                            ml.model.variables[i].rangeTo,
                            ml.model.variables[i].formula)); 
                    }
                    
                    var variableLayouts = [];
                    for (var i = 0; i < ml.layout.variables.length; i++) {
                        variableLayouts.push(new BMA.Model.VariableLayout(
                            ml.layout.variables[i].id,
                            ml.layout.variables[i].positionX,
                            ml.layout.variables[i].positionY,
                            ml.layout.variables[i].cellX,
                            ml.layout.variables[i].cellY,
                            ml.layout.variables[i].angle));
                    }

                    var relationships = [];
                    for (var i = 0; i < ml.model.relationships.length; i++) {
                        relationships.push(new BMA.Model.Relationship(
                            ml.model.relationships[i].id,
                            ml.model.relationships[i].fromVariableId,
                            ml.model.relationships[i].toVariableId,
                            ml.model.relationships[i].type));
                    }

                    var containers = [];
                    for (var i = 0; i < ml.layout.containers.length; i++) {
                        containers.push(new BMA.Model.ContainerLayout(
                            ml.layout.containers[i].id,
                            ml.layout.containers[i].name,
                            ml.layout.containers[i].size,
                            ml.layout.containers[i].positionX,
                            ml.layout.containers[i].positionY));
                    }

                    this.model = new BMA.Model.BioModel(ml.model.name, variables, relationships);
                    this.layout = new BMA.Model.Layout(containers, variableLayouts);
                } else {
                    this.model = new BMA.Model.BioModel("model 1", [], []);
                    this.layout = new BMA.Model.Layout([], []);
                }

                this.states = [];
                this.operations = [];

                this.proofResult = undefined;
                window.Commands.Execute("ModelReset", undefined);
            }

            public Deserialize(serializedModel: string) {

                if (serializedModel !== undefined && serializedModel !== null) {
                    var parsed = JSON.parse(serializedModel);

                    var imported = BMA.Model.ImportModelAndLayout(parsed);
                    this.model = imported.Model;
                    this.layout = imported.Layout;

                    if (parsed.ltl !== undefined) {
                        var ltl = BMA.Model.ImportLTLContents(parsed.ltl);
                        if (ltl.states !== undefined) {
                            this.states = ltl.states;
                        } else {
                            this.states = [];
                        }

                        if (ltl.operations !== undefined) {
                            this.operations = ltl.operations;
                        } else {
                            this.operations = [];
                        }
                    }
                } else {
                    this.model = new BMA.Model.BioModel("model 1", [], []);
                    this.layout = new BMA.Model.Layout([], []);
                    this.states = [];
                    this.operations = [];
                }

                this.proofResult = undefined;
                window.Commands.Execute("ModelReset", undefined);
            }

            public Reset(model: BMA.Model.BioModel, layout: BMA.Model.Layout) {
                this.model = model;
                this.layout = layout;
                window.Commands.Execute("ModelReset", undefined);
            }

            public Serialize(): string {
                var exported = BMA.Model.ExportModelAndLayout(this.model, this.layout);
                var ltl = BMA.Model.ExportLTLContents(this.states, this.operations);
                (<any>exported).ltl = ltl;
                return JSON.stringify(exported);
            }

            constructor() {
                this.model = new BMA.Model.BioModel("model 1", [], []);
                this.layout = new BMA.Model.Layout([], []);
                this.states = [
                    //new BMA.LTLOperations.Keyframe("Init", [])
                ];
            }
        }
    }
}