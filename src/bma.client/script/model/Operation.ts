module BMA {
    export module LTLOperations {

        export class Keyframe implements IOperand {
            private name: string;

            constructor(name: string) {
                this.name = name;
            }

            public GetFormula() {
                return this.name;
            }
        }

        export class Operator {
            private name: string;
            private fun: IGetFormula;

            constructor(name: string, fun: IGetFormula) {
                this.name = name;
                this.fun = fun;
            }

            get Name() {
                return this.name;
            }

            public GetFormula(op: IOperand[]) {
                return this.fun(op);
            }
        }

        export interface IGetFormula {
            (op: IOperand[]): string;
        }

        export interface IOperand {
            GetFormula(): string;
        }

        export class Operation implements IOperand {
            private operator: Operator;
            private operands: IOperand[];

            public set Operator(op) {
                this.operator = op;
            }

            public set Operands(op) {
                this.operands = op;
            }

            public GetFormula() {
                return this.operator.GetFormula(this.operands);
            }
        }

        
    }
}  