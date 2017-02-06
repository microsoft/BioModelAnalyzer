// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.compactltlresult", {

        options: {
            status: "nottested",
            isexpanded: false,
            steps: 10,
            message: undefined,
            maxsteps: 999,
            ontestrequested: undefined,
            onstepschanged: undefined,
            onexpanded: undefined,
            onshowresultsrequested: undefined,
            oncancelrequest: undefined,
        },

        _create: function () {
            this.element.empty();

            this.maindiv = $("<div></div>").appendTo(this.element);
            this._createView();

            this.maindiv.click(function (e) {
                e.stopPropagation();
            });
        },

        _createExpandedView: function () {
        },

        _createView: function () {
            var that = this;
            this.maindiv.empty();
            var opDiv = this.maindiv;
            switch (this.options.status) {
                case "nottested":

                    //if (this.options.isexpanded) {

                        var ltltestdiv = $("<div></div>").addClass("LTL-test-results").addClass("default").appendTo(opDiv);
                        if (that.options.message) {
                            if (that.options.message == "Server Error") {
                                var errorMessage = $("<div>" + that.options.message + "</div>").addClass("red").addClass("errorMessage").appendTo(ltltestdiv);
                            } else {
                                var errorMessage = $("<div>Error</div>").addClass("red").addClass("errorMessage").appendTo(ltltestdiv);
                                errorMessage.tooltip({
                                    content: function () {
                                        //var text = $('<div></div>').addClass('operators-info');
                                        var message = $("<div>" + that.options.message + "</div>").addClass("tooltip-red");
                                        return message;
                                    },
                                    show: null,
                                    hide: false,
                                    items: "div.errorMessage",
                                    close: function (event, ui) {
                                        errorMessage.data("ui-tooltip").liveRegion.children().remove();
                                    },
                                });
                            }
                        }
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(ltltestdiv);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;
                            
                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }
                            }

                            if (that.options.steps == 1) {
                                minusd.addClass("testing");
                                minusb.addClass("testing");
                            }

                            if (that.options.steps == that.options.maxsteps) {
                                plusd.addClass("testing");
                                plusb.addClass("testing");
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(ltltestdiv);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                plusd.removeClass("testing");
                                plusb.removeClass("testing");
                            }
                            if (that.options.steps == 1) {
                                minusd.addClass("testing");
                                minusb.addClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }

                            if (that.options.steps == that.options.maxsteps) {
                                plusd.addClass("testing");
                                plusb.addClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 5).appendTo(ltltestdiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                        var btn = $("<button>TEST </button>").css("margin-top", "0px").appendTo(li);
                        btn.click(function () {
                            if (that.options.ontestrequested !== undefined) {
                                that.options.ontestrequested();
                                minusd.addClass("testing");
                                plusd.addClass("testing");
                                plusb.addClass("testing").unbind("click");
                                minusb.addClass("testing").unbind("click");
                            }
                        });

                    //} else {

                    //    var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                    //    var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    //    var btn = $("<button>TEST </button>").appendTo(li);
                    //    btn.click(function () {
                    //        that.options.isexpanded = true;
                    //        that._createView();
                    //        if (that.options.onexpanded !== undefined) {
                    //            that.options.onexpanded();
                    //        }
                    //    });
                    //}
                    break;
                case "processing":
                    //if (this.options.isexpanded) {

                        var ltltestdiv = $("<div></div>").addClass("LTL-test-results").addClass("default").appendTo(opDiv);
                        var d = $("<div>" + that.options.steps + " steps</div>").addClass("number-of-steps")
                            .appendTo(ltltestdiv);
                        var box = $("<div></div>").addClass("pill-button-box").appendTo(ltltestdiv);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);
                        minusd.addClass("testing");
                        minusb.addClass("testing");
                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusd.addClass("testing");
                        plusb.addClass("testing");
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 5).appendTo(ltltestdiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                        var btn = $("<button></button>").appendTo(li);
                        li.addClass("spin");
                        that.createWaitAnim().appendTo(btn);
                    //} else {
                        ////var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        ////var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                        ////var btn = $("<button></button>").appendTo(li);
                        ////li.addClass("spin");
                        ////that.createWaitAnim().appendTo(btn);
                    //}
                        break;
                case "processinglra":
                    var ltltestdiv = $("<div></div>").addClass("LTL-test-results").css("width", 150).addClass("default").appendTo(opDiv);
                    var d = $("<div>" + that.options.steps + " steps</div>").addClass("number-of-steps")
                        .appendTo(ltltestdiv);
                    var box = $("<div></div>").addClass("pill-button-box").appendTo(ltltestdiv);
                    var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    var minusb = $("<button>-</button>").appendTo(minusd);
                    minusd.addClass("testing");
                    minusb.addClass("testing");
                    var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    var plusb = $("<button>+</button>").appendTo(plusd);
                    plusd.addClass("testing");
                    plusb.addClass("testing");
                    var message = $("<div></div>").text(that.options.message).addClass("grey").css("white-space", "nowrap").appendTo(ltltestdiv);
                    var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(ltltestdiv);
                    var li2 = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    var cancelBtn = $("<button>Cancel</button>").addClass("cancel-button").appendTo(li2).click(function () {
                        if (that.options.oncancelrequest !== undefined) {
                            that.options.oncancelrequest();
                        } else {
                            that.options.status = "nottested";
                            that._createView();
                        }
                    });
                    var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    var btn = $("<button></button>").appendTo(li);
                    li.addClass("spin");
                    that.createWaitAnim().appendTo(btn);
                    break;
                case "success":

                    if (this.options.isexpanded) {

                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("true").appendTo(opDiv);
                        ltlresdiv.html("True for ALL traces<br>");

                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(sr);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;

                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }


                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing").addClass("green");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }

                                plusd.removeClass("testing");
                                plusb.removeClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing").addClass("green");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").css("margin", "5px 0 5px 0").appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button><img src='../images/small-tick.svg'> example </button>").addClass("LTL-sim-true").appendTo(li);
                        btn.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested();
                            }
                        });

                    } else {

                        var ltlresdiv = $("<div>" + that.options.steps + " steps</div>").addClass("closed-results").addClass("true").appendTo(opDiv);//
                        var br = $("<br>").appendTo(opDiv);
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("green").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            that.options.isexpanded = true;
                            that._createView();
                            if (that.options.onexpanded !== undefined) {
                                that.options.onexpanded();
                            }
                        });
                    }

                    break;
                case "partialsuccess":
                    if (this.options.isexpanded) {
                        
                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("true").appendTo(opDiv);
                        ltlresdiv.html("True for SOME traces<br>");

                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(sr);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;

                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }


                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing").addClass("green");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }

                               plusd.removeClass("testing");
                               plusb.removeClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing").addClass("green");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").css("margin", "5px 0 5px 0").appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button><img src='../images/small-tick.svg'> example </button>").addClass("LTL-sim-true").appendTo(li);
                        btn.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested();
                            }
                        });

                    } else {

                        var ltlresdiv = $("<div>" + that.options.steps + " steps</div>").addClass("closed-results").addClass("true").appendTo(opDiv);//
                        var br = $("<br>").appendTo(opDiv);
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("green").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            that.options.isexpanded = true;
                            that._createView();
                            if (that.options.onexpanded !== undefined) {
                                that.options.onexpanded();
                            }
                        });
                    }
                    break;
                case "partialsuccesspartialfail":
                    if (this.options.isexpanded) {
                        
                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("some").appendTo(opDiv);
                        ltlresdiv.html("True/False for SOME traces<br>");

                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(sr);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;

                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }


                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }

                                plusd.removeClass("testing");
                                plusb.removeClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").css("margin-top", 5).appendTo(ltlresdiv);
                        var liOk = $("<li></li>").css("margin-bottom", 5).appendTo(ul);
                        var btnOk = $("<button><img src='../images/small-tick.svg'>  example </button>").addClass("LTL-sim-true").appendTo(liOk);
                        btnOk.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested(true);
                            }
                        });

                        var liX = $("<li></li>").css("margin-bottom", 5).appendTo(ul);
                        var btnX = $("<button><img src='../images/small-cross.svg'> example </button>").addClass("LTL-sim-false").appendTo(liX);
                        btnX.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested(false);
                            }
                        });

                    } else {

                        var ltlresdiv = $("<div>" + that.options.steps + " steps</div>").addClass("closed-results").appendTo(opDiv);//
                        var br = $("<br>").appendTo(opDiv);
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            that.options.isexpanded = true;
                            that._createView();
                            if (that.options.onexpanded !== undefined) {
                                that.options.onexpanded();
                            }
                        });
                    }
                    break;

                case "partialfail":

                    if (this.options.isexpanded) {

                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("false").appendTo(opDiv);
                        var fr = $("<div>False for SOME traces</div>").appendTo(ltlresdiv);
                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(sr);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;

                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }


                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing").addClass("red");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }

                                plusd.removeClass("testing");
                                plusb.removeClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing").addClass("red");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").css("margin-top", 5).css("margin-bottom", 5).appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button><img src='../images/small-cross.svg'> example</button>").addClass("LTL-sim-false").appendTo(li);
                        btn.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested();
                            }
                        });

                    } else {
                        var ltlresdiv = $("<div>" + that.options.steps + " steps</div>").addClass("closed-results").addClass("false").appendTo(opDiv);//
                        var br = $("<br>").appendTo(opDiv);
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("red").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            that.options.isexpanded = true;
                            that._createView();
                            if (that.options.onexpanded !== undefined) {
                                that.options.onexpanded();
                            }
                        });
                    }
                    break;

                case "fail":

                    if (this.options.isexpanded) {
                        
                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("false").appendTo(opDiv);
                        var fr = $("<div>False for ALL traces</div>").appendTo(ltlresdiv);
                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div></div>").addClass("number-of-steps").appendTo(sr);
                        var input = $("<input></input>").attr("type", "text").attr("value", that.options.steps).appendTo(d);
                        input.after("steps");

                        input.bind("change", function () {
                            this.value = this.value.replace(/\D+/g, "");
                            var parsed = parseFloat(this.value);
                            if (isNaN(parsed)) this.value = that.options.steps;
                            else if (parsed > that.options.maxsteps) this.value = that.options.maxsteps;
                            else if (parsed < 1) this.value = 1;

                            if (that.options.steps !== parseFloat(this.value)) {
                                that.options.steps = parseFloat(this.value);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }


                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                            }
                        });

                        var box = $("<div></div>").addClass("pill-button-box").appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);

                        if (that.options.steps == 1) {
                            minusd.addClass("testing");
                            minusb.addClass("testing").addClass("red");
                        }

                        minusb.click((e) => {
                            if (that.options.steps > 1) {
                                that.options.steps--;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                               plusd.removeClass("testing");
                               plusb.removeClass("testing");
                            }
                        });

                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);

                        if (that.options.steps == that.options.maxsteps) {
                            plusd.addClass("testing");
                            plusb.addClass("testing").addClass("red");
                        }

                        plusb.click((e) => {
                            if (that.options.steps < that.options.maxsteps) {
                                that.options.steps++;
                                input.val(that.options.steps);
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }

                                that.options.status = "nottested";
                                that._createView();
                                if (that.options.onexpanded !== undefined) {
                                    that.options.onexpanded();
                                }
                                minusd.removeClass("testing");
                                minusb.removeClass("testing");
                            }
                        });

                        var ul = $("<ul></ul>").addClass("button-list").css("margin-top", 5).css("margin-bottom", 5).appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button><img src='../images/small-cross.svg'> example</button>").addClass("LTL-sim-false").appendTo(li);
                        btn.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested();
                            }
                        });

                    } else {
                        var ltlresdiv = $("<div>" + that.options.steps + " steps</div>").addClass("closed-results").addClass("false").appendTo(opDiv);//
                        var br = $("<br>").appendTo(opDiv);
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("red").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            that.options.isexpanded = true;
                            that._createView();
                            if (that.options.onexpanded !== undefined) {
                                that.options.onexpanded();
                            }
                        });
                    }
                    break;

                default:
                    var ltltestdiv = $("<div></div>").addClass("LTL-test-results").addClass("default").appendTo(opDiv);
                    var d = $("<div>" + that.options.steps + " steps</div>").addClass("number-of-steps")
                        .appendTo(ltltestdiv);
                    var box = $("<div></div>").addClass("pill-button-box").appendTo(ltltestdiv);
                    var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    var minusb = $("<button>-</button>").appendTo(minusd);
                    minusd.addClass("testing");
                    minusb.addClass("testing");
                    var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    var plusb = $("<button>+</button>").appendTo(plusd);
                    plusd.addClass("testing");
                    plusb.addClass("testing");
                    var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 5).appendTo(ltltestdiv);
                    var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    var btn = $("<button></button>").appendTo(li);
                    li.addClass("spin");
                    that.createWaitAnim().appendTo(btn);
                    break;
            }
        },

        createWaitAnim: function () {
            
            var anim = $("<div></div>").addClass("spinner");
            $("<div></div>").addClass("bounce1").appendTo(anim);
            $("<div></div>").addClass("bounce2").appendTo(anim);
            $("<div></div>").addClass("bounce3").appendTo(anim);
            return anim;
        },

        _setOption: function (key, value) {
            var that = this;
            var needRefreshStates = false;
            switch (key) {
                case "status":
                    if (that.options.status !== value)
                        needRefreshStates = true;
                    break;
                case "isexpanded":
                    if (that.options.isexpanded !== value)
                        needRefreshStates = true;
                    break;
                case "steps":
                    if (that.options.steps !== value) {
                        if (that.options.onstepschanged !== undefined) {
                            that.options.onstepschanged(value);
                        }
                        needRefreshStates = true;
                    }
                    break;
                case "message":
                    if (that.options.message !== value)
                        needRefreshStates = true;
                    break;
                default:
                    break;
            }
            this._super(key, value);

            if (needRefreshStates) {
                this._createView();
            }
        },

        _setOptions: function (options) {
            this._super(options);
        },

        _destroy: function () {
            this.element.empty();
        }


    });
} (jQuery));

interface JQuery {
    compactltlresult(): JQuery;
    compactltlresult(settings: Object): JQuery;
    compactltlresult(optionLiteral: string, optionName: string): any;
    compactltlresult(optionLiteral: string, optionName: string, optionValue: any): JQuery;
} 
