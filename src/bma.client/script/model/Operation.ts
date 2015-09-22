﻿module BMA {
    export module LTLOperations {


        export interface IGetFormula {
            (op: IOperand[]): string;
        }

        export interface IOperand {
            GetFormula(): string;
            Clone(): IOperand;
        }

        export class NameOperand implements IOperand {
            private name: string;

            constructor(name: string) {
                this.name = name;
            }

            public get Name(): string {
                return this.name;
            }

            public GetFormula() {
                return this.name;
            }

            public Clone() {
                return new NameOperand(this.name);
            }
        }

        export class ConstOperand implements IOperand {
            private const: number;

            constructor(value: number) {
                this.const = value;
            }

            public get Value(): number {
                return this.const;
            }

            public GetFormula() {
                return this.const.toString();
            }

            public Clone() {
                return new ConstOperand(this.const);
            }
        }

        export class KeyframeEquation implements IOperand {
            private leftOperand: IOperand; //NameOperand | ConstOperand;
            private rightOperand: IOperand; //NameOperand | ConstOperand;
            private operator: string;

            constructor(leftOperand: IOperand /*NameOperand | ConstOperand*/, operator: string, rightOperand: IOperand /*NameOperand | ConstOperand*/) {
                this.leftOperand = leftOperand;
                this.rightOperand = rightOperand;
                this.operator = operator;
            }

            public get LeftOperand() {
                return this.leftOperand;
            }

            public get RightOperand() {
                return this.rightOperand;
            }

            public get Operator() {
                return this.operator;
            }

            public GetFormula() {
                return "(" + this.leftOperand + " " + this.operator + " " + this.rightOperand + ")";
            }

            public Clone() {
                return new KeyframeEquation(this.leftOperand.Clone(), this.operator, this.rightOperand.Clone());
            }
        }

        export class DoubleKeyframeEquation implements IOperand {
            private leftOperand: IOperand; /*NameOperand | ConstOperand*/
            private middleOperand: IOperand; /*NameOperand | ConstOperand*/
            private rightOperand: IOperand; /*NameOperand | ConstOperand*/
            private leftOperator: string;
            private rightOperator: string;


            constructor(leftOperand: IOperand /*NameOperand | ConstOperand*/, leftOperator: string, middleOperand: IOperand /*NameOperand | ConstOperandIOperand*/, rightOperator: string, rightOperand: IOperand /*NameOperand | ConstOperand*/) {
                this.leftOperand = leftOperand;
                this.rightOperand = rightOperand;
                this.middleOperand = middleOperand;
                this.leftOperator = leftOperator;
                this.rightOperator = rightOperator;
            }

            public get LeftOperand() {
                return this.leftOperand;
            }

            public get RightOperand() {
                return this.rightOperand;
            }

            public get MiddleOperand() {
                return this.middleOperand;
            }

            public get LeftOperator() {
                return this.leftOperator;
            }

            public get RightOperator() {
                return this.rightOperator;
            }

            public GetFormula() {
                return "(" + this.leftOperand.GetFormula() + " " + this.leftOperator + " " + this.middleOperand.GetFormula() + ") AND (" + this.middleOperand.GetFormula() + " " + this.rightOperator + " " + this.rightOperand.GetFormula() + ")";
            }

            public Clone() {
                return new DoubleKeyframeEquation(this.leftOperand.Clone(), this.leftOperator, this.middleOperand.Clone(), this.rightOperator, this.rightOperand.Clone());
            }
        }

        export class Keyframe implements IOperand {
            private name: string;
            private operands: IOperand[]; /*(KeyframeEquation | DoubleKeyframeEquation)[];*/

            constructor(name: string, operands: IOperand[] /*(KeyframeEquation | DoubleKeyframeEquation)[];*/) {
                this.name = name;
                this.operands = operands;
            }

            public get Name(): string {
                return this.name;
            }

            public get Operands(): IOperand[] /*(KeyframeEquation | DoubleKeyframeEquation)[];*/ {
                return this.operands;
            }

            public GetFormula() {
                if (this.operands === undefined || this.operands.length < 1) {
                    return "";
                } else {
                    var formula = "";
                    for (var i = 0; i < this.operands.length; i++) {
                        formula += this.operands[i].GetFormula();
                        if (i < this.operands.length - 1) {
                            formula += " AND ";
                        }
                    }
                }
            }

            public Clone() {
                return new BMA.LTLOperations.Keyframe(this.name, this.operands.slice(0));
            }
        }

        export class Operator {
            private name: string;
            private fun: IGetFormula;
            private operandsNumber: number;

            constructor(name: string, operandsCount: number, fun: IGetFormula) {
                this.name = name;
                this.fun = fun;
                this.operandsNumber = operandsCount;
            }

            get Name() {
                return this.name;
            }

            get OperandsCount() {
                return this.operandsNumber;
            }

            public GetFormula(op: IOperand[]) {
                if (op !== undefined && op.length !== this.operandsNumber) {
                    throw "Operator " + name + ": invalid operands count";
                }
                return this.fun(op);
            }
        }


        export class Operation implements IOperand {
            private operator: Operator;
            private operands: IOperand[];

            public get Operator() {
                return this.operator;
            }

            public set Operator(op) {
                this.operator = op;
            }

            public get Operands() {
                return this.operands;
            }

            public set Operands(op) {
                this.operands = op;
            }

            public GetFormula() {
                return this.operator.GetFormula(this.operands);
            }

            public Clone() {
                var operands = [];
                for (var i = 0; i < this.operands.length; i++) {
                    
                    operands.push(this.operands[i] === undefined ? undefined : this.operands[i].Clone());
                }
                var result = new Operation();
                result.Operator = new Operator(this.operator.Name, this.operator.OperandsCount, this.operator.GetFormula);
                result.Operands = operands;

                return result;
            }
        }
    }
}  