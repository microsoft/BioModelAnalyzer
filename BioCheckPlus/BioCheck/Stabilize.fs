(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Stabilize

// Algorithm 1

let stabilization_proving_proc network range =
    let bounds = GenLemmas.stabilize network range
    let stabilized = not <| Map.exists (fun _ (lower, upper) -> upper <> lower) bounds
    if stabilized then
    // if the system is stable, return the unique fixpoint
        let targets = Map.map (fun var (lower, upper) -> upper) bounds
        let pretty_targets = Map.ofList [for node in network -> (node.name, Map.find node.var targets)]
        Result.Stabilizing(pretty_targets)
    else
        Counterexample.find_counterexample network bounds range
        // QSW: output all the fixpoints the system could have
        // Counterexample.find_all_fixpoints network bounds range
        // QSW: output all the cycles the system could have
        // Counterexample.find_all_cycles network bounds range


let stabilization_prover_lazy network range =
    Log.log_debug ("Starting stabilization_prover_lazy network={" + (String.concat "," (List.map QN.str_of_node network)) + "}, range={" + (QN.str_of_range range) + "}")
    GenLemmas.stabilize_lazy network range
    |> Seq.map 
        (fun (still_working,bounds) -> 
            if still_working then 
                Log.log_debug ("Stepping: bounds={" + (QN.str_of_range bounds) + "}")
                Result.ResultStepping(bounds)
            else 
                let stabilized = Map.forall (fun _ (lower,upper) -> upper = lower) bounds
                if stabilized then 
                    Log.log_debug ("Stabilizing: bounds={" + (QN.str_of_range bounds) + "}")
                    let targets = Map.map (fun var (lower, upper) -> upper) bounds
                    // SI: should return var->bound map, rather than pretty_version. 
                    let pretty_targets = Map.ofList [for node in network -> (node.name, Map.find node.var targets)]
                    Result.ResultLastStep(Result.Stabilizing(pretty_targets))
                else 
                    Log.log_debug ("Not Stabilizing. Looking for a CEx...")
                    let cex = Counterexample.find_counterexample network bounds range
                    Log.log_debug (Result.str_of_result cex)
                    Result.ResultLastStep(cex))
   