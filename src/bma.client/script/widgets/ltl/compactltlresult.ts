﻿/// <reference path="..\..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

(function ($) {
    $.widget("BMA.compactltlresult", {

        options: {
            status: "notstarted",
            isexpanded: false,
            steps: 10,
            ontestrequested: undefined,
            onstepschanged: undefined,
            onexpanded: undefined
        },

        _create: function () {
            this.element.empty();

            this.maindiv = $("<div></div>").appendTo(this.element);
            this._createView();
        },

        _createView: function () {
            var that = this;
            this.maindiv.empty();
            var opDiv = this.maindiv;
            switch (this.options.status) {
                case "notstarted":

                    var ul = $("<ul></ul>").addClass("button-list").addClass("LTL-test").css("margin-top", 0).appendTo(opDiv);
                    var li = $("<li></li>").addClass("action-button-small").addClass("grey").appendTo(ul);
                    var btn = $("<button>TEST </button>").appendTo(li);
                    btn.click(function () {
                        if (that.options.ontestrequested !== undefined) {
                            btn.empty();
                            that.createWaitAnim().appendTo(btn);
                            that.options.ontestrequested();
                        }
                    });


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
                        ltlresdiv.html("Simulation Found<br>" + that.options.steps + " steps<br>");
                        var ul = $("<ul></ul>").addClass("button-list").css("margin", "0 0 5px 0").appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button>OPEN </button>").appendTo(li);
                        btn.click(function () {
                            alert("Coming soon!");
                        });

                    } else {


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
                        var fr = $("<div>No Simulation Found</div>").appendTo(ltlresdiv);
                        var sr = $("<div></div>").appendTo(ltlresdiv);
                        var d = $("<div>" + that.options.steps + " steps</div>")
                            .css("display", "inline-block")
                            .appendTo(sr);
                        var box = $("<div></div>").addClass("pill-button-box").css("margin-left", 5).appendTo(sr);
                        var minusd = $("<div></div>").addClass("pill-button").width(17).css("font-size", "13.333px").appendTo(box);
                        var minusb = $("<button>-</button>").appendTo(minusd);
                        minusb.click((e) => {
                            that.options.steps--;
                            d.text(that.options.steps + " steps");
                            if (that.options.onstepschanged !== undefined) {
                                that.options.onstepschanged(that.options.steps);
                            }
                        });
                        var plusd = $("<div></div>").addClass("pill-button").width(17).css("font-size", "13.333px").appendTo(box);
                        var plusb = $("<button>+</button>").appendTo(plusd);
                        plusb.click((e) => {
                            that.options.steps++;
                            d.text(that.options.steps + " steps");
                            if (that.options.onstepschanged !== undefined) {
                                that.options.onstepschanged(that.options.steps);
                            }
                        });
                        var ul = $("<ul></ul>").addClass("button-list").css("margin-top", 5).css("margin-bottom", 5).appendTo(ltlresdiv);
                        var li = $("<li></li>").appendTo(ul);
                        var btn = $("<button>TEST AGAIN</button>").appendTo(li);
                        btn.click(function () {
                            if (that.options.ontestrequested !== undefined) {
                                that.options.ontestrequested();
                            }
                            //if (that.options.onexpanded !== undefined) {
                            //    that.options.onexpanded();
                            //}
                        });

                    } else {

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