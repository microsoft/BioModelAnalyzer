interface Window {
    OperatorsRegistry: BMA.Operators.OperatorsRegistry;
}

module BMA {
    export module Operators {

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

        export class OperatorsRegistry {
            private operators: Operator[];

            public get Operators(): Operator[] {
                return this.operators;
            }

            public GetOperatorByName(name: string): Operator {
                for (var i = 0; i < this.operators.length; i++) {
                    if (this.operators[i].Name === name)
                        return this.operators[i];
                }
                return undefined;
            }

            constructor() {
                var that = this;
                this.operators = [];

                var formulacreator = function (funcname): IGetFormula {
                    return function (op: IOperand[]) {
                              var f = '(' + funcname;
                              for (var i = 0; i < op.length; i++) {
                                  f += ' ' + op[i].GetFormula();
                             }
                              return f + ')';
                          }
                }

                this.operators.push(new Operator('Until', formulacreator('Until')));
                this.operators.push(new Operator('Release', formulacreator('Release')));
                this.operators.push(new Operator('And', formulacreator('And')));
                this.operators.push(new Operator('Or', formulacreator('Or')));
                this.operators.push(new Operator('Implies', formulacreator('Implies')));
                this.operators.push(new Operator('Not', formulacreator('Not')));
                this.operators.push(new Operator('Next', formulacreator('Next')));
                this.operators.push(new Operator('Always', formulacreator('Always')));
                this.operators.push(new Operator('Eventually', formulacreator('Eventually')));
            }
        }
    }
}  