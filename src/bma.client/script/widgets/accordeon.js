(function ($) {
    var accordion = $.widget("BMA.bmaaccordion", {
        version: "1.11.0",
        options: {
            active: 0,
            animate: {},
            collapsible: true,
            event: "click",
            position: "center",
            activate: null,
            beforeActivate: null,
            contentLoaded: { ind: "", val: true }
        },
        hideProps: {},
        showProps: {},
        _create: function () {
            this.element.addClass("bma-accordion-container");
            var options = this.options;

            this.prevShow = this.prevHide = $();
            this.element.addClass("ui-accordion ui-widget ui-helper-reset").attr("role", "tablist");

            if (!options.collapsible && (options.active === false || options.active == null)) {
                options.active = 0;
            }

            this._processPanels();

            if (options.active < 0) {
                options.active += this.headers.length;
            }
            this._refresh();
        },
        _getCreateEventData: function () {
            return {
                header: this.active,
                panel: !this.active.length ? $() : this.active.next()
            };
        },
        _destroy: function () {
            var contents;

            this.element.removeClass("ui-accordion ui-widget ui-helper-reset").removeAttr("role");

            this.headers.removeClass("ui-accordion-header ui-accordion-header-active ui-state-default " + "ui-corner-all ui-state-active ui-state-disabled ui-corner-top").removeAttr("role").removeAttr("aria-expanded").removeAttr("aria-selected").removeAttr("aria-controls").removeAttr("tabIndex").removeUniqueId();

            contents = this.headers.next().removeClass("ui-helper-reset ui-widget-content ui-corner-bottom " + "ui-accordion-content ui-accordion-content-active ui-state-disabled").css("display", "").removeAttr("role").removeAttr("aria-hidden").removeAttr("aria-labelledby").removeUniqueId();
        },
        _processAnimation: function (context) {
            var that = this;
            var position = that.options.position;
            var distantion = 0;

            switch (position) {
                case "left":
                case "right":
                    distantion = context.width();
                    break;
                case "top":
                case "bottom":
                    distantion = context.height();
                    break;
                case "center":
                    return;
            }
            this.hideProps = {};
            this.showProps = {};
            this.hideProps[that.options.position] = "-=" + distantion;
            this.showProps[that.options.position] = "+=" + distantion;

            context.css("z-index", 1);

            this.headers.next().not(context).css("z-index", 0);
        },
        _setOption: function (key, value) {
            var that = this;
            if (key === "active") {
                this._activate(value);
                return;
            }

            if (key === "event") {
                if (this.options.event) {
                    this._off(this.headers, this.options.event);
                }
                value.ind;
                this._setupEvents(value);
            }

            if (key == "contentLoaded") {
                var isthatActive;
                if (typeof value.ind === "number") {
                    isthatActive = that.headers[value.ind][0] === that.active[0];
                    that.loadingList[value.ind] = value.val;
                } else if (typeof value.ind === "string") {
                    isthatActive = $(value.ind)[0] === that.active[0];
                    that.loadingList[that.headers.index($(value.ind))] = value.val;
                } else if (typeof value.ind === "JQuery") {
                    isthatActive = value.ind[0] === that.active[0];
                    that.loadingList[that.headers.index(value.ind)] = value.val;
                }

                if (value.val) {
                    if (isthatActive) {
                        that._hideLoading(that.active);
                        var eventData = {
                            oldHeader: $(),
                            oldPanel: $(),
                            newHeader: that.active,
                            newPanel: that.active.next()
                        };
                        that._toggle(eventData);
                    }
                }
            }

            if (key == "position") {
                switch (value) {
                    case "left":
                    case "right":
                    case "top":
                    case "bottom":
                    case "center":
                        that.options.position = value;
                }
                return;
            }

            if (key === "collapsible" && !value && this.options.active === false) {
                this._activate(0);
            }

            if (key === "disabled") {
                this.element.toggleClass("ui-state-disabled", !!value).attr("aria-disabled", value);
                this.headers.add(this.options.context).toggleClass("ui-state-disabled", !!value);
            }

            this._super(key, value);
        },
        _keydown: function (event) {
            if (event.altKey || event.ctrlKey) {
                return;
            }

            var keyCode = $.ui.keyCode, length = this.headers.length, currentIndex = this.headers.index(event.target), toFocus = undefined;

            switch (event.keyCode) {
                case keyCode.RIGHT:
                case keyCode.DOWN:
                    toFocus = this.headers[(currentIndex + 1) % length];
                    break;
                case keyCode.LEFT:
                case keyCode.UP:
                    toFocus = this.headers[(currentIndex - 1 + length) % length];
                    break;
                case keyCode.SPACE:
                case keyCode.ENTER:
                    this._eventHandler(event);
                    break;
                case keyCode.HOME:
                    toFocus = this.headers[0];
                    break;
                case keyCode.END:
                    toFocus = this.headers[length - 1];
                    break;
            }

            if (toFocus !== undefined) {
                $(event.target).attr("tabIndex", -1);
                $(toFocus).attr("tabIndex", 0);

                event.preventDefault();
            }
        },
        _panelKeyDown: function (event) {
            if (event.keyCode === $.ui.keyCode.UP && event.ctrlKey) {
            }
        },
        refresh: function () {
            var options = this.options;
            this._processPanels();

            if ((options.active === false && options.collapsible === true) || !this.headers.length) {
                options.active = false;
                this.active = $();
            } else if (options.active === false) {
                this._activate(0);
            } else if (this.active.length && !$.contains(this.element[0], this.active[0])) {
                if (this.headers.length === this.headers.find(".ui-state-disabled").length) {
                    options.active = false;
                    this.active = $();
                } else {
                    this._activate(Math.max(0, options.active - 1));
                }
            } else {
                options.active = this.headers.index(this.active);
            }

            this._refresh();
        },
        _processPanels: function () {
            var that = this;
            var position = that.options.position;
            this.element.css(position, 0);
            this.headers = that.element.children().filter(':even');
            this.headers.addClass("bma-accordion-header");

            this.loadingList = [];
            this.headers.each(function (ind) {
                that.loadingList[ind] = true;
                var child = $(this).next();

                var distantion = 0;
                switch (position) {
                    case "left":
                    case "right":
                        distantion = child.width();
                        break;
                    case "top":
                    case "bottom":
                        distantion = child.height();
                        break;
                    case "center":
                        that.headers.removeClass("visibility-header-with-table-show").addClass("visibility-header-only");
                        that.headers.next().hide();
                        return;
                }
                that.headers.css("position", "absolute");
                that.headers.css(position, 0);
                child.css("position", "absolute");
                child.css(position, -distantion);
            });
        },
        _refresh: function () {
            var maxHeight, options = this.options, parent = this.element.parent();
            this.active = $();
            this.active.next().addClass("ui-accordion-content-active");

            var that = this;
            this.headers.attr("role", "tab").each(function () {
                var header = $(this), headerId = header.uniqueId().attr("id"), panel = header.next(), panelId = panel.uniqueId().attr("id");
                header.attr("aria-controls", panelId);
                panel.attr("aria-labelledby", headerId);
            }).next().attr("role", "tabpanel");

            this.headers.not(this.active).attr({
                "aria-selected": "false",
                "aria-expanded": "false",
                tabIndex: -1
            }).next().attr({
                "aria-hidden": "true"
            });

            if (!this.active.length) {
                this.headers.eq(0).attr("tabIndex", 0);
            } else {
                this.active.attr({
                    "aria-selected": "true",
                    "aria-expanded": "true",
                    tabIndex: 0
                }).next().attr({
                    "aria-hidden": "false"
                });
            }

            this._setupEvents(options.event);
        },
        _findActive: function (selector) {
            return typeof selector === "number" ? this.headers.eq(selector) : $();
        },
        _setupEvents: function (event) {
            var events = {
                keydown: "_keydown"
            };
            if (event) {
                $.each(event.split(" "), function (index, eventName) {
                    events[eventName] = "eventHandler";
                });
            }

            this._off(this.headers);
            this._on(this.headers, events);

            this._on(this.headers.next(), { keydown: "_panelKeyDown" });
        },
        eventHandler: function (event) {
            var options = this.options, active = this.active, clicked = $(event.currentTarget).eq(0), clickedIsActive = clicked[0] === active[0], collapsing = clickedIsActive && options.collapsible, toShow = collapsing ? $() : clicked.next(), toHide = this.loadingList[this.headers.index(this.active)] ? active.next() : $(), eventData = {
                oldHeader: active,
                oldPanel: toHide,
                newHeader: clicked,
                newPanel: toShow
            };
            event.preventDefault();
            if ((clickedIsActive && !options.collapsible) || (this._trigger("beforeActivate", event, eventData) === false)) {
                return;
            }
            eventData.newHeader.css("z-index", 2);
            this.headers.not(eventData.newHeader).css("z-index", 0);

            this.active = clickedIsActive ? $() : clicked;

            if (!this.loadingList[this.headers.index(clicked)]) {
                eventData.newPanel = $();
                if (!collapsing) {
                    this._hideLoading(this.headers.not(clicked));
                    this._toggle(eventData);
                    this._showLoading(clicked);
                } else {
                    this._hideLoading(clicked);
                }
                return;
            }

            this._toggle(eventData);

            active.removeClass("ui-accordion-header-active ui-state-active");

            if (!clickedIsActive) {
                clicked.removeClass("ui-corner-all").addClass("ui-accordion-header-active  ui-corner-top");

                clicked.addClass("ui-accordion-content-active");
            }
        },
        _toggle: function (data) {
            var toShow = data.newPanel, toHide = this.prevShow.length ? this.prevShow : data.oldPanel;
            var that = this;

            this.prevShow.add(this.prevHide).stop(true, true);
            this.prevShow = toShow;
            this.prevHide = toHide;

            if (that.options.animate && that.options.position != "center") {
                that._animate(toShow, toHide, data);
            } else {
                toHide.hide();
                toShow.show();

                if (data.newHeader.next().is(":hidden")) {
                    data.newHeader.removeClass("visibility-header-with-table-show").removeClass("visibility-shadow").addClass("visibility-header-only");
                } else {
                    data.newHeader.removeClass("visibility-header-only").addClass("visibility-header-with-table-show").addClass("visibility-shadow");
                }
                that._toggleComplete(data);
            }

            toHide.attr({
                "aria-hidden": "true"
            });
            toHide.prev().attr("aria-selected", "false");

            if (toShow.length && toHide.length) {
                toHide.prev().attr({
                    "tabIndex": -1,
                    "aria-expanded": "false"
                });
            } else if (toShow.length) {
                this.headers.filter(function () {
                    return $(this).attr("tabIndex") === 0;
                }).attr("tabIndex", -1);
            }

            toShow.attr("aria-hidden", "false").prev().attr({
                "aria-selected": "true",
                tabIndex: 0,
                "aria-expanded": "true"
            });
        },
        _showLoading: function (clicked) {
            clicked.animate({ width: "+=60px" });
            $('<img src="../../images/60x60.gif">').appendTo(clicked).addClass("loading");
        },
        _hideLoading: function (toHide) {
            toHide.each(function () {
                var load = $(this).children().filter(".loading");
                if (load.length) {
                    load.detach();
                    $(this).animate({ width: "-=60px" });
                }
            });
        },
        _animate: function (toShow, toHide, data) {
            var total, easing, duration, that = this, adjust = 0, down = toShow.length && (!toHide.length || (toShow.index() < toHide.index())), animate = this.options.animate || {}, options = down && animate.down || animate, complete = function () {
                that._toggleComplete(data);
            };

            if (typeof options === "number") {
                duration = options;
            }
            if (typeof options === "string") {
                easing = options;
            }

            easing = easing || options.easing || animate.easing;
            duration = duration || options.duration || animate.duration;
            var that = this;

            this._hideLoading(this.headers.not(this.active));

            if (!toShow.length) {
                that._processAnimation(toHide);
                that.element.animate(that.hideProps, duration, easing, complete);
                return;
            }

            if (!toHide.length) {
                that._processAnimation(toShow);
                that.element.animate(that.showProps, duration, easing, complete);
                return;
            }

            toHide.css("z-index", 0);
            toShow.css("z-index", 1);
            this._toggleComplete(data);
        },
        _toggleComplete: function (data) {
            var toHide = data.oldHeader;
            var toShow = data.newHeader;

            data.newPanel.css("z-index", 1);

            toHide.removeClass("ui-accordion-content-active").prev().removeClass("ui-corner-top").addClass("ui-corner-all");

            if (toHide.length) {
                toHide.parent()[0].className = toHide.parent()[0].className;
            }
            this._trigger("activate", null, data);
        }
    });
}(jQuery));
