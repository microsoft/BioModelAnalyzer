// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
namespace bma.Cloud

open System.Threading
open System.Threading.Tasks
open System.Diagnostics
open System

type internal ManageableAction<'a> private (f : unit -> 'a, timeout: TimeSpan, isCancellationRequired: unit -> bool, cancelCheckInterval : TimeSpan) =
    let evDone = new ManualResetEvent(false) // Will be destroyed by finalizer, see http://stackoverflow.com/questions/2234128/do-i-need-to-call-close-on-a-manualresetevent
    let evCancel = new ManualResetEvent(false)

    let mutable result = Choice1Of3 ()
    let mutable doCheck = true

    let doAction() =
        try
            result <- Choice2Of3 (f())
        with
        | :? ThreadAbortException ->
            Trace.WriteLine(sprintf "Action interrupted") // exn is rethrown here
        | exn -> 
            Trace.WriteLine(sprintf "Action failed: %A" exn)
            result <- Choice3Of3 exn
        evDone.Set() |> ignore

    let thread = Thread(doAction)
    
    let watchCancellation() =
        Thread.Sleep(cancelCheckInterval)
        while doCheck do
            try
                Trace.WriteLine(sprintf "Checking if the cancellation is required...")
                if isCancellationRequired() then 
                    doCheck <- false
                    evCancel.Set() |> ignore                    
                else
                    Thread.Sleep(cancelCheckInterval)
            with
            | exn -> 
                Trace.WriteLine(sprintf "Exception while checking if cancellation is required: %A" exn)
                Thread.Sleep(cancelCheckInterval)                    
        Trace.WriteLine(sprintf "Stopped watching whether cancellation is required.")


    let cancelThread = Thread(watchCancellation)

    do 
        thread.IsBackground <- true
        thread.Start()
        cancelThread.IsBackground <- true
        cancelThread.Start()

    member x.Result =
        match WaitHandle.WaitAny([| evDone :> WaitHandle; evCancel  :> WaitHandle |], timeout) with
        | 0 -> // done        
            Trace.WriteLine(sprintf "Action succeeded.")
            doCheck <- false
            match result with
            | Choice2Of3 r -> r
            | Choice3Of3 e -> raise e
            | Choice1Of3 _ -> failwith "The function hasn't succeeded to the result" // unreachable case
        | 1 -> // cancelled
            Trace.WriteLine("Action is cancelled")
            thread.Abort()
            thread.Join() // We must wait for the thread to join because of Z3 which cannot be run in parallel
            raise (OperationCanceledException())
        | WaitHandle.WaitTimeout | _ -> 
            doCheck <- false
            thread.Abort()
            thread.Join() // We must wait for the thread to join because of Z3 which cannot be run in parallel
            Trace.WriteLine("Timeout while waiting for the function to succeed")
            raise (TimeoutException "Timeout while waiting for the function to succeed")

    static member Do (f : unit -> 'a) (timeout: TimeSpan) (isCancellationRequired: unit -> bool) (cancelCheckInterval : TimeSpan) : 'a =
        let action = new ManageableAction<'a>(f, timeout, isCancellationRequired, cancelCheckInterval)
        action.Result

