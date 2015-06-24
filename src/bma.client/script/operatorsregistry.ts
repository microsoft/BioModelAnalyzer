interface Window {
    OperatorsRegistry: BMA.Operators.OperatorsRegistry;
}

module BMA {
    export module Operators {

        export class Keyframe {
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

            public GetFormula(op: Operand[]) {
                return this.fun(op);
            }
        }

        export interface IGetFormula {
            (op: Operand[]): string;
        }

        export class Operand {
            private operand: Keyframe | Operation;

            public GetFormula() {
                return this.operand.GetFormula();
            }

            constructor(operand: Keyframe | Operation) {
                this.operand = operand;
            }
        }

        export class Operation {
            private operator: Operator;
            private operands: Operand[];
            
            public set Operator(op) {
                this.operator = op;
            }

            public set Operands(op) {
                this.operands = op;
            }

            public GetFormula() {
                //alert(this.operator.Name + ' ' + this.operands[0].GetFormula() + ' ' + this.operands[1].GetFormula());
                return this.operator.GetFormula(this.operands);
                //var formula = '(' + this.operator.GetFormula(this.operands);
                //for (var i = 0; i < this.operands.length; i++) {
                //    formula += ' ' + this.operands[i].GetFormula();
                //}
                //return formula + ')';
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
                throw "There is no operator you want";
            }

            constructor() {
                var that = this;
                this.operators = [];

                var formulacreator = function (funcname): IGetFormula {
                    return function (op: Operand[]) {
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