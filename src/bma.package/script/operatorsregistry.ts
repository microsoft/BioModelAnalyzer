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

                //Temporal Properties editor operators
                this.operators.push(new Operator('AND', 2, 2, formulacreator('And')));
                this.operators.push(new Operator('OR', 2, 2, formulacreator('Or')));
                this.operators.push(new Operator('IMPLIES', 2, 2, formulacreator('Implies')));
                this.operators.push(new Operator('NOT', 1, 1, formulacreator('Not'), true));
                this.operators.push(new Operator('NEXT', 1, 1, formulacreator('Next'), true));
                this.operators.push(new Operator('ALWAYS', 1, 1, formulacreator('Always'), true));
                this.operators.push(new Operator('EVENTUALLY', 1, 1, formulacreator('Eventually'), true));
                this.operators.push(new Operator('UPTO', 2, 2, formulacreator('Upto')));
                this.operators.push(new Operator('WEAKUNTIL', 2, 2, formulacreator('Weakuntil')));
                this.operators.push(new Operator('UNTIL', 2, 2, formulacreator('Until')));
                this.operators.push(new Operator('RELEASE', 2, 2, formulacreator('Release')));

                
                //Target Function editor operators
                this.operators.push(new Operator('AVG', 2, Number.POSITIVE_INFINITY, functionformulacreator('avg'), true,
                    "avg(x, y, z): The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )"));
                this.operators.push(new Operator('MIN', 2, Number.POSITIVE_INFINITY, functionformulacreator('min'), true,
                    "min(x,y): The minimum of a two expressions. E.g., min( var(X), var(Y)), or min(var(X), 0)"));
                this.operators.push(new Operator('MAX', 2, Number.POSITIVE_INFINITY, functionformulacreator('max'), true,
                    "max(x,y): The maximum of a two expressions. E.g., max( var(X), var(Y))"));

                this.operators.push(new Operator('CEIL', 1, 1, formulacreator('ceil'), true,
                    "ceil(x): The ceiling of an expression. E.g., ceil (var(X))"));
                this.operators.push(new Operator('FLOOR', 1, 1, formulacreator('floor'), true,
                    "floor(x): The floor of an expression. E.g., floor(var(X))"));

                this.operators.push(new Operator('/', 2, 2, operatorformulacreator('/'), false,
                    "x / y: Usual addition operator. E.g., 2/3, 44 / var(X)"));
                this.operators.push(new Operator('*', 2, Number.POSITIVE_INFINITY, operatorformulacreator('*'), false,
                    "x * y: Usual addition operator. E.g., 2*3, 44 * var(X)"));
                this.operators.push(new Operator('+', 2, Number.POSITIVE_INFINITY, operatorformulacreator('+'), false,
                    "x + y: Usual addition operator. E.g., 2+3, 44 + var(X)"));
                this.operators.push(new Operator('-', 2, Number.POSITIVE_INFINITY, operatorformulacreator('-'), false,
                    "x - y: Usual addition operator. E.g., 2-3, 44 - var(X)"));

                this.operators.push(new Operator("VAR", 1, 1, functionformulacreator("var"), true,
                    "var(name): A variable, where name is the name of the variable"));
                this.operators.push(new Operator("CONST", 1, 1, functionformulacreator("const"), true,
                    "22 or const(22): An integer number. E.g., 1234, 42, -9"));
            }
        }
    }
}  