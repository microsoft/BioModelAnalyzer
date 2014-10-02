var BMA;
(function (BMA) {
    (function (Presenters) {
        var FormulaValidationPresenter = (function () {
            function FormulaValidationPresenter(editor) {
                var that = this;
                this.editorDriver = editor;

                window.Commands.On("FormulaEdited", function (formula) {
                    //alert("validation");
                    var r = that.RandomValidation();

                    //$("#log").append("Invoking api/Validate (for correct formula)...<br/>");
                    if (formula !== "")
                        $.ajax({
                            type: "POST",
                            url: "api/Validate",
                            data: {
                                Formula: formula
                            },
                            success: function (res) {
                                that.editorDriver.SetValidation(res.IsValid, res.Message);
                                //$("#log").append("Validate success (for correct). IsValid: " + res.IsValid + "<br/>");
                            },
                            error: function (res) {
                                //$("#log").append("Validate error: " + res.statusText + "<br/>");
                            }
                        });
                    else {
                        that.editorDriver.SetValidation(undefined, '');
                    }
                });
            }
            FormulaValidationPresenter.prototype.RandomValidation = function () {
                return { res: Math.round(Math.random()), error: (Math.random() * 100).toString() };
            };
            return FormulaValidationPresenter;
        })();
        Presenters.FormulaValidationPresenter = FormulaValidationPresenter;
    })(BMA.Presenters || (BMA.Presenters = {}));
    var Presenters = BMA.Presenters;
})(BMA || (BMA = {}));
//# sourceMappingURL=formulavalidationpresenter.js.map
