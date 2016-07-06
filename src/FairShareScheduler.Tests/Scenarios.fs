module Scenarios

open NUnit.Framework
open bma.Cloud
open Microsoft.WindowsAzure.Storage
open System
open System.IO
open bma.Cloud.Jobs
open System.Threading
open Newtonsoft.Json.Linq

let settings = 
    { StorageAccount = CloudStorageAccount.DevelopmentStorageAccount
    ; MaxNumberOfQueues = 3
    ; Name = "testscenarios" }

let scheduler : IScheduler = upcast FairShareScheduler(settings) 

let asStream (obj : JObject) = 
    let ms = new MemoryStream()
    let w = new StreamWriter(ms)
    w.Write(obj.ToString())
    w.Flush()
    ms.Position <- 0L
    ms :> Stream

let waitSuccess (appId, jobId) (scheduler : IScheduler) =
    let isSuccess() =
        match scheduler.TryGetStatus (appId, jobId) with
        | Some (JobStatus.Succeeded, _) -> true
        | Some (JobStatus.Failed, message) -> failwithf "Job has failed: %s" message
        | Some (JobStatus.Executing, _) 
        | Some (JobStatus.Queued, _) -> false
        | _ -> failwith "Failed to get the job status"
    while not (isSuccess()) do Thread.Sleep(100)

let waitFailure (appId, jobId) (scheduler : IScheduler) =
    let isFailed() =
        match scheduler.TryGetStatus (appId, jobId) with
        | Some (JobStatus.Succeeded, _) -> failwithf "Job has succeeded"
        | Some (JobStatus.Failed, message) -> Some message
        | Some (JobStatus.Executing, _) 
        | Some (JobStatus.Queued, _) -> None
        | _ -> failwith "Failed to get the job status"
        
    Seq.initInfinite(id)
    |> Seq.pick (fun _ -> 
        match isFailed() with
        | Some message -> Some message
        | None -> Thread.Sleep(100); None) 

let getResult (appId, jobId) (scheduler : IScheduler) =
    match scheduler.TryGetResult (appId, jobId) with
    | Some (s) -> 
        use sr = new StreamReader(s)
        let s = sr.ReadToEnd()
        JObject.Parse(s)
    | _ -> failwith "Failed to get the job result"


[<Test; Timeout(180000)>]
let ``Enqueue a job and get its result``() =
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", 100), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    waitSuccess (appId, jobId) scheduler
    let r = getResult (appId, jobId) scheduler
    Assert.AreEqual(appId.ToString(), r.["result"].Value<string>(), "Result is incorrect")
    ()

[<Test; Timeout(180000)>]
let ``Enqueue an incorrect job and get the error message``() =
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", "not-a-double"), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    let failure = waitFailure (appId, jobId) scheduler
    StringAssert.Contains("System.FormatException", failure, "Error message")
