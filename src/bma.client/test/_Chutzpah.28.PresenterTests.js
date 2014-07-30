/// <reference path="../Scripts/typings/jasmine/jasmine.d.ts"/>
/// <reference path="../script/model.ts"/>
/// <reference path="../script/layout.ts"/>
/// <chutzpah_reference path="../script/idd.js" />
/// <chutzpah_reference path="../script/svgplot.js" />

describe("PresenterTests", function () {
    it("Should not pass", function () {
        var bioModel = new BMA.Model();
        var layout = new BMA.Layout();
        var svgPlot = new BMAExt.SVGPlot();
        expect(svgPlot).toBeDefined();
    });
});
