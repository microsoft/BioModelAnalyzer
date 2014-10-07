var BMA;
(function (BMA) {
    (function (Model) {
        var AppVisualSettings = (function () {
            function AppVisualSettings() {
                this.lineWidth = 10;
                this.textLabelSize = 10;
                this.gridVisibility = true;
                this.textLabelVisibility = true;
                this.iconsVisibility = true;
                this.iconsSize = 10;
            }
            Object.defineProperty(AppVisualSettings.prototype, "LineWidth", {
                get: function () {
                    return this.lineWidth;
                },
                set: function (lineWidth) {
                    this.lineWidth = lineWidth;
                    window.Commands.Execute("AppCommands.ChangeLineWidth", this.lineWidth);
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(AppVisualSettings.prototype, "TextLabelSize", {
                get: function () {
                    return this.textLabelSize;
                },
                set: function (textLabelSize) {
                    this.textLabelSize = textLabelSize;
                    window.Commands.Execute("AppCommands.ChangeTextLabelSize", this.textLabelSize);
                },
                enumerable: true,
                configurable: true
            });


            Object.defineProperty(AppVisualSettings.prototype, "GridVisibility", {
                get: function () {
                    return this.gridVisibility;
                },
                set: function (gridVisibility) {
                    this.gridVisibility = gridVisibility;
                    window.Commands.Execute("AppCommands.ToggleGridVisibility", this.gridVisibility);
                },
                enumerable: true,
                configurable: true
            });


            Object.defineProperty(AppVisualSettings.prototype, "TextLabelVisibility", {
                get: function () {
                    return this.textLabelVisibility;
                },
                set: function (textLabelVisibility) {
                    this.textLabelVisibility = textLabelVisibility;
                    window.Commands.Execute("AppCommands.ToggleTextLabelVisibility", this.textLabelVisibility);
                },
                enumerable: true,
                configurable: true
            });


            Object.defineProperty(AppVisualSettings.prototype, "IconsVisibility", {
                get: function () {
                    return this.iconsVisibility;
                },
                set: function (iconsVisibility) {
                    this.iconsVisibility = iconsVisibility;
                    window.Commands.Execute("AppCommands.ToggleIconsVisibility", this.iconsVisibility);
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(AppVisualSettings.prototype, "IconsSize", {
                get: function () {
                    return this.iconsSize;
                },
                set: function (iconsSize) {
                    this.iconsSize = iconsSize;
                    window.Commands.Execute("AppCommands.ChangeIconsSize", this.iconsSize);
                },
                enumerable: true,
                configurable: true
            });
            return AppVisualSettings;
        })();
        Model.AppVisualSettings = AppVisualSettings;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
