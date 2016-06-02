module bma.Cloud.Jobs

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json.Linq

type JobId = Guid
type AppId = Guid

type JobStatus =
    | Queued = 0
    | Executing = 1
    | Succeeded = 2
    | Failed = 3
   
let status = 
    function 
    | JobStatus.Queued -> "Queued"
    | JobStatus.Executing -> "Executing"
    | JobStatus.Succeeded -> "Succeeded"
    | JobStatus.Failed -> "Failed"
    | _ -> failwith "Unexpected job status"

let parseStatus status =
    match status with
    | "Queued" -> JobStatus.Queued
    | "Executing" -> JobStatus.Executing
    | "Succeeded" -> JobStatus.Succeeded
    | "Failed" -> JobStatus.Failed
    | _ -> failwith "Unexpected job status"

module JobProperties =  
    let Status = "Status"
    let Result = "Result"
    let QueueName = "QueueName"

type JobEntity(jobId : JobId, appId : AppId) =
    inherit TableEntity()

    do 
        base.RowKey <- jobId.ToString()
        base.PartitionKey <- appId.ToString()

    new() = JobEntity(Guid.Empty, Guid.Empty) 

    member val Request = "" with get, set
    member val Result = "" with get, set
    member val Status = "" with get, set
    member val QueueName = "" with get, set

type JobMessage() =
    member val jobId = "" with get, set
    member val appId = "" with get, set


let buildQueueMessage (jobId : JobId, appId : AppId) =
    sprintf "{ 'jobId' : '%O', 'appId' : '%O' }" jobId appId

let parseQueueMessage (json : string) : JobId * AppId =
    let js = JObject.Parse json
    js.["jobId"].Value<string>() |> Guid.Parse,
    js.["appId"].Value<string>() |> Guid.Parse