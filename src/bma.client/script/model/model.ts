/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module Model {
        export class AppModel {
            private model: BMA.Model.BioModel;
            private layout: BMA.Model.Layout;
            private proofResult: BMA.Model.ProofResult;

            public get BioModel(): BMA.Model.BioModel {
                return this.model;
            }

            public set BioModel(value: BMA.Model.BioModel) {
                this.model = value;
                //TODO: update inner components (analytics)
            }

            public get Layout(): BMA.Model.Layout {
                return this.layout;
            }

            public set Layout(value: BMA.Model.Layout) {
                this.layout = value;
                //TODO: update inner components (analytics)
            }

            public get ProofResult() {
                return this.proofResult;
            }

            public set ProofResult(value: BMA.Model.ProofResult) {
                this.proofResult = value;
            }

            constructor() {
                this.model = new BMA.Model.BioModel("model 1", [], []);
                this.layout = new BMA.Model.Layout([], []);
            }
        }
    }
}