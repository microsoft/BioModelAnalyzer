(* Copyright (c) Microsoft Corporation. All rights reserved. *)
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) 2008  Microsoft Corporation
//
//  Module Name:
//
//      log.fs
//
//  Abstract:
//
//      Central mechanism for controlling spew
//
//  Contact:
//
//      Byron Cook (bycook)
//
//  Environment:
//
//
//  Notes:
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
            member this.LogDebug msg = Printf.printf "%A: %s\n" System.DateTime.Now msg
            member this.LogError msg = Printf.printf "%A: %s\n" System.DateTime.Now msg

// Register LogService. 
// Saves given [log] ILogService to internal state. 
// Then allows us to expose [Log.log] as the global logging function, rather than passing the service handle around.
let log_service:(ILogService option ref) = ref None
let register_log_service (log:ILogService)  = 
    log_service := Some log

// Exposed log service calls
let log_debug s =
    match !log_service with 
    | Some logs -> logs.LogDebug s 
    | None -> ()

let log_error s =
    match !log_service with 
    | Some logs -> logs.LogError s 
    | None -> ()
