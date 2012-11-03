(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module UIMain

// Alternative entry point for UI. 

open System.Xml
open System.Xml.Linq


// Entry points

let main_lazy xmodel =            
    // Convert the AnalysisInput xmodel into a QN.
    let network,range = Marshal.model_of_xml xmodel
    // Run the prover, returning each proof step.
    let results = Stabilize.stabilization_prover_lazy network range 
    // Convert each result to an AnalysisOutput xdoc
    Seq.map (fun r -> Marshal.xml_of_result_steps r) results
    

let main_oneshot xmodel = 
    // Call the lazy prover and take the last elt. 
    let results = main_lazy xmodel
    let result = Seq.nth ((Seq.length results) - 1) results 
    result


// Implement IAnalyzer interface
open BioCheckAnalyzerCommon
    
type Analyzer() = 
    interface IAnalyzer with 
        member this.OneShot input_model = 
            //Log.register_log_service (Log.AnalyzerLogService())
            main_oneshot input_model 
        member this.OneShot (input_model,log) = 
            Log.register_log_service log
            main_oneshot input_model 