module BMA {
    export module Presenters {
        export class FormulaValidationPresenter {
            private editorDriver: BMA.UIDrivers.IVariableEditor;

            constructor(editor: BMA.UIDrivers.IVariableEditor) {
                var that = this;
                this.editorDriver = editor;

                window.Commands.On("FormulaEdited", function (formula) {
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
                    
                })
            }
        }
    }
}
 