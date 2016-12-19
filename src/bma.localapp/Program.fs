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


[<EntryPoint>]
let main argv = 
    typeof<bma.client.Controllers.AnalyzeController>.Assembly |> ignore // Force controllers loading
    typeof<bma.client.VersionController>.Assembly |> ignore
    let settingsReader = System.Configuration.AppSettingsReader()
    let address = settingsReader.GetValue("BackEndUrl", typeof<string>) :?> string
    let mutable webApp : IDisposable = null
    let configure (app : IAppBuilder) =
        let config = new HttpConfiguration();
        config.Routes.MapHttpRoute("default", "api/{controller}") |> ignore
        app.UseWebApi(config) |> ignore
        let container = new UnityContainer()
        let logger = new FailureFileLogger("Failures", true)
        container.RegisterInstance<IFailureLogger>(logger) |> ignore
        config.DependencyResolver <- new UnityResolver(container)
        let fopts = FileServerOptions( RequestPath = PathString.Empty, FileSystem = PhysicalFileSystem(@"..\..\..\bma.client") )
        fopts.StaticFileOptions.ServeUnknownFileTypes <- true
        app.UseFileServer(fopts) |> ignore

    webApp <- Microsoft.Owin.Hosting.WebApp.Start(address, Action<IAppBuilder>(configure))

    System.Diagnostics.Process.Start address |> ignore

    Console.WriteLine "Press any key to shutdown..."
    Console.ReadKey() |> ignore
    0 // return an integer exit code
