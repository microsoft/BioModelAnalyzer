/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module Model {
        export class BioModel {
            private variables: Variable[];
            private containers: Container[];
            private relationships: Relationship[];

            constructor() {
                this.variables = [];
                this.containers = [];
                this.relationships = [];
            }
        }

        export class Variable {
            private containerId: number;
            private type: string;
            private rangeFrom: number;
            private rangeTo: number;
            private formula: string;

            public get ContainerId(): number {
                return this.containerId;
            }

            public get Type(): string {
                return this.type;
            }

            public get RangeFrom(): number {
                return this.rangeFrom;
            }

            public get RangeTo(): number {
                return this.rangeTo;
            }

            public get Formula(): string {
                return this.formula;
            }

            constructor(containerId: number, type: string, rangeFrom: number, rangeTo: number, formula: string) {
                this.containerId = containerId;
                this.type = type;
                this.rangeFrom = rangeFrom;
                this.rangeTo = rangeTo;
                this.formula = formula;
            }
        }

        export class Container {
            private id: number;

            public get Id(): number {
                return this.id;
            }

            constructor(id: number) {
                this.id = id;
            }
        }

        export class Relationship {
            private fromVariableId: number;
            private toVariableId: number;
            private type: string;

            public get FromVariableId(): number {
                return this.fromVariableId;
            }

            public get ToVariableId(): number {
                return this.toVariableId;
            }

            public get Type(): string {
                return this.type;
            }

            constructor(fromVariableId: number, toVariableId: number, type: string) {
                this.fromVariableId = fromVariableId;
                this.toVariableId = toVariableId;
                this.type = type;
            }
        }

        export class Layout {
            constructor() {
            }
        }
    }
} 