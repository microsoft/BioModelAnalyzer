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
function run_clicked() {
    var txt = "run clicked\n";
    console.log(txt);
    $("#log").append(txt);
    $.ajax({
        type: "POST",
        url: "api/Hello",
        data: JSON.stringify("42"),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            $("#log").append(msg);
        },
        error: function (e) {
            $("#log").append("error: " + e);
        }
    });
}
//# sourceMappingURL=app.js.map