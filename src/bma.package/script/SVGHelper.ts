module BMA {
    export module SVGHelper {
        export function AddClass(elem: SVGStylable, c: string) {
            var s = <string>(elem.className.baseVal);
            if (!s || s.indexOf("null") > -1)
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

        export function ChangeStrokeWidth(elem: SVGStylable, width: string /* because usual width string is '2px'*/) {
            elem.style.strokeWidth = width;
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

        export function CreateOperandLayout(op: BMA.LTLOperations.IOperand): any {

        }

        export function CalcAndAssignOperandWidthAndDepth(op: BMA.LTLOperations.IOperand, paddingX: number): { layer: number; width: number } {
            var operator = (<any>op).Operator;
            if (operator !== undefined) {
                var operands = (<BMA.LTLOperations.Operation>op).Operands;
                var layer = 0;
                var width = GetOperatorWidth(operator, paddingX);

                for (var i = 0; i < operands.length; i++) {
                    var calcLW = CalcAndAssignOperandWidthAndDepth(operands[i], paddingX);
                    layer = Math.max(layer, calcLW.layer);
                    width += (calcLW.width + paddingX * 2);
                }

                (<any>op).layer = layer + 1;
                (<any>op).width = width;
                return {
                    layer: layer + 1,
                    width: width
                }
            } else {
                var w = GetKeyframeWidth(<BMA.LTLOperations.Keyframe>op, paddingX);
                (<any>op).layer = 1;
                (<any>op).width = w;
                return {
                    layer: 1,
                    width: w
                }
            }
        }

        export function GetOperatorWidth(op: BMA.LTLOperations.Operator, paddingX: number): number {
            return op.Name.length * 4 + paddingX;
        }

        export function GetKeyframeWidth(op: BMA.LTLOperations.Keyframe, paddingX: number): number {
            return 25 + paddingX;
        }

        export function bboxText(svgDocument: any, text: string): any {
            var data = svgDocument.createTextNode(text);
            var svgns = "";
            var svgElement = svgDocument.createElementNS(svgns, "text");
            svgElement.appendChild(data);
            svgDocument.documentElement.appendChild(svgElement);
            var bbox = svgElement.getBBox();
            svgElement.parentNode.removeChild(svgElement);
            return bbox;
        }
    }
}