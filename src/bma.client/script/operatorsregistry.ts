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