module bma.Cloud.Jobs

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json.Linq

type JobId = Guid
type AppId = Guid

type JobEntity(jobId : JobId, appId : AppId) =
    inherit TableEntity()

    do 
        base.RowKey <- jobId.ToString()
        base.PartitionKey <- appId.ToString()

    new() = JobEntity(Guid.Empty, Guid.Empty) 

    member val request = "" with get, set
    member val result = "" with get, set
    member val status = "" with get, set
    member val queueName = "" with get, set

type JobMessage() =
    member val jobId = "" with get, set
    member val appId = "" with get, set


let buildQueueMessage (jobId : JobId, appId : AppId) =
    sprintf "{ 'jobId' : '%O', 'appId' : '%O' }" jobId appId

let parseQueueMessage (json : string) =
    let js = JObject.Parse json
    js.["jobId"].Value<string>() |> Guid.Parse,
    js.["appId"].Value<string>() |> Guid.Parse