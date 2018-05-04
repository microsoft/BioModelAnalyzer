// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Main

// Implementations of:
// Hall; Pathfinding (PATH)
// Garvit Juniwal; Stability Synthesis (SYN)
// Garvit Juniwal; Shrink-Cut-Merge (SCM)
// Cook, Fisher, Krepska, Piterman; Proving stabilization of biological systems; VMCAI 2011.
// Claessen, Fisher, Ishtiaq, Piterman, Wang; Model-Checking Signal Transduction Networks through Decreasing Reachability Sets; CAV 2013.
// Woodhouse; Attractors

open Newtonsoft.Json
open Newtonsoft.Json.Linq

open BioModelAnalyzer 

//
// CL Parsing
//
type Engine = EngineCAV | EngineVMCAI | EngineSimulate | EngineSCM | EngineSYN | EnginePath | EngineVMCAIAsync | EngineAttractors
let engine_of_string s = 
    match s with 
    | "PATH" | "path" -> Some EnginePath
    | "SYN" | "syn" -> Some EngineSYN
    | "SCM" | "scm" -> Some EngineSCM
    | "CAV" | "cav" -> Some EngineCAV
    | "VMCAI" | "vmcai" -> Some EngineVMCAI 
    | "VMCAIASYNC" | "vmcaiasync" -> Some EngineVMCAIAsync
    | "Simulate" | "simulate" | "SIMULATE"-> Some EngineSimulate
    | "Attractors" | "attractors" | "ATTRACTORS" -> Some EngineAttractors
    | _ -> None 

// Command-line args
// -- General
let engine = ref None 
let model  = ref "" // input model filename 
let run_tests = ref false 
let logging = ref false 
let logging_level = ref 0

// -- QN transformations 
let dump_before_xforms = ref false
// ---- KO
let ko : (QN.var * int) list ref = ref [] // KO each of these vars, replacing their f's with the const. 
let ko_of_string v c =
    let v = try (int)v with _ -> failwith("-ko id const: id has to be integer identifier of var in qn")
    let c = try (int)c with _ -> failwith("-ko id const: const has to be an integer")
    (v,c)
let dump_after_ko_xforms = ref false
// ---- KO_edge
let ko_edge : (QN.var * QN.var * int) list ref = ref []
let ko_edge_of_string x y c = 
    let x = try (int)x with _ -> failwith("-ko_edge id id' const: id has to be integer identifier of var in qn")
    let y = try (int)y with _ -> failwith("-ko_edge id id' const: id' has to be integer identifier of var in qn")
    let c = try (int)c with _ -> failwith("-ko_edge id id' const: const has to be an integer")
    (x,y,c)
let dump_after_ko_edge_xforms = ref false

// -- related to VMCAI engine
let proof_output = ref "proof_output" // output filename 
// -- related to CAV engine
let formula = ref "True"
let number_of_steps = ref -1
let model_check = ref false
let no_sat = ref false
let modelsdir = ref ".\\" 
let output_model = ref false
let output_proof = ref false
// -- related to Simulate engine
let simul_v0     = ref "" // initial values file (csv file, with idXvalue schema per line)
let simul_time   = ref 20 // max time to simulate
let simul_output = ref "" // output log/excel filename. 
let excel_output = ref false // output the xlsx file as well 
// -- related to PATH engine
let model' = ref "" // input model filename for destination qn
let state  = ref "" // input csv describing starting state
let state' = ref "" // input csv describing destination state 
let ltloutputfilename = ref ""
// -- related to Attractor engine
let attractorInitialCsvFilename = ref "" // optional input filename
let attractorOut = ref "" // output filename
let attractorMode = ref Attractors.Sync

let usage i = 
    Printf.printfn "Usage: BioCheckConsole.exe -model input_analysis_file.json"
    Printf.printfn "                           -modelsdir model_directory"
    Printf.printfn "                           -log "
    Printf.printfn "                           -loglevel n"
    Printf.printfn "                         [ -engine [ SCM | SYN ] –prove output_file_name.json |"
    Printf.printfn "                           -engine [ VMCAI | VMCAIASYNC ] –prove output_file_name.json -nosat? |"
    Printf.printfn "                           -engine CAV –formula f –path length –mc?  -outputmodel? –proof? [-ltloutput filename.json]? |"
    Printf.printfn "                           -engine SIMULATE –simulate_v0 initial_value_input_file.csv –simulate_time t –simulate output_file_name.csv -excel? |"
    Printf.printfn "                           -engine ATTRACTORS -out output_file_name -async? [-initial initial.csv]? |"
    Printf.printfn "                           -engine PATH –model2 model2.json –state initial_state.csv –state2 target_state.csv ]"
    Printf.printfn "                           -dump_before_xforms"
    Printf.printfn "                           -ko id const -dump_after_ko_xforms"
    Printf.printfn "                           -ko_edge id id' const -dump_after_ko_edge_xforms"

let rec parse_args args = 
    match args with 
    | [] -> ()
    | "-model" :: m :: rest -> model := m; parse_args rest  
    | "-model2" :: m :: rest -> model' := m; parse_args rest
    | "-state" :: s :: rest -> state := s; parse_args rest
    | "-state2" :: s :: rest -> state' := s; parse_args rest
    | "-engine" :: e :: rest -> engine := engine_of_string e; parse_args rest
    | "-prove" :: o :: rest -> proof_output := o; parse_args rest 
    | "-simulate" :: o :: rest -> simul_output := o; parse_args rest 
    | "-simulate_time" :: t :: rest -> simul_time := (int)t; parse_args rest
    | "-simulate_v0" :: v0 :: rest -> simul_v0 := v0; parse_args rest
    | "-excel" :: rest -> excel_output := true; parse_args rest
    | "-formula" :: f :: rest -> formula := f; parse_args rest
    | "-mc" :: rest -> model_check := true; parse_args rest
    | "-nosat" :: rest -> no_sat := true; parse_args rest
    | "-outputmodel" :: rest -> output_model := true; parse_args rest
    | "-proof" :: rest -> output_proof := true; parse_args rest
    | "-path" :: i :: rest -> number_of_steps := (int)i; parse_args rest
    | "-modelsdir" :: d :: rest -> modelsdir := d; parse_args rest
    | "-dump_before_xforms" :: rest -> dump_before_xforms := true; parse_args rest 
    | "-ko" :: id :: konst :: rest -> ko := (ko_of_string id konst) :: !ko; parse_args rest 
    | "-dump_after_ko_xforms" :: rest -> dump_after_ko_xforms := true; parse_args rest 
    | "-ko_edge" :: id :: id' :: konst :: rest -> ko_edge := (ko_edge_of_string id id' konst) :: !ko_edge; parse_args rest
    | "-dump_after_ko_edge_xforms" :: rest -> dump_after_ko_edge_xforms := true; parse_args rest
    | "-tests" :: rest -> run_tests := true; parse_args rest
    | "-log" :: rest -> logging := true; parse_args rest
    | "-ltloutput" :: fn :: rest -> ltloutputfilename := fn; parse_args rest
    | "-loglevel" :: lvl :: rest -> logging_level := (int) lvl; parse_args rest
    | "-async" :: rest -> attractorMode := Attractors.Async; parse_args rest
    | "-out" :: o :: rest -> attractorOut := o; parse_args rest 
    | "-initial" :: i :: rest -> attractorInitialCsvFilename := i; parse_args rest
    | _ -> failwith "Bad command line args" 


let addVariableNames (model : Model) (layout : Model) =
    let findVariableName id (model : Model) =
        let vars = model.Variables
        let v = Array.find (fun (v : Model.Variable) -> v.Id = id) vars
        v.Name

    let addNameToVariable (var : Model.Variable) (name : string) =
        let mutable copy = var
        copy.Name <- name
        copy

    let addNameToVariables (vars : Model.Variable []) =
        Array.map (fun (var : Model.Variable) -> addNameToVariable var (findVariableName var.Id layout)) vars

    let mutable copy = model
    copy.Variables <- addNameToVariables model.Variables
    copy

//
// QN parsing and printing
//
let read_ModelFile_as_QN model_fname = 
    // Read file
    let jobj = JObject.Parse(System.IO.File.ReadAllText(model_fname))
    // Extract model from json
    let model = (jobj.["Model"] :?> JObject).ToObject<Model>()
    let layout = (jobj.["Layout"] :?> JObject).ToObject<Model>()  
             
    let model = addVariableNames model layout
    // model to QN
    let qn = Marshal.QN_of_Model model
    qn

let print_qn qn postfix =     
    List.iter (fun n -> Printf.printf "%s" (QN.str_of_node n)) qn 
    Printf.printf "%s" postfix

let write_json_to_file (fn:string) o =
    let ser = JsonSerializer ()
    let sw = new System.IO.StreamWriter(fn)
    let writer = new JsonTextWriter(sw)
    ser.Serialize(writer,o)
    writer.Close()

//
// engine wrappers
//
let runSCMEngine qn = 
    Log.log_debug "Running the proof"
    Log.log_debug (sprintf "Num of nodes %d" (List.length qn))
    for node in qn do
        Log.log_debug (QN.str_of_node node)
    let (stablePoint, cex) = Prover.ProveStability qn
    match (stablePoint, cex) with
    | (Some p, None) -> 
        printf "Single Stable Point %s" (Expr.str_of_env p)
    | (None, Some (Prover.Bifurcation(p1, p2))) -> printf "Multi Stable Points: \n %s \n %s" (Expr.str_of_env p1) (Expr.str_of_env p2)
    | (None, Some (Prover.Cycle(p, len))) -> printf "Cycle starting at \n %s \n of length %d" (Expr.str_of_env p) len 
    | _ -> failwith "Bad results from prover"

let runSYNEngine qn =
    Log.log_debug "Running Stability Suggestion Engine"
    let sug = Suggest.SuggestLoop qn
    match sug with
    | Suggest.Stable(p) -> Log.log_debug(sprintf "Single Stable Point \n %s" (Expr.str_of_env p))
    | Suggest.NoSuggestion(b) -> Log.log_debug(sprintf "No Suggestion Found \n %s" (QN.str_of_range qn b))
    | Suggest.Edges(edges, nature) -> 
            Log.log_debug(sprintf "Suggested edges: %s Nature: %A" (Suggest.edgelist_to_str edges) nature)

let runSimulateEngine qn (simul_output : string) start_state_file simulation_time excel_output =
    Log.log_debug "Running the simulation"
    // Format of init.csv: Each line is a var_id,value pair. So there will be as many lines as there are variables.
    let init_values = 
        if (start_state_file <> "" && System.IO.File.Exists start_state_file) then 
            let csv = System.IO.File.ReadAllLines start_state_file
            Array.fold (fun m (l:string) -> let ss = l.Split(',') in  Map.add ((int)ss.[0]) ((int)ss.[1]) m) Map.empty csv
        else List.fold (fun m (n:QN.node) -> Map.add n.var 0 m) Map.empty qn
    Log.log_debug (sprintf "time:%d init_values:[%s]" simulation_time (QN.str_of_env init_values))
    // SI: should check that dom(init_values) is complete wrt qn. 
    assert (QN.env_complete_wrt_qn qn init_values) 
    let final_values = Seq.toList (Simulate.simulate_many qn init_values simulation_time )
    Log.log_debug "Writing simulation log"
    let everything = String.concat "\n" (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) final_values)
    System.IO.File.WriteAllText(simul_output, everything)
    // SI: check why the xlsx is sometimes corrupted ? 
    if excel_output then
        let (app,sheet) = ModelToExcel.model_to_excel qn simulation_time init_values
        Log.log_debug "Writing excel spreadsheet"
        ModelToExcel.saveSpreadsheet app sheet (simul_output + ".xlsx")

let runVMCAIEngine qn (proof_output : string) (no_sat : bool) concurrencyType =
    Log.log_debug "Running the proof"
    let (sr,cex_o) = Stabilize.stabilization_prover qn no_sat concurrencyType
    match (sr,cex_o) with 
    | (Result.SRStabilizing(_), None) -> 
        write_json_to_file proof_output (Marshal.AnalysisResult_of_stability_result sr)
    | (Result.SRNotStabilizing(_), Some(cex)) -> 
        write_json_to_file proof_output (Marshal.AnalysisResult_of_stability_result sr)
        let filename,ext = System.IO.Path.GetFileNameWithoutExtension proof_output, System.IO.Path.GetExtension proof_output
        write_json_to_file (filename + "_cex" + ext) (Marshal.CounterExampleOutput_of_cex_result cex)
    | (Result.SRNotStabilizing(_), None) -> ()
    | _ -> failwith "Bad result from prover"

let runCAVEngine qn length_of_path formula model_check output_proof output_model ltl_output_filename =
    let ltl_formula_str = 
        if (model_check) then
            sprintf "(Not %s)" formula
        else
            formula

    let ltl_formula = LTL.string_to_LTL_formula ltl_formula_str qn false  

    LTL.print_in_order ltl_formula
    if (ltl_formula = LTL.Error) then
        ignore(LTL.unable_to_parse_formula)
    else             
        // Convert the interval based range to a list based range
        let nuRangel = Rangelist.nuRangel qn

        // find out the path with decreasing size, 
        // and the initial value of steps to be unrolled
        // Paths is the list of ranges
        // initK = the length of prefix + the length of loop, which is used as the initial value of K when doing BMC
        let paths = Paths.output_paths qn nuRangel

        if (output_proof) then
            Paths.print_paths qn paths

        // Extend/truncate the list of paths to the required length
        // If the list of paths is shorter than needed repeat the last element 
        // If the list of paths is longer than needed remove the prefix of the list
        let correct_length_paths = Paths.change_list_to_length paths length_of_path
    
        // given the # of steps and the path, do BMC   
        let check_both_polarities = false
        let (res1, model1, res2, model2) = 
                BMC.DoubleBoundedMCWithSim ltl_formula qn correct_length_paths check_both_polarities

        BioCheckPlusZ3.check_model model1 res1 qn

        LTL.print_in_order ltl_formula
        BioCheckPlusZ3.print_model model1 res1 qn output_model
        // BioCheckPlusZ3.print_model model2 res2 qn output_model

        let ltlResult = 
            match ltl_output_filename with
            | "" -> None //nothing to do here
            | _ -> Some (JsonConvert.SerializeObject(Marshal.ltl_result_full res1 model1))

        match ltlResult with
        | None -> ()
        | Some(result) -> System.IO.File.WriteAllText(ltl_output_filename, result)

let runPATHEngine qnX modelsdir other_model_name start_state dest_state =
    Log.log_debug "Running path search"
    let qnY = read_ModelFile_as_QN (modelsdir + "\\" + other_model_name)
    let X   = Array.fold (fun m (l:string) -> let ss = l.Split(',') in  Map.add ((int)ss.[0]) ((int)ss.[1]) m) Map.empty (System.IO.File.ReadAllLines start_state)
    let Y   = Array.fold (fun m (l:string) -> let ss = l.Split(',') in  Map.add ((int)ss.[0]) ((int)ss.[1]) m) Map.empty (System.IO.File.ReadAllLines dest_state)
    match (PathFinder.routes qnX qnY X Y) with
    | PathFinder.Failure(a,b) ->    Log.log_debug (sprintf "Found a way to escape one of the attractors. %A leads to %A" a b)
    | PathFinder.Success L    ->    Log.log_debug (sprintf "There are no escape routes between the attractors. %d states explored" L.safe.Length)
                                    printf "%s" (String.concat "\n" (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) L.safe))

let runAttractorEngine = Attractors.findAttractors 

//
// main
//
[<EntryPoint>]
let main args = 
    let res = ref 0

    try
        parse_args (List.ofArray args)

        if !logging then Log.register_log_service (Log.AnalyzerLogService())

        if (!run_tests) then 
            UnitTests.register_tests ()
            Test.run_tests ()
        
        elif (!model =  "") then 
            usage 1
            res := -1

        else 
            let qn = read_ModelFile_as_QN (!modelsdir + "\\" + !model) 

            // Apply QN xforms 
            if !dump_before_xforms then print_qn qn "\n"

            // Apply QN xforms: ko
            let qn = List.fold
                        (fun current_qn (var,c) -> QN.ko current_qn var c)
                        qn
                        !ko            
            if (!dump_after_ko_xforms) then print_qn qn "\n"

            // Apply QN xforms: ko_edge
            let qn = List.fold 
                        (fun current_qn (x,y,c) -> QN.ko_edge current_qn x y c)
                        qn
                        !ko_edge            
            if (!dump_after_ko_edge_xforms) then print_qn qn "\n"

            let parameters_were_ok = 
                match !engine with
                | Some EnginePath -> 
                    if (!model' <> "" && !state <> "" && !state' <> "") then runPATHEngine qn !modelsdir !model' !state !state'; true
                    else false 
                | Some EngineSYN -> runSYNEngine qn; true
                | Some EngineSCM -> runSCMEngine qn; true
                | Some EngineVMCAI ->
                    if (!proof_output <> "") then runVMCAIEngine qn !proof_output !no_sat Counterexample.Synchronous; true
                    else false
                | Some EngineVMCAIAsync ->
                    if (!proof_output <> "") then runVMCAIEngine qn !proof_output !no_sat Counterexample.Asynchronous; true
                    else false
                | Some EngineCAV -> runCAVEngine qn !number_of_steps !formula !model_check !output_proof !output_model !ltloutputfilename; true
                | Some EngineSimulate ->
                    if (!simul_output <> "") then runSimulateEngine qn !simul_output !simul_v0 !simul_time !excel_output; true
                    else false
                | Some EngineAttractors ->
                    if (!attractorOut <> "") then runAttractorEngine !attractorMode !attractorOut qn !attractorInitialCsvFilename; true
                    else false
                | none -> false

            if (not parameters_were_ok) then
                usage 1
                res := -1

        !res
    with
        | Failure(msg) -> 
            if (msg = "Bad command line args")
            then
              usage 1
            else
              Printf.printfn "Error: %s" msg
            -1





