// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      Log.fs
//
//  Abstract:
//
//      Logging routines
//
//  Contact:
//      Byron Cook (bycook@microsoft.com)
//      Garvit Juniwal (garvitjuniwal@eecs.berkeley.edu)
//
//


#light

module Log

//open Microsoft.FSharp.Core.Operators.Checked
open BioCheckAnalyzerCommon

// Argument parsing -----------------------------------------------------

let print_log = ref true//false
let start_time = ref System.DateTime.Now

//let args = [ ( "-log"
//             , ArgType.Unit (fun () -> print_log := true)
//             , "Turn on verbose logging"
//             )
//           ]
//let do_logging () = !print_log
    
// Default log service 
type AnalyzerLogService() = 
        interface ILogService with
            member this.LogDebug msg = 
                let now = System.DateTime.Now 
                let yr,mm,dd,hr,min,ss,ms = now.Year,now.Month,now.Day,now.Hour,now.Minute,now.Second,now.Millisecond
                Printf.printfn "%d-%d-%d %d-%d-%d-%d: %s" yr mm dd hr min ss ms msg
            member this.LogError msg = Printf.printfn "%A: %s" System.DateTime.Now msg

// Register LogService. 
// Saves given [log] ILogService to internal state. 
// Then allows us to expose [Log.log] as the global logging function, rather than passing the service handle around.
let log_service:(ILogService option ref) = ref None
let register_log_service (log:ILogService) = 
    log_service := Some log
let deregister_log_service () = 
    log_service := None

// Exposed log service calls
let log_debug s =
    match !log_service with 
    | Some logs -> logs.LogDebug s 
    | None -> ()

let log_debug_fmt fmt = 
    match !log_service with 
    | Some s -> s.LogDebug (Printf.sprintf fmt) 
    | None -> ()

let log_error s =
    match !log_service with 
    | Some logs -> logs.LogError s 
    | None -> ()
