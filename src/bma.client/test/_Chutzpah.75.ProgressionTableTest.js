describe("ProgressionTable", function () {
    var widget = $('<div></div>');

    it("should create a widget", function () {
        widget.progressiontable();
    });

    it("should create a header", function () {
        widget.progressiontable();
        expect(widget.find("tr").eq(0).children("td").eq(0).text()).toEqual("Initial Value");
    });

    it("should addClass 'bma-prooftable'"), function () {
        widget.progressiontable();
        expect(widget.find("table").hasClass('bma-prooftable')).toBeTruthy();
    };

    it("should create column with initial values", function () {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        widget.progressiontable({ interval: interval });

        var trs = widget.find("tr");
        expect(trs.length).toEqual(interval.length + 1);
    });
});
