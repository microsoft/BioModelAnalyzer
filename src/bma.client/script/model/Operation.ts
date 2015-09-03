module BMA {
    export module LTLOperations {


        export interface IGetFormula {
            (op: IOperand[]): string;
        }

        export interface IOperand {
            GetFormula(): string;
            Clone(): IOperand;
        }

        export interface IOperationLayout {
            GetSVG(position: { x: number; y: number }): string
            //FindIntersection(point: { x: number; y: number }): 
        }

        export class Keyframe implements IOperand {
            private name: string;

            constructor(name: string) {
                this.name = name;
            }

            public GetFormula() {
                return this.name;
            }

            public Clone() {
                return new BMA.LTLOperations.Keyframe(this.name);
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