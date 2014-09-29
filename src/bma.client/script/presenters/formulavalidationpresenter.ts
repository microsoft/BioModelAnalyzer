module BMA {
    export module Presenters {
        export class FormulaValidationPresenter {
            private editorDriver: BMA.UIDrivers.IVariableEditor;

            constructor(editor: BMA.UIDrivers.IVariableEditor) {
                var that = this;
                this.editorDriver = editor;

                window.Commands.On("FormulaEdited", function () {
                    var v = that.RandomValidation() === 1;
                    that.editorDriver.SetValidation(v);
                })
            }

            public RandomValidation() {
                return Math.round(Math.random());
            }

        }
    }
}
 