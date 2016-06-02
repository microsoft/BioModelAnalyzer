namespace bma.Cloud

open System
open Microsoft.WindowsAzure.Storage


[<Interface>]
type IWorker =
    inherit IDisposable
    abstract Start : (Guid * IO.Stream -> IO.Stream) * TimeSpan -> unit

[<Sealed>]
type Worker =
    static member Create : CloudStorageAccount * string -> IWorker