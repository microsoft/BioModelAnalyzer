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

                //Temporal Properties editor operators
                this.operators.push(new Operator('AND', 2, 2));
                this.operators.push(new Operator('OR', 2, 2));
                this.operators.push(new Operator('IMPLIES', 2, 2));
                this.operators.push(new Operator('NOT', 1, 1, true));
                this.operators.push(new Operator('NEXT', 1, 1, true));
                this.operators.push(new Operator('ALWAYS', 1, 1, true));
                this.operators.push(new Operator('EVENTUALLY', 1, 1, true));
                this.operators.push(new Operator('UPTO', 2, 2));
                this.operators.push(new Operator('WEAKUNTIL', 2, 2));
                this.operators.push(new Operator('UNTIL', 2, 2));
                this.operators.push(new Operator('RELEASE', 2, 2));

                
                //Target Function editor operators
                this.operators.push(new Operator('AVG', 2, Number.POSITIVE_INFINITY, true,
                    "avg(x, y, z): The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )"));
                this.operators.push(new Operator('MIN', 2, Number.POSITIVE_INFINITY, true,
                    "min(x,y): The minimum of a two expressions. E.g., min( var(X), var(Y)), or min(var(X), 0)"));
                this.operators.push(new Operator('MAX', 2, Number.POSITIVE_INFINITY, true,
                    "max(x,y): The maximum of a two expressions. E.g., max( var(X), var(Y))"));

                this.operators.push(new Operator('CEIL', 1, 1, true,
                    "ceil(x): The ceiling of an expression. E.g., ceil (var(X))"));
                this.operators.push(new Operator('FLOOR', 1, 1, true,
                    "floor(x): The floor of an expression. E.g., floor(var(X))"));

                this.operators.push(new Operator('/', 2, 2, false,
                    "x / y: Usual addition operator. E.g., 2/3, 44 / var(X)"));
                this.operators.push(new Operator('*', 2, Number.POSITIVE_INFINITY, false,
                    "x * y: Usual addition operator. E.g., 2*3, 44 * var(X)"));
                this.operators.push(new Operator('+', 2, Number.POSITIVE_INFINITY, false,
                    "x + y: Usual addition operator. E.g., 2+3, 44 + var(X)"));
                this.operators.push(new Operator('-', 2, Number.POSITIVE_INFINITY, false,
                    "x - y: Usual addition operator. E.g., 2-3, 44 - var(X)"));

                this.operators.push(new Operator("VAR", 1, 1, true,
                    "var(name): A variable, where name is the name of the variable"));
                this.operators.push(new Operator("CONST", 1, 1, true,
                    "22 or const(22): An integer number. E.g., 1234, 42, -9"));
            }
        }
    }
}  