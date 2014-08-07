(function (BMAExt, InteractiveDataDisplay, $, undefined) {

    BMAExt.SVGPlot = function (jqDiv, master) {
        this.base = InteractiveDataDisplay.Plot;
        this.base(jqDiv, master);
        var that = this;

        var _svgCnt = undefined;
        var _svg = undefined;

        var mode = "navigation";
        var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that.host);
        that.navigation.gestureSource = gestureSource;

        Object.defineProperty(this, "mode", {
            get: function () { return mode; },
            set: function (value) {
                if (value == mode) return;
                mode = value;

                if (mode == "navigation") {
                    var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that.host);
                    that.navigation.gestureSource = gestureSource;
                } else {
                    that.navigation.gestureSource = undefined;
                }

            },
            configurable: false
        });

        var _pendingContent = undefined;
        this.add = function (svgContent) {
            if (_svg === undefined) {
                _pendingContent = svgContent
            } else {
                _svg.add(svgContent);
            }
        }

        var svgLoaded = function (svg) {

            _svg = svg;

            svg.configure({
                width: _svgCnt.width(),
                height: _svgCnt.height(),
                viewBox: "0 0 1 1",
                preserveAspectRatio: "none meet"
            }, true);

            if (_pendingContent !== undefined) {
                svg.add(_pendingContent);
            }
        };

        var plotToSvg = function (y, plotRect) {
            return (y - (plotRect.y + plotRect.height / 2)) * (-1) + plotRect.y + plotRect.height / 2;
        }

        this.arrange = function (finalRect) {
            InteractiveDataDisplay.CanvasPlot.prototype.arrange.call(this, finalRect);

            if (_svgCnt === undefined) {
                _svgCnt = $("<div></div>").css("overflow", "hidden").appendTo(that.host);
                _svgCnt.svg({ onLoad: svgLoaded });
            }

            _svgCnt.width(finalRect.width).height(finalRect.height);

            if (_svg !== undefined) {
                var plotRect = that.visibleRect;
                _svg.configure({
                    width: _svgCnt.width(),
                    height: _svgCnt.height()
                }, false);

                if (!isNaN(plotRect.y) && !isNaN(plotRect.height)) {
                    _svg.configure({
                        viewBox: plotRect.x + " " + (-plotRect.y - plotRect.height) + " " + plotRect.width + " " + plotRect.height,
                    }, false);
                }
            }
        };

        this.host.click(function (args) {
            //if (that.mode !== "navigation") {
                var tr = that.getScreenToDataTransform();

                var x = tr.screenToDataX(args.clientX - that.host.offset().left);
                var y = tr.screenToDataY(args.clientY - that.host.offset().top);

                var data = "M 27.3 43.4 l -2.2 -0.8 c -12 -4.4 -19.3 -11.5 -20 -19.7 c 0 -0.5 -0.1 -0.9 -0.1 -1.4 c -5.4 -2.6 -9 -7.3 -10.5 -12.3 c -0.6 -2 -0.9 -4.1 -0.8 -6.3 c -4.7 -1.7 -8.2 -4.7 -10.3 -8.2 c -2.1 -3.4 -3.2 -8.1 -2.1 -13.4 c -6.7 -1.8 -12.5 -4.3 -15.9 -5.8 l -7.4 19.9 l 26.7 7.9 L -17 9.1 l -32.8 -9.7 l 11.9 -32 l 3 1.5 c 3.9 1.9 10.8 4.9 18.1 6.9 c 1.9 -4 5.1 -8.1 10 -12.1 c 10.8 -8.9 19.7 -8.1 23.8 -3.4 c 3.5 4 3.6 11.6 -4.2 18.7 c -6.3 5.7 -16.2 5.7 -25.7 3.8 c -0.6 3.2 -0.2 6.2 1.4 8.9 c 1.3 2.2 3.4 4 6.3 5.3 C -3.4 -8.3 0.7 -13.2 8 -16 c 15.9 -6.1 19.9 0.2 20.7 2.2 c 2.1 5.2 -2.4 11.8 -10.1 15 C 11.5 4.2 5.1 5 -0.3 4.4 C -0.2 5.5 0 6.5 0.3 7.5 c 0.9 3.2 3 6.1 6.2 8 C 8 12.1 11 9 15 6.7 C 25 1 32.2 1.6 35.7 4.2 c 2.3 1.7 3.3 4.3 2.7 7.1 c -1.1 5.3 -7.6 9.7 -17.5 11.8 c -3.6 0.8 -6.8 0.8 -9.7 0.4 c 1 4.9 6 9.5 13.9 12.8 l 7.4 -10.7 l 17.4 10.1 l -3 5.1 l -12.6 -7.4 L 27.3 43.4 L 27.3 43.4 Z M 12.1 17.5 c 2.2 0.3 4.8 0.3 7.6 -0.3 c 9.4 -2 12.6 -5.6 12.9 -7.2 c 0.1 -0.4 0 -0.7 -0.4 -1 c -1.4 -1 -6.2 -1.7 -14.1 2.9 C 15.2 13.4 13.2 15.4 12.1 17.5 L 12.1 17.5 Z M 0.6 -1.5 C 5 -1 10.3 -1.7 16.3 -4.2 c 5.4 -2.3 7.4 -6 6.9 -7.3 c -0.4 -1 -4.3 -2.2 -13 1.1 C 5 -8.5 2 -5.1 0.6 -1.5 L 0.6 -1.5 Z M -10.8 -22.8 c 7.8 1.4 15.2 1.3 19.5 -2.6 c 4.7 -4.2 5.4 -8.4 3.7 -10.4 c -2.1 -2.5 -8.3 -1.9 -15.5 4.1 C -6.5 -28.9 -9.1 -25.9 -10.8 -22.8 L -10.8 -22.8 Z";
                var path = _svg.createPath();
                _svg.path(path, { stroke: 'transparent', fill: "#ef4137", strokeWidth: 8.3333, d: data, transform: "translate(" + x + ", " + -y + ") scale(0.36)" });
            //} 
        });

        // Gets the transform functions from data to screen coordinates.
        // Returns { dataToScreenX, dataToScreenY }
        this.getTransform = function () {
            var ct = this.coordinateTransform;
            var plotToScreenX = ct.plotToScreenX;
            var plotToScreenY = ct.plotToScreenY;
            var dataToPlotX = this.xDataTransform && this.xDataTransform.dataToPlot;
            var dataToPlotY = this.yDataTransform && this.yDataTransform.dataToPlot;
            var dataToScreenX = dataToPlotX ? function (x) { return plotToScreenX(dataToPlotX(x)); } : plotToScreenX;
            var dataToScreenY = dataToPlotY ? function (y) { return plotToScreenY(dataToPlotY(y)); } : plotToScreenY;

            return { dataToScreenX: dataToScreenX, dataToScreenY: dataToScreenY };
        };

        // Gets the transform functions from screen to data coordinates.
        // Returns { screenToDataX, screenToDataY }
        this.getScreenToDataTransform = function () {
            var ct = this.coordinateTransform;
            var screenToPlotX = ct.screenToPlotX;
            var screenToPlotY = ct.screenToPlotY;
            var plotToDataX = this.xDataTransform && this.xDataTransform.plotToData;
            var plotToDataY = this.yDataTransform && this.yDataTransform.plotToData;
            var screenToDataX = plotToDataX ? function (x) { return plotToDataX(screenToPlotX(x)); } : screenToPlotX;
            var screenToDataY = plotToDataY ? function (y) { return plotToDataY(screenToPlotY(y)); } : screenToPlotY;

            return { screenToDataX: screenToDataX, screenToDataY: screenToDataY };
        };
    };
    BMAExt.SVGPlot.prototype = new InteractiveDataDisplay.Plot;
    InteractiveDataDisplay.register('svgPlot', function (jqDiv, master) { return new BMAExt.SVGPlot(jqDiv, master); });

})(window.BMAExt = window.BMAExt || {}, InteractiveDataDisplay || {}, jQuery);