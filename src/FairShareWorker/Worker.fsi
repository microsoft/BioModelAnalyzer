namespace bma.Cloud

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.WindowsAzure.Storage


[<Interface>]
type IWorker =
    inherit IDisposable
    abstract Process : (Guid * IO.Stream -> IO.Stream) * TimeSpan -> Async<unit>
    abstract ProcessAsync : Func<Guid, IO.Stream, IO.Stream> * TimeSpan * CancellationToken -> Task

[<Sealed>]
type Worker =
    static member Create : CloudStorageAccount * string -> IWorker