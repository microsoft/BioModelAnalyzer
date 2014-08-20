/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.drawingsurface", {
        _plot: null,
        _gridLinesPlot: null,
        _svgPlot: null,
        _dragService: null,
        options: {
            isNavigationEnabled: true,
            svg: undefined
        },
        _svgLoaded: function () {
            if (this.options.svg !== undefined && this._svgPlot !== undefined) {
                //this._svgPlot.svg.load("../images/svgtest.txt");
            }
        },
        _create: function () {
            var that = this;

            var plotDiv = $("<div></div>").width(this.element.width()).height(this.element.height()).attr("data-idd-plot", "plot").appendTo(that.element);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(plotDiv);
            var svgPlotDiv = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);

            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this._plot.aspectRatio = 1;
            var svgPlot = that._plot.get(svgPlotDiv[0]);
            this._svgPlot = svgPlot;

            if (this.options.svg !== undefined) {
                if (svgPlot.svg === undefined) {
                    svgPlot.host.on("svgLoaded", this._svgLoaded);
                } else {
                    svgPlot.svg.clear();
                    svgPlot.svg.add(this.options.svg);
                }
            }

            plotDiv.bind("click touchstart", function (arg) {
                if (that.options.isNavigationEnabled !== true) {
                    var cs = svgPlot.getScreenToDataTransform();

                    if (arg.originalEvent !== undefined) {
                        arg = arg.originalEvent;
                    }

                    window.Commands.Execute("DrawingSurfaceClick", {
                        x: cs.screenToDataX(arg.pageX - plotDiv.offset().left),
                        y: -cs.screenToDataY(arg.pageY - plotDiv.offset().top)
                    });
                }
            });

            //Subject that converts input mouse events into Pan gestures
            var createPanSubject = function (vc) {
                var _doc = $(document);

                var mouseDown = vc.onAsObservable("mousedown");
                var mouseMove = vc.onAsObservable("mousemove");
                var mouseUp = _doc.onAsObservable("mouseup");

                var stopPanning = mouseUp;

                var mouseDrags = mouseDown.selectMany(function (md) {
                    var cs = svgPlot.getScreenToDataTransform();
                    var x0 = cs.screenToDataX(md.pageX - plotDiv.offset().left);
                    var y0 = -cs.screenToDataY(md.pageY - plotDiv.offset().top);

                    return mouseMove.select(function (mm) {
                        //var cs = svgPlot.getScreenToDataTransform();
                        var x1 = cs.screenToDataX(mm.pageX - plotDiv.offset().left);
                        var y1 = -cs.screenToDataY(mm.pageY - plotDiv.offset().top);

                        return { x0: x0, y0: y0, x1: x1, y1: y1 };
                    }).takeUntil(stopPanning);
                });

                return mouseDrags;
            };

            this._dragService = {
                dragStart: that._plot.centralPart.onAsObservable("mousedown").select(function (md) {
                    var cs = svgPlot.getScreenToDataTransform();
                    return { x: cs.screenToDataX(md.pageX - plotDiv.offset().left), y: -cs.screenToDataY(md.pageY - plotDiv.offset().top) };
                }),
                drag: createPanSubject(that._plot.centralPart),
                dragEnd: $(document).onAsObservable("mouseup")
            };

            this._gridLinesPlot = that._plot.get(gridLinesPlotDiv[0]);

            var yDT = new InteractiveDataDisplay.DataTransform(function (x) {
                return -x;
            }, function (y) {
                return -y;
            }, undefined);

            this._plot.yDataTransform = yDT;

            //this._gridLinesPlot.yDataTransform = yDT;
            if (this.options.isNavigationEnabled) {
                var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._plot.host);
                that._plot.navigation.gestureSource = gestureSource;
            } else {
                that._plot.navigation.gestureSource = undefined;
            }

            var width = 1600;
            that._plot.navigation.setVisibleRect({ x: 0, y: -50, width: width, height: width / 2.5 }, false);

            $(window).resize(function () {
                that.resize();
            });
            that.resize();
            this.refresh();
        },
        resize: function () {
            if (this._plot !== null && this._plot !== undefined) {
                this._plot.host.width(this.element.width());
                this._plot.host.height(this.element.height());
                this._plot.requestUpdateLayout();
            }
        },
        _setOption: function (key, value) {
            switch (key) {
                case "svg":
                    if (this._svgPlot !== undefined && this._svgPlot.svg !== undefined) {
                        this._svgPlot.svg.clear();
                        if (value !== undefined) {
                            console.log(value);
                            this._svgPlot.svg.add(value);
                        }
                    }
                    break;
                case "isNavigationEnabled":
                    if (value === true) {
                        this._plot.navigation.gestureSource = undefined;
                        var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host);
                        this._plot.navigation.gestureSource = gestureSource;
                    } else {
                        this._plot.navigation.gestureSource = undefined;
                    }
                    break;
                case "grid":
                    if (value !== undefined) {
                        this._gridLinesPlot.x0 = value.x0;
                        this._gridLinesPlot.y0 = value.y0;
                        this._gridLinesPlot.xStep = value.xStep;
                        this._gridLinesPlot.yStep = value.yStep;
                    }
                    break;
            }
            this._super(key, value);
        },
        _setOptions: function (options) {
            this._super(options);
            this.refresh();
        },
        refresh: function () {
        },
        _constrain: function (value) {
            return value;
        },
        destroy: function () {
            this.element.empty();
        },
        getDragSubject: function () {
            return this._dragService;
        }
    });
}(jQuery));
//# sourceMappingURL=drawingsurface.js.map
