namespace bma.Cloud

open System
open System.IO
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.Storage.Queue
open Microsoft.WindowsAzure.Storage.Blob
open bma.Cloud.Trace
open bma.Cloud.Jobs

type Job =
    { AppId: AppId
      Body: Stream }



[<Interface>]
type IScheduler =
    abstract AddJob : Job -> JobId


type FairShareSchedulerSettings =
    { StorageAccount : CloudStorageAccount
      MaxNumberOfQueues : int
      Name: string }

[<Class>]
type FairShareScheduler(settings : FairShareSchedulerSettings) =

    let getName (s: string) = "fss" + settings.Name + s

    let queueClient = settings.StorageAccount.CreateCloudQueueClient()      
    let tableClient = settings.StorageAccount.CreateCloudTableClient()
    let table = tableClient.GetTableReference(getName "jobs")
    let blobClient = settings.StorageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference(getName "container")
    
    
    do 
        table.CreateIfNotExists() |> ignore  
        container.CreateIfNotExists() |> ignore


    interface IScheduler with

        member x.AddJob (job : Job) : JobId =
            let jobId = Guid.NewGuid()

            let queueIdx = Math.Abs(job.AppId.GetHashCode()) % settings.MaxNumberOfQueues
            let queueName = getName (queueIdx.ToString())

            let blob = container.GetBlockBlobReference(getName (jobId.ToString("N")))
            blob.UploadFromStream(job.Body)

            let jobEntity = JobEntity(jobId, job.AppId)
            jobEntity.request <- blob.Name
            jobEntity.status <- "Queued"
            jobEntity.queueName <- queueName

            let insert = TableOperation.Insert(jobEntity)
            table.Execute(insert) |> ignore
            logInfo (sprintf "Job added to the table with id = %O, queue = %s" jobId queueName)

            let queue = queueClient.GetQueueReference(queueName)
            queue.CreateIfNotExists() |> ignore
            queue.AddMessage(CloudQueueMessage(buildQueueMessage(jobId, job.AppId)))
            logInfo (sprintf "Job %O is queued" jobId)
            jobId