/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.simulationplot", {
        options: {
            data: undefined,

        },

        _create: function () {
            var that = this;
            
            
            //var chart = $('<div id="chart" data-idd-plot="chart" style="width: 100%; height: 160px;" unselectable="on" class="unselectable idd-plot-master"></div>').appendTo(that.element);
            var plotDiv = $('<div data-idd-plot="polyline" style="width:100%; height:160px"></div>').appendTo(that.element);
            //var grid = $('<div data-idd-plot="grid" data-idd-placement="center" unselectable="on" class="unselectable idd-plot-dependant" style="width: 100%; height: 160px; top: 0px; left: 40px;"></div>').appendTo(chart);
            that._plot = InteractiveDataDisplay.asPlot(plotDiv);
            that._plot.isAutoFitEnabled = true;
            this.refresh();
        //<div id="p" data-idd-plot="polyline" data-idd-style="stroke: rgb(89,150,255); thickness: 3" unselectable="on" class="unselectable idd-plot-dependant" style="width: 760px; height: 571px; top: 0px; left: 40px;"><canvas class="idd-plot-canvas" width="760" height="571"></canvas></div>
    //<div data-idd-plot="plot" data-idd-placement="center" unselectable="on" class="unselectable idd-plot-dependant" style="z-index: 1000; width: 760px; height: 571px; top: 0px; left: 40px; background-color: rgba(0, 0, 0, 0);"><div class="idd-legend unselectable" style="display: block; float: right;"><div class="idd-legend-item"><canvas style="margin-right: 15px" width="20" height="20"></canvas><span>p</span></div></div></div><div class="idd-figure-container" data-idd-placement="bottom" style="width: 760px; left: 40px; top: 571px;"><div data-idd-axis="numeric" data-idd-placement="bottom" class="idd-axis unselectable" style="position: relative; height: 29px; width: 760px;"><canvas id="canvas" style="position:relative; float:left" height="11" width="760"></canvas><div id="labelsDiv" style="position: relative; float: left; width: 760px; height: 18px;"><div class="idd-axis-label" style="display: block; left: 37.4453947368421px;">0</div><div class="idd-axis-label" style="display: block; left: 144.130263157895px;">0.5</div><div class="idd-axis-label" style="display: block; left: 369.5px;">1.5</div><div class="idd-axis-label" style="display: block; left: 594.869736842105px;">2.5</div><div class="idd-axis-label" style="display: none;">0.8</div><div class="idd-axis-label" style="display: block; left: 262.815131578947px;">1</div><div class="idd-axis-label" style="display: none;">1.2</div><div class="idd-axis-label" style="display: none;">1.4</div><div class="idd-axis-label" style="display: none;">1.6</div><div class="idd-axis-label" style="display: none;">1.8</div><div class="idd-axis-label" style="display: block; left: 488.184868421053px;">2</div><div class="idd-axis-label" style="display: none;">2.2</div><div class="idd-axis-label" style="display: none;">2.4</div><div class="idd-axis-label" style="display: none;">2.6</div><div class="idd-axis-label" style="display: none;">2.8</div><div class="idd-axis-label" style="display: block; left: 713.554605263158px;">3</div></div></div></div><div class="idd-figure-container" data-idd-placement="left" style="height: 571px; left: 0px; top: 0px;"><div data-idd-axis="numeric" data-idd-placement="left" class="idd-axis unselectable" style="position: relative; width: 40px; height: 571px;"><div id="labelsDiv" style="position: relative; float: left; height: 571px; width: 26px;"><div class="idd-axis-label" style="display: block; top: 519.572679509632px; left: 12px;">-1</div><div class="idd-axis-label" style="display: block; top: 397.786339754816px; left: 0px;">-0.5</div><div class="idd-axis-label" style="display: block; top: 154.213660245184px; left: 6px;">0.5</div><div class="idd-axis-label" style="display: none;">-0.4</div><div class="idd-axis-label" style="display: none;">-0.2</div><div class="idd-axis-label" style="display: block; top: 276px; left: 18px;">0</div><div class="idd-axis-label" style="display: none;">0.2</div><div class="idd-axis-label" style="display: none;">0.4</div><div class="idd-axis-label" style="display: none;">0.6</div><div class="idd-axis-label" style="display: none;">0.8</div><div class="idd-axis-label" style="display: block; top: 32.4273204903678px; left: 18px;">1</div></div><canvas id="canvas" style="position: relative; float: left; left: 3px;" width="11" height="571"></canvas></div></div></div>
        },

        refresh: function () {
            var that = this;
            var options = this.options;
            if (options.data !== undefined) {
                for (var i = 0; i < options.data.length; i++) {
                    that._plot.draw({ y: options.data[i], thickness: 4, lineJoin: 'round' });
                }
            }
        },

        _destroy: function () {
            this.element.empty();
        },

        _setOption: function (key, value) {
            var that = this;
            if (key === "data") this.options.data = value;

            this._super(key, value);
            if (value !== null && value !== undefined)
                this.refresh();
        }
    });
} (jQuery));

interface JQuery {
    simulationplot(): JQuery;
    simulationplot(settings: Object): JQuery;
    simulationplot(settings: string): any;
    simulationplot(optionLiteral: string, optionName: string): any;
    simulationplot(optionLiteral: string, optionName: string, optionValue: any): JQuery;
}   