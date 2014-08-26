﻿/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module Model {
        export class BioModel {
            private variables: Variable[];
            private relationships: Relationship[];

            public get Variables(): Variable[] {
                return this.variables;
            }

            public get Relationships(): Relationship[] {
                return this.relationships;
            }

            public Clone(): BioModel {
                return new BioModel(this.variables.slice(0), this.relationships.slice(0));
            }

            constructor(variables: Variable[], relationships: Relationship[]) {
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
            private variables: VarialbeLayout[];
            private containers: ContainerLayout[];

            public get Containers(): ContainerLayout[] {
                return this.containers;
            }

            public get Variables(): VarialbeLayout[] {
                return this.variables;
            }

            public Clone(): Layout {
                return new Layout(this.containers.slice(0), this.variables.slice(0));
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

            constructor(id: number, size: number, positionX: number, positionY: number) {
                this.id = id;
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

        export class BMAModel {
            constructor() {

            }
        }
    }
} 