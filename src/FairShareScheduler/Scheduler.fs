namespace bma.Cloud

open System
open System.IO
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.Storage.Queue
open Microsoft.WindowsAzure.Storage.Blob
open bma.Cloud.Trace
open bma.Cloud.Jobs
open bma.Cloud.Naming

type Job =
    { AppId: AppId
      Body: Stream }



[<Interface>]
type IScheduler =
    abstract AddJob : Job -> JobId
    abstract TryGetStatus : AppId * JobId -> JobStatus option
    abstract TryGetResult : AppId * JobId -> IO.Stream option

type FairShareSchedulerSettings =
    { StorageAccount : CloudStorageAccount
      MaxNumberOfQueues : int
      Name: string }

[<Class>]
type FairShareScheduler(settings : FairShareSchedulerSettings) =
    let queueClient = settings.StorageAccount.CreateCloudQueueClient()      
    let tableClient = settings.StorageAccount.CreateCloudTableClient()
    let table = tableClient.GetTableReference (getJobsTableName settings.Name)
    let blobClient = settings.StorageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference (getBlobContainerName settings.Name)
    
    let getResult (resultBlobName: string) =
        let blob = container.GetBlockBlobReference(resultBlobName)
        let stream = new System.IO.MemoryStream()
        blob.DownloadToStream(stream)
        stream.Position <- 0L
        stream :> System.IO.Stream
    
    do 
        table.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore


    interface IScheduler with

        member x.AddJob (job : Job) : JobId =
            let jobId = Guid.NewGuid()
            let queueIdx = Math.Abs(job.AppId.GetHashCode()) % settings.MaxNumberOfQueues
            let queueName = getQueueName queueIdx settings.Name

            let blob = container.GetBlockBlobReference (getJobRequestBlobName jobId settings.Name)
            blob.UploadFromStream(job.Body)

            let jobEntity = JobEntity(jobId, job.AppId)
            jobEntity.Request <- blob.Name
            jobEntity.Status <- status JobStatus.Queued
            jobEntity.QueueName <- queueName

            let insert = TableOperation.Insert(jobEntity)
            table.Execute(insert) |> ignore
            logInfo (sprintf "Job added to the table with id = %O, queue = %s" jobId queueName)

            let queue = queueClient.GetQueueReference(queueName)
            queue.CreateIfNotExists() |> ignore
            queue.AddMessage(CloudQueueMessage(buildQueueMessage(jobId, job.AppId)))
            logInfo (sprintf "Job %O is queued" jobId)
            jobId

        member x.TryGetStatus (appId: AppId, jobId: JobId) : JobStatus option =
            let cols = System.Collections.Generic.List<string>()
            cols.Add(JobProperties.Status)
            let retrieveOperation = TableOperation.Retrieve<JobEntity>(appId.ToString(), jobId.ToString(), cols)
            let r = table.Execute(retrieveOperation)
            match r.HttpStatusCode with
            | error when error < 200 || error >= 300 -> 
                logInfo (sprintf "GetStatus for job %O returned error code %d" jobId error)
                None
            | success ->
                match r.Result with
                | :? JobEntity as job ->
                    job.Status |> parseStatus |> Some
                | _ ->
                    logInfo (sprintf "GetStatus for job %O returned code %d but not a JobEntity instance" jobId success)
                    None

        member x.TryGetResult (appId: AppId, jobId: JobId) : IO.Stream option =
            let cols = System.Collections.Generic.List<string>()
            cols.Add(JobProperties.Status)
            cols.Add(JobProperties.Result)
            let retrieveOperation = TableOperation.Retrieve<JobEntity>(appId.ToString(), jobId.ToString(), cols)
            let r = table.Execute(retrieveOperation)
            match r.HttpStatusCode with
            | error when error < 200 || error >= 300 -> 
                logInfo (sprintf "GetResult for job %O returned error code %d" jobId error)
                None
            | success ->
                match r.Result with
                | :? JobEntity as job ->
                    match parseStatus job.Status with
                    | JobStatus.Succeeded ->
                        getResult job.Result |> Some
                    | _ -> 
                        logInfo (sprintf "GetResult for job %O: job has status %A; cannot return the result for it" jobId job.Status)
                        None
                | _ ->
                    logInfo (sprintf "GetStatus for job %O returned code %d but not a JobEntity instance" jobId success)
                    None