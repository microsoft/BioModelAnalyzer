module Scenarios

open NUnit.Framework
open bma.Cloud
open Microsoft.WindowsAzure.Storage
open System
open System.IO
open bma.Cloud.Jobs
open System.Threading
open Newtonsoft.Json.Linq
open FSharp.Collections.ParallelSeq
open System.Diagnostics

let settings = 
    { StorageAccount = CloudStorageAccount.DevelopmentStorageAccount
    ; MaxNumberOfQueues = 3
    ; Name = "testscenarios" }

let mutable scheduler : IScheduler option = None;

let getScheduler() = scheduler.Value

[<TestFixtureSetUp>]
let Prepare() = 
    Trace.WriteLine("Cleaning the scheduler data...")
    FairShareScheduler.CleanAll settings.Name settings.StorageAccount
    Trace.WriteLine("Initializing scheduler...")
    scheduler <- Some(upcast FairShareScheduler(settings))
    


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

let waitExecuting (appId, jobId) (scheduler : IScheduler) =
    let isExecuting() =
        match scheduler.TryGetStatus (appId, jobId) with
        | Some (JobStatus.Executing, _) -> true
        | Some (JobStatus.Failed, message) -> failwithf "Job has failed: %s" message
        | Some (JobStatus.Succeeded, _) 
        | Some (JobStatus.Queued, _) -> false
        | _ -> failwith "Failed to get the job status"
    while not (isExecuting()) do Thread.Sleep(100)

let getResult (appId, jobId) (scheduler : IScheduler) =
    match scheduler.TryGetResult (appId, jobId) with
    | Some (s) -> 
        use sr = new StreamReader(s)
        let s = sr.ReadToEnd()
        JObject.Parse(s)
    | _ -> failwith "Failed to get the job result"

let doJob (appId:AppId) (duration:int) (scheduler : IScheduler) = 
    use body = JObject(JProperty("sleep", duration), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    waitSuccess (appId, jobId) scheduler
    let r = getResult (appId, jobId) scheduler
    Assert.AreEqual(appId.ToString(), r.["result"].Value<string>(), "Result is incorrect")


[<Test; Timeout(180000)>]
let ``Enqueue a job and get its result``() =
    let appId = Guid.NewGuid()
    doJob appId 100 (getScheduler())

[<Test; Timeout(180000)>]
let ``Enqueue an incorrect job and get the error message``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", "not-a-double"), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    let failure = waitFailure (appId, jobId) scheduler
    StringAssert.Contains("System.FormatException", failure, "Error message")

[<Test; Timeout(180000)>]
let ``Enqueue a too long job and get the error message``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", Timeout.Infinite), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    let failure = waitFailure (appId, jobId) scheduler
    StringAssert.Contains("System.TimeoutException", failure, "Error message")


[<Test; Timeout(180000)>]
let ``Enqueue a poison job and get the error message``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("failworker", true)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    let failure = waitFailure (appId, jobId) scheduler
    StringAssert.Contains("failed 2 times", failure, "Error message")

[<Test; Timeout(180000)>]
let ``Cancel the job immediately``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", Timeout.Infinite), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    Assert.IsTrue(scheduler.DeleteJob (appId, jobId), "Job wasn't deleted")
    Assert.AreEqual(None, scheduler.TryGetStatus (appId, jobId), "Job must not be found")

[<Test; Timeout(180000)>]
let ``Cancel the job after it is executing``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", Timeout.Infinite), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let jobId = scheduler.AddJob(job)
    waitExecuting (appId, jobId) scheduler
    Assert.IsTrue(scheduler.DeleteJob (appId, jobId), "Job wasn't deleted")
    Assert.AreEqual(None, scheduler.TryGetStatus (appId, jobId), "Job must not be found")

[<Test; Timeout(180000)>]
let ``Status 'executing' includes execution start time``() =
    let scheduler = getScheduler()
    let appId = Guid.NewGuid()
    use body = JObject(JProperty("sleep", Timeout.Infinite), JProperty("result", appId)) |> asStream
    let job = { AppId = appId; Body = body }
    let t0 = DateTime.Now
    let jobId = scheduler.AddJob(job)

    waitExecuting (appId, jobId) scheduler
    let t1 = DateTime.Now

    match scheduler.TryGetStatus (appId, jobId) with
    | Some (JobStatus.Executing, info) -> 
        Trace.WriteLine(sprintf "Status info is %s" info)
        match DateTime.TryParse info with
        | true, time -> 
            Trace.WriteLine(sprintf "Execution start time is %A" time)
            Assert.IsTrue(t0 < time, "start time is earlier than expected")
            Assert.IsTrue(t1 > time, "start time is later than expected")
        | false, _ -> Assert.Fail(sprintf "The status info is not a date time string: %s" info)
    | Some (s, _) -> Assert.Fail("Status is not 'executing'")
    | None -> Assert.Fail("Failed to get the job status")

    Assert.IsTrue(scheduler.DeleteJob (appId, jobId), "Job wasn't deleted")


[<Test; Timeout(1800000)>]
let ``Massive jobs handling``() =
    let scheduler = getScheduler()
    let appsN = 10
    let jobsN = 50
    let duration = 1
    
    let app () =
        let appId = Guid.NewGuid()
        Seq.init jobsN id
        |> PSeq.iter (fun i ->
            Trace.WriteLine(sprintf "App %O starts job %d" appId i)
            doJob appId duration scheduler
            Trace.WriteLine(sprintf "App %O succeeded job %d" appId i))
    Seq.init appsN id
    |> PSeq.iter (fun _ -> app ())


