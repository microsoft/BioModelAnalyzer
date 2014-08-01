describe("CommandRegistry", function () {
    it("should register new command with its callback", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var counter = 0;
        var testCommandExecutedCallback = function () {
            counter += 1;
        };

        commandRegistry.On("testCommand", testCommandExecutedCallback);
        commandRegistry.Execute("testCommand", undefined);
        expect(counter).toBe(1);
    });
});
