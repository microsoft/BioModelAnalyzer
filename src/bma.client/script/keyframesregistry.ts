interface Window {
    KeyframesRegistry: BMA.Keyframes.KeyframesRegistry;
}

module BMA {
    export module Keyframes {
        export class BMAKeyframe {
            private name: string;
            private icon: string;
            private toolType: string;


            public get Name(): string {
                return this.name;
            }

            public get Icon(): string {
                return this.icon;
            }

            public get ToolType(): string {
                return this.toolType;
            }

            constructor(
                name: string,
                icon: string,
                toolType: string) {

                this.name = name;
                this.icon = icon;
                this.toolType = toolType;
            }
        }

        export class KeyframesRegistry {
            private keyframes: BMAKeyframe[];
            private imagePath: string = "images";

            public get Keyframes(): BMAKeyframe[] {
                return this.keyframes;
            }

            public get ImagePath(): string {
                return this.imagePath;
            }

            public set ImagePath(value) {
                this.imagePath = value;
            }

            public GetFunctionByName(name: string): BMAKeyframe {
                for (var i = 0; i < this.keyframes.length; i++) {
                    if (this.keyframes[i].Name === name)
                        return this.keyframes[i];
                }
                throw "There is no keyframe as you want";
            }

            constructor(imagePath:string = "images") {
                var that = this;
                this.keyframes = [];
                this.imagePath = imagePath;

                this.keyframes.push(new BMAKeyframe("var", this.imagePath + "/ltlimgs/var.png", "variable"));
                this.keyframes.push(new BMAKeyframe("num", this.imagePath + "/ltlimgs/123.png", "const"));
                this.keyframes.push(new BMAKeyframe("equal",this.imagePath + "/ltlimgs/eq.png", "operator"));
                this.keyframes.push(new BMAKeyframe("more", this.imagePath + "/ltlimgs/mo.png", "operator"));
                this.keyframes.push(new BMAKeyframe("less", this.imagePath + "/ltlimgs/le.png", "operator"));
                this.keyframes.push(new BMAKeyframe("moeq", this.imagePath + "/ltlimgs/moeq.png", "operator"));
                this.keyframes.push(new BMAKeyframe("leeq", this.imagePath + "/ltlimgs/leeq.png", "operator"));
            }                                                   
        }
    }
}  