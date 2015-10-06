/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.ltlresultsviewer", {
        _simulationTables: undefined,
        _simulationSmallTable: undefined,
        _simulationBigTable: undefined,
        _simulationGraph: undefined,

        options: {
            //header: [],
            //numericData: undefined,
            //colorData: undefined,
            data: undefined,
            init: undefined,
            interval: undefined,
            variables: undefined,
            num: 10,
            buttonMode: "ActiveMode",
            step: 10,
            colors: undefined,
        },

        _create: function () {
            var that = this;
            this.element.addClass("ltlresultsviewer");

            this._simulationTables = $("<div></div>").addClass("ltl-results").addClass("scrollable-results").appendTo(this.element);
            this._simulationSmallTable = $("<div></div>").addClass("ltl-results-tables").addClass('small-simulation-popout-table').appendTo(this._simulationTables);
            this._simulationBigTable = $("<div></div>").addClass("ltl-results-tables").addClass('big-simulation-popout-table').appendTo(this._simulationTables);

            this._simulationGraph = $("<div></div>").addClass("ltl-results").appendTo(this.element);

            var stepsdiv = $('<div></div>').addClass('steps-container').appendTo(this._simulationTables);

            this._simulationSmallTable.coloredtableviewer();
            this._simulationBigTable.progressiontable();//{ canEditInitialValue: false });
            this._simulationGraph.simulationplot();

            var step = this.options.step;

            var stepsul = $('<ul></ul>').addClass('button-list').appendTo(stepsdiv);
            var li = $('<li></li>').addClass('action-button-small grey').appendTo(stepsul);
            var li0 = $('<li></li>').appendTo(stepsul);
            var li1 = $('<li></li>').addClass('steps').appendTo(stepsul);
            var li2 = $('<li></li>').appendTo(stepsul);
            var li3 = $('<li></li>').addClass('action-button green').appendTo(stepsul);

            var exportCSV = $('<button></button>')
                .text('EXPORT CSV')
                .appendTo(li);
            exportCSV.bind('click', function () {
                window.Commands.Execute('ExportCSV', {});
            })
            var add10 = $('<button></button>').text('+ ' + step).appendTo(li0);
            add10.bind("click", function () {
                that._setOption("num", that.options.num + step);
            });

            this.num = $('<button></button>').text('STEPS: ' + that.options.num).appendTo(li1);
            var min10 = $('<button></button>').text('- ' + step).appendTo(li2);
            min10.bind("click", function () {
                that._setOption("num", that.options.num - step);
            })
            this.RunButton = $('<button></button>').addClass('run-button').text('Run').appendTo(li3);

            this.refresh();
        },

        _setOption: function (key, value) {
            var that = this;
            switch (key) {
                //case "header": {
                //    this.options.header = value;
                //    if (this.options.numericData !== undefined && this.options.colorData !== undefined)
                //        this._simulationTable.coloredtableviewer({ header: value });
                //    break;
                //}
                //case "numericData": {
                //    this.options.numericData = value;
                //    if (this.options.header.length != 0 && this.options.colorData !== undefined)
                //        this._simulationTable.coloredtableviewer({ numericData: value });
                //    break;
                //}
                //case "colorData": {
                //    this.options.colorData = value;
                //    if (this.options.numericData !== undefined && this.options.header.length != 0)
                //        this._simulationTable.coloredtableviewer({ colorData: value });
                //    break;
                //}

                case "data": {
                    this.options.data = value;
                    if (this.options.interval !== undefined && this.options.interval.length !== 0)
                        this._simulationBigTable.progressiontable({ interval: that.options.interval, data: value });
                    break;
                }
                case "init": {
                    this.options.init = value;
                    this._simulationBigTable.progressiontable({ init: value });
                    break;
                }
                case "num": {
                    if (value < 0) value = 0;
                    this.options.num = value;
                    this.num.text('STEPS: ' + value);
                    break;
                }
                case "variables": {
                    this.options.variables = value;
                    this._simulationSmallTable.coloredtableviewer({
                        header: ["Graph", "Name", "Range"],
                        type: "graph-max",
                        numericData: that.options.variables
                    });
                    break;
                }
                case "interval": {
                    this.options.interval = value;
                    this._simulationBigTable.progressiontable({ interval: value });
                    break;
                }
                case "buttonMode": {
                    this.options.buttonMode = value;
                    this._simulationBigTable.progressiontable({ buttonMode: value });
                    break;
                }
                case "step": {
                    this.options.step = value;
                    this._simulationBigTable.progressiontable({ step: value });
                    break;
                }

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
            this._simulationSmallTable.coloredtableviewer({});
            this._simulationBigTable.progressiontable({});
            //if (this.options.numericData !== undefined && this.options.header.length != 0 && this.options.colorData)
            //    this._simulationTable.coloredtableviewer({ header: this.options.header, numericData: this.options.numericData, colorData: this.options.colorData });
            this._simulationGraph.simulationplot({ colors: this.options.colors });
        },

    });
} (jQuery));

interface JQuery {
    ltlresultsviewer(): JQuery;
    ltlresultsviewer(settings: Object): JQuery;
    ltlresultsviewer(optionLiteral: string, optionName: string): any;
    ltlresultsviewer(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 