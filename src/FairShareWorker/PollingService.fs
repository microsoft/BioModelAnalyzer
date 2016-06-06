namespace bma.Cloud

open System
open System.Threading

[<Sealed>]
type PollingService private () =

    static let loop (intervalMs : int, callback: unit -> bool, onError: exn -> bool) = 
        let mutable cont = ref true
        while !cont do 
            try
                match callback() with
                | true -> ()
                | false -> Thread.Sleep(intervalMs)
            with
            | exc -> cont := (onError exc)
       
    
    /// Starts the polling with given interval.
    /// If the `callback` returns true, the next callback is called immediately; 
    /// otherwise, it will be delayed with given interval.
    /// If `callback` fails, the `onError` function is called;
    /// if it returns false, the polling stops; otherwise it continues.
    static member StartPolling(interval : TimeSpan, callback: unit -> bool, onError: exn -> bool) : unit =
        loop (int(interval.TotalMilliseconds), callback, onError)

