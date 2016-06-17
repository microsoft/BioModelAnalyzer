namespace bma.Cloud

open System
open System.IO
open Microsoft.WindowsAzure.Storage
open Jobs

type Job =
    { AppId: Guid
      Body: Stream }

[<Interface>]
type IScheduler =
    abstract AddJob : Job -> JobId
    abstract DeleteJob : AppId * JobId -> bool
    abstract TryGetStatus : AppId * JobId -> (JobStatus * string) option
    abstract TryGetResult : AppId * JobId -> Stream option


type FairShareSchedulerSettings =
    { StorageAccount : CloudStorageAccount
      MaxNumberOfQueues : int
      Name: string }

[<Class>]
type FairShareScheduler =
    interface IScheduler
    new : FairShareSchedulerSettings -> FairShareScheduler

