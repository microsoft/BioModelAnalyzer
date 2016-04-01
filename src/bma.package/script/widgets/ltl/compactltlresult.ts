/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.compactltlresult", {

        options: {
            status: "nottested",
            isexpanded: false,
            steps: 10,
            error: undefined,
            ontestrequested: undefined,
            onstepschanged: undefined,
            onexpanded: undefined,
            onshowresultsrequested: undefined,
        },

        _create: function () {
            this.element.empty();

            this.maindiv = $("<div></div>").appendTo(this.element);
            this._createView();

            this.maindiv.click(function (e) {
                e.stopPropagation();
            });
        },

        _createView: function () {
            var that = this;
            this.maindiv.empty();
            var opDiv = this.maindiv;
            switch (this.options.status) {
                case "nottested":

                    //if (this.options.isexpanded) {

                        var ltltestdiv = $("<div></div>").addClass("LTL-test-results").addClass("default").appendTo(opDiv);
                        //var sr = $("<div></div>").appendTo(ltltestdiv);
                        if (that.options.error) {
                            var errorMessage = $("<div>" + that.options.error + "</div>").addClass("red").appendTo(ltltestdiv);
                        }
                        var d = $("<div>" + that.options.steps + " steps</div>")
                            .css("display", "inline-block").css("width", 55)
                            .appendTo(ltltestdiv);
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
                                d.text(that.options.steps + " steps");
                                if (that.options.onstepschanged !== undefined) {
                                    that.options.onstepschanged(that.options.steps);
                                }
                            }
                            if (that.options.steps == 1) {
                                minusd.addClass("testing");
                                minusb.addClass("testing");
                            }
                        });
                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusb.click((e) => {
                            that.options.steps++;
                            d.text(that.options.steps + " steps");
                            if (that.options.onstepschanged !== undefined) {
                                that.options.onstepschanged(that.options.steps);
                            }
                            minusd.removeClass("testing");
                            minusb.removeClass("testing");
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

                    //    var ltltestdiv = $("<div></div>").addClass("LTL-test-results").addClass("default").appendTo(opDiv);
                    //    var d = $("<div>" + that.options.steps + " steps</div>")
                    //        .css("display", "inline-block").css("width", 55)
                    //        .appendTo(ltltestdiv);
                    //    var box = $("<div></div>").addClass("pill-button-box").appendTo(ltltestdiv);
                    //    var minusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    //    var minusb = $("<button>-</button>").appendTo(minusd);
                    //    minusd.addClass("testing");
                    //    minusb.addClass("testing");
                    //    var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                    //    var plusb = $("<button>+</button>").appendTo(plusd);
                    //    plusd.addClass("testing");
                    //    plusb.addClass("testing");
                    //    var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 5).appendTo(ltltestdiv);
                    //    var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    //    var btn = $("<button></button>").appendTo(li);
                    //    li.addClass("spin");
                    //    that.createWaitAnim().appendTo(btn);
                    //} else {
                        var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                        var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                        var btn = $("<button></button>").appendTo(li);
                        li.addClass("spin");
                        that.createWaitAnim().appendTo(btn);
                    //}
                    break;
                case "success":

                    if (this.options.isexpanded) {

                        /*
                         <div class="LTL-test-results true">
	                        Simulation Found<br>12 steps<br>
	                        <ul class="button-list">
		                    <li><button>OPEN</button></li>
	                        </ul>
                            </div> 
                        */

                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("true").appendTo(opDiv);
                        ltlresdiv.html("True for all traces<br>");

                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div>" + that.options.steps + " steps</div>")
                            .css("display", "inline-block").css("width", 55)
                            .appendTo(sr);
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
                                d.text(that.options.steps + " steps");
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
                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusb.click((e) => {
                            that.options.steps++;
                            d.text(that.options.steps + " steps");
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

                        /*
                         <div class="LTL-test-results true">
	                        Simulation Found<br>12 steps<br>
	                        <ul class="button-list">
		                    <li><button>OPEN</button></li>
	                        </ul>
                            </div> 
                        */

                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("true").appendTo(opDiv);
                        ltlresdiv.html("True for some traces<br>");

                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div>" + that.options.steps + " steps</div>")
                            .css("display", "inline-block").css("width", 55)
                            .appendTo(sr);
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
                                d.text(that.options.steps + " steps");
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
                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusb.click((e) => {
                            that.options.steps++;
                            d.text(that.options.steps + " steps");
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
                case "fail":

                    if (this.options.isexpanded) {

                        
                        /*
                         <div class="LTL-test-results false">
	                        No Simulation Found<br>
	                        12 steps <div class="pill-button-box">
                        	<div class="pill-button"><button>-</button></div>
                        	<div class="pill-button"><button>+</button></div>
                        </div><br>
                        	<ul class="button-list">
                        		<li><button>TEST AGAIN</button></li>
                        	</ul>
                        </div> 
                         */

                        var ltlresdiv = $("<div></div>").addClass("LTL-test-results").addClass("false").appendTo(opDiv);
                        var fr = $("<div>No trace found</div>").appendTo(ltlresdiv);
                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div>" + that.options.steps + " steps</div>")
                            .css("display", "inline-block").css("width", 55)
                            .appendTo(sr);
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
                                d.text(that.options.steps + " steps");
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
                        var plusd = $("<div></div>").addClass("pill-button").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusb.click((e) => {
                            that.options.steps++;
                            d.text(that.options.steps + " steps");
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
                        });
                        var ul = $("<ul></ul>").addClass("button-list").css("margin-top", 5).css("margin-bottom", 5).appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button><img src='../images/small-cross.svg'> example</button>").addClass("LTL-sim-false").appendTo(li);
                        btn.click(function () {
                            if (that.options.onshowresultsrequested !== undefined) {
                                that.options.onshowresultsrequested();
                            }
                        });
                        //var btn = $("<button>TEST AGAIN</button>").appendTo(li);
                        //btn.click(function () {
                        //    if (that.options.ontestrequested !== undefined) {
                        //        that.options.ontestrequested();
                        //    }
                        //    //if (that.options.onexpanded !== undefined) {
                        //    //    that.options.onexpanded();
                        //    //}
                        //});

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
                    break;
            }
        },

        createWaitAnim: function () {
            /*
             <div class="spinner">
                <div class="bounce1"></div>
                <div class="bounce2"></div>
                <div class="bounce3"></div>
            </div> 
             */

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
                    needRefreshStates = true;
                    break;
                case "isexpanded":
                    needRefreshStates = true;
                    break;
                case "steps":
                    needRefreshStates = true;
                    break;
                case "error":
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