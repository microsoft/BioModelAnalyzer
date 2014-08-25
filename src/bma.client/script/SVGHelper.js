var BMA;
(function (BMA) {
    (function (SVGHelper) {
        function AddClass(elem, c) {
            var s = elem.className.baseVal;
            if (!s)
                elem.className.baseVal = c;
            else if (!BMA.SVGHelper.StringInString(s, c))
                elem.className.baseVal = s + " " + c;
        }
        SVGHelper.AddClass = AddClass;

        function RemoveClass(elem, c) {
            var s = elem.className.baseVal.replace(new RegExp("(\\s|^)" + c + "(\\s|$)"), " ");

            // TODO - coalesce spaces
            if (s == " ")
                s = null;
            elem.className.baseVal = s;
        }
        SVGHelper.RemoveClass = RemoveClass;

        function StringInString(s, find) {
            return s.match(new RegExp("(\\s|^)" + find + "(\\s|$)"));
        }
        SVGHelper.StringInString = StringInString;
    })(BMA.SVGHelper || (BMA.SVGHelper = {}));
    var SVGHelper = BMA.SVGHelper;
})(BMA || (BMA = {}));
//# sourceMappingURL=SVGHelper.js.map
