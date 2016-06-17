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
    let JobId = "JobId"
    let AppId = "PartitionKey"
    let EntryId = "RowKey"
    let Status = "Status"
    let StatusInformation = "StatusInformation"
    let Result = "Result"
    let QueueName = "QueueName"

type JobEntity(entryId : Guid, appId : AppId) =
    inherit TableEntity()

    do 
        base.RowKey <- entryId.ToString()
        base.PartitionKey <- appId.ToString()

    new() = JobEntity(Guid.Empty, Guid.Empty) 

    member val JobId = Guid.Empty with get, set
    member val Request = "" with get, set
    member val Result = "" with get, set
    member val Status = "" with get, set
    member val StatusInformation = "" with get, set
    member val QueueName = "" with get, set

    member x.Clone() =
        let e = JobEntity()
        e.RowKey <- x.RowKey
        e.PartitionKey <- x.PartitionKey
        e.JobId <- x.JobId
        e.Request <- x.Request
        e.Result <- x.Result
        e.Status <- x.Status
        e.StatusInformation <- x.StatusInformation
        e.QueueName <- x.QueueName
        e

type JobMessage() =
    member val entryId = "" with get, set
    member val appId = "" with get, set


let buildQueueMessage (entryId : Guid, appId : AppId) =
    sprintf "{ 'entryId' : '%O', 'appId' : '%O' }" entryId appId

let parseQueueMessage (json : string) : Guid * AppId =
    let js = JObject.Parse json
    js.["entryId"].Value<string>() |> Guid.Parse,
    js.["appId"].Value<string>() |> Guid.Parse