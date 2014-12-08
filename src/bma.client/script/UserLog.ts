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
        LogNewModelCreated();
        LogImportModel();
        LogSaveModel();
    }

    export class SessionLog implements ISessionLog {
        private userId: string;

        private logIn: Date;
        private logOut: Date;
        private duration: number;

        private proofCount: number;
        private simulationCount: number;
        private furtherTestingCount: number;
        private newModelCount: number;
        private saveModelCount: number;
        private importModelCount: number;

        constructor() {
            this.userId = $.cookie("userID");
            if (this.userId === undefined) {

            }

            this.logIn = new Date();
            this.logOut = new Date();
            this.duration = 0;

            this.proofCount = 0;
            this.simulationCount = 0;
            this.furtherTestingCount = 0;
            this.newModelCount = 0;
            this.saveModelCount = 0;
            this.importModelCount = 0;
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
            this.duration = Math.abs(this.logOut.getSeconds() - this.logIn.getSeconds());

            return {
                Proof: this.proofCount,
                Simulation: this.simulationCount,
                FurtherTesting: this.furtherTestingCount,
                NewModel: this.newModelCount,
                ImportModel: this.importModelCount,
                SaveModel: this.saveModelCount,
                LogIn: this.logIn.toJSON(),
                LogOut: this.logOut.toJSON(),
                Duration: this.duration
            };
        }
    }
} 