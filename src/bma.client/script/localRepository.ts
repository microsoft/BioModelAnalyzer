module BMA {
    export class LocalRepositoryTool implements BMA.UIDrivers.IModelRepository {
        
        private messagebox: BMA.UIDrivers.IMessageServise;

        constructor(messagebox: BMA.UIDrivers.IMessageServise) {
            this.messagebox = messagebox;
        }

        public IsInRepo(id: string) {
            return window.localStorage.getItem(id) !== null;
        }

        public Save(key: string, appModel: string) {
            try {
                window.localStorage.setItem(key, appModel);
                window.Commands.Execute("LocalStorageChanged", {});
            }
            catch (e) {
                if (e === 'QUOTA_EXCEEDED_ERR') {
                    this.messagebox.Show("Error: Local repository is full");
                }
            }
        }

        public ParseItem(item): boolean {
            var ml = JSON.parse(item);

            if (ml === undefined || ml.model === undefined || ml.layout === undefined ||
                ml.model.variables === undefined ||
                ml.layout.variables === undefined ||
                ml.model.variables.length !== ml.layout.variables.length ||
                ml.layout.containers === undefined ||
                ml.model.relationships === undefined) {
                return false;
            }
            else return true;
        }

        public SaveModel(id: string, model: JSON) {
            if (window.localStorage.getItem(id) !== null) {
                if (confirm("Overwrite the file?"))
                    this.Save(id, JSON.stringify(model));
            }
            else this.Save(id, JSON.stringify(model));
        }

        public RemoveModel(id: string) {
            window.localStorage.removeItem(id);
            window.Commands.Execute("LocalStorageChanged", {});
        }

        public LoadModel(id: string): JSON {
            var model = window.localStorage.getItem(id);
            if (model !== null) {
                var app = new BMA.Model.AppModel();
                app.Reset(model);
                return JSON.parse(app.Serialize());
            }
            else return null;
        }

        public GetModelList(): string[] {
            var keys = [];
            for (var i = 0; i < window.localStorage.length; i++) {
                var key = window.localStorage.key(i);
                var item = window.localStorage.getItem(key);
                if (this.ParseItem(item)) {
                    keys.push(key);
                }
            }
            return keys;
        }
    }
}