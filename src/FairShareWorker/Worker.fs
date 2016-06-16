namespace bma.Cloud

open System
open System.Threading
open System.Threading.Tasks
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

    let handleMessage (doJob: Guid * IO.Stream -> IO.Stream) (m : Queue.CloudQueueMessage) =
        let entryId, appId = parseQueueMessage m.AsString
        match tryGetJob (entryId, appId) with
        | Some job -> // job status is "Queued"
            //sprintf "Received job ticket %O" jobId |> info
            job.RowKey <- Guid.NewGuid().ToString() // new entry id

            upsertStatus job JobStatus.Executing String.Empty

            use jobReq = getJobRequest job.Request

            let mutable result = Choice2Of2(null)
            try
                result <- Choice1Of2(doJob (job.JobId, jobReq))
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

    let tryNextMessage (doJob: Guid * IO.Stream -> IO.Stream) () =
        match selectQueue() with
        | Some queue ->
            let visibilityTimeout = TimeSpan.FromMinutes 70.0
            match queue.GetMessage(visibilityTimeout = Nullable visibilityTimeout) with       
            | null -> false                 
            | m ->
                m |> handleMessage doJob // fails only because of infrastructure problems
                try
                    queue.DeleteMessage(m)
                    true
                with
                | :? StorageException as ex when ex.RequestInformation.ExtendedErrorInformation.ErrorCode = "MessageNotFound" -> // we don't own the message already
                    // http://blog.smarx.com/posts/deleting-windows-azure-queue-messages-handling-exceptions
                    // pop receipt must be invalid
                    true        
                    // trace "Cannot delete the message because the pop receipt is invalid"
                | exn -> 
                    raise exn
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
    static member Create (storageAccount : CloudStorageAccount, schedulerName : string) : IWorker =
        upcast new FairShareWorker(storageAccount,schedulerName)