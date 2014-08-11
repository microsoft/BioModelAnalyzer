﻿/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function($) {
    var accordion = $.widget("BMA.bmaaccordion", {
        version: "1.11.0",
        options: {
            active: 0,
            animate: {},
            collapsible: true,
            event: "click",
            position: "center",
            // callbacks
            activate: null,
            beforeActivate: null,
            showLoading: false
        },

        hideProps: {},
        showProps: {},


        _create: function () {
            this.element.addClass("accordion-container");
            var options = this.options;

            this.prevShow = this.prevHide = $();
            this.element.addClass("ui-accordion ui-widget ui-helper-reset")
                // ARIA
                .attr("role", "tablist");

            // don't allow collapsible: false and active: false / null
            if (!options.collapsible && (options.active === false || options.active == null)) {
                options.active = 0;
            }
            
            this._processPanels(); 
            // handle negative values
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

            // clean up main element
            this.element
                .removeClass("ui-accordion ui-widget ui-helper-reset")
                .removeAttr("role");

            // clean up headers
            this.headers
                .removeClass("ui-accordion-header ui-accordion-header-active ui-state-default " +
                    "ui-corner-all ui-state-active ui-state-disabled ui-corner-top")
                .removeAttr("role")
                .removeAttr("aria-expanded")
                .removeAttr("aria-selected")
                .removeAttr("aria-controls")
                .removeAttr("tabIndex")
                .removeUniqueId();

            // clean up content panels
            contents = this.headers.next()
                .removeClass("ui-helper-reset ui-widget-content ui-corner-bottom " +
                    "ui-accordion-content ui-accordion-content-active ui-state-disabled")
                .css("display", "")
                .removeAttr("role")
                .removeAttr("aria-hidden")
                .removeAttr("aria-labelledby")
                .removeUniqueId();
        },

        _processAnimation: function(context){
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
            this.hideProps[that.options.position] = "-="+distantion;
            this.showProps[that.options.position] = "+=" + distantion;
        },

        _setOption: function (key, value) {
            if (key === "active") {
                // _activate() will handle invalid values and update this.options
                this._activate(value);
                return;
            }

            if (key === "event") {
                if (this.options.event) {
                    this._off(this.headers, this.options.event);
                }
                this._setupEvents(value);
            }

            this._super(key, value);

            // setting collapsible: false while collapsed; open first panel
            if (key === "collapsible" && !value && this.options.active === false) {
                this._activate(0);
            }
            
            // #5332 - opacity doesn't cascade to positioned elements in IE
            // so we need to add the disabled class to the headers and panels
            if (key === "disabled") {
                this.element
                    .toggleClass("ui-state-disabled", !!value)
                    .attr("aria-disabled", value);
                this.headers.add(this.options.context)
                    .toggleClass("ui-state-disabled", !!value);
            }
        },

        _keydown: function (event) {
            if (event.altKey || event.ctrlKey) {
                return;
            }

            var keyCode = $.ui.keyCode,
                length = this.headers.length,
                currentIndex = this.headers.index(event.target),
                toFocus = undefined;

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
                //toFocus.focus();
                event.preventDefault();
            }
        },

        _panelKeyDown: function (event) {
            if (event.keyCode === $.ui.keyCode.UP && event.ctrlKey) {
                //$(event.currentTarget).prev().focus();
            }
        },

        refresh: function () {
            var options = this.options;
            this._processPanels();

            // was collapsed or no panel
            if ((options.active === false && options.collapsible === true) || !this.headers.length) {
                options.active = false;
                this.active = $();
                // active false only when collapsible is true
            } else if (options.active === false) {
                this._activate(0);
                // was active, but active panel is gone
            } else if (this.active.length && !$.contains(this.element[0], this.active[0])) {
                // all remaining panel are disabled
                if (this.headers.length === this.headers.find(".ui-state-disabled").length) {
                    options.active = false;
                    this.active = $();
                    // activate previous panel
                } else {
                    this._activate(Math.max(0, options.active - 1));
                }
                // was active, active panel still exists
            } else {
                // make sure active index is correct
                options.active = this.headers.index(this.active);
            }

            this._refresh();
        },

        _processPanels: function () {
            var that = this;
            var position = that.options.position;
            this.element.css(position, 0);
            this.headers = that.element.children().filter(':even');
            //this.headers
            //.addClass("ui-accordion-header ui-state-default ui-corner-all");
            var loading = that.options.showLoading;
            this.headers.each(function () {
                if (loading) {
                    $('<img src="../../images/60x60.gif">').appendTo(this).addClass("invisible");
                }
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
                        that.headers
                            .removeClass("show")
                            .addClass("only");
                        return;
                }
                that.headers.css("position", "absolute");
                that.headers.css(position, 0);
                child.css("position", "absolute");
                child.css(position, -distantion);
            })
        },

        _refresh: function () {
            var maxHeight,
                options = this.options,
                parent = this.element.parent();
            this.active = $();
            this.active.next()
                .addClass("ui-accordion-content-active")
                //.show();
            var that = this;
            this.headers
                .attr("role", "tab")
                .each(function () {
                    var header = $(this),
                        headerId = header.uniqueId().attr("id"),
                        panel = header.next(),
                        panelId = panel.uniqueId().attr("id");
                    header.attr("aria-controls", panelId);
                    panel.attr("aria-labelledby", headerId);
                })
                .next()
                    .attr("role", "tabpanel");
            
            this.headers
                .not(this.active)
                .attr({
                    "aria-selected": "false",
                    "aria-expanded": "false",
                    tabIndex: -1
                })
                .next()
                    .attr({
                        "aria-hidden": "true"
                    })
                .hide();

            // make sure at least one header is in the tab order

            if (!this.active.length) {
                this.headers.eq(0).attr("tabIndex", 0);
            } else {
                this.active.attr({
                    "aria-selected": "true",
                    "aria-expanded": "true",
                    tabIndex: 0
                })
                .next()
                    .attr({
                        "aria-hidden": "false"
                    });
            }

            this._setupEvents(options.event);
        },


        _findActive: function (selector) {
            return typeof selector === "number" ? this.options.header : $();
        },

        _setupEvents: function (event) {
            var events = {
                keydown: "_keydown"
            };
            if (event) {
                $.each(event.split(" "), function (index, eventName) {
                    events[eventName] = "_eventHandler";
                });
            }

            //this._off(this.headers.add(this.options.context));
            this._off(this.headers);
            this._on(this.headers, events);
            //this._on(this.options.context, { keydown: "_panelKeyDown" });
            this._on(this.headers.next(), { keydown: "_panelKeyDown" });
            //this._hoverable(this.headers);
            //this._focusable(this.headers);
        },

        _eventHandler: function (event) {
            var options = this.options,
                active = this.active,
                clicked = $(event.currentTarget),
                
                clickedIsActive = clicked[0] === active[0],
                collapsing = clickedIsActive && options.collapsible,
                toShow = collapsing ? $() : clicked.next(),
                toHide = active.next(),
                //toShow = collapsing ? $() : options.context,
                eventData = {
                    oldHeader: active,
                    oldPanel: toHide,
                    newHeader: clicked,//collapsing ? $() : clicked,
                    newPanel: toShow
                };
            event.preventDefault();
            if (
                // click on active header, but not collapsible
                    (clickedIsActive && !options.collapsible) ||
                // allow canceling activation
                    (this._trigger("beforeActivate", event, eventData) === false)) {
                return;
            }
            
            // when the call to ._toggle() comes after the class changes
            // it causes a very odd bug in IE 8 (see #6720)
            this.active = clickedIsActive ? $() : clicked;

            if (this.options.showLoading) {
                if (!collapsing) this._showLoading(clicked);
                else this._hideLoading(clicked);
                return;
            }


            this._toggle(eventData);

            // switch classes
            // corner classes on the previously active header stay after the animation
            active.removeClass("ui-accordion-header-active ui-state-active");

            if (!clickedIsActive) {
                clicked
                    .removeClass("ui-corner-all")
                    //.addClass("ui-accordion-header-active ui-state-active ui-corner-top");

                //clicked
                active.next()
                    .addClass("ui-accordion-content-active");
            }
        },

        _toggle: function (data) {

            var toShow = data.newPanel,
                toHide = this.prevShow.length ? this.prevShow : data.oldPanel;

            // handle activating a panel during the animation for another activation
            this.prevShow.add(this.prevHide).stop(true, true);
            this.prevShow = toShow;
            this.prevHide = toHide;

            

            if (this.options.animate && this.options.position != "center") {
                this._animate(toShow, toHide, data);
            } else {
                toHide.hide();
                toShow.show();
                //if (this.options.context.is(":hidden"))
                if (data.newHeader.next().is(":hidden"))
                    this.options.header
                        .removeClass("show")
                        .addClass("only");
                else this.options.header
                        .removeClass("only")
                        .addClass("show");
                this._toggleComplete(data);
            }

            toHide.attr({
                "aria-hidden": "true"
            });
            toHide.prev().attr("aria-selected", "false");
            // if we're switching panels, remove the old header from the tab order
            // if we're opening from collapsed state, remove the previous header from the tab order
            // if we're collapsing, then keep the collapsing header in the tab order
            if (toShow.length && toHide.length) {
                toHide.prev().attr({
                    "tabIndex": -1,
                    "aria-expanded": "false"
                });
            } else if (toShow.length) {
                this.headers.filter(function () {
                    return $(this).attr("tabIndex") === 0;
                })
                .attr("tabIndex", -1);
            }

            toShow
                .attr("aria-hidden", "false")
                .prev()
                    .attr({
                        "aria-selected": "true",
                        tabIndex: 0,
                        "aria-expanded": "true"
                    });
        },



        _showLoading: function (clicked) {
            clicked.animate({ width: "+=60px" });
            clicked.children().filter(".invisible").show();
        },

        _hideLoading: function (clicked) {
            clicked.animate({ width: "-=60px" });
            clicked.children().filter(".invisible").hide();
        },


        _animate: function (toShow, toHide, data) {

            var total, easing, duration,
                that = this,
                adjust = 0,
                down = toShow.length &&
                    (!toHide.length || (toShow.index() < toHide.index())),
                animate = this.options.animate || {},
                options = down && animate.down || animate,
                complete = function () {
                    that._toggleComplete(data);
                };

            if (typeof options === "number") {
                duration = options;
            }
            if (typeof options === "string") {
                easing = options;
            }
            // fall back from options to animation in case of partial down settings
            easing = easing || options.easing || animate.easing;
            duration = duration || options.duration || animate.duration;
            var that = this;

            if (!toShow.length) {
                that._processAnimation(toHide);
                that.element.animate(that.hideProps, duration, easing, function () {
                    
                    that._toggleComplete(data, "", "");
                    toHide.hide();
                });
                return;
            }

            if (!toHide.length) {
                toShow.show();
                that._processAnimation(toShow);
                that.element.animate(that.showProps, duration, easing, that._toggleComplete(data, "", ""));
                return ;
            }
            toHide.hide();
            toShow.show();
        },

        _toggleComplete: function (data, classadd, classremove) {
            var toHide = data.oldPanel;

            toHide
                .removeClass("ui-accordion-content-active")
                .prev()
                    .removeClass("ui-corner-top")
                    .addClass("ui-corner-all");

            // Work around for rendering bug in IE (#5421)
            if (toHide.length) {
                toHide.parent()[0].className = toHide.parent()[0].className;
            }
            this._trigger("activate", null, data);
        }
    });
}(jQuery));

interface JQuery {
    bmaaccordion(): JQuery;
    bmaaccordion(settings: Object): JQuery;
}