module BMA {
    export module LTLOperations {

        export interface IGetFormula {
            (op: IOperand[]): string;
        }

        export interface IOperand {
            GetFormula(): string;
            Clone(): IOperand;
            Equals(operand: IOperand): boolean;
        }

        /*
        export class FlexOperand implements IOperand {
            constructor() { }

            public GetFormula() {
                return "";
            }

            public Clone() {
                return new FlexOperand();
            }
        }
        */

        export class NameOperand implements IOperand {
            private name: string;
            private id: any;

            constructor(name: string, id: any = undefined) {
                this.name = name;
                this.id = id;
            }

            public get Name(): string {
                return this.name;
            }

            public get Id(): any {
                return this.id;
            }

            public GetFormula() {
                return this.id; //this.name;
            }

            public Clone() {
                return new NameOperand(this.name, this.id);
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.NameOperand) {
                    return this.name === op.Name && this.id === op.Id;
                } else return false;
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

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.ConstOperand) {
                    return this.const === op.Value;
                } else return false;
            }
        }

        export class KeyframeEquation implements IOperand {
            private leftOperand: NameOperand | ConstOperand;
            private rightOperand: NameOperand | ConstOperand;
            private operator: string;

            constructor(leftOperand: NameOperand | ConstOperand, operator: string, rightOperand: NameOperand | ConstOperand) {
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
                return "(" + this.operator + " " + this.leftOperand.GetFormula() + " " + this.rightOperand.GetFormula() + ")";
            }

            public Clone() {
                return new KeyframeEquation(this.leftOperand.Clone(), this.operator, this.rightOperand.Clone());
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.KeyframeEquation) {
                    return this.leftOperand.Equals(op.LeftOperand) && this.operator == op.Operator && this.rightOperand.Equals(op.RightOperand);
                } else return false;
            }
        }

        export class DoubleKeyframeEquation implements IOperand {
            private leftOperand: NameOperand | ConstOperand
            private middleOperand: NameOperand | ConstOperand
            private rightOperand: NameOperand | ConstOperand
            private leftOperator: string;
            private rightOperator: string;


            constructor(leftOperand: NameOperand | ConstOperand, leftOperator: string, middleOperand: NameOperand | ConstOperand, rightOperator: string, rightOperand: NameOperand | ConstOperand) {
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
                return "(And " + "(" + this.Invert(this.leftOperator) + " " + this.middleOperand.GetFormula() + " " + this.leftOperand.GetFormula() + ") (" + this.rightOperator + " " + this.middleOperand.GetFormula() + " " + this.rightOperand.GetFormula() + "))";
            }

            public Clone() {
                return new DoubleKeyframeEquation(this.leftOperand.Clone(), this.leftOperator, this.middleOperand.Clone(), this.rightOperator, this.rightOperand.Clone());
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.DoubleKeyframeEquation) {
                    return this.leftOperand.Equals(op.LeftOperand) && this.leftOperator == op.LeftOperator && this.middleOperand.Equals(op.MiddleOperand)
                        && this.rightOperator == op.RightOperator && this.rightOperand.Equals(op.RightOperand);
                } else return false;
            }

            private Invert(operator: string): string {
                switch (operator) {
                    case ">":
                        return "<";
                    case "<":
                        return ">";
                    case ">=":
                        return "<=";
                    case "<=":
                        return ">=";
                    default:
                        return operator;
                }
            }
        }

        export class TrueKeyframe implements IOperand {
            public GetFormula() {
                return "True";
            }

            public Clone() {
                return new TrueKeyframe();
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.TrueKeyframe) {
                    return true;
                } else return false;
            }
        }

        export class SelfLoopKeyframe implements IOperand {
            public GetFormula() {
                return "SelfLoop";
            }

            public Clone() {
                return new SelfLoopKeyframe();
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.SelfLoopKeyframe) {
                    return true;
                } else return false;
            }
        }

        export class OscillationKeyframe implements IOperand {
            public GetFormula() {
                return "Oscillation";
            }

            public Clone() {
                return new OscillationKeyframe();
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.OscillationKeyframe) {
                    return true;
                } else return false;
            }
        }

        export class Keyframe implements IOperand {
            private name: string;
            private description: string;
            private operands: (KeyframeEquation | DoubleKeyframeEquation)[];

            constructor(name: string, description: string, operands: (KeyframeEquation | DoubleKeyframeEquation)[]) {
                this.name = name;
                this.description = description;
                this.operands = operands;
            }

            public set Name(name: string) {
                this.name = name;
            }

            public get Name(): string {
                return this.name;
            }

            public get Description(): string {
                return this.description;
            }

            public get Operands(): (KeyframeEquation | DoubleKeyframeEquation)[] {
                return this.operands;
            }

            public GetFormula() {
                if (this.operands === undefined || this.operands.length < 1) {
                    return "";
                } else {
                    if (this.operands.length === 1)
                        return this.operands[0].GetFormula();

                    var formula = this.operands[this.operands.length - 1].GetFormula();

                    for (var i = this.operands.length - 2; i >= 0; i--) {
                        formula = "(And " + this.operands[i].GetFormula() + " " + formula + ")";
                    }

                    return formula;
                }
            }

            public Clone() {
                return new BMA.LTLOperations.Keyframe(this.name, this.description, this.operands.slice(0));
            }

            private CompareOperands(operands: (BMA.LTLOperations.KeyframeEquation | BMA.LTLOperations.DoubleKeyframeEquation)[]): boolean {
                if (this.operands.length !== operands.length) return false;
                var isEqual = true;

                var sortOps = (x, y) => {
                    if (x instanceof KeyframeEquation && y instanceof KeyframeEquation) {
                        if (x.Equals(y)) return 0;
                        var xLeft = x.LeftOperand, yLeft = y.LeftOperand;
                        if (xLeft instanceof NameOperand && yLeft instanceof NameOperand) {
                            return xLeft.Name < yLeft.Name ? -1 : 1;
                        } else if (xLeft instanceof ConstOperand) {
                            if (yLeft instanceof ConstOperand)
                                return xLeft.Value < yLeft.Value ? -1 : 1;
                            else return 1;
                        } else return -1;
                    } else if (x instanceof DoubleKeyframeEquation) {
                        if (y instanceof DoubleKeyframeEquation) {
                            if (x.Equals(y)) return 0;
                            var xLeft = x.LeftOperand, yLeft = y.LeftOperand;
                            if (xLeft instanceof NameOperand && yLeft instanceof NameOperand) {
                                return xLeft.Name < yLeft.Name ? -1 : 1;
                            } else if (xLeft instanceof ConstOperand) {
                                if (yLeft instanceof ConstOperand)
                                    return xLeft.Value < yLeft.Value ? -1 : 1;
                                else return 1;
                            } else return -1;
                        } else return 1;
                    } else return -1;
                };

                var operands1 = this.operands.sort(sortOps);
                var operands2 = operands.sort(sortOps);

                for (var i = 0; i < operands1.length; i++) {
                    if (!isEqual) return false;
                    isEqual = isEqual && operands1[i].Equals(operands2[i]);
                }

                return isEqual;
            }

            public Equals(op: BMA.LTLOperations.IOperand): boolean {
                if (op instanceof BMA.LTLOperations.Keyframe) {
                    var isEqual = this.name == op.Name && this.description == op.Description;
                    return isEqual && this.CompareOperands(op.Operands);
                } else return false;
            }
        }

        export class Operator {
            private name: string;
            private fun: IGetFormula;
            private operandsNumber: number;
            private isFunction: boolean

            constructor(name: string, operandsCount: number, fun: IGetFormula, isFunction: boolean = false) {
                this.name = name;
                this.fun = fun;
                this.operandsNumber = operandsCount;
                this.isFunction = isFunction;
            }

            get IsFunction() {
                return this.isFunction;
            }

            get Name() {
                return this.name;
            }

            get OperandsCount() {
                return this.operandsNumber;
            }

            get Function() {
                return this.fun;
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
                result.Operator = new Operator(this.operator.Name, this.operator.OperandsCount, this.operator.Function, this.Operator.IsFunction);
                result.Operands = operands;

                return result;
            }

            public Equals(operation: BMA.LTLOperations.IOperand) {
                if (operation instanceof Operation && this.operator == operation.operator) {
                    var isEqual = true;
                    for (var i = 0; i < this.operands.length; i++) {
                        isEqual = isEqual && (this.operands[i] && operation.Operands[i] ?
                            this.operands[i].Equals(operation.Operands[i])
                            : (this.operands[i] === undefined && operation.Operands[i] === undefined) ? true : false);
                    }
                    return isEqual;
                } else return false;
            }
        }

        export function RefreshStatesInOperation(operation: IOperand, states: Keyframe[]): boolean {
            if (operation === undefined)
                return false;

            if (operation instanceof Operation) {
                var wasUpdated = false;
                var operands = (<Operation>operation).Operands;

                for (var i = 0; i < operands.length; i++) {
                    var op = operands[i];

                    if (op === undefined)
                        continue;

                    if (op instanceof Operation) {
                        wasUpdated = this.RefreshStatesInOperation(operands[i], states) || wasUpdated;
                    } else {
                        if (op instanceof Keyframe) {
                            var name = (<Keyframe>op).Name;
                            if (name !== undefined) {
                                var updated = false;
                                for (var j = 0; j < states.length; j++) {
                                    if (states[j].Name === name) {
                                        wasUpdated = operands[i].GetFormula() !== states[j].GetFormula() || wasUpdated;
                                        operands[i] = states[j];
                                        updated = true;
                                        break;
                                    }
                                }
                                if (!updated) {
                                    operands[i] = undefined;
                                    wasUpdated = true;
                                }
                            }
                        }
                    }
                }

                return wasUpdated;
            }

            return false;
        }
    }
}  