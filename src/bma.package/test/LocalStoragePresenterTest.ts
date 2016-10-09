describe("BMA.Presenters.LocalStoragePresenter", () => {

    window.Commands = new BMA.CommandRegistry();

    var appModel = new BMA.Model.AppModel();
    var testWaitScreen = new BMA.Test.TestWaitScreen();

    var name = "TestBioModel";
    var v1 = new BMA.Model.Variable(34, 15, "type1", "name1", 3, 7, "formula1");
    var v2 = new BMA.Model.Variable(38, 10, "type2", "name2", 1, 14, "formula2");
    var r1 = new BMA.Model.Relationship(3, 34, 38, "type1");
    var r2 = new BMA.Model.Relationship(3, 38, 34, "type2");
    var r3 = new BMA.Model.Relationship(3, 34, 34, "type3");
    var variables = [v1, v2];
    var relationships = [r1, r2, r3];
    var biomodel = new BMA.Model.BioModel(name, variables, relationships);

    var VL1 = new BMA.Model.VariableLayout(34, 97, 0, 54, 32, 16);
    var VL2 = new BMA.Model.VariableLayout(38, 22, 41, 0, 3, 7);
    //var VL3 = new BMA.Model.VariableLayout(9, 14, 75, 6, 4, 0);
    var CL1 = new BMA.Model.ContainerLayout(7, "", 5, 1, 6);
    var CL2 = new BMA.Model.ContainerLayout(3, "", 24, 81, 56);
    var containers = [CL1, CL2];
    var varialbes = [VL1, VL2];//, VL3];
    var layout = new BMA.Model.Layout(containers, varialbes);

    appModel.BioModel = biomodel;
    appModel.Layout = layout;

    var localStorageWidget = $('<div></div>').localstoragewidget();
    var localStorageTestDriver = new BMA.Test.LocalStorageTestDriver(localStorageWidget);
    var modelRepositoryTest = new BMA.Test.ModelRepositoryTest();
    var messagebox = new BMA.UIDrivers.MessageBoxDriver();
    var checker = new BMA.ChangesChecker();
    var logService = new BMA.SessionLog();

    modelRepositoryTest.SaveModel("user." + name, JSON.parse(appModel.Serialize()));

    it("should be defined", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        expect(localStorageTestPresenter).toBeDefined();
    });

    it("should GetModelList and SetItems on 'LocalStorageChanged' command", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        spyOn(modelRepositoryTest, "GetModelList").and.callThrough();
        spyOn(localStorageTestDriver, "SetItems");
        window.Commands.Execute("LocalStorageChanged", {});
        expect(modelRepositoryTest.GetModelList).toHaveBeenCalled();
        var keys;
        modelRepositoryTest.GetModelList().done(function (result) { keys = result; });
        expect(localStorageTestDriver.SetItems).toHaveBeenCalledWith(keys);
    });

    it("should RemoveModel on 'LocalStorageRemoveModel' command", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        spyOn(modelRepositoryTest, "RemoveModel");
        var list = localStorageWidget.find("ol").children("li");
        list.eq(0).children("button").click();
        //window.Commands.Execute("LocalStorageRemoveModel", key);
        expect(modelRepositoryTest.RemoveModel).toHaveBeenCalled();
    });

    it("should Show storage viewer with updated model list on 'LocalStorageRequested' command", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        spyOn(localStorageTestDriver, "SetItems");
        //spyOn(localStorageTestDriver, "Show");
        window.Commands.Execute("LocalStorageRequested", {});
        expect(localStorageTestDriver.SetItems).toHaveBeenCalled();
    });

    it("should SaveModel on 'LocalStorageSaveModel' command", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        spyOn(modelRepositoryTest, "SaveModel");
        window.Commands.Execute("LocalStorageSaveModel", {});
        expect(modelRepositoryTest.SaveModel).toHaveBeenCalledWith(name, JSON.parse(appModel.Serialize()));
    });

    it("should reset appModel when item from list was selected", () => {
        var localStorageTestPresenter = new BMA.Presenters.LocalStoragePresenter(appModel, localStorageTestDriver, modelRepositoryTest, messagebox, checker, logService, testWaitScreen);
        localStorageTestPresenter.SetOnRequestLoad(function (key) {
            localStorageTestPresenter.LoadModel(key);
        });
        spyOn(appModel, "Deserialize");
        var ol = localStorageWidget.find("ol").eq(0);
        var li = ol.children().eq(0);
        li.click();
        //ol.children().eq(0).addClass("ui-selected");
        //var st = ol.selectable("option", "stop");
        //st();
        expect(appModel.Deserialize).toHaveBeenCalled();
    });
    
}); 