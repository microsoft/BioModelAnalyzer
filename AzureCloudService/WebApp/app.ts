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

class SimulationInput {
    public pgm: string[];
    public condition: string;
    constructor(pgm: string[], condition: string) {
        this.pgm = pgm;
        this.condition = condition;
    }
}

class SimulationOutput {
    public output: string;
}

function run_clicked() {
    var txt = "run clicked\n";
    console.log(txt);
    $("#log").append(txt);

    var i = new SimulationInput(["x", "y"], "condition");
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
    })
    

}




