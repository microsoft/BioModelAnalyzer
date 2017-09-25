// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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
        var d = $.Deferred();
        this.data[name] = {};
        d.resolve(name);
        return <JQueryPromise<string>>d.promise();
    }

    // Finds a root folder with given name.
    // Returns its ID or null, if the folder is not found.
    public FindFolder(name: string): JQueryPromise<string> {
        var d = $.Deferred();
        if (typeof (this.data[name]) === "object") d.resolve(name);
        else d.resolve(null);
        return <JQueryPromise<string>>d.promise();
    }

    public EnumerateFiles(folderId: string): JQueryPromise<BMA.OneDrive.OneDriveFile[]> {
        var d = $.Deferred();
        var r = [];

        for (var p in this.files) {
            r.push(this.files[p]);
        }
        d.resolve(r);

        return <JQueryPromise<BMA.OneDrive.OneDriveFile[]>>d.promise();
    }


    public EnumerateSharedWithMeFiles(): JQueryPromise<BMA.OneDrive.OneDriveFile[]> {
        var d = $.Deferred();
        d.resolve([]);
        return <JQueryPromise<BMA.OneDrive.OneDriveFile[]>>d.promise();
    }

    /// Creates or replaces a file in the given folder. 
    /// Returns the saved file information.
    public SaveFile(folderId: string, name: string, content: JSON): JQueryPromise<BMA.OneDrive.OneDriveFile> {
        var d = $.Deferred();
        if (typeof (this.data[folderId]) === "object") {
            this.files[name] = content;
            d.resolve({ id: name, name: name });
        } else { // folder doesn't exist
            d.reject("folder doesn't exist");
        }
        return <JQueryPromise<BMA.OneDrive.OneDriveFile>>d.promise();
    }

    public FileExists(fileId: string): JQueryPromise<boolean> {
        var d = $.Deferred();
        d.resolve(typeof (this.files[fileId]) === "object");
        return <JQueryPromise<boolean>>d.promise();
    }

    public LoadFile(fileId: string): JQueryPromise<JSON> {
        var d = $.Deferred();
        if (typeof (this.files[fileId]) === "object") {
            d.resolve(this.files[fileId]);
        }
        else d.reject("not found");
        return <JQueryPromise<JSON>>d.promise();
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
            });
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

    //xit("enumerate models includes shared with me files", function (done) {
    //    oneDrive.EnumerateSharedWithMeFiles = function () {
    //        var d = $.Deferred();
    //        d.resolve([
    //            { id: "0", file: { mimeType: "plain/text" }, name: "1.txt" },
    //            { id: "1", file: { mimeType: "application/json" }, name: "1.json" },
    //            { id: "2", file: { mimeType: "application/json" }, name: "2.json" }
    //        ]);
    //        return d.promise();
    //    };

    //    repo.GetModelList()
    //        .done(function (models) {
    //            expect(models.length).toEqual(2);
    //            expect(models[0].name).toEqual("1");
    //            expect(models[1].name).toEqual("2");
    //            done();
    //        })
    //        .fail(function (err) {
    //            expect(true).toBeFalsy();
    //        });
    //});

    //xit("enumerate models includes my files and shared with me files", function (done) {
    //    oneDrive.EnumerateSharedWithMeFiles = function () {
    //        var d = $.Deferred();
    //        d.resolve([
    //            { id: "0", file: { mimeType: "plain/text" }, name: "1.txt" },
    //            { id: "1", file: { mimeType: "application/json" }, name: "1.json" },
    //            { id: "2", file: { mimeType: "application/json" }, name: "2.json" }
    //        ]);
    //        return d.promise();
    //    };

    //    repo.SaveModel("my model", model)
    //        .fail(function (err) {
    //            expect(true).toBeFalsy();
    //        })
    //        .done(function (info: BMA.UIDrivers.ModelInfo) {
    //            repo.GetModelList()
    //                .done(function (models) {
    //                    expect(models.length).toEqual(3);
    //                    expect(models[0].name).toEqual("sample");
    //                    expect(models[1].name).toEqual("1");
    //                    expect(models[2].name).toEqual("2");
    //                    done();
    //                })
    //                .fail(function (err) {
    //                    expect(true).toBeFalsy();
    //                });
    //        });
    //});

    it("enumerate models includes my files", function (done) {
        repo.SaveModel("my model", model)
            .fail(function (err) {
                expect(true).toBeFalsy();
            })
            .done(function (info: BMA.UIDrivers.ModelInfo) {
                repo.GetModelList()
                    .done(function (models) {
                        expect(models.length).toEqual(1);
                        expect(models[0].name).toEqual("sample");
                        done();
                    })
                    .fail(function (err) {
                        expect(true).toBeFalsy();
                    });
            });
    });
});
