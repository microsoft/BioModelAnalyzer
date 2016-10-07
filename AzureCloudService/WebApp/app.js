/// <reference path="Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="Scripts\typings\jqueryui\jqueryui.d.ts"/>
// Simulation Model

var ProgramsInput = (function () {
    function ProgramsInput(pgm) {
        this.pgm = pgm;
    }
    return ProgramsInput;
})();

var ProgramsCellInput = (function () {
    function ProgramsCellInput(pgm, cell) {
        this.pgm = pgm;
        this.cell = cell;
    }
    return ProgramsCellInput;
})();

var ProgramsOutpus = (function () {
    function ProgramsOutput() {
    }
    return ProgramsOutput;
})();

var ProgramsCellCondInput = (function () {
    function ProgramsCellCondInput(pgm, cell, cond) {
        this.pgm = pgm;
        this.cell = cell;
        this.cond = cond;
    }
    return ProgramsCellCondInput;
})();

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
        var pgm = $('#pgm').val();
        var s_cond = $('input[name=s_condition]:checked').val();
        console.log('pgm:' + pgm);
        console.log('s_cond: ' + s_cond);
        // SI: is pgm:string[]?
        var i = new SimulationInput(pgm, s_cond);
        $.ajax({
            type: "POST",
            url: "api/Simulation",
            data: JSON.stringify(i),
            contentType: "application/json",
            dataType: "json",
            success: function (msg) {
                var o = msg.Output;
                $("#log").val(o);
            },
            error: function (e) {
                $("#log").val("error: " + e);
            }
        });
    }
    $("#output_area").show();
}
// SI: old default code. 
window.onload = function () {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
};
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


function addOptionsToSelectFromList(select, list) {
    var elem = document.getElementById(select);
    if (elem == null) {
        console.log("Could not find select" + select);
        return;
    }
    if (list.length == 0) {
        console.log("Trying to add 0 options to a select");
        return;
    }

    var htmlOptionList = '<option value="select option">Select Option</option>';
    for (var i = 0 ; i < list.length ; i++) {
        var val = list[i];
        htmlOptionList += '<option value="' + val + '">';
        htmlOptionList += val + '</option>';
    }
    elem.innerHTML = htmlOptionList;
}

function addRadioFromList(wrapperId, radioId, valList) {
    console.log("addRadioFromList(" + wrapperId + ',' + radioId + ',...)');
    var elem = document.getElementById(wrapperId);
    if (elem == null) {
        console.log("Could not find element " + wrapperId);
        return;
    }
    if (valList.length == 0) {
        console.log("Trying to create a radio button with no elements");
        return;
    }

    var htmlRadioList = '';
    for (var i = 0 ; i < valList.length ; i++) {
        var val = valList[i];
        htmlRadioList += '<label for="' + val + '">' + val + '</label>';
        htmlRadioList += '<input type="radio" name="' + radioId + '" id="' + val + '" value="' + val + '" />';
    }
    console.log(htmlRadioList);
    elem.innerHTML = htmlRadioList;
    elem.style.display = "block";
}

function addRadioButtons(selectId, radioButtonWrapId, radioButtonId) {
    console.log("addRadioButtons accodring to " + selectId + " inside " + radioButtonWrapId);
    var sel = document.getElementById(selectId);
    var choice = sel.options[sel.selectedIndex].text;
    console.log("The choice is " + choice);
    if (choice != "none" && choice != "Select Option") {
        var radio = document.getElementById(radioButtonId);
        addRadioFromList(radioButtonWrapId, radioButtonId, ['a', 'b', 'c']);
    } else {
        $('#' + radioButtonWrapId).hide();
    }
}

function addDropDownMenuWithListOfPrograms(selectId) {
    console.log("addDropDownMenuWithListOfPrograms(" + selectId + ")");

    var pgm = $('#pgm').val();
    var pinput = new ProgramsInput(pgm);
    console.log("The contents of program is: " + pgm);
    console.log("The input sent to ajax is: " + JSON.stringify(pinput));
    $.ajax({ 
        type: "POST",
        url: "api/Programs",
        data: JSON.stringify(pinput),
        contentType: "application/json",
        dataType: "json",
        success: function (msg) {
            var o = msg.Output;
            var os = JSON.stringify(o);
            if ("Error:" == os.substr(1, 6)) {
                console.log("There was an error in the program:" + os.substr(8,1000));
                $('#output_area').show();
                $('#log').val(os.substr(1,os.length-2));
            }
            console.log("Return:" + os);
            console.log("Substr:" + os.substr(1, 6));
        },
        error: function (e) {
            console.log("Error:" + JSON.stringify(e));
        }
    });
    addOptionsToSelectFromList('simulation_select', ['a', 'b', 'c', 'd']);
}


function mainChoiceChange() {
    console.log("mainChoiceChange()");
    var mainChoice = $('input[name="function"]:checked').val();
    if (mainChoice == 'simulate') {
        addDropDownMenuWithListOfPrograms('simulation_select');
        $('#ctrl_simulate_subctrl').show();
        $('#simulate_br').hide();
        $('#ctrl_abnrml_simulate_subctrl').hide();
    }
    else if (mainChoice == 'overlap') {
        $('#ctrl_simulate_subctrl').hide();
        $('#simulate_br').show();
        $('#ctrl_abnrml_simulate_subctrl').hide();
    }
    else if (mainChoice == 'overlap_raw') {
        $('#ctrl_simulate_subctrl').hide();
        $('#simulate_br').show();
        $('#ctrl_abnrml_simulate_subctrl').hide();
    }
    else if (mainChoice == 'existence') {
        $('#ctrl_simulate_subctrl').hide();
        $('#simulate_br').show();
        $('#ctrl_abnrml_simulate_subctrl').hide();
    }
    else if (mainChoice == 'abnormal_simulate') {
        $('#ctrl_simulate_subctrl').hide();
        $('#simulate_br').show();
        $('#ctrl_abnrml_simulate_subctrl').show();
    }
    else {
        console.log("There is no main choice");
        $('#ctrl_simulate_subctrl').hide();
        $('#simulate_br').hide();
        $('#ctrl_abnrml_simulate_subctrl').hide();
    }
}

function updatedTheProgram() {
    show_if_not_empty('pgm', 'ctrl_input');
}

function returnedFromBackEnd() {

}

function mainStructureControl(theEvent) {
    console.log("mainStructureControl(" + theEvent + ")");
    // What are the main events possible:
    // 1. Click on some choice
    // 2. Update the program
    // 3. Return from backend function
    if ('click' == theEvent) {
        console.log("Clicked something");
        mainChoiceChange();
    } 
    else if ('update' == theEvent) {
        console.log("Updated the program");
        updatedTheProgram();
    }
    else if ('return' == theEvent) {
        console.log("Returned from backend");
        returnedFromBackEnd();
    }
    else {
        console.log("Error: called with wrong parameter");
    }
}

function show_if_not_empty(notempty,toshow) {
    if (document.getElementById(notempty).value.length >= 1) {
        $('#' + toshow).show();
    }
}

// Modified from:
// http://stackoverflow.com/questions/19017010/how-to-load-a-file-locally-and-display-its-contents-using-html-javascript-withou
function readSingleFile(evt) {
    //Retrieve the first (and only!) File from the FileList object
    var f = evt.target.files[0];

    if (f) {
        var r = new FileReader();
        r.onload = function (e) {
            var contents = e.target.result;
            document.getElementById('pgm').value = contents;
        }
        r.readAsText(f);
    } else {
        alert("Failed to load "+f);
    }
    $('#ctrl_input').show();
    // show_if_not_empty('pgm', 'ctrl_input');
}

window.onload = function () {
    document.getElementById('file_chooser').addEventListener('change', readSingleFile, false)
    
    // Show the control of the simulation only after entering program
    show_if_not_empty('pgm', 'ctrl_input');
    $("#pgm").keypress(function () {
        show_if_not_empty('pgm', 'ctrl_input');
    });

    // Show the subcontrol and the run button only after making a choice
    var simChoice = document.getElementById('simulate');
    var overlapChoice = document.getElementById('overlap');
    var overlapRawChoice = document.getElementById('overlap_raw');
    var existenceChoice = document.getElementById('existence');
    var abnormalSimChoice = document.getElementById('abnormal_simulate');
    if (simChoice == null || overlapChoice == null || overlapRawChoice == null ||
        existenceChoice == null || abnormalSimChoice == null) {
        console.log("could not find one of the radio buttons");
        return;
    }

    simChoice.onclick = mainChoiceChange('click');
    overlapChoice.onclick = mainChoiceChange('click');
    overlapRawChoice.onclick = mainChoiceChange('click'); 
    existenceChoice.onclick = mainChoiceChange('click');
    abnormalSimChoice.onclick = mainChoiceChange('click');
}

//# sourceMappingURL=app.js.map