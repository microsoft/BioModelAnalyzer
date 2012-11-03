(* Copyright (c) Microsoft Corporation. All rights reserved. *)

module Counterexample

// Find a counterexample to the stability of a network.
// The output is {Map<string, int>}^+

let find_counterexample (net : QN.node list) (bounds : Map<QN.var, int*int>) range =

    // Try to find a bifurcation first....
    let bifurcation = BioCheckZ3.find_bifurcation net range

    match bifurcation with
    | Some(x) -> Result.Bifurcation(x)
    | None ->
        printfn "No bifurcation..."

        // No bifurcation - let's try to find a cycle...
        let cycle = BioCheckZ3.find_cycle_steps net bounds -1 range

        match cycle with
        | Some(x) -> Result.Cycle(x)
        | None ->
            printfn "No cycle..."

            // No cycle either... so if there is a fixpoint, it must be unique
            // and reachable from every other state.
            let fix = BioCheckZ3.find_fixpoint net range 

            match fix with
            | Some(x) -> Result.Fixpoint(x)
            | None ->
                printfn "...and no fixpoint???"
                Result.Unknown
 
 /// QSW: find all fixpoints 

// let find_all_fixpoints (net : QN.node list) (bounds : Map<QN.var, int*int>) range =
 


