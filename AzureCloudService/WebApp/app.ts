/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>

class Greeter {
    element: HTMLElement;
    span: HTMLElement;
    timerToken: number;

    constructor(element: HTMLElement) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }

    start() {
        this.timerToken = setInterval(() => this.span.innerHTML = new Date().toUTCString(), 500);
    }

    stop() {
        clearTimeout(this.timerToken);
    }

}

window.onload = () => {
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
    })
    

}




