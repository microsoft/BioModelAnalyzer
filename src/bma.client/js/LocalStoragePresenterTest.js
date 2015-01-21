describe("BMA.Presenters.LocalStoragePresenter", function () {
    window.Commands = new BMA.CommandRegistry();

    var appModel = new BMA.Model.AppModel();

    var name = "TestBioModel";
    var v1 = new BMA.Model.Variable(34, 15, "type1", "name1", 3, 7, "formula1");
    var v2 = new BMA.Model.Variable(38, 10, "type2", "name2", 1, 14, "formula2");
    var r1 = new BMA.Model.Relationship(3, 34, 38, "type1");
    var r2 = new BMA.Model.Relationship(3, 38, 34, "type2");
    var r3 = new BMA.Model.Relationship(3, 34, 34, "type3");
    var variables = [v1, v2];
    var relationships = [r1, r2, r3];
    var biomodel = new BMA.Model.BioModel(name, variables, relationships);

    var VL1 = new BMA.Model.VariableLayout(15, 97, 0, 54, 32, 16);
    var VL2 = new BMA.Model.VariableLayout(62, 22, 41, 0, 3, 7);
    var VL3 = new BMA.Model.VariableLayout(9, 14, 75, 6, 4, 0);
    var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
    var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
    var containers = [CL1, CL2];
    var varialbes = [VL1, VL2, VL3];
    var layout = new BMA.Model.Layout(containers, varialbes);

    appModel.BioModel = biomodel;
    appModel.Layout = layout;

    var localStorageTestDriver = new BMA.Test.LocalStorageTestDriver();
    var modelRepositoryTest = new BMA.Test.ModelRepositoryTest();
    var messagebox = new BMA.UIDrivers.MessageBoxDriver();
    var checker = new BMA.ChangesChecker();
    var logService = new BMA.SessionLog();

    it("should be defined", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        expect(localStorageTestPresenter).toBeDefined();
    });

    it("should GetModelList and SetItems on 'LocalStorageChanged' command", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(modelRepositoryTest, "GetModelList");
        spyOn(localStorageTestDriver, "SetItems");
        window.Commands.Execute("LocalStorageChanged", {});
        expect(modelRepositoryTest.GetModelList).toHaveBeenCalledWith();
        var keys = modelRepositoryTest.GetModelList();
        expect(localStorageTestDriver.SetItems).toHaveBeenCalledWith(keys);
    });

    it("should RemoveModel on 'LocalStorageRemoveModel' command", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(modelRepositoryTest, "RemoveModel");
        var key = "3";
        window.Commands.Execute("LocalStorageRemoveModel", key);
        expect(modelRepositoryTest.RemoveModel).toHaveBeenCalledWith(key);
    });

    it("should Show storage viewer on 'LocalStorageRequested' command", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(localStorageTestDriver, "Show");
        window.Commands.Execute("LocalStorageRequested", {});
        expect(localStorageTestDriver.Show).toHaveBeenCalledWith();
    });

    it("should SaveModel on 'LocalStorageSaveModel' command", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(modelRepositoryTest, "SaveModel");
        window.Commands.Execute("LocalStorageSaveModel", {});
        expect(modelRepositoryTest.SaveModel).toHaveBeenCalledWith(name, JSON.parse(appModel.Serialize()));
    });

    xit("should Reset appModel on 'LocalStorageLoadModel' command when id is correct", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(appModel, "Reset");

        //var key = '4';
        window.Commands.Execute("LocalStorageSaveModel", {});
        window.Commands.Execute("LocalStorageLoadModel", "user." + name);
        expect(appModel.Reset).toHaveBeenCalledWith(JSON.stringify(modelRepositoryTest.LoadModel(name)));
    });

    xit("shouldn't Reset appModel on 'LocalStorageLoadModel' command when id is not correct", function () {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService);
        spyOn(appModel, "Reset");
        var key = 'testkey';
        window.Commands.Execute("LocalStorageLoadModel", key);
        expect(appModel.Reset).not.toHaveBeenCalled();
    });
});
