interface Window {
    KeyframesRegistry: BMA.Keyframes.KeyframesRegistry;
}

module BMA {
    export module Keyframes {
        export class BMAKeyframe {
            private name: string;
            private icon: string;


            public get Name(): string {
                return this.name;
            }

            public get Icon(): string {
                return this.icon;
            }

            constructor(
                name: string,
                icon: string) {

                this.name = name;
                this.icon = icon;
            }
        }

        export class KeyframesRegistry {
            private keyframes: BMAKeyframe[];

            public get Keyframes(): BMAKeyframe[] {
                return this.keyframes;
            }

            public GetFunctionByName(name: string): BMAKeyframe {
                for (var i = 0; i < this.keyframes.length; i++) {
                    if (this.keyframes[i].Name === name)
                        return this.keyframes[i];
                }
                throw "There is no keyframe as you want";
            }

            constructor() {
                var that = this;
                this.keyframes = [];

                this.keyframes.push(new BMAKeyframe("var", "images/ltlimgs/var.png"));
                this.keyframes.push(new BMAKeyframe("num", "images/ltlimgs/123.png"));
                this.keyframes.push(new BMAKeyframe("equal", "images/ltlimgs/eq.png"));
                this.keyframes.push(new BMAKeyframe("more", "images/ltlimgs/mo.png"));
                this.keyframes.push(new BMAKeyframe("less", "images/ltlimgs/le.png"));
                this.keyframes.push(new BMAKeyframe("moeq", "images/ltlimgs/moeq.png"));
                this.keyframes.push(new BMAKeyframe("leeq", "images/ltlimgs/leeq.png"));
            }
        }
    }
}  