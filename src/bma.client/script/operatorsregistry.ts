interface Window {
    OperatorsRegistry: BMA.LTLOperations.OperatorsRegistry;
}

module BMA {
    export module LTLOperations {

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

                this.operators.push(new Operator('UNTIL', 2, formulacreator('Until')));
                this.operators.push(new Operator('RELEASE', 2, formulacreator('Release')));
                this.operators.push(new Operator('AND', 2, formulacreator('And')));
                this.operators.push(new Operator('OR', 2, formulacreator('Or')));
                this.operators.push(new Operator('IMPLIES', 2, formulacreator('Implies')));
                this.operators.push(new Operator('NOT', 1, formulacreator('Not')));
                this.operators.push(new Operator('NEXT', 1, formulacreator('Next')));
                this.operators.push(new Operator('ALWAYS', 1, formulacreator('Always')));
                this.operators.push(new Operator('EVENTUALLY', 1, formulacreator('Eventually')));
            }
        }
    }
}  