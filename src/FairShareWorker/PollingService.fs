// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
namespace bma.Cloud

open System
open System.Threading

[<Sealed>]
type PollingService private () =

    static let loop (intervalMs : int, maxIntervalMs : int, callback: unit -> bool, onError: exn -> bool) = 
        let mutable cont = ref true
        let mutable interval = ref intervalMs
        while !cont do 
            System.Diagnostics.Trace.WriteLine(sprintf "Polling the service (current polling interval is %O)..." !interval)
            try
                match callback() with
                | true -> 
                    //interval := max intervalMs (!interval / 2)
                    interval := intervalMs
                | false -> 
                    Thread.Sleep(!interval)
                    interval := min maxIntervalMs (2 * !interval)
            with
            | exc -> cont := (onError exc)
       
    
    /// Starts the polling with given interval.
    /// If the `callback` returns true, the next callback is called immediately; 
    /// otherwise, it will be delayed with given interval.
    /// If `callback` fails, the `onError` function is called;
    /// if it returns false, the polling stops; otherwise it continues.
    static member StartPolling(interval : TimeSpan, maxInterval : TimeSpan, callback: unit -> bool, onError: exn -> bool) : unit =
        loop (int(interval.TotalMilliseconds), int(maxInterval.TotalMilliseconds), callback, onError)

