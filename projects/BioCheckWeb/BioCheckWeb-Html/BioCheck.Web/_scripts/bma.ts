/******************************************************************************

  Copyright 2014 Microsoft Corporation.  All Rights Reserved.

  Core BMA web UI

******************************************************************************/

/// <reference path="jquery/jquery.d.ts" />
/// <reference path="jqueryui/jqueryui.d.ts" />
// /// <reference path="vuePlotTypes.ts" /> maybe use later?

var svg: SVGSVGElement;

window.onload = () => {
    var svgjq = $("#svgroot");
    // Indirection via <any> to stop compiler complaining :-|
    svg = <any>svgjq[0];
    svgjq.mousedown(startDrag);
    svgjq.mousemove(doDrag);
    svgjq.mouseup(drawItemOrStopDrag);

    $("#general-tools").buttonset();
    $("#drawing-tools").buttonset();
    $("#prover-tools").buttonset();

    var b = $("#button-undo");
    b.button("option", "disabled", true);
    b.click(ModelStack.undo);
    b = $("#button-redo");
    b.button("option", "disabled", true);
    b.click(ModelStack.redo);

    b = $("#button-delete");
    b.button("option", "disabled", true);
    b.click(deleteSelectedItem);

    $("#drawing-tools input").click(drawingToolClick);
    $("img.draggable-button").each(function (i) {
        $(this).draggable({
            helper: null,
            cursor: getCursorUrl(this),
            delay: 300,
            start: function () { dragFromButton = true; }
        })
    });

    $("#design-surface").droppable({ drop: doDropFromDrawingTool });

    $("#zoom-slider").slider({
        min: -2,
        max: 3,
        value: SvgViewBoxManager.zoomLevel,
        step: 0.1,
        // TODO - define slider args type - see http://stackoverflow.com/questions/17999653/jquery-ui-widgets-in-typescript
        slide: function (e: JQueryEventObject, ui /*: JQueryUI.SliderUIParams */) { SvgViewBoxManager.zoomLevel = ui.value; }
    });
    $("#button-zoomtofit").button().click(SvgViewBoxManager.zoomToFit);

    var d = $("#dialog-variable");
    d.dialog({
        title: "Properties",
        autoOpen: false,
        modal: true,
        minWidth: 400,
        minHeight: 300,
        buttons: {
            OK: function () {
                ModelStack.dup();
                // TODO - validation
                var v: Variable = $(this).data("item");
                var t: SVGTextElement = $(this).data("text");
                t.textContent = v.name = $("#variable-name").val();
                v.range0 = $("#variable-range0").val();
                v.range1 = $("#variable-range1").val();
                v.formula = $("#variable-function").val(); // TODO - replace newline with space?
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        open: function () {
            var v : Variable = $(this).data("item");
            $("#variable-name").val(v.name);
            $("#variable-range0").val(v.range0);
            $("#variable-range1").val(v.range1);
            $("#variable-function").val(v.formula);
            var options = [$("<option>Inputs</option>")];
            for (var i = 0; i < v.toLinks.length; ++i){
                var o = $("<option>");
                o.text(v.toLinks[i].source.name);
                options.push(o);
            }
            $("#variable-variable-list").empty().append(options);
        }
    });
    $("#variable-range0").spinner();
    $("#variable-range1").spinner();
    $("#variable-function-list").change(function () {
        var f = $("#variable-function-list option:selected");
        $("#variable-function-syntax").text(f.data("syntax"));
        $("#variable-function-description").text(f.data("description"));
        $("#variable-function-insert").data("insert",f.val()).data("back",f.data("back"));
    });
    b = $("#variable-function-insert");
    b.button();
    b.click(function () {
        //alert($(this).data("insert"));
        var t = $(this);
        insertText($("#variable-function").get()[0], t.data("insert"), t.data("back"));
    });
    $("#variable-variable-list").change(function () {
        var v = $("#variable-variable-list option:selected");
        //alert(v.text());
        insertText($("#variable-function").get()[0], v.text());
    });

    d = $("#dialog-container");
    d.dialog({
        title: "Properties",
        autoOpen: false,
        modal: true,
        buttons: {
            OK: function () {
                ModelStack.dup();
                // TODO - validation
                var c: Container = $(this).data("item");
                var t: SVGTextElement = $(this).data("text");
                t.textContent = c.name = $("#container-name").val();
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        open: function () {
            var c: Container = $(this).data("item");
            $("#container-name").val(c.name);
        }
    });

    document.onmousewheel = doWheel;
    document.onkeyup = doKey; // Need to use this to trap the delete key (since keypress hides that)

    ModelStack.set(new Model());
};

function drawingToolClick(e: JQueryEventObject) {
    selectItem(null);
    var target = <HTMLElement>e.target;
    drawingItem = ItemType[$(target).attr("data-type")];
    dragMode = DragMode.None;
    dragObject = null;
    dragFromButton = false;

    // Draggable causes the cursor to be set on the body, so override that
    // here. Note that the default behaviour of jQueryUI's drop seems to be to
    // return the cursor to "auto" hence caching cursor here to reapply
    // explicitly on drop.
    document.body.style.cursor = bodyCursor = getCursorUrl(target);
}

function doWheel(e: MouseWheelEvent) {
    var x = e.clientX, y = e.clientY;
    var up = e.wheelDelta > 0;
    if (e.shiftKey) {
        // Vertical
        SvgViewBoxManager.moveBy(0, up ? -20 : 20);
    } else if (e.ctrlKey) {
        // Horizontal
        SvgViewBoxManager.moveBy(up ? -30 : 30, 0);
    } else {
        // Zoom
        var zoom = $("#zoom-slider");
        // TypeScript of slider API doesn't include single arg fn, hence <any> cast
        zoom.slider("value", (<any>zoom).slider("value") + (up ? 0.1 : -0.1));
        // Indirecting via the slider control automatically applies range limits
        var p = screenToSvg(x, y);
        SvgViewBoxManager.scaleAroundPoint((<any>zoom).slider("value"), p.x, p.y);
    }
    // Prevent the browser from handling ctrl-wheel to zoom
    // Probably overkill, and might not actually work with Chrome...
    if (e.preventDefault)
        e.preventDefault();
    if (e.stopPropagation)
        e.stopPropagation();
    return false;
}

function doKey(e: KeyboardEvent) {
    // TODO - don't do this when text box editing is happening!
    // TODO - don't do this when the mouse is down (ie, in the middle of some other operation)
    if (e.keyCode == 46 /*DEL*/)
        deleteSelectedItem();
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

    // Clear any selection until mouse up
    selectItem(null);

    dragMode = DragMode.None;

    var src = (<any>e).originalEvent.srcElement || (<any>e).originalEvent.originalTarget;
    var elem = null;
    if (src && (src.tagName == "path" || src.tagName == "g"))
        elem = getAssociatedItemElement(src);

    // Mouse down for drawing lines *doesn't* trigger a drag
    if (elem && (drawingItem == ItemType.Activate || drawingItem == ItemType.Inhibit)) {
        // Bit naughty to cast to Variable, but not accessing any specific
        // properties of Variable until after the type (which is common) has
        // been validated
        var item = <Variable>elem.item;
        if (item.type == ItemType.Variable || item.type == ItemType.Constant || item.type == ItemType.Receptor) {
            var pt = getTranslation(elem);
            drawingLineSource = item;
            drawingLine = addLink(item, drawingItem);
        }
    } else {
        dragObject = elem;
        dragMode = dragObject ? DragMode.DragStart : DragMode.Panning;
        lastX = e.clientX; lastY = e.clientY;
    }
}

function doDrag(e /*: JQueryMouseEventObject*/) {
    // e.button isn't set while mouse-moving, but e.buttons is, but only in
    // IE is seems (!?) so rely on explicit flags set on mouse down operations
    // rather than asking for the state here
    //if (e.buttons != 1) return;
    if (dragMode != DragMode.Panning && !dragObject && !dragFromButton && !drawingLine) return;

    var x = e.clientX, y = e.clientY;

    // Min movement distance before enabling dragging, to avoid jittery clicks
    if (dragMode == DragMode.DragStart) {
        if (Math.abs(x - lastX) < 5 && Math.abs(y - lastY) < 5)
            return;
        dragMode = DragMode.Dragging;
        ModelStack.dup();
    }

    var p = screenToSvg(x, y);

    if (drawingLine) {
        drawingLine.redraw(p.x, p.y);
        return;
    }

    var p0 = screenToSvg(lastX, lastY);
    lastX = x; lastY = y;
    var dx = p.x - p0.x, dy = p.y - p0.y;

    if (dragMode == DragMode.Panning) {
        // Panning the design surface, obviously
        var origin = SvgViewBoxManager.origin;
        origin.x -= dx; origin.y -= dy;
        SvgViewBoxManager.origin = origin;
    } else {
        // Determine if drag over background or on cell, etc
        // TODO - remove currently dragged object from hit testing
        var hit = getEventElementAndPart(e.originalEvent, dragObject);
        console.log(hit && hit.type);
        if (dragObject) {
            // Dragging an object
            (<Item>(<any>dragObject).item).moveBy(dx, dy);
        } else /* must be dragFromToolbar */ {
            // Participating in drag from toolbar
        }
    }
}

function drawItemOrStopDrag(e /*: JQueryMouseEventObject*/) {
    if (drawingLine) {
        // If over a suitable item, persist line, otherwise throw it away
        var deleteIt = true;
        var hit = getEventElementAndPart(e.originalEvent, drawingLine.element);
        if (hit) {
            var item = <Item>hit.elem.item;
            if (item.type == ItemType.Variable || item.type == ItemType.Constant || item.type == ItemType.Receptor) {
                // TODO - handle self-links
                var target = <Variable>item;
                // Does this link already exist? If so, don't add
                // TODO - are links in the opposite direction allowed? (Should the lines bend to make both visible?)
                var present = false;
                for (var i = 0; i < drawingLineSource.fromLinks.length; ++i) {
                    if (drawingLineSource.fromLinks[i].target && drawingLineSource.fromLinks[i].target.id == target.id) {
                        present = true;
                        break;
                    }
                }
                if (!present) {
                    // The below are already done (in addLink)
                    //drawingLine.source = drawingLineSource;
                    //drawingLineSource.fromLinks.push(drawingLine);
                    drawingLine.target = target;
                    target.toLinks.push(drawingLine);
                    // Adjust endpoint correctly (was dropped on item, needs to be a little way away)
                    drawingLine.redraw();
                    deleteIt = false;
                }
            }
        }
        if (deleteIt) {
            svg.removeChild(drawingLine.element);
            ModelStack.undo(); // Get rid of the nascent line - bit heavyweight!
            ModelStack.truncate();
        }
    } else if (drawingItem /*&& !dragObject*/) {
        var pt = screenToSvg(e.clientX, e.clientY);
        addItem(drawingItem, pt, getEventElementAndPart(e.originalEvent));
    } else if (dragObject) {
        // Verify that new placement is valid, revert to initial location if not
        var item = <Item>(<any>dragObject).item;
        var hit = getEventElementAndPart(e.originalEvent, dragObject);
        if (item.isValidNewPlacement(hit)) {
            if (item.type == ItemType.Variable || item.type == ItemType.Receptor) {
                // Reparent
            }
        } else {
            ModelStack.undo();
            ModelStack.truncate();
        }
    }

    // Reset *everything* to clear any draggy operation
    dragMode = DragMode.None;
    dragObject = null;
    dragFromButton = false;
    drawingLine = null;
    drawingLineSource = null;
}

function doDropFromDrawingTool(e /*: JQueryEventObject*/, ui: JQueryUI.DroppableEventUIParam) {
    var type = ItemType[$(ui.draggable).attr("data-type")];

    document.body.style.cursor = bodyCursor;

    var sx = e.clientX, sy = e.clientY;
    var pt = screenToSvg(sx, sy);

    // Determine if drop on background or on cell, etc
    var hit = getEventElementAndPart(e.originalEvent.originalEvent);

    // Small spot to check drop location calculation
    //var circ = createSvgElement("circle", pt.x, pt.y);
    //circ.setAttribute("fill", "red");
    //circ.setAttribute("r", "2px");
    //svg.appendChild(circ);

    addItem(type, pt, hit);
}

// TODO - incorporate drag from button instead of flagging that separately
enum DragMode { None, Panning, DragStart, Dragging }

var dragMode: DragMode;
var lastX: number, lastY: number;
var dragObject: SVGGElement;
var drawingItem: ItemType;
var dragFromButton: boolean;
var drawingLineSource: Variable;
var drawingLine: Link;
var selectedItem; // Item or Link

var bodyCursor: string = "auto";

function getCursorUrl(elem: HTMLElement) {
    var type = elem.getAttribute("data-type");
    return type ? "url(_images/" + type + ".cur), pointer" : "auto";
}

//function hitTest(x: number, y: number) {
//    var o = $("#design-surface").offset();
//    var r = svg.createSVGRect();
//    r.width = r.height = 1;
//    r.x = x - o.left; r.y = y - o.top;
//    return svg.getIntersectionList(r, null);
//}

// Different browsers have different support for SVG hit testing, hence this mess
function svgHitTest(x: number, y: number) : any {
    // IE10+ is the easiest - meElementsFromPoint returns all elements all the
    // way up to <html> - see http://ie.microsoft.com/testdrive/HTML5/HitTest/
    if (document.msElementsFromPoint)
        return document.msElementsFromPoint(x, y);
    // document.elementFromPoint allegedly returns the <svg> element on Opera
    // rather than the sub-elements, but getIntersectionList seems to work
    // http://stackoverflow.com/questions/2259613/locate-an-element-within-svg-in-opera-by-coordinates
    // This returns SVG elements, and nothing higher than that
    if (svg.getIntersectionList) {
        var o = $("#design-surface").offset();
        var r = svg.createSVGRect();
        r.width = r.height = 1;
        r.x = x - o.left; r.y = y - o.top;
        return svg.getIntersectionList(r, null);
    }
    // getIntersectionList not available in FireFox so need a fallback...
    // https://developer.mozilla.org/en-US/docs/SVG_in_Firefox
    // https://bugzilla.mozilla.org/show_bug.cgi?id=501421
    // Solution taken from http://stackoverflow.com/questions/9910008/dispatching-a-mouse-event-to-an-element-that-is-visually-behind-the-receiving
    var nodes = [];
    var visibilities: string[] = [];
    var node;
    // This stops at the SVG element
    while ((node = document.elementFromPoint(x, y)) && node != svg) {
        nodes.push(node);
        visibilities.push(node.style.visibility);
        node.style.visibility = "hidden";
    }
    for (var i = 0; i < nodes.length; ++i)
        nodes[i].style.visibility = visibilities[i];
    return nodes;
}

function getEventElementAndPartOld(e) : ElementAndPart {
    var src = e.srcElement || e.originalTarget;
    if (!src || (src.tagName != "path" && src.tagName != "g")) return null;

    var node = src;
    var itemClass = node.getAttribute("class");
    while (node && !svgHasClass(node, "object")) {
        if (!itemClass)
            itemClass = node.getAttribute("class");
        node = <Element>node.parentNode;
    }

    if (node) {
        // TODO - better split job, currently fingers crossed that the class of interest is at the start! Maybe use something other than class?
        itemClass = itemClass.split(" ")[0];
        return { elem: node, type: itemClass };
    }
    else
        return null;
}

function getEventElementAndPart(e, ignore = null, abort: ItemType[] = []): ElementAndPart {
    var nodes = svgHitTest(e.clientX, e.clientY);
    for (var i = 0; i < nodes.length; ++i) {
        var node = nodes[i];
        if (node == svg)
            return null;

        var itemClass = node.getAttribute("class");
        while (node && !svgHasClass(node, "object")) {
            if (!itemClass)
                itemClass = node.getAttribute("class");
            node = <Element>node.parentNode;
        }

        if (node) {
            if (abort.indexOf(node.item.type) >= 0)
                return null;
            if (node == ignore)
                continue;
            // TODO - better split job, currently fingers crossed that the
            // class of interest is at the start! Maybe use something other
            // than class?
            itemClass = itemClass.split(" ")[0];
            return { elem: node, type: itemClass };
        }
    }
    return null;
}

interface ElementAndPart {
    elem: any; // TODO - stronger typing here - SVG element with "item" property
    type: string;
}

class ModelStack {
    static get current() { return ModelStack.models[ModelStack.index]; }
    static get hasModel() { return ModelStack.index >= 0; }

    static get canUndo() { return ModelStack.index > 0; }
    static get canRedo() { return ModelStack.index < ModelStack.models.length - 1; }

    static undo() {
        selectItem(null);
        if (ModelStack.canUndo) {
            ModelStack.current.deleteSvg();
            --ModelStack.index;
            ModelStack.current.createSvg();
        }
        $("#button-undo").button("option", "disabled", !ModelStack.canUndo);
        $("#button-redo").button("option", "disabled", false);
    }

    static redo() {
        selectItem(null);
        if (ModelStack.canRedo) {
            ModelStack.current.deleteSvg();
            ++ModelStack.index;
            ModelStack.current.createSvg();
        }
        $("#button-undo").button("option", "disabled", false);
        $("#button-redo").button("option", "disabled", !ModelStack.canRedo);
    }

    static set(m: Model) {
        if (ModelStack.hasModel)
            ModelStack.current.deleteSvg();
        ModelStack.models = [m];
        ModelStack.index = 0;
        ModelStack.current.createSvg();
        $("#button-undo").button("option", "disabled", true);
        $("#button-redo").button("option", "disabled", true);
    }

    static dup() {
        ModelStack.truncate();
        // Because the caller may have references to items in the model, place
        // the duplicate *second* on the stack; also means the SVG resources
        // don't need to be torn down and rebuilt
        var orig = ModelStack.current;
        ModelStack.models[ModelStack.index] = orig.clone();
        ModelStack.models.push(orig);
        ++ModelStack.index;
        $("#button-undo").button("option", "disabled", false);
        $("#button-redo").button("option", "disabled", true);
    }

    static truncate() {
        ModelStack.models.length = ModelStack.index + 1;
        $("#button-redo").button("option", "disabled", true);
    }

    private static models: Model[] = [];
    private static index: number = -1;
}

interface Point {
    x: number;
    y: number;
}

// Assumes (and requires) that horizontal and vertical scales are the same
class SvgViewBoxManager {
    static get zoomLevel() {
        return SvgViewBoxManager.scaleToZoomLevel(2000 / svg.viewBox.baseVal.width);
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
        var s2 = SvgViewBoxManager.zoomLevelToScale(zoomLevel);
        // In the absence of a pointer location, zoom centre is window centre
        if (typeof xc === "undefined") {
            xc = box.width / 2 + box.x;
            yc = box.height / 2 + box.y;
        }

        var xo2 = xc - (xc - xo1) * s1 / s2;
        var yo2 = yc - (yc - yo1) * s1 / s2;

        SvgViewBoxManager.setViewBox(xo2, yo2, 2000 / s2, 1000 / s2);
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

    static zoomToFit() {
        var left = Number.MAX_VALUE, right = Number.MIN_VALUE, top = Number.MAX_VALUE, bottom = Number.MIN_VALUE;
        if (!ModelStack.current.children.length) {
            left = 0;
            right = 2000;
            top = 0;
            bottom = 1000;
        } else {
            // TODO - maybe this should be in the SVG section
            for (var i = 0; i < ModelStack.current.children.length; ++i) {
                var elem = ModelStack.current.children[i].element;
                var box = getTrueBBox(elem);
                if (box.x < left) left = box.x;
                if (box.y < top) top = box.y;
                if (box.x + box.width > right) right = box.x + box.width;
                if (box.y + box.height > bottom) bottom = box.y + box.height;
            }
        }
        var width = right - left, height = bottom - top;
        // Add a bit of a margin
        left -= 20; width += 40;
        top -= 20; height += 40;

        // Need to keep a consistent zoom level across both axes
        var sx = 2000 / width, sy = 1000 / height;
        var s = sx < sy ? sx : sy;

        // Limit to range available via the UI, and keep the UI in step
        // TODO - too much coupling
        var zoom = $("#zoom-slider");
        zoom.slider("value", SvgViewBoxManager.scaleToZoomLevel(s));

        // Reading back from the slider control automatically applies limits
        s = SvgViewBoxManager.zoomLevelToScale((<any>zoom).slider("value"));

        // Adjust offsets to centre the display
        var displayWidth = 2000 / s, displayHeight = 1000 / s;
        left += 0.5 * (width - displayWidth);
        top += 0.5 * (height - displayHeight);
        SvgViewBoxManager.setViewBox(left, top, displayWidth, displayHeight);
    }

    private static zoomLevelToScale(v: number) {
        return Math.exp(v);
    }

    private static scaleToZoomLevel(v: number) {
        return Math.log(v);
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

// Debug hackery
function drawSpot(x: number, y: number, radius: number = 2, fill: string = "red") {
    var circ = createSvgElement("circle", x, y);
    circ.setAttribute("fill", fill);
    circ.setAttribute("r", radius + "px");
    svg.appendChild(circ);
}

function createSvgPath(data: string, color: string, x: number = 0, y: number = 0, scale: number = 1.0) {
    var elem = <SVGPathElement>createSvgElement("path", x, y, scale);
    elem.setAttribute("d", data);
    elem.setAttribute("fill", color);
    return elem;
}

function createSvgText(text: string, x: number, y: number) {
    var elem = <SVGTextElement>createSvgElement("text", x, y);
    // TODO set colour, size & font
    elem.textContent = text;
    return elem;
}

function createSvgGroup(children: SVGElement[], x: number, y: number, scale: number = 1.0) {
    var elem = <SVGGElement>createSvgElement("g", x, y, scale);
    for (var i in children)
        elem.appendChild(children[i]);
    return elem;
}

// Requires that first element be a strokeless path, and that path be the
// outermost, since its stroke is manipulated to make a highlight outline
function createHighlightableSvgGroup(children: SVGElement[], x: number, y: number, scale: number = 1.0) {
    var highlightPath = <SVGPathElement>children[0];
    highlightPath.setAttribute("stroke-width", (3 / scale) + "px");
    highlightPath.setAttribute("stroke", "transparent");
    var group = createSvgGroup(children, x, y, scale);
    group.onmouseover = function (e: MouseEvent) { svgAddClass(this.firstChild, 'svg-highlight') };
    group.onmouseout = function (e: MouseEvent) { svgRemoveClass(this.firstChild, 'svg-highlight') };
    group.onmouseup = function (e: MouseEvent) { if (e.button == 0) selectItem(getAssociatedItemElement(this).item) };
    // Allow the invisible stroke to still participate in hit testing
    group.setAttribute("pointer-events", "all");
    svgAddClass(group, "shape");
    return group;
}

function createTopGroupAndAdd(children: SVGElement[], x: number, y: number) {
    var elem = createSvgGroup(children, x, y);
    svgAddClass(elem, "object");
    svg.appendChild(elem);
    return elem;
}

function getAssociatedItemElement(elem) {
    while (elem && !svgHasClass(elem, "object"))
        elem = elem.parentNode;
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

function getTranslation(elem: SVGGElement): Point {
    // TODO - matrix manipulation instead
    var transformList = elem.transform.baseVal;
    for (var i = 0; i < transformList.numberOfItems; ++i) {
        var transform = transformList.getItem(i);
        if (transform.type == SVGTransform.SVG_TRANSFORM_TRANSLATE)
            return { x: transform.matrix.e, y: transform.matrix.f };
    }
    return { x: 0, y: 0 };
}

// getBBox doesn't take transformations into account; this function looks at
// the currently applied translation (note, doesn't walk any further up the
// tree, nor does it take scale into account so only useful for top level
// objects)
function getTrueBBox(elem: SVGGElement) {
    var box = elem.getBBox();
    var tr = getTranslation(elem);
    return { x: box.x + tr.x, y: box.y + tr.y, width: box.width, height: box.height };
}

// SVG class seems to be treated as a "normal" string attribute in all but the
// most recent IE, so roll our own class manipulation

function stringInString(s: string, find: string) {
    return s && s.match(new RegExp("(\\s|^)" + find + "(\\s|$)"));
}

function svgHasClass(elem: SVGStylable, c: string) {
    return stringInString(elem.className.baseVal, c);
}

function svgAddClass(elem: SVGStylable, c: string) {
    var s = elem.className.baseVal;
    if (!s)
        elem.className.baseVal = c;
    else if (!stringInString(s, c))
        elem.className.baseVal = s + " " + c;
}

function svgRemoveClass(elem: SVGStylable, c: string) {
    // TODO - there's probably a better way to coalesce spaces than 2 REs
    var s = elem.className.baseVal.replace(new RegExp("(\\s+|^)" + c + "(\\s+|$)"), " ");
    s = s.replace(/\s+/g, " "); // TODO - remove trailing space
    if (s == " ")
        s = "";
    elem.className.baseVal = s;
}

// Item derived object, or Link - could do with tidying this up a bit
function selectItem(item) {
    // Only select item if not drawing
    if (drawingItem || drawingLine)
        return;
    if (selectedItem) {
        if (selectedItem.type == ItemType.Activate || selectedItem.type==ItemType.Inhibit)
            svgRemoveClass(selectedItem.element.firstChild.firstChild, "svg-line-selected");
        else
            svgRemoveClass(selectedItem.element.firstChild.firstChild, "svg-selected");
        selectedItem = null;
    }
    if (item) {
        selectedItem = item;
        if (selectedItem.type == ItemType.Activate || selectedItem.type == ItemType.Inhibit)
            svgAddClass(selectedItem.element.firstChild.firstChild, "svg-line-selected");
        else
            svgAddClass(selectedItem.element.firstChild.firstChild, "svg-selected");
    }
    $("#button-delete").button("option", "disabled", !selectedItem);
}

function deleteSelectedItem() {
    if (selectedItem) {
        ModelStack.dup();
        selectedItem.remove();
        // Don't use selectItem, because setting the class on the previously selected item will fail now that it's been removed
        selectedItem = null;
        $("#button-delete").button("option", "disabled", true);
    }
}

function addItem(type: ItemType, pt: Point, elemAndPart: ElementAndPart): Item {
    switch (type) {
        case ItemType.Container:
            if (Container.isValidPlacement(elemAndPart))
                return addContainer(pt.x, pt.y);
            break;
        case ItemType.Variable:
            if (Variable.isValidPlacement(type, elemAndPart))
                return addVariable(pt.x, pt.y, elemAndPart.elem.item);
            break;
        case ItemType.Constant:
            if (Variable.isValidPlacement(type, elemAndPart))
                return addConstant(pt.x, pt.y);
            break;
        case ItemType.Receptor:
            if (Variable.isValidPlacement(type, elemAndPart))
                return addReceptor(pt.x, pt.y, elemAndPart.elem.item);
            break;
    }
    return null;
}

function addContainer(x: number, y: number) {
    ModelStack.dup();
    var container = new Container(x, y);
    ModelStack.current.children.push(container);
    container.createSvgElement();
    return container;
}

function addVariable(x: number, y: number, container: Container) {
    ModelStack.dup();
    var variable = new Variable(ItemType.Variable, x, y);
    variable.parent = container;
    container.children.push(variable);
    variable.createSvgElement();
    return variable;
}

function addConstant(x: number, y: number) {
    ModelStack.dup();
    var constant = new Variable(ItemType.Constant, x, y);
    ModelStack.current.children.push(constant);
    constant.createSvgElement();
    return constant;
}

function addReceptor(x: number, y: number, container: Container) {
    ModelStack.dup();
    var receptor = new Variable(ItemType.Receptor, x, y);
    container.children.push(receptor);
    receptor.parent = container;
    receptor.createSvgElement();
    return receptor;
}

function addLink(source: Variable, type: ItemType) {
    ModelStack.dup();
    var link = new Link(type);
    link.source = source;
    source.fromLinks.push(link);
    link.createSvgElement();
    return link;
}

enum ItemType { Invalid, Container, Variable, Constant, Receptor, Activate, Inhibit, Model }

class Item {
    constructor(public type: ItemType, public x: number, public y: number) {
        this.id = getNextId();
        this.name = ItemType[type] + this.id;
    }

    remove() {
        this.deleteSvgElement();
    }

    // Create the SVG structures that correspond to this particular item
    createSvgElement() {
        // This should never occur - ideally would be an abstract method
        throw new Error("Cannot create SVG element for item " + this.name + " (" + this.id + ")");
    }

    // The above will not create links when called on containers or
    // variables - the reason drawing is split into two phases is to
    // ensure that links are at the top of the z-order. This method
    // creates the links...
    createSvgLinkElements() {
    }

    // Delete all the SVG structures corresponding to this item
    deleteSvgElement() {
        if (this.element) {
            this.element.parentNode.removeChild(this.element);
            this.element = null;
        }
    }

    // Make a deep copy of this model data, excluding any SVG structures
    clone(variableMap: { [original: number]: Variable }, linkList: Link[]): Item {
        // This should never occur - ideally would be an abstract method
        throw new Error("Cannot clone item " + this.name + " (" + this.id + ")");
    }

    // Move to absolute coordinates on the screen
    moveTo(x: number, y: number): void {
        moveBy(x - this.x, y - this.y);
    }

    // Move to relative coordinate
    moveBy(dx: number, dy: number): void {
        this.x += dx;
        this.y += dy;
        translateSvgElement(this.element, this.x, this.y);
    }

    // Check if this item can be placed at the locaton specified
    // TODO - snapping?
    isValidNewPlacement(elemAndPart: ElementAndPart) {
        return false;
    }

    id: number;
    name: string;
    parent: Item; // Null parent = model itself
    element: SVGGElement;
}

class Variable extends Item {
    constructor(type: ItemType, x: number, y: number) {
        super(type, x, y);
        this.range0 = 0;
        this.range1 = 1;
        this.formula = "avg(pos) - avg(neg)";
        this.fromLinks = [];
        this.toLinks = [];
    }

    remove() {
        while (this.fromLinks.length)
            this.fromLinks[0].remove();
        while (this.toLinks.length)
            this.toLinks[0].remove();
       if (this.parent)
            removeItem(this.parent.children, this);
        super.remove();
    }

    createSvgElement() {
        var path: SVGPathElement;
        var scale: number;
        switch(this.type){
            case ItemType.Variable:
                path = createSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#EF4137");
                scale = 0.36;
                break;
            case ItemType.Constant:
                path = createSvgPath("M27.3,43.4l-2.2-0.8c-12-4.4-19.3-11.5-20-19.7c0-0.5-0.1-0.9-0.1-1.4c-5.4-2.6-9-7.3-10.5-12.3c-0.6-2-0.9-4.1-0.8-6.3c-4.7-1.7-8.2-4.7-10.3-8.2c-2.1-3.4-3.2-8.1-2.1-13.4c-6.7-1.8-12.5-4.3-15.9-5.8l-7.4,19.9l26.7,7.9L-17,9.1l-32.8-9.7l11.9-32l3,1.5c3.9,1.9,10.8,4.9,18.1,6.9c1.9-4,5.1-8.1,10-12.1c10.8-8.9,19.7-8.1,23.8-3.4c3.5,4,3.6,11.6-4.2,18.7c-6.3,5.7-16.2,5.7-25.7,3.8c-0.6,3.2-0.2,6.2,1.4,8.9c1.3,2.2,3.4,4,6.3,5.3C-3.4-8.3,0.7-13.2,8-16c15.9-6.1,19.9,0.2,20.7,2.2c2.1,5.2-2.4,11.8-10.1,15C11.5,4.2,5.1,5-0.3,4.4C-0.2,5.5,0,6.5,0.3,7.5c0.9,3.2,3,6.1,6.2,8C8,12.1,11,9,15,6.7C25,1,32.2,1.6,35.7,4.2c2.3,1.7,3.3,4.3,2.7,7.1c-1.1,5.3-7.6,9.7-17.5,11.8c-3.6,0.8-6.8,0.8-9.7,0.4c1,4.9,6,9.5,13.9,12.8l7.4-10.7l17.4,10.1l-3,5.1l-12.6-7.4L27.3,43.4L27.3,43.4z M12.1,17.5c2.2,0.3,4.8,0.3,7.6-0.3c9.4-2,12.6-5.6,12.9-7.2c0.1-0.4,0-0.7-0.4-1c-1.4-1-6.2-1.7-14.1,2.9C15.2,13.4,13.2,15.4,12.1,17.5L12.1,17.5z M0.6-1.5C5-1,10.3-1.7,16.3-4.2c5.4-2.3,7.4-6,6.9-7.3c-0.4-1-4.3-2.2-13,1.1C5-8.5,2-5.1,0.6-1.5L0.6-1.5z M-10.8-22.8c7.8,1.4,15.2,1.3,19.5-2.6c4.7-4.2,5.4-8.4,3.7-10.4c-2.1-2.5-8.3-1.9-15.5,4.1C-6.5-28.9-9.1-25.9-10.8-22.8L-10.8-22.8z", "#BBBDBF");
                scale = 0.4;
                break;
            case ItemType.Receptor:
                path = createSvgPath("M9.9-10.5c-1.4-1.9-2.3,0.1-5.1,0.8C2.6-9.2,2.4-13.2,0-13.2c-2.4,0-2.4,3.5-4.8,3.5c-2.4,0-3.8-2.7-5.2-0.8l8.2,11.8v12.1c0,1,0.8,1.7,1.7,1.7c1,0,1.7-0.8,1.7-1.7V1.3L9.9-10.5z", "#3BB34A");
                scale = 1.0;
                break;
            default:
                throw new Error("Invalid variable type: " + this.type);
        }
        var graphic = createHighlightableSvgGroup([path], 0, 0, scale);
        svgAddClass(graphic, "shape");
        var text = createSvgText(this.name, 0, 50); // offset...
        text.onmouseup = (e: MouseEvent) => { if (e.button == 0) this.showPropertyPage(text); };
        var elem = createTopGroupAndAdd([graphic, text], this.x, this.y);
        this.element = elem;
        (<any>elem).item = this;
    }

    createSvgLinkElements() {
        for (var i = 0; i < this.toLinks.length; ++i)
            this.toLinks[i].createSvgElement();
    }

    deleteSvgElement() {
        for (var i = 0; i < this.toLinks.length; ++i)
            this.toLinks[i].deleteSvgElement();
        super.deleteSvgElement();
    }

    clone(variableMap: { [original: number]: Variable }, linkList: Link[]): Variable {
        var v = new Variable(this.type, this.x, this.y);
        v.id = this.id;
        v.name = this.name;
        v.range0 = this.range0;
        v.range1 = this.range1;
        v.formula = this.formula;
        for (var i = 0; i < this.fromLinks.length; ++i) {
            var link = new Link(this.fromLinks[i].type);
            link.source = v;
            link.target = this.fromLinks[i].target; // This is the OLD target, will be patched up later
            v.fromLinks.push(link);
            linkList.push(link);
        }
        variableMap[this.id] = v;
        return v;
    }

    moveBy(dx: number, dy: number): void {
        super.moveBy(dx, dy);
        for (var i = 0; i < this.fromLinks.length; ++i)
            this.fromLinks[i].redraw();
        for (var i = 0; i < this.toLinks.length; ++i)
            this.toLinks[i].redraw();
    }

    isValidNewPlacement(elemAndPart: ElementAndPart) {
        return Variable.isValidPlacement(this.type, elemAndPart);
    }

    static isValidPlacement(type: ItemType, elemAndPart: ElementAndPart) {
        switch (type) {
            case ItemType.Variable:
                return elemAndPart && elemAndPart.type == "cell-inner";
            case ItemType.Constant:
                return elemAndPart == null;
            case ItemType.Receptor:
                return elemAndPart && elemAndPart.type == "cell-outer";
        }
    }

    // Takes the text element directly to avoid having to find it within the UI elements
    private showPropertyPage(text: SVGTextElement) {
        //alert("Property page: name " + this.name + " (" + text.textContent + ")");
        var d = $("#dialog-variable");
        // Rather clunky way to pass data to the dialog box - is there a better way?
        d.data("item", this);
        d.data("text", text);
        d.dialog("open");
    }

    formula: string;
    range0: number;
    range1: number;
    parent: Container;
    fromLinks: Link[]; // Links coming from this element - the link's source points to this
    toLinks: Link[]; // Links coming to this element - the link's target points to this
}

class Container extends Item {
    constructor(x: number, y: number) {
        super(ItemType.Container, x, y);
        this.children = [];
    }

    remove() {
        while (this.children.length)
            this.children[0].remove();
        removeItem(ModelStack.current.children, this);
        super.remove();
    }

    createSvgElement() {
        var outerPath = createSvgPath("M3.6-49.9c-26.7,0-48.3,22.4-48.3,50c0,27.6,21.6,50,48.3,50c22.8,0,41.3-22.4,41.3-50C44.9-27.5,26.4-49.9,3.6-49.9z", "#FAAF42");
        svgAddClass(outerPath, "cell-outer");
        var innerPath = createSvgPath("M3.6,45.5C-16.6,45.5-33,25.1-33,0.1c0-25,16.4-45.3,36.6-45.3c20.2,0,36.6,20.3,36.6,45.3C40.2,25.1,23.8,45.5,3.6,45.5z", "#FFF");
        svgAddClass(innerPath, "cell-inner");
        var graphic = createHighlightableSvgGroup([outerPath, innerPath], 0, 0, 2.5);
        svgAddClass(graphic, "shape");
        var text = createSvgText(this.name, -100, -125); // offset...
        text.onmouseup = (e: MouseEvent) => { if (e.button == 0) this.showPropertyPage(text); };
        var elem = createTopGroupAndAdd([graphic, text], this.x, this.y);
        this.element = elem;
        (<any>elem).item = this;
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].createSvgElement();
    }

    createSvgLinkElements() {
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].createSvgLinkElements();
    }

    deleteSvgElement() {
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].deleteSvgElement();
        super.deleteSvgElement();
    }

    clone(variableMap: { [original: number]: Variable }, linkList: Link[]): Container {
        var c = new Container(this.x, this.y);
        c.id = this.id;
        c.name = this.name;
        for (var i = 0; i < this.children.length; ++i) {
            var cc = this.children[i].clone(variableMap, linkList);
            c.children.push(cc);
            cc.parent = c;
        }
        return c;
    }

    moveBy(dx: number, dy: number): void {
        super.moveBy(dx, dy);
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].moveBy(dx, dy);
    }

    isValidNewPlacement(elemAndPart: ElementAndPart) {
        return Container.isValidPlacement(elemAndPart);
    }

    static isValidPlacement(elemAndPart: ElementAndPart) {
        return elemAndPart == null;
    }

    // Takes the text element directly to avoid having to find it within the UI elements
    private showPropertyPage(text: SVGTextElement) {
        var d = $("#dialog-container");
        // Rather clunky way to pass data to the dialog box - is there a better way?
        d.data("item", this);
        d.data("text", text);
        d.dialog("open");
    }

    children: Variable[];
    size: number;
}

class Link {
    constructor(public type: ItemType) {
    }

    remove() {
        this.deleteSvgElement();
        removeItem(this.source.fromLinks, this);
        if (this.target)
            removeItem(this.target.toLinks, this);
    }

    createSvgElement() {
        // TODO - currently links are implemented as lines, differently to the
        // other objects (this is primarily because they change length, which
        // makes a stroked type easier to deal with than outline paths.
        // However, this does mean that selection and highlighting have to be
        // reimplemented differently for links... Maybe reconcile?
        var line;
        var v = this.source;
        var x1 = v.x, y1 = v.y;
        if (this.target && this.target.id == this.source.id) {
            line = createSvgElement("path", 0, 0);
            line.setAttribute("d", "M" + x1 + "," + (y1 + 20) + "a30,30 270 1 0 0,-40");
        } else {
            line = createSvgElement("line", 0, 0);
            if (this.target)
                v = this.target;
            var x2 = v.x, y2 = v.y;
            // Adjust line to be a short distance from the actual centre
            var dx = x2 - x1, dy = y2 - y1;
            var len = Math.sqrt(dx * dx + dy * dy);
            dx *= 30 / len;
            dy *= 30 / len;
            line.x1.baseVal.value = x1 + dx;
            line.y1.baseVal.value = y1 + dy;
            line.x2.baseVal.value = x2 - dx;
            line.y2.baseVal.value = y2 - dy;
        }
        svgAddClass(line, "svg-line");
        // Ack - serious problem with IE - marker-ended lines don't draw properly
        // http://connect.microsoft.com/IE/feedback/details/801938/dynamically-updated-svg-path-with-a-marker-end-does-not-update
        // http://connect.microsoft.com/IE/feedback/details/781964/svg-marker-is-not-updated-when-the-svg-element-is-moved-using-the-dom
        // http://stackoverflow.com/questions/17654578/svg-marker-does-not-work-in-ie9-10 suggests
        // remove and re-add as solution, which is why there's a lot of that in line drawing here
        // TODO - class for end markers?
        line.setAttribute("marker-end", this.type == ItemType.Activate ? "url('#link-activate')" : "url('#link-inhibit')");
        // TODO - highlight
        var graphic = createSvgGroup([line], 0, 0, 1);
        graphic.onmouseover = function (e: MouseEvent) { svgAddClass(this.firstChild, 'svg-line-highlight') };
        graphic.onmouseout = function (e: MouseEvent) { svgRemoveClass(this.firstChild, 'svg-line-highlight') };
        graphic.onmouseup = function (e: MouseEvent) { if (e.button == 0) selectItem(getAssociatedItemElement(this).item) };
        svgAddClass(graphic, "shape");
        var elem = createTopGroupAndAdd([graphic], 0, 0);
        this.element = elem;
        (<any>elem).item = this;
    }

    deleteSvgElement() {
        if (this.element) {
            this.element.parentNode.removeChild(this.element);
            this.element = null;
        }
    }

    // Reposition the line from source to target; if there is no target, use
    // the supplied coordinates (eg, used in dragging when drawing the line in
    // the first place). If the target is the source, draw a loop
    redraw(x2: number = 0, y2: number = 0) {
        // TODO - code dupe with line creation
        var parent = this.element.firstChild; // The group containing the line/path object
        var line = <any>parent.firstChild;
        parent.removeChild(line);
        var v = this.source;
        var x1 = v.x, y1 = v.y;
        v = this.target;
        if (v && v.id == this.source.id) {
            if (line.nodeName == "line") {
                var newLine = createSvgElement("path", 0, 0);
                Link.copyAttributes(line, newLine);
                line = newLine;            }
            line.setAttribute("d", "M" + x1 + "," + (y1 + 20) + " a30, 30 270 1 0 0, -40");
        } else {
            if (v) {
                x2 = v.x; y2 = v.y;
            }
            // This should never happen in practice - a line might end up
            // being converted from a straight line to a curve when being
            // drawn for the first time, but a curve will never be converted
            // to a straight segment - but leave the code here for
            // completeness.)
            if (line.nodeName != "line") {
                var newLine = createSvgElement("line", 0, 0);
                Link.copyAttributes(line, newLine);
                line = newLine;
            }
            // Adjust line to be a short distance from the actual centre
            var dx = x2 - x1, dy = y2 - y1;
            var len = Math.sqrt(dx * dx + dy * dy);
            dx *= 30 / len;
            dy *= 30 / len;
            line.x1.baseVal.value = x1 + dx;
            line.y1.baseVal.value = y1 + dy;
            line.x2.baseVal.value = x2 - dx;
            line.y2.baseVal.value = y2 - dy;
        }
        parent.appendChild(line);
    }

    private static copyAttributes(src: SVGElement, dst: SVGElement) {
        var attrs = ["class", "marker-end"];
        for (var i = 0; i < attrs.length; ++i)
            dst.setAttribute(attrs[i], src.getAttribute(attrs[i]));
    }

    source: Variable;
    target: Variable;
    element: SVGGElement;
}

class Model {
    constructor() {
        this.children = [];
    }

    createSvg() {
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].createSvgElement();
        for (i = 0; i < this.children.length; ++i)
            this.children[i].createSvgLinkElements();
    }

    deleteSvg() {
        for (var i = 0; i < this.children.length; ++i)
            this.children[i].deleteSvgElement();
    }

    clone(): Model {
        var m = new Model();
        m.name = this.name;
        var variableMap: { [original: number]: Variable } = {};
        var linkList: Link[] = [];
        for (var i = 0; i < this.children.length; ++i)
            m.children.push(this.children[i].clone(variableMap, linkList));
        // Patch up backward links
        for (i = 0; i < linkList.length; ++i) {
            var link = linkList[i];
            link.target = variableMap[link.target.id];
            link.target.toLinks.push(link);
        }
        return m;
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
    return getMaxId(ModelStack.current, 0) + 1;
}

function screenToSvg(x: number, y: number) {
    var screenPt = svg.createSVGPoint();
    screenPt.x = x;
    screenPt.y = y;
    var ctm = svg.getScreenCTM();
    return screenPt.matrixTransform(ctm.inverse());
}

function removeItem(array: any[], item) {
    var index = array.indexOf(item);
    if (index >= 0)
        array.splice(index, 1);
}

function insertText(text: HTMLTextAreaElement, s: string, stepBack: number = 0) {
    var s0 = text.selectionStart, s1 = text.selectionEnd;
    var orig = text.value;
    text.value = orig.substring(0, s0) + s + orig.substring(s1);
    text.selectionStart = text.selectionEnd = s0 + s.length - stepBack;
}
