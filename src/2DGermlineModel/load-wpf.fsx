//-----------------------------------------------------------------------------
// A script utility for using WPF with F# Interactive (fsi.exe)
//
// Copyright (c) Microsoft Corporation 2005-2006.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 
//-----------------------------------------------------------------------------


#light
// When running inside fsi, this will install a WPF event loop
#if INTERACTIVE

#I "c:/Program Files/Reference Assemblies/Microsoft/Framework/v3.0";;
#I "C:/WINDOWS/Microsoft.NET/Framework/v3.0/WPF/";;
#r "presentationcore.dll";;
#r "presentationframework.dll";;
#r "WindowsBase.dll";;

module WPFEventLoop = 
    open System
    open System.Windows
    open System.Windows.Threading
    open Microsoft.FSharp.Compiler.Interactive
    open Microsoft.FSharp.Compiler.Interactive.Settings
    type RunDelegate<'b> = delegate of unit -> 'b 
    
    let Create() = 
        let app  = 
            try 
                // Ensure the current application exists. This may fail, if it already does.
                let app = new Application() in 
                // Create a dummy window to act as the main window for the application.
                // Because  we're in FSI we never want to clean this up.
                new Window() |> ignore; 
                app 
             with :? InvalidOperationException -> Application.Current
        let disp = app.Dispatcher
        let restart = ref false
        { new IEventLoop with
             member x.Run() =   
                 app.Run() |> ignore
                 !restart
             member x.Invoke(f) = 
                 try disp.Invoke(DispatcherPriority.Send,new RunDelegate<_>(fun () -> box(f ()))) |> unbox
                 with e -> eprintf "\n\n ERROR: %O\n" e; reraise() // reraise
             member x.ScheduleRestart() =   ()
                 //restart := true;
                 //app.Shutdown()
        } 
    let Install() = fsi.EventLoop <-  Create()
    
WPFEventLoop.Install();;

#endif

