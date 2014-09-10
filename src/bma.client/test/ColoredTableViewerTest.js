describe("ColoredTableViewer", function () {
    var widget = $('<div></div>');
    afterEach(function () {
        widget.coloredtableviewer("destroy");
    });

    it("creates widget", function () {
        widget.coloredtableviewer();
    });

    it("creates widget with options", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];

        widget.coloredtableviewer({ header: header, numericData: numericData, colorData: colorData });
        var l = widget.find("tr").length;
        expect(l).toEqual(numericData.length + 1);
        expect(widget.find("td").length).toEqual(12);
    });

    it("changes header option", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];

        widget.coloredtableviewer({ header: header, numericData: numericData, colorData: colorData });

        header = [4, 5, 6];
        widget.coloredtableviewer({
            header: header
        });
        var headertd = widget.find("tr").eq(0).children("td");
        expect(headertd.each(function (ind, val) {
            header[ind].toString() === $(val).text();
        })).toBeTruthy();
    });

    it("changes numericData option", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];

        widget.coloredtableviewer({ header: header, numericData: numericData });

        numericData[0] = [4, 4, 4];
        numericData[1] = [5, 5, 5];
        numericData[2] = [6, 6, 6];

        widget.coloredtableviewer({
            numericData: numericData
        });

        var datatd0 = widget.find("tr").eq(1).children("td");
        expect(datatd0.each(function (ind, val) {
            numericData[0][ind].toString() === $(val).text();
        })).toBeTruthy();
    });

    it("creates proper table with colored background", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];

        widget.coloredtableviewer({ header: header, numericData: numericData, colorData: colorData });
        var tr = widget.find("tr");
        for (var i = 0; i < header.length; i++) {
            expect(tr.eq(0).children().eq(i).text()).toEqual(header[i].toString());
        }
        for (var j = 0; j < numericData.length; j++) {
            for (var i = 0; i < numericData[j].length; i++) {
                var ij = tr.eq(j + 1).children().eq(i);
                expect(ij.text()).toEqual(numericData[j][i].toString());
                if (colorData[j][i])
                    expect(ij.css("background-color")).toEqual("rgb(204, 255, 153)");
                else
                    expect(ij.css("background-color")).toEqual("rgb(255, 173, 173)");
            }
        }
    });

    it("creates widget only with header", function () {
        var header = [1, 2, 3];
        widget.coloredtableviewer({ header: header });
        expect(widget.find("tr").length).toEqual(0);
    });

    it("creates widget only with colorData", function () {
        console.log("color only");
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];
        widget.coloredtableviewer({ colorData: colorData });
        expect(widget.find("tr").length).toEqual(colorData.length);
        expect(widget.find("td").length).toEqual(colorData.length * 3);
    });

    it("creates widget with not compatible data sizes", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false];
        colorData[2] = [true, true, true];
        colorData[3] = [true, true, true];
        widget.coloredtableviewer({ header: header, numericData: numericData, colorData: colorData });
    });

    it("creates widget with not compatible data sizes-2", function () {
        var header = [1, 2, 3];
        var numericData = [];
        numericData[0] = [1, 1, 1];
        numericData[1] = [2, 2, 2];
        numericData[2] = [3, 3, 3];
        var colorData = [];
        colorData[0] = [true, false, true];
        colorData[1] = [false, false, false, false];
        colorData[2] = [true, true, true];
        widget.coloredtableviewer({ header: header, numericData: numericData, colorData: colorData });
    });
});
//# sourceMappingURL=ColoredTableViewerTest.js.map
