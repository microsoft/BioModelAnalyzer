﻿
describe("ProgressionTable", () => {

    var widget = $('<div></div>');
    afterEach(() => {
        widget.progressiontable("destroy");
    })

    it("should create a widget", () => {
        widget.progressiontable();
    })

    it("should create a header", () => {
        widget.progressiontable();
        expect(widget.find("tr").eq(0).children("td").eq(0).text()).toEqual("Initial Value");
    })

    it("should addClass 'bma-prooftable'"), () => {
        widget.progressiontable();
        expect(widget.find("table").hasClass('bma-prooftable')).toBeTruthy();
    }

    it("should create column with initial values", () => {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        widget.progressiontable({ interval: interval });

        var trs = widget.find("tr").not(":first-child");
        trs.each(function (ind, val) {
            var td0 = $(this).children("td").eq(0);
            expect(td0.text()).toEqual(interval[ind][0].toString());
            expect(td0.children("div").hasClass("bma-random-icon1")).toBeTruthy();
        })
        expect(trs.length).toEqual(interval.length);
    })

    it("should randomize value on clicking random-icon", () => {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        widget.progressiontable({ interval: interval });

        var trs = widget.find("tr").not(":first-child");
        var td = trs.eq(2).children("td").eq(0);
        expect(td.text()).toEqual('7');
        var rand = td.children("div").eq(0);
        rand.click();
        console.log(td.text());
        //expect(td.text()).not.toEqual('7');
    })

    it("should randomize all", () => {
        var interval = [];
        interval[0] = [1, 3];
        interval[1] = [3, 7];
        interval[2] = [2, 35];
        var td = [];

        widget.progressiontable({ interval: interval });

        var tds = widget.find("tr").not(":first-child").children("td:first-child");
        tds.each(function (ind) {
            td[ind] = $(this).text();
        });
        console.log(td);
        var rand = widget.children("div").eq(0);
        rand.click();

        tds.each(function (ind) {
            td[ind] = $(this).text();
        });
        console.log(td);

    })

    it("should create widget with data", () => {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        var data = [2, 3, 5];

        widget.progressiontable({ interval: interval, data: data });
        var trs = widget.find("tr").not(":first-child");
        for (var i = 0; i < trs.length; i++) {
            expect(trs.eq(i).children("td").eq(1).text()).toEqual(data[i].toString());
        }
    })

    it("should add data", () => {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        var data = [2, 3, 5];

        widget.progressiontable({ interval: interval, data: data });

        var data2 = [4, 6, 1];
        widget.progressiontable({ data: data2 });

        var trs = widget.find("tr").not(":first-child");
        for (var i = 0; i < trs.length; i++) {
            expect(trs.eq(i).children("td").eq(1).text()).toEqual(data[i].toString());
            expect(trs.eq(i).children("td").eq(2).text()).toEqual(data2[i].toString());
        }
    })

})