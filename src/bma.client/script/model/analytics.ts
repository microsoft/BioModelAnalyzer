module BMA {
    export module Model {
        export class ProofResult {
            private isStable: boolean;
            private time: number;

            public get IsStable() {
                return this.isStable;
            }

            public get Time() {
                return this.time;
            }

            constructor(isStable: boolean, time: number) {
                this.isStable = isStable;
                this.time = time;
            }
        }
    }
} 