module BMA {
    export module Model {
        export class AppVisualSettings {
            private lineWidth: number;
            private textLabelSize: number;
            private gridVisibility: boolean;
            private textLabelVisibility: boolean;
            private iconsVisibility: boolean;
            private iconsSize: number;

            constructor() {
                this.lineWidth = 10;
                this.textLabelSize = 10;
                this.gridVisibility = true;
                this.textLabelVisibility = true;
                this.iconsVisibility = true;
                this.iconsSize = 10;
            }

            public get LineWidth(): number {
                return this.lineWidth;
            }
            public set LineWidth(lineWidth: number) {
                this.lineWidth = lineWidth;
            }

            public get TextLabelSize(): number {
                return this.textLabelSize;
            }

            public set TextLabelSize(textLabelSize: number) {
                this.textLabelSize = textLabelSize;
            }

            public get GridVisibility(): boolean {
                return this.gridVisibility;
            }

            public set GridVisibility(gridVisibility: boolean) {
                this.gridVisibility = gridVisibility;
            }

            public get TextLabelVisibility(): boolean {
                return this.textLabelVisibility;
            }

            public set TextLabelVisibility(textLabelVisibility: boolean) {
                this.textLabelVisibility = textLabelVisibility;
            }

            public get IconsVisibility(): boolean {
                return this.iconsVisibility;
            }
            public set IconsVisibility(iconsVisibility: boolean) {
                this.iconsVisibility = iconsVisibility;
            }

            public get IconsSize(): number {
                return this.iconsSize;
            }
            public set IconsSize(iconsSize: number) {
                this.iconsSize = iconsSize;
            }

        }
    }
} 