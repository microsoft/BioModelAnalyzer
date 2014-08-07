var BMA;
(function (BMA) {
    var CommandRegistry = (function () {
        function CommandRegistry() {
            this.registeredCommands = [];
        }
        CommandRegistry.prototype.Execute = function (commandName, params) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].Execute(params);
                    return;
                }
            }
        };

        CommandRegistry.prototype.On = function (commandName, onExecutedCallback) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].RegisterCallback(onExecutedCallback);
                    return;
                }
            }

            var newCommand = new ApplicationCommand(commandName);
            newCommand.RegisterCallback(onExecutedCallback);
            this.registeredCommands.push(newCommand);
        };

        CommandRegistry.prototype.Off = function (commandName, onExecutedCallback) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].UnregisterCallback(onExecutedCallback);
                    return;
                }
            }
        };
        return CommandRegistry;
    })();
    BMA.CommandRegistry = CommandRegistry;

    var ApplicationCommand = (function () {
        function ApplicationCommand(name) {
            this.name = name;
            this.callbacks = [];
        }
        Object.defineProperty(ApplicationCommand.prototype, "Name", {
            get: function () {
                return this.name;
            },
            enumerable: true,
            configurable: true
        });

        ApplicationCommand.prototype.RegisterCallback = function (callback) {
            this.callbacks.push(callback);
        };

        ApplicationCommand.prototype.UnregisterCallback = function (callback) {
            var index = this.callbacks.indexOf(callback);
            if (index > -1) {
                this.callbacks.splice(index, 1);
            }
        };

        ApplicationCommand.prototype.Execute = function (params) {
            for (var i = 0; i < this.callbacks.length; i++) {
                this.callbacks[i](params);
            }
        };
        return ApplicationCommand;
    })();
    BMA.ApplicationCommand = ApplicationCommand;
})(BMA || (BMA = {}));
