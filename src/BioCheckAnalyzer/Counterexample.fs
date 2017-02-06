// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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
// Diameter calculation was needed for running bound on Z.find_cycle_steps net diameter bounds
//        let diameter =
//                Map.fold 
//                    (fun total _ (lo: int, hi: int) -> 
//                        bigint.Multiply(total, bigint.Subtract(bigint.Add(bigint.One, bigint(hi)), bigint(lo))))
//                    bigint.One
//                    bounds

        // old find cycle (looks for a cycle up to the number of steps in the network): 
//        let cycle = Z.find_cycle_steps net diameter bounds //range

    let cycle = Z.find_cycle_steps_optimized net bounds //range

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


type concurrency = Synchronous | Asynchronous

/// Find a counterexample to the stability of a network.
// Compose the above three functions; not part of the UI Interface itself but used by Main and UIMain.
// Right now, we call the inners of each of these three functions; we should just call them straight. 
let find_cex (net : QN.node list) (bounds : Map<QN.var, int*int>) (no_sat : bool) concurrencyType =

    if (no_sat) then
        Log.log_debug "Not allowed to run sat solver. Just returning none"
        Result.CExUnknown
    else
        // Try to find a bifurcation first....
        Log.log_debug "CEx(1): check whether the model bifurcates."
        let bifurcation = Z.find_bifurcation net bounds(*was: range*)

        match bifurcation with
        | Some(x) -> Result.CExBifurcation(x)
        | None ->
            Log.log_debug "No bifurcation..."

            // No bifurcation - let's try to find a cycle...


            // TODO:
            // Try to find one fixpoint. 
            // If there is no fixpoint - just run a simulation from an arbitrary point
            //                           this simulation will return a cycle and this cycle
            //                           can be returned as the counter example.
            // If there is a fixpoint continue to finding a cycle as before.

            // diameter = \Pi_{v \in bounds} (hi (bounds v)) - (lo (bounds v)) + 1

    // Diameter calculation was needed for running bound on Z.find_cycle_steps net diameter bounds
    //        let diameter =
    //                Map.fold 
    //                    (fun total _ (lo: int, hi: int) -> 
    //                        bigint.Multiply(total, bigint.Subtract(bigint.Add(bigint.One, bigint(hi)), bigint(lo))))
    //                    bigint.One
    //                    bounds

    //        let cycle = Z.find_cycle_steps net diameter bounds //range
            match concurrencyType with
            | Synchronous ->
                Log.log_debug "CEx(2): check whether the model cycles."
                let cycle = Z.find_cycle_steps_optimized net bounds//range

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
            | Asynchronous -> 
                failwith "Not implemented yet BAH"
                // Log.log_debug "CEx(2): check whether the model has a endcomponent."
                // Log.log_debug "CEx(2a): check whether the model has a frustrated fixpoint."
                // let fix = Z.find_frustrated_fixpoints net bounds
                // //First find a cycle
                // let cycle = lazy (Log.log_debug "CEx(2b): check whether the model has collapsing cycles."; Z.find_cycle_steps_optimized net bounds false)
                // //Test to see if the cycle collapses in async space
                // match (fix,cycle) with
                // | (Some(x),_) -> Result.CExEndComponent(x)
                // | (_,Lazy(Some(x))) -> Result.CExEndComponent(x)
                // | (_,Lazy(None)) ->
                //     Log.log_debug "No endComponent..."
                //     Log.log_debug "CEx(3): check whether the model has a fixpoint."
                //     let fix = Z3Util.find_fixpoint net bounds(*was: range*)

                //     match fix with
                //     | Some(x) -> Result.CExFixpoint(x)
                //     | None ->
                //         Log.log_debug "...and no fixpoint???"
                //         Result.CExUnknown

