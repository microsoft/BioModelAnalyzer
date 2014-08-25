module BMA {
    export module SVGHelper {
        export function AddClass(elem: SVGStylable, c: string) {
            var s = elem.className.baseVal;
            if (!s)
                elem.className.baseVal = c;
            else if (!BMA.SVGHelper.StringInString(s, c))
                elem.className.baseVal = s + " " + c;
        }

        export function RemoveClass(elem: SVGStylable, c: string) {
            var s = elem.className.baseVal.replace(new RegExp("(\\s|^)" + c + "(\\s|$)"), " ");
            // TODO - coalesce spaces
            if (s == " ")
                s = null;
            elem.className.baseVal = s;
        }

        export function StringInString(s: string, find: string) {
            return s.match(new RegExp("(\\s|^)" + find + "(\\s|$)"));
        }
    }
}