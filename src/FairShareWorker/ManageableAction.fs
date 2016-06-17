namespace bma.Cloud

open System.Threading
open System.Diagnostics
open System

type internal ManageableAction<'a> private (f : unit -> 'a, timeout: TimeSpan) =
    let ev = new ManualResetEvent(false)
    let mutable result = Choice1Of3 ()

    let doAction() =
        try
            result <- Choice2Of3 (f())
        with
        | :? ThreadAbortException as exn ->
            Trace.WriteLine(sprintf "Action interrupted")
            result <- Choice3Of3 (exn :> exn)
        | exn -> 
            Trace.WriteLine(sprintf "Action failed: %A" exn)
            result <- Choice3Of3 exn
        ev.Set() |> ignore

    let thread = Thread(doAction)

    do 
        thread.IsBackground <- true
        thread.Start()

    member x.Result =
        match ev.WaitOne(timeout) with
        | true ->
            match result with
            | Choice2Of3 r -> r
            | Choice3Of3 e -> raise e
            | Choice1Of3 _ -> failwith "The function hasn't succeeded to the result" // unreachable case
        | false -> 
            thread.Abort()
            Trace.WriteLine("Timeout while waiting for the function to succeed")
            raise (TimeoutException "Timeout while waiting for the function to succeed")

    interface IDisposable with
        member x.Dispose() =
            ev.Dispose()


    static member Do (f : unit -> 'a) (timeout: TimeSpan) : 'a =
        use action = new ManageableAction<'a>(f, timeout)
        action.Result

