(function (BMA, InteractiveDataDisplay, $, undefined) {

    BMA.GridLinesPlot = function (jqDiv, master) {
        this.base = InteractiveDataDisplay.CanvasPlot;
        this.base(jqDiv, master);

        var _x0 = 0, _xstep = 1, _y0 = 0, _ystep = 1;
        var _thickness = "1px";
        var _stroke = "LightGray";

        var style = {};
        InteractiveDataDisplay.Utils.readStyle(this.host, style);
        if (style) {
            _stroke = typeof style.stroke != "undefined" ? style.stroke : _stroke;
            _thickness = typeof style.thickness != "undefined" ? style.thickness : _thickness;
        }

        Object.defineProperty(this, "x0", {
            get: function () { return _x0; },
            set: function (value) {
                if (value == _x0) return;
                _x0 = value;
                this.requestUpdateLayout();
            },
            configurable: false
        });

        Object.defineProperty(this, "xStep", {
            get: function () { return _xstep; },
            set: function (value) {
                if (value == _xstep) return;

                if (_xstep <= 0) throw "xStep show be positive";

                _xstep = value;
                this.requestUpdateLayout();
            },
            configurable: false
        });

        Object.defineProperty(this, "y0", {
            get: function () { return _y0; },
            set: function (value) {
                if (value == _y0) return;
                _y0 = value;
                this.requestUpdateLayout();
            },
            configurable: false
        });

        Object.defineProperty(this, "yStep", {
            get: function () { return _ystep; },
            set: function (value) {
                if (value == _ystep) return;

                if (_ystep <= 0) throw "yStep show be positive";

                _ystep = value;
                this.requestUpdateLayout();
            },
            configurable: false
        });

        Object.defineProperty(this, "thickness", {
            get: function () { return _thickness; },
            set: function (value) {
                if (value == _thickness) return;
                if (value <= 0) throw "GridLines thickness must be positive";
                _thickness = value;

                this.requestNextFrameOrUpdate();
            },
            configurable: false
        });

        Object.defineProperty(this, "stroke", {
            get: function () { return _stroke; },
            set: function (value) {
                if (value == _stroke) return;
                _stroke = value;

                this.requestNextFrameOrUpdate();
            },
            configurable: false
        });

        this.renderCore = function (plotRect, screenSize) {
            InteractiveDataDisplay.GridlinesPlot.prototype.renderCore.call(this, plotRect, screenSize);

            var transform = this.getTransform();
            var ctx = this.getContext(true);
            ctx.strokeStyle = _stroke;
            ctx.fillStyle = _stroke;
            ctx.lineWidth = 1;

            var strokeThickness = parseInt(_thickness.slice(0, -2));

            var ticks = [];
            var xmin = Math.ceil((plotRect.x - _x0) / _xstep) * _xstep + _x0;
            var xmax = (Math.ceil((plotRect.x + plotRect.width - _x0) / _xstep) - 1) * _xstep + _x0;
            var index = xmin;
            while (index <= xmax) {
                ticks.push(index);
                index += _xstep;
            }

            var v;
            for (var i = 0, len = ticks.length; i < len; i++) {
                v = transform.dataToScreenX(ticks[i]);
                ctx.fillRect(v, 0, strokeThickness, screenSize.height);
            }

            ticks = [];
            var ymin = Math.ceil((plotRect.y - _y0) / _ystep) * _ystep + _y0;
            var ymax = (Math.ceil((plotRect.y + plotRect.height - _y0) / _ystep) - 1) * _ystep + _y0;
            index = ymin;
            while (index <= ymax) {
                ticks.push(index);
                index += _ystep;
            }

            v;
            for (var i = 0, len = ticks.length; i < len; i++) {
                v = transform.dataToScreenY(ticks[i]);
                ctx.fillRect(0, v, screenSize.width, strokeThickness);
            }
        };

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
    }
    BMA.GridLinesPlot.prototype = new InteractiveDataDisplay.CanvasPlot;
    InteractiveDataDisplay.register('scalableGridLines', function (jqDiv, master) { return new BMA.GridLinesPlot(jqDiv, master); });

})(window.BMA = window.BMA || {}, InteractiveDataDisplay || {}, jQuery);