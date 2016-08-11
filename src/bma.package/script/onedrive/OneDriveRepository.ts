/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>

// <script src="//js.live.net/v5.0/wl.js"></script>

module BMA.OneDrive {
    //************************************************************************************************
    // 
    // OneDrive API
    //
    //************************************************************************************************

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

    export class OneDriveUserProfile {
        id: string;
        first_name: string;
        last_name: string;
        imageUri: string;
    }
    
    export class OneDriveFile {
        public file: string;
        public name: string;
        public shared: any;
    }

    export class LoginFailure {
        public error: any;
        public error_description: any;        
    }

    export interface IOneDrive {
        GetUserProfile(): JQueryPromise<OneDriveUserProfile>;
        //CreateFolder(name:string) : JQueryPromise<string>;
        
        // Finds a root folder with given name.
        // Returns its ID or null, if the folder is not found.
        FindFolder(name:string) : JQueryPromise<string>;
        EnumerateFiles(folderId:string) : JQueryPromise<OneDriveFile[]>;        
    }

    export interface IOneDriveConnector {
        Enable(onLogin: (oneDrive: IOneDrive) => void, onLoginFailed: (error: LoginFailure) => void, onLogout: (any) => void): void;
    }



    class OneDrive implements IOneDrive {
        private accessToken: string;

        constructor(session : any){
            this.accessToken = <string>session.access_token;
        }

        private oneDriveApi(method, uri, body?) : JQueryXHR {
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
            return $.ajax("https://api.onedrive.com/v1.0" + uri + "?access_token=" + this.accessToken, settings);                
        }

        public GetUserProfile(): JQueryPromise<OneDriveUserProfile> {
            var d = $.Deferred();
            WL.api({
                path: "me",
                method: "GET"
            }).then(
                function (response) {
                    var userProfile : OneDriveUserProfile = $.extend({}, response);
                    userProfile.imageUri = "https://apis.live.net/v5.0/" + response.id + "/picture";
                    d.resolve(userProfile);
                },
                function(responseFailed) {
                    d.fail(responseFailed);
                }
            );
            return d.promise();
        }

        public FindFolder(name:string) : JQueryPromise<string> {
            return this.oneDriveApi("GET", "/drive/root/children")
                .then(function(r) {
                    var folderId : string = null;
                    for(var i = 0; i < r.value.length; i++)
                        if(r.value[i].folder && r.value[i].name == name) {
                            folderId = r.value[i].id;
                            break; 
                        }
                    // TODO: Check for @odata.next if there are more than 200 items!!!
                    return folderId;                
                });
        }

        public EnumerateFiles(folderId:string) : JQueryPromise<OneDriveFile[]> {
            return this.oneDriveApi("GET", "/drive/items/" + folderId + "/children")
                .then(function (r) {
                    return r.value;
                });
        }
    }

    export class OneDriveConnector implements IOneDriveConnector {
        private settings: OneDriveSettings;

        constructor(settings: OneDriveSettings){
            this.settings = settings;
        }

        public Enable(onLogin: (oneDrive: IOneDrive) => void, onLoginFailed: (error: LoginFailure) => void, onLogout: (any) => void): void {
            
            WL.Event.subscribe("auth.login", function(response){
                if(response.error){
                    onLoginFailed(response);
                }else{
                    var oneDrive = new OneDrive(response.session);
                    onLogin(oneDrive);
                }
            });

            WL.Event.subscribe("auth.logout", function(response){
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

    //************************************************************************************************
    // 
    // Models and repository
    //
    //************************************************************************************************


    export class OneDriveRepository {
        private oneDrive : IOneDrive;

        constructor(oneDrive : IOneDrive){
            this.oneDrive = oneDrive;
        }

       public GetUserProfile(): JQueryPromise<OneDriveUserProfile> {
           return this.oneDrive.GetUserProfile();
       }        

       public GetModelList(): JQueryPromise<OneDriveFile[]> {
           var d = $.Deferred();
           var oneDrive = this.oneDrive;

           oneDrive.FindFolder("BioModelAnalyzer")
            .done(function(folderId: string){
                if (folderId){
                    oneDrive.EnumerateFiles(folderId)
                        .done(function (files) {
                            d.resolve(files);
                        })
                        .fail(function (error: any) {
                            d.reject(error);
                        });
                }else{
                    console.log("not found");
                    d.reject("todo: create then enum");
                }
            })
            .fail(function(error){
                d.reject(error);
            });
            return d.promise();
       }
    }


    //export class OneDriveRepository implements BMA.UIDrivers.IModelRepository {

    //    private messagebox: BMA.UIDrivers.IMessageServiсe;

    //    constructor(messagebox: BMA.UIDrivers.IMessageServiсe) {
    //        this.messagebox = messagebox;
    //    }

    //    public IsInRepo(id: string) {
    //        return window.localStorage.getItem(id) !== null;
    //    }

    //    public Save(key: string, appModel: string) {
    //        try {
    //            window.localStorage.setItem(key, appModel);
    //            window.Commands.Execute("LocalStorageChanged", {});
    //        }
    //        catch (e) {
    //            if (e === 'QUOTA_EXCEEDED_ERR') {
    //                this.messagebox.Show("Error: Local repository is full");
    //            }
    //        }
    //    }

    //    public ParseItem(item): boolean {
    //        try {
    //            var ml = JSON.parse(item);
    //            BMA.Model.ImportModelAndLayout(ml);
    //            return true;
    //        }
    //        catch (e) {
    //            return false;
    //        }
    //    }

    //    public SaveModel(id: string, model: JSON) {
    //        if (window.localStorage.getItem(id) !== null) {
    //            if (confirm("Overwrite the file?"))
    //                this.Save("user." + id, JSON.stringify(model));
    //        }
    //        else this.Save("user." + id, JSON.stringify(model));
    //    }

    //    public RemoveModel(id: string) {
    //        window.localStorage.removeItem(id);
    //        window.Commands.Execute("LocalStorageChanged", {});
    //    }

    //    public LoadModel(id: string): JSON {
    //        var model = window.localStorage.getItem(id);
    //        if (model !== null) {
    //            try {
    //                var app = new BMA.Model.AppModel();
    //                app.Deserialize(model);
    //                return JSON.parse(app.Serialize());
    //            }
    //            catch (ex) { alert(ex); }
    //        }
    //        else return null;
    //    }

    //    public GetModelList(): string[] {
    //        var keys = [];
    //        for (var i = 0; i < window.localStorage.length; i++) {
    //            var key = window.localStorage.key(i);
    //            var usrkey = this.IsUserKey(key);
    //            if (usrkey !== undefined) {
    //                var item = window.localStorage.getItem(key);
    //                if (this.ParseItem(item)) {
    //                    keys.push(usrkey);
    //                }
    //            }
    //        }
    //        return keys;
    //    }

    //    private IsUserKey(key: string): string {
    //        var sp = key.split('.');
    //        if (sp[0] === "user") {
    //            var q = sp[1];
    //            for (var i = 2; i < sp.length; i++) {
    //                q = q.concat('.');
    //                q = q.concat(sp[i]);
    //            }
    //            return q;
    //        }
    //        else return undefined;
    //    }
    //}
}