/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
var BMA;
(function (BMA) {
    function LogicToTable(table, log) {
        for (var i = 0; i < log.length; i++) {
            for (var j = 0; j < log[i].length; j++) {
                var td = table.find("tr").eq(i).children("td").eq(j);
                if (log[i][j] !== undefined) {
                    if (log[i][j])
                        td.css("background-color", "#CCFF99");
                    else
                        td.css("background-color", "#FFADAD");
                }
            }
        }
        return;
    }
    BMA.LogicToTable = LogicToTable;
})(BMA || (BMA = {}));
//# sourceMappingURL=logictableconverter.js.map
