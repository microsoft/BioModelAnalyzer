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

                var functionformulacreator = function (funcname): IGetFormula {
                    return function (op: IOperand[]) {
                        var f = funcname + '(';
                        for (var i = 0; i < op.length - 1 /*because last can be FlexSlot*/; i++) {
                            f += ', ' + op[i].GetFormula();
                        }

                        /*
                        if (op[op.length - 1] instanceof FlexOperand) {
                            f += ")";
                        } else {
                            f += +  ", " + op[op.length - 1].GetFormula() + ")";
                        }
                        */
                        f += +  ", " + op[op.length - 1].GetFormula() + ")";

                        return f;
                    }
                }

                var operatorformulacreator = function (funcname): IGetFormula {
                    return function (op: IOperand[]) {
                        var f = '(' + op[0].GetFormula();
                        for (var i = 1; i < op.length - 1 /*because last can be FlexSlot*/; i++) {
                            f += + " " + funcname + " " + op[i].GetFormula();
                        }
                        /*
                        if (op[op.length - 1] instanceof FlexOperand) {
                            f += ")";
                        } else {
                            f += + " " + funcname + " " + op[op.length - 1].GetFormula() + ")";
                        }
                        */
                        f += + " " + funcname + " " + op[op.length - 1].GetFormula() + ")";

                        return f;
                    }
                }


                this.operators.push(new Operator('AND', 2, formulacreator('And')));
                this.operators.push(new Operator('OR', 2, formulacreator('Or')));
                this.operators.push(new Operator('IMPLIES', 2, formulacreator('Implies')));
                this.operators.push(new Operator('NOT', 1, formulacreator('Not'), true));
                this.operators.push(new Operator('NEXT', 1, formulacreator('Next'), true));
                this.operators.push(new Operator('ALWAYS', 1, formulacreator('Always'), true));
                this.operators.push(new Operator('EVENTUALLY', 1, formulacreator('Eventually'), true));
                this.operators.push(new Operator('UPTO', 2, formulacreator('Upto')));
                this.operators.push(new Operator('WEAKUNTIL', 2, formulacreator('Weakuntil')));
                this.operators.push(new Operator('UNTIL', 2, formulacreator('Until')));
                this.operators.push(new Operator('RELEASE', 2, formulacreator('Release')));

                /*
                //Target Function Editor operators

                this.operators.push(new Operator('AVG', Number.POSITIVE_INFINITY, functionformulacreator('avg')));
                this.operators.push(new Operator('MIN', Number.POSITIVE_INFINITY, functionformulacreator('min')));
                this.operators.push(new Operator('MAX', Number.POSITIVE_INFINITY, functionformulacreator('max')));

                this.operators.push(new Operator('CEIL', 1, formulacreator('ceil')));
                this.operators.push(new Operator('FLOOR', 1, formulacreator('floor')));

                this.operators.push(new Operator('/', 2, operatorformulacreator('/')));
                this.operators.push(new Operator('*', Number.POSITIVE_INFINITY, operatorformulacreator('*')));
                this.operators.push(new Operator('+', Number.POSITIVE_INFINITY, operatorformulacreator('+')));
                this.operators.push(new Operator('-', Number.POSITIVE_INFINITY, operatorformulacreator('-')));
                */
            }
        }
    }
}  