namespace bma.Cloud

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.Storage.Table.Queryable
open bma.Cloud.Naming
open bma.Cloud.Jobs

[<Interface>]
type IWorker =
    inherit IDisposable
    abstract Process : Func<Guid, IO.Stream, IO.Stream> * TimeSpan * TimeSpan -> unit

[<Class>]
type internal FairShareWorker(storageAccount : CloudStorageAccount, schedulerName : string) =
    let rand = Random()
    let queueClient = storageAccount.CreateCloudQueueClient()
    let blobClient = storageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference (getBlobContainerName schedulerName)
    let tableClient = storageAccount.CreateCloudTableClient()
    let table = tableClient.GetTableReference (getJobsTableName schedulerName)   
    
    let mutable disp : IDisposable option = None     

    let handle (doJob: (Guid * IO.Stream -> IO.Stream), pollingInterval: TimeSpan, maxPollingInterval: TimeSpan) =
            if(disp.IsSome) then failwith "The worker is already started"

            let getJob (jobId:JobId, appId:AppId) =
                let rkey = jobId.ToString()
                let pkey = appId.ToString()
                let retrieveOperation = TableOperation.Retrieve<JobEntity>(pkey, rkey)
                let r = table.Execute(retrieveOperation)
                let job = r.Result :?> JobEntity
                job

            let updateStatus (job : JobEntity) (jobStatus:JobStatus) (resultBlobName:string option) =
                job.Status <- status jobStatus
                resultBlobName |> Option.iter (fun blobName -> job.Result <- blobName)
                let merge = TableOperation.Merge(job)
                table.Execute(merge) |> ignore

            let getJobRequest (blobName : string) =
                let blob = container.GetBlockBlobReference(blobName)
                let stream = new System.IO.MemoryStream()
                blob.DownloadToStream(stream)
                stream.Position <- 0L
                stream :> System.IO.Stream

            let putResult (jobId:JobId) (resultBlobName:string) (result : IO.Stream) =                
                let blob = container.GetBlockBlobReference(resultBlobName)
                blob.UploadFromStream(result)

            let tryHandleJob() =
                let query = 
                    TableQuery<JobEntity>()
                     .Where(
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition(JobProperties.Status, QueryComparisons.Equal, status JobStatus.Queued),
                            TableOperators.Or,
                            TableQuery.GenerateFilterCondition(JobProperties.Status, QueryComparisons.Equal, status JobStatus.Executing)))
                     .Select([| JobProperties.QueueName |])
           
                let queueNames = table.ExecuteQuery(query) |> Seq.map(fun job -> job.QueueName) |> Seq.distinct |> Seq.toArray
                if queueNames.Length > 0 then
                    let queueName = queueNames.[rand.Next queueNames.Length]
                    //info (sprintf "Chosen queue is %s" queueName)

                    let queue = queueClient.GetQueueReference(queueName)
                    queue.CreateIfNotExists() |> ignore

                    match queue.GetMessage(visibilityTimeout = Nullable(TimeSpan.FromHours 1.0)) with
                    | null -> 
                        //info "No message"
                        false
                    | m ->
                        let jobId, appId = parseQueueMessage m.AsString
                        let resultBlobName = getJobResultBlobName jobId schedulerName

                        //sprintf "Received job ticket %O" jobId |> info
                        let job = getJob (jobId, appId) 
                        updateStatus job JobStatus.Executing (Some resultBlobName)

                        use jobReq = getJobRequest job.Request
                        use result = doJob (jobId, jobReq)
                        //sprintf "Job %O request is complete" jobId |> info

                        result |> putResult jobId resultBlobName

                        queue.DeleteMessage(m)
                        // todo: check if delete failed
                        // TODO: what is this role is stopped before the status updated?
                        updateStatus job JobStatus.Succeeded None
                        true
                else
                    //info "No queued message in the job table"
                    false

            let onError (exc:exn) = // if tryHandleJob() failed
                // error "..."
                true // go on
        
            PollingService.StartPolling(pollingInterval, maxPollingInterval, tryHandleJob, onError)

    do
        container.CreateIfNotExists() |> ignore
        table.CreateIfNotExists() |> ignore
        

    interface IWorker with
        member x.Dispose() = disp |> Option.iter(fun d -> d.Dispose())
    
        member x.Process (doJob: Func<Guid, IO.Stream, IO.Stream>, pollingInterval: TimeSpan, maxPollingInterval: TimeSpan) =
            handle(doJob.Invoke, pollingInterval, maxPollingInterval)


[<Sealed>]
type Worker private () =
    static member Create (storageAccount : CloudStorageAccount, schedulerName : string) : IWorker =
        upcast new FairShareWorker(storageAccount,schedulerName)