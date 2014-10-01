/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
declare var BMAExt: any;
declare var InteractiveDataDisplay: any;
declare var Rx: any;

(function ($) {
    $.widget("BMA.drawingsurface", {
        _plot: null,
        _gridLinesPlot: null,
        _svgPlot: null,
        _dragService: null,
        _zoomObservable: undefined,
        _zoomObs: undefined,

        options: {
            isNavigationEnabled: true,
            svg: undefined,
            zoom: 50
        },


        _svgLoaded: function () {
            if (this.options.svg !== undefined && this._svgPlot !== undefined) {
                //this._svgPlot.svg.load("../images/svgtest.txt");
            }
        },

        _create: function () {
            var that = this;

            this._zoomObs = undefined;
            this._zoomObservable = Rx.Observable.create(function (rx) {
                that._zoomObs = rx;
            });

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

            plotDiv.droppable({
                drop: function (event, ui) {

                    if (that.options.isNavigationEnabled !== true) {
                        var cs = svgPlot.getScreenToDataTransform();

                        window.Commands.Execute("DrawingSurfaceClick",
                            {
                                x: cs.screenToDataX(event.pageX - plotDiv.offset().left),
                                y: -cs.screenToDataY(event.pageY - plotDiv.offset().top)
                            });
                    }
                }
            });

            plotDiv.bind("click touchstart", function (arg) {
                var cs = svgPlot.getScreenToDataTransform();

                if (arg.originalEvent !== undefined) {
                    arg = arg.originalEvent;
                }

                window.Commands.Execute("DrawingSurfaceClick",
                    {
                        x: cs.screenToDataX(arg.pageX - plotDiv.offset().left),
                        y: -cs.screenToDataY(arg.pageY - plotDiv.offset().top)
                    });
            });

            plotDiv.dblclick(function (arg) {
                var cs = svgPlot.getScreenToDataTransform();

                if (arg.originalEvent !== undefined) {
                    arg = arg.originalEvent;
                }

                window.Commands.Execute("DrawingSurfaceDoubleClick",
                    {
                        x: cs.screenToDataX(arg.pageX - plotDiv.offset().left),
                        y: -cs.screenToDataY(arg.pageY - plotDiv.offset().top)
                    });
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


                var touchStart = vc.onAsObservable("touchstart");
                var touchMove = vc.onAsObservable("touchmove");
                var touchEnd = _doc.onAsObservable("touchend");
                var touchCancel = _doc.onAsObservable("touchcancel");

                var gestures = touchStart.selectMany(function (md) {
                    var cs = svgPlot.getScreenToDataTransform();
                    var x0 = cs.screenToDataX(md.originalEvent.pageX - plotDiv.offset().left);
                    var y0 = -cs.screenToDataY(md.originalEvent.pageY - plotDiv.offset().top);

                    return touchMove.takeUntil(touchEnd.merge(touchCancel)).select(function (tm) {
                        var x1 = cs.screenToDataX(tm.originalEvent.pageX - plotDiv.offset().left);
                        var y1 = -cs.screenToDataY(tm.originalEvent.pageY - plotDiv.offset().top);

                        return { x0: x0, y0: y0, x1: x1, y1: y1 };
                    });
                });

                return mouseDrags.merge(gestures);
            }

            var createDragStartSubject = function (vc) {
                var _doc = $(document);
                var mousedown = vc.onAsObservable("mousedown");
                var mouseMove = vc.onAsObservable("mousemove");
                var mouseUp = _doc.onAsObservable("mouseup");

                var stopPanning = mouseUp;

                var dragStarts = mousedown.selectMany(function (md) {
                    var cs = svgPlot.getScreenToDataTransform();
                    var x0 = cs.screenToDataX(md.pageX - plotDiv.offset().left);
                    var y0 = -cs.screenToDataY(md.pageY - plotDiv.offset().top);

                    return mouseMove.select(function (mm) { return { x: x0, y: y0 }; }).first().takeUntil(mouseUp);
                });


                var touchStart = vc.onAsObservable("touchstart");
                var touchMove = vc.onAsObservable("touchmove");
                var touchEnd = _doc.onAsObservable("touchend");
                var touchCancel = _doc.onAsObservable("touchcancel");

                var touchDragStarts = touchStart.selectMany(function (md) {
                    var cs = svgPlot.getScreenToDataTransform();
                    var x0 = cs.screenToDataX(md.originalEvent.pageX - plotDiv.offset().left);
                    var y0 = -cs.screenToDataY(md.originalEvent.pageY - plotDiv.offset().top);

                    return touchMove.select(function (mm) { return { x: x0, y: y0 }; }).first().takeUntil(touchEnd.merge(touchCancel));
                });

                return dragStarts;
            }

            var createDragEndSubject = function (vc) {
                var _doc = $(document);
                var mousedown = that._plot.centralPart.onAsObservable("mousedown");
                var mouseMove = vc.onAsObservable("mousemove");
                var mouseUp = _doc.onAsObservable("mouseup");

                var touchEnd = _doc.onAsObservable("touchend");
                var touchCancel = _doc.onAsObservable("touchcancel");

                var stopPanning = mouseUp.merge(touchEnd).merge(touchCancel);

                var dragEndings = stopPanning;//.takeWhile(mouseMove);

                return dragEndings;
            }

            this._dragService = {
                dragStart: createDragStartSubject(that._plot.centralPart),
                drag: createPanSubject(that._plot.centralPart),
                dragEnd: createDragEndSubject(that._plot.centralPart)
            };


            this._gridLinesPlot = that._plot.get(gridLinesPlotDiv[0]);

            var yDT = new InteractiveDataDisplay.DataTransform(
                function (x) {
                    return -x;
                },
                function (y) {
                    return -y;
                },
                undefined);

            this._plot.yDataTransform = yDT;
            //this._gridLinesPlot.yDataTransform = yDT;

            var width = 1600;

            if (this.options.isNavigationEnabled) {
                var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._plot.host);
                that._plot.navigation.gestureSource = gestureSource.merge(this._zoomObservable).where(function (g) {
                    console.log(g.scaleFactor + "   " + that._plot.visibleRect.width + "   ");
                    return g.Type !== "Zoom" || g.scaleFactor > 1 && that._plot.visibleRect.width < 2665 || g.scaleFactor < 1 && that._plot.visibleRect.width > 923;
                });
                this._zoomService = gestureSource.where(function (g) { return g.Type === "Zoom"; });
            } else {
                that._plot.navigation.gestureSource = this._zoomObservable;
            }

            that._plot.navigation.setVisibleRect({ x: 0, y: -50, width: width, height: width / 2.5 }, false);

            $(window).resize(function () { that.resize(); });
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
                            this._svgPlot.svg.add(value);
                        }
                    }
                    break;
                case "isNavigationEnabled":
                    if (value === true) {
                        if (this._plot.navigation.gestureSource === undefined) {
                            var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host);

                            if (this._zoomObservable !== undefined) {
                                gestureSource = gestureSource.merge(this._zoomObservable);
                            }

                            this._zoomService = gestureSource.where(function (g) { return g.Type === "Zoom"; });

                            this._plot.navigation.gestureSource = gestureSource;
                        }
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
                        this._plot.requestUpdateLayout();
                    }
                    break;
                case "zoom":
                    if (value !== undefined) {
                        console.log(value);
                        var currentZoom = this._getZoom();
                        var zoom = Math.pow(currentZoom, (value - this.options.zoom) / 10);
                        this._zoomObs.onNext(new InteractiveDataDisplay.Gestures.ZoomGesture(this._gridLinesPlot.centralPart.width() / 2, this._gridLinesPlot.centralPart.height() / 2, zoom, "Mouse"));
                        this.options.zoom = value;
                        //alert(this._plot.visibleRect.width);
                    }
                    break;
                case "gridVisibility":
                    this._gridLinesPlot.isVisible = value;
                    this._plot.requestUpdateLayout();
                    break;
            }
            this._super(key, value);
        },

        _getZoom: function () {
            //var plotRect = this._plot.visibleRect;
            ////console.log(plotRect.width);
            //return 0;
            if (this._gridLinesPlot.mapControl === undefined)

                return InteractiveDataDisplay.Gestures.zoomLevelFactor;

            else

                return 3.0;
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
        },

        getPlotX: function (left: number) {
            var cs = this._svgPlot.getScreenToDataTransform();
            return cs.screenToDataX(left);
        },

        getPlotY: function (top: number) {
            var cs = this._svgPlot.getScreenToDataTransform();
            return -cs.screenToDataY(top);
        },

        getPixelWidth: function () {
            var cs = this._svgPlot.getScreenToDataTransform();
            return cs.screenToDataX(1) - cs.screenToDataX(0);
        },

        getZoomSubject: function () {
            return this._zoomService;
        }

    });
} (jQuery));

interface JQuery {
    drawingsurface(): JQueryUI.Widget;
    drawingsurface(settings: Object): JQueryUI.Widget;
    drawingsurface(methodName: string, arg: any): JQueryUI.Widget;
}
