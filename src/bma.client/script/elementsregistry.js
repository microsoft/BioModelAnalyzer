﻿var BMA;
(function (BMA) {
    (function (Elements) {
        var Element = (function () {
            function Element(type, renderToSvg, description, iconUrl) {
                this.type = type;
                this.renderToSvg = renderToSvg;
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
            return Element;
        })();
        Elements.Element = Element;

        var ElementsRegistry = (function () {
            function ElementsRegistry() {
                this.elements = [];

                this.elements.push(new Element("Container", function (model, layout) {
                    return undefined;
                }, "Cell", "images/container.png"));

                this.elements.push(new Element("Constant", function (model, layout) {
                    return this.CreateSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#EF4137");
                }, "Extracellural Protein", "images/constant.png"));

                this.elements.push(new Element("Default", function (model, layout) {
                    return this.CreateSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#BBBDBF");
                }, "Intracellural Protein", "images/variable.png"));

                this.elements.push(new Element("MembraneReceptor", function (model, layout) {
                    return this.CreateSvgPath("M9.9-10.5c-1.4-1.9-2.3,0.1-5.1,0.8C2.6-9.2,2.4-13.2,0-13.2c-2.4,0-2.4,3.5-4.8,3.5c-2.4,0-3.8-2.7-5.2-0.8l8.2,11.8v12.1c0,1,0.8,1.7,1.7,1.7c1,0,1.7-0.8,1.7-1.7V1.3L9.9-10.5z", "#3BB34A");
                }, "Membrane Receptor", "images/receptor.png"));

                this.elements.push(new Element("Activator", function (model, layout) {
                    return undefined;
                }, "Activating Relationship", "images/activate.png"));

                this.elements.push(new Element("Inhibitor", function (model, layout) {
                    return undefined;
                }, "Inhibiting Relationship", "images/inhibit.png"));
            }
            ElementsRegistry.prototype.CreateSvgElement = function (type, x, y, scale) {
                if (typeof scale === "undefined") { scale = 1.0; }
                var elem = document.createElementNS("http://www.w3.org/2000/svg", type);
                var transform = "";
                if (scale != 1.0)
                    transform += "scale(" + scale + "," + scale + ")";
                if (x != 0 || y != 0)
                    applyNewTranslation(elem, x, y);
                if (transform.length > 0)
                    elem.setAttribute("transform", transform);
                return elem;
            };

            ElementsRegistry.prototype.CreateSvgPath = function (data, color, x, y, scale) {
                if (typeof x === "undefined") { x = 0; }
                if (typeof y === "undefined") { y = 0; }
                if (typeof scale === "undefined") { scale = 1.0; }
                var elem = this.CreateSvgElement("path", x, y, scale);
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
                    if (this.elements[i].Type === type) {
                        return this.elements[i];
                    }
                }

                return undefined;
            };
            return ElementsRegistry;
        })();
        Elements.ElementsRegistry = ElementsRegistry;
    })(BMA.Elements || (BMA.Elements = {}));
    var Elements = BMA.Elements;
})(BMA || (BMA = {}));
//# sourceMappingURL=elementsregistry.js.map
