var BMA;
(function (BMA) {
    (function (Presenters) {
        var FormulaValidationPresenter = (function () {
            function FormulaValidationPresenter(editor) {
                var that = this;
                this.editorDriver = editor;

                window.Commands.On("FormulaEdited", function (formula) {
                    var r = that.RandomValidation();

                    if (formula !== "")
                        $.ajax({
                            type: "POST",
                            url: "api/Validate",
                            data: {
                                Formula: formula
                            },
                            success: function (res) {
                                that.editorDriver.SetValidation(res.IsValid, res.Message);
                            },
                            error: function (res) {
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
