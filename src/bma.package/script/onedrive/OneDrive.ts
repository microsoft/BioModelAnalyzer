/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>

// <script src="//js.live.net/v5.0/wl.js"></script>

module BMA.OneDrive {
    declare var WL: any;

    export class OneDriveSettings {
        private clientId: string;
        private redirectUri: string;
        private signinElementId: string;

        constructor(clientId: string, redirectUri: string, signinElementId: string) {
            this.clientId = clientId;
            this.redirectUri = redirectUri;
            this.signinElementId = signinElementId;
        }

        public get ClientId(): string {
            return this.clientId;
        }

        public get RedirectUri(): string {
            return this.redirectUri;
        }

        public get SignInElementId(): string {
            return this.signinElementId;
        }
    }

    // todo: enable pages when getting file list
    class OneDrive implements IOneDrive {
        private static selectFields = "select=id,name";
        private static selectDownloadFields = OneDrive.selectFields + ",@content.downloadUrl";
        private accessToken: string;

        constructor(session: any) {
            this.accessToken = <string>session.access_token;
        }

        private oneDriveApi(method, uri, body?, queryParams?): JQueryXHR {
            var settings = {
                method: method,
                headers: { "Authorization": "bearer " + this.accessToken },
                data: undefined,
                contentType: undefined
            };
            if (body) {
                settings.data = JSON.stringify(body);
                settings.contentType = "application/json; charset=utf-8";
            }
            var reqUri = "https://api.onedrive.com/v1.0" + uri + "?access_token=" + this.accessToken;
            if (queryParams) {
                reqUri += "&" + queryParams;
            }
            return $.ajax(reqUri, settings);
        }

        private get(uri): JQueryXHR {
            var settings = {
                method: "GET",
            };
            return $.ajax(uri, settings);
        }


        public GetUserProfile(): JQueryPromise<OneDriveUserProfile> {
            var d = $.Deferred();
            WL.api({
                path: "me",
                method: "GET"
            }).then(
                function (response) {
                    var userProfile: OneDriveUserProfile = $.extend({}, response);
                    userProfile.imageUri = "https://apis.live.net/v5.0/" + response.id + "/picture";
                    d.resolve(userProfile);
                },
                function (responseFailed) {
                    d.fail(responseFailed);
                }
                );
            return d.promise();
        }

        public CreateFolder(name: string): JQueryPromise<string> {
            var body = {
                "name": name,
                "folder": {}
            };
            return this.oneDriveApi("POST", "/drive/root/children", body)
                .then(function (r) {
                    return r.id;
                });
        }

        public FindFolder(name: string): JQueryPromise<string> {
            // https://dev.onedrive.com/items/list.htm
            var d = $.Deferred<string>();
            var that = this;
            var find = function (uri) {
                that.oneDriveApi("GET", uri)
                    .done(function (r) {
                        for (var i = 0; i < r.value.length; i++)
                            if (r.value[i].folder && r.value[i].name == name) {
                                d.resolve(r.value[i].id); // folder found
                                return;
                            }
                        // Check for @odata.nextLink if there are more than 200 items (default page size):
                        if (typeof (r["@odata.nextLink"]) === "string") {
                            find(r["@odata.nextLink"]); // continue search on the next page
                        } else {
                            d.resolve(null); // folder not found
                        }
                    })
                    .fail(function (err) {
                        d.reject(err);
                    });
            };

            find("/drive/root/children");
            return d.promise();
        }

        public EnumerateFiles(folderId: string): JQueryPromise<OneDriveFile[]> {
            return this.oneDriveApi("GET", "/drive/items/" + folderId + "/children", null, OneDrive.selectFields)
                .then(function (r) {
                    // TODO: Check for @odata.next if there are more than 200 items!!!
                    return r.value;
                });
        }

        public SaveFile(folderId: string, name: string, content: JSON): JQueryPromise<OneDriveFile> {
            // Replaces if exists
            return this.oneDriveApi("PUT", "/drive/items/" + folderId + "/children/" + name + "/content", content);
        }

        public LoadFile(fileId: string): JQueryPromise<JSON> {
            var that = this;
            return this.oneDriveApi("GET", "/drive/items/" + fileId, null, OneDrive.selectDownloadFields)
                .then(function (file) {
                    return that.get(file["@content.downloadUrl"])
                        .then(function (data, status, xhr: JQueryXHR) {
                            return data;
                        });
                });
        }

        public FileExists(fileId: string): JQueryPromise<boolean> {
            return this.oneDriveApi("GET", "/drive/items/" + fileId, null, OneDrive.selectFields + ",file")
                .then(function (file) {
                    return typeof (file) === "object" && typeof (file["file"]) === "object";
                });
        }
    }

    export class OneDriveConnector implements IOneDriveConnector {
        private settings: OneDriveSettings;

        constructor(settings: OneDriveSettings) {
            this.settings = settings;
        }

        public Enable(onLogin: (oneDrive: IOneDrive) => void, onLoginFailed: (error: LoginFailure) => void, onLogout: (any) => void): void {

            WL.Event.subscribe("auth.login", function (response) {
                if (response.error) {
                    onLoginFailed(response);
                } else {
                    var oneDrive = new OneDrive(response.session);
                    onLogin(oneDrive);
                }
            });

            WL.Event.subscribe("auth.logout", function (response) {
                onLogout(response);
            });


            WL.init({
                client_id: this.settings.ClientId,
                redirect_uri: this.settings.RedirectUri,
                scope: ["wl.signin", "onedrive.readwrite"],
                response_type: "token"
            });
            WL.ui({
                name: this.settings.SignInElementId,
                element: "signin",
                brand: "skydrive",
                theme: "white",
                type: "connect"
            });
        }
    }
}