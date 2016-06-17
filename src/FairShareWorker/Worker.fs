namespace bma.Cloud

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.Storage.Table.Queryable
open bma.Cloud.Naming
open bma.Cloud.Jobs
open System.Diagnostics
open Microsoft.WindowsAzure.Storage.Queue

type WorkerSettings =
    { JobTimeout : TimeSpan
      Retries : int
      VisibilityTimeout : TimeSpan 
      CancellationCheckInterval : TimeSpan }

[<Interface>]
type IWorker =
    inherit IDisposable
    abstract Process : Func<Guid, IO.Stream, IO.Stream> * TimeSpan * TimeSpan -> unit

[<Class>]
type internal FairShareWorker(storageAccount : CloudStorageAccount, schedulerName : string, settings : WorkerSettings) =
    let rand = Random()
    let queueClient = storageAccount.CreateCloudQueueClient()
    let blobClient = storageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference (getBlobContainerName schedulerName)
    let tableClient = storageAccount.CreateCloudTableClient()
    let table = tableClient.GetTableReference (getJobsTableName schedulerName)   
    
    let mutable disp : IDisposable option = None     

    let tryGetJob (entryId:JobId, appId:AppId) : JobEntity option =
        let rkey = entryId.ToString()
        let pkey = appId.ToString()
        let retrieveOperation = TableOperation.Retrieve<JobEntity>(pkey, rkey)
        let r = table.Execute(retrieveOperation)
        match r.Result with
        | null -> None // couldn't be retrieved (considering as not found)
        | _ as job -> job :?> JobEntity |> Some

    let upsertStatus (job : JobEntity) (jobStatus:JobStatus) (info:string) =
        job.Status <- status jobStatus
        job.StatusInformation <- info
        let op = TableOperation.InsertOrReplace(job)
        let entity = table.Execute(op)
        match entity.Result with
        | null -> failwith "Couldn't retrieve job entry when upserting it"
        | _ -> ()

    let getJobRequest (blobName : string) =
        let blob = container.GetBlockBlobReference(blobName)
        let stream = new System.IO.MemoryStream()
        blob.DownloadToStream(stream)
        stream.Position <- 0L
        stream :> System.IO.Stream

    let putResult (resultBlobName:string) (result : IO.Stream) =                
        let blob = container.GetBlockBlobReference(resultBlobName)
        blob.UploadFromStream(result)

    let isCancellationRequested (entryId:JobId, appId:AppId) : bool =
        tryGetJob (entryId, appId) |> Option.isNone

    let poisonMessage (entryId, appId) =
        match tryGetJob (entryId, appId) with
        | Some job -> 
            job.RowKey <- Guid.NewGuid().ToString() 
            upsertStatus job JobStatus.Failed (sprintf "The worker failed more than %d times during execution of the job %A" settings.Retries job.JobId)
        | None -> () // nothing to do; the job is either cancelled or complete&cleared

    let handleMessage (doJob: Guid * IO.Stream -> IO.Stream) (entryId, appId) =
        match tryGetJob (entryId, appId) with
        | Some job -> // job status is "Queued"
            //sprintf "Received job ticket %O" jobId |> info
            job.RowKey <- Guid.NewGuid().ToString() // new entry id

            upsertStatus job JobStatus.Executing String.Empty

            use jobReq = getJobRequest job.Request

            let mutable result = Choice2Of2(null)
            try
                let r = ManageableAction.Do 
                            (fun () -> doJob (job.JobId, jobReq)) settings.JobTimeout // do this job with time limitation
                            (fun () -> isCancellationRequested (entryId, appId)) settings.CancellationCheckInterval  // check if cancellation is requested
                result <- Choice1Of2 r
                //sprintf "Job %O request is complete" jobId |> info
            with
            | exn -> // job failed
                //sprintf "Job %O failed" jobId |> error
                result <- Choice2Of2(exn)
                
            match result with
            | Choice1Of2 outcome ->
                outcome |> putResult job.Result
                outcome.Dispose()
                upsertStatus job JobStatus.Succeeded String.Empty
            | Choice2Of2 exn ->
                upsertStatus job JobStatus.Failed (sprintf "Job execution failed: %A" exn)
        | None -> () // nothing to do; the job is either cancelled or complete&cleared

    let selectQueue () =
        let query = 
            TableQuery<JobEntity>()
                .Where(TableQuery.GenerateFilterCondition(JobProperties.Status, QueryComparisons.Equal, status JobStatus.Queued))
                .Select([| JobProperties.QueueName |])
           
        let queueNames = table.ExecuteQuery(query) |> Seq.map(fun job -> job.QueueName) |> Seq.distinct |> Seq.toArray
        if queueNames.Length > 0 then
            let queueName = queueNames.[rand.Next queueNames.Length]
            let queue = queueClient.GetQueueReference(queueName)
            if queue.Exists() then
                //info (sprintf "Chosen queue is %s" queueName)
                Some queue
            else None
        else None

    let parse (m : CloudQueueMessage) = parseQueueMessage m.AsString

    let deleteMessage (queue : CloudQueue) (m : CloudQueueMessage) =
        try // todo: get rid of "Queued" entry otherwise it will participate in all "selectQueue" responses
            queue.DeleteMessage(m)
            Trace.WriteLine(sprintf "Message '%s' deleted from the queue" m.AsString)
        with
        | :? StorageException as ex when ex.RequestInformation.ExtendedErrorInformation.ErrorCode = "MessageNotFound" -> // we don't own the message already
            // http://blog.smarx.com/posts/deleting-windows-azure-queue-messages-handling-exceptions
            Trace.WriteLine(sprintf "Cannot delete the message '%s' because the pop receipt is invalid" m.AsString)
        | exn -> 
            raise exn 

    let clean (jobId, appId) =
        match tryGetJob (jobId, appId) with
        | Some job -> // removing the main job entry, where status is 'Queued'
            TableOperation.Delete job |> table.Execute |> ignore 
        | None -> // it was cancelled
            Trace.WriteLine(sprintf "Job %A was cancelled; cleaning the table" jobId)
            Jobs.deleteJob (appId, jobId) (table, container) |> ignore

    let tryNextMessage (doJob: Guid * IO.Stream -> IO.Stream) () =
        match selectQueue() with
        | Some queue ->            
            match queue.GetMessage(visibilityTimeout = Nullable settings.VisibilityTimeout) with       
            | null -> false          
            | m when m.DequeueCount < settings.Retries ->
                use lease = new AutoLeaseRenewal(queue, m, settings.VisibilityTimeout)
                let ticket = parse m
                ticket |> handleMessage doJob // fails only because of infrastructure problems
                deleteMessage queue m
                clean ticket
                true
            | m ->
                Trace.WriteLine(sprintf "Message '%s' is poisoned" m.AsString)
                let ticket = parse m
                ticket |> poisonMessage // the Jobs table will contain new entry where status is Failed.
                deleteMessage queue m
                clean ticket
                true
        | None -> false

    do
        container.CreateIfNotExists() |> ignore
        table.CreateIfNotExists() |> ignore
        

    interface IWorker with
        member x.Dispose() = disp |> Option.iter(fun d -> d.Dispose())
    
        member x.Process (doJob: Func<Guid, IO.Stream, IO.Stream>, pollingInterval: TimeSpan, maxPollingInterval: TimeSpan) =
            if(disp.IsSome) then failwith "The worker is already started"
            let onError (exc:exn) = // if tryNextMessage() failed
                // error "..."
                true // go on; the message will be taken by another worker
            PollingService.StartPolling(pollingInterval, maxPollingInterval, tryNextMessage (doJob.Invoke), onError)


[<Sealed>]
type Worker private () =
    static member Create (storageAccount : CloudStorageAccount, schedulerName : string, settings : WorkerSettings) : IWorker =
        upcast new FairShareWorker(storageAccount, schedulerName, settings)