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

type JobStatusWithInfo =
    | Succeeded
    | Queued of position:int
    | Executing of started:DateTimeOffset
    | Failed of message:string



[<Interface>]
type IScheduler =
    abstract AddJob : Job -> JobId
    abstract DeleteJob : AppId * JobId -> bool
    abstract TryGetStatus : AppId * JobId -> JobStatusWithInfo option
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

    let getQueuePosition (job : JobEntity) =
        try
            let q_queued = 
                TableQuery<JobEntity>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("Status", QueryComparisons.Equal, status JobStatus.Queued),
                                TableOperators.And,
                                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, job.Timestamp)),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, job.RowKey)))
                    .Select([| JobProperties.JobId |])
            let queued = table.ExecuteQuery(q_queued) |> Seq.map(fun j -> j.JobId) |> Seq.toArray
            if queued.Length = 0 then 0
            else
                let exec = tableExec.ExecuteQuery<JobExecutionEntity>(TableQuery<JobExecutionEntity>()) |> Seq.map(fun j -> Guid.Parse j.RowKey) |> Seq.toArray
                Set.difference (Set.ofArray queued) (Set.ofArray exec) |> Set.count                        
        with ex -> 
            logInfo (sprintf "Failed to count message queue position: %A" ex)
            -1


    do 
        table.CreateIfNotExists() |> ignore  
        tableExec.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore

    
    new (connectionString, maxNumberOfQueues, name:string) = 
        FairShareScheduler(
            {
                StorageAccount = CloudStorageAccount.Parse(connectionString)
                MaxNumberOfQueues = maxNumberOfQueues
                Name = name   
            })

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

        member x.TryGetStatus (appId: AppId, jobId: JobId) : JobStatusWithInfo option =
            match tryGetJobEntry (appId, jobId) table with
            | None ->
                logInfo (sprintf "Job %A not found" jobId)
                None
            | Some job ->
                match parseStatus job.Status with
                | JobStatus.Succeeded -> JobStatusWithInfo.Succeeded
                | JobStatus.Failed -> JobStatusWithInfo.Failed job.StatusInformation
                | _ ->
                    match isExecuting (appId, jobId) with 
                    | Some time -> // EXECUTING
                        JobStatusWithInfo.Executing time
                    | None -> // QUEUED
                        let queuePos = getQueuePosition job
                        JobStatusWithInfo.Queued queuePos
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