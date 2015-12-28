describe("LTLResultsViewerDriver", () => {

    var ltlResultsDriver: BMA.UIDrivers.LTLResultsViewer;
    var popup = $("<div></div>");
    var ltlcommands = new BMA.CommandRegistry();

    beforeEach(() => {
        ltlResultsDriver = new BMA.UIDrivers.LTLResultsViewer(ltlcommands, popup);
    });

    it("PreparePlotLabels", () => {

    });
});