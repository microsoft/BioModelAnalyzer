module BMA {
    export module Presenters {
        export class FormulaValidationPresenter {
            private editorDriver: BMA.UIDrivers.IVariableEditor;

            constructor(editor: BMA.UIDrivers.IVariableEditor) {
                var that = this;
                this.editorDriver = editor;

                window.Commands.On("FormulaEdited", function () {
                    var r = that.RandomValidation();
                    that.editorDriver.SetValidation(r.res === 1, r.error);
                })
            }

            public RandomValidation(): { res; error} {
                return { res: Math.round(Math.random()), error: (Math.random()*100).toString() };
            }

        }
    }
}
 