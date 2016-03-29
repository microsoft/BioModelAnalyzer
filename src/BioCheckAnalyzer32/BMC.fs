(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module BMC

open Microsoft.Z3

open System.Xml
open System.Xml.Linq
open System.Diagnostics

open LTL
open Simulate


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


// ltl_formula - the ltl formula
// network - the QN
// paths - list (according to time) of the ranges of all the variables
// loop_closure_loop is either -1 (in which case the loop_closure_loop is left open for the SAT solver to set)
//                   or a value (in which case the loop closes in that value)
// polarity - check the formula (true) or its negation (false)
let SingleSideBoundedMC (ltl_formula : LTLFormulaType) network (paths : Map<QN.var,int list> list) (loop_closure_loc : int) (polarity : bool) =
    
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
   
    let model = ref null
    ctx.Push()

    /// The system encoding part
    
    // 1. encode_the_time_variables_for_z3 for the mutual agreement on where the loop closes
    InitEncodingForSys.encode_loop_closure_variables ctx paths.Length loop_closure_loc ;

    // 2. Encode the path as a Boolean constraint
    InitEncodingForSys.encode_boolean_paths network ctx paths ;
    InitEncodingForSys.encode_loop_closure network ctx paths loop_closure_loc ; 

    // 3. Encode the automaton as a boolean constraint
    let list_of_maps = EncodingForFormula.create_list_of_maps_of_formula_constraints ltl_formula network ctx paths
    EncodingForFormula.encode_formula_transitions_over_path ltl_formula network ctx list_of_maps 
    EncodingForFormula.encode_formula_transitions_in_loop_closure ltl_formula network ctx list_of_maps
    EncodingForFormula.encode_formula_loop_fairness ltl_formula network ctx list_of_maps


    // 4. Model check 
    if polarity then
        EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head true
    else
        EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head false

    let start_time = System.DateTime.Now
    let sat = ctx.CheckAndGetModel (model)
    let end_time = System.DateTime.Now
    let duration = end_time.Subtract start_time
    Log.log_debug ("Satisfiability check time (pos)" + duration.ToString())

    let (the_result,the_model) = 
        if sat = LBool.True then
            (true, BioCheckPlusZ3.z3_model_to_loop (!model) paths)
        else
            (false,(0,Map.empty))

    if (!model) <> null then (!model).Dispose()

    ctx.Pop()
    ctx.Dispose()
    cfg.Dispose()

    (the_result, the_model)

let simulation_to_loop (loop : int) (simulation : Map<QN.var, int> list) =
    let indices = [ 0 .. (simulation.Length - 1) ]
    let new_sim = List.fold2 (fun m index state -> Map.add index state m) Map.empty indices simulation
    (loop, new_sim)
    

let change_to_right_length (simulation : Map<QN.var, int> list) (loop : int) (desired_length : int) =
    if desired_length < simulation.Length - loop then 
        (simulation, loop) // There is no point to do anything
                           // the simulation produced a too long loop and is not usable
                           // the SAT solver will find that
    elif desired_length < simulation.Length then // but desired length >= simulation.Length - loop
        let rec get_suffix (current_simulation : Map<QN.var, int> list) =
            if current_simulation.Length <= desired_length then current_simulation
            else get_suffix (List.tail current_simulation) 
        ((get_suffix simulation), (loop - (simulation.Length - desired_length)))
    elif desired_length > simulation.Length then // but desired length >= simulation.Length - loop
        let rec extend_length curr_simulation extend_by index =
            if extend_by = 0 then curr_simulation
            else
                extend_length (List.append curr_simulation ([ List.nth curr_simulation index ])) (extend_by-1) (index+1)
        ((extend_length simulation (desired_length - simulation.Length) loop) , (loop + desired_length - simulation.Length))
    else // desired_length = simulation.Length
        (simulation, loop)


let SimulationBasedMC (ltl_formula : LTLFormulaType) network (paths : Map<QN.var, int list> list) = 
    let initial_ranges = paths.Head
    let initial_values = Map.fold (fun m k (l : int list) -> Map.add k l.Head m) Map.empty initial_ranges
    let (simulation, loop) = Simulate.simulate_up_to_loop network initial_values
    let (right_length_sim, right_length_loop) = change_to_right_length simulation loop paths.Length
    let list_simulation = List.map (fun elem -> (Map.map (fun key t -> [t]) elem)) right_length_sim 
    let (res, model) = SingleSideBoundedMC ltl_formula network list_simulation right_length_loop true
    if res then (res, model)
    else (res, (simulation_to_loop right_length_loop right_length_sim))


let PolarityBoundedMC ltl_formula network paths previous_res previous_model =
    if previous_res then
        let (res1, model1) = (previous_res, previous_model)
        let (res2, model2) =
            SingleSideBoundedMC ltl_formula network paths -1 false
        (res1, model1, res2, model2)
    else
        let (res1, model1) = SingleSideBoundedMC ltl_formula network paths -1 true
        let (res2, model2) = (true, previous_model)
        (res1, model1, res2, model2)

//Method duplicates previous but with more clear output
//let PolarityBoundedMC2 ltl_formula network paths previous_res =
//    if previous_res then
//        //let (res1, model1) = (previous_res, previous_model)
//        let (res2, model2) =
//            SingleSideBoundedMC ltl_formula network paths -1 false
//        (res1, model1, res2, model2)
//    else
//        let (res1, model1) = SingleSideBoundedMC ltl_formula network paths -1 true
//        //let (res2, model2) = (true, previous_model)
//        (res1, model1, res2, model2)

    
let DoubleBoundedMCWithSim (ltl_formula : LTLFormulaType) network (paths : Map<QN.var,int list> list) check_both =
    let (res,model) = SimulationBasedMC ltl_formula network paths
    if check_both then
        PolarityBoundedMC ltl_formula network paths res model
    else
        (res, model, false, (0,Map.empty))

let BoundedMC (ltl_formula : LTLFormulaType) network (paths : Map<QN.var,int list> list) check_both =
    
    let cfg = new Config()
    cfg.SetParamValue("MODEL", "true")
    let ctx = new Context(cfg)
   
    let model1 = ref null
    let model2 = ref null
    ctx.Push()

    /// The system encoding part
    
    // 1. encode_the_time_variables_for_z3 for the mutual agreement on where the loop closes
    InitEncodingForSys.encode_loop_closure_variables ctx paths.Length -1 ;

    // 2. Encode the path as a Boolean constraint
    InitEncodingForSys.encode_boolean_paths network ctx paths ;
    InitEncodingForSys.encode_loop_closure network ctx paths -1 ; 

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
    

    let negative = //(the_model2,the_result2) = 
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
            Some(in_result2, in_model2)
        else 
            None//(false,(0,Map.empty))
    ctx.Pop()

    ctx.Pop()
    ctx.Dispose()
    cfg.Dispose()

    (the_result1,the_model1,negative)
