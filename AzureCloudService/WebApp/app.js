/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
var Greeter = (function () {
    function Greeter(element) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }
    Greeter.prototype.start = function () {
        var _this = this;
        this.timerToken = setInterval(function () { return _this.span.innerHTML = new Date().toUTCString(); }, 500);
    };
    Greeter.prototype.stop = function () {
        clearTimeout(this.timerToken);
    };
    return Greeter;
})();
window.onload = function () {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
};
var SimulationInput = (function () {
    function SimulationInput(pgm, condition) {
        this.pgm = pgm;
        this.condition = condition;
    }
    return SimulationInput;
})();
var SimulationOutput = (function () {
    function SimulationOutput() {
    }
    return SimulationOutput;
})();
function run_clicked() {
    var txt = "run clicked\n";
    console.log(txt);
    var func = $('input[name=function]:checked').val();
    if (func == "simulate") {
        console.log('func=simulate=' + func);
        var s_cond = $('input[name=s_condition]:checked').val();
        console.log('s_cond:' + s_cond);
    }
    var i = new SimulationInput(["x", "y"], "func");
    $.ajax({
        type: "POST",
        url: "api/Simulation",
        data: JSON.stringify(i),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var o = msg.Output;
            $("#log").append(o);
        },
        error: function (e) {
            $("#log").append("error: " + e);
        }
    });
}
//# sourceMappingURL=app.js.map