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

    xit("should call Invoke method on command 'FormulaEdited'", (done) => {
        var d = $.Deferred();
        d.done();
        spyOn(ajaxTestDriver, "Invoke");
        var formula = "test";
        //window.Commands.Execute("FormulaEdited", formula);
        var r = ajaxTestDriver.Invoke("api/Validate", { Formula: formula });
        
        r.done(function() {
            console.log("done");
        })

        expect(ajaxTestDriver.Invoke).toHaveBeenCalledWith("api/Validate", { Formula: formula });
        done();

    })
}); 