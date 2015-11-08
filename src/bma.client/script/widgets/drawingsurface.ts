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
        _lightSvgPlot: null,
        _rectsPlot: null,
        _dragService: null,
        //_zoomObservable: undefined,
        _zoomObs: undefined,
        _onlyZoomEnabled: false,
        _mouseMoves: null,
        _domPlot: null,

        options: {
            isNavigationEnabled: true,
            svg: undefined,
            zoom: 50,
            dropFilter: ["drawingsurface-droppable"],
            useContraints: true
        },

        _plotSettings: {
            MinWidth: 0.01,
            MaxWidth: 1e5
        },

        _checkDropFilter: function (ui) {
            if (this.options !== undefined && this.options.dropFilter !== undefined) {
                var classes = this.options.dropFilter;
                for (var i = 0; i < classes.length; i++) {
                    if (ui.hasClass(classes[i]))
                        return true;
                }
            }

            return false;
        },


        _svgLoaded: function () {
            if (this.options.svg !== undefined && this._svgPlot !== undefined) {
                this._svgPlot.svg.clear();
                this._svgPlot.svg.add(this.options.svg);
            }
        },

        _lightSvgLoaded: function () {
            if (this.options.lightSvg !== undefined && this._lightSvgPlot !== undefined) {
                this._lightSvgPlot.svg.configure({ "pointer-events": "none" }, false);
                this._lightSvgPlot.svg.clear();
                this._lightSvgPlot.svg.add(this.options.svg);
            }
        },

        _create: function () {
            var that = this;

            if (window.PlotSettings !== undefined) {
                this._plotSettings = window.PlotSettings;
            }

            //this._zoomObs = undefined;
            //this._zoomObservable = Rx.Observable.create(function (rx) {
            //    that._zoomObs = rx;
            //});

            var plotDiv = $("<div></div>").width(this.element.width()).height(this.element.height()).attr("data-idd-plot", "plot").appendTo(that.element);
            var gridLinesPlotDiv = $("<div></div>").attr("data-idd-plot", "scalableGridLines").appendTo(plotDiv);
            var rectsPlotDiv = $("<div></div>").attr("data-idd-plot", "rectsPlot").appendTo(plotDiv);
            var svgPlotDiv = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);
            var domPlotDiv = $("<div></div>").attr("data-idd-plot", "dom").appendTo(plotDiv);

            var svgPlotDiv2 = $("<div></div>").attr("data-idd-plot", "svgPlot").appendTo(plotDiv);
            this.lightSVGDiv = svgPlotDiv2;

            //empty div for event handling
            //$("<div></div>").attr("data-idd-plot", "plot").appendTo(plotDiv);

            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            this._plot.aspectRatio = 1;

            var svgPlot = that._plot.get(svgPlotDiv[0]);
            this._svgPlot = svgPlot;

            var lightSvgPlot = that._plot.get(svgPlotDiv2[0]);
            this._lightSvgPlot = lightSvgPlot;

            this._domPlot = that._plot.get(domPlotDiv[0]);

            this._rectsPlot = that._plot.get(rectsPlotDiv[0]);
            //rectsPlot.draw({ rects: [{ x: 0, y: 0, width: 500, height: 500, fill: "red" }] })

            if (this.options.svg !== undefined) {
                if (svgPlot.svg === undefined) {
                    svgPlot.host.on("svgLoaded", this._svgLoaded);
                } else {
                    svgPlot.svg.clear();
                    svgPlot.svg.add(this.options.svg);
                }
            }

            if (lightSvgPlot.svg === undefined) {
                lightSvgPlot.host.on("svgLoaded", this._lightSvgLoaded);
            } else {
                //lightSvgPlot.svg.configure({ style: "pointer-events:none;" }, false);
                lightSvgPlot.svg.clear();
                if (this.options.lightSvg !== undefined)
                    lightSvgPlot.svg.add(this.options.lightSvg);
            }

            plotDiv.droppable({
                drop: function (event, ui) {
                    event.stopPropagation();
                    if (!that._checkDropFilter(ui.draggable))
                        return;

                    var cs = svgPlot.getScreenToDataTransform();
                    var position = {
                        x: cs.screenToDataX(event.pageX - plotDiv.offset().left),
                        y: -cs.screenToDataY(event.pageY - plotDiv.offset().top)
                    };
                    if (that.options.isNavigationEnabled !== true) {
                        that._executeCommand("DrawingSurfaceClick", position);
                    }

                    that._executeCommand("DrawingSurfaceDrop", position);

                }
            });

            plotDiv.bind("click touchstart", function (arg) {
                var cs = svgPlot.getScreenToDataTransform();

                if (arg.originalEvent !== undefined) {
                    arg = arg.originalEvent;
                }

                arg.stopPropagation();

                that._executeCommand("DrawingSurfaceClick",
                    {
                        x: cs.screenToDataX(arg.pageX - plotDiv.offset().left),
                        y: -cs.screenToDataY(arg.pageY - plotDiv.offset().top),
                        screenX: arg.pageX - plotDiv.offset().left,
                        screenY: arg.pageY - plotDiv.offset().top
                    });
            });


            plotDiv.mousedown(function (e) {
                e.stopPropagation();
            });

            plotDiv.dblclick(function (arg) {
                var cs = svgPlot.getScreenToDataTransform();

                if (arg.originalEvent !== undefined) {
                    arg = arg.originalEvent;
                }

                that._executeCommand("DrawingSurfaceDoubleClick",
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
                var mousedown = vc.onAsObservable("mousedown").where(function (md) {
                    return md.button === 0;
                });
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

            this._mouseMoves = that._plot.centralPart.onAsObservable("mousemove").select(function (mm) {

                var cs = svgPlot.getScreenToDataTransform();
                var x0 = cs.screenToDataX(mm.originalEvent.pageX - plotDiv.offset().left);
                var y0 = -cs.screenToDataY(mm.originalEvent.pageY - plotDiv.offset().top);

                return {
                    x: x0,
                    y: y0
                };
            });


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

            /*
            this._domPlot.yDataTransform = new InteractiveDataDisplay.DataTransform(
                function (x) {
                    return x;
                },
                function (y) {
                    return y;
                },
                undefined);
            */

            var width = 1600;
            that.options.zoom = width;

            if (this.options.isNavigationEnabled) {
                this._setGestureSource(this._onlyZoomEnabled);
                //var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(that._plot.host).where(function (g) {
                //    return g.Type !== "Zoom" || g.scaleFactor > 1 && that._plot.visibleRect.width < that._plotSettings.MaxWidth || g.scaleFactor < 1 && that._plot.visibleRect.width > that._plotSettings.MinWidth;
                //});
                //that._plot.navigation.gestureSource = gestureSource;
            } else {
                that._plot.navigation.gestureSource = undefined;
            }

            that._plot.navigation.setVisibleRect({ x: 0, y: -50, width: width, height: width / 2.5 }, false);
            that._plot.host.bind("visibleRectChanged", function (args) {
                if (Math.round(that._plot.visibleRect.width) !== that.options.zoom) {
                    that._executeCommand("VisibleRectChanged", that._plot.visibleRect.width);
                }
            })

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

        _executeCommand: function (commandName, args) {
            if (this.options.commands !== undefined) {
                this.options.commands.Execute(commandName, args);
            } else {
                window.Commands.Execute(commandName, args);
            }
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                case "svg":
                    if (this._svgPlot !== undefined && this._svgPlot.svg !== undefined) {
                        this._svgPlot.svg.clear();
                        if (value !== undefined) {
                            this._svgPlot.svg.add(value);
                        }
                    }
                    break;
                case "lightSvg":
                    if (this._lightSvgPlot !== undefined && this._lightSvgPlot.svg !== undefined) {
                        this._lightSvgPlot.svg.clear();
                        if (value !== undefined) {
                            this._lightSvgPlot.svg.add(value);
                        }
                    }
                    break;
                case "isNavigationEnabled":
                    if (value === true) {
                        if (this._onlyZoomEnabled === true) {
                            this._setGestureSource(false);
                            //var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host).where(function (g) {
                            //    return g.Type !== "Zoom" || g.scaleFactor > 1 && that._plot.visibleRect.width < that._plotSettings.MaxWidth || g.scaleFactor < 1 && that._plot.visibleRect.width > that._plotSettings.MinWidth;
                            //});
                            //this._plot.navigation.gestureSource = gestureSource;
                            this._onlyZoomEnabled = false;
                        }
                    } else {
                        this._setGestureSource(true);
                        //var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host).where(function (g) {
                        //    return g.Type === "Zoom" && (g.scaleFactor > 1 && that._plot.visibleRect.width < that._plotSettings.MaxWidth || g.scaleFactor < 1 && that._plot.visibleRect.width > that._plotSettings.MinWidth);
                        //});
                        //this._plot.navigation.gestureSource = gestureSource;
                        this._onlyZoomEnabled = true;
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
                        if (that._plot.visibleRect.width !== value) {

                            var oldPlotRect = that._plot.visibleRect;
                            var xCenter = oldPlotRect.x + oldPlotRect.width / 2;
                            var yCenter = oldPlotRect.y + oldPlotRect.height / 2;
                            var scale = oldPlotRect.width / value;
                            var newHeight = oldPlotRect.height / scale;
                            var newrect = {
                                x: xCenter - value / 2,
                                y: yCenter - newHeight / 2,
                                width: value,
                                height: newHeight
                            };
                            //console.log(newrect.y);
                            that._plot.navigation.setVisibleRect(newrect, false);
                            that.options.zoom = value;
                        }
                    }
                    break;
                case "visibleRect":
                    if (value !== undefined) {
                        that._plot.navigation.setVisibleRect({ x: value.x, y: -value.y - value.height, width: value.width, height: value.height }, false);
                    }
                    break;
                case "gridVisibility":
                    this._gridLinesPlot.isVisible = value;
                    this._plot.requestUpdateLayout();
                    break;
                case "rects":
                    this._rectsPlot.draw({ rects: value });
                    this._plot.requestUpdateLayout();
                    break;
                case "isLightSVGTop":
                    if (value) {
                        //this.lightSVGDiv.css("z-index", 1501);
                    } else {
                        //this.lightSVGDiv.css("z-index", undefined);
                    }
                    break;
                case "plotConstraint":
                    this._plotSettings = value;
                    break;
                case "useContraints":
                    this._setGestureSource(this._onlyZoomEnabled);
                    break;

            }
            this._super(key, value);
        },

        _setGestureSource: function (onlyZoom) {
            var that = this;
            var gestureSource = InteractiveDataDisplay.Gestures.getGesturesStream(this._plot.host).where(function (g) {
                var constraint = onlyZoom ?
                    g.Type === "Zoom" && (!that.options.useContraints || g.scaleFactor > 1 && that._plot.visibleRect.width < that._plotSettings.MaxWidth || g.scaleFactor < 1 && that._plot.visibleRect.width > that._plotSettings.MinWidth) :
                    g.Type !== "Zoom" || (!that.options.useContraints || g.scaleFactor > 1 && that._plot.visibleRect.width < that._plotSettings.MaxWidth || g.scaleFactor < 1 && that._plot.visibleRect.width > that._plotSettings.MinWidth);
                return constraint;
            });
            this._plot.navigation.gestureSource = gestureSource;
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

        getMouseMoves: function () {
            return this._mouseMoves;
        },

        getPlotX: function (left: number) {
            var cs = this._svgPlot.getScreenToDataTransform();
            return cs.screenToDataX(left);
        },

        getPlotY: function (top: number) {
            var cs = this._svgPlot.getScreenToDataTransform();
            return -cs.screenToDataY(top);
        },

        getLeft: function (x: number) {
            var cs = this._svgPlot.getTransform();
            return -cs.dataToScreenX(x);
        },

        getTop: function (y: number) {
            var cs = this._svgPlot.getTransform();
            return -cs.dataToScreenY(y);
        },

        getPixelWidth: function () {
            var cs = this._svgPlot.getScreenToDataTransform();
            return cs.screenToDataX(1) - cs.screenToDataX(0);
        },

        getZoomSubject: function () {
            return this._zoomService;
        },

        setCenter: function (p) {
            var plotRect = this._plot.visibleRect;
            this._plot.navigation.setVisibleRect({ x: p.x - plotRect.width / 2, y: p.y - plotRect.height / 2, width: plotRect.width, height: plotRect.height }, false);
        },

        getSVG: function () {
            return this._svgPlot.svg;
        },

        getSecondarySVG: function () {
            return this._lightSvgPlot.svg;
        },

        getCentralPart: function () {
            return this._domPlot;
        },

        updateLayout: function () {
            this._plot.updateLayout();
        }

    });
} (jQuery));

interface JQuery {
    drawingsurface(): any;
    drawingsurface(settings: Object): any;
    drawingsurface(methodName: string, arg: any): any;
}
