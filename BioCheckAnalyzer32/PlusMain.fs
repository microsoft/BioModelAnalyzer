(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module PlusMain
// SI: ToDo integrate this with Main 

open System.Xml
open System.Xml.Linq
open System.Diagnostics

open LTL

// Entry point
let plusmain _ =

    let input_file = ref ""
    let formula = ref "True"
    let number_of_steps = ref -1
    let naive_computation = ref false
    let model_check = ref false
    let modelsdir = ref "C:\\Users\\np183\\tools\\BioCheckPlus\\BioCheck\\xml Models"
    let output_model = ref false
    let output_proof = ref false

    let args = [ ("-file", ArgType.String (fun i -> input_file := i), "Name of Input File");
                 // 
                 ("-formula", ArgType.String (fun i -> formula := i), "Formula to check");
                 //
                 ("-mc", ArgType.Unit (fun _ -> model_check := true), "Model check the formula (vs search for a path satisfying the formula)");
                 //
                 ("-outputmodel", ArgType.Unit (fun _ -> output_model := true), "Output the model in case of satisfiability");
                 //
                 ("-naive", ArgType.Unit (fun _ -> naive_computation := true), "Compute naive representation of paths");
                 //
                 ("-proof", ArgType.Unit (fun _ -> output_proof := true), "Output the ranges of variables over time (does not work with naive)");
                 //
                 ("-path", ArgType.Int (fun i -> number_of_steps := i), "Number of steps"); 
                 //
                 ("-modelsdir", ArgType.String (fun d -> modelsdir := d), "Models directory"); 
                 
                 ]
               |> List.map (fun (n,a,s) -> ArgInfo(n,a,s))
               |> List.toSeq
    ArgParser.Parse(args)

    let start_time = System.DateTime.Now

    let file = !input_file

    // Negate the formula if needed
    let ltl_formula_str = 
        if (!model_check) then
            sprintf "(Not %s)" !formula
        else
            !formula
    let length_of_path = !number_of_steps

    // read out the vpc model from xml UI file
    // we cannot use the "range" here directly, as what we need is a list not a pair
    let network = Marshal.model_of_xml(XDocument.Load(!modelsdir + "\\" + file))
    let ltl_formula = LTL.string_to_LTL_formula ltl_formula_str network

    LTL.print_in_order ltl_formula
    if (ltl_formula = Error) then
        ignore(LTL.unable_to_parse_formula)
    else             
        // Convert the interval based range to a list based range
        let nuRangel = Rangelist.nuRangel network

        // find out the path with decreasing size, 
        // and the initial value of steps to be unrolled
        // Paths is the list of ranges
        // initK = the length of prefix + the length of loop, which is used as the initial value of K when doing BMC
        let paths = Paths.output_paths network nuRangel !naive_computation

        if (!output_proof && not !naive_computation) then
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
    
    let end_time = System.DateTime.Now
    let duration = end_time.Subtract start_time
    printfn "Time: %A" duration


    