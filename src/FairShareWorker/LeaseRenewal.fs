namespace bma.Cloud

open Microsoft.WindowsAzure.Storage.Queue
open System
open System.Threading
open System.Diagnostics

type AutoLeaseRenewal(queue : CloudQueue, message : CloudQueueMessage, interval : TimeSpan) =
    let stop = new ManualResetEvent(false)

    let threadFunc() =
        let period = int(interval.TotalMilliseconds * 0.75)
        try
            while not (stop.WaitOne period) do
                Trace.WriteLine("Updating message lease...")
                queue.UpdateMessage(message, interval, MessageUpdateFields.Visibility)
        with
        | exn -> 
            Trace.WriteLine(sprintf "AutoLeaseRenewal: thread function failed: %A" exn)

    let thread = new Thread(threadFunc)
    let mutable isDisposed = false

    do
        thread.Start()

    member x.Stop() =
        if not isDisposed then
            isDisposed <- true
            try
                Trace.WriteLine("Auto lease renewal is being stopped...")
                if stop.Set() && thread.Join(TimeSpan.FromMinutes 1.0) then 
                    Trace.WriteLine("Auto lease renewal stopped")
                    stop.Dispose()
                    true
                else
                    Trace.WriteLine("Auto lease renewal thread wasn't stopped")
                    false
            with
            | exn ->
                Trace.WriteLine(sprintf "Failed to stop AutoLeaseRenewal: %A" exn)
                false
        else false

    interface IDisposable with
        member x.Dispose() =
            x.Stop() |> ignore