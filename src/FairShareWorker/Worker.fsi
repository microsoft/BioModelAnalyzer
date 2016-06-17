namespace bma.Cloud

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.WindowsAzure.Storage


type WorkerSettings =
    { JobTimeout : TimeSpan
      Retries : int
      VisibilityTimeout : TimeSpan }

[<Interface>]
type IWorker =
    inherit IDisposable
    abstract Process : Func<Guid, IO.Stream, IO.Stream> * TimeSpan * TimeSpan -> unit

[<Sealed>]
type Worker =
    static member Create : CloudStorageAccount * string * WorkerSettings -> IWorker