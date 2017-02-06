// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
interface Window {
    FunctionsRegistry: BMA.Functions.FunctionsRegistry;
}

module BMA {
    export module Functions {
        export class BMAFunction {
            private name: string;
            private head: string;
            private about: string;
            private inserttext: string;
            private offset: number;
            

            public get Name(): string {
                return this.name;
            }

            public get Head(): string {
                return this.head;
            }

            public get About(): string {
                return this.about;
            }

            public get InsertText(): string {
                return this.inserttext;
            }

            public get Offset(): number {
                return this.offset;
            }
            
            constructor(
                name: string,
                head: string,
                about: string,
                inserttext: string,
                offset: number) {

                this.name = name;
                this.head = head;
                this.about = about;
                this.inserttext = inserttext;
                this.offset = offset;
            }
        }


        

        export class FunctionsRegistry {
            private functions: BMAFunction[];

            public get Functions(): BMAFunction[] {
                return this.functions;
            }

            public GetFunctionByName(name: string): BMAFunction {
                for (var i = 0; i < this.functions.length; i++) {
                    if (this.functions[i].Name === name)
                        return this.functions[i];
                }
                throw "There is no function as you want";
            }

            constructor() {
                var that = this;
                this.functions = [];

                this.functions.push(new BMAFunction("VAR", "var(name)", "A variable, where name is the name of the variable", "var()", 4));
                this.functions.push(new BMAFunction("POS", "pos(name)", "", "pos()", 4));
                this.functions.push(new BMAFunction("NEG", "neg(name)", "", "neg()", 4));
                this.functions.push(new BMAFunction("AVG", "avg(x,y,z)", "The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )", "avg(,)", 4));
                this.functions.push(new BMAFunction("MIN", "min(x,y)", "The minimum of a two expressions. E.g., min( var(X), var(Y)), or min(var(X), 0)", "min(,)", 4));
                this.functions.push(new BMAFunction("MAX", "max(x,y)", "The maximum of a two expressions. E.g., max( var(X), var(Y))", "max(,)", 4));
                this.functions.push(new BMAFunction("CONST", "22 or const(22)", "An integer number. E.g., 1234, 42, -9", "const()", 6));
                this.functions.push(new BMAFunction("+", "x + y", "Usual addition operator. E.g., 2+3, 44 + var(X)", " + ", 3));
                this.functions.push(new BMAFunction("-", "x - y", "Usual addition operator. E.g., 2-3, 44 - var(X)", " - ", 3));
                this.functions.push(new BMAFunction("*", "x * y", "Usual addition operator. E.g., 2*3, 44 * var(X)", " * ", 3));
                this.functions.push(new BMAFunction("/", "x / y", "Usual addition operator. E.g., 2/3, 44 / var(X)", " / ", 3));
                this.functions.push(new BMAFunction("CEIL", "ceil(x)", "The ceiling of an expression. E.g., ceil (var(X))", "ceil()", 5));
                this.functions.push(new BMAFunction("FLOOR","floor(x)", "The floor of an expression. E.g., floor(var(X))", "floor()", 6));
            }
        }
    }
} 
