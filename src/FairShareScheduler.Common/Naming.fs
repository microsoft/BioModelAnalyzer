// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module bma.Cloud.Naming

open System

let getName (objName: string) (schedulerName : string) = "fss" + schedulerName + objName
let getJobsTableName (schedulerName : string) = getName "jobs" schedulerName
let getJobsExecutionTableName (schedulerName : string) = getName "jobsexec" schedulerName
let getBlobContainerName (schedulerName : string) = getName "container" schedulerName
let getJobRequestBlobName (jobId : Guid) (schedulerName : string) = getName (jobId.ToString("N")) schedulerName
let getJobResultBlobName (jobId : Guid) (schedulerName : string) = getName (jobId.ToString("N") + "res") schedulerName
let getQueueName (queueIndex : int) (schedulerName : string) = getName (queueIndex.ToString()) schedulerName

