interface Window {
    ElementRegistry: BMA.Elements.ElementsRegistry;
}

module BMA {
    export module Elements {
        export class Element {
            private type: string;
            private renderToSvg: (renderParams: any) => SVGElement;
            private contains: (pointerX: number, pointerY: number, elementX, elementY) => boolean;
            private description: string;
            private iconUrl: string;

            public get Type(): string {
                return this.type;
            }

            public get RenderToSvg(): (renderParams: any) => SVGElement {
                return this.renderToSvg;
            }

            public get Description(): string {
                return this.description;
            }

            public get IconURL(): string {
                return this.iconUrl;
            }

            public get Contains(): (pointerX: number, pointerY: number, elementX, elementY) => boolean {
                return this.contains;
            }

            constructor(
                type: string,
                renderToSvg: (renderParams: any) => SVGElement,
                contains: (pointerX: number, pointerY: number, elementX, elementY) => boolean,
                description: string,
                iconUrl: string) {

                this.type = type;
                this.renderToSvg = renderToSvg;
                this.contains = contains;
                this.description = description;
                this.iconUrl = iconUrl;

            }
        }

        export class BboxElement extends Element {
            private getBbox: (x: number, y: number) => { x: number; y: number; width: number; height: number };

            public get GetBoundingBox(): (x: number, y: number) => { x: number; y: number; width: number; height: number } {
                return this.getBbox;
            }

            constructor(
                type: string,
                renderToSvg: (renderParams: any) => SVGElement,
                contains: (pointerX: number, pointerY: number, elementX, elementY) => boolean,
                getBbox: (x: number, y: number) => { x: number; y: number; width: number; height: number },
                description: string,
                iconUrl: string) {

                super(type, renderToSvg, contains, description, iconUrl);

                this.getBbox = getBbox;
            }
        }

        export class BorderContainerElement extends Element {
            private intersectsBorder: (pointerX: number, pointerY: number, elementX: number, elementY: number, elementParams: any) => boolean;
            private containsBBox: (bbox: { x: number; y: number; width: number; height: number }, elementX: number, elementY: number, elementParams: any) => boolean;

            public get IntersectsBorder(): (pointerX: number, pointerY: number, elementX, elementY, elementParams: any) => boolean {
                return this.intersectsBorder;
            }

            public get ContainsBBox(): (bbox: { x: number; y: number; width: number; height: number }, elementX: number, elementY: number, elementParams: any) => boolean {
                return this.containsBBox;
            }

            constructor(
                type: string,
                renderToSvg: (renderParams: any) => SVGElement,
                contains: (pointerX: number, pointerY: number, elementX, elementY) => boolean,
                intersectsBorder: (pointerX: number, pointerY: number, elementX: number, elementY: number, elementParams: any) => boolean,
                containsBBox: (bbox: { x: number; y: number; width: number; height: number }, elementX: number, elementY: number, elementParams: any) => boolean,
                description: string,
                iconUrl: string) {

                super(type, renderToSvg, contains, description, iconUrl);

                this.intersectsBorder = intersectsBorder;
                this.containsBBox = containsBBox;
            }
        }

        export class ElementsRegistry {
            private elements: Element[];
            private variableWidthConstant = 35;
            private variableHeightConstant = 30;
            private variableSizeConstant = 30;
            private relationshipBboxOffset = 20;
            private containerRadius = 100;
            private svg;

            private lineWidth = 1;
            private labelSize = 10;
            private labelVisibility = true;

            public get LineWidth(): number {
                return this.lineWidth;
            }

            public set LineWidth(value: number) {
                this.lineWidth = value;
                console.log(this.lineWidth);
            }

            public get LabelSize(): number {
                return this.labelSize;
            }

            public set LabelSize(value: number) {
                this.labelSize = value;
            }

            public get LabelVisibility(): boolean {
                return this.labelVisibility;
            }

            public set LabelVisibility(value: boolean) {
                this.labelVisibility = value;
            }

            private CreateSvgElement(type: string, renderParams: any) {
                var elem = <SVGElement>document.createElementNS("http://www.w3.org/2000/svg", type);
                var transform = "";
                if (renderParams.x != 0 || renderParams.y != 0)
                    transform += "translate(" + renderParams.x + "," + renderParams.y + ")";
                if (renderParams.scale !== undefined && renderParams.scale != 1.0)
                    transform += "scale(" + renderParams.scale + "," + renderParams.scale + ")";
                if (transform.length > 0)
                    elem.setAttribute("transform", transform);
                return elem;
            }

            private CreateSvgPath(data: string, color: string, x: number = 0, y: number = 0, scale: number = 1.0) {
                var elem = <SVGPathElement>this.CreateSvgElement("path", { x: x, y: y, scale: scale });
                elem.setAttribute("d", data);
                elem.setAttribute("fill", color);
                return elem;

            }

            public get Elements(): Element[] {
                return this.elements;
            }

            public GetElementByType(type: string): Element {
                for (var i = 0; i < this.elements.length; i++) {
                    if (this.elements[i].Type === type)
                        return this.elements[i];
                }
                throw "the is no element for specified type";
            }

            constructor() {
                var that = this;
                this.elements = [];

                var svgCnt = $("<div></div>");
                svgCnt.svg({
                    onLoad: (svg) => {
                        this.svg = svg;
                    }
                });

                var containerInnerEllipseWidth = 102;
                var containerInnerEllipseHeight = 124;
                var containerOuterEllipseWidth = 112;
                var containerOuterEllipseHeight = 130;
                var containerInnerCenterOffset = 5;
                var containerOuterCenterOffset = 0;

                this.elements.push(new BorderContainerElement(
                    "Container",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        var x = (renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + (renderParams.layout.Size - 1) * renderParams.grid.xStep / 2;
                        var y = (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep + (renderParams.layout.Size - 1) * renderParams.grid.yStep / 2;

                        var g = jqSvg.group({
                            transform: "translate(" + x + ", " + y + ") scale(" + 0.45 * renderParams.layout.Size + ") translate(-250, -290)"
                        });

                        var cellData = "M249,577 C386.518903,577 498,447.83415 498,288.5 C498,129.16585 386.518903,0 249,0 C111.481097,0 0,129.16585 0,288.5 C0,447.83415 111.481097,577 249,577 Z M262,563 C387.368638,563 489,440.102164 489,288.5 C489,136.897836 387.368638,14 262,14 C136.631362,14 35,136.897836 35,288.5 C35,440.102164 136.631362,563 262,563 Z";
                        var cellPath = jqSvg.createPath();
                        var op = jqSvg.path(g, cellPath, {
                            stroke: 'transparent',
                            fill: "#FAAF40",
                            "fill-rule": "evenodd",
                            d: cellData,
                        });

                        $(op).attr("onmouseover", "BMA.SVGHelper.AddClass(this, 'modeldesigner-element-hover')");
                        $(op).attr("onmouseout", "BMA.SVGHelper.RemoveClass(this, 'modeldesigner-element-hover')");

                        /*
                        //Helper bounding ellipses
                        jqSvg.ellipse(
                            (renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + containerOuterCenterOffset * renderParams.layout.Size + (renderParams.layout.Size - 1) * renderParams.grid.xStep / 2,
                            (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep + (renderParams.layout.Size - 1) * renderParams.grid.yStep / 2,
                            containerOuterEllipseWidth * renderParams.layout.Size, containerOuterEllipseHeight * renderParams.layout.Size, { stroke: "red", fill: "none" });
                        
                        jqSvg.ellipse(
                            (renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + containerInnerCenterOffset * renderParams.layout.Size + (renderParams.layout.Size - 1) * renderParams.grid.xStep / 2,
                            (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep + (renderParams.layout.Size - 1) * renderParams.grid.yStep / 2,
                            containerInnerEllipseWidth * renderParams.layout.Size, containerInnerEllipseHeight * renderParams.layout.Size, { stroke: "red", fill: "none" });

                        jqSvg.ellipse(
                            x + containerOuterCenterOffset * renderParams.layout.Size / 2,
                            y,
                            (containerInnerEllipseWidth + containerOuterEllipseWidth) * renderParams.layout.Size / 2,
                            (containerInnerEllipseHeight + containerOuterEllipseHeight) * renderParams.layout.Size / 2,
                            { stroke: "red", fill: "none" });
                        */

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;
                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        return false;
                    },
                    function (pointerX: number, pointerY: number, elementX, elementY, elementParams: any) {
                        var innerCenterX = elementX + containerInnerCenterOffset * elementParams.Size + elementParams.xStep * (elementParams.Size - 1);
                        var dstXInner = Math.abs(pointerX - innerCenterX);

                        var outerCenterX = elementX + containerOuterCenterOffset * elementParams.Size + elementParams.xStep * (elementParams.Size - 1);
                        var dstXOuter = Math.abs(pointerX - outerCenterX);

                        var centerY = elementY + elementParams.yStep * (elementParams.Size - 1);
                        var dstY = Math.abs(pointerY - centerY);

                        var outerCheck = Math.pow(dstXOuter / (containerOuterEllipseWidth * elementParams.Size), 2) + Math.pow(dstY / (containerOuterEllipseHeight * elementParams.Size), 2) < 1;
                        var innerCheck = Math.pow(dstXInner / (containerInnerEllipseWidth * elementParams.Size), 2) + Math.pow(dstY / (containerInnerEllipseHeight * elementParams.Size), 2) > 1;
                        return outerCheck && innerCheck;
                    },
                    function (bbox: { x: number; y: number; width: number; height: number }, elementX: number, elementY: number, elementParams: any) {

                        var iscontaining = function (x, y) {
                            var dstX = Math.abs(x - (elementX + containerInnerCenterOffset * elementParams.Size + elementParams.xStep * (elementParams.Size - 1)));
                            var dstY = Math.abs(y - elementY - elementParams.yStep * (elementParams.Size - 1));
                            return Math.pow(dstX / (containerInnerEllipseWidth * elementParams.Size), 2) + Math.pow(dstY / (containerInnerEllipseHeight * elementParams.Size), 2) < 1
                        }

                        var leftTop = iscontaining(bbox.x, bbox.y);
                        var leftBottom = iscontaining(bbox.x, bbox.y + bbox.height);
                        var rightTop = iscontaining(bbox.x + bbox.width, bbox.y);
                        var rightBottom = iscontaining(bbox.x + bbox.width, bbox.y + bbox.height);


                        return leftTop && leftBottom && rightTop && rightBottom;
                    },
                    "Cell",
                    "images/container.png"));

                this.elements.push(new BboxElement(
                    "Constant",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        var g = jqSvg.group({
                            transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")",
                        });

                        var data = "M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z";
                        var path = jqSvg.createPath();
                        var variable = jqSvg.path(g, path, {
                            stroke: 'transparent',
                            fill: "#BBBDBF",
                            "stroke-width": 8,
                            d: data,
                            transform: "scale(0.36)"
                        });

                        if (that.labelVisibility === true) {
                            jqSvg.text(g, 0, 0, renderParams.model.Name, {
                                transform: "translate(" + -that.variableWidthConstant / 2 + ", " + (that.variableHeightConstant / 2 + that.labelSize) + ")",
                                "font-size": that.labelSize
                            });
                        }

                        $(variable).attr("onmouseover", "BMA.SVGHelper.AddClass(this, 'modeldesigner-element-hover')");
                        $(variable).attr("onmouseout", "BMA.SVGHelper.RemoveClass(this, 'modeldesigner-element-hover')");

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;
                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                    },
                    function (elementX: number, elementY: number) {
                        return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                    },
                    "Extracellural Protein",
                    "images/constant.png"));

                this.elements.push(new BboxElement(
                    "Default",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        var g = jqSvg.group({
                            transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")",
                        });

                        var data = "M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z";
                        var path = jqSvg.createPath();
                        var variable = jqSvg.path(g, path, {
                            stroke: 'transparent',
                            fill: "#EF4137",
                            strokeWidth: 8,
                            d: data,
                            transform: "scale(0.36)"
                        });

                        if (that.labelVisibility === true) {
                            jqSvg.text(g, 0, 0, renderParams.model.Name, {
                                transform: "translate(" + -that.variableWidthConstant / 2 + ", " + (that.variableHeightConstant / 2 + that.labelSize) + ")",
                                "font-size": that.labelSize
                            });
                        }

                        $(variable).attr("onmouseover", "BMA.SVGHelper.AddClass(this, 'modeldesigner-element-hover')");
                        $(variable).attr("onmouseout", "BMA.SVGHelper.RemoveClass(this, 'modeldesigner-element-hover')");

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;
                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                    },
                    function (elementX: number, elementY: number) {
                        return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                    },
                    "Intracellural Protein",
                    "images/variable.png"));

                this.elements.push(new BboxElement(
                    "MembraneReceptor",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        var g = jqSvg.group({
                            transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")",
                        });

                        var data = "M9.9-10.5c-1.4-1.9-2.3,0.1-5.1,0.8C2.6-9.2,2.4-13.2,0-13.2c-2.4,0-2.4,3.5-4.8,3.5c-2.4,0-3.8-2.7-5.2-0.8l8.2,11.8v12.1c0,1,0.8,1.7,1.7,1.7c1,0,1.7-0.8,1.7-1.7V1.3L9.9-10.5z";
                        var path = jqSvg.createPath();
                        var variable = jqSvg.path(g, path, {
                            stroke: 'transparent',
                            fill: "#3BB34A",
                            strokeWidth: 8,
                            d: data,
                            transform: "scale(1.2) rotate(" + renderParams.layout.Angle + ")"
                        });

                        if (that.labelVisibility === true) {
                            jqSvg.text(g, 0, 0, renderParams.model.Name, {
                                transform: "translate(" + -that.variableWidthConstant / 2 + ", " + (that.variableHeightConstant / 2 + that.labelSize) + ")",
                                "font-size": that.labelSize
                            });
                        }

                        $(variable).attr("onmouseover", "BMA.SVGHelper.AddClass(this, 'modeldesigner-element-hover')");
                        $(variable).attr("onmouseout", "BMA.SVGHelper.RemoveClass(this, 'modeldesigner-element-hover')");

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;
                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                    },
                    function (elementX: number, elementY: number) {
                        return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                    },
                    "Membrane Receptor",
                    "images/receptor.png"));

                this.elements.push(new Element(
                    "Activator",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        if (renderParams.layout.start.Id === renderParams.layout.end.Id) {

                            var x0 = renderParams.layout.start.PositionX;
                            var y0 = renderParams.layout.start.PositionY;
                            var w = that.variableWidthConstant * 0.7;
                            var h = that.variableHeightConstant * 0.7;

                            var path = jqSvg.createPath();
                            jqSvg.path(path.move(x0, y0 - h)
                                .curveQ(x0 + w / 2, y0 - h * 1.5,
                                x0 + w, y0 - h)
                                .curveQ(x0 + w * 1.5, y0,
                                x0 + w, y0 + h)
                                .curveQ(x0 + w / 2, y0 + h * 1.5,
                                x0, y0 + h),
                                { fill: 'none', stroke: '#808080', strokeWidth: that.lineWidth + 1, "marker-end": "url(#Activator)" });

                        } else {

                            var dir = {
                                x: renderParams.layout.end.PositionX - renderParams.layout.start.PositionX,
                                y: renderParams.layout.end.PositionY - renderParams.layout.start.PositionY
                            };
                            var dirLen = Math.sqrt(dir.x * dir.x + dir.y * dir.y);

                            dir.x /= dirLen;
                            dir.y /= dirLen;

                            jqSvg.line(
                                renderParams.layout.start.PositionX + dir.x * that.relationshipBboxOffset,
                                renderParams.layout.start.PositionY + dir.y * that.relationshipBboxOffset,
                                renderParams.layout.end.PositionX - dir.x * that.relationshipBboxOffset,
                                renderParams.layout.end.PositionY - dir.y * that.relationshipBboxOffset,
                                { stroke: "#808080", strokeWidth: 2, "marker-end": "url(#Activator)" });
                        }

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;

                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        if (elementX.x !== elementY.x || elementX.y !== elementY.y) {
                            var d = Math.abs((elementY.y - elementX.y) * pointerX - (elementY.x - elementX.x) * pointerY + elementY.x * elementX.y - elementX.x * elementY.y);
                            d /= Math.sqrt(Math.pow(elementY.y - elementX.y, 2) + Math.pow(elementY.x - elementX.x, 2));
                            return d < elementX.pixelWidth;
                        } else {
                            return false;
                        }
                    },
                    "Activating Relationship",
                    "images/activate.png"));

                this.elements.push(new Element(
                    "Inhibitor",
                    function (renderParams) {
                        var jqSvg = that.svg;
                        if (jqSvg === undefined)
                            return undefined;
                        jqSvg.clear();

                        if (renderParams.layout.start.Id === renderParams.layout.end.Id) {

                            var x0 = renderParams.layout.start.PositionX;
                            var y0 = renderParams.layout.start.PositionY;
                            var w = that.variableWidthConstant * 0.7;
                            var h = that.variableHeightConstant * 0.7;

                            var path = jqSvg.createPath();
                            jqSvg.path(path.move(x0, y0 - h)
                                .curveQ(x0 + w / 2, y0 - h * 1.5,
                                x0 + w, y0 - h)
                                .curveQ(x0 + w * 1.5, y0,
                                x0 + w, y0 + h)
                                .curveQ(x0 + w / 2, y0 + h * 1.5,
                                x0, y0 + h),
                                { fill: 'none', stroke: '#808080', strokeWidth: that.lineWidth + 1, "marker-end": "url(#Inhibitor)" });

                        } else {

                            var dir = {
                                x: renderParams.layout.end.PositionX - renderParams.layout.start.PositionX,
                                y: renderParams.layout.end.PositionY - renderParams.layout.start.PositionY
                            };
                            var dirLen = Math.sqrt(dir.x * dir.x + dir.y * dir.y);

                            dir.x /= dirLen;
                            dir.y /= dirLen;

                            jqSvg.line(
                                renderParams.layout.start.PositionX + dir.x * that.relationshipBboxOffset,
                                renderParams.layout.start.PositionY + dir.y * that.relationshipBboxOffset,
                                renderParams.layout.end.PositionX - dir.x * that.relationshipBboxOffset,
                                renderParams.layout.end.PositionY - dir.y * that.relationshipBboxOffset,
                                { stroke: "#808080", strokeWidth: 2, "marker-end": "url(#Inhibitor)" });
                        }

                        var svgElem: any = $(jqSvg.toSVG()).children();
                        return <SVGElement>svgElem;

                    },
                    function (pointerX: number, pointerY: number, elementX, elementY) {
                        if (elementX.x !== elementY.x || elementX.y !== elementY.y) {
                            var d = Math.abs((elementY.y - elementX.y) * pointerX - (elementY.x - elementX.x) * pointerY + elementY.x * elementX.y - elementX.x * elementY.y);
                            d /= Math.sqrt(Math.pow(elementY.y - elementX.y, 2) + Math.pow(elementY.x - elementX.x, 2));
                            return d < elementX.pixelWidth;
                        } else {
                            return false;
                        }
                    },
                    "Inhibiting Relationship",
                    "images/inhibit.png"));
            }
        }
    }
} 