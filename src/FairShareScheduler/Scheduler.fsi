namespace bma.Cloud

open System
open System.IO
open Microsoft.WindowsAzure.Storage
open Jobs

type Job =
    { AppId: Guid
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
    abstract TryGetResult : AppId * JobId -> Stream option


type FairShareSchedulerSettings =
    { StorageAccount : CloudStorageAccount
      MaxNumberOfQueues : int
      Name: string }

[<Class>]
type FairShareScheduler =
    interface IScheduler
    new : FairShareSchedulerSettings -> FairShareScheduler
    new : connectionString:string * maxNumberOfQueues:int * name:string -> FairShareScheduler

    static member CleanAll : name:string -> storageAccount:CloudStorageAccount -> unit

