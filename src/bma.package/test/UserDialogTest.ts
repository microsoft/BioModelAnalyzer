// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("UserDialog",() => {

    var container: JQuery = $('<div></div>');
    var widget: JQuery;
    window.FunctionsRegistry = new BMA.Functions.FunctionsRegistry();
    window.Commands = new BMA.CommandRegistry();

    beforeEach(() => {
        widget = $("<div></div>").appendTo(container);
        
    });

    afterEach(() => {
        container.empty();
        widget.userdialog();
        widget.userdialog("destroy");
    });

    it("creates yes-no-cancel dialog by default",() => {
        widget.userdialog();
        var btnlist = widget.find('.button-list');
        var bttns = btnlist.find('button');
        expect(bttns.length).toEqual(3);
        expect(bttns.eq(0).text()).toEqual('Yes');
        expect(bttns.eq(1).text()).toEqual('No');
        expect(bttns.eq(2).text()).toEqual('Cancel');
    });

    it('creates widget with message in div with class "window-title"',() => {
        var message = 'testmessage';
        widget.userdialog({ message: message });
        expect(widget.userdialog('option', 'message')).toEqual(message);
        expect(widget.children().eq(1)[0].classList.contains('window-title')).toBeTruthy();
        expect(widget.children().eq(1).text()).toEqual(message);
    });

    it('triggers callback on click',() => {
        var x = -1;
        var actions = [
            { button: 'Yes', callback: function () { x = 0; } },
            { button: 'No', callback: function () { x = 1; } },
            { button: 'Cancel', callback: function () { x = 2; } }
        ];

        widget.userdialog({
            actions: actions
        });

        var btnlist = widget.find('.button-list');
        var bttns = btnlist.find('button');
        var ind = 2;
        var callback = widget.userdialog('option', 'actions')[ind];
        bttns.eq(ind).trigger('click');
        expect(x).toEqual(ind);
    });
});
