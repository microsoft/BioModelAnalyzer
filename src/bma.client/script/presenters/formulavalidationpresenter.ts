module BMA {
    export module Presenters {
        export class FormulaValidationPresenter {
            private editorDriver: BMA.UIDrivers.IVariableEditor;
            private ajax: BMA.UIDrivers.IServiceDriver;

            constructor(editor: BMA.UIDrivers.IVariableEditor, ajax: BMA.UIDrivers.IServiceDriver) {
                var that = this;
                this.editorDriver = editor;
                this.ajax = ajax;

                window.Commands.On("FormulaEdited", function (param) {
                    var formula = param.formula;
                    var inputs = param.inputs;

                    for (var item in inputs) {
                        if (inputs[item] > 1) {
                            if (formula.split(item).length - 1 !== inputs[item]) {
                                that.editorDriver.SetValidation(false, 'Need equal number of repeating inputs in formula');
                                return;
                            }
                        }
                    }

                    if (formula !== "")
                        var result = that.ajax.Invoke({ Formula: formula })
                            .done(function (res) {
                                that.editorDriver.SetValidation(res.IsValid, res.Message);
                            })
                            .fail(function (res) {
                                that.editorDriver.SetValidation(undefined, '');
                            });
                    else {
                        that.editorDriver.SetValidation(undefined, '');
                    }
                })
            }
        }
    }
}
 