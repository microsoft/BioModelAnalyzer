/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlresultsviewer", {
        _plot: undefined,
        _variables: undefined,
        _table: undefined,

        options: {
            data: [],
            init: [],
            variables: [],
        },

        _create: function () {
            var that = this;
            this.element.empty();
            this.element.addClass("ltlresultsviewer");

            /*
            <div class="ltlresultsviewer">
                <div id="coloredTable" class="small-simulation-popout-table"></div>
                <div id="progressionTable" class="big-simulation-popout-table simulation-progression-table-container"></div>
                <div class="ltl-simplot-container">
                    <div id="simulationPlot" class="ltl-results"></div>
                </div>
            </div>
            */
            var root = this.element;
            this._variables = $("<div></div>").addClass("small-simulation-popout-table").appendTo(root);
            this._table = $("<div></div>").addClass("big-simulation-popout-table").addClass("simulation-progression-table-container").appendTo(root);

            var plotContainer = $("<div></div>").addClass("ltl-simplot-container").appendTo(root);
            this._plot = $("<div></div>").addClass("ltl-results").appendTo(root);

        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {

                case "data": {
                    this.options.data = value;
                    if (this.options.interval !== undefined && this.options.interval.length !== 0)
                        this._simulationBigTable.progressiontable({ interval: that.options.interval, data: value, canEditInitialValue: false });
                    break;
                }
                
                default: break;
            }
            this._super(key, value);
            this.refresh();
        },

        _setOptions: function (options) {
            this._super(options);
        },

        refresh: function () {
            var that = this;
            
        },

    });
} (jQuery));

interface JQuery {
    ltlresultsviewer(): JQuery;
    ltlresultsviewer(settings: Object): JQuery;
    ltlresultsviewer(optionLiteral: string, optionName: string): any;
    ltlresultsviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 