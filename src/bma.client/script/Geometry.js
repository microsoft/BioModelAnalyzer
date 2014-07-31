var BMA;
(function (BMA) {
    var GeometryDictionary = (function () {
        function GeometryDictionary() {
            this.geometries = {};
        }
        GeometryDictionary.prototype.GetGeometry = function (type) {
            if (this.geometries[type] == undefined) {
                return this.geometries[type];
            } else {
                throw "unknown geometry";
            }
        };

        GeometryDictionary.prototype.AddGeometry = function (type, geometry) {
            this.geometries[type] = geometry;
        };
        return GeometryDictionary;
    })();
    BMA.GeometryDictionary = GeometryDictionary;
})(BMA || (BMA = {}));
//# sourceMappingURL=geometry.js.map
