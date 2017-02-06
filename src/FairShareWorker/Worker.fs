// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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


    let upsertExecution (jobId:JobId, appId:AppId) =
        let e = JobExecutionEntity(jobId, appId)
        let op = TableOperation.InsertOrReplace(e)
        let entity = tableExec.Execute(op)
        match entity.Result with
        | null -> failwith "Couldn't retrieve job execution entry when upserting it"
        | :? JobExecutionEntity -> ()
        | _ -> failwith "Unexpected type of the job execution entry"
        
    let deleteExecution (jobId:JobId, appId:AppId) =
        // http://stackoverflow.com/questions/16170915/best-practice-in-deleting-azure-table-entities-in-foreach-loop
        let e = DynamicTableEntity(appId.ToString(), jobId.ToString())
        e.ETag <- "*"
        let op = TableOperation.Delete(e)
        tableExec.Execute(op) |> ignore

    let getJobRequest (blobName : string) = getBlobContent blobName container

    let putResult (resultBlobName:string) (result : IO.Stream) =                
        let blob = container.GetBlockBlobReference(resultBlobName)
        blob.UploadFromStream(result)

    let isCancellationRequested (entryId:JobId, appId:AppId) : bool =
        tryGetJobEntry (appId, entryId) table |> Option.isNone
        
    // Returns true if the status is set to the succeeded or failed (by this worker or by another worker);
    // returns false if the job has been cancelled.
    let updateStatus (job:JobEntity) newStatus info =
        job.Status <- status newStatus
        job.StatusInformation <- info
        let update = TableOperation.Replace job
        try
            table.Execute update |> ignore
            true
        with
        | :? StorageException as se when se.RequestInformation.HttpStatusCode = 404 -> // entity was deleted, i.e. the job is cancelled
            Trace.WriteLine("Received 404 when tried to update status; the job has been cancelled")
            false
        | :? StorageException as se when se.RequestInformation.HttpStatusCode = 412 -> // update condition not satisfied, i.e. the job status has been already changed
            Trace.WriteLine("Received 412 when tried to update status; the job has been completed already")
            true
            

    let poisonMessage (jobId, appId) =
        try
            match tryGetJobEntry (appId, jobId) table with
            | Some job -> 
                updateStatus job JobStatus.Failed (sprintf "The job %O has failed %d times" jobId settings.Retries) |> ignore
                deleteBlob job.Result container |> ignore
                deleteExecution (jobId, appId)
            | None -> () // nothing to do; the job is either cancelled or complete&cleared
        with
        | exn ->
            Trace.WriteLine(sprintf "The job %O has failed %d times; failed to clean the job: %A" jobId settings.Retries exn)

    let handleMessage (doJob: Guid * IO.Stream -> IO.Stream) (jobId, appId) =
        match tryGetJobEntry (appId, jobId) table with
        | Some job when job.Status = "Queued" ->
            upsertExecution (jobId, appId) // inform that we are handling the job            
            Trace.WriteLine(sprintf "Job %O is started" jobId)
            
            use jobReq = getJobRequest job.Request
            let mutable result = Choice2Of2(null)
            try
                let r = ManageableAction.Do 
                            (fun () -> doJob (job.JobId, jobReq)) settings.JobTimeout // do this job with time limitation
                            (fun () -> isCancellationRequested (jobId, appId)) settings.CancellationCheckInterval // check if cancellation is requested
                result <- Choice1Of2 r
                Trace.WriteLine(sprintf "Job %O succeeded" jobId)
            with
            | exn -> // job failed
                Trace.WriteLine(sprintf "Job %O failed: %A" jobId exn)
                result <- Choice2Of2 exn
                        
            match result with
            | Choice1Of2 outcome ->
                outcome |> putResult job.Result
                outcome.Dispose()
                let notCancelled = updateStatus job JobStatus.Succeeded String.Empty
                if not notCancelled then deleteBlob job.Result container |> ignore
            | Choice2Of2 exn ->
                updateStatus job JobStatus.Failed (sprintf "Job execution failed: %A" exn) |> ignore
        
            deleteExecution (jobId, appId) 
            
        | Some _ // status is either Succeeded or Failed
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
            | m when m.DequeueCount <= settings.Retries ->
                use lease = new AutoLeaseRenewal(queue, m, settings.VisibilityTimeout)
                parse m |> handleMessage doJob // fails only because of infrastructure problems
                deleteMessage queue m
                true
            | m ->
                Trace.WriteLine(sprintf "Message '%s' is poisoned" m.AsString)
                parse m |> poisonMessage // the Jobs table will contain new entry where status is Failed.
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
