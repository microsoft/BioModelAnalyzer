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
    let tableExec = tableClient.GetTableReference (getJobsExecutionTableName settings.Name)
    let blobClient = settings.StorageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference (getBlobContainerName settings.Name)

       
    /// Returns None, if the job isn't being executed at the moment
    /// otherwise, returns Some of execution start time.
    let isExecuting (appId: AppId, jobId: JobId) = 
        tableExec 
        |> tryGetJobExecutionEntry (appId, jobId) 
        |> Option.map(fun e -> e.Timestamp)

    do 
        table.CreateIfNotExists() |> ignore  
        tableExec.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore


    interface IScheduler with

        member x.AddJob (job : Job) : JobId =
            let jobId = Guid.NewGuid()
            let queueIdx = Math.Abs(job.AppId.GetHashCode()) % settings.MaxNumberOfQueues
            let queueName = getQueueName queueIdx settings.Name

            let blobReq = container.GetBlockBlobReference (getJobRequestBlobName jobId settings.Name)
            blobReq.UploadFromStream(job.Body)

            let blobResName = getJobResultBlobName jobId settings.Name

            let jobEntity = JobEntity(jobId, job.AppId) //, blobReq.Name, blobResName, status JobStatus.Queued, String.Empty, queueName)
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
            match tryGetJobEntry (appId, jobId) table with
            | None ->
                logInfo (sprintf "Job %A not found" jobId)
                None
            | Some job ->
                match parseStatus job.Status with
                | JobStatus.Succeeded -> JobStatus.Succeeded, job.StatusInformation
                | JobStatus.Failed -> JobStatus.Failed, job.StatusInformation
                | _ ->
                    match isExecuting (appId, jobId) with
                    | Some time -> JobStatus.Executing, time.ToString("o")
                    | None -> JobStatus.Queued, job.StatusInformation
                |> Some

        member x.TryGetResult (appId: AppId, jobId: JobId) : IO.Stream option =
            match tryGetJobEntry (appId, jobId) table with
            | None ->
                logInfo (sprintf "Job %A not found" jobId)
                None
            | Some job ->
                match parseStatus job.Status with
                | JobStatus.Succeeded -> getBlobContent job.Result container |> Some
                | _ ->  None

        member x.DeleteJob (appId: AppId, jobId: JobId) : bool =
            Jobs.deleteJob (appId, jobId) (table, tableExec, container)

    static member CleanAll (name:string) (storageAccount : CloudStorageAccount) =
        let tableClient = storageAccount.CreateCloudTableClient()
        let table = tableClient.GetTableReference (getJobsTableName name)
        table.DeleteIfExists() |> ignore
        let table = tableClient.GetTableReference (getJobsExecutionTableName name)
        table.DeleteIfExists() |> ignore
        
        let blobClient = storageAccount.CreateCloudBlobClient()
        let container = blobClient.GetContainerReference (getBlobContainerName name)
        container.DeleteIfExists() |> ignore

        let queueClient = storageAccount.CreateCloudQueueClient()      
        queueClient.ListQueues(getName "" name)
        |> Seq.iter(fun queue -> queue.DeleteIfExists() |> ignore)