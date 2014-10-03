(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Counterexample

let find_cex_bifurcation (net : QN.node list) (bounds : Map<QN.var, int*int>) =
    Log.log_debug "CEx(1): check whether the model bifurcates."
    let bifurcation = Z.find_bifurcation net bounds
    match bifurcation with
    | Some(x,y) -> Some(Result.CExBifurcation(x,y))
    | None -> 
        Log.log_debug "No bifurcation..."
        None

let find_cex_cycle (net : QN.node list) (bounds : Map<QN.var, int*int>) =
    Log.log_debug "CEx(2): check whether the model cycles."
    // diameter = \Pi_{v \in bounds} (hi (bounds v)) - (lo (bounds v)) + 1
    let diameter =
            Map.fold 
                (fun total _ (lo: int, hi: int) -> 
                    bigint.Multiply(total, bigint.Subtract(bigint.Add(bigint.One, bigint(hi)), bigint(lo))))
                bigint.One
                bounds

    let cycle = Z.find_cycle_steps net diameter bounds 

    match cycle with
    | Some(x) -> Some(Result.CExCycle(x))
    | None ->
        Log.log_debug "No cycle..."
        None

let find_cex_fixpoint (net : QN.node list) (bounds : Map<QN.var, int*int>) =
    Log.log_debug "CEx(3): check whether the model has a fixpoint."
    let fix = Z.find_fixpoint net bounds

    match fix with
    | Some(x) -> Some(Result.CExFixpoint(x))
    | None ->
        Log.log_debug "...and no fixpoint???"
        None


/// Find a counterexample to the stability of a network.
// Compose the above three functions; not part of the UI Interface itself but used by Main and UIMain.
// Right now, we call the inners of each of these three functions; we should just call them straight. 
let find_cex (net : QN.node list) (bounds : Map<QN.var, int*int>) =

    // Try to find a bifurcation first....
    Log.log_debug "CEx(1): check whether the model bifurcates."
    let bifurcation = Z.find_bifurcation net bounds(*was: range*)

    match bifurcation with
    | Some(x) -> Result.CExBifurcation(x)
    | None ->
        Log.log_debug "No bifurcation..."

        // No bifurcation - let's try to find a cycle...
        Log.log_debug "CEx(2): check whether the model cycles."
        // diameter = \Pi_{v \in bounds} (hi (bounds v)) - (lo (bounds v)) + 1
        let diameter =
                Map.fold 
                    (fun total _ (lo: int, hi: int) -> 
                        bigint.Multiply(total, bigint.Subtract(bigint.Add(bigint.One, bigint(hi)), bigint(lo))))
                    bigint.One
                    bounds

        let cycle = Z.find_cycle_steps net diameter bounds //range

        match cycle with
        | Some(x) -> Result.CExCycle(x)
        | None ->
            Log.log_debug "No cycle..."

            // No cycle either... so if there is a fixpoint, it must be unique
            // and reachable from every other state.
            Log.log_debug "CEx(3): check whether the model has a fixpoint."
            let fix = Z.find_fixpoint net bounds(*was: range*)

            match fix with
            | Some(x) -> Result.CExFixpoint(x)
            | None ->
                Log.log_debug "...and no fixpoint???"
                Result.CExUnknown


