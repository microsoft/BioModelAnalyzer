module BMA {
    export interface ICommand {
        CanExecute(e: any): boolean;
        Execute(e: any);
    }
}