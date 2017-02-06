// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
open System
open System.Web
open Owin
open Microsoft.Owin
open Microsoft.Owin.FileSystems
open Microsoft.Owin.StaticFiles
open System.Web.Http
open System.IO
open Microsoft.Practices.Unity
open BMAWebApi
open System.Diagnostics
open System.Threading

let startWebApp (address: string) =
    typeof<bma.client.Controllers.AnalyzeController>.Assembly |> ignore // Force controllers loading
    typeof<bma.client.VersionController>.Assembly |> ignore
    let configure (app : IAppBuilder) =
        let config = new HttpConfiguration();
        config.Routes.MapHttpRoute("default", "api/{controller}") |> ignore
        app.UseWebApi(config) |> ignore
        let container = new UnityContainer()
        let logger = new FailureFileLogger("Failures", true)
        container.RegisterInstance<IFailureLogger>(logger) |> ignore
        config.DependencyResolver <- new UnityResolver(container)
        let fopts = FileServerOptions( RequestPath = PathString.Empty, FileSystem = PhysicalFileSystem(@".\bma.client") )
        fopts.StaticFileOptions.ServeUnknownFileTypes <- true
        app.UseFileServer(fopts) |> ignore
    Microsoft.Owin.Hosting.WebApp.Start(address, Action<IAppBuilder>(configure))

let helptext = @"This program self-hosts BMA (both API and UI). Available switches:
-h|--help - displays this text
-b|--background - hosts BMA in a separate process; returns exit code 0 if BMA was started successfully and exit code 1 otherwise
start without switches to host BMA in the currently opened console window"

[<EntryPoint>]
let main argv =
    if argv.Length = 0 then
        try
            let settingsReader = System.Configuration.AppSettingsReader()
            let address = settingsReader.GetValue("BackEndUrl", typeof<string>) :?> string
            startWebApp address |> ignore
            Console.WriteLine ("BioModelAnalyzer application is now hosted on " + address)
            Console.WriteLine "Press any key to shut it down..."
            Console.WriteLine "\n\nServer trace output:\n"
            Console.ReadKey() |> ignore
        with
            | ex ->
                    Trace.TraceError ("Failed to start BMA: " + ex.Message)
                    exit 1
    elif argv.[0] = "-b" || argv.[0] = "--background" then
        let guid = Guid.NewGuid().ToString()
        let sem = new Semaphore(0, 1, guid)
        let pi = Process.GetCurrentProcess()
        let fi = pi.MainModule.FileName
        let child = new Process()
        child.StartInfo <- ProcessStartInfo(fi, guid)
        child.EnableRaisingEvents <- true
        child.Exited.Add(fun a -> if child.ExitCode <> 0 then exit 1)
        child.Start() |> ignore
        sem.WaitOne() |> ignore
        sem.Close()
    elif argv.[0] = "-h" || argv.[0] = "--help" then
        Console.WriteLine helptext
    elif fst (Guid.TryParse(argv.[0])) then
        try
            let sem = Semaphore.OpenExisting(argv.[0])
            let settingsReader = System.Configuration.AppSettingsReader()
            let address = settingsReader.GetValue("BackEndUrl", typeof<string>) :?> string
            startWebApp address |> ignore
            sem.Release() |> ignore
            Console.WriteLine ("BioModelAnalyzer application is now hosted on " + address)
            Console.WriteLine "Press any key to shut it down..."
            Console.WriteLine "\n\nServer trace output:\n"
            Console.ReadKey() |> ignore
        with
            | ex ->
                    Trace.TraceError ("Failed to start BMA: " + ex.Message)
                    exit 1
    else
        Console.WriteLine "ERROR: incorrect parameters"
        Console.WriteLine helptext

    0 // return an integer exit code
