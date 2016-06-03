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
open Newtonsoft.Json

open bma.Cloud
open bma.LTLPolarity

type WorkerRole() =
    inherit RoleEntryPoint() 

    let log message (kind : string) = Trace.TraceInformation(message, kind)
    let logInfo message = log message "Information"

    let mutable worker : IWorker option = None

    let doJob(jobId: Guid, input: IO.Stream) : IO.Stream = 
        logInfo (sprintf "Doing the job #%O" jobId)

        let reader = new IO.StreamReader(input)
        let input_s = reader.ReadToEnd()
        let query = JsonConvert.DeserializeObject<LTLPolarityAnalysisInputDTO>(input_s)

        
        let res = bma.LTLPolarity.Algorithms.Check(query)



        let jsRes = JsonConvert.SerializeObject(res)
        let output_s = jsRes.ToString()
        let ms = new IO.MemoryStream()
        let writer = new IO.StreamWriter(ms)
        writer.Write(output_s)
        writer.Flush()
        ms.Position <- 0L
        upcast ms


    override wr.Run() =
        let worker = worker.Value

        logInfo "LTLPolarityCheckRole entry point called"
        worker.Start(doJob, TimeSpan.FromSeconds 1.0)
        base.Run()

    override wr.OnStart() = 
        let storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
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
