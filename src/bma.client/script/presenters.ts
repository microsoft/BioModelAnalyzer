/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
/// <reference path="model.ts"/>
/// <reference path="uidrivers.ts"/>
/// <reference path="commands.ts"/>


module BMA {
    export module Presenters {
        export class DesignSurfacePresenter {
            private model: BMA.Model.BioModel;
            private layout: BMA.Model.Layout;
            private selectedType: string;
            private driver: BMA.UIDrivers.ISVGPlot;

            constructor(bioModel: BMA.Model.BioModel, layout: BMA.Model.Layout, svgPlotDriver: BMA.UIDrivers.ISVGPlot) {
                var that = this;
                this.model = bioModel;
                this.layout = layout;
                this.driver = svgPlotDriver;

                window.Commands.On("AddElementSelect", (type: string) => {
                    this.selectedType = type;
                    this.driver.TurnNavigation(type === undefined);
                });

                window.Commands.On("DrawingSurfaceClick", (args: { x: number; y: number }) => {
                    if (that.selectedType !== undefined) {
                        var element = window.ElementRegistry.GetElementByType(that.selectedType);
                        that.driver.Draw(element.RenderToSvg(args.x, args.y));
                    }
                });
            }

            private CreateSvg(): SVGElement {



                return null;
            }
        }
    }
} 