/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

interface JQueryStatic {
    cookie(name: string): any;
    cookie(name: string, value: any, options: any): any;
    cookie(name: string, value: any): any;
}

module BMA {
    export interface ISessionLog {
        LogProofRun();
        LogSimulationRun();
        LogFurtherTestingRun();

        LogProofError();
        LogSimulationError();
        LogFurtherTestingError();


        LogNewModelCreated();
        LogImportModel();
        LogSaveModel();
    }

    function generateUUID() {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    };

    export class SessionLog implements ISessionLog {
        private userId: string;
        private sessionId: string;

        private logIn: Date;
        private logOut: Date;

        private proofCount: number;
        private simulationCount: number;
        private furtherTestingCount: number;
        private newModelCount: number;
        private saveModelCount: number;
        private importModelCount: number;
        private proofErrorCount: number;
        private simulationErrorCount: number;
        private furtherTestingErrorCount: number;

        constructor() {
            this.userId = $.cookie("BMAClient.UserID");
            if (this.userId === undefined) 
                this.userId = generateUUID();
            $.cookie("BMAClient.UserID", this.userId, { expires: 365 * 10 }); // Set cookie with persistent user ID that will last for 10 years from now

            this.sessionId = generateUUID();

            this.logIn = new Date();
            this.logOut = new Date();

            this.proofCount = 0;
            this.simulationCount = 0;
            this.furtherTestingCount = 0;
            this.newModelCount = 0;
            this.saveModelCount = 0;
            this.importModelCount = 0;
            this.proofErrorCount = this.furtherTestingErrorCount = this.simulationErrorCount = 0;
        }

        public LogProofError() {
            this.proofErrorCount++;
        }

        public LogSimulationError() {
            this.simulationErrorCount++;
        }

        public LogFurtherTestingError() {
            this.furtherTestingErrorCount++;
        }

        public LogProofRun() {
            this.proofCount++;
        }

        public LogSimulationRun() {
            this.simulationCount++;
        }

        public LogFurtherTestingRun() {
            this.furtherTestingCount++;
        }

        public LogNewModelCreated() {
            this.newModelCount++;
        }

        public LogImportModel() {
            this.importModelCount++;
        }

        public LogSaveModel() {
            this.saveModelCount++;
        }

        public CloseSession() {
            this.logOut = new Date();
            return {
                Proof: this.proofCount,
                Simulation: this.simulationCount,
                FurtherTesting: this.furtherTestingCount,
                NewModel: this.newModelCount,
                ImportModel: this.importModelCount,
                SaveModel: this.saveModelCount,
                LogIn: this.logIn.toJSON(),
                LogOut: this.logOut.toJSON(),
                UserID: this.userId,
                SessionID: this.sessionId,
                ProofErrorCount: this.proofErrorCount,
                SimulationErrorCount: this.simulationErrorCount,
                FurtherTestingErrorCount: this.furtherTestingErrorCount
            };
        }
    }
} 