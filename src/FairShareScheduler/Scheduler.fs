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
    abstract DeleteJob : AppId * JobId -> bool
    abstract TryGetStatus : AppId * JobId -> (JobStatus * string) option
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
    
    let getBlobContent (blobName: string) =
        let blob = container.GetBlockBlobReference(blobName)
        let stream = new System.IO.MemoryStream()
        blob.DownloadToStream(stream)
        stream.Position <- 0L
        stream :> System.IO.Stream

    let getJobEntry (appId: AppId, jobId: JobId) = 
        let retrieve = TableOperation.Retrieve<JobEntity>(appId, jobId)
        match table.Execute retrieve with
        | null -> None
        | :? JobEntity as job -> Some job
        | _ -> None
        
    let isExecuting (appId: AppId, jobId: JobId) = 
        let retrieve = TableOperation.Retrieve<JobExecutionEntity>(appId, jobId)
        match table.Execute retrieve with
        | null -> false
        | _ -> true

    do 
        table.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore


    interface IScheduler with

        member x.AddJob (job : Job) : JobId =
            let jobId = Guid.NewGuid()
            let queueIdx = Math.Abs(job.AppId.GetHashCode()) % settings.MaxNumberOfQueues
            let queueName = getQueueName queueIdx settings.Name

            let blobReq = container.GetBlockBlobReference (getJobRequestBlobName jobId settings.Name)
            blobReq.UploadFromStream(job.Body)

            let blobResName = getJobResultBlobName jobId settings.Name

            let jobEntity = JobEntity(jobId, job.AppId)
            jobEntity.Request <- blobReq.Name
            jobEntity.Result <- blobResName
            jobEntity.Status <- status JobStatus.Queued
            jobEntity.QueueName <- queueName

            let insert = TableOperation.Insert(jobEntity)
            table.Execute(insert) |> ignore
            logInfo (sprintf "Job added to the table with jobId = %O, queue = %s" jobId queueName)

            let queue = queueClient.GetQueueReference(queueName)
            queue.CreateIfNotExists() |> ignore
            queue.AddMessage(CloudQueueMessage(buildQueueMessage(jobId, job.AppId)))
            logInfo (sprintf "Job %O is queued" jobId)
            jobId

        member x.TryGetStatus (appId: AppId, jobId: JobId) : (JobStatus * string) option =
            match getJobEntry (appId, jobId) with
            | None ->
                logInfo (sprintf "Job %A not found" jobId)
                None
            | Some job ->
                match parseStatus job.Status with
                | JobStatus.Succeeded -> JobStatus.Succeeded, job.StatusInformation
                | JobStatus.Failed -> JobStatus.Failed, job.StatusInformation
                | _ when isExecuting (appId, jobId) -> JobStatus.Executing, ""
                | _ -> JobStatus.Queued, job.StatusInformation

        member x.TryGetResult (appId: AppId, jobId: JobId) : IO.Stream option =
            match getJobEntry (appId, jobId) with
            | None ->
                logInfo (sprintf "Job %A not found" jobId)
                None
            | Some job ->
                match parseStatus job.Status with
                | JobStatus.Succeeded -> getBlobContent job.Result
                | _ ->  None

        member x.DeleteJob (appId: AppId, jobId: JobId) : bool =
            Jobs.deleteJob (appId, jobId) (table, container)

