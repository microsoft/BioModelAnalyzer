(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module BMC

open Microsoft.Z3

open System.Xml
open System.Xml.Linq
open System.Diagnostics

open LTL


//SI: un-used? 
////  Qinsi's encoding of the automaton to Z3
//let encode_automaton network paths ctx initK = 
//    /// The ltl (TGBA) encoding part
//    //Process.Start("ltl2tgba2.exe", "vpcp3.txt").WaitForExit()
//    let automaton = List.filter (fun x -> x <> "") (List.ofSeq(LTL2TGBA.readLines("true.txt")))
//    
//    let (locations, transitions, accset) = ReadTGBA.LocTransAcc automaton
//    // classify the transitions to obtain the AccSet F
//    let accRelTransSet = AccRelatedTrans.AccClassification accset transitions
//    //printf "%A" accRelTransSet
//    let transToBeHandled = TGBATransEncoding.ClassifyTrans transitions
//    //assert initial location
//    TGBATransEncoding.assetInitLoc locations ctx
//    //assert the transitions for initK steps
//    for currStep in [0..initK] do
//        let assertionForAllTrans = TGBATransEncoding.assertTransAtOneStep transToBeHandled currStep ctx
//        ctx.AssertCnstr assertionForAllTrans 
//        //printfn "%d" currStep
//    printfn "%s" "finish assertion for unloop conditions"
//    // from now on, all the assertions will be k-depandent, thus we need to add push and pop here
//    ctx.Push()
//    // first, I just consider the initK condition
//    
//    //assert the entire loop condition
//    let mutable assertionForLoop = ctx.MkFalse()
//    let mutable loopStart = 0
//    while loopStart <= initK do
//        // first, the loop condition for system part, denoted as (l)_L_(k), which means the state S(l) and S(k) are the same
//        // detailedly, all the variables in states share the same values
//        let assertionForOnePairOfStates = SameStateForSys.assertSameStatesForSys network loopStart initK ctx
//        
//        let assertionForAllAccSubsets = 
//            if accset.Length > 0 &&  accset.Head <> "NULL" then 
//                BuchiAcc.Z3ForAccCon accRelTransSet loopStart initK ctx
//            else
//                ctx.MkTrue()
//        //printfn "%A" assertionForAllAccSubsets
//        let assertionForOnePairOfLocs = SameLocationForTGBA.assertSameLocsForTGBA locations loopStart initK ctx
//        assertionForLoop <- ctx.MkOr(assertionForLoop, ctx.MkAnd(assertionForOnePairOfStates, ctx.MkAnd(assertionForAllAccSubsets, assertionForOnePairOfLocs)))
//        loopStart <- loopStart + 1
//        //printfn "%d" loopStart
//    ctx.AssertCnstr assertionForLoop

//    printfn "%s" "finish all assertions in K steps"


let BoundedMC (ltl_formula : LTLFormulaType) network initBound (paths : Map<QN.var,int list> list) check_both =
    
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
   
    let model1 = ref null
    let model2 = ref null
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
    EncodingForFormula.encode_formula_transitions_over_path ltl_formula network ctx list_of_maps 
    EncodingForFormula.encode_formula_transitions_in_loop_closure ltl_formula network ctx list_of_maps
    EncodingForFormula.encode_formula_loop_fairness ltl_formula network ctx list_of_maps

    // 4. Model check positive
    ctx.Push()
    EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head true

    let start_time1 = System.DateTime.Now
    let sat1 = ctx.CheckAndGetModel (model1)
    let end_time1 = System.DateTime.Now
    let duration1 = end_time1.Subtract start_time1
    Log.log_debug ("Satisfiability check time (pos)" + duration1.ToString())

    // 5. Translate the model back
    let (the_result1,the_model1) = 
        if sat1 = LBool.True then
            (true, BioCheckPlusZ3.z3_model_to_loop (!model1) paths)
        else
            (false,(0,Map.empty))

    if (!model1) <> null then (!model1).Dispose()
    ctx.Pop()

    ctx.Push()
    // 6. Model check negative
    let (the_model2,the_result2) = 
        if (check_both) then 
            EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head false
            let start_time2 = System.DateTime.Now
            let sat2 = ctx.CheckAndGetModel(model2)
            let end_time2 = System.DateTime.Now
            let duration2 = end_time2.Subtract start_time2
            Log.log_debug("Satisfiability check time (neg)" + duration2.ToString())

            let (in_result2,in_model2) =
                if sat2 = LBool.True then
                    (true, BioCheckPlusZ3.z3_model_to_loop (!model2) paths)
                else
                    (false, (0,Map.empty))

            if (!model2) <> null then (!model2).Dispose()
            (in_result2, in_model2)
        else 
            (false,(0,Map.empty))
    ctx.Pop()

    ctx.Pop()
    ctx.Dispose()
    cfg.Dispose()

    (the_result1,the_model1,the_result2,the_model2)