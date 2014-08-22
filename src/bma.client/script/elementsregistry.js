var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var BMA;
(function (BMA) {
    (function (Elements) {
        var Element = (function () {
            function Element(type, renderToSvg, contains, description, iconUrl) {
                this.type = type;
                this.renderToSvg = renderToSvg;
                this.contains = contains;
                this.description = description;
                this.iconUrl = iconUrl;
            }
            Object.defineProperty(Element.prototype, "Type", {
                get: function () {
                    return this.type;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Element.prototype, "RenderToSvg", {
                get: function () {
                    return this.renderToSvg;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Element.prototype, "Description", {
                get: function () {
                    return this.description;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Element.prototype, "IconURL", {
                get: function () {
                    return this.iconUrl;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(Element.prototype, "Contains", {
                get: function () {
                    return this.contains;
                },
                enumerable: true,
                configurable: true
            });
            return Element;
        })();
        Elements.Element = Element;

        var BboxElement = (function (_super) {
            __extends(BboxElement, _super);
            function BboxElement(type, renderToSvg, contains, getBbox, description, iconUrl) {
                _super.call(this, type, renderToSvg, contains, description, iconUrl);

                this.getBbox = getBbox;
            }
            Object.defineProperty(BboxElement.prototype, "GetBoundingBox", {
                get: function () {
                    return this.getBbox;
                },
                enumerable: true,
                configurable: true
            });
            return BboxElement;
        })(Element);
        Elements.BboxElement = BboxElement;

        var BorderContainerElement = (function (_super) {
            __extends(BorderContainerElement, _super);
            function BorderContainerElement(type, renderToSvg, contains, intersectsBorder, containsBBox, description, iconUrl) {
                _super.call(this, type, renderToSvg, contains, description, iconUrl);

                this.intersectsBorder = intersectsBorder;
                this.containsBBox = containsBBox;
            }
            Object.defineProperty(BorderContainerElement.prototype, "IntersectsBorder", {
                get: function () {
                    return this.intersectsBorder;
                },
                enumerable: true,
                configurable: true
            });

            Object.defineProperty(BorderContainerElement.prototype, "ContainsBBox", {
                get: function () {
                    return this.containsBBox;
                },
                enumerable: true,
                configurable: true
            });
            return BorderContainerElement;
        })(Element);
        Elements.BorderContainerElement = BorderContainerElement;

        var ElementsRegistry = (function () {
            function ElementsRegistry() {
                this.variableWidthConstant = 35;
                this.variableHeightConstant = 30;
                this.variableSizeConstant = 30;
                this.relationshipBboxOffset = 20;
                this.containerRadius = 100;
                var that = this;
                this.elements = [];

                this.elements.push(new BorderContainerElement("Container", function (jqSvg, renderParams) {
                    var g = jqSvg.group({
                        transform: "translate(" + (renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + ", " + (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep + ") scale(2.5)"
                    });

                    var innerCellData = "M3.6-49.9c-26.7,0-48.3,22.4-48.3,50c0,27.6,21.6,50,48.3,50c22.8,0,41.3-22.4,41.3-50C44.9-27.5,26.4-49.9,3.6-49.9z";
                    var innerPath = jqSvg.createPath();
                    jqSvg.path(g, innerPath, {
                        stroke: 'transparent',
                        fill: "#FAAF42",
                        d: innerCellData
                    });

                    var outeCellData = "M3.6,45.5C-16.6,45.5-33,25.1-33,0.1c0-25,16.4-45.3,36.6-45.3c20.2,0,36.6,20.3,36.6,45.3C40.2,25.1,23.8,45.5,3.6,45.5z";
                    var outerPath = jqSvg.createPath();
                    jqSvg.path(g, outerPath, {
                        stroke: 'transparent',
                        fill: "#FFF",
                        d: outeCellData
                    });

                    /*
                    //Helper bounding ellipses
                    jqSvg.ellipse((renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + 8,
                    (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep,
                    93, 113, { stroke: "red", fill: "none" });
                    jqSvg.ellipse((renderParams.layout.PositionX + 0.5) * renderParams.grid.xStep + 3,
                    (renderParams.layout.PositionY + 0.5) * renderParams.grid.yStep,
                    113, 125, { stroke: "red", fill: "none" });
                    */
                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return false;
                }, function (pointerX, pointerY, elementX, elementY) {
                    var dstXInner = Math.abs(pointerX - (elementX + 8));
                    var dstXOuter = Math.abs(pointerX - (elementX + 3));
                    var dstY = Math.abs(pointerY - elementY);
                    return Math.pow(dstXOuter / 113, 2) + Math.pow(dstY / 125, 2) < 1 && Math.pow(dstXInner / 93, 2) + Math.pow(dstY / 113, 2) > 1;
                }, function (bbox, elementX, elementY) {
                    var iscontaining = function (x, y) {
                        var dstX = Math.abs(x - (elementX + 8));
                        var dstY = Math.abs(y - elementY);
                        return Math.pow(dstX / 93, 2) + Math.pow(dstY / 113, 2) < 1;
                    };

                    return iscontaining(bbox.x, bbox.y) && iscontaining(bbox.x + bbox.width, bbox.y) && iscontaining(bbox.x, bbox.y + bbox.height) && iscontaining(bbox.x + bbox.width, bbox.y + bbox.height);
                }, "Cell", "images/container.png"));

                this.elements.push(new BboxElement("Constant", function (jqSvg, renderParams) {
                    var g = jqSvg.group({
                        transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")"
                    });

                    var data = "M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z";
                    var path = jqSvg.createPath();
                    jqSvg.path(g, path, {
                        stroke: 'transparent',
                        fill: "#BBBDBF",
                        strokeWidth: 8.3333,
                        d: data,
                        transform: "scale(0.36)"
                    });
                    jqSvg.text(g, -that.variableWidthConstant / 2, 50, renderParams.model.Name, { transform: "scale(0.4)" });

                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                }, function (elementX, elementY) {
                    return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                }, "Extracellural Protein", "images/constant.png"));

                this.elements.push(new BboxElement("Default", function (jqSvg, renderParams) {
                    var g = jqSvg.group({
                        transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")"
                    });

                    var data = "M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z";
                    var path = jqSvg.createPath();
                    jqSvg.path(g, path, {
                        stroke: 'transparent',
                        fill: "#EF4137",
                        strokeWidth: 8.3333,
                        d: data,
                        transform: "scale(0.36)"
                    });
                    jqSvg.text(g, -that.variableWidthConstant / 2, 50, renderParams.model.Name, { transform: "scale(0.4)" });

                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                }, function (elementX, elementY) {
                    return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                }, "Intracellural Protein", "images/variable.png"));

                this.elements.push(new BboxElement("MembraneReceptor", function (jqSvg, renderParams) {
                    var g = jqSvg.group({
                        transform: "translate(" + renderParams.layout.PositionX + ", " + renderParams.layout.PositionY + ")"
                    });

                    var data = "M9.9-10.5c-1.4-1.9-2.3,0.1-5.1,0.8C2.6-9.2,2.4-13.2,0-13.2c-2.4,0-2.4,3.5-4.8,3.5c-2.4,0-3.8-2.7-5.2-0.8l8.2,11.8v12.1c0,1,0.8,1.7,1.7,1.7c1,0,1.7-0.8,1.7-1.7V1.3L9.9-10.5z";
                    var path = jqSvg.createPath();
                    jqSvg.path(g, path, {
                        stroke: 'transparent',
                        fill: "#3BB34A",
                        strokeWidth: 8.3333,
                        d: data,
                        transform: "scale(1.2) rotate(" + renderParams.layout.Angle + ")"
                    });
                    jqSvg.text(g, -that.variableWidthConstant / 2, 50, renderParams.model.Name, { transform: "scale(0.4)" });

                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return Math.sqrt(Math.pow(pointerX - elementX, 2) + Math.pow(pointerY - elementY, 2)) < that.variableSizeConstant;
                }, function (elementX, elementY) {
                    return { x: elementX - that.variableWidthConstant / 2, y: elementY - that.variableHeightConstant / 2, width: that.variableWidthConstant, height: that.variableHeightConstant };
                }, "Membrane Receptor", "images/receptor.png"));

                this.elements.push(new Element("Activator", function (jqSvg, renderParams) {
                    if (renderParams.layout.start.Id === renderParams.layout.end.Id) {
                        var x0 = renderParams.layout.start.PositionX;
                        var y0 = renderParams.layout.start.PositionY;
                        var w = that.variableWidthConstant * 0.7;
                        var h = that.variableHeightConstant * 0.7;

                        var path = jqSvg.createPath();
                        jqSvg.path(path.move(x0, y0 - h).curveQ(x0 + w / 2, y0 - h * 1.5, x0 + w, y0 - h).curveQ(x0 + w * 1.5, y0, x0 + w, y0 + h).curveQ(x0 + w / 2, y0 + h * 1.5, x0, y0 + h), { fill: 'none', stroke: '#808080', strokeWidth: 2, "marker-end": "url(#Activator)" });
                    } else {
                        var dir = {
                            x: renderParams.layout.end.PositionX - renderParams.layout.start.PositionX,
                            y: renderParams.layout.end.PositionY - renderParams.layout.start.PositionY
                        };
                        var dirLen = Math.sqrt(dir.x * dir.x + dir.y * dir.y);

                        dir.x /= dirLen;
                        dir.y /= dirLen;

                        jqSvg.line(renderParams.layout.start.PositionX + dir.x * that.relationshipBboxOffset, renderParams.layout.start.PositionY + dir.y * that.relationshipBboxOffset, renderParams.layout.end.PositionX - dir.x * that.relationshipBboxOffset, renderParams.layout.end.PositionY - dir.y * that.relationshipBboxOffset, { stroke: "#808080", strokeWidth: 2, "marker-end": "url(#Activator)" });
                    }

                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return false;
                }, "Activating Relationship", "images/activate.png"));

                this.elements.push(new Element("Inhibitor", function (jqSvg, renderParams) {
                    if (renderParams.layout.start.Id === renderParams.layout.end.Id) {
                        var x0 = renderParams.layout.start.PositionX;
                        var y0 = renderParams.layout.start.PositionY;
                        var w = that.variableWidthConstant * 0.7;
                        var h = that.variableHeightConstant * 0.7;

                        var path = jqSvg.createPath();
                        jqSvg.path(path.move(x0, y0 - h).curveQ(x0 + w / 2, y0 - h * 1.5, x0 + w, y0 - h).curveQ(x0 + w * 1.5, y0, x0 + w, y0 + h).curveQ(x0 + w / 2, y0 + h * 1.5, x0, y0 + h), { fill: 'none', stroke: '#808080', strokeWidth: 2, "marker-end": "url(#Inhibitor)" });
                    } else {
                        var dir = {
                            x: renderParams.layout.end.PositionX - renderParams.layout.start.PositionX,
                            y: renderParams.layout.end.PositionY - renderParams.layout.start.PositionY
                        };
                        var dirLen = Math.sqrt(dir.x * dir.x + dir.y * dir.y);

                        dir.x /= dirLen;
                        dir.y /= dirLen;

                        jqSvg.line(renderParams.layout.start.PositionX + dir.x * that.relationshipBboxOffset, renderParams.layout.start.PositionY + dir.y * that.relationshipBboxOffset, renderParams.layout.end.PositionX - dir.x * that.relationshipBboxOffset, renderParams.layout.end.PositionY - dir.y * that.relationshipBboxOffset, { stroke: "#808080", strokeWidth: 2, "marker-end": "url(#Inhibitor)" });
                    }

                    var svgElem = $(jqSvg.toSVG()).children();
                    return svgElem;
                }, function (pointerX, pointerY, elementX, elementY) {
                    return false;
                }, "Inhibiting Relationship", "images/inhibit.png"));
            }
            ElementsRegistry.prototype.CreateSvgElement = function (type, renderParams) {
                var elem = document.createElementNS("http://www.w3.org/2000/svg", type);
                var transform = "";
                if (renderParams.x != 0 || renderParams.y != 0)
                    transform += "translate(" + renderParams.x + "," + renderParams.y + ")";
                if (renderParams.scale !== undefined && renderParams.scale != 1.0)
                    transform += "scale(" + renderParams.scale + "," + renderParams.scale + ")";
                if (transform.length > 0)
                    elem.setAttribute("transform", transform);
                return elem;
            };

            ElementsRegistry.prototype.CreateSvgPath = function (data, color, x, y, scale) {
                if (typeof x === "undefined") { x = 0; }
                if (typeof y === "undefined") { y = 0; }
                if (typeof scale === "undefined") { scale = 1.0; }
                var elem = this.CreateSvgElement("path", { x: x, y: y, scale: scale });
                elem.setAttribute("d", data);
                elem.setAttribute("fill", color);
                return elem;
            };

            Object.defineProperty(ElementsRegistry.prototype, "Elements", {
                get: function () {
                    return this.elements;
                },
                enumerable: true,
                configurable: true
            });

            ElementsRegistry.prototype.GetElementByType = function (type) {
                for (var i = 0; i < this.elements.length; i++) {
                    if (this.elements[i].Type === type)
                        return this.elements[i];
                }
                throw "the is no element for specified type";
            };
            return ElementsRegistry;
        })();
        Elements.ElementsRegistry = ElementsRegistry;
    })(BMA.Elements || (BMA.Elements = {}));
    var Elements = BMA.Elements;
})(BMA || (BMA = {}));
//# sourceMappingURL=elementsregistry.js.map
