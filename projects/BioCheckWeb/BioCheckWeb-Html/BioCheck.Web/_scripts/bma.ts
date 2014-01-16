/// <reference path="jquery/jquery.d.ts" />
/// <reference path="jqueryui/jqueryui.d.ts" />
// /// <reference path="vuePlotTypes.ts" />

var svg: SVGSVGElement;
var model: Model;

window.onload = () => {
    var svgjq = $("#svgroot");
    // Indirection via <any> to stop compiler complaining :-|
    svg = <any>svgjq[0];
    //svg = <any>document.getElementById("svgroot");
    svgjq.mousedown(startDrag);
    svgjq.mousemove(doDrag);
    svgjq.mouseup(drawItemOrStopDrag);

    // onmousedown="startDrag()" onmousemove="doDrag()" onmouseup="drawItemOrStopDrag()
    $("#drawing-tools").buttonset();
    $("#drawing-tools input").click(drawingToolClick);
    $("img.draggable-button").each(function (i) {
        $(this).draggable({
            helper: null,
            cursor: getCursorUrl(this),
            delay: 300
        })
    });

    $("#design-surface").droppable({ drop: doDrop });

    $("#zoom-slider").slider({
        min: -2,
        max: 3,
        value: SvgViewBoxManager.zoomLevel,
        step: 0.1,
        // TODO - define slider args type - see http://stackoverflow.com/questions/17999653/jquery-ui-widgets-in-typescript
        slide: function (e: JQueryEventObject, ui /*: JQueryUI.SliderUIParams */) { SvgViewBoxManager.zoomLevel = ui.value; }
    });

    document.body.onmousewheel = doWheel;

    model = new Model();
};

function drawingToolClick(e: JQueryEventObject) {
    var target = <HTMLElement>e.target;
    drawingItem = ItemType[$(target).attr("data-type")];
    dragging = false;
    dragObject = null;

    // Draggable causes the cursor to be set on the body, so override that
    // here. Note that the default behaviour of jQueryUI's drop seems to be to
    // return the cursor to "auto" hence caching cursor here to reapply
    // explicitly on drop.
    document.body.style.cursor = bodyCursor = getCursorUrl(target);
}

function doWheel(e: MouseWheelEvent) {
    var x = e.clientX, y = e.clientY;
    var p = screenToSvg(x, y);
    // TODO - check for keyboard modifiers and scroll if ctrl/shift
    var zoom = $("#zoom-slider");
    // TypeScript of slider API doesn't include single arg fn, hence <any> cast
    zoom.slider("value", (<any>zoom).slider("value") + (e.wheelDelta > 0 ? 0.1 : -0.1));
    // Indirecting via the slider control automatically applies range limits
    SvgViewBoxManager.scaleAroundPoint((<any>zoom).slider("value"), p.x, p.y);
}

function startDrag(e: JQueryMouseEventObject) {
    //var sx = window.event.x, sy = window.event.y;
    //var pt = screenToSvg(sx, sy);
    //var circ = createSvgElement("circle", pt.x, pt.y);
    //circ.setAttribute("fill", "red");
    //circ.setAttribute("r", "2px");
    //svg.appendChild(circ);
    //return;

    if (e.button != 0) return;

    var src = (<any>e).originalEvent.srcElement;
    if (src && (src.tagName == "path" || src.tagName == "g")) {
        var node = src;
        while (node && node.getAttribute("class") != "object")
            node = <Element>node.parentNode;
        dragObject = <SVGGElement>node;
    }
    else
        dragObject = null;

    dragging = !dragObject;
    lastX = e.clientX; lastY = e.clientY;
}

function doDrag(e: JQueryMouseEventObject) {
    var origin = SvgViewBoxManager.origin;
    var x = e.clientX, y = e.clientY;
    var p0 = screenToSvg(lastX, lastY);
    lastX = x; lastY = y;
    var p = screenToSvg(x, y);
    var dx = p.x - p0.x, dy = p.y - p0.y;

    if (dragging) {
        origin.x -= dx; origin.y -= dy;
        SvgViewBoxManager.origin = origin;
    } else if (dragObject) {
        translateSvgElementBy(dragObject, dx, dy);
    }
}

function drawItemOrStopDrag(e: JQueryMouseEventObject) {
    if (drawingItem && !dragObject) {
        var pt = screenToSvg(e.clientX, e.clientY);
        switch (drawingItem) {
            case ItemType.Container:
                var container = addContainer(pt.x, pt.y);
                break;
            //case ItemType.Variable:
            //    var variable = addVariable(pt.x, pt.y);
            //    break;
            case ItemType.Constant:
                var constant = addConstant(pt.x, pt.y);
                break;
        }
    }

    dragging = false;
    dragObject = null;
}

function doDrop(e: JQueryEventObject, ui: JQueryUI.DroppableEventUIParam) {
    //alert("Dropped " + $(ui.draggable).attr("data-type"));
    document.body.style.cursor = bodyCursor;

    var sx = e.clientX, sy = e.clientY;
    var pt = screenToSvg(sx, sy);

    var hits = hitTest(sx, sy);
    //alert(hits.length);
    //alert(e.target.nodeName + "  " + hits.length);
    var s = "";
    for (var i = 0; i < hits.length; ++i)
        s += i + " " + (<SVGElement>hits[i].parentNode).getAttribute("class") + "\n";
    if (s != "")
        alert(s);
    var circ = createSvgElement("circle", pt.x, pt.y);
    circ.setAttribute("fill", "red");
    circ.setAttribute("r", "2px");
    svg.appendChild(circ);

    //addConstant(pt.x, pt.y);

    //var sx = e.pageX, sy = e.pageY;
    //var pt = screenToSvg(sx, sy);
    //var circ = createSvgElement("circle", pt.x, pt.y);
    //circ.setAttribute("fill", "red");
    //circ.setAttribute("r", "2px");
    //svg.appendChild(circ);
}

var dragging: boolean;
var lastX: number, lastY: number;
var dragObject: SVGGElement;
var drawingItem: ItemType;

var bodyCursor: string = "auto";

function getCursorUrl(elem: HTMLElement) {
    var type = elem.getAttribute("data-type");
    return type ? "url(_images/" + type + ".cur), pointer" : "auto";
}

function hitTest(x: number, y: number) {
    var o = $("#design-surface").offset();
    var r = svg.createSVGRect();
    r.width = r.height = 1;
    r.x = x - o.left; r.y = y - o.top;
    return svg.getIntersectionList(r, null);
}

interface Point {
    x: number;
    y: number;
}

// Assumes (and requires) that horizontal and vertical scales are the same
class SvgViewBoxManager {
    static get zoomLevel() {
        return Math.log(2000 / svg.viewBox.baseVal.width);
    }

    static set zoomLevel(v: number) {
        SvgViewBoxManager.scaleAroundPoint(v);
    }

    // TODO: use overloading rather than optional arguments because want x & y
    // to be present or absent together
    static scaleAroundPoint(zoomLevel: number, xc?: number, yc?: number) {
        // The equation used here is that
        //    (xc - xo) * s = constant
        // where xc is the offset to the zoom centre (normalised by scale),
        //       xo is the viewbox offset, and s the scale.
        // Thus, to move from scale s1 to s2 keeping xc fixed, we have:
        //    xo2 = xc - (xc - xo1) * s1 / s2
        // (and, obviously, the same for y)

        var box = svg.viewBox.baseVal;
        var xo1 = box.x, yo1 = box.y;
        var s1 = 2000 / box.width;
        var s2 = Math.exp(zoomLevel);
        // In the absence of a pointer location, zoom centre is window centre
        if (typeof xc === "undefined") {
            xc = box.width / 2 + box.x;
            yc = box.height / 2 + box.y;
        }

        var xo2 = xc - (xc - xo1) * s1 / s2;
        var yo2 = yc - (yc - yo1) * s1 / s2;

        SvgViewBoxManager.setViewBox(xo2, yo2, 2000 / s2, 1000 / s2);
    }

    static scaleAroundPointOLD(zoomLevel: number, x: number, y: number) {
        var scale = Math.exp(zoomLevel);
        var w = 1000 / scale, h = 500 / scale; // 1000 here matches the 1000 in scale() above

        var box = svg.viewBox.baseVal;
        var oldScale = 1000 / box.width;

        var xOff = (x - box.x) * oldScale / scale, yOff = (y - box.y) * oldScale / scale;

        //var xf = box.x / 2000, yf = box.y / 1000;
        //var xoff = xf * scale, yoff = yf * scale;
        var dx = xOff - x, dy = yOff - y;
        //var dx = (w - box.width) * x / 2000, dy = (h - box.height) * y / 1000; // Actual viewbox size here
        ////var dx = (w - box.width) * x / svg.viewBox.baseVal.width, dy = (h - box.height) * y / svg.viewBox.baseVal.height;
        SvgViewBoxManager.setViewBox(box.x + dx, box.y + dy, w, h);
    }

    static get origin(): Point {
        var box = svg.viewBox.baseVal;
        return { x: box.x, y: box.y };
    }

    static set origin(p: Point) {
        var box = svg.viewBox.baseVal;
        SvgViewBoxManager.setViewBox(p.x, p.y, box.width, box.height);
    }

    static moveBy(dx: number, dy: number) {
        var box = svg.viewBox.baseVal;
        SvgViewBoxManager.setViewBox(box.x + dx, box.y + dy, box.width, box.height);
    }

    private static setViewBox(x: number, y: number, w: number, h: number) {
        svg.viewBox.baseVal.x = x; svg.viewBox.baseVal.y = y;
        svg.viewBox.baseVal.width = w; svg.viewBox.baseVal.height = h;
    }
}

// The objects on display are represented as a "group" (SVG "g") with class
// "object" and which contains two sub elements: a group representing the
// graphical layout of the object and a text element giving its name. The
// first group will be a list of paths, the first of which is normally
// invisible but which can be lit up to indicate hover, selection, etc.

function createSvgElement(type: string, x: number, y: number, scale: number = 1.0) {
    var elem = <SVGElement>document.createElementNS("http://www.w3.org/2000/svg", type);
    // TODO - combine into matrix? Easier to adjust later? (And to incorporate rotation for receptor)
    var transform = "";
    if (scale != 1.0)
        transform += "scale(" + scale + "," + scale + ")";
    if (x != 0 || y != 0)
        applyNewTranslation(elem, x, y);
    if (transform.length > 0)
        elem.setAttribute("transform", transform);
    return elem;
}

function createSvgPath(data: string, color: string, x: number = 0, y: number = 0, scale: number = 1.0) {
    var elem = <SVGPathElement>createSvgElement("path", x, y, scale);
    elem.setAttribute("d", data);
    elem.setAttribute("fill", color);
    return elem;
}

function createSvgText(text: string, x: number, y: number) {
    var elem = <SVGTextElement>createSvgElement("text", x, y);
    // set colour, size & font
    elem.textContent = text;
    return elem;
}

function createSvgGroup(children: SVGElement[], x: number, y: number, scale: number = 1.0) {
    var elem = <SVGGElement>createSvgElement("g", x, y, scale);
    for (var i in children)
        elem.appendChild(children[i]);
    return elem;
}

// Requires that first element be a path, and that path be the outermost, since it's cloned to make a highlight outline
function createHighlightableSvgGroup(children: SVGElement[], x: number, y: number, scale: number = 1.0) {
    var highlightPath = <SVGPathElement>children[0].cloneNode(true);
    highlightPath.setAttribute("stroke-width", (3 / scale) + "px");
    highlightPath.setAttribute("stroke", "transparent");
    highlightPath.setAttribute("fill", "transparent");
    children.unshift(highlightPath);
    var group = createSvgGroup(children, x, y, scale);
    group.setAttribute("onmouseover", "this.childNodes[0].setAttribute('stroke', 'gray')");
    group.setAttribute("onmouseout", "this.childNodes[0].setAttribute('stroke', 'transparent')");
    group.setAttribute("pointer-events", "all");
    group.setAttribute("class", "shape");
    return group;
}

function createTopGroupAndAdd(children: SVGElement[], x: number, y: number) {
    var elem = createSvgGroup(children, x, y);
    elem.setAttribute("class", "object");
    svg.appendChild(elem);
    return elem;
}

function applyNewTranslation(elem: any, x: number, y: number) {
    var transform = svg.createSVGTransform();
    transform.setTranslate(x, y);
    elem.transform.baseVal.appendItem(transform);
}

function translateSvgElement(elem: SVGGElement, x: number, y: number) {
    // TODO - matrix manipulation instead
    var transformList = elem.transform.baseVal;
    for (var i in transformList) {
        var transform = transformList.getItem(i);
        if (transform.type == SVGTransform.SVG_TRANSFORM_TRANSLATE) {
            transform.setTranslate(x, y);
            return;
        }
    }
    // Getting here means no translation was present
    applyNewTranslation(elem, x, y);
}

function translateSvgElementBy(elem: SVGGElement, dx: number, dy: number) {
    // TODO - matrix manipulation instead
    var transformList = elem.transform.baseVal;
    for (var i = 0; i < transformList.numberOfItems; ++i) {
        var transform = transformList.getItem(i);
        if (transform.type == SVGTransform.SVG_TRANSFORM_TRANSLATE) {
            var x = transform.matrix.e;
            var y = transform.matrix.f;
            transform.setTranslate(x + dx, y + dy);
            return;
        }
    }
    // Getting here means no translation was present
    //applyNewTranslation(elem, dx, dy);
}

function addContainer(x: number, y: number) {
    var container = new Container();

    var outerPath = createSvgPath("M3.6-49.9c-26.7,0-48.3,22.4-48.3,50c0,27.6,21.6,50,48.3,50c22.8,0,41.3-22.4,41.3-50C44.9-27.5,26.4-49.9,3.6-49.9z", "#FAAF42");
    var innerPath = createSvgPath("M3.6,45.5C-16.6,45.5-33,25.1-33,0.1c0-25,16.4-45.3,36.6-45.3c20.2,0,36.6,20.3,36.6,45.3C40.2,25.1,23.8,45.5,3.6,45.5z", "#FFF");
    var graphic = createHighlightableSvgGroup([outerPath, innerPath], 0, 0, 2.5);
    graphic.setAttribute("class", "shape");
    var text = createSvgText(container.name, -100, -125); // offset...
    var elem = createTopGroupAndAdd([graphic, text], x, y);
    container.element = elem;

    model.children.push(container);
    return container;
}

function addVariable(container: Container, x: number, y: number) {
    var variable = new Variable(ItemType.Variable);
    var path = createSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#EF4137", 0, 0, 0.24);
    var graphic = createHighlightableSvgGroup([path], 0, 0, 0.36);
    graphic.setAttribute("class", "shape");
    var text = createSvgText(variable.name, 0, 50); // offset...
    variable.element = createTopGroupAndAdd([graphic, text], x, y);

    container.children.push(variable);
    return variable;
}

function addConstant(x: number, y: number) {
    var constant = new Variable(ItemType.Constant);
    var path = createSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#BBBDBF");
    var graphic = createHighlightableSvgGroup([path], 0, 0, 0.36);
    graphic.setAttribute("class", "shape");
    var text = createSvgText(constant.name, 0, 50); // offset...
    constant.element = createTopGroupAndAdd([graphic, text], x, y);
    //constant.element.setAttribute("data-object", constant);

    model.children.push(constant);
    return constant;
}

enum ItemType { Invalid, Container, Variable, Constant, Receptor, Activate, Inhibit, Model }

class Item {
    constructor(public type: ItemType) {
        this.id = getNextId();
        this.name = ItemType[type] + this.id;
    }
    id: number;
    name: string;
    element: SVGGElement;
}

class Variable extends Item {
    constructor(type: ItemType) {
        super(type);
        this.fromLinks = [];
        this.toLinks = [];
    }
    formula: string;
    parent: Container;
    fromLinks: Link[];
    toLinks: Link[];
}

class Container extends Item {
    constructor() {
        super(ItemType.Container);
        this.children = [];
    }
    children: Variable[];
}

class Link /*extends Item*/ {
    constructor(public type: ItemType) {
        /*super(type);*/
    }
    source: Variable;
    target: Variable;
}

class Model {
    constructor() {
        this.children = [];
    }

    name: string;
    children: Item[];
}

function getMaxId(node, v: number) {
    if (node.id) v = Math.max(v, node.id);
    if (node.children)
        for (var i = 0; i < node.children.length; ++i)
            v = getMaxId(node.children[i], v);
    return v;
}

function getNextId() {
    return getMaxId(model, 0) + 1;
}

function screenToSvg(x: number, y: number) {
    var screenPt = svg.createSVGPoint();
    screenPt.x = x;
    screenPt.y = y;
    var ctm = svg.getScreenCTM();
    return screenPt.matrixTransform(ctm.inverse());
}
