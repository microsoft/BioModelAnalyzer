namespace LTLPolarityCheckRole

open System
open System.Collections.Generic
open System.Diagnostics
open System.Linq
open System.Net
open System.Threading
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Diagnostics
open Microsoft.WindowsAzure.ServiceRuntime
open Microsoft.WindowsAzure.Storage

open bma.Cloud

type WorkerRole() =
    inherit RoleEntryPoint() 

    let log message (kind : string) = Trace.TraceInformation(message, kind)
    let logInfo message = log message "Information"

    let mutable worker : IWorker option = None

    let doJob(jobId: Guid, input: IO.Stream) = 
        logInfo (sprintf "Doing the job #%O" jobId)
        input


    override wr.Run() =
        let worker = worker.Value

        logInfo "LTLPolarityCheckRole entry point called"
        worker.Start(doJob, TimeSpan.FromSeconds 1.0)
        base.Run()

    override wr.OnStart() = 
        let storageAccount = CloudStorageAccount.DevelopmentStorageAccount
        let schedulerName = "ltlpolarity"; // todo: can differ for different controllers; use setter injection with name?

        ServicePointManager.DefaultConnectionLimit <- 12

        try
            worker <- Worker.Create (storageAccount, schedulerName) |> Some
            base.OnStart()
        with
        | ex -> 
            log (sprintf "Exception during OnStart: %O" ex) "Error"
            false

    override wr.OnStop() =
        worker |> Option.iter (fun w -> w.Dispose())
