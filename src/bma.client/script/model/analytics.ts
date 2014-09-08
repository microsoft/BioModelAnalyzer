module BMA {
    export module Model {
        export class ProofResult {
            private isStable: boolean;
            private time: number;
            private ticks: any;

            public get IsStable() {
                return this.isStable;
            }

            public get Time() {
                return this.time;
            }

            public get Ticks() {
                return this.ticks;
            }

            constructor(isStable: boolean, time: number, ticks: any) {
                this.isStable = isStable;
                this.time = time;
                this.ticks = ticks;
            }
        }
    }
} 