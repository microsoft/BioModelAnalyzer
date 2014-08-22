var BMA;
(function (BMA) {
    var functionConverter = (function () {
        function functionConverter() {
        }
        functionConverter.getAbout = function (fun) {
            switch (fun) {
                case "var":
                    return { head: "var(name)", content: "A variable, where name is the name of the variable", offset: 4, insertText: "var()" };
                case "avg":
                    return { head: "avg(x,y,z)", content: "The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )", offset: 4, insertText: "avg(,)" };
                case "min":
                    return { head: "min(x,y)", content: "The minimum of a two expressions. E.g., min( var(X), var(Y)), or min(var(X), 0)", offset: 4, insertText: "min(,)" };
                case "max":
                    return { head: "min(x,y)", content: "The maximum of a two expressions. E.g., max( var(X), var(Y))", offset: 4, insertText: "max(,)" };
                case "const":
                    return { head: "22 or const(22)", content: "An integer number. E.g., 1234, 42, -9", offset: 6, insertText: "const()" };
                case "plus":
                    return { head: "x + y", content: "Usual addition operator. E.g., 2+3, 44 + var(X)", offset: 3, insertText: " + " };
                case "minus":
                    return { head: "x - y", content: "Usual subtraction operator. E.g., 2-3, 44 - var(X)", offset: 3, insertText: " - " };
                case "times":
                    return { head: "x * y", content: "Usual multiplication operator. E.g., 2*3, 44 * var(X)", offset: 3, insertText: " * " };
                case "div":
                    return { head: "x / y", content: "Usual division operator. E.g., 2/3, 44 / var(X)", offset: 3, insertText: " / " };
                case "ceil":
                    return { head: "ceil(x)", content: "The ceiling of an expression. E.g., ceil (var(X))", offset: 5, insertText: "ceil()" };
                case "floor":
                    return { head: "floor(x)", content: "The floor of an expression. E.g., floor(var(X))", offset: 6, insertText: "floor()" };
            }
        };
        return functionConverter;
    })();
    BMA.functionConverter = functionConverter;
})(BMA || (BMA = {}));
//# sourceMappingURL=functionConverter.js.map
