(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module GenLemmas

//
// Alg 2. Lemma generation procedure GenLemmas.
//

open Microsoft.FSharp.Collections

/// Attempt to find the unique fixpoint of a QN network
let stabilize (network : QN.node list) (range : Map<QN.var,(int*int)>) =
    // The ultimate bounds we have calculated for each variable. Initialize to range 
    let mutable bounds = Map.ofList [for node in network -> (node.var, (Map.find node.var range))]

    // The frontier set - variable that have had their bounds updated recently.
    // Initially, all variables are in the frontier.
    let mutable frontier = Set.ofList [for node in network -> node.var]

    // The expression associated with each variable.
    let exprs = Map.ofList [for node in network -> (node.var, node.f)]

    // The inputs to a variable, i.e. a map saying "variable x depends on variables
    // y and z."
    let inputs = Map.ofList [for node in network -> (node.var, node.inputs)]

    // The outputs for each variable, i.e. a map saying "variable x is an input to
    // variables y and z."
    let mutable outputs = Map.ofList [for node in network -> (node.var, Set.empty)]

    for node in network do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    // The main loop - keep tightening bounds until we can't tighten any further.
    while not frontier.IsEmpty do
        // Arbitrarily pick the lowest node in the frontier.
        let node = frontier.MinimumElement
        frontier <- Set.remove node frontier

        let expr = Map.find node exprs
        let (lower, upper) = Map.find node bounds

        let (new_lower, new_upper) = FNewLemmas.tighten node range expr lower upper (Map.find node inputs) bounds

        // If we were able to tighten the bound on this variable, keep a note of the new bounds
        // and add any dependent variables to the frontier
        if new_upper < upper || new_lower > lower then
            bounds <- Map.remove node bounds
            bounds <- Map.add node (new_lower, new_upper) bounds

            for out in Map.find node outputs do
                frontier <- Set.add out frontier

    // We've tightened our bounds as much as we can - either every variable has a singleton
    // bound now (i.e. it's stabilized) or there's at least one variable with a non-constant
    // bound, in which case we haven't proved anything.
    bounds


/// Attempt to find the unique fixpoint of a QN network.
/// But do so lazily, returning a sequence of tighter bounds until the final answer
let stabilize_lazy (network : QN.node list) (range : Map<QN.var,(int*int)>) =
    // The ultimate bounds we have calculated for each variable. Initialize to range 
    let mutable bounds = Map.ofList [for node in network -> (node.var, (Map.find node.var range))]

    // The frontier set - variable that have had their bounds updated recently.
    // Initially, all variables are in the frontier.
    let frontier = Set.ofList [for node in network -> node.var] // QSW: should this frontier be mutable?

    // The expression associated with each variable.
    let exprs = Map.ofList [for node in network -> (node.var, node.f)]

    // The inputs to a variable, i.e. a map saying "variable x depends on variables
    // y and z."
    let inputs = Map.ofList [for node in network -> (node.var, node.inputs)]

    // The outputs for each variable, i.e. a map saying "variable x is an input to
    // variables y and z."
    let mutable outputs = Map.ofList [for node in network -> (node.var, Set.empty)]

    for node in network do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    // initial state for Seq.unfold 
    let still_working = true 
    let initial_state = (still_working,frontier,bounds,outputs)

    // The main loop - keep tightening bounds until we can't tighten any further.
    Seq.unfold 
        (fun (still_working,frontier,bounds,outputs) ->
            if still_working then
                if (not (Set.isEmpty frontier)) then
                    // Arbitrarily pick the lowest node in the frontier.
                    let node = Set.minElement frontier
                    let frontier = Set.remove node frontier

                    let expr = Map.find node exprs
                    let (lower, upper) = Map.find node bounds

                    let (new_lower, new_upper) = FNewLemmas.tighten node range expr lower upper (Map.find node inputs) bounds

                    // If we were able to tighten the bound on this variable, keep a note of the new bounds
                    // and add any dependent variables to the frontier
                    let bounds,frontier = 
                        if new_upper < upper || new_lower > lower then
                            let bounds' = Map.add node (new_lower, new_upper) (Map.remove node bounds)
                            let frontier' = Set.fold (fun fr o -> Set.add o fr) frontier (Map.find node outputs)
                            (bounds',frontier')
                        else bounds, frontier

                    Some ((still_working,bounds),(still_working,frontier,bounds,outputs))

                // We've tightened our bounds as much as we can - either every variable has a singleton
                // bound now (i.e. it's stabilized) or there's at least one variable with a non-constant
                // bound, in which case we haven't proved anything.
                else
                    Some ((not still_working,bounds),(not still_working,frontier,bounds,outputs))
                //          ^                           indicate to client that this is the last proof step
                //                                    ^ indicate to next Seq.unfold that we're done
            else None)
        initial_state

