(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Main

// Implementations of:
// Garvit Juniwal; Stability Synthesis (SYN)
// Garvit Juniwal; Shrink-Cut-Merge (SCM)
// Cook, Fisher, Krepska, Piterman; Proving stabilization of biological systems; VMCAI 2011.
// Claessen, Fisher, Ishtiaq, Piterman, Wang; Model-Checking Signal Transduction Networks through Decreasing Reachability Sets; CAV 2013.
open System.Xml
open System.Xml.Linq

type Engine = EngineCAV | EngineVMCAI | EngineSimulate | EngineSCM | EngineSYN
let engine_of_string s = 
    match s with 
    | "SYN" | "syn" -> Some EngineSYN
    | "SCM" | "scm" -> Some EngineSCM
    | "CAV" | "cav" -> Some EngineCAV
    | "VMCAI" | "vmcai" -> Some EngineVMCAI 
    | "Simulate" | "simulate" -> Some EngineSimulate
    | _ -> None 

// Command-line args
// -- General
let engine = ref None 
let model  = ref "" // input model filename 
let run_tests = ref false 
let logging = ref false 
let logging_level = ref 0

// -- related to VMCAI engine
let proof_output = ref "proof_output" // output filename 
// -- related to CAV engine
let formula = ref "True"
let number_of_steps = ref -1
let model_check = ref false
let modelsdir = ref ".\\" 
let output_model = ref false
let output_proof = ref false
// -- related to Simulate engine
let simul_v0     = ref "" // initial values file (csv file, with idXvalue schema per line)
let simul_time   = ref 20 // max time to simulate
let simul_output = ref "" // output log/excel filename. 

let rec parse_args args = 
    match args with 
    | [] -> ()
    | "-model" :: m :: rest -> model := m; parse_args rest
    | "-engine" :: e :: rest -> engine := engine_of_string e; parse_args rest
    | "-prove" :: o :: rest -> proof_output := o; parse_args rest 
    | "-simulate" :: o :: rest -> simul_output := o; parse_args rest 
    | "-simulate_time" :: t :: rest -> simul_time := (int)t; parse_args rest
    | "-simulate_v0" :: v0 :: rest -> simul_v0 := v0; parse_args rest
    | "-formula" :: f :: rest -> formula := f; parse_args rest
    | "-mc" :: rest -> model_check := true; parse_args rest
    | "-outputmodel" :: rest -> output_model := true; parse_args rest
    | "-proof" :: rest -> output_proof := true; parse_args rest
    | "-path" :: i :: rest -> number_of_steps := (int)i; parse_args rest
    | "-modelsdir" :: d :: rest -> modelsdir := d; parse_args rest
    | "-tests" :: rest -> run_tests := true; parse_args rest
    | "-log" :: rest -> logging := true; parse_args rest
    | "-loglevel" :: lvl :: rest -> logging_level := (int) lvl; parse_args rest
    | _ -> failwith "Bad command line args" 

//type IA = BioCheckAnalyzerCommon.IAnalyzer2
//let analyzer = UIMain.Analyzer2()

[<EntryPoint>]
let main args = 
    let res = ref 0
    
    parse_args (List.ofArray args)

    if !logging then Log.register_log_service (Log.AnalyzerLogService())

    //Run SYN engine
    if (!model <> "" && !engine = Some EngineSYN) then
        Log.log_debug "Running Stability Suggestion Engine"
        let model = XDocument.Load(!modelsdir + "\\" + !model) |> Marshal.model_of_xml
        let sug = Suggest.SuggestLoop model
        match sug with
        | Suggest.Stable(p) -> Log.log_debug(sprintf "Single Stable Point \n %s" (Expr.str_of_env p))
        | Suggest.NoSuggestion(b) -> Log.log_debug(sprintf "No Suggestion Found \n %s" (QN.str_of_range model b))
        | Suggest.Edges(edges, nature) -> 
                Log.log_debug(sprintf "Suggested edges: %s Nature: %A" (Suggest.edgelist_to_str edges) nature)

    //Run SCM engine
    if (!model <> "" && !engine = Some EngineSCM) then    
        Log.log_debug "Running the proof"
        let model = XDocument.Load(!modelsdir + "\\" + !model) |> Marshal.model_of_xml
        Log.log_debug (sprintf "Num of nodes %d" (List.length model))
        for node in model do
            Log.log_debug (QN.str_of_node node)
        let (stablePoint, cex) = Prover.ProveStability model
        match (stablePoint, cex) with
        | (Some p, None) -> 
            Log.log_debug(sprintf "Single Stable Point %s" (Expr.str_of_env p))
            //let stable_res_xml = Marshal.xml_of_smap p
            //stable_res_xml.Save(!proof_output)
        | (None, Some (Prover.Bifurcation(p1, p2))) -> Log.log_debug(sprintf "Multi Stable Points: \n %s \n %s" (Expr.str_of_env p1) (Expr.str_of_env p2))
        | (None, Some (Prover.Cycle(p, len))) -> Log.log_debug(sprintf "Cycle starting at \n %s \n of length %d" (Expr.str_of_env p) len)
        | _ -> failwith "Bad results from prover"
        

    // Run VMCAI engine
    if (!model <> "" && !engine = Some EngineVMCAI &&
        !proof_output <> "") then    
        Log.log_debug "Running the proof"
        let model = XDocument.Load(!modelsdir + "\\" + !model) |> Marshal.model_of_xml
//        Log.log_debug (sprintf "Num of nodes %d" (List.length model))
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
        | (Result.SRNotStabilizing(_), None) -> ()
        | _ -> failwith "Bad result from prover"

    // Run CAV engine
    elif (!model <> "" && !engine = Some EngineCAV) then 
            // Negate the formula if needed
            let ltl_formula_str = 
                if (!model_check) then
                    sprintf "(Not %s)" !formula
                else
                    !formula
            let length_of_path = !number_of_steps // SI: only used once, just use !num_of_steps in change_list below? 

            let network = Marshal.model_of_xml(XDocument.Load(!modelsdir + "\\" + !model))
        
            let ltl_formula = LTL.string_to_LTL_formula ltl_formula_str network 

            LTL.print_in_order ltl_formula
            if (ltl_formula = LTL.Error) then
                ignore(LTL.unable_to_parse_formula)
            else             
                // Convert the interval based range to a list based range
                let nuRangel = Rangelist.nuRangel network

                // find out the path with decreasing size, 
                // and the initial value of steps to be unrolled
                // Paths is the list of ranges
                // initK = the length of prefix + the length of loop, which is used as the initial value of K when doing BMC
                let paths = Paths.output_paths network nuRangel

                if (!output_proof) then
                    Paths.print_paths network paths

                // Extend/truncate the list of paths to the required length
                // If the list of paths is shorter than needed repeat the last element 
                // If the list of paths is longer than needed remove the prefix of the list
                let correct_length_paths = Paths.change_list_to_length paths length_of_path
    
                // given the # of steps and the path, do BMC   
                let (res,model) =
                    BMC.BoundedMC ltl_formula network nuRangel correct_length_paths

                BioCheckPlusZ3.check_model model res network
                BioCheckPlusZ3.print_model model res network !output_model

    // Run Simulation engine
    elif (!model <> "" && !engine = Some EngineSimulate &&
          !simul_output <> "") then 
        Log.log_debug "Running the simulation"
        let qn = Marshal.model_of_xml (XDocument.Load !model)
        // Format of init.csv: Each line is a var_id,value pair. So there will be as many lines as there are variables.
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
    //    UnitTests.register_tests2 (analyzer)
        Expr.register_tests()
        Test.run_tests ()

    // Incorrect flags. 
    if ((!model = "" && !engine = None) && !run_tests = false) then  
        Printf.printfn "Please provide an input model, and prove or simulate output."
        res := -1

    !res



