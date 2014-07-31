module BMA {
    export class GeometryDictionary {
        private geometries: any;

        public GetGeometry(type: string): string {
            if (this.geometries[type] == undefined) {
                return <string>this.geometries[type];
            } else {
                throw "unknown geometry";
            }
        }

        public AddGeometry(type: string, geometry: string) {
            this.geometries[type] = geometry;
        }

        constructor() {
            this.geometries = {};
        }
    }
} 