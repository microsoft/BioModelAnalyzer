namespace bma.Cloud

open System
open System.Threading

[<Sealed>]
type PollingService private () =

    static let rec loop (intervalMs : int, callback: unit -> bool, onError: exn -> bool) =        
        async {
            let! cont =
                try                
                    match callback() with
                    | true -> async.Return true // continue immediately
                    | false -> 
                        async {
                            do! Async.Sleep intervalMs
                            return true
                        }
                with
                | exc -> async.Return (onError exc)                    
            if cont then return! loop (intervalMs, callback, onError)
            else return ()
        } 
    
    /// Starts the polling with given interval.
    /// If the `callback` returns true, the next callback is called immediately; 
    /// otherwise, it will be delayed with given interval.
    /// If `callback` fails, the `onError` function is called;
    /// if it returns false, the polling stops; otherwise it continues.
    static member StartPolling(interval : TimeSpan, callback: unit -> bool, onError: exn -> bool) : Async<unit> =
        loop (int(interval.TotalMilliseconds), callback, onError)

