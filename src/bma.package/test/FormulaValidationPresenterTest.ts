describe("BMA.Presenters.FormulaValidationPresenter", () => {

    var variableEditorDriver: BMA.Test.VariableEditorTestDriver;
    var ajaxTestDriver: BMA.Test.AjaxTestDriver;
    var presenter: BMA.Presenters.FormulaValidationPresenter;
    window.Commands = new BMA.CommandRegistry();

    beforeEach(() => {
        variableEditorDriver = new BMA.Test.VariableEditorTestDriver();
        ajaxTestDriver = new BMA.Test.AjaxTestDriver();
        presenter = new BMA.Presenters.FormulaValidationPresenter(variableEditorDriver, ajaxTestDriver);
    })

    afterEach(() => {
        variableEditorDriver = undefined;
        ajaxTestDriver = undefined;
        presenter = undefined;
    })

    it("should be defined", () => {
        expect(presenter).toBeDefined();
    })

}); 