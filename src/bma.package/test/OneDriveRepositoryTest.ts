/// <reference path="../script/onedrive/onedriverepository.ts" />



class OneDriveMockup implements BMA.OneDrive.IOneDrive {
    private data: Object;
    private files: Object;

    constructor() {
        this.data = {};
        this.files = {};
    }

    public GetUserProfile(): JQueryPromise<BMA.OneDrive.OneDriveUserProfile> {
        throw "not implemented";
    }

    /// Creates a root folder.
    /// Returns its ID.
    public CreateFolder(name: string): JQueryPromise<string> {
        let d = $.Deferred();
        this.data[name] = {};
        d.resolve(name);
        return d.promise();
    }

    // Finds a root folder with given name.
    // Returns its ID or null, if the folder is not found.
    public FindFolder(name: string): JQueryPromise<string> {
        let d = $.Deferred();
        if (typeof (this.data[name]) === "object") d.resolve(name);
        else d.resolve(null);
        return d.promise();
    }

    public EnumerateFiles(folderId: string): JQueryPromise<BMA.OneDrive.OneDriveFile[]> {
        throw "not implemented";
    }

    /// Creates or replaces a file in the given folder. 
    /// Returns the saved file information.
    public SaveFile(folderId: string, name: string, content: JSON): JQueryPromise<BMA.OneDrive.OneDriveFile> {
        let d = $.Deferred();
        if (typeof (this.data[folderId]) === "object") {
            this.files[name] = content;
            d.resolve({ id: name, name: name });
        } else { // folder doesn't exist
            d.reject("folder doesn't exist");
        }
        return d.promise();
    }

    public FileExists(fileId: string): JQueryPromise<boolean> {
        let d = $.Deferred();
        d.resolve(typeof (this.files[fileId]) === "object");
        return d.promise();
    }

    public LoadFile(fileId: string): JQueryPromise<JSON> {
        let d = $.Deferred();
        if (typeof (this.files[fileId]) === "object") {
            d.resolve(this.files[fileId]);
        }
        else d.reject("not found");
        return d.promise();
    }

    /// Returns true, if the operation is successful.
    public RemoveFile(fileId: string): JQueryPromise<boolean> {
        throw "not implemented";
    }
}

describe("OneDrive repository", function () {
    var oneDrive;
    var repo;
    var model = { name: "sample" };

    beforeEach(() => {
        oneDrive = new OneDriveMockup();
        repo = new BMA.OneDrive.OneDriveRepository(oneDrive);
    });

    it("initially returns an empty model list", function (done) {
        repo.GetModelList()
            .done(function (models) {
                expect(models.length).toEqual(0);
                done();
            })
            .fail(function (err) {
                expect(true).toBeFalsy();
            });;
    });

    it("checks if the model is missing", function (done) {
        repo.IsInRepo("missing_model")
            .done(function (isInRepo) {
                expect(isInRepo).toBeFalsy();
                done();
            })
            .fail(function (err) {
                expect(true).toBeFalsy();
            });
    });

    it("fails if asked to load a missing model", function (done) {
        repo.LoadModel("missing_model")
            .done(function () {
                expect(true).toBeFalsy();
            })
            .fail(function (err) {
                expect(true).toBeTruthy();
                done();
            });;
    });

    it("saves a model and gets the model information", function (done) {
        repo.SaveModel("my model", model)
            .fail(function (err) {
                expect(true).toBeFalsy();
            })
            .done(function (info: BMA.UIDrivers.ModelInfo) {
                expect(info.name).toEqual("my model");
                done();
            });
    });


    it("loads an existing model", function (done) {
        repo.SaveModel("my model", model)
            .fail(function (err) {
                expect(true).toBeFalsy();
            })
            .done(function (info: BMA.UIDrivers.ModelInfo) {
                repo.LoadModel(info.id)
                    .fail(function (err) {
                        expect(true).toBeFalsy();
                    })
                    .done(function (content: JSON) {
                        expect(content["name"]).toEqual(model.name);
                        done();
                    });
            });
    });
});