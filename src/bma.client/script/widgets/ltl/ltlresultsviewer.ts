/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlresultsviewer", {
        _simulationTable: undefined,
        _simulationGraph: undefined,

        options: {
            header: [],
            numericData: undefined,
            colorData: undefined,
            //data: undefined,
            //init: undefined,
            //interval: undefined,
            //variables: undefined,
            //num: 10,
            //buttonMode: "ActiveMode",
            //step: 10,
            colors: undefined,
        },

        _create: function () {
            var that = this;
            this._simulationTable = $("<div></div>").addClass("ltl-results").appendTo(this.element);
            this._simulationGraph = $("<div></div>").addClass("ltl-results").appendTo(this.element);

            this._simulationTable.coloredtableviewer();
            this._simulationGraph.simulationplot();
            this.refresh();
        },

        _setOption: function (key, value) {
            switch (key) {
                case "header": {
                    this.options.header = value;
                    if (this.options.numericData !== undefined && this.options.colorData !== undefined)
                        this._simulationTable.coloredtableviewer({ header: value });
                    break;
                }
                case "numericData": {
                    this.options.numericData = value;
                    if (this.options.header.length != 0 && this.options.colorData !== undefined)
                        this._simulationTable.coloredtableviewer({ numericData: value });
                    break;
                }
                case "colorData": {
                    this.options.colorData = value;
                    if (this.options.numericData !== undefined && this.options.header.length != 0)
                        this._simulationTable.coloredtableviewer({ colorData: value });
                    break;
                }
                //case "data": {
                //    this.options.data = value;
                //    this._simulationTable.progressiontable({ data: value });
                //    break;
                //}
                //case "init": {
                //    this.options.init = value;
                //    this._simulationTable.progressiontable({ init: value });
                //    break;
                //}
                //case "interval": {
                //    this.options.interval = value;
                //    this._simulationTable.progressiontable({ interval: value });
                //    break;
                //}
                //case "variables": {
                //    this.options.variables = value;
                //    this._simulationTable.progressiontable({ variables: value });
                //    break;
                //}
                //case "num": {
                //    this.options.num = value;
                //    this._simulationTable.progressiontable({ num: value });
                //    break;
                //}
                //case "buttonMode": {
                //    this.options.buttonMode = value;
                //    this._simulationTable.progressiontable({ buttonMode: value });
                //    break;
                //}
                //case "step": {
                //    this.options.step = value;
                //    this._simulationTable.progressiontable({ step: value });
                //    break;
                //}
                case "colors": {
                    this.options.colors = value;
                    this._simulationGraph.simulationplot({ colors: value });
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
            if (this.options.numericData !== undefined && this.options.header.length != 0 && this.options.colorData)
                this._simulationTable.coloredtableviewer({ header: this.options.header, numericData: this.options.numericData, colorData: this.options.colorData });
            this._simulationGraph.simulationplot({ colors: this.options.colors });
        },

    });
} (jQuery));

interface JQuery {
    statessimulation(): JQuery;
    statessimulation(settings: Object): JQuery;
    statessimulation(optionLiteral: string, optionName: string): any;
    statessimulation(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 