/// <reference path="..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\Scripts\typings\jqueryui\jqueryui.d.ts"/>
var BMA;
(function (BMA) {
    function ArrayToTable(array) {
        var table = $('<table></table>');
        for (var i = 0; i < array.length; i++) {
            var tr = $('<tr></tr>').appendTo(table);
            for (var j = 0; j < array[i].length; j++) {
                $('<td>' + array[i][j] + '</td>').appendTo(tr);
            }
        }
        return table;
    }
    BMA.ArrayToTable = ArrayToTable;
})(BMA || (BMA = {}));
//# sourceMappingURL=arraytotableconverter.js.map
