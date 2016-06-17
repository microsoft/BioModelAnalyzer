module bma.Cloud.Jobs

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json.Linq
open System.Diagnostics

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

let getJobEntries (appId: AppId, jobId: JobId) (table : CloudTable) = 
    let query = 
        TableQuery<JobEntity>()
            .Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(JobProperties.AppId, QueryComparisons.Equal, appId.ToString()),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForGuid(JobProperties.JobId, QueryComparisons.Equal, jobId)))
    table.ExecuteQuery(query)

let deleteJob (appId: AppId, jobId: JobId) (table : CloudTable, container : CloudBlobContainer) : bool =
    let deleteBlob (blobName: string) =
        try
            let blob = container.GetBlockBlobReference(blobName)
            blob.DeleteIfExists() |> ignore
        with
        | exn -> Trace.WriteLine (sprintf "Cannot delete blob %s: %A" blobName exn)

    match getJobEntries (appId, jobId) table |> Seq.toList with
    | [] ->
        Trace.WriteLine (sprintf "Job %O is not found" jobId)
        false
    | jobEntries -> 
        let fails =
            jobEntries 
            |> List.fold(fun fails entry -> 
                try
                    TableOperation.Delete entry |> table.Execute |> ignore
                    fails
                with
                | exn -> 
                    Trace.WriteLine (sprintf "Failed to delete the job entry %A: %A" entry.RowKey exn)
                    exn :: fails
                ) []
        let job = jobEntries.Head
        deleteBlob job.Result
        deleteBlob job.Request
        match fails with
        | [] -> true
        | _ -> raise (AggregateException("Failed to delete some or all entries for the job", fails))
