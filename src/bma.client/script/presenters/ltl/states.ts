module BMA {
    export module LTL {
        export class StatesPresenter {
            constructor() {
            }

            public GetStateByName(name: string): BMA.LTLOperations.Keyframe {
                return new BMA.LTLOperations.Keyframe(name);
            }
        }
    }
} 