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
    let tableExec = tableClient.GetTableReference (getJobsExecutionTableName schedulerName)  
    
    let mutable disp : IDisposable option = None     

    let tryGetJob (jobId:JobId, appId:AppId) : JobEntity option =
        let rkey = jobId.ToString()
        let pkey = appId.ToString()
        let retrieveOperation = TableOperation.Retrieve<JobEntity>(pkey, rkey)
        let r = table.Execute(retrieveOperation)
        match r.Result with
        | null -> None // couldn't be retrieved (considering as not found)
        | _ as job -> job :?> JobEntity |> Some

    let upsertExecution (jobId:JobId, appId:AppId) =
        let e = JobExecutionEntity(jobId, appId)
        let op = TableOperation.InsertOrReplace(e)
        let entity = table.Execute(op)
        match entity.Result with
        | null -> failwith "Couldn't retrieve job execution entry when upserting it"
        | _ -> ()
        
    let deleteExecution (jobId:JobId, appId:AppId) =
        let e = JobExecutionEntity(jobId, appId)
        let op = TableOperation.Delete(e)
        let entity = table.Execute(op)
        match entity.Result with
        | null -> failwith "Couldn't retrieve job execution entry when upserting it"
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
        
    let updateStatus (job:JobEntity) status info =
        job.Status <- status
        job.StatusInformation <- info
        let update = TableOperation.Replace job
        let res = table.Execute update 
        match res.HttpStatusCode with // https://msdn.microsoft.com/en-us/library/azure/dd179438.aspx
        | 204 (* NoContent *) -> //https://msdn.microsoft.com/en-us/library/azure/dd179427.aspx
            true
        | 409 (* Conflict *) -> // found but was changed - that's ok, someone else already did the job
            true
        | 404 (* Not found *) -> // !!! CHECK THE RESPONSE
            false
            

    let poisonMessage (entryId, appId) =
        match tryGetJob (entryId, appId) with
        | Some job -> 
            updateStatus job JobStatus.Failed (sprintf "The job %A failed more than %d times" settings.Retries job.JobId) |> ignore
        | None -> () // nothing to do; the job is either cancelled or complete&cleared

    let handleMessage (doJob: Guid * IO.Stream -> IO.Stream) (jobId, appId) =
        match tryGetJob (jobId, appId) with
        | Some job when job.Status = "Queued" ->
            upsertExecution (jobId, appId) // inform that we are handling the job            
            Trace.WriteLine(sprintf "Job %O is started" jobId)
            
            use jobReq = getJobRequest job.Request
            let mutable result = Choice2Of2(null)
            try
                let r = ManageableAction.Do 
                            (fun () -> doJob (job.JobId, jobReq)) settings.JobTimeout // do this job with time limitation
                            (fun () -> isCancellationRequested (jobId, appId)) settings.CancellationCheckInterval // check if cancellation is requested
                result <- Choice1Of2 
                Trace.WriteLine(sprintf "Job %O succeeded" jobId)
            with
            | exn -> // job failed
                Trace.WriteLine(sprintf "Job %O failed: %A" jobId exn)
                result <- Choice2Of2(exn)
                        
            match result with
            | Choice1Of2 outcome ->
                outcome |> putResult job.Result
                outcome.Dispose()
                let notCancelled = updateStatus job JobStatus.Succeeded String.Empty
                if not notCancelled then deleteBlob job.Result
            | Choice2Of2 exn ->
                updateStatus job JobStatus.Failed (sprintf "Job execution failed: %A" exn)
        
            deleteExecution (jobId, appId) 
            
        | Some job // status is either Succeeded or Failed
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
                true
            | m ->
                Trace.WriteLine(sprintf "Message '%s' is poisoned" m.AsString)
                let ticket = parse m
                ticket |> poisonMessage // the Jobs table will contain new entry where status is Failed.
                deleteMessage queue m
                true
        | None -> false

    do
        container.CreateIfNotExists() |> ignore
        table.CreateIfNotExists() |> ignore
        tableExec.CreateIfNotExists() |> ignore
        

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