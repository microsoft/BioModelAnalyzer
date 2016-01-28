interface Window {
    Commands: BMA.CommandRegistry;
}

module BMA {
    export interface ICommandRegistry {
        Execute(commandName: string, params: any);
        On(commandName: string, onExecutedCallback: (params: any) => void);
        Off(commandName: string, onExecutedCallback: (params: any) => void);
    }

    export class CommandRegistry implements ICommandRegistry {
        private registeredCommands: ApplicationCommand[];

        constructor() {
            this.registeredCommands = [];
        }

        public Execute(commandName: string, params: any) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].Execute(params);
                    return;
                }
            }
        }

        public On(commandName: string, onExecutedCallback: (params: any) => void) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].RegisterCallback(onExecutedCallback);
                    return;
                }
            }

            var newCommand = new ApplicationCommand(commandName);
            newCommand.RegisterCallback(onExecutedCallback);
            this.registeredCommands.push(newCommand);
        }

        public Off(commandName: string, onExecutedCallback: (params: any) => void) {
            for (var i = 0; i < this.registeredCommands.length; i++) {
                if (this.registeredCommands[i].Name == commandName) {
                    this.registeredCommands[i].UnregisterCallback(onExecutedCallback);
                    return;
                }
            }
        }
    }

    export class ApplicationCommand {
        private name: string;
        private callbacks: {
            (params: any): void
        }[];

        public get Name(): string {
            return this.name;
        }

        public RegisterCallback(callback: (params: any) => void): void {
            this.callbacks.push(callback);
        }

        public UnregisterCallback(callback: (params: any) => void): void {
            var index = this.callbacks.indexOf(callback);
            if (index > -1) {
                this.callbacks.splice(index, 1);
            }
        }

        public Execute(params: any) {
            for (var i = 0; i < this.callbacks.length; i++) {
                this.callbacks[i](params);
            }
        }

        constructor(name: string) {
            this.name = name;
            this.callbacks = [];
        }
    }
} 