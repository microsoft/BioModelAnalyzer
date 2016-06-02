module bma.Cloud.Jobs

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json.Linq

type JobId = Guid
type AppId = Guid

module JobStatus =
    let Queued = "Queued"
    let Executing = "Executing"
    let Succeeded = "Succeeded"

module JobProperties =  
    let Status = "Status"
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