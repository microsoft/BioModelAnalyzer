// Learn more about F# at http://fsharp.net

(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Program

// Implementation of:
// Cook, Fisher, Krepska, Piterman.
// 'Proving stabilization of biological systems',
// VMCAI 2011.to

open System.Xml
open System.Xml.Linq

// Command-line args
let model  = ref "" // input model filename 
let proof_output = ref "" // output filename 

let simul_v0     = ref "" // initial values file (csv file, with idXvalue schema)
let simul_time   = ref 20 // max time to simulate
let simul_output = ref "" // output log/excel filename. 

let run_tests = ref false 
let logging = ref false 

let args = [ ("-model", ArgType.String (fun i -> model := i), "Input xml model");             
             //
             ("-prove",          ArgType.String (fun o -> proof_output := o), "Proof output filename");
             // 
             ("-simulate",       ArgType.String (fun o -> simul_output := o), "Simulation output filename");
             ("-simulate_time",  ArgType.Int (fun t -> simul_time := t), "Simulate for X ticks (default 20)");
             ("-simulate_v0", ArgType.String (fun f -> simul_v0 := f), "Simulation initial values (default 0s)"); 
             // 
             ("-tests",  ArgType.Unit (fun _ -> run_tests := true), "Run tests");
             ("-log",    ArgType.Unit (fun _ -> logging := true),  "Enable logging") ]
            |> List.map (fun (n,a,s) -> ArgInfo(n,a,s))
            |> List.toSeq

ArgParser.Parse(args)

type IA = BioCheckAnalyzerCommon.IAnalyzer2
let analyzer = UIMain.Analyzer2()

if !logging then (analyzer :> IA).LoggingOn(Log.AnalyzerLogService())   

// Do the proof 
if (!model <> "" && !proof_output <> "") then    
    Log.log_debug "Running the proof"
    let model = XDocument.Load(!model) |> Marshal.model_of_xml
    let (sr,cex_o) = Stabilize.stabilization_prover model
    match (sr,cex_o) with 
    | (Result.SRStabilizing(_), None) -> 
        let stable_res_xml = Marshal.xml_of_stability_result sr
        stable_res_xml.Save(!proof_output)
    | (Result.SRNotStabilizing(_), Some(cex)) -> 
        let unstable_res_xml = Marshal.xml_of_stability_result sr
        unstable_res_xml.Save(!proof_output)
        let cex_xml = Marshal.xml_of_cex_result cex
        let filename,ext = System.IO.Path.GetFileNameWithoutExtension !proof_output, System.IO.Path.GetExtension !proof_output
        cex_xml.Save(filename + "_cex." + ext)
    | _ -> failwith "bad results from stabilization_prover"

// Do the simulation
elif (!model <> "" && !simul_output <> "") then 
    Log.log_debug "Running the simulation"
    let qn = Marshal.model_of_xml (XDocument.Load !model)
    let init_values = 
        if (!simul_v0 <> "" && System.IO.File.Exists !simul_v0) then 
            let csv = System.IO.File.ReadAllLines !simul_v0 
            Array.fold (fun m (l:string) -> let ss = l.Split(',') in  Map.add ((int)ss.[0]) ((int)ss.[1]) m) Map.empty csv
        else List.fold (fun m (n:QN.node) -> Map.add n.var 0 m) Map.empty qn
    Log.log_debug (sprintf "time:%d init_values:[%s]" !simul_time (QN.str_of_env init_values))
    // SI: should check that dom(init_values) is complete wrt qn. 
    assert (QN.env_complete_wrt_qn qn init_values) 
    let final_values = Seq.toList (Simulate.simulate_many qn init_values !simul_time )
    Log.log_debug "Writing simulation log"
    let everything = String.concat "\n" (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) final_values)
    System.IO.File.WriteAllText(!simul_output, everything)
    // SI: check why the xlsx is sometimes corrupted ? 
    let (app,sheet) = ModelToExcel.model_to_excel qn !simul_time init_values
    Log.log_debug "Writing excel spreadsheet"
    ModelToExcel.saveSpreadsheet app sheet (!simul_output + ".xlsx")

// Run tests.     
elif (!run_tests) then 
    UnitTests.register_tests2 (analyzer)
    Expr.register_tests()
    Test.run_tests ()

// Incorrect flags. 
if ((!model = "" && !proof_output = "") && 
    (!model = "" && !simul_output= "") &&
    !run_tests = false) then  
    Printf.printfn "Please provide an input model, and prove or simulate output."
