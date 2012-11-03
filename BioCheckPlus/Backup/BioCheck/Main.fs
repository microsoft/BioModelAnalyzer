(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Main


open System.Xml
open System.Xml.Linq
open System.Diagnostics

open LTL

// Entry point
let main _ =

    let input_file = ref ""
    let formula = ref "True"
    let number_of_steps = ref 0
    let naive_computation = ref false

    let args = [ ("-file", ArgType.String (fun i -> input_file := i), "Name of Input File");
                 // 
                 ("-formula", ArgType.String (fun i -> formula := i), "Formula to check");
                 //
                 ("-naive", ArgType.Unit (fun _ -> naive_computation := true), "Compute naive representation of paths");
                 //
                 ("-path", ArgType.Int (fun i -> number_of_steps := i), "Number of steps"); ]
               |> List.map (fun (n,a,s) -> ArgInfo(n,a,s))
               |> List.toSeq
    ArgParser.Parse(args)

    let start_time = System.DateTime.Now

    let modelsdir = "C:\\Users\\np183\\tools\\BioCheckPlus\\BioCheck\\xml Models\\"
    //let file = "VerySmallTestCaseAnalysisInput.xml"
    //let file = "SmallTestCaseAnalysisInput.xml" 
    //let file = "VPC_lin15ko AnalysisInput.xml"
    //
    // Return to these three models! Why doesn't it find a loop?
    //
    //let file = "BooleanLoopAnalysisInput.xml"
    //let file = "NoLoopFoundAnalysisInput.xml"
    //let file = "Skin1D_TFAnalysisInput.xml"
    //let file = "Model4AnalysisInput.xml"
    let file = !input_file
    //let ltl_formula_str = "(Always True)"
    let ltl_formula_str = !formula
    // let mutable number_of_steps = 0
    let length_of_path = !number_of_steps

    // read out the vpc model from xml UI file
    // we cannot use the "range" here directly, as what we need is a list not a pair
    let network, range = Marshal.model_of_xml(XDocument.Load(modelsdir+file))
    let ltl_formula = LTL.string_to_LTL_formula ltl_formula_str network

 (*   if (ltl_formula = Error) then
        ignore(LTL.unable_to_parse_formula)
    else *)
    // Convert the interval based range to a list based range
    let nuRangel = Rangelist.nuRangel network

    // find out the path with decreasing size, 
    // and the initial value of steps to be unrolled
    // Paths is the list of ranges
    // initK = the length of prefix + the length of loop, which is used as the initial value of K when doing BMC
    let paths = Paths.OutputPaths network nuRangel !naive_computation

    // Extend the list of paths by repeating the last element the required number of times
    let padded_paths = Paths.pad_paths paths length_of_path
    
    // given the # of steps and the path, do BMC   
    let (res,model) =
        BMC.BoundedMC ltl_formula network nuRangel padded_paths

    BioCheckPlusZ3.print_model model res network
    
    let end_time = System.DateTime.Now
    let duration = end_time.Subtract start_time
    printfn "Time: %A" duration

main ()
    