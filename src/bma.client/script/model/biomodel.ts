/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module Model {
        export class BioModel {
            private variables: Variable[];
            private relationships: Relationship[];
            private name: string;

            public get Name(): string {
                return this.name;
            }

            public set Name(value: string) {
                this.name = value;
            }

            public get Variables(): Variable[] {
                return this.variables.slice(0);
            }

            public get Relationships(): Relationship[] {
                return this.relationships.slice(0);
            }

            public Clone(): BioModel {
                return new BioModel(this.Name, this.variables.slice(0), this.relationships.slice(0));
            }

            public SetVariableProperties(id: number, name: string, rangeFrom: number, rangeTo: number, formula: string) {
                for (var i = 0; i < this.variables.length; i++) {
                    if (this.variables[i].Id === id) {
                        this.variables[i] = new BMA.Model.Variable(
                            this.variables[i].Id,
                            this.variables[i].ContainerId,
                            this.variables[i].Type,
                            name === undefined ? this.variables[i].Name : name,
                            isNaN(rangeFrom) ? this.variables[i].RangeFrom : rangeFrom,
                            isNaN(rangeTo) ? this.variables[i].RangeTo : rangeTo,
                            formula === undefined ? this.variables[i].Formula : formula);

                        return;
                    }
                }
            }

            public GetVariableById(id: number) {
                for (var i = 0; i < this.variables.length; i++) {
                    if (this.variables[i].Id === id) {
                        return this.variables[i];
                    }
                }

                return undefined;
            }

            public GetJSON() {
                var vars = [];
                for (var i = 0; i < this.variables.length; i++) {
                    vars.push(this.variables[i].GetJSON());
                }

                var rels = [];
                for (var i = 0; i < this.relationships.length; i++) {
                    rels.push(this.relationships[i].GetJSON());
                }

                return {
                    ModelName: this.name,
                    Engine: "VMCAI",
                    Variables: vars,
                    Relationships: rels
                };
            }

            constructor(name: string, variables: Variable[], relationships: Relationship[]) {
                this.name = name;
                this.variables = variables;
                this.relationships = relationships;
            }
        }

        export class Variable {
            private id: number;
            private containerId: number;
            private type: string;
            private rangeFrom: number;
            private rangeTo: number;
            private formula: string;
            private name: string;

            public get Id(): number {
                return this.id;
            }

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

            public get Name(): string {
                return this.name;
            }

            public GetJSON() {
                return {
                    Id: this.id,
                    Name: this.name,
                    RangeFrom: this.rangeFrom,
                    RangeTo: this.rangeTo,
                    formula: this.formula
                };
            }

            constructor(id: number, containerId: number, type: string, name: string, rangeFrom: number, rangeTo: number, formula: string) {
                this.id = id;
                this.containerId = containerId;
                this.type = type;
                this.rangeFrom = rangeFrom;
                this.rangeTo = rangeTo;
                this.formula = formula;
                this.name = name;
            }
        }

        export class Relationship {
            private id: number;
            private fromVariableId: number;
            private toVariableId: number;
            private type: string;

            public get Id(): number {
                return this.id;
            }

            public get FromVariableId(): number {
                return this.fromVariableId;
            }

            public get ToVariableId(): number {
                return this.toVariableId;
            }

            public get Type(): string {
                return this.type;
            }

            public GetJSON() {
                return {
                    Id: this.id, 
                    FromVariableId: this.fromVariableId,
                    ToVariableId: this.toVariableId,
                    Type: this.type
                };
            }

            constructor(id: number, fromVariableId: number, toVariableId: number, type: string) {
                this.id = id;
                this.fromVariableId = fromVariableId;
                this.toVariableId = toVariableId;
                this.type = type;
            }
        }

        export class Layout {
            private variables: VarialbeLayout[];
            private containers: ContainerLayout[];

            public get Containers(): ContainerLayout[] {
                return this.containers.slice(0);
            }

            public get Variables(): VarialbeLayout[] {
                return this.variables.slice(0);
            }

            public Clone(): Layout {
                return new Layout(this.containers.slice(0), this.variables.slice(0));
            }

            public GetVariableById(id: number) {
                for (var i = 0; i < this.variables.length; i++) {
                    if (this.variables[i].Id === id) {
                        return this.variables[i];
                    }
                }

                return undefined;
            }

            public GetContainerById(id: number) {
                for (var i = 0; i < this.containers.length; i++) {
                    if (this.containers[i].Id === id) {
                        return this.containers[i];
                    }
                }

                return undefined;
            }


            constructor(containers: ContainerLayout[], varialbes: VarialbeLayout[]) {
                this.containers = containers;
                this.variables = varialbes;
            }
        }

        export class ContainerLayout {
            private id: number;
            private size: number;
            private positionX: number;
            private positionY: number;
            private name: string;

            public get Name(): string {
                return this.name;
            }

            public set Name(value: string) {
                this.name = value;
            }

            public get Id(): number {
                return this.id;
            }

            public get Size(): number {
                return this.size;
            }

            public get PositionX(): number {
                return this.positionX;
            }

            public get PositionY(): number {
                return this.positionY;
            }

            constructor(id: number, name: string, size: number, positionX: number, positionY: number) {
                this.id = id;
                this.name = name;
                this.size = size;
                this.positionX = positionX;
                this.positionY = positionY;
            }
        }

        export class VarialbeLayout {
            private id: number;
            private positionX: number;
            private positionY: number;
            private cellX: number;
            private cellY: number;
            private angle: number;

            public get Id(): number {
                return this.id;
            }

            public get PositionX(): number {
                return this.positionX;
            }

            public get PositionY(): number {
                return this.positionY;
            }

            public get CellX(): number {
                return this.cellX;
            }

            public get CellY(): number {
                return this.cellY;
            }

            public get Angle(): number {
                return this.angle;
            }

            constructor(id: number, positionX: number, positionY: number, cellX: number, cellY: number, angle: number) {
                this.id = id;
                this.positionX = positionX;
                this.positionY = positionY;
                this.cellX = cellX;
                this.cellY = cellY;
                this.angle = angle;
            }
        }
    }
} 