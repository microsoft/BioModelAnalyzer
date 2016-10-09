module BMA {
    export module LTLOperations {

        export interface IOperand {
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
            private operands: (BMA.LTLOperations.KeyframeEquation | BMA.LTLOperations.DoubleKeyframeEquation)[];

            constructor(name: string, description: string, operands: (BMA.LTLOperations.KeyframeEquation | BMA.LTLOperations.DoubleKeyframeEquation)[]) {
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

            public get Operands(): (BMA.LTLOperations.KeyframeEquation | BMA.LTLOperations.DoubleKeyframeEquation)[] {
                return this.operands;
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
            private minOperandsCount: number;
            private maxOperandsCount: number;
            private isFunction: boolean;
            private description: string;

            constructor(name: string, minOperandsCount: number, maxOperandsCount: number = -1, isFunction: boolean = false, description: string = undefined) {
                this.name = name;
                this.minOperandsCount = minOperandsCount;
                if (maxOperandsCount === -1) {
                    this.maxOperandsCount = minOperandsCount;
                } else {
                    this.maxOperandsCount = maxOperandsCount;
                }
                this.isFunction = isFunction;
                this.description = description;
            }

            get IsFunction() {
                return this.isFunction;
            }

            get Name() {
                return this.name;
            }

            get MinOperandsCount() {
                return this.minOperandsCount;
            }

            get MaxOperandsCount() {
                return this.maxOperandsCount;
            }

            get Description() {
                return this.description;
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

            public Clone() {
                var operands = [];
                for (var i = 0; i < this.operands.length; i++) {

                    operands.push(this.operands[i] === undefined ? undefined : this.operands[i].Clone());
                }
                var result = new Operation();
                result.Operator = new Operator(this.operator.Name, this.operator.MinOperandsCount, this.operator.MaxOperandsCount, this.Operator.IsFunction);
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
                                        wasUpdated = GetLTLServiceProcessingFormula(operands[i]) !== GetLTLServiceProcessingFormula(states[j]) || wasUpdated;
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