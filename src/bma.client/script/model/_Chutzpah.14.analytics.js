var BMA;
(function (BMA) {
    (function (Model) {
        var ProofResult = (function () {
            function ProofResult(isStable, time) {
                this.isStable = isStable;
                this.time = time;
            }
            Object.defineProperty(ProofResult.prototype, "IsStable", {
                get: function () {
                    return this.isStable;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(ProofResult.prototype, "Time", {
                get: function () {
                    return this.time;
                },
                enumerable: true,
                configurable: true
            });
            return ProofResult;
        })();
        Model.ProofResult = ProofResult;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
