// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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

type JobEntity(jobId : Guid, appId : AppId) =
    inherit TableEntity()

    do 
        base.RowKey <- jobId.ToString()
        base.PartitionKey <- appId.ToString()

    new() = JobEntity(Guid.Empty, Guid.Empty) 

    member val Request = "" with get, set
    member val Result = "" with get, set
    member val Status = "" with get, set
    member val StatusInformation = "" with get, set
    member val QueueName = "" with get, set

    [<IgnoreProperty>]
    member x.JobId = Guid.Parse(x.RowKey)
    
type JobExecutionEntity(jobId : Guid, appId : AppId) =
    inherit TableEntity()

    do 
        base.RowKey <- jobId.ToString()
        base.PartitionKey <- appId.ToString()

    new() = JobExecutionEntity(Guid.Empty, Guid.Empty) 

type JobMessage() =
    member val jobId = "" with get, set
    member val appId = "" with get, set


let buildQueueMessage (jobId : JobId, appId : AppId) =
    sprintf "{ 'jobId' : '%O', 'appId' : '%O' }" jobId appId

let parseQueueMessage (json : string) : JobId * AppId =
    let js = JObject.Parse json
    js.["jobId"].Value<string>() |> Guid.Parse,
    js.["appId"].Value<string>() |> Guid.Parse

    
let getBlobContent (blobName: string) (container : CloudBlobContainer) : IO.Stream =
    let blob = container.GetBlockBlobReference(blobName)
    let stream = new System.IO.MemoryStream()
    blob.DownloadToStream(stream)
    stream.Position <- 0L
    stream :> System.IO.Stream

let tryGetJobEntry (appId: AppId, jobId: JobId) (table : CloudTable) = 
    let retrieve = TableOperation.Retrieve<JobEntity>(appId.ToString(), jobId.ToString())
    let resp = table.Execute retrieve
    match resp.Result with
    | null -> None
    | :? JobEntity as job -> Some job
    | _ -> None
    
let tryGetJobExecutionEntry (appId: AppId, jobId: JobId) (table : CloudTable) = 
    let retrieve = TableOperation.Retrieve<JobExecutionEntity>(appId.ToString(), jobId.ToString())
    let resp = table.Execute retrieve
    match resp.Result with
    | null -> None
    | :? JobExecutionEntity as job -> Some job
    | _ -> None

let internal ignoreExn (f : unit -> unit) (message : string) =
    try
        f()
        true
    with
    | exn -> 
        Trace.WriteLine(sprintf "%s: %A" message exn)
        false

let deleteBlob (blobName: string) (container : CloudBlobContainer) =
    ignoreExn (fun() ->
        let blob = container.GetBlockBlobReference(blobName)
        blob.DeleteIfExists() |> ignore) 
        (sprintf "Cannot delete blob %s" blobName)

let deleteJob (appId: AppId, jobId: JobId) (table : CloudTable, tableExecution : CloudTable, container : CloudBlobContainer) : bool =
    match tryGetJobEntry (appId, jobId) table with
    | None ->
        Trace.WriteLine (sprintf "Job %O is not found" jobId)
        false
    | Some job -> 
        ignoreExn (fun () -> TableOperation.Delete job |> table.Execute |> ignore) "Cannot delete job entry" &&
        ignoreExn (fun () -> 
            tryGetJobExecutionEntry (appId, jobId) tableExecution
            |> Option.iter(fun e ->
                TableOperation.Delete e |> tableExecution.Execute |> ignore)) "Cannot delete information about job execution" &&
        deleteBlob job.Result container &&
        deleteBlob job.Request container

        
