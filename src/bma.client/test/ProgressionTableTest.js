describe("ProgressionTable", function () {
    window.Commands = new BMA.CommandRegistry();
    var widget = $('<div></div>');

    afterEach(function () {
        widget.progressiontable("destroy");
    });

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

    it("should create column with initial values from interval", function () {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        widget.progressiontable({ interval: interval });

        var trs = widget.find("tr").not(":first-child");
        trs.each(function (ind, val) {
            var tds = $(this).children("td").children("input");
            var td0 = tds.eq(0);
            expect(td0.val()).toEqual(interval[ind][0].toString());
        });
        expect(trs.length).toEqual(interval.length);
    });

    it("should not create column with initial values from init array without interval", function () {
        var init = [11, 22, 33];

        widget.progressiontable({ init: init });

        var tds = widget.find("tr").not(":first-child").children(":first-child");
        expect(tds.length).toEqual(0);
    });

    it("should create column with initial values from init array and add data from interval if necessary", function () {
        var init = [11, 22, 33];
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];
        interval[3] = [6, 10];
        widget.progressiontable({ init: init, interval: interval });
        expect(widget.progressiontable("option", "init")).toEqual(init);
        expect(widget.progressiontable("option", "interval")).toEqual(interval);

        var tds = widget.find("tr").not(":first-child").children(":first-child").children("input");
        expect(tds.length).toEqual(interval.length);
        for (var i = 0; i < init.length; i++) {
            expect(tds.eq(i).val()).toEqual(init[i].toString());
        }
        for (var i = init.length; i < interval.length; i++) {
            expect(tds.eq(i).val()).toEqual(interval[i][0].toString());
        }
    });

    xit("should randomize value on clicking random-icon", function () {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        widget.progressiontable({ interval: interval });

        var trs = widget.find("tr").not(":first-child");
        var td = trs.eq(2).children("td").eq(0).children("input");
        expect(td.val()).toEqual('7');
        var rand = td.children("div").eq(0);
        rand.click();
        console.log(td.children("input").val());
        //expect(td.text()).not.toEqual('7');
    });

    xit("should randomize all", function () {
        var interval = [];
        interval[0] = [1, 3];
        interval[1] = [3, 7];
        interval[2] = [2, 35];
        var td = [];

        widget.progressiontable({ interval: interval });

        var tds = widget.find("tr").not(":first-child").children("td:first-child");
        tds.children("input").each(function (ind) {
            td[ind] = $(this).val();
        });
        console.log(td);
        var rand = widget.children("div").eq(0);
        rand.click();

        tds.children("input").each(function (ind) {
            td[ind] = $(this).val();
        });
        console.log(td);
    });

    it("should create widget with data", function () {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        var data = [2, 3, 5];

        widget.progressiontable({ interval: interval, data: data });
        var trs = widget.find("table").eq(1).find("tr");
        for (var i = 0; i < trs.length; i++) {
            expect(trs.eq(i).children("td").eq(0).text()).toEqual(data[i].toString());
        }
    });

    it("should add data", function () {
        var interval = [];
        interval[0] = [2, 3];
        interval[1] = [0, 5];
        interval[2] = [7, 18];

        var data = [2, 3, 5];

        widget.progressiontable({ interval: interval, data: data });

        var data2 = [4, 6, 1];
        widget.progressiontable({ data: data2 });

        var trs = widget.find("table").eq(1).find("tr");
        for (var i = 0; i < trs.length; i++) {
            expect(trs.eq(i).children("td").eq(0).text()).toEqual(data[i].toString());
            expect(trs.eq(i).children("td").eq(1).text()).toEqual(data2[i].toString());
        }
    });

    it("should compare different columns", function () {
        var table = $('<table></table>');
        var tr0 = $('<tr></tr>').appendTo(table);
        var tr1 = $('<tr></tr>').appendTo(table);
        var td0 = $('<td></td>').text(1).appendTo(tr0);
        var td1 = $('<td></td>').text(2).appendTo(tr0);
        var td2 = $('<td></td>').text(3).appendTo(tr0);
        var td3 = $('<td></td>').text(4).appendTo(tr1);
        var td4 = $('<td></td>').text(5).appendTo(tr1);
        var td5 = $('<td></td>').text(6).appendTo(tr1);
        var t1 = table.find('tr').children("td:nth-child(2)");

        //console.log("Start ");
        //for (var i = 0; i < t1.length; i++)
        //    console.log(t1.eq(i).text());
        var t2 = table.find("tr").children("td:nth-child(3)");

        //for (var i = 0; i < t2.length; i++)
        //    console.log(t2.eq(i).text());
        widget.progressiontable();
        expect(widget.progressiontable("IsClone", t1, t2)).toBeFalsy();
    });

    it("should compare equal columns", function () {
        var table = $('<table></table>');
        var tr0 = $('<tr></tr>').appendTo(table);
        var tr1 = $('<tr></tr>').appendTo(table);
        var td0 = $('<td></td>').text(1).appendTo(tr0);
        var td1 = $('<td></td>').text(2).appendTo(tr0);
        var td2 = $('<td></td>').text(2).appendTo(tr0);
        var td3 = $('<td></td>').text(4).appendTo(tr1);
        var td4 = $('<td></td>').text(2).appendTo(tr1);
        var td5 = $('<td></td>').text(2).appendTo(tr1);
        var t1 = table.find('tr').children("td:nth-child(2)");
        var t2 = table.find("tr").children("td:nth-child(3)");

        widget.progressiontable();
        expect(widget.progressiontable("IsClone", t1, t2)).toBeTruthy();
    });
});
//# sourceMappingURL=ProgressionTableTest.js.map
