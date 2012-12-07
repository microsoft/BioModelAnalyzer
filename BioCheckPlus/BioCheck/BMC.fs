(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module BMC

open Microsoft.Z3

open System.Xml
open System.Xml.Linq
open System.Diagnostics

open LTL


//  Qinsi's encoding of the automaton to Z3
let encode_automaton network paths ctx initK = 
    /// The ltl (TGBA) encoding part
    //Process.Start("ltl2tgba2.exe", "vpcp3.txt").WaitForExit()
    let automaton = List.filter (fun x -> x <> "") (List.ofSeq(LTL2TGBA.readLines("true.txt")))
    
    let (locations, transitions, accset) = ReadTGBA.LocTransAcc automaton
    // classify the transitions to obtain the AccSet F
    let accRelTransSet = AccRelatedTrans.AccClassification accset transitions
    //printf "%A" accRelTransSet
    let transToBeHandled = TGBATransEncoding.ClassifyTrans transitions
    //assert initial location
    TGBATransEncoding.assetInitLoc locations ctx
    //assert the transitions for initK steps
    for currStep in [0..initK] do
        let assertionForAllTrans = TGBATransEncoding.assertTransAtOneStep transToBeHandled currStep ctx
        ctx.AssertCnstr assertionForAllTrans 
        //printfn "%d" currStep
    printfn "%s" "finish assertion for unloop conditions"
    // from now on, all the assertions will be k-depandent, thus we need to add push and pop here
    ctx.Push()
    // first, I just consider the initK condition
    
    //assert the entire loop condition
    let mutable assertionForLoop = ctx.MkFalse()
    let mutable loopStart = 0
    while loopStart <= initK do
        // first, the loop condition for system part, denoted as (l)_L_(k), which means the state S(l) and S(k) are the same
        // detailedly, all the variables in states share the same values
        let assertionForOnePairOfStates = SameStateForSys.assertSameStatesForSys network loopStart initK ctx
        
        let assertionForAllAccSubsets = 
            if accset.Length > 0 &&  accset.Head <> "NULL" then 
                BuchiAcc.Z3ForAccCon accRelTransSet loopStart initK ctx
            else
                ctx.MkTrue()
        //printfn "%A" assertionForAllAccSubsets
        let assertionForOnePairOfLocs = SameLocationForTGBA.assertSameLocsForTGBA locations loopStart initK ctx
        assertionForLoop <- ctx.MkOr(assertionForLoop, ctx.MkAnd(assertionForOnePairOfStates, ctx.MkAnd(assertionForAllAccSubsets, assertionForOnePairOfLocs)))
        loopStart <- loopStart + 1
        //printfn "%d" loopStart
    ctx.AssertCnstr assertionForLoop

    printfn "%s" "finish all assertions in K steps"


let BoundedMC (ltl_formula : LTLFormulaType) network initBound (paths : Map<QN.var,int list> list) =
    
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
   
    let model = ref null
    ctx.Push()

    /// The system encoding part
    
    // 0. Pad the paths with the last element in it to the right length.
    
    // 1. encode_the_time_variables_for_z3 for the mutual agreement on where the loop closes
    InitEncodingForSys.encode_loop_closure_variables ctx paths.Length ;

    // 2. Encode the path as a Boolean constraint
    InitEncodingForSys.encode_boolean_paths network ctx paths ;
    InitEncodingForSys.encode_loop_closure network ctx paths ; 

    // 3. Encode the automaton as a boolean constraint
    let list_of_maps = EncodingForFormula.create_list_of_maps_of_formula_constraints ltl_formula network ctx paths
    EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head
    EncodingForFormula.encode_formula_transitions_over_path ltl_formula network ctx list_of_maps 
    EncodingForFormula.encode_formula_transitions_in_loop_closure ltl_formula network ctx list_of_maps
    EncodingForFormula.encode_formula_loop_fairness ltl_formula network ctx list_of_maps

    // 5. Solve the constraint.
    let start_time = System.DateTime.Now
    let sat = ctx.CheckAndGetModel (model)
    let end_time = System.DateTime.Now
    let duration = end_time.Subtract start_time
    printfn "Satisfiability check time: %A" duration


    // 6. Translate the model back
    let (the_result,the_model) = 
        if sat = LBool.True then
            (true, BioCheckPlusZ3.z3_model_to_loop (!model) paths)
        else
            (false,(0,Map.empty))

    if (!model) <> null then (!model).Dispose()

    ctx.Pop()
    // ctx.Pop()
    ctx.Dispose()
    cfg.Dispose()

    (the_result,the_model)