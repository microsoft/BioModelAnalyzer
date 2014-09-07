describe("CommandRegistry", function () {
    it("should register new command with its callback", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var testObj = {
            testCallback: function () {
            }
        };

        spyOn(testObj, "testCallback");

        commandRegistry.On("testCommand", testObj.testCallback);
        commandRegistry.Execute("testCommand", undefined);

        expect(testObj.testCallback).toHaveBeenCalled();
    });

    it("should execute callback with specified parameter", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var testObj = {
            testCallback: function () {
            }
        };

        var testParam = "test parameter";

        spyOn(testObj, "testCallback");

        commandRegistry.On("testCommand", testObj.testCallback);
        commandRegistry.Execute("testCommand", testParam);

        expect(testObj.testCallback).toHaveBeenCalledWith(testParam);
    });

    it("should register additional callback for existing command", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var testObj = {
            testCallback: function () {
            },
            testCallback2: function () {
            }
        };

        spyOn(testObj, "testCallback");
        spyOn(testObj, "testCallback2");

        commandRegistry.On("testCommand", testObj.testCallback);
        commandRegistry.On("testCommand", testObj.testCallback2);

        commandRegistry.Execute("testCommand", undefined);
        expect(testObj.testCallback).toHaveBeenCalled();
        expect(testObj.testCallback2).toHaveBeenCalled();
    });

    it("should unregister new command with its callback", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var testObj = {
            testCallback: function () {
            }
        };

        spyOn(testObj, "testCallback");

        commandRegistry.On("testCommand", testObj.testCallback);
        commandRegistry.Off("testCommand", testObj.testCallback);

        commandRegistry.Execute("testCommand", undefined);

        expect(testObj.testCallback).not.toHaveBeenCalled();
    });

    it("should register 2 separate commands with their own callbacks", function () {
        var commandRegistry = new BMA.CommandRegistry();

        var testObj = {
            testCallback: function () {
            },
            testCallback2: function () {
            }
        };

        spyOn(testObj, "testCallback");
        spyOn(testObj, "testCallback2");

        commandRegistry.On("testCommand", testObj.testCallback);
        commandRegistry.On("testCommand2", testObj.testCallback2);

        commandRegistry.Execute("testCommand", undefined);
        expect(testObj.testCallback).toHaveBeenCalled();

        commandRegistry.Execute("testCommand2", undefined);
        expect(testObj.testCallback2).toHaveBeenCalled();
    });
});
