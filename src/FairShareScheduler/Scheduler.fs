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

    let getJobEntries (appId: AppId, jobId: JobId) = 
        let query = 
            TableQuery<JobEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(JobProperties.AppId, QueryComparisons.Equal, appId.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForGuid(JobProperties.JobId, QueryComparisons.Equal, jobId)))
                //.Select([| JobProperties.EntryId; JobProperties.JobId; JobProperties.Status; JobProperties.StatusInformation; JobProperties.Result |])
        table.ExecuteQuery(query)


    do 
        table.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore


    interface IScheduler with

        member x.AddJob (job : Job) : JobId =
            let jobId = Guid.NewGuid()
            let entryId = jobId
            let queueIdx = Math.Abs(job.AppId.GetHashCode()) % settings.MaxNumberOfQueues
            let queueName = getQueueName queueIdx settings.Name

            let blobReq = container.GetBlockBlobReference (getJobRequestBlobName jobId settings.Name)
            blobReq.UploadFromStream(job.Body)

            let blobRes = container.GetBlockBlobReference (getJobResultBlobName jobId settings.Name)
            blobRes.UploadFromStream(job.Body)

            let jobEntity = JobEntity(entryId, job.AppId)
            jobEntity.JobId <- jobId
            jobEntity.Request <- blobReq.Name
            jobEntity.Result <- blobRes.Name
            jobEntity.Status <- status JobStatus.Queued
            jobEntity.QueueName <- queueName

            let insert = TableOperation.Insert(jobEntity)
            table.Execute(insert) |> ignore
            logInfo (sprintf "Job added to the table with jobId = %O, queue = %s" jobId queueName)

            let queue = queueClient.GetQueueReference(queueName)
            queue.CreateIfNotExists() |> ignore
            queue.AddMessage(CloudQueueMessage(buildQueueMessage(entryId, job.AppId)))
            logInfo (sprintf "Job %O is queued" jobId)
            jobId

        member x.TryGetStatus (appId: AppId, jobId: JobId) : (JobStatus * string) option =
            match getJobEntries (appId, jobId) |> Seq.toList with
            | [] ->
                logInfo (sprintf "Job %O is not found" jobId)
                None
            | jobEntries ->
                jobEntries 
                |> List.fold(fun (rs,rsi) s -> 
                    match parseStatus s.Status, rs with
                    | JobStatus.Succeeded, _ | _, JobStatus.Succeeded -> JobStatus.Succeeded, String.Empty
                    | JobStatus.Failed, _ | _, JobStatus.Failed -> JobStatus.Failed, s.StatusInformation
                    | JobStatus.Executing, _ | _, JobStatus.Executing -> JobStatus.Executing, String.Empty
                    | _ -> JobStatus.Queued, String.Empty) (JobStatus.Queued, String.Empty)
                |> Some

        member x.TryGetResult (appId: AppId, jobId: JobId) : IO.Stream option =
            match getJobEntries (appId, jobId) |> Seq.toList with
            | [] ->
                logInfo (sprintf "Job %O is not found" jobId)
                None
            | jobEntries -> // todo: use special table request: get status=succeeded top 1 select result
                jobEntries 
                |> List.tryPick(fun entry -> 
                    match parseStatus entry.Status with
                    | JobStatus.Succeeded -> Some entry.Result
                    | _ -> None)
                |> Option.map(getBlobContent)

        member x.DeleteJob (appId: AppId, jobId: JobId) : bool =
            Jobs.deleteJob (appId, jobId) (table, container)

