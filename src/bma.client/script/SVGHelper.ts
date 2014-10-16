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

        export function DoNothing() {
            return null;
        }

        export function GeEllipsePoint(ellipseX, ellipseY, ellipseWidth, ellipseHeight, pointX, pointY): { x: number; y: number } {

            if (pointX === ellipseX)
                return { x: ellipseX, y: ellipseY + ellipseHeight };

            var a = (ellipseY - pointY) / (ellipseX - pointX);
            var b = (ellipseX * pointY - pointX * ellipseY) / (ellipseX - pointX);
            var a1 = ellipseHeight * ellipseHeight + ellipseWidth * ellipseWidth * a * a;
            var b1 = 2 * (a * (b - ellipseY) * ellipseWidth * ellipseWidth - ellipseHeight * ellipseHeight * ellipseX);
            var c1 = ellipseX * ellipseX * ellipseHeight * ellipseHeight + ellipseWidth * ellipseWidth * (b - ellipseY) * (b - ellipseY) -
                ellipseHeight * ellipseHeight * ellipseWidth * ellipseWidth;


            var sign = (pointX - ellipseX) / Math.abs(pointX - ellipseX);

            var x = (- b1 + sign * Math.sqrt(b1 * b1 - 4 * a1 * c1)) / (2 * a1);
            var y = a * x + b;

            return { x: x, y: y };
        }

        export function GeEllipsePoints(ellipseX, ellipseY, ellipseWidth, ellipseHeight, pointX, pointY): { x: number; y: number }[] {

            if (pointX === ellipseX)
                return [{ x: ellipseX, y: ellipseY + ellipseHeight }, { x: ellipseX, y: ellipseY - ellipseHeight }];

            var a = (ellipseY - pointY) / (ellipseX - pointX);
            var b = (ellipseX * pointY - pointX * ellipseY) / (ellipseX - pointX);
            var a1 = ellipseHeight * ellipseHeight + ellipseWidth * ellipseWidth * a * a;
            var b1 = 2 * (a * (b - ellipseY) * ellipseWidth * ellipseWidth - ellipseHeight * ellipseHeight * ellipseX);
            var c1 = ellipseX * ellipseX * ellipseHeight * ellipseHeight + ellipseWidth * ellipseWidth * (b - ellipseY) * (b - ellipseY) -
                ellipseHeight * ellipseHeight * ellipseWidth * ellipseWidth;


            var sign = (pointX - ellipseX) / Math.abs(pointX - ellipseX);

            var x1 = (- b1 + sign * Math.sqrt(b1 * b1 - 4 * a1 * c1)) / (2 * a1);
            var y1 = a * x1 + b;

            var x2 = (- b1 - sign * Math.sqrt(b1 * b1 - 4 * a1 * c1)) / (2 * a1);
            var y2 = a * x2 + b;


            return [{ x: x1, y: y1 }, { x: x2, y: y2 }];
        }

    }
}