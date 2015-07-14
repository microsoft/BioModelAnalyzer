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

        /*
        export function GetOperationSVG(svg: any, position: { x: number; y: number }, op: BMA.LTLOperations.Operation): string {
            var operator = op.Operator;
            var operands = op.Operands;
            return GetOperatorSVG(svg, position, operator, operands);
        }

        export function GetOperandSVG(svg: any, position: { x: number; y: number }, op: BMA.LTLOperations.IOperand): { svg: any; depthLevel: number } {

            //TODO: Rethink method of distingushing keyframes from operations
            var operator = (<any>op).Operator;

            if (operator !== undefined) {
                return GetOperationSVG(svg, position, <BMA.LTLOperations.Operation>op);
            } else {
                return {
                    svg: GetKeyFrameSVG(svg, position, <BMA.LTLOperations.Keyframe>op), depthLevel: 1
                };
            }
        }

        export function GetKeyFrameSVG(svg: any, position: { x: number; y: number }, keyframe: BMA.LTLOperations.Keyframe): string {
            return "";
        }

        export function GetOperatorSVG(svg: any, position: { x: number; y: number }, op: BMA.LTLOperations.Operator, operands: BMA.LTLOperations.IOperand[]): { svg: any; depthLevel: number } {
            if (operands.length !== op.OperandsCount)
                throw "Invalid Operands Count for Operator's rendering";

            var operandSVGs = [];
            for (var i = 0; i < operands.length; i++) {
                operandSVGs.push(GetOperandSVG(svg, position, operands[i]));
            }

            switch (operands.length) {
                case 1:
                    return {
                        svg: undefined,
                        depthLevel: (<any>operandSVGs[0]).depthLevel
                    }
                    break;
                case 2:
                    return {
                        svg: undefined,
                        depthLevel: Math.max((<any>operandSVGs[0]).depthLevel,(<any>operandSVGs[1]).depthLevel)
                    }
                    break;
                default:
                    throw "Rendering of operators with " + operands.length + " operands is not supported"; 
            }
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
        */

    }
}